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

namespace NeuralAction.WPF
{
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

    public class Settings
    {
        public static Settings Current { get; private set; }
        
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

        public static void Save()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));
            using (StreamWriter wr = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "Settings.xml")))
            {
                xmlSerializer.Serialize(wr, Current);
            }
        }

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

        bool headSmooth = true;
        public bool HeadSmooth
        {
            get => headSmooth;
            set { headSmooth = value; OnPropertyChanged(); }
        }

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

        EyeOpenDetectMode openMode = EyeOpenDetectMode.V1;
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

        ClickEyeTarget openEyeTarget = ClickEyeTarget.All;
        public ClickEyeTarget OpenEyeTarget
        {
            get => openEyeTarget;
            set { openEyeTarget = value; OnPropertyChanged(); }
        }

        bool cursorSmooth = true;
        public bool CursorSmooth
        {
            get => cursorSmooth;
            set { cursorSmooth = value; OnPropertyChanged(); }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
