using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Linq;

// Author : Shayan Firoozi 2020 - Bandar Abbas , Iran
// Shayan.Firoozi@gmail.com

namespace ProcessMemory
{
    public class ProcessMemory
    {

        #region WinAPI
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 2035711,
            CreateThread = 2,
            DupHandle = 64,
            QueryInformation = 1024,
            SetInformation = 512,
            Synchronize = 1048576,
            Terminate = 1,
            VMOperation = 8,
            VMRead = 16,
            VMWrite = 32
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetLastError();


        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(IntPtr hProcess, UIntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpflOldProtect);


        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
          private static extern bool CloseHandle(IntPtr hObject);


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
         ProcessAccessFlags processAccess,
         bool bInheritHandle,
         int processId);


        [DllImport("kernel32.dll", SetLastError = true)]
          private static extern bool ReadProcessMemory(
           IntPtr hProcess,
           IntPtr lpBaseAddress,
           byte[] lpBuffer,
           Int32 nSize,
           int lpNumberOfBytesRead);




        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, 
            UIntPtr lpBaseAddress, 
            byte[] lpBuffer, 
            UIntPtr nSize, 
            IntPtr lpNumberOfBytesWritten);




        #endregion

        public enum ReadWriteMemoryEncoding : int
        {
            ASCII = 0,
            Unicode = 1,
            UTF7 = 2,
            UTF8 = 3,
            UTF32 = 4
        }

        public Process ProcessInfo = new Process();
        public ProcessModule ModuleInfo;
        public IntPtr ProcessAddressPointer = new IntPtr(0);





        #region Class Constructors
        /// <summary>
        /// Open Process based on the Process Name
        /// </summary>
        /// <param name="ProcessName">Name of the process , examples : Shayan.exe or Shayan</param>
        public ProcessMemory(string ProcessName, bool WinAPI_Open_Process_For_Full_Access)
        {

            _Init_Process(ProcessName);


            if (WinAPI_Open_Process_For_Full_Access == true)
            {
                _Init_WinAPI_Open_Process();
            }

        }

        /// <summary>
        /// Open Process based on the Process Name and the Process Module name
        /// </summary>
        /// <param name="ProcessName">Name of the process , examples : Shayan.exe or Shayan</param>
        /// <param name="ModuleName">Name of the process module , example : shell32.dll or ole32.dll </param>
        public ProcessMemory(string ProcessName, string ModuleName, bool WinAPI_Open_Process_For_Full_Access)
        {
            _Init_Process(ProcessName);

            _Init_Module(ModuleName);

            if (WinAPI_Open_Process_For_Full_Access == true)
            {
                _Init_WinAPI_Open_Process();
            }

        } 
        #endregion




        /// <summary>
        /// Class Destructor
        /// </summary>
          ~ProcessMemory()
        {

            try
            {
                ProcessInfo.Dispose();
                ModuleInfo.Dispose();

                ProcessInfo = null;
                ModuleInfo = null;


            }
            catch { }

            if (ProcessAddressPointer == IntPtr.Zero)
            {
                return;
            }

            try
            {
                CloseHandle(ProcessAddressPointer);
            }
            catch { }
        }



        #region ReadMemoryFunctions

        public byte[] ReadProcMemoryByteArray(IntPtr ptrAddress, int ptrSize)
        {
           return _Read_Proc_Memory(ptrAddress, ptrSize);
        }


        public string ReadProcMemoryString(IntPtr ptrAddress, int ptrSize , ReadWriteMemoryEncoding encoding)
        {
            if (encoding == ReadWriteMemoryEncoding.ASCII)
            {
                return Encoding.ASCII.GetString(ReadProcMemoryByteArray(ptrAddress, ptrSize));
            }


            if (encoding == ReadWriteMemoryEncoding.Unicode)
            {
                return Encoding.Unicode.GetString(ReadProcMemoryByteArray(ptrAddress, ptrSize));
            }

            if (encoding == ReadWriteMemoryEncoding.UTF32)
            {
                return Encoding.UTF32.GetString(ReadProcMemoryByteArray(ptrAddress, ptrSize));
            }


            if (encoding == ReadWriteMemoryEncoding.UTF7)
            {
                return Encoding.UTF7.GetString(ReadProcMemoryByteArray(ptrAddress, ptrSize));
            }


            if (encoding == ReadWriteMemoryEncoding.UTF8)
            {
                return Encoding.UTF8.GetString(ReadProcMemoryByteArray(ptrAddress, ptrSize));
            }

            return Encoding.Default.GetString(ReadProcMemoryByteArray(ptrAddress, ptrSize));

        }


        public string ReadProcMemoryString(IntPtr ptrBeginAddress, IntPtr ptrEndAppress, ReadWriteMemoryEncoding encoding)
        {

            long _begin = 0;
            long _end = 0;

            _begin = ptrBeginAddress.ToInt64();
            _end = ptrEndAppress.ToInt64();

            string _result = "";


            for (long _offset = _begin; _offset < _end ;_offset++)
            {
               
                _result += ReadProcMemoryString(new IntPtr(_offset), (int)1, encoding);
            }

              return _result.Replace('\0',' ');
        }


        #endregion


        #region WriteMemoryFunctions


        public void WriteProcMemory(UIntPtr ptrAddress, string _Data, ReadWriteMemoryEncoding encoding)
        {
            // _Data = _Data + "\0";

            if (encoding == ReadWriteMemoryEncoding.ASCII)
            {
                _WriteProcMemory(ptrAddress, Encoding.ASCII.GetBytes(_Data));
            }

            if (encoding == ReadWriteMemoryEncoding.Unicode)
            {
                _WriteProcMemory(ptrAddress, Encoding.Unicode.GetBytes(_Data));
            }


            if (encoding == ReadWriteMemoryEncoding.UTF32)
            {
                _WriteProcMemory(ptrAddress, Encoding.UTF32.GetBytes(_Data));
            }

            if (encoding == ReadWriteMemoryEncoding.UTF7)
            {
                _WriteProcMemory(ptrAddress, Encoding.UTF7.GetBytes(_Data));
            }

            if (encoding == ReadWriteMemoryEncoding.UTF8)
            {
                _WriteProcMemory(ptrAddress, Encoding.UTF8.GetBytes(_Data));
            }

          
        }


        #endregion


        #region PrivateFunctions
        private void _Init_Process(string ProcessName)
        {
            try
            {
                ProcessName = ProcessName.Replace(".exe", "");

                if (Process.GetProcessesByName(ProcessName).ToList().Count == 0)
                {
                    throw new Exception("Process name is invalid or the process is not running.");
                }

                ProcessInfo = Process.GetProcessesByName(ProcessName)[0];


                if (ProcessInfo.Id == 0)
                {
                    throw new Exception("Process id is not valid.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown exception has been occured : " + Environment.NewLine + ex.Message);
            }

        }

        private void _Init_Module(string ModuleName)
        {

            try
            {
                // Search for the module in process

                foreach (ProcessModule _module in ProcessInfo.Modules)
                {
                    if (_module.ModuleName == ModuleName)
                    {
                        ModuleInfo = _module;
                        break;
                    }
                }


                if (ModuleInfo == null) throw new Exception("Module not found in the process.");
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown exception has been occured : " + Environment.NewLine + ex.Message);
            }

        }

        private void _Init_WinAPI_Open_Process()
        {
            try
            {
                if (ProcessInfo.Id == 0)
                {
                    throw new Exception("No Process ID found to open with WinAPI");
                }

                ProcessAddressPointer = OpenProcess(ProcessAccessFlags.All, false, ProcessInfo.Id);

                if (ProcessAddressPointer == IntPtr.Zero)
                {
                    throw new Exception("The Process has been found but can not Open it with WinAPI in full access mode.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown exception has been occured : " + Environment.NewLine + ex.Message);
            }
        }

        private byte[] _Read_Proc_Memory(IntPtr pOffset, int pSize)
        {
            if (ProcessAddressPointer == IntPtr.Zero)
            {
                throw new Exception("The Process has not been opened.");
            }

            try
            {

                byte[] buffer = new byte[pSize];
                ReadProcessMemory(ProcessAddressPointer, pOffset, buffer, pSize, 0);
                return buffer;

              
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown exception has been occured : " + Environment.NewLine + ex.Message);
            }
        }


        private void _WriteProcMemory(UIntPtr ptrAddress, byte[] pBytes)
        {
            if (ProcessAddressPointer == IntPtr.Zero)
            {
                throw new Exception("The Process has not been opened.");
            }

            try
            {
                uint _old_protection = 0;

                VirtualProtectEx(ProcessAddressPointer, ptrAddress, (int)1, (uint)0x00000040, out _old_protection);
                  WriteProcessMemory(ProcessAddressPointer, ptrAddress, pBytes, (UIntPtr)pBytes.Length, IntPtr.Zero);
                VirtualProtectEx(ProcessAddressPointer, ptrAddress, (int)1, _old_protection, out _old_protection);

            }
            catch (Exception ex)
            {
                throw new Exception("Unknown exception has been occured : " + Environment.NewLine + ex.Message);
            }
        }


        #endregion


    }
}