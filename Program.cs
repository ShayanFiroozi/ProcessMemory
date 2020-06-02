using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMemory
{
     class Program
    {
        private static void Main(string[] args)
        {
            if (IsAdministrator() == false)
            {

                Console.WriteLine("Please run this app as administrator...");
                Console.WriteLine("Press any key to exit...");

                Console.ReadLine();
                return;
            }


            ProcessMemory processMemory = new ProcessMemory("_LNPR_.exe",true);


                Console.WriteLine(processMemory.ReadProcMemoryString(processMemory.ProcessInfo.MainModule.BaseAddress, 
                                                                     (int)1024,
                                                                     ProcessMemory.ReadWriteMemoryEncoding.ASCII));
            


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
