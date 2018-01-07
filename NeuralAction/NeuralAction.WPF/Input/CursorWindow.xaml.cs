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
using System.Windows.Interop;

using Native = NeuralAction.WPF.Magnify.NativeMethods;
using SetWindowPosFlags = NeuralAction.WPF.Magnify.SetWindowPosFlags;

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
        public double WpfScale { get; set; }

        public double ActualLeft
        {
            get => Left * WpfScale - parent.TargetScreen.Bounds.Left;
        }
        public double ActualTop
        {
            get => Top * WpfScale - parent.TargetScreen.Bounds.Top;
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
        bool show = false;
        bool AllowControl => parent.ControlAllowed && Visibility == Visibility.Visible;

        IntPtr hwnd;
        double actualW, actualH;

        public CursorWindow(CursorService service)
        {
            Send.AddWindow(this);

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
                actualW = ActualWidth;
                actualH = ActualHeight;
                hwnd = new WindowInteropHelper(this).Handle;
                WpfScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
                TargetPosition = new Point(ActualLeft, ActualTop);
            };

            Closed += delegate 
            {
                focus.Stop();
            };
        }
        
        void SetActualPosition(double x, double y)
        {
            var scr = parent.TargetScreen.Bounds;
            x += scr.Left;
            y += scr.Top;
            Native.SetWindowPos(hwnd, IntPtr.Zero, (int)x, (int)y, 0, 0, (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOSIZE);
        }

        void UpdateTargetPosition()
        {
            if (Smooth)
            {
                if (moveTimer == null)
                {
                    moveTimer = new DispatcherTimer(DispatcherPriority.Render);
                    moveTimer.Interval = TimeSpan.FromMilliseconds(20);
                    moveTimer.Tick += (sender, arg) =>
                    {
                        var clm = UseSpeedClamp ? SpeedClamp : 1000000;
                        SetActualPosition(ActualLeft + Clamp((TargetPosition.X - ActualLeft) / 4, -clm, clm), ActualTop + Clamp((TargetPosition.Y - ActualTop) / 4, -clm, clm));
                        InternalMove();
                    };
                }
                moveTimer.Start();
            }
            else
            {
                moveTimer?.Stop();
                SetActualPosition(TargetPosition.X, TargetPosition.Y);
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

        public Vision.Point Clicked(bool click = true)
        {
            if (AllowControl && parent.ClickAllowed && click)
                MouseEvent.Click(MouseButton.Left);
            Vision.Point pt = null;
            Dispatcher.Invoke(() =>
            {
                CursorControl.Click();
                pt = ActualPosition;
            });
            return pt;
        }

        public void SetPosition(double x, double y)
        {
            TargetPosition = new Point(x - actualW / 2 * WpfScale, y - actualH / 2 * WpfScale);
        }

        public void SetAvailable(bool value)
        {
            if (show != value)
            {
                if (cursorAniWaiter == null)
                {
                    cursorAniWaiter = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
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
        }

        double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
