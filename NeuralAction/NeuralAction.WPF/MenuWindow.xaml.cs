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
    /// <summary>
    /// MenuWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MenuWindow : Window
    {

        private SettingWindow settingWindow;

        public MenuWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (settingWindow != null)
            {
                settingWindow.Close();
            }
            Top = SystemParameters.WorkArea.Height - ActualHeight + 1;
            Left = SystemParameters.WorkArea.Width - ActualWidth;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            InputService.Current.ShowKeyboard();
            this.Hide();

        }

        private void Calibration_Click(object sender, RoutedEventArgs e)
        {
            if (!InputService.Current.Cursor.GazeService.GazeDetector.Calibrator.IsStarted)
                InputService.Current.Cursor.GazeService.GazeDetector.Calibrator.Start(InputService.Current.Cursor.GazeService.ScreenProperties);

        }


        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            if (settingWindow == null)
            {
                SettingWindow settingWindow = new SettingWindow();
                settingWindow.Owner = this;
                settingWindow.Closed += delegate
                {
                    settingWindow = null;
                };
                settingWindow.Show();
            }
            else
            {
                settingWindow.Activate();
            }
            this.Hide();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }


    }
}
