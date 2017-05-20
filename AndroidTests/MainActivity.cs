using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Vision;
using Vision.Tests;
using Vision.Android;
using System.Threading;
using System.Diagnostics;
using Android.Util;

namespace AndroidTests
{
    [Activity(Label = "AndroidTests", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate 
            {
                button.Text = string.Format("{0} clicks!", count++);

                captureon = false;
                capThread.Join(1000);
                capThread.Abort();

                index++;
                index %= 2;

                startCapture();

                Thread.Sleep(500);
            };

            ImageView img = FindViewById<ImageView>(Resource.Id.imageView1);

            Core.Init(new AndroidCore(this, this, img));

            detect = new EyesDetector(new EyesDetectorXmlLoader());
            detect.Interpolation = Interpolation.Cubic;
            detect.MaxSize = 240;
            detect.MaxFaceSize = 90;
            detect.FaceMaxFactor = 0.9;

            startCapture();
        }

        EyesDetector detect;
        Capture capture;
        Thread capThread;
        object captureLock = new object();
        bool captureon = true;
        int index = 1;
        int yoffset = 0;

        private void startCapture()
        {
            capThread = new Thread(new ThreadStart(() =>
            {
                record();
                return;
            }));
            capThread.IsBackground = true;

            capThread.Start();
        }

        private void record()
        {
            if (capture != null)
            {
                capture.Dispose();
                capture = null;
                Thread.Sleep(250);
            }

            captureon = true;
            capture = Capture.New(index);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            int fpscount = 0;
            int fps = 0;
            long framelastMs = 0;
            long lastMs = 0;
            double targetFPS = capture.FPS;

            long detectMs = 0;
            int detectCount = 0;
            double detectAvg = 0;

            while (captureon)
            {
                lastMs = sw.ElapsedMilliseconds;
                Profiler.Start("CapMain");
                using (VMat mat = capture.QueryFrame())
                {
                    if (mat != null && !mat.IsEmpty)
                    {
                        fpscount++;

                        Profiler.Start("DetectionALL");
                        long predetect = sw.ElapsedMilliseconds;
                        FaceRect[] rect = detect.Detect(mat);
                        detectCount++;
                        detectMs += sw.ElapsedMilliseconds - predetect;
                        Profiler.End("DetectionALL");

                        Profiler.Start("Draw");
                        foreach (FaceRect f in rect)
                            f.Draw(mat, 3, true);
                        yoffset += 10;
                        yoffset %= 500;
                        mat.DrawText(50, 120 + yoffset, "HELLO WORLD");
                        mat.DrawText(50, 50, "FPS: " + fps + " Detect: " + detectAvg.ToString("0.00") + "ms", Scalar.Green);
                        Profiler.End("Draw");

                        Profiler.Start("imshow");
                        Core.Cv.ImgShow("camera", mat);
                        Profiler.End("imshow");

                        Profiler.End("CapMain");
                    }
                    else
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                }

                if (sw.ElapsedMilliseconds - framelastMs >= 1000)
                {
                    fps = fpscount;
                    fpscount = 0;
                    detectAvg = (double)detectMs / detectCount;
                    detectMs = detectCount = 0;
                    Logger.Log("FPS : " + fps);
                    framelastMs = sw.ElapsedMilliseconds;
                }

                int sleep = (int)Math.Max(1, Math.Min(1000, (1000/targetFPS) - sw.ElapsedMilliseconds + lastMs - 1));
                if(sleep != 1)
                    Thread.Sleep(sleep);
            }

            sw.Stop();
        }
    }
}

