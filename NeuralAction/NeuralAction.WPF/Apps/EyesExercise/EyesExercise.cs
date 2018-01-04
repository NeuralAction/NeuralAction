using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace NeuralAction.WPF.Apps.EyesExercise
{
    public class EyesExercise : IDisposable {

        public static EyesExercise Current = new EyesExercise();

        public bool IsShowed { get; set; } = false;

        EyesExerciseWindow Window;

        Timer moveUpdater;

        Stopwatch stopwatch;

        double screenwidth = System.Windows.SystemParameters.PrimaryScreenWidth;
        double screenheight = System.Windows.SystemParameters.PrimaryScreenHeight;

        public EyesExercise()
        {

            Window = new EyesExerciseWindow();

        }


        public void Show()
        {
            var scr = InputService.Current.TargetScreen.Bounds;
            Window.MainTitle.Text = "wait a second...";
            Window.TimerProgress.Height = screenheight * 0.35;
            Window.Show();
            stopwatch = new Stopwatch();
            if (moveUpdater == null)
            {
                moveUpdater = new Timer();
                moveUpdater.Interval = 10;
                moveUpdater.Tick += MoveUpdater_Tick;
            }
            moveUpdater.Start();


            var cursor = InputService.Current.Cursor;
            cursor.Window.Visibility = Visibility.Visible;
            GlobalKeyHook.Hook.KeyboardPressed += Hook_KeyboardPressed;
            IsShowed = true;
        }

        public void Close()
        {
            GlobalKeyHook.Hook.KeyboardPressed -= Hook_KeyboardPressed;

            Window.Hide();

            moveUpdater.Stop();
            IsShowed = false;
        }

        void Hook_KeyboardPressed(object sender, GlobalKeyHookEventArgs e)
        {
            if (e.VKeyCode == VKeyCode.F4)
            {
                if (GlobalKeyHook.IsKeyPressed(VKeyCode.LeftControl) && GlobalKeyHook.IsKeyPressed(VKeyCode.LeftAlt))
                {
                    Close();
                }
            }
        }

        int step = 0;

        void MoveUpdater_Tick(object sender, EventArgs e)
        {

            if (InputService.Current.Cursor.Position != null)
            {

                stopwatch.Start();

            if (stopwatch.Elapsed.Seconds == 5 && step != 4)
            {
                step++;
                stopwatch.Reset();
                Window.TimerProgress.Value = 0;
                }

            if (stopwatch.Elapsed.Seconds == 10)
            {
                step++;
                stopwatch.Reset();
                Window.TimerProgress.Value = 0;
                }

            if (step == 0 || step == 1)
            {
                Window.EyesExerciseArrow.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            }

            if (step == 2 || step == 5 || step == 8)
            {
                Window.EyesExerciseArrow.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            }

            if (step == 3 || step == 6 || step == 7)
            {
                Window.EyesExerciseArrow.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            }

            if(step == 0 || step == 5 || step == 7)
            {
                Window.EyesExerciseArrow.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            }

            if (step == 1 || step == 6 || step == 8)
            {
                Window.EyesExerciseArrow.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            }

            if (step == 2 || step == 3)
            {
                Window.EyesExerciseArrow.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            }


            if (step == 0)
            {
                    Window.EyesExerciseArrow.RenderTransform = new RotateTransform(90, 0,0);
                    Window.TimerProgress.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    Window.TimerProgress.Margin = new Thickness(0,0,0,0);
                    if (InputService.Current.Cursor.Position.Y <= screenheight * 0.35)
                    {
                        stopwatch.Start();
                        Window.TimerProgress.Value += 2.2;
                    } else {
                        stopwatch.Stop();
                    }
                    Window.MainTitle.Text = "Looking at top without eyes-focus for " + (5 - stopwatch.Elapsed.Seconds);
            }

            if (step == 1)
            {
                    Window.EyesExerciseArrow.RenderTransform = new RotateTransform(270, 0, 0);
                    Window.TimerProgress.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    Window.TimerProgress.Margin = new Thickness(0,screenheight - (screenheight * 0.35),0,0);
                    if (InputService.Current.Cursor.Position.Y >= screenheight - (screenheight * 0.35))
                    {
                        stopwatch.Start();
                        Window.TimerProgress.Value += 2.2;
                    }
                    else
                    {
                        stopwatch.Stop();
                    }

                    Window.MainTitle.Text = "Looking at bottom without eyes-focus for " + (5 - stopwatch.Elapsed.Seconds);
            }

            if (step == 2)
            {
                    Window.EyesExerciseArrow.RenderTransform = new RotateTransform(0, 0, 0);
                    Window.TimerProgress.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    Window.TimerProgress.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    Window.TimerProgress.Width = screenwidth * 0.35;
                    Window.TimerProgress.Height = screenheight;
                    Window.TimerProgress.Orientation = System.Windows.Controls.Orientation.Vertical;
                    Window.TimerProgress.Margin = new Thickness(0, 0, 0, 0);
                    if (InputService.Current.Cursor.Position.X <= screenwidth * 0.35)
                    {
                        stopwatch.Start();
                        Window.TimerProgress.Value += 2.2;
                    }
                    else
                    {
                        stopwatch.Stop();
                    }
                    Window.MainTitle.Text = "Looking at left without eyes-focus for " + (5 - stopwatch.Elapsed.Seconds);
            }

            if (step == 3)
            {
                    Window.EyesExerciseArrow.RenderTransform = new RotateTransform(180, 0, 0);
                    Window.TimerProgress.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    Window.TimerProgress.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    if (InputService.Current.Cursor.Position.X >= screenwidth - (screenwidth * 0.35))
                    {
                        stopwatch.Start();
                        Window.TimerProgress.Value += 2.2;
                    }
                    else
                    {
                        stopwatch.Stop();
                    }
                    Window.MainTitle.Text = "Looking at right without eyes-focus for " + (5 - stopwatch.Elapsed.Seconds);
            }

            if (step == 4)
            {
                    Window.TimerProgress.Width = screenwidth;
                    Window.TimerProgress.Height = screenheight;
                    Window.TimerProgress.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    Window.TimerProgress.Orientation = System.Windows.Controls.Orientation.Horizontal;
                    Window.EyesExerciseArrow.Opacity = 0;
                    Window.MainTitle.Text = "Close your eyes for " + (10 - stopwatch.Elapsed.Seconds);
            }

            if (step == 5)
            {
                Window.EyesExerciseArrow.Opacity = 1;
                Window.EyesExerciseArrow.RenderTransform = new RotateTransform(45, 0, 0);
                Window.MainTitle.Text = "Looking at left-top without eyes-focus for " + (5 - stopwatch.Elapsed.Seconds);
            }
            if (step == 6)
            {
                Window.EyesExerciseArrow.RenderTransform = new RotateTransform(225, 0, 0);
                Window.MainTitle.Text = "Looking at right-bottom without eyes-focus for " + (5 - stopwatch.Elapsed.Seconds);
            }
            if (step == 7)
            {
                Window.EyesExerciseArrow.RenderTransform = new RotateTransform(135, 0, 0);
                Window.MainTitle.Text = "Looking at right-top without eyes-focus for " + (5 - stopwatch.Elapsed.Seconds);
            }
            if (step == 8)
            {
                Window.EyesExerciseArrow.RenderTransform = new RotateTransform(315, 0, 0);
                Window.MainTitle.Text = "Looking at left-bottom without eyes-focus for " + (5 - stopwatch.Elapsed.Seconds);
            }
            } else
            {
                Window.MainTitle.Text = "Can't detect your eyes!";
                stopwatch.Stop();
            }

        }
        public void Dispose()
        {
            if (IsShowed)
                Close();

            Window?.Close();
            Window = null;
        }

    }
}
