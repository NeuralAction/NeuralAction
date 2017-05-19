using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision.Windows
{
    public class WindowsCapture : Capture
    {
        public VideoCapture InnerCapture;
        public override object Object
        {
            get { return InnerCapture; }
            set { throw new NotImplementedException(); }
        }
        public override double FPS
        {
            get { return InnerCapture.Get(CaptureProperty.Fps); }
            set { InnerCapture.Set(CaptureProperty.Fps, value); }
        }

        public WindowsCapture(int index)
        {
            InnerCapture = new VideoCapture(index);
        }

        public WindowsCapture(string filepath)
        {
            InnerCapture = new VideoCapture(filepath);
        }

        public override bool CanQuery()
        {
            if (InnerCapture == null)
                return false;

            return InnerCapture.IsOpened();
        }

        public override void Dispose()
        {
            if(InnerCapture != null)
            {
                InnerCapture.Dispose();
                InnerCapture = null;
            }
        }

        public override VMat QueryFrame()
        {
            if(InnerCapture == null)
            {
                return null;
            }

            Mat frame = new Mat();
            if (InnerCapture.Read(frame))
            {
                return new WindowsMat(frame);
            }
            else
            {
                if (frame != null)
                    frame.Dispose();
                return null;
            }
        }

        protected override bool Opened()
        {
            if (InnerCapture == null)
                return false;

            return InnerCapture.IsOpened();
        }
    }
}
