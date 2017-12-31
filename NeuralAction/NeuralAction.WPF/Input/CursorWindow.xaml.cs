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

        public bool Smooth { get; set; } = true;
        public bool UseSpeedClamp { get; set; } = true;
        public double SpeedClamp { get; set; } = 100;
        
        public double ActualLeft
        {
            get => Left * WpfScale - parent.TargetScreen.Bounds.Left;
            set { Left = (value + parent.TargetScreen.Bounds.Left) / WpfScale; }
        }
        public double ActualTop
        {
            get => Top * WpfScale - parent.TargetScreen.Bounds.Top;
            set { Top = (value + parent.TargetScreen.Bounds.Top) / WpfScale; }
        }
        public Vision.Point ActualPosition => new Vision.Point(ActualLeft + ActualWidth / 2 * WpfScale, ActualTop + ActualHeight / 2 * WpfScale);

        public List<CursorTimes> MoveList { get; set; } = new List<CursorTimes>();
        public object MoveLocker { get; set; } = new object();

        /// <summary>
        /// Visibility of Cursor. If set hidden, it would be lost control.
        /// </summary>
        public new Visibility Visibility
        {
            get => base.Visibility;
            set => base.Visibility = value;
        }
        
        CursorService parent;
        DispatcherTimer moveTimer;
        Point targetPosition;
        Point TargetPosition
        {
            get => targetPosition;
            set { targetPosition = value; UpdateTargetPosition(); }
        }
        DispatcherTimer cursorAniWaiter;
        double WpfScale;
        bool show = false;
        bool AllowControl => parent.ControlAllowed && Visibility == Visibility.Visible;

        public CursorWindow(CursorService service)
        {
            parent = service;

            SourceInitialized += delegate
            {
                WinApi.SetTransClick(this);
            };

            InitializeComponent();

            var focus = new DispatcherTimer();
            focus.Interval = TimeSpan.FromMilliseconds(150);
            focus.Tick += delegate { Topmost = false; Topmost = true; };
            focus.Start();

            Loaded += delegate
            {
                TargetPosition = new Point(ActualLeft, ActualTop);
                WpfScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            };

            Closed += delegate 
            {
                focus.Stop();
            };
        }

        void UpdateTargetPosition()
        {
            if (Smooth)
            {
                if (moveTimer == null)
                {
                    moveTimer = new DispatcherTimer();
                    moveTimer.Interval = TimeSpan.FromMilliseconds(30);
                    moveTimer.Tick += (sender, arg) =>
                    {
                        var clm = UseSpeedClamp ? SpeedClamp : 1000000;
                        ActualLeft += Clamp((TargetPosition.X - ActualLeft) / 4, -clm, clm);
                        ActualTop += Clamp((TargetPosition.Y - ActualTop) / 4, -clm, clm);
                        InternalMove();
                    };
                }
                moveTimer.Start();
            }
            else
            {
                moveTimer?.Stop();
                ActualLeft = TargetPosition.X;
                ActualTop = TargetPosition.Y;
                InternalMove();
            }
        }

        void InternalMove()
        {
            lock (MoveLocker)
                MoveList.Add(new CursorTimes(ActualPosition, DateTime.Now.TimeOfDay));

            if (show)
            {
                var pt = new Point(ActualPosition.X, ActualPosition.Y);
                if(AllowControl)
                    MouseEvent.MoveAt(pt);
                Moved?.Invoke(this, pt);
            }
        }

        public void Move(Point pt)
        {
            Dispatcher.Invoke(() =>
            {
                if (moveTimer != null)
                    moveTimer.Stop();

                var pre = Smooth;
                Smooth = false;
                TargetPosition = new Point(pt.X - ActualWidth / 2 * WpfScale, pt.Y - ActualHeight / 2 * WpfScale);
                Smooth = pre;
            });
        }

        public void Clicked()
        {
            if (AllowControl)
                MouseEvent.Click(MouseButton.Left);
            Dispatcher.Invoke(() => 
            {
                CursorControl.Click();
            });
        }

        public void SetPosition(double x, double y)
        {
            Dispatcher.Invoke(() => 
            {
                TargetPosition = new Point(x - ActualWidth / 2 * WpfScale, y - ActualHeight / 2 * WpfScale);
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
                                CursorControl.Show();
                            else
                                CursorControl.Hide();

                            cursorAniWaiter.Stop();
                        };
                    }
                    cursorAniWaiter.Start();
                }

                show = value;
            });
        }

        double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
