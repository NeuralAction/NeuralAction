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

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public NotifyIcon NotifyIcon { get; set; }

        private SettingWindow settingWindow;

        private System.Windows.Controls.ContextMenu notifyMenu;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Closed += MainWindow_Closed;

            Settings.Load();
            InputService.Init();

            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();

            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem();
            open.Text = "Open";
            open.Click += Open_Click;

            System.Windows.Forms.MenuItem setting = new System.Windows.Forms.MenuItem();
            setting.Text = "Setting";
            setting.Click += Setting_Click;

            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem();
            exit.Text = "Exit";
            exit.Click += Exit_Click;

            menu.MenuItems.Add(open);
            menu.MenuItems.Add(setting);
            menu.MenuItems.Add(exit);

            NotifyIcon = new NotifyIcon();
            NotifyIcon.Icon = Properties.Resources.neuralaction_ico;
            NotifyIcon.Visible = true;
            NotifyIcon.ContextMenu = menu;
            NotifyIcon.Text = "NeuralAction";

            InputService.Current.Owner = this;
            InputService.Current.Settings = Settings.Current;
            InputService.Current.Start();

            InputService.Current.Cursor.GazeTracked += Cursor_GazeTracked;
           
        }

        private void Cursor_GazeTracked(object sender, GazeEventArgs e)
        {
            if(e.IsGazeDetected)
            {
                // 눈 감는거 인식 어떻게하죵... 일단 e.point.x, e.point.y 로 좌표 구하고
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint) e.Point.X, (uint) e.Point.Y, 0, 0);
            }
        }

        private void Open_Click(object sender, EventArgs e)
        {
            InputService.Current.ShowKeyboard();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            NotifyIcon.Visible = false;
            Settings.Save();
            Environment.Exit(0);
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Setting_Click(object sender, EventArgs e)
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


    }
}