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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Vision;
using Vision.Windows;

namespace EyeGazeGen
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        EyeGazeModelRecorder recorder;
        EyesDetector detector;
        VMat frame;

        public MainWindow()
        {
            InitializeComponent();

            Vision.Core.Init(new Vision.Windows.WindowsCore());

            detector = new EyesDetector(new EyesDetectorXmlLoader());
        }

        private void Bt_Start_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void Start()
        {
            Stop();

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Panel_StartMenu.Visibility = Visibility.Hidden;
            Panel_StartMenu.IsEnabled = false;
            Bt_Stop.Visibility = Visibility.Visible;
            Bt_Stop.IsEnabled = true;

            UpdateLayout();

            recorder = new EyeGazeModelRecorder(Tb_Session.Text, new Vision.Size(canvas.ActualWidth, canvas.ActualHeight));
            recorder.SetPoint += Recorder_SetPoint;
            recorder.FrameReady += Recorder_FrameReady;
            recorder.Start();
        }

        private void Recorder_FrameReady(object sender, VMat e)
        {
            if (frame != null)
                frame.Dispose();

            Profiler.Count("fps");

            frame = e.Clone();
            FaceRect[] rect = detector.Detect(frame);
            foreach (FaceRect r in rect)
                r.Draw(frame, 1, true);
            frame.DrawText(0, 50, $"fps:{Profiler.Get("fps")}");

            Dispatcher.Invoke(() =>
            {
                Background = new ImageBrush(frame.ToBitmapSource())
                {
                    Stretch = Stretch.Uniform
                };
                Background.Freeze();
            });
        }

        private Vision.Point lastpoint = new Vision.Point(0,0);
        private Storyboard storyboard;
        private void Recorder_SetPoint(object sender, EyeGazePointArg e)
        {
            canvas.Dispatcher.Invoke(() => 
            {
                if(storyboard != null)
                {
                    storyboard.Stop();
                    storyboard = null;
                }

                storyboard = new Storyboard();

                DoubleAnimation aniX = new DoubleAnimation(lastpoint.X, e.Point.X, new Duration(TimeSpan.FromMilliseconds(e.WaitTime * 0.66)));
                DoubleAnimation aniY = new DoubleAnimation(lastpoint.Y, e.Point.Y, new Duration(TimeSpan.FromMilliseconds(e.WaitTime * 0.66)));

                Storyboard.SetTargetProperty(aniX, new PropertyPath("(Canvas.Left)"));
                Storyboard.SetTargetProperty(aniY, new PropertyPath("(Canvas.Top)"));
                Storyboard.SetTarget(aniX, pointer);
                Storyboard.SetTarget(aniY, pointer);

                storyboard.Children.Add(aniX);
                storyboard.Children.Add(aniY);

                storyboard.Begin();

                lastpoint = e.Point;

                SolidColorBrush brush = new SolidColorBrush(Color.FromArgb((byte)e.Color.Value4, (byte)e.Color.Value1, (byte)e.Color.Value2, (byte)e.Color.Value3));
                brush.Freeze();
                ellipse.Fill = brush;
            });
        }

        private void Stop()
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.ThreeDBorderWindow;
            Panel_StartMenu.Visibility = Visibility.Visible;
            Panel_StartMenu.IsEnabled = true;
            Bt_Stop.Visibility = Visibility.Hidden;
            Bt_Stop.IsEnabled = false;

            if (recorder != null)
            {
                recorder.Stop();
                recorder = null;
            }
        }

        private void Bt_Stop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }
    }
}
