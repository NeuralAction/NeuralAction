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




        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            SKPaint Paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,

                IsAntialias = true,
                Color = ((SKCanvasView)sender).BackgroundColor.ToSKColor()
            };
            SKPoint center = new SKPoint(info.Width / 2, info.Height / 2);

            canvas.Clear();

            float radius = Math.Min(info.Width / 2, info.Height / 2);

            using (SKPath path = new SKPath())
            {
                path.MoveTo(center);

                path.AddCircle(center.X , center.Y, radius);

                path.Close();

                canvas.DrawPath(path, Paint);

                canvas.Restore();
                ((SKCanvasView)sender).BackgroundColor = Color.Transparent;

            }

            //SKCanvasView info = (SKCanvasView)sender;
            //float size = (float)info.WidthRequest;
            //SKSurface surface = args.Surface;
            //SKCanvas canvas = surface.Canvas;

            //SKPoint center = new SKPoint(size / 2, size / 2);

            //canvas.Clear();

            //float radius = size / 2;     

            //using (SKPath path = new SKPath())
            //{
            //    path.MoveTo(center);

            //    path.AddCircle(center.X - radius, center.Y - radius, radius);

            //    path.Close();

            //    canvas.DrawPath(path, arcPaint);
               
            //    canvas.Restore();
            //}
        }


        public MainPage()
        {
            InitializeComponent();

        }


private void CircleButton_Clicked(object sender, EventArgs e)
        {
            //App.Current.MainPage.DisplayAlert("Alert", "X: " + ((Point)sender).X.ToString() + " |  Y: " + ((Point)sender).Y.ToString() , "OK");
        }

    }
}
