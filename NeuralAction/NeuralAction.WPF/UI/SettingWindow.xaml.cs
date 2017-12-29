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
using Vision.Detection;

namespace NeuralAction.WPF
{
    /// <summary>
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
            DataContext = Settings.Current;
            Loaded += Window_Loaded;

            InitComboBox<EyeGazeDetectMode>(Cbb_GazeMode);
            InitComboBox<PointSmoother.SmoothMethod>(Cbb_GazeSmoothMode);
            InitComboBox<ClickEyeTarget>(Cbb_OpenEyeTarget);
            InitComboBox<EyeOpenDetectMode>(Cbb_OpenMode);
        }

        void InitComboBox<T>(ComboBox box)
        {
            box.ItemsSource = Enum.GetNames(typeof(T));
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Top = SystemParameters.WorkArea.Height - ActualHeight + 1;
            Left = SystemParameters.WorkArea.Width - ActualWidth;
        }

        void Bt_Apply_Click(object sender, RoutedEventArgs e)
        {
            int ind = int.MinValue;
            try
            {
                ind = Convert.ToInt32(Tb_Camera.Text);
                Settings.Current.CameraIndex = ind;
            }
            catch { }

            Settings.Save();
        }

        void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                Close();
            }
            else if(e.Key == Key.F12)
            {
                if (InputDebugWindow.Default.IsShowed)
                    InputDebugWindow.Default.Close();
                else
                    InputDebugWindow.Default.Show(InputService.Current);
            }
        }
    }
}
