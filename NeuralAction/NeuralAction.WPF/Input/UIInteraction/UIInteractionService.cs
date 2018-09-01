using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using UIAccessibility;

namespace NeuralAction.WPF
{
    public class UIInteractionService : IDisposable
    {
        public AccessibleNotifyService Service { get; set; }

        double magnetTempFactor;
        bool magnet = false;
        public bool Magnet
        {
            get => magnet;
            set
            {
                if (magnet != value)
                {
                    magnet = value;
                    if (magnet)
                    {
                        magnetTempFactor = InputService.Current.Cursor.Window.SmoothFactor;
                        InputService.Current.Cursor.Window.SmoothFactor = magnetTempFactor * 6;
                    }
                    else
                    {
                        InputService.Current.Cursor.Window.SmoothFactor = magnetTempFactor;
                    }
                }
            }
        }

        bool serviceStart = false;
        public bool ServiceStart
        {
            get => serviceStart;
            set
            {
                if (value != serviceStart)
                {
                    serviceStart = value;
                    if(value)
                        Start();
                    else 
                        Stop();
                }
            }
        }

        Highlighter highlighter;
        AccessibleTrackedArgs clickedArg;
        AccessibleTrackedArgs targetArg;

        public UIInteractionService()
        {
            Service = new AccessibleNotifyService();
            Service.Tracked += Service_Tracked;

            highlighter = new Highlighter();
        }

        public void Start()
        {
            serviceStart = true;
            highlighter.Show();
            Service.Start();
            InputService.Current.Cursor.Clicked += Cursor_Clicked;
            InputService.Current.Cursor.Released += Cursor_Released;
        }

        public void Stop()
        {
            serviceStart = false;
            highlighter.Hide();
            Service.Stop();
            InputService.Current.Cursor.Clicked -= Cursor_Clicked;
            InputService.Current.Cursor.Released -= Cursor_Released;
            Magnet = false;
        }

        void Cursor_Clicked(object sender, Vision.Point e)
        {
            clickedArg = targetArg;
        }

        void Cursor_Released(object sender, CursorReleasedArgs e)
        {
            if (e.Duration < Settings.Current.CursorOpenMenuWaitDuration)
            {
                var element = clickedArg.Element;
                if (element != null)
                {
                    var type = element.Type;
                    var position = new Vision.Point(clickedArg.Position.X, clickedArg.Position.Y);

                    switch (type)
                    {
                        case UIElementType.ScrollViewer:
                            e.BlockClick = true;
                            highlighter.Dispatcher.Invoke(() =>
                            {
                                var action = InputService.Current.Cursor.Action;
                                if (action.IsScrolling)
                                {
                                    action.ScrollStop();
                                }
                                else
                                {
                                    action.ScrollStart(position);
                                }
                            });
                            break;
                        case UIElementType.TextBox:
                            e.BlockClick = true;
                            WinApi.SetForegroundWindow(element.Hwnd);
                            Send.FocusedHandle = element.Hwnd;

                            highlighter.Dispatcher.Invoke(() =>
                            {
                                InputService.Current.ShowKeyboard();
                            });
                            break;
                        case UIElementType.TitleBar:
                            e.BlockClick = true;
                            highlighter.Dispatcher.Invoke(() =>
                            {
                                var action = InputService.Current.Cursor.Action;
                                if (action.IsDragging)
                                {
                                    action.DragStop();
                                }
                                else
                                {
                                    action.DragStart(position);
                                }
                            });
                            break;
                        case UIElementType.Chrome:
                        case UIElementType.Button:
                        case UIElementType.None:
                        default:
                            break;
                    }
                }
            }
            clickedArg = null;
        }

        void Service_Tracked(object sender, AccessibleTrackedArgs e)
        {
            targetArg = e;
            if(serviceStart)
                Highlight(targetArg.Element, targetArg);
        }

        void Highlight(Accessible acc, AccessibleTrackedArgs arg)
        {
            highlighter.Dispatcher.Invoke(() =>
            {
                if (acc != null && acc.Type != UIElementType.None)
                {
                    var location = acc.Location;
                    var rect = new System.Windows.Rect(location.X, location.Y, location.Width, location.Height);
                    switch (acc.Type)
                    {
                        case UIElementType.TextBox:
                        case UIElementType.Button:
                        case UIElementType.TitleBar:
                            Magnet = true;
                            break;
                        case UIElementType.ScrollViewer:
                        case UIElementType.Chrome:
                        default:
                            Magnet = false;
                            break;
                    }

                    highlighter.On = true;
                    highlighter.Highlight(rect);
                }
                else
                {
                    Magnet = false;
                    highlighter.On = false;
                }
            });
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
