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
        private System.Windows.Controls.ContextMenu notifyMenu;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Closed += MainWindow_Closed;

            Settings.Load();
            InputService.Init();

            notifyMenu = (System.Windows.Controls.ContextMenu)FindResource("NotifyContextMenu");

            NotifyIcon = new NotifyIcon();
            NotifyIcon.Icon = Properties.Resources.neuralaction_ico;
            NotifyIcon.Text = "NeuralAction";
            NotifyIcon.Visible = true;
            NotifyIcon.MouseDown += NotifyMouseDown;

            InputService.Current.Owner = this;
            InputService.Current.Settings = Settings.Current;
            InputService.Current.Start();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            NotifyIcon.Visible = false;
            Settings.Save();
            Environment.Exit(0);
        }

        private void NotifyMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                notifyMenu.IsOpen = true;
                notifyMenu.StaysOpen = false;
            }
        }

        private void MenuSetting_Click(object sender, RoutedEventArgs e)
        {
            if (settingWindow == null)
            {
                SettingWindow settingWindow = new SettingWindow();
                settingWindow.Owner = this;
                settingWindow.Closed += delegate
                {
                    settingWindow = null;
                };
                settingWindow.Show();
            }
            else
            {
                settingWindow.Activate();
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            InputService.Current.ShowKeyboard();
        }
    }
}