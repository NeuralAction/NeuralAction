using Microsoft.Expression.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace NeuralAction.WPF
{

    public partial class MainWindow : Window
    {


        public NotifyIcon notify;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            notify = new NotifyIcon();
            notify.Icon = NeuralAction.WPF.Properties.Resources.neuralaction_ico;
            notify.Visible = true;

            notify.MouseClick += new System.Windows.Forms.MouseEventHandler(NotifyMouseDown);
            KeyWindow MW = new KeyWindow();
            MW.Owner = this;
            MW.ShowInTaskbar = false;
            MW.Show();

        }

        private void NotifyMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Windows.Controls.ContextMenu menu = (System.Windows.Controls.ContextMenu)this.FindResource("NotifierContextMenu");
            menu.IsOpen = true;
        }

        private void MenuItemSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow SW = new SettingWindow();
            SW.Show();
        }
    }
}