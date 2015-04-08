using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronManConsole
{
    class Action
    {
        private Win32framework win;

        public Action()
        {
            win = new Win32framework();
        }

        public void Left()
        {
            if (win.GetCurrentWindowName().ToUpper().Contains("POWERPOINT SLIDE SHOW"))
            {
                win.KeyLeft();
            }
            else
            {
                win.altEscape();
            }
        }

        public void Right()
        {
            if (win.GetCurrentWindowName().ToUpper().Contains("POWERPOINT SLIDE SHOW"))
            {
                win.KeyRight();
            }
            else
            {
                win.altShiftEscape();
            }

        }

        public void Up()
        {
            if (!win.GetCurrentWindowName().ToUpper().Contains("POWERPOINT SLIDE SHOW"))
            {
                this.win.WinUp();
            }
        }

        public void Down()
        {
            if (!win.GetCurrentWindowName().ToUpper().Contains("POWERPOINT SLIDE SHOW"))
            {
                this.win.WinDown();
            }
        }

        public void Pinch(int size, Point location)
        {
            if (!win.GetCurrentWindowName().ToUpper().Contains("POWERPOINT SLIDE SHOW"))
            {
                win.ResizeWindow(size, location);
            }
        }

        public void VolUp()
        {
            this.win.VolUp();
        }

        public void VolDown()
        {
            this.win.WinDown();
        }
    }
}
