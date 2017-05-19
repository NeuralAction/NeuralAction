using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vision;
using Vision.Tests;

namespace WindowsTests
{
    class Program
    {
        static string[] HelpMessages = new string[]
        {
            "Help Informations ===",
            Core.ProjectInfromation,
            Core.VersionInfromation,
            "",
            "FACE \t Face detection example",
            "VIDEO \t Video playing example",
            "INFO \t Build Information of CV",
            "CLR \t Clear console",
            "EXIT \t Exit program"
        };

        [STAThread]
        static void Main(string[] args)
        {
            Core.Init(new Vision.Windows.WindowsCore());

            Program prg = new Program();
            prg.Run();
        }

        OpenFileDialog ofd = new OpenFileDialog() { Title = "Select File" };

        public void Run()
        {
            while (true)
            {
                Console.Write(">>> ");
                string read_raw = Console.ReadLine();
                string read = read_raw.ToLower();

                switch (read)
                {
                    case "info":
                        Console.WriteLine(Core.Cv.BuildInformation);
                        break;
                    case "clr":
                        Console.Clear();
                        break;
                    case "video":
                        SimpleVideo();
                        break;
                    case "face":
                        FaceDetection();
                        break;
                    case "help":
                        foreach (string line in HelpMessages)
                        {
                            Console.WriteLine(line);
                        }
                        break;
                    case "exit":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Unknown Command : \"{0}\"", read_raw);
                        break;
                }
            }
        }

        public void SimpleVideo()
        {
            if (DialogResult.OK == ofd.ShowDialog())
            {
                SimpleVideoPlayer player = new SimpleVideoPlayer(ofd.FileName);
                player.Run();
            }
        }

        public void FaceDetection()
        {
            if(DialogResult.OK == ofd.ShowDialog())
            {
                FaceDetection detect = new FaceDetection(ofd.FileName, 
                    Path.Combine(Environment.CurrentDirectory, "lbpcascade_frontalface_improved.xml"), 
                    Path.Combine(Environment.CurrentDirectory, "haarcascade_eye_tree_eyeglasses.xml"));
                detect.Run();
            }
        }
    }
}
