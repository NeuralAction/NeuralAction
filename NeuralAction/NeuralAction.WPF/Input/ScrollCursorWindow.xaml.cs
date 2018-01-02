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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Vision;

using Point = System.Windows.Point;

namespace NeuralAction.WPF
{
    /// <summary>
    /// ScrollCursorWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScrollCursorWindow : Window
    {
        public double ActualTop
        {
            get => (Top + ActualHeight / 2) * wpfScale;
            set => Top = value / wpfScale - ActualHeight / 2;
        }

        public double ActualLeft
        {
            get => (Left + ActualWidth / 2) * wpfScale;
            set => Left = value / wpfScale - ActualWidth / 2;
        }

        MeanSmoother mean = new MeanSmoother() { QueueCount = 10 };
        KalmanFilter kalman = new KalmanFilter();
        DispatcherTimer timer;
        Point position;

        double wpfScale = InputService.Current.Cursor.Window.WpfScale;
        double distY = 0;
        double screenH;

        public ScrollCursorWindow(Point position)
        {
            Send.AddWindow(this);

            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(20);
            timer.Tick += Timer_Tick;
            timer.Start();

            this.position = position;
            screenH = InputService.Current.TargetScreen.Bounds.Height;

            Loaded += delegate
            {
                ActualLeft = position.X;
                ActualTop = position.Y;

                WinApi.NotWindowsFocus(this);
                WinApi.SetTransClick(this);
            };

            Closed += delegate
            {
                timer.Stop();
            };

            if (Settings.Current.AllowControl)
                MouseEvent.MoveAt(position);
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            var pt = MouseEvent.GetCursorPosition();
            var distY = (pt.Y - ActualTop) / screenH;
            var delta = (distY * distY) * screenH;

            if(distY > 0)
            {
                RectBot.Height = delta;
                delta *= -1;
            }
            else
            {
                Rect.Height = delta;
            }

            delta = kalman.Calculate(mean.Smooth(delta));

            if (Settings.Current.AllowControl)
            {
                MouseEvent.MoveAt(position);
                MouseEvent.Scroll((int)Math.Floor(delta));
                MouseEvent.MoveAt(pt);
            }
        }
    }
}
