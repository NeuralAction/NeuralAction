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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeuralAction.WPF
{
    public partial class MenuWindow : Window
    {
        Storyboard MenuOn;
        Storyboard MenuOff;

        public MenuWindow()
        {
            Send.AddWindow(this);

            InitializeComponent();

            Open.Content = InputService.Current.IsKeyboardShowen ? "Close" : "Open";

            Loaded += delegate
            {
                Top = SystemParameters.WorkArea.Height - ActualHeight + 1;
                Left = SystemParameters.WorkArea.Width - ActualWidth;
                MenuOn.Begin();
            };

            Deactivated += delegate
            {
                Close();
            };

            MenuOn = (Storyboard)FindResource("MenuOn");
            MenuOff = (Storyboard)FindResource("MenuOff");
            MenuOff.Completed += delegate
            {
                base.Close();
            };
        }

        bool isClosed = false;
        public new void Close()
        {
            if (isClosed)
                return;
            isClosed = true;
            MenuOff.Begin();
        }

        public new void Show()
        {
            base.Show();
            Activate();
        }

        void Open_Click(object sender, RoutedEventArgs e)
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

        void Calibration_Click(object sender, RoutedEventArgs e)
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

        void Setting_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenSetting();
        }

        void Exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Exit();
        }

        void Bt_App_Glass_Click(object sender, RoutedEventArgs e)
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
