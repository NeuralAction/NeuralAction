using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        void NotWindowsFocus()
        {
            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
            GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        public static IntPtr FocusedHandle { get; set; }
        public static bool RestoreClipboard { get; set; } = true;

        public KeyWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.KeymapChange(Keyboard.GetKeymapArray(Keyboard.CurrentLanguage));

            NotWindowsFocus();
        }

        private void Keyboard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NotWindowsFocus();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
