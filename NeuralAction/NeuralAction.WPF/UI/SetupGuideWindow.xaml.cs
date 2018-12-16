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
using System.Windows.Shapes;

namespace NeuralAction.WPF
{
    /// <summary>
    /// SetupGuideWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SetupGuideWindow : Window
    {
        public List<Func<FrameworkElement>> Pages { get; set; } = new List<Func<FrameworkElement>>()
        {
            ()=>new SetupGuidePlaceWebcam(),
            ()=>new SetupGuideDPI(),
            ()=>new SetupGuideOther(),
        };

        int index;
        public int Index
        {
            get => index;
            set
            {
                if (value != index)
                {
                    index = value;
                    if (index == Pages.Count)
                        Close();
                    else
                        UpdatePage();
                }
            }
        }

        public SetupGuideWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += delegate
            {
                DragMove();
            };

            UpdatePage();
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            Index = Math.Max(0, Index - 1);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            Index = Math.Min(Pages.Count, Index + 1);
        }

        KeyTime FromMill(double mill)
        {
            return KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(mill));
        }

        Storyboard SlideAni(FrameworkElement control, EasingMode mode)
        {
            double opFrom = 0, opTo = 1, marFrom = -24, marTo = 0;
            if (mode == EasingMode.EaseOut)
            {
                opFrom = 1; opTo = 0; marFrom = 0; marTo = 24;
            }
            var storyboard = new Storyboard();
            var ease = new CubicEase() { EasingMode = mode };
            var opacity = new DoubleAnimationUsingKeyFrames();
            opacity.KeyFrames.Add(new EasingDoubleKeyFrame(opFrom, FromMill(0), ease));
            opacity.KeyFrames.Add(new EasingDoubleKeyFrame(opTo, FromMill(500), ease));
            var margin = new ThicknessAnimationUsingKeyFrames();
            margin.KeyFrames.Add(new EasingThicknessKeyFrame(new Thickness(marFrom, 0, -marFrom, 0), FromMill(0), ease));
            margin.KeyFrames.Add(new EasingThicknessKeyFrame(new Thickness(marTo, 0, -marTo, 0), FromMill(500), ease));
            Storyboard.SetTarget(margin, control);
            Storyboard.SetTarget(opacity, control);
            Storyboard.SetTargetProperty(margin, new PropertyPath(MarginProperty));
            Storyboard.SetTargetProperty(opacity, new PropertyPath(OpacityProperty));
            storyboard.Children.Add(margin);
            storyboard.Children.Add(opacity);
            return storyboard;
        }

        void UpdatePage()
        {
            if (Grid_Content.Children.Count > 0)
            {
                var item = (FrameworkElement)Grid_Content.Children[Grid_Content.Children.Count - 1];
                var ani = SlideAni(item, EasingMode.EaseOut);
                ani.Completed += delegate
                {
                    Grid_Content.Children.Remove(item);
                };
                ani.Begin();
            }

            var current = Pages[index]();
            current.Opacity = 0;
            Grid_Content.Children.Add(current);
            var aniIn = SlideAni(current, EasingMode.EaseIn);
            aniIn.Begin();
        }
    }
}
