//written by AinL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vision;
using Vision.Detection;

namespace NeuralAction.WPF
{
    public class GazeEventArgs : EventArgs
    {
        public Point Point { get; set; }
        public ScreenProperties ScreenProperties { get; set; }
        public bool IsFaceDetected { get; set; }
        public bool IsGazeDetected { get; set; }

        public GazeEventArgs(Point pt, ScreenProperties screen, bool isFace, bool isGaze)
        {
            Point = pt;
            ScreenProperties = screen;

            IsFaceDetected = isFace;
            IsGazeDetected = isGaze;
        }
    }

    public class CursorService : SettingListener, IDisposable
    {
        public event EventHandler<GazeEventArgs> GazeTracked;
        public event EventHandler Started;
        public event EventHandler Stopped;

        public CursorWindow Window { get; protected set; }

        public EyeGazeService GazeService { get; protected set; }

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            protected set
            {
                _isRunning = value;
                if (value)
                {
                    Started?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Stopped?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public bool IsLoaded { get; protected set; }

        private bool _smooth = true;
        public bool Smooth
        {
            get => _smooth;
            set
            {
                _smooth = value;
                if (GazeService != null)
                {
                    GazeService.GazeDetector.UseSmoothing = value;
                    GazeService.FaceDetector.SmoothLandmarks = value;
                    GazeService.FaceDetector.SmoothVectors = value;
                }
            }
        }

        private ScreenProperties screen;
        public ScreenProperties Screen
        {
            get => screen;
            set
            {
                if (GazeService != null)
                    GazeService.ScreenProperties = value;
                screen = value;
            }
        }

        object stateLocker = new object();
        bool faceDetected = false;

        public CursorService(ScreenProperties screen)
        {
            Screen = screen;

            Init();
        }

        public CursorService()
        {
            Screen s = System.Windows.Forms.Screen.PrimaryScreen;

            Screen = ScreenProperties.CreatePixelScreen(s.Bounds.Width, s.Bounds.Height, 96);

            Init();
        }

        public CursorService(System.Windows.Size screenPixelSize, double dpi) : this(screenPixelSize.Width, screenPixelSize.Height, dpi)
        {
            
        }

        public CursorService(double width, double height, double dpi)
        {
            Screen = ScreenProperties.CreatePixelScreen(width, height, dpi);

            Init();
        }

        private void Init()
        {
            GazeService = new EyeGazeService(Screen);
            GazeService.GazeTracked += GazeService_GazeTracked;
            GazeService.FaceTracked += GazeService_FaceTracked;

            Window = new CursorWindow();
            Window.Show();
        }

        public void StartAsync(int camera)
        {
            Task.Factory.StartNew(() =>
            {
                Start(camera);
            });
        }

        public void Start(int camera)
        {
            lock (stateLocker)
            {
                Window.SetAvailable(false);

                if (GazeService != null)
                {
                    GazeService.Stop();
                }

                GazeService.GazeDetector.ClipToBound = true;
                Smooth = Smooth;

                GazeService.Start(camera);

                IsRunning = true;
                Started?.Invoke(this, EventArgs.Empty);
            }
        }

        private void GazeService_FaceTracked(object sender, FaceRect[] e)
        {
            if (e != null && e.Length > 0)
            {
                faceDetected = true;
            }
            else
            {
                faceDetected = false;
            }
        }

        private void GazeService_GazeTracked(object sender, Point e)
        {
            var arg = new GazeEventArgs(e, Screen, faceDetected, e != null);

            if (arg.IsFaceDetected && arg.IsGazeDetected)
            {
                Window.SetAvailable(true);
                Window.SetPosition(e.X, e.Y);
            }
            else
            {
                Window.SetAvailable(false);
            }

            GazeTracked?.Invoke(this, arg);
        }

        public void StopAsync()
        {
            Task t = new Task(() =>
            {
                Stop();
            });

            t.Start();
        }

        public void Stop()
        {
            lock (stateLocker)
            {
                Window.SetAvailable(false);

                if (GazeService != null)
                {
                    GazeService.Stop();
                }

                IsRunning = false;
                Stopped?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            if(Window != null)
            {
                Window.Close();
                Window = null;
            }

            if (GazeService != null)
            {
                lock (stateLocker)
                {
                    GazeService.Dispose();
                    GazeService = null;

                    IsRunning = false;
                }
            }
        }

        public void SetCamera(int index)
        {
            if (IsRunning)
            {
                Stop();
            }
            Start(index);
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.CameraIndex):
                    SetCamera(Settings.CameraIndex);
                    break;
                case nameof(Settings.GazeExtendModel):
                    GazeService.GazeDetector.UseBothEyes = Settings.GazeExtendModel;
                    break;
                case nameof(Settings.GazeSmooth):
                    GazeService.GazeDetector.UseSmoothing = Settings.GazeSmooth;
                    break;
                case nameof(Settings.HeadSmooth):
                    GazeService.FaceDetector.SmoothLandmarks = Settings.HeadSmooth;
                    break;
                case nameof(Settings.GazeOffsetX):
                    GazeService.GazeDetector.OffsetX = Settings.GazeOffsetX;
                    break;
                case nameof(Settings.GazeOffsetY):
                    GazeService.GazeDetector.OffsetY = Settings.GazeOffsetY;
                    break;
                case nameof(Settings.GazeSensitiveX):
                    GazeService.GazeDetector.SensitiveX = Settings.GazeSensitiveX;
                    break;
                case nameof(Settings.GazeSensitiveY):
                    GazeService.GazeDetector.SensitiveY = Settings.GazeSensitiveY;
                    break;
            }
        }

        protected override void OnSettingChanged(Settings set)
        {
            GazeService.FaceDetector.SmoothLandmarks = set.HeadSmooth;
            GazeService.GazeDetector.UseSmoothing = set.GazeSmooth;
            GazeService.GazeDetector.UseBothEyes = set.GazeExtendModel;
            GazeService.GazeDetector.OffsetX = set.GazeOffsetX;
            GazeService.GazeDetector.OffsetY = set.GazeOffsetY;
            GazeService.GazeDetector.SensitiveX = set.GazeSensitiveX;
            GazeService.GazeDetector.SensitiveY = set.GazeSensitiveY;
            SetCamera(set.CameraIndex);
        }
    }
}
