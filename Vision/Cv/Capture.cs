using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision
{
    public abstract class Capture : VirtualObject, IDisposable
    {
        public virtual bool IsOpened
        {
            get
            {
                return Opened();
            }
        }

        public virtual double FPS { get; set; }

        public abstract void Dispose();
        public abstract VMat QueryFrame();
        public abstract bool CanQuery();

        protected abstract bool Opened();

        public static Capture New(int index)
        {
            return Core.Cv.CreateCapture(index);
        }

        public static Capture New(string filePath)
        {
            return Core.Cv.CreateCapture(filePath);
        }
    }
}
