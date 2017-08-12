using System;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NeuralAction.WPF
{
    /// <summary>
    /// CursorIcon.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CursorWindow : Window
    {
        public double Scale { get; set; } = 1;

        public CursorWindow()
        {
            InitializeComponent();
        }

        public void SetPosition(double x, double y)
        {
            Dispatcher.Invoke(() => 
            {
                Left = x - ActualWidth / 2;
                Top = y - ActualHeight / 2;
            });
        }

        public void SetAvailable(bool result)
        {
            Dispatcher.Invoke(() =>
            {
                if (result)
                {
                    Opacity = 1;
                }
                else
                {
                    Opacity = 0.33;
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Scale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
        }
    }
}
