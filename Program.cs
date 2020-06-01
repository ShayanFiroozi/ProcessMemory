using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMemory
{
     class Program
    {
        private static void Main(string[] args)
        {

            ProcessMemory processMemory = new ProcessMemory("devenv.exe","bcrypt.dll",true);


                Console.WriteLine(processMemory.ReadProcMemoryString(processMemory.ModuleInfo.BaseAddress, 
                                                                     (int)100,
                                                                     ProcessMemory.ReadWriteMemoryEncoding.ASCII));
            


            Console.ReadLine();

        }
    }
}
