using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NeuralAction.WPF
{
    /// <summary>
    /// CursorIcon.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CursorIcon : Window
    {

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

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }


        int currentDPI;
        int scale;

        public CursorIcon()
        {
            InitializeComponent();
        }

        Timer mousemove = new Timer();

       void mousemove_event(object sender, System.EventArgs e)
        {

          
            this.Left = (int) (GetCursorPosition().X / scale - 25);


            this.Top = (int) (GetCursorPosition().Y / scale - 25);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

           
         
            
            scale = (int) PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            mousemove.Interval = 1;
            mousemove.Tick += new EventHandler(mousemove_event);
            mousemove.Enabled = true;
     


        }
    }
}
