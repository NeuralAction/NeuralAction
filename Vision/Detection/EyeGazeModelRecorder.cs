using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vision;

namespace Vision
{
    public class EyeGazePointArg
    {
        public Point Point { get; set; }
        public Scalar Color { get; set; }
        public double WaitTime { get; set; }

        public EyeGazePointArg(Point point, Scalar color, double wait)
        {
            Point = point;
            Color = color;
            WaitTime = wait;
        }
    }

    public class EyeGazeModelRecorder
    {
        public event EventHandler<EyeGazePointArg> SetPoint;
        public event EventHandler<VMat> FrameReady;
        public bool IsRecording { get; set; } = false;
        public DirectoryNode Parent { get; set; }
        public Size ScreenSize { get; set; }
        public string SessionName { get; set; }

        private Task recThread;
        private Capture capture;
        private object matLocker = new object();
        private VMat mat;
        private int captureCount = 0;
        private CancellationTokenSource tokenSource;

        public EyeGazeModelRecorder(string session, Size screenSize)
        {
            SessionName = session;
            ScreenSize = screenSize;

            DirectoryNode node = Storage.Root.GetDirectory($"[{DateTime.Now.ToString()}] {session}");
            Storage.FixPathChars(node);
            if (!node.IsExist)
                node.Create();
            Parent = node;

            FileNode file = node.GetFile("model.txt");
            if (file.IsExist)
                file.Delete();
            file.Create();
            using (Stream s = file.Open())
            {
                using (StreamWriter write = new StreamWriter(s))
                {
                    write.WriteLine($"scr:{screenSize.Width},{screenSize.Height}");
                    write.Flush();
                }
            }
        }

        public void Start()
        {
            Stop();

            if(SetPoint == null)
            {
                throw new ArgumentNullException(nameof(SetPoint));
            }

            capture = Capture.New(0);
            capture.FrameReady += Capture_FrameReady;
            capture.Start();

            tokenSource = new CancellationTokenSource();
            CancellationToken tk = tokenSource.Token;
            recThread = new Task(()=> 
            {
                while (!tk.IsCancellationRequested)
                {
                    Random rnd = new Random();
                    Point pt = new Point(rnd.NextDouble(0, ScreenSize.Width), rnd.NextDouble(0, ScreenSize.Height));

                    SetPoint.Invoke(this, new EyeGazePointArg(pt, Scalar.Red, 700));
                    Core.Sleep(700);

                    if (tk.IsCancellationRequested)
                        return;
                    SetPoint.Invoke(this, new EyeGazePointArg(pt, Scalar.Yellow, 500));
                    Core.Sleep(500);

                    if (tk.IsCancellationRequested)
                        return;
                    SetPoint.Invoke(this, new EyeGazePointArg(pt, Scalar.Green, 100));

                    while (mat == null)
                    {
                        Core.Sleep(1);

                        if (tk.IsCancellationRequested)
                            return;
                    }

                    lock (matLocker)
                    {
                        if (mat != null)
                        {
                            FileNode node = Parent.GetFile($"{captureCount},{Math.Round(pt.X)},{Math.Round(pt.Y)}.jpg");
                            Core.Cv.ImgWrite(node, mat);
                            captureCount++;
                            Logger.Log($"ImageCaptured. [{pt.ToString()}]");
                        }
                        else
                        {
                            throw new Exception("mat is null");
                        }
                    }

                    Core.Sleep(100);
                }
            }, tk);
            IsRecording = true;
            recThread.Start();
        }

        private void Capture_FrameReady(object sender, FrameArgs e)
        {
            lock (matLocker)
            {
                if(mat != null)
                {
                    mat.Dispose();
                    mat = null;
                }

                mat = e.VMat;
                FrameReady?.Invoke(this, mat);
            }
        }

        public void Stop()
        {
            if(capture != null)
            {
                capture.Stop();
                capture.Dispose();
                capture = null;
            }

            if (recThread != null)
            {
                IsRecording = false;
                tokenSource.Cancel();
                recThread = null;
                tokenSource.Dispose();
                tokenSource = null;
            }

            Core.Cv.CloseAllWindows();
        }
    }
}
