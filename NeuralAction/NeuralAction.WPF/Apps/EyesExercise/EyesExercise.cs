using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace NeuralAction.WPF.Apps.EyesExercise
{
    public class EyesExercise : IDisposable  {

        public static EyesExercise Current = new EyesExercise();

        public bool IsShowed { get; set; } = false;

        double wpfScale = InputService.Current.Cursor.Window.WpfScale;
        EyesExerciseWindow Window;

        Timer moveUpdater;

        Stopwatch stopwatch;

        public EyesExercise()
        {
       
            Window = new EyesExerciseWindow();

        }

        public void Show()
        {
            var scr = InputService.Current.TargetScreen.Bounds;

            Window.Show();
            if (moveUpdater == null)
            {
                moveUpdater = new Timer();
                moveUpdater.Interval = 10;
                moveUpdater.Tick += MoveUpdater_Tick;
            }
            moveUpdater.Start();
            stopwatch = new Stopwatch();

            var cursor = InputService.Current.Cursor;
            cursor.Window.Visibility = Visibility.Collapsed;
            GlobalKeyHook.Hook.KeyboardPressed += Hook_KeyboardPressed;
            IsShowed = true;
        }

        public void Close()
        {
            GlobalKeyHook.Hook.KeyboardPressed -= Hook_KeyboardPressed;

            Window.Hide();

            moveUpdater.Stop();
            IsShowed = false;
        }

        void Hook_KeyboardPressed(object sender, GlobalKeyHookEventArgs e)
        {
            if (e.VKeyCode == VKeyCode.F4)
            {
                if (GlobalKeyHook.IsKeyPressed(VKeyCode.LeftControl) && GlobalKeyHook.IsKeyPressed(VKeyCode.LeftAlt))
                {
                    Close();
                }
            }
        }

        void MoveUpdater_Tick(object sender, EventArgs e)
        {

            stopwatch.Start();

            Window.MainTitle.Text = stopwatch.Elapsed.Seconds.ToString();


        }
        public void Dispose()
        {
            if (IsShowed)
                Close();

            Window?.Close();
            Window = null;
        }

    }
}
