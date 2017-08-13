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

namespace NeuralAction.WPF
{
    /// <summary>
    /// KeyWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class KeyWindow : Window
    {


        public static IntPtr FocusedHandle { get; set; }
        public static bool RestoreClipboard { get; set; } = true;

        public KeyWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CursorService cursorServcie = new CursorService();

            cursorServcie.StartAsync(0);

            Keyboard.KeymapChange(Keyboard.GetKeymapArray(Keyboard.CurrentLanguage));
        }


    }
}
