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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Vision;

namespace EyeGazeGen
{
    /// <summary>
    /// ModelViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ModelViewer : Window
    {
        EyeGazeModel model;
        public ModelViewer(Window wnd, EyeGazeModel model)
        {
            this.model = model;

            InitializeComponent();

            Owner = wnd;
            Title = model.SessionName;

            viewControl.Model = model;
            viewControl.Update();
        }
    }
}
