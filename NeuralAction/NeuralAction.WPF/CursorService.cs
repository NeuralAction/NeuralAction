//written by AinL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
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

    public class CursorTimes
    {
        public Point Point { get; set; }
        public TimeSpan Time { get; set; }

        public CursorTimes(Point pt, TimeSpan t)
        {
            Point = pt;
            Time = t;
        }
    }

    public class CursorService : SettingListener, IDisposable
    {
        public event EventHandler<GazeEventArgs> GazeTracked;
        public event EventHandler<System.Windows.Point> Moved;
        public event EventHandler<Point> Clicked;
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
                    GazeService.FaceDetector.UseSmooth = value;
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

        public bool UseClickDelay { get; set; } = false;
        public double ClickDelay { get; set; } = 300;
        public double ClickWait { get; set; } = 150;

        public Point Point { get; protected set; }

        System.Timers.Timer clickWaiter;
        object logLocker = new object();
        Dictionary<TimeSpan, bool> clickLog = new Dictionary<TimeSpan, bool>();
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
            GazeService.Clicked += GazeService_Clicked;

            Window = new CursorWindow();
            Window.Moved += Window_Moved;
            Window.Show();
        }

        private void Window_Moved(object sender, System.Windows.Point e)
        {
            Moved?.Invoke(sender, e);
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

        private void GazeService_Clicked(object sender, Point e)
        {
            Profiler.ReportOn = false;

            if (UseClickDelay)
            {
                lock (logLocker)
                lock (Window.MoveLocker)
                {
                    var movelist = Window.MoveList;
                    if (e != null && movelist != null)
                    {
                        if (clickWaiter == null)
                        {
                            clickWaiter = new System.Timers.Timer();
                            clickWaiter.Elapsed += delegate
                            {
                                var now = DateTime.Now.TimeOfDay;

                                var logLimit = now.TotalMilliseconds - ClickWait;
                                var log = from l in clickLog
                                            where l.Key.TotalMilliseconds > logLimit
                                            select l;

                                if (log.Count() > 0)
                                {
                                    double logScore = 0;
                                    foreach (var l in log)
                                        if (l.Value)
                                            logScore++;
                                    logScore = logScore / log.Count();

                                    if (logScore > 0.6)
                                    {
                                        var dataLimit = now.TotalMilliseconds - ClickDelay;
                                        var data = from pt in movelist
                                                    where pt.Time.TotalMilliseconds > dataLimit
                                                    orderby pt.Time.TotalMilliseconds
                                                    select pt;

                                        if (data.Count() > 0)
                                        {
                                            var click = data.First().Point;

                                            InternalClicked(click);
                                        }
                                    }
                                }

                                Logger.Log("timer stop");
                                clickWaiter.Stop();
                            };

                            clickWaiter.Interval = ClickWait;
                        }

                        if (!clickWaiter.Enabled)
                        {
                            Logger.Log("timer start" + e.ToString());
                            clickWaiter.Start();
                        }
                    }
                }
            }
            else
            {
                if(e!=null)
                    InternalClicked(null);
            }
        }

        private void InternalClicked(Point click)
        {
            if (click != null)
            {
                var winClick = new System.Windows.Point(click.X, click.Y);
                Window.Goto(winClick);
                Logger.Log("Clicked" + click.ToString());
            }
            MouseEvent.Click(MouseButton.Left);
            Window.Clicked();
            Clicked?.Invoke(GazeService, Window.Point);
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

                lock (logLocker)
                {
                    clickLog.Add(DateTime.Now.TimeOfDay, GazeService.IsLeftClicking || GazeService.IsRightClicking);
                }
            }
            else
            {
                Window.SetAvailable(false);
            }

            Point = e; 
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
                    GazeService.GazeDetector.DetectMode = EyeGazeDetectMode.Face;
                    break;
                case nameof(Settings.GazeSmooth):
                    GazeService.GazeDetector.UseSmoothing = Settings.GazeSmooth;
                    break;
                case nameof(Settings.HeadSmooth):
                    GazeService.FaceDetector.UseSmooth = Settings.HeadSmooth;
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
                case nameof(Settings.GazeSpeedLimit):
                    Window.SpeedLimit = Settings.GazeSpeedLimit;
                    break;
            }
        }

        protected override void OnSettingChanged(Settings set)
        {
            GazeService.FaceDetector.UseSmooth = set.HeadSmooth;
            GazeService.GazeDetector.UseSmoothing = set.GazeSmooth;
            GazeService.GazeDetector.DetectMode = set.GazeExtendModel ? EyeGazeDetectMode.Face : EyeGazeDetectMode.Both;
            GazeService.GazeDetector.OffsetX = set.GazeOffsetX;
            GazeService.GazeDetector.OffsetY = set.GazeOffsetY;
            GazeService.GazeDetector.SensitiveX = set.GazeSensitiveX;
            GazeService.GazeDetector.SensitiveY = set.GazeSensitiveY;
            Window.SpeedLimit = Settings.GazeSpeedLimit;
            SetCamera(set.CameraIndex);
        }
    }
}
