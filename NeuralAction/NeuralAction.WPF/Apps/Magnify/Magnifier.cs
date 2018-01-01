using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace NeuralAction.WPF.Magnify
{
    public class Magnifier : IDisposable
    {
        double magnification = 2.0f;
        public double Magnification
        {
            get => magnification;
            set
            {
                if (magnification != value)
                {
                    magnification = value;
                    // Set the magnification factor.
                    Transformation matrix = new Transformation((float)magnification);
                    NativeMethods.MagSetWindowTransform(hwndMag, ref matrix);
                }
            }
        }

        RECT sourceRect = new RECT();

        public double Top => sourceRect.top;
        public double Left => sourceRect.left;
        public double Bottom => sourceRect.bottom;
        public double Right => sourceRect.right;
        public double Width => Right - Left;
        public double Height => Bottom - Top;

        public int CenterX { get; set; } = 100;
        public int CenterY { get; set; } = 100;

        Form Form;
        IntPtr hwndMag;
        RECT magWindowRect = new RECT();
        Timer timer;

        bool initialized;

        public Magnifier(Form form)
        {
            Form = form;

            Form.Resize += FormResize;
            Form.FormClosing += FormClosing;

            timer = new Timer();
            timer.Tick += TimerTick;

            initialized = NativeMethods.MagInitialize();
            if (initialized)
            {
                SetupMagnifier();

                timer.Interval = NativeMethods.USER_TIMER_MINIMUM;
                //timer.Enabled = true;
            }
            else { throw new Exception(); }
        }

        public void UpdateMaginifier()
        {
            if ((!initialized) || (hwndMag == IntPtr.Zero))
                return;

            sourceRect = new RECT();

            int width = (int)((magWindowRect.right - magWindowRect.left) / magnification);
            int height = (int)((magWindowRect.bottom - magWindowRect.top) / magnification);

            sourceRect.left = CenterX - width / 2;
            sourceRect.top = CenterY - height / 2;

            // Don't scroll outside desktop area.
            if (sourceRect.left < 0)
            {
                sourceRect.left = 0;
            }
            if (sourceRect.left > NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN) - width)
            {
                sourceRect.left = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN) - width;
            }
            sourceRect.right = sourceRect.left + width;

            if (sourceRect.top < 0)
            {
                sourceRect.top = 0;
            }
            if (sourceRect.top > NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN) - height)
            {
                sourceRect.top = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN) - height;
            }
            sourceRect.bottom = sourceRect.top + height;

            if (this.Form == null)
            {
                timer.Enabled = false;
                return;
            }

            if (this.Form.IsDisposed)
            {
                timer.Enabled = false;
                return;
            }

            // Set the source rectangle for the magnifier control.
            NativeMethods.MagSetWindowSource(hwndMag, sourceRect);

            // Reclaim topmost status, to prevent unmagnified menus from remaining in view. 
            NativeMethods.SetWindowPos(Form.Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                (int)SetWindowPosFlags.SWP_NOACTIVATE | (int)SetWindowPosFlags.SWP_NOMOVE | (int)SetWindowPosFlags.SWP_NOSIZE);

            // Force redraw.
            NativeMethods.InvalidateRect(hwndMag, IntPtr.Zero, true);
        }

        void SetupMagnifier()
        {
            if (!initialized)
                return;

            IntPtr hInst;

            hInst = NativeMethods.GetModuleHandle(null);

            // Make the window opaque.
            Form.AllowTransparency = true;
            Form.TransparencyKey = Color.Empty;
            Form.Opacity = 255;

            // Create a magnifier control that fills the client area.
            NativeMethods.GetClientRect(Form.Handle, ref magWindowRect);
            hwndMag = NativeMethods.CreateWindow((int)ExtendedWindowStyles.WS_EX_TOOLWINDOW, NativeMethods.WC_MAGNIFIER,
                "MagnifierWindow", (int)WindowStyles.WS_CHILD | (int)MagnifierStyle.MS_SHOWMAGNIFIEDCURSOR | (int)WindowStyles.WS_VISIBLE,
                magWindowRect.left, magWindowRect.top, magWindowRect.right, magWindowRect.bottom, Form.Handle, IntPtr.Zero, hInst, IntPtr.Zero);

            if (hwndMag == IntPtr.Zero)
            {
                return;
            }

            // Set the magnification factor.
            Transformation matrix = new Transformation((float)magnification);
            NativeMethods.MagSetWindowTransform(hwndMag, ref matrix);
        }

        void TimerTick(object sender, EventArgs e)
        {
            UpdateMaginifier();
        }

        void FormResize(object sender, EventArgs e)
        {
            ResizeMagnifier();
        }

        void ResizeMagnifier()
        {
            if (initialized && (hwndMag != IntPtr.Zero))
            {
                NativeMethods.GetClientRect(Form.Handle, ref magWindowRect);
                // Resize the control to fill the window.
                NativeMethods.SetWindowPos(hwndMag, IntPtr.Zero, magWindowRect.left, magWindowRect.top, magWindowRect.right, magWindowRect.bottom, 0);
            }
        }

        void FormClosing(object sender, FormClosingEventArgs e)
        {
            timer.Enabled = false;
        }
        
        #region IDisposable Members

        void RemoveMagnifier()
        {
            if (initialized)
                NativeMethods.MagUninitialize();
        }

        void Dispose(bool disposing)
        {
            timer.Enabled = false;
            if (disposing)
                timer.Dispose();
            timer = null;
            Form.Resize -= FormResize;
            RemoveMagnifier();
        }

        ~Magnifier()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
