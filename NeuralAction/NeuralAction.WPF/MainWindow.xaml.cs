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
using System.Windows.Threading;

namespace NeuralAction.WPF
{
    public partial class MainWindow : Window
    {
        public static NotifyIcon NotifyIcon;
        public static SettingWindow SettingWindow;
        public static MenuWindow MenuWindow;
        public static IconGadget Gadget;

        static DispatcherTimer gadgetTimer;
        static MainWindow()
        {
            gadgetTimer = new DispatcherTimer();
            gadgetTimer.Tick += delegate
            {
                if(MenuWindow == null && SettingWindow == null && Gadget == null)
                {
                    Gadget = new IconGadget();
                    Gadget.Closed += delegate
                    {
                        Gadget = null;
                    };
                    Gadget.Show();
                }
            };
            gadgetTimer.Interval = TimeSpan.FromMilliseconds(300);
            gadgetTimer.Start();
        }

        public static void OpenSetting()
        {
            MenuWindow?.Close();
            Gadget?.Close();

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
            Gadget?.Close();

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
            SettingWindow?.Close();
            Gadget?.Close();
            MenuWindow?.Close();

            NotifyIcon.Visible = false;
            NotifyIcon.Icon = null;
            NotifyIcon.Dispose();

            Settings.Save();

            InputService.Current.Dispose();

            Environment.Exit(0);
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Send.AddWindow(this);

            Closed += MainWindow_Closed;

            Settings.Load();
            InputService.Init();

            NotifyIcon = new NotifyIcon();
            using (var stream = Util.GetResourceStream("Resources/icon.ico"))
                NotifyIcon.Icon = new System.Drawing.Icon(stream);
            NotifyIcon.Visible = true;
            NotifyIcon.MouseClick += delegate
            {
                if (MenuWindow == null)
                    OpenMenu();
                else
                    MenuWindow.Close();
            };
            NotifyIcon.Text = "NeuralAction";

            InputService.Current.Owner = this;
            InputService.Current.Start();

            InputDebugWindow.Default.Show();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            Exit();
        }
    }
}