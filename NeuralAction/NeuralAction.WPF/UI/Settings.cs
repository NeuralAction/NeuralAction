using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using Vision.Detection;
using Vision;
using System.Windows.Input;

namespace NeuralAction.WPF
{
    public class CommandHandler : ICommand
    {
        Action action;
        bool canExecute;

        public CommandHandler(Action action, bool canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            action();
        }
    }

    public class BasicSettingListener : SettingListener
    {
        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;
        public event EventHandler<Settings> SettingChanged;

        public BasicSettingListener()
        {

        }

        public BasicSettingListener(Settings settings)
        {
            Settings = settings;
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        protected override void OnSettingChanged(Settings newSettings)
        {
            SettingChanged?.Invoke(this, newSettings);
        }
    }

    public abstract class SettingListener
    {
        private Settings settings;
        public virtual Settings Settings
        {
            get => settings;
            set
            {
                if (settings != null)
                {
                    settings.PropertyChanged -= OnPropertyChanged;
                }
                settings = value;
                settings.PropertyChanged += OnPropertyChanged;
                OnSettingChanged(settings);
            }
        }

        protected abstract void OnSettingChanged(Settings newSettings);

        protected abstract void OnPropertyChanged(object sender, PropertyChangedEventArgs e);
    }

    public class NotifyPropertyChnagedBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Settings : NotifyPropertyChnagedBase
    {
        #region Static

        public static Settings Current
        {
            get => Listener.Settings;
            set => Listener.Settings = value;
        }
        public static BasicSettingListener Listener { get; private set; } = new BasicSettingListener();
        
        public static void Load()
        {
            Current = new Settings();

            string path = Path.Combine(Environment.CurrentDirectory, "Settings.xml");
            if (File.Exists(path))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));
                using (StreamReader rdr = new StreamReader(path))
                {
                    var decoded = (Settings)xmlSerializer.Deserialize(rdr);
                    Current = decoded;
                }
            }
            else
            {
                Current = new Settings();
            }
        }

        public static void Reset()
        {
            Current = new Settings();
        }

        public static void Save()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));
            using (StreamWriter wr = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "Settings.xml")))
            {
                xmlSerializer.Serialize(wr, Current);
            }
        }

        #endregion Static

        #region Command

        ICommand commandReset;
        public ICommand CommandReset
        {
            get
            {
                return commandReset ?? (commandReset = new CommandHandler(() => { Reset(); }, true));
            }
        }

        ICommand dpiReset;
        public ICommand CommandDPIReset
        {
            get
            {
                return dpiReset ?? (dpiReset = new CommandHandler(() => { DPI = WinApi.GetDpi(); }, true));
            }
        }

        #endregion Command

        #region Common 

        int cameraIndex = 0;
        public int CameraIndex
        {
            get => cameraIndex;
            set { cameraIndex = value; OnPropertyChanged(); }
        }

        bool allowControl = false;
        public bool AllowControl
        {
            get => allowControl;
            set { allowControl = value; OnPropertyChanged(); }
        }

        bool allowClick = false;
        public bool AllowClick
        {
            get => allowClick;
            set { allowClick = value; OnPropertyChanged(); }
        }

        double dpi = 96;
        public double DPI
        {
            get => dpi;
            set { dpi = value; OnPropertyChanged(); }
        }

        bool headSmooth = true;
        public bool HeadSmooth
        {
            get => headSmooth;
            set { headSmooth = value; OnPropertyChanged(); }
        }

        #endregion Common

        #region Gaze

        EyeGazeDetectMode gazeMode = EyeGazeDetectMode.FaceMobile;
        public EyeGazeDetectMode GazeMode
        {
            get => gazeMode;
            set { gazeMode = value; OnPropertyChanged(); }
        }

        bool gazeUseCalib = true;
        public bool GazeUseCalib
        {
            get => gazeUseCalib;
            set { gazeUseCalib = value; OnPropertyChanged(); }
        }

        int gazeCalibGridWidth = 4;
        public int GazeCalibGridWidth
        {
            get => gazeCalibGridWidth;
            set { gazeCalibGridWidth = value; OnPropertyChanged(); }
        }

        int gazeCalibGridHeight = 3;
        public int GazeCalibGridHeight
        {
            get => gazeCalibGridHeight;
            set { gazeCalibGridHeight = value; OnPropertyChanged(); }
        }

        int gazeCalibSampleCount = 5;
        public int GazeCalibSampleCount
        {
            get => gazeCalibSampleCount;
            set { gazeCalibSampleCount = value; OnPropertyChanged(); }
        }

        bool gazeSmooth = true;
        public bool GazeSmooth
        {
            get => gazeSmooth;
            set { gazeSmooth = value; OnPropertyChanged(); }
        }

        PointSmoother.SmoothMethod gazeSmoothMode = PointSmoother.SmoothMethod.MeanKalman;
        public PointSmoother.SmoothMethod GazeSmoothMode
        {
            get => gazeSmoothMode;
            set { gazeSmoothMode = value; OnPropertyChanged(); }
        }

        int gazeSmoothCount = 6;
        public int GazeSmoothCount
        {
            get => gazeSmoothCount;
            set { gazeSmoothCount = value; OnPropertyChanged(); }
        }

        double gazeOffsetX = EyeGazeDetector.DefaultOffsetX;
        public double GazeOffsetX
        {
            get => gazeOffsetX;
            set { gazeOffsetX = value; OnPropertyChanged(); }
        }

        double gazeOffsetY = EyeGazeDetector.DefaultOffsetY;
        public double GazeOffsetY
        {
            get => gazeOffsetY;
            set { gazeOffsetY = value; OnPropertyChanged(); }
        }

        double gazeSensitiveX = EyeGazeDetector.DefaultSensitiveX;
        public double GazeSensitiveX
        {
            get => gazeSensitiveX;
            set { gazeSensitiveX = value; OnPropertyChanged(); }
        }

        double gazeSensitiveY = EyeGazeDetector.DefaultSensitiveY;
        public double GazeSensitiveY
        {
            get => gazeSensitiveY;
            set { gazeSensitiveY = value; OnPropertyChanged(); }
        }

        #endregion Gaze

        #region Open

        EyeOpenDetectMode openMode = EyeOpenDetectMode.V2;
        public EyeOpenDetectMode OpenMode
        {
            get => openMode;
            set { openMode = value; OnPropertyChanged(); }
        }

        bool openSmooth = true;
        public bool OpenSmooth
        {
            get => openSmooth;
            set { openSmooth = value; OnPropertyChanged(); }
        }

        ClickEyeTarget openEyeTarget = ClickEyeTarget.Both;
        public ClickEyeTarget OpenEyeTarget
        {
            get => openEyeTarget;
            set { openEyeTarget = value; OnPropertyChanged(); }
        }

        #endregion Open

        #region Cursor

        bool cursorSmooth = true;
        public bool CursorSmooth
        {
            get => cursorSmooth;
            set { cursorSmooth = value; OnPropertyChanged(); }
        }

        bool cursorUseSpeedLimit = true;
        public bool CursorUseSpeedLimit
        {
            get => cursorUseSpeedLimit;
            set { cursorUseSpeedLimit = value; OnPropertyChanged(); }
        }

        double cursorSpeedLimit = 15;
        public double CursorSpeedLimit
        {
            get => cursorSpeedLimit;
            set { cursorSpeedLimit = value; OnPropertyChanged(); }
        }

        #endregion Cursor

        #region Magnify

        double magnifyFactor = 1.5;
        public double MagnifyFactor
        {
            get => magnifyFactor;
            set { magnifyFactor = value; OnPropertyChanged(); }
        }

        double magnifySpeedMin = 5;
        public double MagnifySpeedMin
        {
            get => magnifySpeedMin;
            set { magnifySpeedMin = value; OnPropertyChanged(); }
        }

        double magnifySpeedMax = 150;
        public double MagnifySpeedMax
        {
            get => magnifySpeedMax;
            set { magnifySpeedMax = value; OnPropertyChanged(); }
        }

        double magnifyMoveSmooth = 3;
        public double MagnifyMoveSmooth
        {
            get => magnifyMoveSmooth;
            set { magnifyMoveSmooth = value; OnPropertyChanged(); }
        }

        double magnifyZoomSmooth = 1.25;
        public double MagnifyZoomSmooth
        {
            get => magnifyZoomSmooth;
            set { magnifyZoomSmooth = value; OnPropertyChanged(); }
        }

        bool magnifyUseDynZoom = true;
        public bool MagnifyUseDynZoom
        {
            get => magnifyUseDynZoom;
            set { magnifyUseDynZoom = value; OnPropertyChanged(); }
        }

        #endregion Magnify
    }
}
