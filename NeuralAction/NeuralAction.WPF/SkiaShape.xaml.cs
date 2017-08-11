using SkiaSharp;
using SkiaSharp.Views.Desktop;
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

namespace NeuralAction.WPF
{
    /// <summary>
    /// SkiaShape.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SkiaShape : Window
    {
        public SkiaShape()
        {
            InitializeComponent();
        }


        SKPaint arcPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,

            IsAntialias = true,
            Color = SKColor.Parse("#4A4A4A")
        };

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {

            

            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            SKPoint center = new SKPoint(info.Width / 2, info.Height / 2);

            canvas.Clear();

            float explodeOffset = 60;
            float radius = Math.Min(info.Width / 2, info.Height / 2) - 2 * explodeOffset;
            SKRect rect = new SKRect(center.X - radius, center.Y - radius,
                                     center.X + radius, center.Y + radius);


            float startAngle = (float)0;
            float sweepAngle = (float)45.0;

            using (SKPath path = new SKPath())
            {
                path.MoveTo(center);

                path.ArcTo(rect, startAngle, sweepAngle, false);

                path.Close();

                canvas.DrawPath(path, arcPaint);

                canvas.Restore();
            }
        }


    }
}
