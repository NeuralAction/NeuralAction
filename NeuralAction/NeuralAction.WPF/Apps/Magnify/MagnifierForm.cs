using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace NeuralAction.WPF.Magnify
{
    public class MagnifierForm
    {
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
                WinApi.NotWindowsFocus(Form.Handle);
                WinApi.SetTransClick(Form.Handle);
            };
        }

        public void Show()
        {
            Form.Show();
        }

        public void Close()
        {
            Form.Close();
        }
    }
}
