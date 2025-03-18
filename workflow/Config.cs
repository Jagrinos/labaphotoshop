using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace labaphotoshop.workflow
{
    internal class Config
    {
        internal static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        internal static readonly string DefImgPath = Path.Combine(BasePath, "images", "default.png");
        internal static readonly string CloseIcon = Path.Combine(BasePath, "images", "closeicon.png");
        internal static readonly string DownIcon = Path.Combine(BasePath, "images", "downicon.png");
        internal static readonly string UpIcon = Path.Combine(BasePath, "images", "upicon.png");
        internal const int HistogramHeight = 320;
    }
}
