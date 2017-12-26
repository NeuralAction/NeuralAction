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
        /// <summary>
        /// actual cursor point
        /// </summary>
        public Vision.Point Point { get => new Vision.Point(ActualLeft + ActualWidth / 2, ActualTop + ActualHeight / 2); }
        public List<CursorTimes> MoveList { get; set; } = new List<CursorTimes>();
        public object MoveLocker { get; set; } = new object();

        Storyboard CursorOn;
        Storyboard CursorOff;
        Storyboard CursorClick;
        Storyboard CursorClickOff;

        DispatcherTimer moveTimer;
        Point targetPt;
        Point TargetPt
        {
            get => targetPt;
            set
            {
                targetPt = value;

                if (Smooth)
                {
                    if (moveTimer == null)
                    {
                        moveTimer = new DispatcherTimer();
                        moveTimer.Interval = TimeSpan.FromMilliseconds(30);
                        moveTimer.Tick += (sender, arg) =>
                        {
                            ActualLeft += (TargetPt.X - ActualLeft) / 4;
                            ActualTop += (TargetPt.Y - ActualTop) / 4;

                            Topmost = false;
                            Topmost = true;

                            lock (MoveLocker)
                                MoveList.Add(new CursorTimes(Point, DateTime.Now.TimeOfDay));

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
                    moveTimer?.Stop();

                    ActualLeft = TargetPt.X;
                    ActualTop = TargetPt.Y;

                    if (show)
                    {
                        var pt = new Point(Point.X, Point.Y);
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
            CursorClick = (Storyboard)FindResource("CursorClick");
            CursorClickOff = (Storyboard)FindResource("CursorClickOff");
        }

        private void CursorWindow_SourceInitialized(object sender, EventArgs e)
        {
            WinApi.SetTransClick(this);
        }

        private void CursorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TargetPt = new Point(ActualLeft, ActualTop);
        }

        public void Goto(Point pt)
        {
            Dispatcher.Invoke(() =>
            {
                if (moveTimer != null)
                    moveTimer.Stop();

                var pre = Smooth;
                Smooth = false;
                TargetPt = new Point(pt.X - ActualWidth / 2, pt.Y - ActualHeight / 2);
                Smooth = pre;
            });
        }

        DispatcherTimer clickWait;
        public void Clicked()
        {
            Dispatcher.Invoke(() => 
            {
                CursorClickOff.Stop();
                CursorClick.Begin();
                if (clickWait == null)
                {
                    clickWait = new DispatcherTimer();
                    clickWait.Tick += delegate
                    {
                        CursorClick.Stop();
                        CursorClickOff.Begin();
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
                TargetPt = new Point(x - ActualWidth / 2, y - ActualHeight / 2);
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
                        cursorAniWaiter.Tick += delegate
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
