using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMemory
{
     class Program
    {
        private static void Main(string[] args)
        {


            Console.ForegroundColor = ConsoleColor.DarkRed;

            Console.WriteLine(new string('-',35)  + "Process Memory" + new string('-', 35));
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            
              Console.WriteLine("Author : Shayan Firoozi , Bandar Abbas - Iran");
              Console.WriteLine("Email :  Shayan.Firoozi@gmail.com");

            Console.ForegroundColor = ConsoleColor.DarkRed;

            Console.WriteLine("");
            Console.WriteLine(new string('-', 84));
            Console.WriteLine("");

            if (IsAdministrator() == false)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please run this app as administrator...");
                Console.WriteLine("Press any key to exit...");

                Console.ReadLine();
                return;
            }

           
            try
            {

                ProcessMemory processMemory = new ProcessMemory("_LNPR_.exe", "BNazaninBold.ttf",true);

                Console.ForegroundColor = ConsoleColor.Red;

                if (processMemory.ProcessInfo != null)
                {

                    Console.WriteLine("Process Name : " + processMemory.ProcessInfo.ProcessName +
                                                        " running on " + processMemory.ProcessInfo.MachineName);

                    Console.WriteLine("Process Base Address : " + processMemory.ProcessInfo.MainModule.BaseAddress.ToString());

                    Console.WriteLine("");


                    if (processMemory.ModuleInfo != null)
                    {

                        Console.WriteLine("Module Name : " + processMemory.ModuleInfo.ModuleName +
                                                            " running on " + processMemory.ProcessInfo.ProcessName);


                        Console.WriteLine("Module Address : " + processMemory.ModuleInfo.BaseAddress.ToString());
                    }


                    Console.ForegroundColor = ConsoleColor.Blue;

                    Console.WriteLine("");
                    Console.WriteLine("Reading process memory : ");
                    Console.WriteLine("");

                    Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine(processMemory.ReadProcMemoryString(new IntPtr(processMemory.ProcessInfo.MainModule.BaseAddress.ToInt32()),
                                                                             (int)1024,
                                                                             ProcessMemory.ReadWriteMemoryEncoding.ASCII));

                }



            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occured : " + Environment.NewLine + ex.Message);
            }


            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("");
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();

        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

    }
}
