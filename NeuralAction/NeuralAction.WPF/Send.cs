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

        static int InstanceCount = 0;
        public int uid;

        public string Content { get; set; }
        public string Name { get; set; }

        public Send(string name, string content)
        {
            uid = InstanceCount;
            InstanceCount++;

            Name = name;
            Content = content;
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public void Work()
        {
            IntPtr hwnd = MainWindow.FocusedHandle;

            if (hwnd == IntPtr.Zero && Content == null)
                return;

            try
            {


                string previousClip = Clipboard.GetText();

                if (previousClip == "{BACK}")
                {

                    SetForegroundWindow(MainWindow.FocusedHandle);

                    SendKeys.SendWait("{BACKSPACE}");
                    SendKeys.Flush();

                    if (MainWindow.RestoreClipboard)
                    {
                     
                        if (string.IsNullOrEmpty(previousClip))
                            Clipboard.SetText(previousClip);

         
                    }

                } else  {

             
                

                    Clipboard.SetText(Content);

                
                    SendKeys.SendWait("^(v)");
                    SendKeys.Flush();

                    if (MainWindow.RestoreClipboard)
                    {
                      
                        if (string.IsNullOrEmpty(previousClip))
                            Clipboard.SetText(previousClip);
                        
         

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR Keydata59:\n" + ex.ToString());
           
            }
        }

    }
}
