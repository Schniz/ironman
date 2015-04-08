using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronManConsole
{
    public class Action
    {
        private Win32framework win;

        public const int PINCH_INTERVAL = 1;

        private bool isSlide()
        {
            return win.GetCurrentWindowName().ToUpper().Contains("POWERPOINT SLIDE SHOW") || win.GetCurrentWindowName().ToUpper().Contains("GOOGLE SLIDE");
        }

        public Action()
        {
            win = new Win32framework();
        }

        public void Left()
        {
            if (this.isSlide())
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
            if (this.isSlide())
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
            if (!this.isSlide())
            {
                this.win.WinUp();
            }
        }

        public void Down()
        {
            if (!this.isSlide())
            {
                this.win.WinDown();
            }
        }

        public void Pinch(int size, Point location)
        {
            if (!this.isSlide() && size != 0)
            {
                int j = size / PINCH_INTERVAL;
                for (int i = 0; i < PINCH_INTERVAL; i++)
                {
                    win.ResizeWindow(j, location);
                }
            }
        }

        public void VolUp()
        {
            this.win.VolUp();
            this.win.VolUp();
        }

        public void VolDown()
        {
            this.win.VolDown();
            this.win.VolDown();
        }
    }
}
