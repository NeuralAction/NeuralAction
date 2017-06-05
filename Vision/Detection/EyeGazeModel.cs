using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision
{
    public class EyeGazeModel
    {
        public static bool IsModel(DirectoryNode node)
        {
            return node.GetFile("model.txt").IsExist;
        }

        public DirectoryNode Directory { get; set; }

        public string SessionName { get; set; }
        public string TimeStamp { get; set; }
        public Size ScreenSize { get; set; }
        public List<EyeGazeModelElement> Elements { get; set; } = new List<EyeGazeModelElement>();

        public EyeGazeModel()
        {

        }

        public EyeGazeModel(DirectoryNode node)
        {
            Load(node);
        }

        public void Load(DirectoryNode node)
        {
            if (!IsModel(node))
                throw new Exception("this directory is not eye gaze model");

            Directory = node;

            //read metadata
            string dirname = System.IO.Path.GetFileName(node.AbosolutePath);
            StringBuilder builder = new StringBuilder();
            bool time = true;
            foreach (char c in dirname)
            {
                builder.Append(c);
                if (c == ']' && time)
                {
                    TimeStamp = builder.ToString().Trim();
                    builder.Clear();
                    time = false;
                }
            }
            SessionName = builder.ToString().Trim();

            FileNode modelTXT = Directory.GetFile("model.txt");
            string[] lines = modelTXT.ReadLines();
            foreach(string line in lines)
            {
                if (line.StartsWith("scr:"))
                {
                    string[] spl = line.Split(':');
                    string[] screenSizes = spl[1].Split(',');
                    ScreenSize = new Size(Convert.ToDouble(screenSizes[0]), Convert.ToDouble(screenSizes[1]));
                }
            }

            //read model
            Elements.Clear();
            FileNode[] files = Directory.GetFiles();
            foreach(FileNode file in files)
            {
                EyeGazeModelElement ele = new EyeGazeModelElement(file);
                if (ele.Loaded)
                    Elements.Add(ele);
            }
        }
    }

    public class EyeGazeModelElement
    {
        public bool Loaded { get; set; } = false;
        public int Index { get; set; }
        public Point Point { get; set; }
        public FileNode File { get; set; }

        public EyeGazeModelElement(FileNode node)
        {
            File = node;
            string name = File.Name;
            if (name.EndsWith(".jpg"))
            {
                string[] spl = name.Split('.');
                string[] args = spl[0].Split(',');
                Index = Convert.ToInt32(args[0]);
                Point = new Point(Convert.ToDouble(args[1]), Convert.ToDouble(args[2]));
                Loaded = true;
            }
        }
    }
}
