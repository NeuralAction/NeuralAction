using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UIAccessibility
{
    public class AccessibleTrackedArgs : EventArgs
    {
        public Point Position { get; set; }
        public Accessible RawHit { get; set; }
        public Accessible Element { get; set; }
        
        public UIElementType ElementType => Element == null ? UIElementType.None : Element.Type;

        public AccessibleTrackedArgs()
        {

        }
    }

    public class AccessibleNotifyService : IDisposable
    {
        public event EventHandler<AccessibleTrackedArgs> Tracked;
        public double Interval { get; set; } = 10;

        Thread thread;

        public AccessibleNotifyService()
        {

        }

        public void Start()
        {
            if (thread != null)
                Stop();

            thread = new Thread(() =>
            {
                while (true)
                {
                    Proc();
                    Thread.Sleep((int)Math.Max(1, Interval));
                }
            });
            thread.Name = "AccessibleNotifyService";
            thread.IsBackground = true;
            thread.Start();
        }

        void Proc()
        {
            var pt = NativeMethods.GetCursorPos();
            var result = UiHitTest.GetUiHit(new Point(pt));
            var arg = new AccessibleTrackedArgs()
            {
                Position = new Point(pt),
                Element = result.UiElement,
                RawHit = result.RawHit
            };

            Tracked?.Invoke(this, arg);
        }

        public void Stop()
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
