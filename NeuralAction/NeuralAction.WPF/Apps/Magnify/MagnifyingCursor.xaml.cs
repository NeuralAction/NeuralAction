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

namespace NeuralAction.WPF.Magnify
{
    /// <summary>
    /// MagnifyingGlassWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MagnifyingCursor : Window
    {
        public MagnifyingGlass Model { get; set; }

        public double ActualTop
        {
            get => (Top + ActualHeight / 2) * wpfScale;
            set => Top = value / wpfScale - ActualHeight / 2;
        }

        public double ActualLeft
        {
            get => (Left + ActualWidth / 2) * wpfScale;
            set => Left = value / wpfScale - ActualWidth / 2;
        }

        bool available = false;
        public bool Available
        {
            get => available;
            set
            {
                if(available != value)
                {
                    available = value;
                    if (value)
                        CursorControl.Show();
                    else
                        CursorControl.Hide();
                }
            }
        }

        double wpfScale = InputService.Current.Cursor.Window.WpfScale;

        public MagnifyingCursor(MagnifyingGlass Model)
        {
            InitializeComponent();

            this.Model = Model;
            DataContext = Model;

            Owner = App.Current.MainWindow;

            Loaded += delegate
            {
                WinApi.SetTransClick(this);
                WinApi.NotWindowsFocus(this);
            };
        }

        public void Click()
        {
            if(Settings.Current.AllowControl && Settings.Current.AllowClick)
                MouseEvent.Click(MouseButton.Left);
            CursorControl.Click();
        }
    }
}
