using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeuralAction.WPF
{
    public class Send
    {
        public static IntPtr FocusedHandle { get; set; }
        public static bool RestoreClipboard { get; set; } = true;
        static int InstanceCount = 0;

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

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public void Work()
        {
            IntPtr hwnd = FocusedHandle;

            if (hwnd == IntPtr.Zero && Content == null)
                return;

            try
            {
                string previousClip = Clipboard.GetText();

                if (previousClip == "{BACK}")
                {
                    SetForegroundWindow(FocusedHandle);

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
            catch (Exception ex)
            {
                MessageBox.Show("ERROR Keydata 72:\n" + ex.ToString());
            }
        }
    }
}
