using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronManConsole
{
    class App
    {
        Camera cam;

        public void run()
        {
            System.Threading.Thread.CurrentThread.Name = "RUNNER";
            this.cam = new Camera();
            this.cam.Start();
        }
    }
}
