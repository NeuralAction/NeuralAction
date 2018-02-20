using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Screen = System.Windows.Forms.Screen;

namespace NeuralAction.WPF
{
    public enum KeyboardStartupOption
    {
        FullScreen,
        CenterCursor,
    }

    public class InputService : SettingListener, IDisposable
    {
        public static InputService Current { get; set; }
        public static void Init()
        {
            if (Current == null)
            {
                Current = new InputService();
                Current.Settings = Settings.Current;
            }
        }

        private Window owner;
        public Window Owner
        {
            get => owner;
            set
            {
                owner = value;
                if (KeyWindow != null)
                    KeyWindow.Owner = value;
                Cursor.Window.Owner = value;
            }
        }

        public int CameraIndex { get; set; } = 0;
        public bool IsKeyboardShowen => KeyWindow != null;

        Screen targetScreen;
        public Screen TargetScreen
        {
            get => targetScreen;
            set
            {
                targetScreen = value;
                KeyWindow?.UpdateScreen();
            }
        }

        public ApiServer Server { get; set; }

        public CursorService Cursor { get; set; }
        public double WpfScale => Cursor.Window.WpfScale;
        public KeyWindow KeyWindow { get; set; }
        public KeyboardStartupOption KeyboardStartupOption { get; set; } = KeyboardStartupOption.FullScreen;
        public double KeyboardSize { get; set; } = 0.7;

        public UIInteractionService Interaction { get; set; }

        public InputService()
        {
            TargetScreen = Screen.PrimaryScreen;
            Cursor = new CursorService(this);
            Server = new ApiServer(this);
            Interaction = new UIInteractionService();
            Server.Start();
        }

        public void Start()
        {
            Cursor.StartAsync(CameraIndex);
        }

        public void Stop()
        {
            Cursor.StopAsync();
        }

        public void CloseKeyboard()
        {
            KeyWindow?.Close();
        }

        public void ShowKeyboard()
        {
            if (KeyWindow == null)
            {
                KeyWindow = new KeyWindow(this);
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

        protected override void OnSettingChanged(Settings newSettings)
        {
            Interaction.ServiceStart = Settings.InteractUse;
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.InteractUse):
                    Interaction.ServiceStart = Settings.InteractUse;
                    break;
            }
        }

        public void Dispose()
        {
            if (KeyWindow != null)
            {
                KeyWindow.Close();
                KeyWindow = null;
            }

            if (Cursor != null)
            {
                Cursor.Dispose();
                Cursor = null;
            }
        }
    }
}
