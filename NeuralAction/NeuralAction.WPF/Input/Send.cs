using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

using Clipboard = System.Windows.Clipboard;

namespace NeuralAction.WPF
{
    public class Send
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public static List<IntPtr> OwnedWindows = new List<IntPtr>();
        public static IntPtr FocusedHandle = IntPtr.Zero;
        public static bool RestoreClipboard = true;

        static int InstanceCount = 0;
        static Thread thread;

        static Send()
        {
            thread = new Thread(()=> 
            {
                while (true)
                {
                    HandleUpdateProc();
                    Thread.Sleep(100);
                }
            });
            thread.Start();
        }

        static void HandleUpdateProc()
        {
            var hwnd = GetForegroundWindow();

            foreach (var item in OwnedWindows)
            {
                if (item == hwnd)
                    return;
            }

            FocusedHandle = hwnd;
        }

        public static void AddWindow(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != IntPtr.Zero)
                OwnedWindows.Add(hwnd);
            else
            {
                window.Loaded += delegate
                {
                    hwnd = new WindowInteropHelper(window).Handle;
                    OwnedWindows.Add(hwnd);
                };
            }

            window.Closed += delegate
            {
                for (int i = 0; i < OwnedWindows.Count; i++)
                {
                    if (OwnedWindows[i] == hwnd)
                    {
                        OwnedWindows.RemoveAt(i);
                        i = 0;
                    }
                }
            };
        }

        public static void SendKey(string key)
        {
            IntPtr hwnd = FocusedHandle;
            if (hwnd == IntPtr.Zero)
                return;

            SetForegroundWindow(hwnd);
            var count = GetWindowTextLength(hwnd);
            var str = new StringBuilder(count);
            GetWindowText(hwnd, str, count);
            var title = str.ToString();

            SendKeys.SendWait(key);
            SendKeys.Flush();
        }

        public int UID;
        public string Content { get; set; }
        public string Name { get; set; }

        public Send(string name, string content)
        {
            UID = InstanceCount;
            InstanceCount++;

            Name = name;
            Content = content;
        }

        public void Work()
        {
            IntPtr hwnd = FocusedHandle;

            if (hwnd == IntPtr.Zero || Content == null)
                return;
            SetForegroundWindow(FocusedHandle);

            string previousClip = Clipboard.GetText();

            if (previousClip == "{BACK}")
            {
                SendKeys.SendWait("{BACKSPACE}");
                SendKeys.Flush();

                if (RestoreClipboard)
                {
                    if (string.IsNullOrEmpty(previousClip))
                        Clipboard.SetText(previousClip);
                }
            }
            else
            {
                Clipboard.SetText(Content);
                    
                SendKeys.SendWait("^(v)");
                SendKeys.Flush();

                if (RestoreClipboard)
                {
                    if (string.IsNullOrEmpty(previousClip))
                        Clipboard.SetText(previousClip);
                }
            }
        }
    }
}
