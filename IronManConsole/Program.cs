using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IronManConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //new App().run();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Main a = new Main();
            a.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - a.Width, Screen.PrimaryScreen.WorkingArea.Height - a.Height);
            a.Load += delegate
            {
                a.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - a.Width, Screen.PrimaryScreen.WorkingArea.Height - a.Height);
            };
            Application.Run(a);
        }
    }
}
