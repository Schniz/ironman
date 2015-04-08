using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using WindowScrape;
using System.Diagnostics;

namespace IronManConsole
{

    public class Win32framework
    {
        #region Consts
        // Mouse clicks
        const uint MOUSEEVENTF_LEFTDOWN = 0x0008;
        const uint MOUSEEVENTF_LEFTUP = 0x0010;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0002;
        const uint MOUSEEVENTF_RIGHTUP = 0x0004;

        // Keybord Key values
        const uint VK_RIGHT = 0x27;
        const uint VK_UP = 0x26;
        const uint VK_LEFT = 0x25;
        const uint VK_DOWN = 0x28;
        const uint VK_ALT = 0x12;
        const uint VK_CTRL = 0x11;
        const uint VK_TAB = 0x09;
        const uint VK_LWIN = 0x5B;

        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_EXTENDED = 0x0001;

        #endregion

        #region Dll declaration
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool keybd_event(uint bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        #endregion

        private void makeKeyPress(uint key)
        {
            keybd_event(key, 0, 0, 0);
            Thread.Sleep(50);
            keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
        }

        private void combineKeyPress(uint[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keybd_event(keys[i], 0, 0, 0);
            }

            Thread.Sleep(1000);

            for (int i = keys.Length - 1; i >= 0; i--)
            {
                keybd_event(keys[i], 0, KEYEVENTF_KEYUP | KEYEVENTF_EXTENDED, 0);
            }
        }

        public void WinDown()
        {
            this.combineKeyPress(new uint[] { VK_LWIN, VK_DOWN });
        }

        public void WinUp()
        {
            this.combineKeyPress(new uint[] { VK_UP, VK_DOWN });
        }


        /// <summary>
        /// Set cursor position by x,y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetCursorPosition(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }

        public void MinimizeActiveWindow()
        {
            uint minimize = 6;
            IntPtr currentWindow = GetForegroundWindow();
            ShowWindow(currentWindow, minimize);
        }

        public void MaximizeActiveWindow()
        {
            uint maximize = 3;
            IntPtr currentWindow = GetForegroundWindow();
            ShowWindow(currentWindow, maximize);
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

        public void CursorRightClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_RIGHTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
        }

        public void CursorLeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
        }

        public void KeyLeft()
        {
            this.makeKeyPress(VK_LEFT);
        }

        public void KeyRight()
        {
            this.makeKeyPress(VK_RIGHT);
        }

        public void KeyUp()
        {
            this.makeKeyPress(VK_UP);
        }

        public void KeyDown()
        {
            this.makeKeyPress(VK_DOWN);
        }

        public void AltCtrlTab()
        {

            uint[] altCtrlTab = {VK_ALT};

            this.combineKeyPress(new uint[] { VK_ALT,VK_CTRL, VK_TAB });
        }

        public void ResizeWindow(int size, Point location)
        {
            IntPtr a = GetForegroundWindow();
            WindowScrape.Types.HwndObject hw = new WindowScrape.Types.HwndObject(a);
            Point pnt = hw.Location;
            Size oldSize = hw.Size;
            
            MoveWindow(a, pnt.X - size/2 + location.X, pnt.Y - size/2 + location.Y, oldSize.Width + size, oldSize.Height + size, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCurrentWindowName()
        {
            IntPtr a = GetForegroundWindow();
            WindowScrape.Types.HwndObject hw = new WindowScrape.Types.HwndObject(a);
            return hw.Title;
        }


    }
}
