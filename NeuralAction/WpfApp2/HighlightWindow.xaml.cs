using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace WpfApp2
{
    /// <summary>
    /// HighlightWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HighlightWindow : Window, INotifyPropertyChanged
    {
        Brush borderColor = Brushes.Red;
        public Brush BorderColor { get => borderColor; set { borderColor = value; OnPropertyChanged(); } }
        public HighlightWindow()
        {
            DataContext = this;

            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += delegate
            {
                Topmost = false;
                Topmost = true;
            };
            timer.Start();
        }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
