using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vision;

namespace Vision.Tests
{
    public class FaceDetection
    {
        string FilePath;
        int index = -1;
        EyesDetector detector;

        private FaceDetection(string faceXml, string eyeXml)
        {
            detector = new EyesDetector(faceXml, eyeXml);
        }

        public FaceDetection(string filePath, string faceXml, string eyeXml) : this(faceXml, eyeXml)
        {
            FilePath = filePath;
        }

        public FaceDetection(int index, string faceXml, string eyeXml) : this(faceXml, eyeXml)
        {
            this.index = index;
        }

        public FaceDetection(int index, EyesDetectorXmlLoader loader) : this(index, loader.FaceXmlPath, loader.EyeXmlPath)
        {

        }

        public FaceDetection(string filepath, EyesDetectorXmlLoader loader) : this(filepath, loader.FaceXmlPath, loader.EyeXmlPath)
        {

        }

        public void Run()
        {
            Logger.Log(this, "Press E to Exit");
            Logger.Log(this, "Press D to Toggle Draw");
            Logger.Log(this, "Press C to Capture");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Capture capture;
            if (FilePath != null)
            {
                capture = Capture.New(FilePath);
            }
            else
            {
                capture = Capture.New(index);
            }

            double lastms = 0;
            double fps = capture.FPS;
            bool on = true;
            bool draw = true;
            bool save = false;
            while (on)
            {
                lastms = sw.ElapsedMilliseconds;

                if (capture.CanQuery())
                {
                    using (VMat frame = capture.QueryFrame())
                    {
                        if (frame != null && !frame.IsEmpty)
                        {
                            FaceRect[] rect = detector.Detect(frame, draw);

                            if (save)
                            {
                                save = false;

                                for (int i = 0; i < rect.Length; i++)
                                {
                                    Core.Cv.ImgWrite("face"+i.ToString()+".jpg", rect[i].ROI(frame));

                                    for (int j = 0; j < rect[i].Children.Count; j++)
                                    {
                                        Core.Cv.ImgWrite("eye" + j.ToString() + ".jpg", rect[i].Children[j].ROI(frame));
                                    }
                                }
                            }

                            Core.Cv.ImgShow("Vision Tests - FaceDetection", frame);
                        }
                    }
                }

                int wait = (int)Math.Round(Math.Max(1, Math.Min(1000, (1000/Math.Max(1, fps)) - sw.ElapsedMilliseconds + lastms)));
                char c = Core.Cv.WaitKey(wait);
                switch (c)
                {
                    case 'e':
                        on = false;
                        break;
                    case 'd':
                        draw = !draw;
                        break;
                    case 'c':
                        save = true;
                        break;
                }
            }

            sw.Stop();

            Core.Cv.CloseAllWindows();
        }
    }
}
