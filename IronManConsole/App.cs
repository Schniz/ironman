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
            this.cam = new Camera(this.onFiredGesture);
            this.cam.Start();
        }

        private void onFiredGesture(PXCMHandData.GestureData gestureData)
        {
            if (gestureData.name.CompareTo("spreadfingers") == 0)
            {
                Console.WriteLine("spreadfingers");
            }

            if (gestureData.name.CompareTo("thumb_up") == 0)
            {
                Console.WriteLine("thumbs");
            }
        }
    }
}
