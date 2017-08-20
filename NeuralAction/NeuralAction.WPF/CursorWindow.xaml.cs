using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media;

namespace NeuralAction.WPF
{
    /// <summary>
    /// CursorIcon.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CursorWindow : Window
    {
        public event EventHandler<Point> Moved;

        public double SpeedLimit { get; set; } = 4;
        public double Scale { get; set; } = 1;
        public bool Smooth { get; set; } = true;
        public double ActualLeft
        {
            get => Left * Scale;
            set
            {
                Left = value / Scale;
            }
        }
        public double ActualTop
        {
            get => Top * Scale;
            set
            {
                Top = value / Scale;
            }
        }
        public Vision.Point Point { get => new Vision.Point(ActualLeft + ActualWidth / 2, ActualTop + ActualHeight / 2); }
        public List<CursorTimes> MoveList { get; set; } = new List<CursorTimes>();
        public object MoveLocker { get; set; } = new object();

        Storyboard CursorOn;
        Storyboard CursorOff;
        DispatcherTimer moveTimer;
        Point _targetPt;
        Point targetPt
        {
            get => _targetPt;
            set
            {
                _targetPt = value;

                if (Smooth)
                {
                    if (moveTimer == null)
                    {
                        moveTimer = new DispatcherTimer();
                        moveTimer.Interval = TimeSpan.FromMilliseconds(15);
                        moveTimer.Tick += (sender, arg) =>
                        {
                            if (Math.Abs(targetPt.X - ActualLeft) + Math.Abs(targetPt.Y - ActualTop) < 1)
                            {
                                ActualLeft = targetPt.X;
                                ActualTop = targetPt.Y;
                                moveTimer.Stop();
                            }
                            else
                            {
                                ActualLeft += Clamp((targetPt.X - ActualLeft) / 15);
                                ActualTop += Clamp((targetPt.Y - ActualTop) / 15);
                            }
                            Topmost = false;
                            Topmost = true;
                            lock (MoveLocker)
                            {
                                MoveList.Add(new CursorTimes(Point, DateTime.Now.TimeOfDay));
                            }

                            if (show)
                            {
                                var pt = new Point(Point.X, Point.Y);
                                MouseEvent.MoveAt(pt);
                                Moved?.Invoke(this, pt);
                            }
                        };
                    }
                    moveTimer.Start();
                }
                else
                {
                    ActualLeft = targetPt.X;
                    ActualTop = targetPt.Y;

                    if (show)
                    {
                        var pt = new System.Windows.Point(Point.X, Point.Y);
                        MouseEvent.MoveAt(pt);
                        Moved?.Invoke(this, pt);
                    }
                }
            }
        }
        bool show = false;
        DispatcherTimer cursorAniWaiter;

        public CursorWindow()
        {
            SourceInitialized += CursorWindow_SourceInitialized;
            Loaded += CursorWindow_Loaded;

            InitializeComponent();

            CursorOff = (Storyboard)FindResource("CursorOff");
            CursorOff.Begin();
            CursorOn = (Storyboard)FindResource("CursorOn");
        }

        private void CursorWindow_SourceInitialized(object sender, EventArgs e)
        {
            WinApi.SetTransClick(this);
        }

        private void CursorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            targetPt = new Point(ActualLeft, ActualTop);
        }
        
        private double Clamp(double value)
        {
            double mul = 1;
            if (value < 0)
                mul = -1;
            value = Math.Abs(value);
            if (value > 3)
                value = Math.Sqrt(value * SpeedLimit);
            return value * mul;
        }

        public void Goto(Point pt)
        {
            Dispatcher.Invoke(() =>
            {
                if (moveTimer != null)
                    moveTimer.Stop();

                var pre = Smooth;
                Smooth = false;
                targetPt = new Point(pt.X - ActualWidth / 2, pt.Y - ActualHeight / 2);
                Smooth = pre;
            });
        }

        DispatcherTimer clickWait;
        public void Clicked()
        {
            Dispatcher.Invoke(() => 
            {
                Background = Brushes.Red;
                if (clickWait == null)
                {
                    clickWait = new DispatcherTimer();
                    clickWait.Tick += delegate
                    {
                        Background = null;
                        clickWait.Stop();
                    };
                    clickWait.Interval = TimeSpan.FromMilliseconds(250);
                }
                clickWait.Start();
            });
        }

        public void SetPosition(double x, double y)
        {
            Dispatcher.Invoke(() => 
            {
                targetPt = new Point(x - ActualWidth / 2, y - ActualHeight / 2);
            });
        }

        public void SetAvailable(bool value)
        {
            Dispatcher.Invoke(() =>
            {
                if(show != value)
                {
                    if(cursorAniWaiter == null)
                    {
                        cursorAniWaiter = new DispatcherTimer();
                        cursorAniWaiter.Interval = TimeSpan.FromMilliseconds(100);
                        cursorAniWaiter.Tick += (s, o) =>
                        {
                            if (show)
                            {
                                CursorOff.Stop();
                                CursorOn.Begin();
                            }
                            else
                            {
                                CursorOn.Stop();
                                CursorOff.Begin();
                            }

                            cursorAniWaiter.Stop();
                        };
                    }
                    cursorAniWaiter.Start();
                }

                show = value;
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Scale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
        }
    }
}
