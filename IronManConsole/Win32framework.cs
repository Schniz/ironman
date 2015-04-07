using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace IronManConsole
{
    public class Win32framework
    {
        #region Consts

        private const UInt32 MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const UInt32 MOUSEEVENTF_LEFTUP = 0x0004;
        [DllImport("user32.dll")]
        private static extern void mouse_event(
               UInt32 dwFlags, // motion and click options
               UInt32 dx, // horizontal position or change
               UInt32 dy, // vertical position or change
               UInt32 dwData, // wheel movement
               IntPtr dwExtraInfo // application-defined information
        );

        #endregion

        /// <summary>
        /// Set cursor position by x,y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetCursorPosition(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }

        /// <summary>
        /// Add or sub cursor position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ChangeCursorPosition(int x, int y)
        {
            Point currCursorPosition = Cursor.Position;

            SetCursorPosition(currCursorPosition.X + x, currCursorPosition.Y + y);
        }

        public void MakeKeyPress()
        {

        }

        public void CursorLeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new System.IntPtr());
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, new System.IntPtr());
        }

    }
}
