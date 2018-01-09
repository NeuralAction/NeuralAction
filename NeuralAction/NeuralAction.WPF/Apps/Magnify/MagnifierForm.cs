using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace NeuralAction.WPF.Magnify
{
    public class MagnifierForm : IDisposable
    {
        double magnification = 2;
        public double Magnification
        {
            get => magnification;
            set
            {
                magnification = value;
                if (Magnifier != null)
                    Magnifier.Magnification = magnification;
            }
        }
        public Magnifier Magnifier { get; set; }
        public Form Form { get; set; }

        public MagnifierForm()
        {
            Form = new Form()
            {
                FormBorderStyle = FormBorderStyle.None,
                TopMost = true,
            };

            Form.Load += delegate
            {
                Magnifier = new Magnifier(Form);
                Magnifier.Magnification = magnification;

                WinApi.NotWindowsFocus(Form.Handle);
                WinApi.SetTransClick(Form.Handle);
            };
        }

        public void Hide()
        {
            Form.Hide();
        }

        public void Show()
        {
            Form.Show();
        }

        public void Close()
        {
            Form?.Close();
        }

        public void Dispose()
        {
            Close();

            Magnifier?.Dispose();
            Magnifier = null;
        }
    }
}
