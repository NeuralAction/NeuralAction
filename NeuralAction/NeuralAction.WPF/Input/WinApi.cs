using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NeuralAction.WPF
{
    public class WinApi
    {
        public enum GWL
        {
            ExStyle = -20
        }

        public enum WS_EX
        {
            Transparent = 0x20,
            Layered = 0x80000,
            NoActivate = 0x08000000,
        }

        public enum LWA
        {
            ColorKey = 0x1,
            Alpha = 0x2
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, int crKey, byte alpha, LWA dwFlags);

        [DllImport("user32.dll")]
        public static extern int GetDpiForWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "mouse_event", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void MouseEvent(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static int GetDpi()
        {
            return GetDpiForWindow(GetDesktopWindow());
        }

        public static void SetTransClick(Window e)
        {
            IntPtr handle = new WindowInteropHelper(e).Handle;

            SetTransClick(handle);
        }

        public static void SetTransClick(IntPtr handle)
        {
            int wl = GetWindowLong(handle, GWL.ExStyle);
            wl = wl | 0x80000 | 0x20;
            SetWindowLong(handle, GWL.ExStyle, wl);
        }

        public static void NotWindowsFocus(Window w)
        {
            WindowInteropHelper helper = new WindowInteropHelper(w);
            NotWindowsFocus(helper.Handle);
        }

        public static void NotWindowsFocus(IntPtr handle)
        {
            SetWindowLong(handle, (int)GWL.ExStyle, GetWindowLong(handle, GWL.ExStyle) | (int)WS_EX.NoActivate);
        }
    }
}
