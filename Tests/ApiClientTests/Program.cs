using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiClientTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new NeuralAction.WPF.ApiClient();
            client.Clicked += Client_Clicked;
            client.GazeTracked += Client_GazeTracked;
            client.Released += Client_Released;
            client.Start();
            client.Join();
        }

        private static void Client_GazeTracked(object sender, NeuralAction.WPF.ApiSerializer.GazeEventArgs e)
        {
            Console.WriteLine("Tracked " + e.Position);
        }

        private static void Client_Released(object sender, NeuralAction.WPF.ApiSerializer.CursorReleasedArgs e)
        {
            Console.WriteLine($"Released {e.StartPosition} {e.Duration}ms");
        }

        private static void Client_Clicked(object sender, Vision.Point e)
        {
            Console.WriteLine($"Clicked {e}");
        }
    }
}
