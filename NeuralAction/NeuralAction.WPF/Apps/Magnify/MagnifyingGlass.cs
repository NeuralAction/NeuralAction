using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NeuralAction.WPF.Magnify;
using Vision;

namespace NeuralAction.WPF
{
    public class MagnifyingGlass : NotifyPropertyChnagedBase, IDisposable
    {
        public static MagnifyingGlass Current = new MagnifyingGlass();

        public double ZoomSmooth { get; set; } = 1.25;
        public double MoveMinimum { get; set; } = 3;
        public double MoveMaximum { get; set; } = 80;
        public double MoveSmooth { get; set; } = 5;
        public bool UseDynamicZoom { get; set; } = true;

        public bool IsShowed { get; set; } = false;

        double wpfScale = InputService.Current.Cursor.Window.WpfScale;
        MagnifierForm Mag;
        MagnifyingCursor Window;
        DispatcherTimer moveUpdater;

        public MagnifyingGlass()
        {
            Mag = new MagnifierForm();
            Window = new MagnifyingCursor(this);

            Settings.Listener.PropertyChanged += Listener_PropertyChanged;
            Settings.Listener.SettingChanged += Listener_SettingChanged;
            Listener_SettingChanged(null, Settings.Current);
        }

        public void Show()
        {
            var scr = InputService.Current.TargetScreen.Bounds;
            Mag.Form.Width = scr.Width;
            Mag.Form.Height = scr.Height;
            Mag.Form.Top = scr.Top;
            Mag.Form.Left = scr.Left;
            Mag.Show();

            Window.Left = InputService.Current.Cursor.Window.Left;
            Window.Top = InputService.Current.Cursor.Window.Top;
            Window.Show();

            if (moveUpdater == null)
            {
                moveUpdater = new DispatcherTimer();
                moveUpdater.Interval = TimeSpan.FromMilliseconds(5);
                moveUpdater.Tick += MoveUpdater_Tick;
            }
            moveUpdater.Start();

            InputService.Current.Cursor.Window.Visibility = Visibility.Collapsed;
            InputService.Current.Cursor.GazeTracked += Cursor_GazeTracked;
            InputService.Current.Cursor.Clicked += Cursor_Clicked;

            IsShowed = true;
        }

        public void Close()
        {
            Mag.Hide();
            Window.Hide();

            moveUpdater.Stop();

            InputService.Current.Cursor.Window.Visibility = Visibility.Visible;
            InputService.Current.Cursor.GazeTracked -= Cursor_GazeTracked;
            InputService.Current.Cursor.Clicked -= Cursor_Clicked;

            IsShowed = false;
        }

        void Cursor_Clicked(object sender, Vision.Point e)
        {
            Window.Dispatcher.Invoke(() =>
            {
                Window.Click();
            });
        }

        void Cursor_GazeTracked(object sender, GazeEventArgs e)
        {
            Window.Dispatcher.Invoke(() =>
            {
                gazeArg = e;
                Window.Available = e.IsAvailable;
            });
        }

        PointSmoother pointSmoother = new PointSmoother()
        {
            Method = PointSmoother.SmoothMethod.MeanKalman,
            QueueCount = 10,
        };

        KalmanFilter kalman = new KalmanFilter();
        MeanSmoother mean = new MeanSmoother()
        {
            QueueCount = 10,
        };
        GazeEventArgs gazeArg;
        void MoveUpdater_Tick(object sender, EventArgs e)
        {
            if (gazeArg != null && gazeArg.IsAvailable)
            {
                Mag.Magnifier.UpdateMaginifier();

                var scr = gazeArg.ScreenProperties.PixelSize;
                var x = Window.ActualLeft;
                var y = Window.ActualTop;
                x = (x - Mag.Magnifier.Left) / Mag.Magnifier.Width;
                y = (y - Mag.Magnifier.Top) / Mag.Magnifier.Height;
                var trackedX = gazeArg.Position.X / scr.Width;
                var trackedY = gazeArg.Position.Y / scr.Height;

                var xPow = trackedX - x;
                var yPow = trackedY - y;
                var power = 1 - Drop(Math.Pow(Math.Sqrt(xPow * xPow + yPow * yPow), 2) / ZoomSmooth, 0.1);
                if (!UseDynamicZoom)
                    power = 1;
                power = mean.Smooth(power);
                power = kalman.Calculate(power);
                xPow = Clamp(Drop(xPow * Mag.Magnifier.Width / MoveSmooth, MoveMinimum), MoveMaximum);
                yPow = Clamp(Drop(yPow * Mag.Magnifier.Height / MoveSmooth, MoveMinimum), MoveMaximum);

                var smt = pointSmoother.Smooth(new Vision.Point(Window.ActualLeft + xPow, Window.ActualTop + yPow));

                if (Settings.Current.AllowControl)
                    MouseEvent.MoveAt(new System.Windows.Point(smt.X, smt.Y));

                Window.ActualLeft = smt.X;
                Window.ActualTop = smt.Y;
                Mag.Magnifier.CenterX = (int)(smt.X);
                Mag.Magnifier.CenterY = (int)(smt.Y);
                Mag.Magnifier.Magnification = Math.Max(1, Mag.Magnification * power);
            }
            Mag.Magnifier.UpdateMaginifier();
        }

        double Drop(double value, double abs)
        {
            if (value > 0)
                value = Math.Max(0, value - abs);
            else if (value < 0)
                value = Math.Min(0, value + abs);
            return value;
        }

        double Clamp(double value, double abs)
        {
            return Math.Max(-abs, Math.Min(abs, value));
        }

        void Listener_SettingChanged(object sender, Settings set)
        {
            Mag.Magnification = set.MagnifyFactor;
            MoveSmooth = set.MagnifyMoveSmooth;
            MoveMaximum = set.MagnifySpeedMax;
            MoveMinimum = set.MagnifySpeedMin;
            ZoomSmooth = set.MagnifyZoomSmooth;
            UseDynamicZoom = set.MagnifyUseDynZoom;
        }

        void Listener_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var set = Settings.Current;

            switch (e.PropertyName)
            {
                case nameof(Settings.MagnifyFactor):
                    Mag.Magnification = set.MagnifyFactor;
                    break;
                case nameof(Settings.MagnifyMoveSmooth):
                    MoveSmooth = set.MagnifyMoveSmooth;
                    break;
                case nameof(Settings.MagnifySpeedMax):
                    MoveMaximum = set.MagnifySpeedMax;
                    break;
                case nameof(Settings.MagnifySpeedMin):
                    MoveMinimum = set.MagnifySpeedMin;
                    break;
                case nameof(Settings.MagnifyZoomSmooth):
                    ZoomSmooth = set.MagnifyZoomSmooth;
                    break;
                case nameof(Settings.MagnifyUseDynZoom):
                    UseDynamicZoom = set.MagnifyUseDynZoom;
                    break;
            }
        }

        public void Dispose()
        {
            if (IsShowed)
                Close();

            Window?.Close();
            Window = null;

            Settings.Listener.PropertyChanged += Listener_PropertyChanged;
            Settings.Listener.SettingChanged += Listener_SettingChanged;
        }
    }
}
