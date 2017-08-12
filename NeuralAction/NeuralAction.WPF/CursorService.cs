//written by AinL

using System;
using System.Collections.Generic;
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

    public class CursorService
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

        public ScreenProperties Screen { get; set; }

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
            Window = new CursorWindow();

            Window.Show();
        }

        public void StartAsync(int camera)
        {
            Task t = new Task(() =>
            {
                lock (stateLocker)
                {
                    Window.SetAvailable(false);

                    if (GazeService != null)
                    {
                        GazeService.Dispose();
                        GazeService = null;
                    }

                    GazeService = new EyeGazeService(Screen);
                    GazeService.GazeDetector.ClipToBound = true;
                    GazeService.GazeTracked += GazeService_GazeTracked;
                    GazeService.FaceTracked += GazeService_FaceTracked;
                    Smooth = Smooth;

                    GazeService.Start(camera);

                    IsRunning = true;
                    Started?.Invoke(this, EventArgs.Empty);
                }
            });

            t.Start();
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
                lock (stateLocker)
                {
                    Window.SetAvailable(false);

                    if(GazeService != null)
                    {
                        GazeService.Dispose();
                        GazeService = null;

                        IsRunning = false;
                        Stopped?.Invoke(this, EventArgs.Empty);
                    }
                }
            });

            t.Start();
        }
    }
}
