using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NeuralAction.WPF.Magnify
{
    /// <summary>
    /// MagnifyingGlassWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MagnifyingCursor : Window
    {
        public MagnifyingGlass Model { get; set; }

        public double ActualTop
        {
            get => (Top + ActualHeight / 2) * wpfScale;
        }

        public double ActualLeft
        {
            get => (Left + ActualWidth / 2) * wpfScale;
        }

        bool available = false;
        public bool Available
        {
            get => available;
            set
            {
                if(available != value)
                {
                    available = value;
                    if (value)
                        CursorControl.Show();
                    else
                        CursorControl.Hide();
                }
            }
        }

        double actualW, actualH;
        double wpfScale = InputService.Current.Cursor.Window.WpfScale;
        IntPtr hwnd;

        public MagnifyingCursor(MagnifyingGlass Model)
        {
            Send.AddWindow(this);

            InitializeComponent();

            this.Model = Model;
            DataContext = Model;

            Owner = App.Current.MainWindow;

            Loaded += delegate
            {
                actualW = ActualWidth;
                actualH = ActualHeight;
                hwnd = new WindowInteropHelper(this).Handle;
                WinApi.SetTransClick(this);
                WinApi.NotWindowsFocus(this);
            };
        }

        public Point GetActualPosition()
        {
            var rect = new RECT();
            NativeMethods.GetWindowRect((int)hwnd, ref rect);
            return new Point(rect.left + actualW * wpfScale / 2, rect.top + actualH * wpfScale / 2);
        }

        public void SetActualPosition(double x, double y)
        {
            x = x - actualW * wpfScale / 2;
            y = y - actualH * wpfScale / 2;
            NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, (int)x, (int)y, 0, 0, (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOSIZE);
        }

        public void Click(bool click = true)
        {
            if(Settings.Current.AllowControl && Settings.Current.AllowClick && click)
                MouseEvent.Click(MouseButton.Left);
            CursorControl.Click();
            Topmost = false;
            Topmost = true;
        }
    }
}
