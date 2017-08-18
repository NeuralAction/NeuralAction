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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeuralAction.WPF
{
    /// <summary>
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window
    {
        int scale;
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            scale = (int)PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;

            this.Height = SystemParameters.WorkArea.Height;
            this.Top = 0;
            this.Left = (SystemParameters.WorkArea.Width) - this.Width;
           
        }
    }
}
