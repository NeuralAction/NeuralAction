using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public static void Save()
        {

        }

        private int cameraIndex = 0;
        public int CameraIndex
        {
            get => cameraIndex;
            set { cameraIndex = value; OnPropertyChanged(); }
        }

        private bool headSmooth = true;
        public bool HeadSmooth
        {
            get => headSmooth;
            set { headSmooth = value; OnPropertyChanged(); }
        }

        private bool gazeSmooth = true;
        public bool GazeSmooth
        {
            get => gazeSmooth;
            set { gazeSmooth = value; OnPropertyChanged(); }
        }
        
        private bool gazeExtendModel = true;
        public bool GazeExtendModel
        {
            get => gazeExtendModel;
            set { gazeExtendModel = value; OnPropertyChanged(); }
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
