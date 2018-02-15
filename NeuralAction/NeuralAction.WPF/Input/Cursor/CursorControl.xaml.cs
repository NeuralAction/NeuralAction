using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NeuralAction.WPF
{
    /// <summary>
    /// CursorControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CursorControl : UserControl
    {
        Storyboard CursorOn;
        Storyboard CursorOff;
        Storyboard CursorClick;
        Storyboard CursorClickOff;

        public CursorControl()
        {
            InitializeComponent();

            CursorOff = (Storyboard)FindResource("CursorOff");
            CursorOn = (Storyboard)FindResource("CursorOn");
            CursorClick = (Storyboard)FindResource("CursorClick");
            CursorClickOff = (Storyboard)FindResource("CursorClickOff");

            CursorOff.Begin();
        }

        public void Show()
        {
            CursorOff.Stop();
            CursorOn.Begin();
        }

        public void Hide()
        {
            CursorOn.Stop();
            CursorOff.Begin();
        }

        DispatcherTimer clickWait;
        public void Click()
        {
            CursorClickOff.Stop();
            CursorClick.Begin();
            if (clickWait == null)
            {
                clickWait = new DispatcherTimer();
                clickWait.Tick += delegate
                {
                    CursorClick.Stop();
                    CursorClickOff.Begin();
                    clickWait.Stop();
                };
                clickWait.Interval = TimeSpan.FromMilliseconds(208);
            }
            clickWait.Start();
        }
    }
}
