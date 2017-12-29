using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vision;
using Vision.Detection;

namespace NeuralAction.WPF
{
    public class InputDebugWindow : IDisposable
    {
        public static InputDebugWindow Default;
        static InputDebugWindow()
        {
            Default = new InputDebugWindow();
        }

        public bool IsShowed { get; set; } = false;

        InputService service;

        CancellationTokenSource cancellation;
        Task task;

        Mat frame;
        object frameLocker = new object();
        bool frameUpdated = false;
        FaceRect[] faces;

        public InputDebugWindow()
        {

        }

        public void Show(InputService service)
        {
            if (IsShowed)
                Close();

            this.service = service;
            service.Cursor.GazeService.FrameCaptured += GazeService_FrameCaptured;
            service.Cursor.GazeService.FaceTracked += GazeService_FaceTracked;
            cancellation = new CancellationTokenSource();
            task = Task.Factory.StartNew(()=>Proc(cancellation.Token));
            IsShowed = true;
        }

        public void Close()
        {
            service.Cursor.GazeService.FrameCaptured -= GazeService_FrameCaptured;
            service.Cursor.GazeService.FaceTracked -= GazeService_FaceTracked;
            service = null;
            IsShowed = false;

            cancellation.Cancel();
            task?.Wait();
        }

        void Proc(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Core.Cv.CloseAllWindows();
                    return;
                }

                var key = Core.Cv.WaitKey(1);
                if (frameUpdated)
                {
                    lock (frameLocker)
                    {
                        frameUpdated = false;
                        Draw(frame, faces);
                        Core.Cv.ImgShow("Debug", frame);
                    }
                }
            }
        }

        void Draw(Mat mat, FaceRect[] faces)
        {
            if (faces != null)
            {
                foreach (var item in faces)
                {
                    item.Draw(mat, 2, true, true);
                }
            }
        }

        void GazeService_FaceTracked(object sender, FaceRect[] e)
        {
            lock (frameLocker)
            {
                faces = e;
            }
        }

        void GazeService_FrameCaptured(object sender, OpenCvSharp.Native.FrameArgs e)
        {
            lock (frameLocker)
            {
                frameUpdated = true;
                if (frame == null || frame.Width != e.Mat.Width)
                {
                    frame?.Dispose();
                    frame = e.Mat.Clone();
                }
                else
                {
                    e.Mat.CopyTo(frame);
                }
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
