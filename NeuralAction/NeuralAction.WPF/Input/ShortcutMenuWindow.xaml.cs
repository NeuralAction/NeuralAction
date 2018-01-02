using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Vision.Windows;
using Logger = Vision.Logger;

namespace NeuralAction.WPF
{
    /// <summary>
    /// ShortcutMenuWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ShortcutMenuWindow : Window
    {
        public static double KeepAliveDuraton { get; set; } = 10000;

        static ShortcutMenuWindow current;

        static ShortcutMenuWindow()
        {
            Settings.Listener.PropertyChanged += Listener_PropertyChanged;
            Settings.Listener.SettingChanged += Listener_SettingChanged;
            Listener_SettingChanged(null, Settings.Current);
        }

        public static void OpenPopup(Point Position)
        {
            if (current != null)
            {
                current.Closed -= Current_Closed;
                current.Close();
            }

            current = new ShortcutMenuWindow(Position);
            current.Closed += Current_Closed;
            current.Show();
            current.Activate();
        }

        static void Listener_SettingChanged(object sender, Settings e)
        {
            KeepAliveDuraton = e.CursorMenuAliveDuration;
        }

        static void Listener_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            KeepAliveDuraton = Settings.Current.CursorMenuAliveDuration;
        }

        static void Current_Closed(object sender, EventArgs e)
        {
            current = null;
        }

        public double ActualLeft
        {
            get => (Left + ActualWidth / 2) * wpfScale;
            set => Left = value / wpfScale - ActualWidth / 2;
        }

        public double ActualTop
        {
            get => (Top + ActualHeight / 2) * wpfScale;
            set => Top = value / wpfScale - ActualHeight / 2;
        }

        Storyboard MenuOn;
        Storyboard MenuOff;
        DispatcherTimer leaveTimer;
        Point position;

        double wpfScale = InputService.Current.Cursor.Window.WpfScale;
        bool opened = true;

        ShortcutMenuWindow(Point position)
        {
            Send.AddWindow(this);

            InitializeComponent();

            this.position = position;

            leaveTimer = new DispatcherTimer();
            leaveTimer.Interval = TimeSpan.FromMilliseconds(KeepAliveDuraton);
            leaveTimer.Tick += delegate
            {
                TryClose();
                leaveTimer.Stop();
            };
            leaveTimer.Start();

            MouseEnter += delegate { leaveTimer.Stop(); };
            MouseMove += delegate { leaveTimer.Stop(); };
            MouseLeave += delegate { leaveTimer.Start(); };
            Deactivated += delegate { TryClose(); };

            MenuOn = (Storyboard)FindResource("MenuOn");
            MenuOff = (Storyboard)FindResource("MenuOff");
            MenuOff.Completed += delegate
            {
                base.Close();
            };

            SourceInitialized += delegate
            {
                WinApi.NotWindowsFocus(this);
            };

            Loaded += delegate
            {
                ActualLeft = position.X;
                ActualTop = position.Y;

                var scr = Screen.PrimaryScreen.WorkingArea;
                var wpfScale = InputService.Current.Cursor.Window.WpfScale;

                Left = Math.Max(-20, Math.Min(scr.Width / wpfScale - ActualWidth + 20, Left));
                Top = Math.Max(-20, Math.Min(scr.Height / wpfScale - ActualHeight + 20, Top));
            };
        }

        public new void Show()
        {
            MenuOn.Begin();
            base.Show();
        }

        public new void Close()
        {
            IsEnabled = false;
            MenuOff.Begin();
        }

        void TryClose()
        {
            if (opened)
            {
                try
                {
                    Close();
                }
                catch (InvalidOperationException) { }
            }
            opened = false;
        }

        void Cut_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Send.SendKey("^(x)");
            Close();
        }

        void Paste_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Send.SendKey("^(v)");
            Close();
        }

        void Copy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Send.SendKey("^(c)");
            Close();
        }

        void Drag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
            Closed += delegate
            {
                InputService.Current.Cursor.Action.DragStart(position.ToPoint());
            };
        }

        void Scroll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
            Closed += delegate
            {
                InputService.Current.Cursor.Action.ScrollStart(position.ToPoint());
            };
        }
    }
}
