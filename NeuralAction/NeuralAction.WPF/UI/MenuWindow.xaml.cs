using NeuralAction.WPF.Apps.EyesExercise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeuralAction.WPF
{
    public partial class MenuWindow : Window
    {
        public MenuWindow()
        {
            InitializeComponent();

            Open.Content = InputService.Current.IsKeyboardShowen ? "Close" : "Open";

            Loaded += delegate
            {
                Top = SystemParameters.WorkArea.Height - ActualHeight + 1;
                Left = SystemParameters.WorkArea.Width - ActualWidth;
            };
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (InputService.Current.IsKeyboardShowen)
            {
                InputService.Current.CloseKeyboard();
            }
            else
            {
                InputService.Current.ShowKeyboard();
            }
            Close();
        }

        private void Calibration_Click(object sender, RoutedEventArgs e)
        {
            var calib = InputService.Current.Cursor.GazeService.GazeDetector.Calibrator;
            if (!calib.IsStarted)
            {
                calib.Start(InputService.Current.Cursor.GazeService.ScreenProperties);
                Close();
            }
            else
            {
                calib.Stop();
            }
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenSetting();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            MainWindow.Exit();
        }

        private void Bt_App_Glass_Click(object sender, RoutedEventArgs e)
        {
            if (!MagnifyingGlass.Current.IsShowed)
            {
                MagnifyingGlass.Current.Show();
                Close();
            }
            else
            {
                MagnifyingGlass.Current.Close();
            }
        }

        private void Bt_App_Exercise_Click(object sender, RoutedEventArgs e)
        {
            if (!EyesExercise.Current.IsShowed)
            {
                EyesExercise.Current.Show();
                Close();
            }
            else
            {
                EyesExercise.Current.Close();
            }
        }
    }
}
