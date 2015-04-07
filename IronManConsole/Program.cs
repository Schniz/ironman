using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IronManConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            new App().run();
            Console.Read();
            Win32framework a = new Win32framework();
        }
    }
}
