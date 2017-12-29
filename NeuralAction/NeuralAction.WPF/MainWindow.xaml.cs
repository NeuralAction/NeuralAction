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
        public static NotifyIcon NotifyIcon { get; set; }
        public static SettingWindow SettingWindow;
        public static MenuWindow MenuWindow;

        public static void OpenSetting()
        {
            MenuWindow?.Close();

            if (SettingWindow == null)
            {
                SettingWindow = new SettingWindow();
                SettingWindow.Owner = App.Current.MainWindow;
                SettingWindow.Closed += delegate
                {
                    SettingWindow = null;
                };
                SettingWindow.Show();
            }
            else
            {
                SettingWindow.Activate();
            }
        }

        public static void OpenMenu()
        {
            SettingWindow?.Close();

            if (MenuWindow == null)
            {
                MenuWindow = new MenuWindow();
                MenuWindow.Owner = App.Current.MainWindow;
                MenuWindow.Closed += delegate
                {
                    MenuWindow = null;
                };
                MenuWindow.Show();
            }
            else
            {
                MenuWindow.Activate();
            }
        }

        public static void Exit()
        {
            NotifyIcon.Visible = false;
            Settings.Save();
            InputService.Current.Dispose();
            Environment.Exit(0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Closed += MainWindow_Closed;

            Settings.Load();
            InputService.Init();

            NotifyIcon = new NotifyIcon();
            using (var stream = System.Windows.Application.GetResourceStream(new Uri("Resources/icon.ico", UriKind.Relative)).Stream)
                NotifyIcon.Icon = new System.Drawing.Icon(stream);
            NotifyIcon.Visible = true;
            NotifyIcon.MouseClick += NotifyIcon_MouseClick;
            NotifyIcon.Text = "NeuralAction";

            InputService.Current.Owner = this;
            InputService.Current.Settings = Settings.Current;
            InputService.Current.Start();

            InputDebugWindow.Default.Show(InputService.Current);
        }

        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            OpenMenu();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Exit();
        }
    }
}