using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Vision
{
    public class EyeRect : Rect
    {
        public FaceRect Parent { get; set; }

        public EyeRect(FaceRect parent, Rect rect) : base(rect)
        {
            Parent = parent;
        }

        public EyeRect(FaceRect parent, Rectangle rect) : base(rect)
        {
            Parent = parent;
        }
        
        public void Draw(VMat frame, double thickness = 1)
        {
            Point center = new Point(Parent.X + X + Width * 0.5, Parent.Y + Y + Height * 0.5);
            double radius = (Width + Height) * 0.25;

            Core.Cv.DrawCircle(frame, center, radius, Scalar.Red, thickness, LineType.Link4, 0);
        }

        public VMat ROI(VMat frame)
        {
            return VMat.New(frame, new Rect(Parent.X + X, Parent.Y + Y, Width, Height));
        }
    }
}
