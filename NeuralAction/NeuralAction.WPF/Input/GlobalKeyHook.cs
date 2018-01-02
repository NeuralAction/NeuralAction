using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeuralAction.WPF
{
    public class GlobalKeyHookEventArgs : HandledEventArgs
    {
        public VKeyState KeyboardState { get; private set; }
        public LowLevelKeyInputArgs KeyboardData { get; private set; }

        public VKeyCode VKeyCode => (VKeyCode)KeyboardData.VirtualCode;

        public GlobalKeyHookEventArgs(LowLevelKeyInputArgs keyboardData, VKeyState keyboardState)
        {
            KeyboardData = keyboardData;
            KeyboardState = keyboardState;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LowLevelKeyInputArgs
    {
        /// <summary>
        /// A virtual-key code. The code must be a value in the range 1 to 254.
        /// </summary>
        public int VirtualCode;

        /// <summary>
        /// A hardware scan code for the key. 
        /// </summary>
        public int HardwareScanCode;

        /// <summary>
        /// The extended-key flag, event-injected Flags, context code, and transition-state flag. This member is specified as follows. An application can use the following values to test the keystroke Flags. Testing LLKHF_INJECTED (bit 4) will tell you whether the event was injected. If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether or not the event was injected from a process running at lower integrity level.
        /// </summary>
        public int Flags;

        /// <summary>
        /// The time stamp stamp for this message, equivalent to what GetMessageTime would return for this message.
        /// </summary>
        public int TimeStamp;

        /// <summary>
        /// Additional information associated with the message. 
        /// </summary>
        public IntPtr AdditionalInformation;
    }

    public enum VKeyState
    {
        KeyDown = 0x0100,
        KeyUp = 0x0101,
        SysKeyDown = 0x0104,
        SysKeyUp = 0x0105
    }

    public enum VKeyCode
    {
        LeftControl = 0xA2, RightControl = 0xA3,
        LeftShift = 0xA0, RightShift = 0xA1,
        LeftWin = 0x5B, RightWin = 0x5C,
        LeftAlt = 0xA4, RightAlt = 0xA5,

        A_Key = 0x41, B_Key = 0x42, C_Key = 0x43, D_Key = 0x44, E_Key = 0x45,
        F_Key = 0x46, G_Key = 0x47, H_Key = 0x48, I_Key = 0x49, J_Key = 0x4A,
        K_Key = 0x4B, L_Key = 0x4C, M_Key = 0x4D, N_Key = 0x4E, O_Key = 0x4F,
        P_Key = 0x50, Q_Key = 0x51, R_Key = 0x52, S_Key = 0x53, T_Key = 0x54,
        U_Key = 0x55, V_Key = 0x56, W_Key = 0x57, X_Key = 0x58, Y_Key = 0x59,
        Z_Key = 0x5A,

        F1 = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73, F5 = 0x74, F6 = 0x75,
        F7 = 0x76, F8 = 0x77, F9 = 0x78, F10 = 0x79, F11 = 0x7A, F12 = 0x7B,
    };

    //Based on https://gist.github.com/Stasonix
    public class GlobalKeyHook : IDisposable
    {
        public const int KfAltdown = 0x2000;
        public const int LlkhfAltdown = (KfAltdown >> 8);

        public const int WH_KEYBOARD_LL = 13;
        public const int HC_ACTION = 0;

        static GlobalKeyHook hook = new GlobalKeyHook();
        public static GlobalKeyHook Hook => hook;

        public event EventHandler<GlobalKeyHookEventArgs> KeyboardPressed;

        IntPtr windowsHookHandle;
        IntPtr user32LibraryHandle;
        HookProc hookProc;

        delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        GlobalKeyHook()
        {
            windowsHookHandle = IntPtr.Zero;
            user32LibraryHandle = IntPtr.Zero;
            hookProc = LowLevelKeyboardProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.

            user32LibraryHandle = LoadLibrary("User32");
            if (user32LibraryHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }

            windowsHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, user32LibraryHandle, 0);
            if (windowsHookHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }
        }

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatKeyStroke = false;

            var wparamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(VKeyState), wparamTyped))
            {
                object o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyInputArgs));
                LowLevelKeyInputArgs p = (LowLevelKeyInputArgs)o;

                var eventArguments = new GlobalKeyHookEventArgs(p, (VKeyState)wparamTyped);

                EventHandler<GlobalKeyHookEventArgs> handler = KeyboardPressed;
                handler?.Invoke(this, eventArguments);

                fEatKeyStroke = eventArguments.Handled;
            }

            return fEatKeyStroke ? (IntPtr)1 : CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // because we can unhook only in the same thread, not in garbage collector thread
                if (windowsHookHandle != IntPtr.Zero)
                {
                    if (!UnhookWindowsHookEx(windowsHookHandle))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }
                    windowsHookHandle = IntPtr.Zero;

                    // ReSharper disable once DelegateSubtraction
                    hookProc -= LowLevelKeyboardProc;
                }
            }

            if (user32LibraryHandle != IntPtr.Zero)
            {
                if (!FreeLibrary(user32LibraryHandle)) // reduces reference to library by 1.
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }
                user32LibraryHandle = IntPtr.Zero;
            }
        }

        ~GlobalKeyHook()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static bool IsKeyPressed(VKeyCode code)
        {
            return (GetKeyState(code) & 0x80) != 0;
        }

        public static short GetKeyState(VKeyCode code)
        {
            return GetKeyState((int)code);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short VkKeyScan(char keyChar);

        /// <summary>
        /// The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// </summary>
        /// <param name="hhk">handle to hook procedure</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        [DllImport("USER32", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// The SetWindowsHookEx function installs an application-defined hook procedure into a hook chain.
        /// You would install a hook procedure to monitor the system for certain types of events. These events are
        /// associated either with a specific thread or with all threads in the same desktop as the calling thread.
        /// </summary>
        /// <param name="idHook">hook type</param>
        /// <param name="lpfn">hook procedure</param>
        /// <param name="hMod">handle to application instance</param>
        /// <param name="dwThreadId">thread identifier</param>
        /// <returns>If the function succeeds, the return value is the handle to the hook procedure.</returns>
        [DllImport("USER32", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        /// <summary>
        /// The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain.
        /// A hook procedure can call this function either before or after processing the hook information.
        /// </summary>
        /// <param name="hHook">handle to current hook</param>
        /// <param name="code">hook code passed to hook procedure</param>
        /// <param name="wParam">value passed to hook procedure</param>
        /// <param name="lParam">value passed to hook procedure</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        [DllImport("USER32", SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);
    }
}
