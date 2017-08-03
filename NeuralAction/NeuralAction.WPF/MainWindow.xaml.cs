using Microsoft.Expression.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace NeuralAction.WPF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        String currentlanguage = "en";

        DivideKorean DK = new DivideKorean();

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr FocusedHandle { get; set; }

        public static bool RestoreClipboard { get; set; } = true;

        int inputcount = 0;

        public MainWindow()
        {
            InitializeComponent();
            
        }

        public void Inputing(String text)
        {

       
           for(int i = 0; i < text.Length; i++)
            {
                SendKeys.Send(text[i].ToString());
                System.Windows.MessageBox.Show(DivideKorean.DivideJaso("뭐"));
            }
        }


        CursorIcon CursorIcon = new CursorIcon();

        private void blankpie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.MessageBox.Show("blankpie");
        }

        private void markpie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.MessageBox.Show("markpie");
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            NotWindowsFocus();
      

        }

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

            this.Topmost = false;
            CursorIcon.Topmost = true;


        }

       void NotWindowsFocus() {
            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
            CursorIcon.Show();
        }

        private void Inputing_eyes(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if(sender.GetType().ToString() == "System.Windows.Controls.TextBlock")
            {

                string RealSendKey = ((TextBlock)sender).Tag.ToString();

                if (RealSendKey == "Backspace")
                {
                    RealSendKey = "{BACK}";
                }

                System.Windows.Forms.Clipboard.SetText(RealSendKey);

                Send sendkeys = new Send(((TextBlock)sender).Tag.ToString(), RealSendKey);

                sendkeys.Work();

            } else if(sender.GetType().ToString() == "Microsoft.Expression.Shapes.Arc") {

                string RealSendKey = ((Arc)sender).Tag.ToString();

                if (RealSendKey == "Backspace")
                {
                    RealSendKey = "{BACK}";
                }

                System.Windows.Forms.Clipboard.SetText(RealSendKey);

                Send sendkeys = new Send(((Arc)sender).Tag.ToString(), RealSendKey);

                sendkeys.Work();
            }

        }

        private void KeypadRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NotWindowsFocus();
        }
    }
}
