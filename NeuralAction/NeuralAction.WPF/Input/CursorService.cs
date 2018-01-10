//written by AinL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using Vision;
using Vision.Windows;
using Vision.Detection;

namespace NeuralAction.WPF
{
    public class GazeEventArgs : EventArgs
    {
        public Point Position { get; set; }
        public ScreenProperties ScreenProperties { get; set; }
        public bool IsAvailable => IsFaceDetected && IsGazeDetected;
        public bool IsFaceDetected { get; set; }
        public bool IsGazeDetected { get; set; }

        public GazeEventArgs(Point pt, ScreenProperties screen, bool isFace, bool isGaze)
        {
            Position = pt;
            ScreenProperties = screen;

            IsFaceDetected = isFace;
            IsGazeDetected = isGaze;
        }
    }

    public class CursorTimes
    {
        public Point Position { get; set; }
        public TimeSpan Time { get; set; }

        public CursorTimes(Point pt, TimeSpan t)
        {
            Position = pt;
            Time = t;
        }
    }

    public class CursorReleasedArgs : EventArgs
    {
        public Point EndPosition { get; set; }
        public Point StartPosition { get; set; }
        public double Duration { get; set; }

        public CursorReleasedArgs(Point start, Point end, double ts)
        {
            EndPosition = end;
            StartPosition = start;
            Duration = ts;
        }
    }

    public class CursorService : IDisposable
    {
        public event EventHandler<GazeEventArgs> GazeTracked;
        public event EventHandler<System.Windows.Point> Moved;
        public event EventHandler<Point> Clicked;
        public event EventHandler<CursorReleasedArgs> Released;
        public event EventHandler Started;
        public event EventHandler Stopped;

        public MouseAction Action { get; private set; }
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
                    Started?.Invoke(this, EventArgs.Empty);
                else
                    Stopped?.Invoke(this, EventArgs.Empty);
            }
        }
        public bool IsLoaded { get; protected set; }

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

        public InputService Parent { get; set; }

        public Point Position { get; protected set; }
        public Screen TargetScreen => Parent == null ? System.Windows.Forms.Screen.PrimaryScreen : Parent.TargetScreen;

        public bool ControlAllowed { get; set; } = false;
        public bool ClickAllowed { get; set; } = false;

        public bool UseClickDelay { get; set; } = false;
        public double ClickDelay { get; set; } = 500;
        public double ClickWait { get; set; } = 500;

        public double OpenMenuWaitDuration { get; set; } = 500;

        Settings Settings => Settings.Current;
        System.Timers.Timer clickWaiter;
        Dictionary<TimeSpan, bool> clickLog = new Dictionary<TimeSpan, bool>();

        object logLocker = new object();
        object stateLocker = new object();
        bool faceDetected = false;

        public CursorService(InputService service, ScreenProperties screen)
        {
            Parent = service;
            Screen = screen;
            Init();
        }

        public CursorService(InputService service)
        {
            Parent = service;

            var dpi = Settings.DPI;
            Logger.Log(this, $"DPI: {dpi}");

            Screen s = TargetScreen;
            Screen = ScreenProperties.CreatePixelScreen(s.Bounds.Width, s.Bounds.Height, dpi);

            Init();
        }

        private void Init()
        {
            Settings.Listener.PropertyChanged += OnPropertyChanged;
            Settings.Listener.SettingChanged += (s, o) => { OnSettingChanged(o); };

            GazeService = new EyeGazeService(Screen);
            GazeService.GazeTracked += GazeService_GazeTracked;
            GazeService.FaceTracked += GazeService_FaceTracked;
            GazeService.Clicked += GazeService_Clicked;
            GazeService.Released += GazeService_Released;
            GazeService.GazeDetector.Calibrator.CalibrateBegin += GazeCalibrater_CalibrateBegin;
            GazeService.GazeDetector.Calibrator.Calibrated += GazeCalibrater_Calibrated;

            Window = new CursorWindow(this);
            Window.Moved += Window_Moved;
            Window.Show();

            Action = new MouseAction(this);
        }

        private void Window_Moved(object sender, System.Windows.Point e)
        {
            Moved?.Invoke(sender, e);
        }

        public void StartAsync(int camera)
        {
            Task.Factory.StartNew(() => { Start(camera); });
        }

        public void Start(int camera)
        {
            lock (stateLocker)
            {
                Window.SetAvailable(false);

                GazeService?.Stop();

                OnSettingChanged(Settings);
                GazeService.GazeDetector.ClipToBound = true;

                GazeService.Start(camera);

                IsRunning = true;
                Started?.Invoke(this, EventArgs.Empty);
            }
        }

        bool tempControlAllow = true;
        void GazeCalibrater_Calibrated(object sender, CalibratedArgs e)
        {
            ControlAllowed = tempControlAllow;

            if (e != null)
            {
                EyeGazeCalibrationLog logger = new EyeGazeCalibrationLog(e.Data);

                Task.Factory.StartNew(() =>
                {
                    using (OpenCvSharp.Mat frame = logger.Plot(screen, GazeService.GazeDetector.Calibrator))
                    {
                        var savepath = logger.File.AbosolutePath;
                        savepath = savepath.Replace(".clb", ".jpg");
                        Core.Cv.ImgWrite(savepath, frame);

                        Core.Cv.ImgShow("Press Any Key To Close", frame);
                        var c = Core.Cv.WaitKey(0);
                        Core.Cv.CloseAllWindows();
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }

        void GazeCalibrater_CalibrateBegin(object sender, EventArgs e)
        {
            Window.Dispatcher.Invoke(() =>
            {
                tempControlAllow = ControlAllowed;
                ControlAllowed = false;

                InputService.Current.CloseKeyboard();

                var window = new CalibrateWindow(GazeService.GazeDetector.Calibrator);
                window.Show();
            });
        }

        void GazeService_Clicked(object sender, Point e)
        {
            Profiler.ReportOn = false;

            if (e == null)
                return;

            lock (logLocker)
            {
                clickLog.Add(DateTime.Now.TimeOfDay, true);
            }

            if (UseClickDelay)
            {
                if (clickWaiter == null)
                {
                    clickWaiter = new System.Timers.Timer();
                    clickWaiter.Elapsed += delegate
                    {
                        var now = DateTime.Now.TimeOfDay.TotalMilliseconds;

                        CursorTimes[] movelist;
                        KeyValuePair<TimeSpan, bool>[] clickLog;
                        lock (logLocker)
                        {
                            lock (Window.MoveLocker)
                            {
                                movelist = Window.MoveList.ToArray();
                                clickLog = this.clickLog.ToArray();
                            }
                        }

                        var logLimit = now - ClickWait;
                        var log = (from l in clickLog
                                   where l.Key.TotalMilliseconds > logLimit
                                   select l).ToArray();

                        if (log.Length > 0)
                        {
                            double logScore = 0;
                            foreach (var l in log)
                                if (l.Value)
                                    logScore++;
                            logScore = logScore / log.Count();

                            if (logScore > 0.5)
                            {
                                var dataLimit = now - ClickDelay;
                                var data = (from pt in movelist
                                            where pt.Time.TotalMilliseconds > dataLimit
                                            orderby pt.Time.TotalMilliseconds
                                            select pt).ToArray();

                                if (data.Length > 0)
                                {
                                    var click = data[0].Position;

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
            else
            {
                InternalClicked(null);
            }
        }

        long lastClickMs = long.MaxValue;
        Point lastClickPos;

        void InternalClicked(Point click)
        {
            releaseWaiter?.Stop();

            if (click != null)
                Window.Move(click.ToPoint());
            var pt = Window.Clicked(false);

            lastClickPos = pt;
            lastClickMs = Logger.Stopwatch.ElapsedMilliseconds;

            Logger.Log("Clicked" + pt.ToString());
            Clicked?.Invoke(GazeService, new Point(pt.X, pt.Y));
        }

        System.Timers.Timer releaseWaiter;
        Point lastReleased;
        double lastReleasedDuration;
        void GazeService_Released(object sender, Point e)
        {
            if (e != null)
            {
                lastReleased = e;
                lastReleasedDuration = Logger.Stopwatch.ElapsedMilliseconds - lastClickMs;

                if (releaseWaiter == null)
                {
                    releaseWaiter = new System.Timers.Timer(100);
                    releaseWaiter.Elapsed += delegate
                    {
                        releaseWaiter.Stop();

                        lock (logLocker)
                        {
                            clickLog.Add(DateTime.Now.TimeOfDay, false);
                        }

                        Logger.Log("Clicking duration : " + lastReleasedDuration + "ms");

                        if (lastReleasedDuration > OpenMenuWaitDuration && Window.Visibility == System.Windows.Visibility.Visible)
                        {
                            var pt = Window.Clicked(false);
                            Window.Dispatcher.Invoke(() =>
                            {
                                ShortcutMenuWindow.OpenPopup(lastClickPos.ToPoint());
                            });
                            Logger.Log("Open menu at " + lastClickPos);
                        }
                        else
                        {
                            var pt = Window.Clicked();
                        }

                        Released?.Invoke(this, new CursorReleasedArgs(lastClickPos, lastReleased.Clone(), lastReleasedDuration));
                    };
                }
                releaseWaiter.Start();
            }
        }

        void GazeService_FaceTracked(object sender, FaceRect[] e)
        {
            faceDetected = e != null && e.Length > 0;
        }

        void GazeService_GazeTracked(object sender, Point e)
        {
            var arg = new GazeEventArgs(e, Screen, faceDetected, e != null);

            if (arg.IsAvailable)
            {
                Window?.SetAvailable(true);
                Window?.SetPosition(e.X, e.Y);
            }
            else
            {
                Window?.SetAvailable(false);
            }

            Position = e;
            GazeTracked?.Invoke(this, arg);
        }

        public void StopAsync()
        {
            Task.Factory.StartNew(() => { Stop(); });
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

        public void SetCamera(int index)
        {
            if (IsRunning && GazeService.CaptureIndex != index)
            {
                Stop();
                Start(index);
            }
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (GazeService == null)
                return;

            switch (e.PropertyName)
            {
                case nameof(Settings.CameraIndex):
                    SetCamera(Settings.CameraIndex);
                    break;
                case nameof(Settings.HeadSmooth):
                    GazeService.FaceDetector.UseSmooth = Settings.HeadSmooth;
                    break;
                case nameof(Settings.GazeMode):
                    GazeService.GazeDetector.DetectMode = Settings.GazeMode;
                    break;
                case nameof(Settings.GazeSmooth):
                    GazeService.GazeDetector.UseSmoothing = Settings.GazeSmooth;
                    break;
                case nameof(Settings.GazeSmoothMode):
                    GazeService.GazeDetector.Smoother.Method = Settings.GazeSmoothMode;
                    break;
                case nameof(Settings.GazeSmoothCount):
                    GazeService.GazeDetector.Smoother.QueueCount = Settings.GazeSmoothCount;
                    break;
                case nameof(Settings.OpenMode):
                    GazeService.OpenDetector.DetectMode = Settings.OpenMode;
                    break;
                case nameof(Settings.OpenSmooth):
                    GazeService.SmoothOpen = Settings.OpenSmooth;
                    break;
                case nameof(Settings.OpenEyeTarget):
                    GazeService.ClickTraget = Settings.OpenEyeTarget;
                    break;
                case nameof(Settings.GazeUseModification):
                    GazeService.GazeDetector.UseModification = Settings.GazeUseModification;
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
                case nameof(Settings.CursorSmooth):
                    Window.Smooth = Settings.CursorSmooth;
                    break;
                case nameof(Settings.CursorSpeedLimit):
                    Window.SpeedClamp = Settings.CursorSpeedLimit;
                    break;
                case nameof(Settings.CursorUseSpeedLimit):
                    Window.UseSpeedClamp = Settings.CursorUseSpeedLimit;
                    break;
                case nameof(Settings.CursorOpenMenuWaitDuration):
                    OpenMenuWaitDuration = Settings.CursorOpenMenuWaitDuration;
                    break;
                case nameof(Settings.AllowControl):
                    ControlAllowed = Settings.AllowControl;
                    break;
                case nameof(Settings.AllowClick):
                    ClickAllowed = Settings.AllowClick;
                    break;
                case nameof(Settings.GazeUseCalib):
                    GazeService.GazeDetector.UseCalibrator = Settings.GazeUseCalib;
                    break;
                case nameof(Settings.GazeCalibGridWidth):
                    GazeService.GazeDetector.Calibrator.GridWidth = Settings.GazeCalibGridWidth;
                    break;
                case nameof(Settings.GazeCalibGridHeight):
                    GazeService.GazeDetector.Calibrator.GridHeight = Settings.GazeCalibGridHeight;
                    break;
                case nameof(Settings.GazeCalibSampleCount):
                    GazeService.GazeDetector.Calibrator.SampleCount = Settings.GazeCalibSampleCount;
                    break;
                case nameof(Settings.DPI):
                    Screen = ScreenProperties.CreatePixelScreen(TargetScreen.Bounds.Width, TargetScreen.Bounds.Height, Settings.DPI);
                    break;
            }
        }

        void OnSettingChanged(Settings set)
        {
            if (GazeService == null)
                return;

            GazeService.FaceDetector.UseSmooth = set.HeadSmooth;
            GazeService.GazeDetector.UseCalibrator = set.GazeUseCalib;
            GazeService.GazeDetector.Calibrator.GridWidth = Settings.GazeCalibGridWidth;
            GazeService.GazeDetector.Calibrator.GridHeight = Settings.GazeCalibGridHeight;
            GazeService.GazeDetector.Calibrator.SampleCount = Settings.GazeCalibSampleCount;
            GazeService.GazeDetector.DetectMode = set.GazeMode;
            GazeService.GazeDetector.UseSmoothing = set.GazeSmooth;
            GazeService.GazeDetector.Smoother.Method = set.GazeSmoothMode;
            GazeService.GazeDetector.Smoother.QueueCount = set.GazeSmoothCount;
            GazeService.OpenDetector.DetectMode = Settings.OpenMode;
            GazeService.SmoothOpen = set.OpenSmooth;
            GazeService.ClickTraget = set.OpenEyeTarget;
            GazeService.GazeDetector.UseModification = Settings.GazeUseModification;
            GazeService.GazeDetector.OffsetX = set.GazeOffsetX;
            GazeService.GazeDetector.OffsetY = set.GazeOffsetY;
            GazeService.GazeDetector.SensitiveX = set.GazeSensitiveX;
            GazeService.GazeDetector.SensitiveY = set.GazeSensitiveY;
            Window.Smooth = Settings.CursorSmooth;
            Window.SpeedClamp = Settings.CursorSpeedLimit;
            Window.UseSpeedClamp = Settings.CursorUseSpeedLimit;
            OpenMenuWaitDuration = Settings.CursorOpenMenuWaitDuration;
            ControlAllowed = Settings.AllowControl;
            ClickAllowed = Settings.AllowClick;
            Screen = ScreenProperties.CreatePixelScreen(TargetScreen.Bounds.Width, TargetScreen.Bounds.Height, Settings.DPI);
            SetCamera(set.CameraIndex);
        }

        public void Dispose()
        {
            if (Window != null)
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
    }
}
