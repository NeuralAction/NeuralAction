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
using System.Windows.Threading;

namespace NeuralAction.WPF
{
    /// <summary>
    /// ButtonHighlighter.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Highlighter : Window
    {
        bool on = false;
        public bool On
        {
            get => on; set
            {
                if (on != value)
                {
                    on = value;
                    if (on)
                    {
                        Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        public Highlighter()
        {
            InitializeComponent();

            Owner = App.Current.MainWindow;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += delegate
            {
                Topmost = false;
                Topmost = true;
            };
            timer.Start();
        }

        public void Highlight(Rect rect)
        {
            double wpfScale = InputService.Current.WpfScale;

            Left = rect.Left / wpfScale - 5;
            Top = rect.Top / wpfScale - 5;
            Width = rect.Width / wpfScale + 10;
            Height = rect.Height / wpfScale + 10;
        }
    }
}
