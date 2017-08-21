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
        public NotifyIcon NotifyIcon { get; set; }

        private SettingWindow settingWindow;

        private MenuWindow menuWindow;

        private System.Windows.Controls.ContextMenu notifyMenu;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Closed += MainWindow_Closed;

            Settings.Load();
            InputService.Init();

            NotifyIcon = new NotifyIcon();
            NotifyIcon.Icon = Properties.Resources.neuralaction_ico;
            NotifyIcon.Visible = true;
            NotifyIcon.MouseClick += NotifyIcon_MouseClick;
            NotifyIcon.Text = "NeuralAction";

            InputService.Current.Owner = this;
            InputService.Current.Settings = Settings.Current;
            InputService.Current.Start();
        }

        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if(settingWindow != null)
            {
                settingWindow.Close();
            }

            if (menuWindow == null)
            {
                MenuWindow menuWindow = new MenuWindow();
                menuWindow.Owner = this;
                menuWindow.Closed += delegate
                {
                    menuWindow = null;
                };
                menuWindow.Show();
            }
            else
            {
                menuWindow.Activate();
            }

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            NotifyIcon.Visible = false;
            Settings.Save();
            Environment.Exit(0);
        }


    }
}