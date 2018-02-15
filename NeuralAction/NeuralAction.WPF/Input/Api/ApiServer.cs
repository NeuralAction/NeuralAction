using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vision;
using Vision.Detection;

namespace NeuralAction.WPF
{
    public class ApiServer
    {
        public bool IsStarted { get; set; } = false;

        NamedPipeServerStream pipe;
        Task server;
        CancellationTokenSource cancel;
        Queue<string> message = new Queue<string>();
        bool isOpened;

        public ApiServer(InputService service)
        {
            var cursor = service.Cursor;
            cursor.Clicked += Cursor_Clicked;
            cursor.GazeTracked += Cursor_GazeTracked;
            cursor.Released += Cursor_Released;
        }

        public void Start()
        {
            Stop();

            pipe = new NamedPipeServerStream("GazeTracking");

            cancel = new CancellationTokenSource();
            var token = cancel.Token;
            server = new Task(() => { Proc(token); }, TaskCreationOptions.LongRunning);
            server.Start();

            IsStarted = true;
        }

        public void Stop()
        {
            if (IsStarted)
            {
                isOpened = false;
                IsStarted = false;
                cancel.Cancel();
                server.Wait();

                pipe.Dispose();
                pipe = null;
            }
        }

        void Proc(CancellationToken token)
        {
            pipe.WaitForConnectionAsync(token).Wait();

            var reader = new StreamReader(pipe);
            var writer = new StreamWriter(pipe);

            message.Clear();
            while (true)
            {
                isOpened = true;
                var line = reader.ReadLine();
                if (message.Count > 0)
                {
                    var msg = message.Dequeue();
                    writer.WriteLine(msg);
                    writer.Flush();
                }

                if (token.IsCancellationRequested)
                    break;

                Thread.Sleep(1);
            }

            reader.Dispose();
            writer.Dispose();
        }

        void AddMessage(string msg)
        {
            if (isOpened)
            {
                message.Enqueue(msg);
            }
        }

        void Cursor_Released(object sender, CursorReleasedArgs e)
        {
            AddMessage("released|" + ApiSerializer.Released(new ApiSerializer.CursorReleasedArgs(e.StartPosition, e.EndPosition, e.Duration)));
        }

        void Cursor_GazeTracked(object sender, GazeEventArgs e)
        {
            AddMessage("tracked|" + ApiSerializer.Tracked(new ApiSerializer.GazeEventArgs(e.Position, e.ScreenProperties, e.IsFaceDetected, e.IsGazeDetected)));
        }

        void Cursor_Clicked(object sender, Point e)
        {
            AddMessage("clicked|" + ApiSerializer.Clicked(e));
        }
    }
}
