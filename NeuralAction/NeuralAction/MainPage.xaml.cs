using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;


namespace NeuralAction
{
    public partial class MainPage : ContentPage
    {
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
            SKRect rect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
            
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

        public MainPage()
        {
            InitializeComponent();
        }

        private void CircleButton_Clicked(object sender, EventArgs e)
        {
            App.Current.MainPage.DisplayAlert("Alert", "X: " + ((Point)sender).X.ToString() + " |  Y: " + ((Point)sender).Y.ToString() , "OK");
        }
    }
}
