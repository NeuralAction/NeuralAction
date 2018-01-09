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
            };

            Keyboard.Closed += delegate
            {
                base.Close();
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
            var scr = InputService.Current.TargetScreen.Bounds;
            var wpfScale = InputService.Current.WpfScale;
            Left = scr.Left / wpfScale;
            Top = scr.Top / wpfScale;
            Width = scr.Width / wpfScale;
            Height = scr.Height / wpfScale;

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
            Keyboard.Width = w;
            Keyboard.Height = h;
            var top = Math.Max(scr.Top / wpfScale, Math.Min(pt.Y - h / 2, (scr.Top + scr.Height) / wpfScale - h));
            var left = Math.Max(scr.Left / wpfScale, Math.Min(pt.X - w / 2, (scr.Left + scr.Width) / wpfScale - w));
            Keyboard.Margin = new Thickness(left, top, 0, 0);
        }

        void FullScreen()
        {
            var scale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            var scr = service.TargetScreen.WorkingArea;
            var top = scr.Top / scale;
            var left = scr.Left / scale;
            Keyboard.Width = scr.Width / scale;
            Keyboard.Height = scr.Height / scale;
            Keyboard.Margin = new Thickness(left, top, 0, 0);
        }

        void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
