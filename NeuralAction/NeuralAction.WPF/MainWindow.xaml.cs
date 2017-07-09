using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NeuralAction.WPF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void onepie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("onepie");
        }
        private void twopie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("twopie");
        }

        private void threepie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("threepie");
        }

        private void fourpie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("fourpie");
        }

        private void fivepie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("fivepie");
        }

        private void sixpie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("sixpie");
        }

        private void sevenpie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("sevenpie");
        }

        private void eightpie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("eightpie");
        }

        private void blankpie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("blankpie");
        }

        private void markpie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("markpie");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CursorIcon CursorIcon = new CursorIcon();
            CursorIcon.Show();
        }
    }
}
