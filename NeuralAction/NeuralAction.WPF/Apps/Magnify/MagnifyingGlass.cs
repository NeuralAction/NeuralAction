using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using NeuralAction.WPF.Magnify;

namespace NeuralAction.WPF
{
    public class MagnifyingGlass : NotifyPropertyChnagedBase, IDisposable
    {
        public static MagnifyingGlass Current = new MagnifyingGlass();

        BitmapSource image;
        public BitmapSource Image
        {
            get => image;
            set { image = value; OnPropertyChanged(); }
        }

        public bool IsShowed { get; set; } = false;

        MagnifyingCursor Window;

        public MagnifyingGlass()
        {
            Window = new MagnifyingCursor(this);
            Window.Loaded += delegate
            {
                WinApi.NotWindowsFocus(Window);
            };
        }

        public void Show()
        {
            Window.Show();

            InputService.Current.Cursor.Window.Visibility = Visibility.Collapsed;
            InputService.Current.Cursor.GazeTracked += Cursor_GazeTracked;
            InputService.Current.Cursor.Clicked += Cursor_Clicked;

            IsShowed = true;
        }

        public void Close()
        {
            Window.Hide();

            InputService.Current.Cursor.Window.Visibility = Visibility.Visible;
            InputService.Current.Cursor.GazeTracked -= Cursor_GazeTracked;
            InputService.Current.Cursor.Clicked -= Cursor_Clicked;

            IsShowed = false;
        }

        void Cursor_Clicked(object sender, Vision.Point e)
        {
            Window.Click();
        }

        void Cursor_GazeTracked(object sender, GazeEventArgs e)
        {

        }

        public void Dispose()
        {
            if(IsShowed)
                Close();

            Window?.Close();
            Window = null;
        }
    }
}
