using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision
{
    public class EyesDetector
    {
        public double FaceScaleFactor { get; set; } = 1.2;
        public double FaceMinFactor { get; set; } = 0.15;
        public double FaceMaxFactor { get; set; } = 1.1;
        public double EyesScaleFactor { get; set; } = 1.15;
        public double EyesMinFactor { get; set; } = 0.15;
        public double EyesMaxFactor { get; set; } = 0.6;
        public int MaxSize { get; set; } = 320;
        public int MaxFaceSize { get; set; } = 110;
        public Interpolation Interpolation { get; set; } = Interpolation.Linear;

        CascadeClassifier FaceCascade;
        CascadeClassifier EyesCascade;

        public EyesDetector(string FaceXml, string EyesXml)
        {
            FaceCascade = CascadeClassifier.New(FaceXml);

            EyesCascade = CascadeClassifier.New(EyesXml);
        }

        public EyesDetector(EyesDetectorXmlLoader Loader) : this(Loader.FaceXmlPath, Loader.EyeXmlPath)
        {

        }
        
        public FaceRect[] Detect(VMat frame, bool debug = false)
        {
            using (VMat frame_gray = VMat.New())
            {
                Profiler.Start("DetectionPre");
                //pre conversion with opt.
                frame.ConvertColor(frame_gray, ColorConversion.BgrToGray);
                double scaleFactor = frame_gray.CalcScaleFactor(MaxSize);
                VMat frame_face = null;
                if(scaleFactor != 1)
                {
                    frame_face = VMat.New();
                    frame_gray.CopyTo(frame_face);
                    frame_gray.ClampSize(MaxSize, Interpolation);
                }
                else
                {
                    frame_face = frame_gray;
                }
                frame_gray.EqualizeHistogram();
                Profiler.End("DetectionPre");

                Profiler.Start("DetectionFace");
                double frameMinSize = Math.Min(frame_gray.Width, frame_gray.Height);
                Rect[] faces = FaceCascade.DetectMultiScale(frame_gray, FaceScaleFactor, 2, HaarDetectionType.ScaleImage, new Size(frameMinSize * FaceMinFactor), new Size(frameMinSize * FaceMaxFactor));
                List<FaceRect> FaceList = new List<FaceRect>();
                Profiler.End("DetectionFace");

                Profiler.Start("DetectionEyes");
                foreach (Rect face in faces)
                {
                    FaceRect faceRect = new FaceRect(face);
                    faceRect.Scale(1 / scaleFactor);

                    if (debug) { faceRect.Draw(frame); }

                    Profiler.Start("DetectionEye");
                    using (VMat faceROI = VMat.New(frame_face, faceRect))
                    {
                        double eyeScale = faceROI.ClampSize(MaxFaceSize, Interpolation);
                        double faceMinSize = Math.Min(faceROI.Width, faceROI.Height);
                        Rect[] eyes = EyesCascade.DetectMultiScale(faceROI, EyesScaleFactor, 2, HaarDetectionType.ScaleImage, new Size(faceMinSize * EyesMinFactor), new Size(faceMinSize * EyesMaxFactor));

                        foreach (Rect eye in eyes)
                        {
                            eye.Scale(1 / eyeScale);
                            EyeRect eyeRect = new EyeRect(faceRect, eye);
                            faceRect.Add(eyeRect);
                            if (debug) { eyeRect.Draw(frame); }
                        }
                    }
                    Profiler.End("DetectionEye");

                    FaceList.Add(faceRect);
                }
                Profiler.End("DetectionEyes");

                //clr
                if (frame_face != null)
                {
                    frame_face.Dispose();
                    frame_face = null;
                }
                return FaceList.ToArray();
            }
        }
    }
}
