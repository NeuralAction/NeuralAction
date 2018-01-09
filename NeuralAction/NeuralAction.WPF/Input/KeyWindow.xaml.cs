using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Vision;

namespace NeuralAction.WPF
{
    /// <summary>
    /// KeyWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class KeyWindow : Window
    {
        MouseKeyboardLibrary.MouseHook mouseHook;
        InputService service;

        public KeyWindow(InputService service)
        {
            Send.AddWindow(this);
            this.service = service;

            InitializeComponent();

            Loaded += delegate
            {
                Keyboard.KeymapChange(Keyboard.GetKeymapArray(Keyboard.CurrentLanguage));
                UpdateScreen();
                WinApi.NotWindowsFocus(this);

                mouseHook = new MouseKeyboardLibrary.MouseHook();
                mouseHook.MouseDown += Hook_MouseDown;
                mouseHook.Start();
            };

            Keyboard.Closed += delegate
            {
                base.Close();
            };

            Closed += delegate
            {
                mouseHook.Stop();
            };
        }

        public new void Show()
        {
            base.Show();
            Keyboard.Show();
        }

        bool closed = false;
        public new void Close()
        {
            if (closed)
                return;
            closed = true;
            Keyboard.Close();
        }

        public void UpdateScreen()
        {
            switch (service.KeyboardStartupOption)
            {
                case KeyboardStartupOption.FullScreen:
                    FullScreen();
                    break;
                case KeyboardStartupOption.CenterCursor:
                    CenterCursor();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        void CenterCursor()
        {
            var pt = MouseEvent.GetCursorPosition();
            var wpfScale = InputService.Current.Cursor.Window.WpfScale;
            var scr = service.TargetScreen.WorkingArea;
            pt.X /= wpfScale;
            pt.Y /= wpfScale;
            var w = scr.Width * service.KeyboardSize / wpfScale;
            var h = scr.Height * service.KeyboardSize / wpfScale;
            Width = w;
            Height = h;
            Top = pt.Y - h / 2;
            Left = pt.X - w / 2;
            Top = Math.Max(scr.Top / wpfScale, Math.Min(Top, (scr.Top + scr.Height) / wpfScale - h));
            Left = Math.Max(scr.Left / wpfScale, Math.Min(Left, (scr.Left + scr.Width) / wpfScale - w));
        }

        void FullScreen()
        {
            var scale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            var scr = service.TargetScreen.WorkingArea;
            Top = scr.Top / scale;
            Left = scr.Left / scale;
            Width = scr.Width / scale;
            Height = scr.Height / scale;
        }

        void Hook_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var wpfScale = InputService.Current.WpfScale;
            var actualW = ActualWidth * wpfScale;
            var actualH = ActualHeight * wpfScale;
            var actualX = Left * wpfScale;
            var actualY = Top * wpfScale;

            if (e.X < actualX || e.X > actualX + actualW || e.Y < actualY || e.Y > actualY + actualH)
            {
                Close();
            }
        }

        void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
