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
    public partial class CursorWindow : Window
    {
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

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            return lpPoint;
        }

        double scale;
        Timer mousemove = new Timer();

        public CursorWindow()
        {
            InitializeComponent();
        }

        void mousemove_event(object sender, EventArgs e)
        {
            Left = (int)(GetCursorPosition().X / scale - 25);
            Top = (int)(GetCursorPosition().Y / scale - 25);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            scale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

            mousemove.Interval = 1;
            mousemove.Tick += mousemove_event;
            mousemove.Enabled = true;
        }
    }
}
