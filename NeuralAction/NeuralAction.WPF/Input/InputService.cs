using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeuralAction.WPF
{
    public class InputService : SettingListener, IDisposable
    {
        public static InputService Current { get; set; }
        public static void Init()
        {
            if (Current == null)
                Current = new InputService();
        }

        public CursorService Cursor { get; set; }
        public KeyWindow KeyWindow { get; set; }

        private Window owner;
        public Window Owner
        {
            get => owner;
            set
            {
                owner = value;
                if(KeyWindow != null)
                    KeyWindow.Owner = value;
                Cursor.Window.Owner = value;
            }
        }

        public override Settings Settings
        {
            get => base.Settings;
            set
            {
                base.Settings = value;
                if (Cursor != null)
                    Cursor.Settings = value;
            }
        }

        public int CameraIndex { get; set; } = 0;

        public bool IsKeyboardShowen => KeyWindow != null;

        public InputService()
        {
            Cursor = new CursorService();
        }

        public void Start()
        {
            Cursor.StartAsync(CameraIndex);
            //ShowKeyboard();
        }

        public void CloseKeyboard()
        {
            KeyWindow?.Close();
        }

        public void ShowKeyboard()
        {
            if(KeyWindow == null)
            {
                KeyWindow = new KeyWindow();
                KeyWindow.Owner = Owner;
                KeyWindow.Closed += delegate
                {
                    KeyWindow = null;
                };
                KeyWindow.Show();
                KeyWindow.Activate();

                Cursor.Window.Activate();
            }
            else
            {
                KeyWindow.Activate();
                Cursor.Window.Activate();
            }
        }

        public void Dispose()
        {
            if (KeyWindow != null)
            {
                KeyWindow.Close();
                KeyWindow = null;
            }

            if(Cursor != null)
            {
                Cursor.Dispose();
                Cursor = null;
            }
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        protected override void OnSettingChanged(Settings newSettings)
        {

        }
    }
}
