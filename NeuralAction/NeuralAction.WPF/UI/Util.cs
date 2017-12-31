using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralAction.WPF
{
    public static class Util
    {
        public static Stream GetResourceStream(string path)
        {
            return System.Windows.Application.GetResourceStream(new Uri(path, UriKind.Relative)).Stream;
        }
    }
}
