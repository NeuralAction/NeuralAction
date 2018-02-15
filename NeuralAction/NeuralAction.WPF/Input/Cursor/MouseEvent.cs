using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MouseKeyboardLibrary
{
    using System.Windows.Forms;
    public abstract class GlobalHook
    {
        #region Windows API Code

        [StructLayout(LayoutKind.Sequential)]
        protected class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected class MouseLLHookStruct
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected class KeyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto,
           CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        protected static extern int SetWindowsHookEx(
            int idHook,
            HookProc lpfn,
            IntPtr hMod,
            int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        protected static extern int UnhookWindowsHookEx(int idHook);


        [DllImport("user32.dll", CharSet = CharSet.Auto,
             CallingConvention = CallingConvention.StdCall)]
        protected static extern int CallNextHookEx(
            int idHook,
            int nCode,
            int wParam,
            IntPtr lParam);

        [DllImport("user32")]
        protected static extern int ToAscii(int uVirtKey,
            int uScanCode,
            byte[] lpbKeyState,
            byte[] lpwTransKey,
            int fuState);

        [DllImport("user32")]
        protected static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        protected static extern short GetKeyState(int vKey);

        protected delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        protected const int WH_MOUSE_LL = 14;
        protected const int WH_KEYBOARD_LL = 13;

        protected const int WH_MOUSE = 7;
        protected const int WH_KEYBOARD = 2;
        protected const int WM_MOUSEMOVE = 0x200;
        protected const int WM_LBUTTONDOWN = 0x201;
        protected const int WM_RBUTTONDOWN = 0x204;
        protected const int WM_MBUTTONDOWN = 0x207;
        protected const int WM_LBUTTONUP = 0x202;
        protected const int WM_RBUTTONUP = 0x205;
        protected const int WM_MBUTTONUP = 0x208;
        protected const int WM_LBUTTONDBLCLK = 0x203;
        protected const int WM_RBUTTONDBLCLK = 0x206;
        protected const int WM_MBUTTONDBLCLK = 0x209;
        protected const int WM_MOUSEWHEEL = 0x020A;
        protected const int WM_KEYDOWN = 0x100;
        protected const int WM_KEYUP = 0x101;
        protected const int WM_SYSKEYDOWN = 0x104;
        protected const int WM_SYSKEYUP = 0x105;

        protected const byte VK_SHIFT = 0x10;
        protected const byte VK_CAPITAL = 0x14;
        protected const byte VK_NUMLOCK = 0x90;

        protected const byte VK_LSHIFT = 0xA0;
        protected const byte VK_RSHIFT = 0xA1;
        protected const byte VK_LCONTROL = 0xA2;
        protected const byte VK_RCONTROL = 0x3;
        protected const byte VK_LALT = 0xA4;
        protected const byte VK_RALT = 0xA5;

        protected const byte LLKHF_ALTDOWN = 0x20;

        #endregion

        #region Private Variables

        protected int _hookType;
        protected int _handleToHook;
        protected bool _isStarted;
        protected HookProc _hookCallback;

        #endregion

        #region Properties

        public bool IsStarted => _isStarted;

        #endregion

        #region Constructor
        public GlobalHook()
        {
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }
        ~GlobalHook()
        {
            Application_ApplicationExit(this, null);
        }
        #endregion

        #region Methods
        public void Start()
        {
            if (!_isStarted &&
                _hookType != 0)
            {

                // Make sure we keep a reference to this delegate!
                // If not, GC randomly collects it, and a NullReference exception is thrown
                _hookCallback = new HookProc(HookCallbackProcedure);

                _handleToHook = SetWindowsHookEx(
                    _hookType,
                    _hookCallback,
                    Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]),
                    0);

                // Were we able to sucessfully start hook?
                if (_handleToHook != 0)
                {
                    _isStarted = true;
                }
            }
        }

        public void Stop()
        {
            if (_isStarted)
            {
                UnhookWindowsHookEx(_handleToHook);

                _isStarted = false;
            }
        }

        protected virtual int HookCallbackProcedure(int nCode, Int32 wParam, IntPtr lParam)
        {
            // This method must be overriden by each extending hook
            return 0;
        }

        protected void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (_isStarted)
                Stop();
        }
        #endregion
    }

    /// <summary>
    /// Captures global mouse events
    /// </summary>
    public class MouseHook : GlobalHook
    {
        #region MouseEventType Enum
        private enum MouseEventType
        {
            None,
            MouseDown,
            MouseUp,
            DoubleClick,
            MouseWheel,
            MouseMove
        }
        #endregion

        #region Events

        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
        public event MouseEventHandler MouseWheel;

        public event EventHandler Click;
        public event EventHandler DoubleClick;

        #endregion

        #region Constructor
        public MouseHook()
        {
            _hookType = WH_MOUSE_LL;
        }
        #endregion

        #region Methods

        protected override int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode > -1 && (MouseDown != null || MouseUp != null || MouseMove != null))
            {
                MouseLLHookStruct mouseHookStruct =
                    (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));

                MouseButtons button = GetButton(wParam);
                MouseEventType eventType = GetEventType(wParam);

                MouseEventArgs e = new MouseEventArgs(
                    button,
                    (eventType == MouseEventType.DoubleClick ? 2 : 1),
                    mouseHookStruct.pt.x,
                    mouseHookStruct.pt.y,
                    (eventType == MouseEventType.MouseWheel ? (short)((mouseHookStruct.mouseData >> 16) & 0xffff) : 0));

                // Prevent multiple Right Click events (this probably happens for popup menus)
                if (button == MouseButtons.Right && mouseHookStruct.flags != 0)
                {
                    eventType = MouseEventType.None;
                }

                switch (eventType)
                {
                    case MouseEventType.MouseDown:
                        MouseDown?.Invoke(this, e);
                        break;
                    case MouseEventType.MouseUp:
                        Click?.Invoke(this, new EventArgs());
                        MouseUp?.Invoke(this, e);
                        break;
                    case MouseEventType.DoubleClick:
                        DoubleClick?.Invoke(this, new EventArgs());
                        break;
                    case MouseEventType.MouseWheel:
                        MouseWheel?.Invoke(this, e);
                        break;
                    case MouseEventType.MouseMove:
                        MouseMove?.Invoke(this, e);
                        break;
                    default:
                        break;
                }
            }
            return CallNextHookEx(_handleToHook, nCode, wParam, lParam);
        }

        private MouseButtons GetButton(Int32 wParam)
        {
            switch (wParam)
            {
                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                case WM_LBUTTONDBLCLK:
                    return MouseButtons.Left;
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                case WM_RBUTTONDBLCLK:
                    return MouseButtons.Right;
                case WM_MBUTTONDOWN:
                case WM_MBUTTONUP:
                case WM_MBUTTONDBLCLK:
                    return MouseButtons.Middle;
                default:
                    return MouseButtons.None;
            }
        }

        private MouseEventType GetEventType(Int32 wParam)
        {
            switch (wParam)
            {
                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_MBUTTONDOWN:
                    return MouseEventType.MouseDown;
                case WM_LBUTTONUP:
                case WM_RBUTTONUP:
                case WM_MBUTTONUP:
                    return MouseEventType.MouseUp;
                case WM_LBUTTONDBLCLK:
                case WM_RBUTTONDBLCLK:
                case WM_MBUTTONDBLCLK:
                    return MouseEventType.DoubleClick;
                case WM_MOUSEWHEEL:
                    return MouseEventType.MouseWheel;
                case WM_MOUSEMOVE:
                    return MouseEventType.MouseMove;
                default:
                    return MouseEventType.None;
            }
        }

        #endregion
    }
}

namespace NeuralAction.WPF
{
    [Flags()]
    public enum MouseEventFlag : int
    {
        Absolute = 0x8000,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        Move = 0x0001,
        RightDown = 0x0008,
        RightUp = 0x0010,
        Wheel = 0x0800,
        XDown = 0x0080,
        XUp = 0x0100,
        HWheel = 0x1000,
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle,
        X,
    }

    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }

    public class MouseEvent
    {
        const int ABSOLUTE_SIZE = 65535;
        public static bool AllowControl { get; private set; } = true;
        public static Size ActualDisplaySize { set; get; }

        static MouseEvent()
        {
            var bound = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            ActualDisplaySize = new Size(bound.Width, bound.Height);

            GlobalKeyHook.Hook.KeyboardPressed += Hook_KeyboardPressed;
        }

        ~MouseEvent()
        {
            GlobalKeyHook.Hook.KeyboardPressed -= Hook_KeyboardPressed;
        }

        static void Hook_KeyboardPressed(object sender, GlobalKeyHookEventArgs e)
        {
            switch (e.VKeyCode)
            {
                case VKeyCode.LeftControl:
                    switch (e.KeyboardState)
                    {
                        case VKeyState.KeyDown:
                            AllowControl = false;
                            break;
                        case VKeyState.KeyUp:
                            AllowControl = true;
                            break;
                    }
                    break;
            }
        }

        public static void Move(Point Point)
        {
            MouseEventFlag Flag = MouseEventFlag.Move;

            Event((int)Flag, (int)Point.X, (int)Point.Y, 0, IntPtr.Zero);
        }

        public static void MoveAt(Point Point)
        {
            MouseEventFlag Flag = MouseEventFlag.Move | MouseEventFlag.Absolute;

            int X = (int)((ABSOLUTE_SIZE / ActualDisplaySize.Width) * Point.X);
            int Y = (int)((ABSOLUTE_SIZE / ActualDisplaySize.Height) * Point.Y);

            Event((int)Flag, X, Y, 0, IntPtr.Zero);
        }

        public static void MoveAbsolute(Point Point)
        {
            MouseEventFlag Flag = MouseEventFlag.Move | MouseEventFlag.Absolute;

            Event((int)Flag, (int)Point.X, (int)Point.Y, 0, IntPtr.Zero);
        }

        public static void Click(MouseButton button)
        {
            Down(button);
            Up(button);
        }

        public static void Down(MouseButton Button)
        {
            MouseEventFlag Flag;
            switch (Button)
            {
                case MouseButton.Left:
                    Flag = MouseEventFlag.LeftDown;
                    break;
                case MouseButton.Right:
                    Flag = MouseEventFlag.RightDown;
                    break;
                case MouseButton.Middle:
                    Flag = MouseEventFlag.MiddleDown;
                    break;
                case MouseButton.X:
                    Flag = MouseEventFlag.XDown;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Event((int)Flag, 0, 0, 0, IntPtr.Zero);
        }

        public static void Up(MouseButton Button)
        {
            MouseEventFlag Flag;
            switch (Button)
            {
                case MouseButton.Left:
                    Flag = MouseEventFlag.LeftUp;
                    break;
                case MouseButton.Right:
                    Flag = MouseEventFlag.RightUp;
                    break;
                case MouseButton.Middle:
                    Flag = MouseEventFlag.MiddleUp;
                    break;
                case MouseButton.X:
                    Flag = MouseEventFlag.XUp;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Event((int)Flag, 0, 0, 0, IntPtr.Zero);
        }

        public static void Scroll(int delta)
        {
            Event((int)MouseEventFlag.Wheel, 0, 0, delta, IntPtr.Zero);
        }

        public static Point GetCursorPosition()
        {
            WinApi.GetCursorPos(out POINT lpPoint);

            return lpPoint;
        }

        static void Event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo)
        {
            if (AllowControl)
                WinApi.MouseEvent(dwFlags, dx, dy, dwData, dwExtraInfo);
        }
    }
}
