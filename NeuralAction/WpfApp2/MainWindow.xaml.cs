using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UIAccessibility;

namespace WpfApp2
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string targetHitInfo;
        public string TargetHitInfo
        {
            get => targetHitInfo;
            set { targetHitInfo = value; OnPropertyChanged(); }
        }

        string targetInfo;
        public string TargetInfo
        {
            get => targetInfo;
            set { targetInfo = value; OnPropertyChanged(); }
        }

        string targetElement;
        public string TargetElement
        {
            get => targetElement;
            set { targetElement = value; OnPropertyChanged(); }
        }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        HighlightWindow highlight;
        HighlightWindow highlightYellow;

        public MainWindow()
        {
            DataContext = this;

            InitializeComponent();

            highlight = new HighlightWindow();
            highlight.Show();

            highlightYellow = new HighlightWindow();
            highlightYellow.BorderColor = Brushes.Yellow;
            highlightYellow.Show();

            highlight.Owner = highlightYellow;

            var service = new AccessibleNotifyService();
            service.Tracked += (s, a) =>
            {
                var hit = a.RawHit;
                var element = a.Element;

                Dispatcher.Invoke(() =>
                {
                    TargetElement = element == null ? "null" : element.Type.ToString();
                    TargetInfo = element == null ? "null" : element.ToString();
                    TargetHitInfo = hit == null ? "null" : hit.GetParentTree();

                    if (element != null)
                    {
                        highlight.Visibility = Visibility.Visible;

                        var loc =  element.Location;

                        highlight.Top = loc.Top;
                        highlight.Left = loc.Left;
                        highlight.Width = loc.Width;
                        highlight.Height = loc.Height;
                    }
                    else
                    {
                        highlight.Visibility = Visibility.Hidden;
                    }

                    if (hit != null)
                    {
                        highlightYellow.Visibility = Visibility.Visible;

                        var loc = hit.Location;

                        highlightYellow.Top = loc.Top;
                        highlightYellow.Left = loc.Left;
                        highlightYellow.Width = loc.Width;
                        highlightYellow.Height = loc.Height;
                    }
                    else
                    {
                        highlightYellow.Visibility = Visibility.Hidden;
                    }
                });
            };
            service.Start();

            Closed += delegate
            {
                service.Dispose();
                Environment.Exit(0);
            };
        }

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
