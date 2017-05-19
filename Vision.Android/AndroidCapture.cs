using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Hardware = Android.Hardware;
using Graphics = Android.Graphics;
using OpenCV.VideoIO;
using OpenCV.Core;

#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.

namespace Vision.Android
{
    public class AndroidCapture : Capture
    {
        public override object Object
        {
            get
            {
                return Camera;
            }
        }

        private double fps;
        public override double FPS { get { return fps; } }

        private Hardware.Camera Camera;
        private int cameraIndex;
        private int width;
        private int height;
        private Graphics.SurfaceTexture Texture;
        private Graphics.ImageFormatType captureType;
        private Mat capturedBuffer;
        private object capturedBufferLocker = new object();

        public AndroidCapture(int index)
        {
            cameraIndex = index;
            try
            {
                Camera = Hardware.Camera.Open(index);

                Texture = new Graphics.SurfaceTexture(0);

                CameraPreviewCallback callback = new CameraPreviewCallback();
                callback.PreviewUpdated += Callback_PreviewUpdated;

                Hardware.Camera.Parameters parameter = Camera.GetParameters();
                List<Hardware.Camera.Size> supportSize = parameter.SupportedPreviewSizes.ToList();
                foreach (Hardware.Camera.Size size in supportSize)
                {
                    Logger.Log(this, string.Format("Camera Support Size: W{0},H{1}", size.Width, size.Height));

                    if (size.Width == 1920 && size.Height == 1080)
                    {
                        parameter.SetPreviewSize(size.Width, size.Height);
                        Logger.Log(this, string.Format("SET Camera Size: W{0},H{1}", size.Width, size.Height));
                        //break;
                    }
                }
                width = parameter.PreviewSize.Width;
                height = parameter.PreviewSize.Height;
                fps = parameter.PreviewFrameRate;
                captureType = parameter.PreviewFormat;

                string[] supportedFocusMode = parameter.SupportedFocusModes.ToArray();
                if (supportedFocusMode.Contains(Hardware.Camera.Parameters.FocusModeContinuousPicture))
                {
                    parameter.FocusMode = Hardware.Camera.Parameters.FocusModeContinuousPicture;
                }
                else if (supportedFocusMode.Contains(Hardware.Camera.Parameters.FocusModeContinuousVideo))
                {
                    parameter.FocusMode = Hardware.Camera.Parameters.FocusModeContinuousVideo;
                }

                Logger.Log(this, string.Format("Camera is creating W{0} H{1} FPS{2}", width, height, fps));
                Camera.SetParameters(parameter);
                Camera.SetPreviewTexture(Texture);
                Camera.SetPreviewCallback(callback);
                Camera.StartPreview();
            }
            catch(Exception ex)
            {
                Logger.Error(this, "Camera Init Failed.\n" + ex.ToString());

                Dispose();

                throw new ArgumentException("Camera Exception", ex);
            }
        }
        
        private void Callback_PreviewUpdated(object sender, PreviewUpdatedEventArgs e)
        {
            if (e.Buffer != null)
            {
                Profiler.Start("CaptureCvt");
                Mat mat = new Mat((int)Math.Round(height * 1.5), width, CvType.Cv8uc1);

                mat.Put(0, 0, e.Buffer);

                switch (captureType)
                {
                    case Graphics.ImageFormatType.Nv16:
                        OpenCV.ImgProc.Imgproc.CvtColor(mat, mat, (int)ColorConversion.YuvToRgba_NV12);
                        break;
                    case Graphics.ImageFormatType.Nv21:
                        OpenCV.ImgProc.Imgproc.CvtColor(mat, mat, (int)ColorConversion.YuvToRgba_NV21);
                        break;
                    case Graphics.ImageFormatType.Rgb565:
                        OpenCV.ImgProc.Imgproc.CvtColor(mat, mat, (int)ColorConversion.Bgr565ToRgba);
                        break;
                    case Graphics.ImageFormatType.Yuv420888:
                        OpenCV.ImgProc.Imgproc.CvtColor(mat, mat, (int)ColorConversion.Bgr565ToRgba);
                        break;
                    case Graphics.ImageFormatType.Unknown:
                    case Graphics.ImageFormatType.Yuy2:
                    case Graphics.ImageFormatType.Yv12:
                    case Graphics.ImageFormatType.Raw10:
                    case Graphics.ImageFormatType.RawSensor:
                    default:
                        throw new NotImplementedException("Unknown Camera Format");
                }

                OpenCV.Core.Core.Transpose(mat, mat);

                if (cameraIndex == 1)
                {
                    OpenCV.Core.Core.Flip(mat, mat, (int)FlipMode.XY);
                }
                else
                {
                    OpenCV.Core.Core.Flip(mat, mat, (int)FlipMode.Y);
                }
                Profiler.End("CaptureCvt");

                lock (capturedBufferLocker)
                {
                    capturedBuffer = mat;
                }
            }
        }

        public AndroidCapture(string filepath)
        {
            throw new NotImplementedException();
        }

        public override bool CanQuery()
        {
            if(Camera != null && capturedBuffer != null)
            {
                return true;
            }
            return false;
        }

        public override void Dispose()
        {
            if (Camera != null)
            {
                Camera.StopPreview();
                Camera.SetPreviewCallback(null);
                Camera.SetPreviewTexture(null);
                Camera.Release();
                Camera.Dispose();
                Camera = null;
            }
            
            if(Texture != null)
            {
                Texture.Release();
                Texture.Dispose();
                Texture = null;
            }
        }

        public override VMat QueryFrame()
        {
            lock (capturedBufferLocker)
            {
                if(capturedBuffer != null)
                {
                    VMat ret = new AndroidMat(capturedBuffer);

                    capturedBuffer = null;

                    return ret;
                }
                return null;
            }
        }

        protected override bool Opened()
        {
            if (Camera != null)
            {
                return true;
            }
            return false;
        }
    }
}

#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.