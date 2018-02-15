using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Vision;
using Vision.Windows;

namespace NeuralAction.WPF
{
    public class MouseAction
    {
        public bool IsScrolling { get; set; } = false;
        public bool IsDragging { get; set; } = false;

        CursorService service;

        public MouseAction(CursorService service)
        {
            this.service = service;
        }

        #region Scroll

        DispatcherTimer scrollTimer;
        ScrollCursorWindow scroll;
        public void ScrollStart(Point startPt)
        {
            if (!IsScrolling)
            {
                IsScrolling = true;

                if (scrollTimer == null)
                {
                    scrollTimer = new DispatcherTimer();
                    scrollTimer.Interval = TimeSpan.FromMilliseconds(500);
                    scrollTimer.Tick += delegate
                    {
                        service.Clicked += ScrollClicked;
                        scrollTimer.Stop();
                    };
                }
                scrollTimer.Start();

                scroll = new ScrollCursorWindow(startPt.ToPoint());
                scroll.Show();
            }
        }

        void ScrollClicked(object sender, Point e)
        {
            ScrollStop();
        }

        public void ScrollStop()
        {
            if (IsScrolling)
            {
                IsScrolling = false;
                service.Clicked -= DragClicked;
                
                scrollTimer.Stop();

                if (scroll != null)
                {
                    scroll.Dispatcher.Invoke(() =>
                    {
                        scroll.Close();
                        scroll = null;
                    });
                }
            }
        }

        #endregion Scroll

        #region Drag

        DispatcherTimer dragTimer;
        double dragPreSpeedLimit;
        bool dragPreUseSpeedLimit;
        bool dragPreUseClick;
        public void DragStart(Point startPt)
        {
            if (!IsDragging)
            {
                IsDragging = true;

                if (dragTimer == null)
                {
                    dragTimer = new DispatcherTimer();
                    dragTimer.Tick += delegate
                    {
                        if (IsDragging)
                        {
                            service.Clicked += DragClicked;
                        }
                        else
                        {
                            Settings.Current.CursorSpeedLimit = dragPreSpeedLimit;
                            Settings.Current.CursorUseSpeedLimit = dragPreUseSpeedLimit;
                            Settings.Current.AllowClick = dragPreUseClick;
                        }
                        dragTimer.Stop();
                    };
                    dragTimer.Interval = TimeSpan.FromMilliseconds(300);
                }
                dragTimer.Start();

                dragPreSpeedLimit = Settings.Current.CursorSpeedLimit;
                dragPreUseSpeedLimit = Settings.Current.CursorUseSpeedLimit;
                dragPreUseClick = Settings.Current.AllowClick;
                Settings.Current.CursorSpeedLimit = 3;
                Settings.Current.CursorUseSpeedLimit = true;
                Settings.Current.AllowClick = false;

                if (Settings.Current.AllowControl)
                    service.Window.Move(startPt.ToPoint());
                if(dragPreUseClick)
                    MouseEvent.Down(MouseButton.Left);
            }
        }

        void DragClicked(object sender, Point e)
        {
            DragStop();
        }

        public void DragStop()
        {
            if (IsDragging)
            {
                IsDragging = false;
                service.Clicked -= DragClicked;

                dragTimer.Stop();
                dragTimer.Start();

                if(dragPreUseClick)
                    MouseEvent.Up(MouseButton.Left);
            }
        }

        #endregion Drag

        #region Click

        public void RightClick(Point position)
        {
            var move = Settings.Current.AllowControl;
            var click = Settings.Current.AllowClick;
            var prePt = MouseEvent.GetCursorPosition();
            if(move)
                MouseEvent.MoveAt(position.ToPoint());
            if(click)
                MouseEvent.Click(MouseButton.Right);
            if(move)
                MouseEvent.MoveAt(prePt);
        }

        public void DoubleClick(Point position)
        {
            var move = Settings.Current.AllowControl;
            var click = Settings.Current.AllowClick;
            var prePt = MouseEvent.GetCursorPosition();
            if (move)
                MouseEvent.MoveAt(position.ToPoint());
            if (click)
            {
                MouseEvent.Click(MouseButton.Left);
                MouseEvent.Click(MouseButton.Left);
            }
            if(move)
                MouseEvent.MoveAt(prePt);
        }

        #endregion Click
    }
}
