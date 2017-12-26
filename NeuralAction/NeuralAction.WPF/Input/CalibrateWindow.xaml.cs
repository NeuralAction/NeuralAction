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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Vision;
using Vision.Detection;

namespace NeuralAction.WPF
{
    /// <summary>
    /// CaliCircleWIndow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CalibrateWindow : Window
    {
        double scrScale;
        public CalibrateWindow(EyeGazeCalibrater calib)
        {
            InitializeComponent();

            calib.Calibarting += Calib_Calibarting;
            calib.Calibrated += Calib_Calibrated;

            WindowState = WindowState.Maximized;

            Loaded += delegate
            {
                var scale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
                scrScale = scale;
                var scr = Screen.GetBounds(new System.Drawing.Point((int)(Left + ActualWidth / 2), (int)(Top + ActualHeight / 2)));
                var mw = (scr.Width / scale - ActualWidth) / 2;
                var mh = (scr.Height / scale - ActualHeight) / 2;
                Grid_Background.Margin = new Thickness(mw, mh, mw, mh);
            };
        }
        
        private void Calib_Calibarting(object sender, CalibratingArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var calib = (EyeGazeCalibrater)sender;
                double duration;
                Color color;
                switch (e.State)
                {
                    case CalibratingState.Point:
                        color = Colors.Lime;
                        duration = calib.Interval;
                        break;
                    case CalibratingState.Wait:
                        color = Colors.Yellow;
                        duration = calib.WaitInterval;
                        break;
                    case CalibratingState.SampleWait:
                        color = Colors.Orange;
                        duration = calib.SampleWaitInterval;
                        break;
                    case CalibratingState.Sample:
                        color = Colors.Red;
                        duration = calib.SampleInterval;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                Calib_Text.Text = (e.Percent * 100).ToString("0.0") + "%";
                Calib_Circle.Fill = new SolidColorBrush(color);
                Calib_Circle.Fill.Freeze();

                var storyboard = new Storyboard();
                var aniDur = new Duration(TimeSpan.FromMilliseconds(duration * 0.8));
                var aniX = new DoubleAnimation(Canvas.GetLeft(Calib), e.Data.X / scrScale, aniDur);
                var aniY = new DoubleAnimation(Canvas.GetTop(Calib), e.Data.Y / scrScale, aniDur);
                Storyboard.SetTarget(aniX, Calib);
                Storyboard.SetTarget(aniY, Calib);
                Storyboard.SetTargetProperty(aniX, new PropertyPath(Canvas.LeftProperty));
                Storyboard.SetTargetProperty(aniY, new PropertyPath(Canvas.TopProperty));
                storyboard.Children.Add(aniX);
                storyboard.Children.Add(aniY);
                Timeline.SetDesiredFrameRate(storyboard, 45);
                storyboard.Begin();
            });
        }

        private void Calib_Calibrated(object sender, CalibratedArgs e)
        {
            Dispatcher.Invoke(() => { Close(); });
        }
    }
}
