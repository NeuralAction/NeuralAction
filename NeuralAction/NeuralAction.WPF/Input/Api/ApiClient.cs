using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vision;
using Vision.Detection;

namespace NeuralAction.WPF
{
    public static class ApiSerializer
    {
        public class GazeEventArgs : EventArgs
        {
            public Point Position { get; set; }
            public ScreenProperties ScreenProperties { get; set; }
            public bool IsAvailable => IsFaceDetected && IsGazeDetected;
            public bool IsFaceDetected { get; set; }
            public bool IsGazeDetected { get; set; }

            public GazeEventArgs(Point pt, ScreenProperties screen, bool isFace, bool isGaze)
            {
                Position = pt;
                ScreenProperties = screen;

                IsFaceDetected = isFace;
                IsGazeDetected = isGaze;
            }
        }

        public class CursorReleasedArgs : EventArgs
        {
            public Point EndPosition { get; set; }
            public Point StartPosition { get; set; }
            public double Duration { get; set; }

            public CursorReleasedArgs(Point start, Point end, double ts)
            {
                EndPosition = end;
                StartPosition = start;
                Duration = ts;
            }
        }

        public static string Clicked(Point screenPoint)
        {
            return screenPoint.Serialize();
        }

        public static Point Clicked(string msg)
        {
            return msg.ToPoint();
        }

        public static string Tracked(GazeEventArgs arg)
        {
            return $"{arg.IsFaceDetected}/{arg.IsGazeDetected}/{arg.Position.Serialize()}/{arg.ScreenProperties}";
        }

        public static GazeEventArgs Tracked(string msg)
        {
            var spl = msg.Split('/');
            return new GazeEventArgs(spl[2].ToPoint(), spl[3].ToScreenProperties(), spl[0].ToBool(), spl[1].ToBool());
        }

        public static string Released(CursorReleasedArgs arg)
        {
            return $"{arg.Duration}/{arg.StartPosition.Serialize()}/{arg.EndPosition.Serialize()}";
        }

        public static CursorReleasedArgs Released(string msg)
        {
            var spl = msg.Split('/');
            return new CursorReleasedArgs(spl[1].ToPoint(), spl[2].ToPoint(), spl[0].ToDouble());
        }

        static double ToDouble(this string str)
        {
            return Convert.ToDouble(str);
        }

        static bool ToBool(this string str)
        {
            return Convert.ToBoolean(str);
        }

        static string Serialize(this Point pt)
        {
            if (pt == null)
                return "null";
            return $"{pt.X} {pt.Y}";
        }

        static Point ToPoint(this string str)
        {
            if (str == "null")
                return null;

            var parse = ParseString(str);
            return new Point(parse[0], parse[1]);
        }

        static string Serialize(this ScreenProperties scr)
        {
            return $"{scr.PixelSize.Width} {scr.PixelSize.Height} {scr.Size.Width} {scr.Size.Height} {scr.Origin.X} {scr.Origin.Y} {scr.Origin.Z}";
        }

        static ScreenProperties ToScreenProperties(this string str)
        {
            var parse = ParseString(str);
            return new ScreenProperties(new Point3D(parse[4], parse[5], parse[6]), new Size(parse[2], parse[3]), new Size(parse[0], parse[1]));
        }

        static double[] ParseString(string str)
        {
            var spl = str.Split(' ');
            var ret = new double[spl.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = Convert.ToDouble(spl[i]);
            }
            return ret;
        }
    }

    public class ApiClient
    {
        public bool IsStarted { get; set; } = false;

        public event EventHandler<ApiSerializer.GazeEventArgs> GazeTracked;
        public event EventHandler<Point> Clicked;
        public event EventHandler<ApiSerializer.CursorReleasedArgs> Released;

        NamedPipeClientStream client;
        Thread server;

        public void Start(int timeout = 10000)
        {
            Stop();

            client = new NamedPipeClientStream("GazeTracking");
            client.Connect(timeout);

            server = new Thread(() => { Proc(); });
            server.Start();
        }

        public void Join()
        {
            if(server != null)
            {
                server.Join();
            }
        }

        void Proc()
        {
            var reader = new StreamReader(client);
            var writer = new StreamWriter(client);

            while (IsStarted)
            {
                var read = reader.ReadLine();

                var spl = read.Split('|');
                switch (spl[0])
                {
                    case "released":
                        Released?.Invoke(this, ApiSerializer.Released(spl[1]));
                        break;
                    case "tracked":
                        GazeTracked?.Invoke(this, ApiSerializer.Tracked(spl[1]));
                        break;
                    case "clicked":
                        Clicked?.Invoke(this, ApiSerializer.Clicked(spl[1]));
                        break;
                    default:
                        throw new NotImplementedException("Unknown mode");
                }

                Thread.Sleep(1);
            }

            reader.Dispose();
            writer.Dispose();
        }

        public void Stop()
        {
            if (IsStarted)
            {
                IsStarted = false;
                server.Join(100);
                server.Abort();
                server = null;
            }
        }
    }
}
