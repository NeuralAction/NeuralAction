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
        }

        public void UpdateScreen()
        {
            var scale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            var scr = service.TargetScreen.WorkingArea;
            Top = scr.Top / scale;
            Left = scr.Left / scale;
            Width = scr.Width / scale;
            Height = scr.Height / scale;
        }

        void Keyboard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WinApi.NotWindowsFocus(this);
        }
    }
}
