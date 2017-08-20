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
    /// Menu.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Menu : Window
    {

        private SettingWindow settingWindow;

        public Menu()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Top = SystemParameters.WorkArea.Height - ActualHeight + 1;
            Left = SystemParameters.WorkArea.Width - ActualWidth;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            InputService.Current.ShowKeyboard();
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
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
