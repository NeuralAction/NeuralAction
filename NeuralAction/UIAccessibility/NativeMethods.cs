// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
// All other rights reserved.

using System;
using System.Text;
using System.Runtime.InteropServices;
using Accessibility;

namespace UIAccessibility
{
    internal static class NativeMethods
    {
        //
        // Primitives
        //
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            public bool IsEmpty
            {
                get
                {
                    return left >= right || top >= bottom;
                }
            }
            static public RECT Empty
            {
                get
                {
                    return new RECT(0, 0, 0, 0);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int x;
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public POINT(Point pt)
            {
                x = (int)pt.X;
                y = (int)pt.Y;
            }
        }

        internal const int GW_HWNDNEXT = 2;
        internal const int GW_CHILD = 5;
        internal const int WM_CLOSE = 0x0010;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct GUITHREADINFO
        {
            public int cbSize;
            public int dwFlags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rc;
        }
        
        public static string GetClassName(IntPtr hwnd)
        {
            StringBuilder builder = new StringBuilder(255);
            GetClassName(hwnd, builder, builder.Capacity + 1);
            return builder.ToString();
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref POINT lpPoint);

        public static POINT GetCursorPos()
        {
            var pt = new POINT();
            GetCursorPos(ref pt);
            return pt;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool GetGUIThreadInfo(int idThread, ref GUITHREADINFO guiThreadInfo);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetForegroundWindow(IntPtr hwnd);

        internal const int GA_PARENT = 1;
        internal const int GA_ROOT = 2;
        internal const int GA_ROOTOWNER = 3;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr GetAncestor(IntPtr hwnd, int gaFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hwnd, ref RECT rc);

        internal const int SRCCOPY = 0x00CC0020;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr GetDC(IntPtr hwnd);

        // Simplified version of ReleaseDC
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetClassName(IntPtr hWnd, StringBuilder classname, int nMax);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool IsWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int nMax);

        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        internal const int MAX_PATH = 260;
        internal static string GetWindowText(IntPtr hWnd)
        {
            System.Text.StringBuilder windowName = new System.Text.StringBuilder(MAX_PATH + 1);
            GetWindowText(hWnd, windowName, MAX_PATH);
            return windowName.ToString();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetDesktopWindow();

        [DllImport("kernel32.dll")]
        internal static extern int GetCurrentProcessId();

        #region Accessibility
        internal const int SELFLAG_NONE = 0x00000000;
        internal const int SELFLAG_TAKEFOCUS = 0x00000001;
        internal const int SELFLAG_TAKESELECTION = 0x00000002;
        internal const int SELFLAG_EXTENDSELECTION = 0x00000004;
        internal const int SELFLAG_ADDSELECTION = 0x00000008;
        internal const int SELFLAG_REMOVESELECTION = 0x00000010;
        internal const int SELFLAG_VALID = 0x0000001F;

        internal const int CHILD_SELF = 0x0;


        [DllImport("oleacc.dll")]
        internal static extern int AccessibleObjectFromPoint(POINT pt, [In, Out] ref IAccessible ppvObject, [In, Out] ref object varChild);

        [DllImport("oleacc.dll")]
        internal static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint id, ref Guid iid,
            [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);

        [DllImport("oleacc.dll")]
        internal static extern int WindowFromAccessibleObject(IAccessible acc, ref IntPtr hwnd);

        [DllImport("user32.dll")]
        internal static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        // Overload for IAccessibles, much user friendly.
        internal static int AccessibleObjectFromWindow(IntPtr hwnd, uint idObject, ref IAccessible acc)
        {
            Guid IID_IUnknown = new Guid(0x00000000, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

            object obj = null;
            int hr = AccessibleObjectFromWindow(hwnd, idObject, ref IID_IUnknown, ref obj);

            acc = (IAccessible)obj;
            return hr;
        }

        [DllImport("oleacc.dll")]
        internal static extern int AccessibleChildren(Accessibility.IAccessible paccContainer, int iChildStart, int cChildren, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2), In, Out] object[] rgvarChildren, out int pcObtained);

        [DllImport("oleacc.dll")]
        internal static extern int AccessibleObjectFromEvent(
            IntPtr hwnd,
            int idObject,
            int idChild,
            [In, Out] ref IAccessible ppvObject,
            [In, Out] ref Object varChild);

        internal const int ChildIdSelf = 0;
        internal const int StateSystemFocused = 0x00000004;
        internal const int StateSystemUnavailable = 0x00000001;
        internal const int ObjIdCaret = -8;
        internal const int ObjIdClient = -4;
        internal const int ObjIdWindow = 0;

        [DllImport("oleacc.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetRoleText(int dwRole, StringBuilder lpszRole, uint cchRoleMax);
        #endregion

        #region COM
        internal static Guid IID_IUnknown = new Guid(0x00000000, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);
        internal static Guid IID_IAccessible = new Guid(0x618736e0, 0x3c3d, 0x11cf, 0x81, 0x0c, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

        internal const int S_OK = 0;
        #endregion
    }
}
