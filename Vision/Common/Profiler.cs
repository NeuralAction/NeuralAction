using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision
{
    public static class Profiler
    {
        public static bool IsDebug = true;
        public static Stopwatch Stopwatch;
        public static event EventHandler<Dictionary<string, ProfilerData>> Reported;
        public static long ReportWait = 1000;

        static object DataLocker = new object();
        static Dictionary<string, ProfilerData> Data = new Dictionary<string, ProfilerData>();
        static long lastMs = 0;

        static Profiler()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public static void Start(string name)
        {
            if (!IsDebug)
                return;

            lock (DataLocker)
            {
                if (!Data.ContainsKey(name))
                {
                    Data.Add(name, new ProfilerData(name));
                }

                Data[name].Start(Stopwatch.ElapsedMilliseconds);
                Report();
            }
        }

        public static void End(string name)
        {
            if (!IsDebug)
                return;

            lock (DataLocker)
            {
                Data[name].End(Stopwatch.ElapsedMilliseconds);
                Report();
            }
        }

        static StringBuilder sb = new StringBuilder();
        private static void Report()
        {
            if(Stopwatch.ElapsedMilliseconds - lastMs > ReportWait)
            {
                foreach (ProfilerData d in Data.Values)
                {
                    sb.AppendLine(d.ToString());

                    d.Clear();
                }

                Logger.Log(sb.ToString());

                sb.Clear();
                
                Reported?.Invoke(null, Data);
                lastMs = Stopwatch.ElapsedMilliseconds;
            }
        }
    }

    public class ProfilerData
    {
        public string Name;
        public double Average
        {
            get
            {
                if(CaptureCount != 0)
                    return (double)CaptureDuration / CaptureCount;
                return 0;
            }
        }
        public int CaptureCount = 0;
        public long CaptureDuration = 0;

        long startMs = 0;
        bool isStarted = false;

        public ProfilerData(string name)
        {
            Name = name;
        }

        public void Start(long nowMs)
        {
            if (!isStarted)
            {
                startMs = nowMs;
            }
            else
            {

            }
            isStarted = true;
        }

        public void End(long nowMs)
        {
            if (isStarted)
            {
                CaptureDuration += nowMs - startMs;
                CaptureCount++;
                isStarted = false;
            }
        }

        public void Clear()
        {
            CaptureCount = 0;
            CaptureDuration = 0;
        }

        public override string ToString()
        {
            return string.Format("ProfilerData[{0}] Average: {1}", Name, Average.ToString("0.000"));
        }
    }
}
