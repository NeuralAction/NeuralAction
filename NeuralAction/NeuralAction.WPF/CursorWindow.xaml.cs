using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace NeuralAction.WPF
{
    /// <summary>
    /// CursorIcon.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CursorWindow : Window
    {
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
                                ActualLeft += (targetPt.X - ActualLeft) * 0.6;
                                ActualTop += (targetPt.Y - ActualTop) * 0.6;
                            }
                        };
                    }
                    moveTimer.Start();
                }
                else
                {
                    ActualLeft = targetPt.X;
                    ActualTop = targetPt.Y;
                }
            }
        }
        bool show = false;

        public CursorWindow()
        {
            Loaded += CursorWindow_Loaded;

            InitializeComponent();

            CursorOff = (Storyboard)FindResource("CursorOff");
            CursorOff.Begin();
            CursorOn = (Storyboard)FindResource("CursorOn");
        }

        private void CursorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            targetPt = new Point(ActualLeft, ActualTop);
        }

        public void SetPosition(double x, double y)
        {
            Dispatcher.Invoke(() => 
            {
                targetPt = new Point(x - ActualWidth / 2, y - ActualHeight / 2);
            });
        }

        DispatcherTimer cursorAniWaiter;

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
