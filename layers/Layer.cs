
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.workflow;

namespace labaphotoshop.layers
{
    internal class Layer
    {
        public Layer(string filepath)
        {
            Image = Funcs.BitmapChangeFormatTo32(new Bitmap(filepath));
            ModeOfMultiply = "Sum";
            Opacity = 1.0f; 
        }

        public Layer(string filepath, string? modeOfMultiply, float opacity)
        {
            Image = Funcs.BitmapChangeFormatTo32(new Bitmap(filepath));
            ModeOfMultiply = modeOfMultiply;
            Opacity = opacity;
        }

        public Layer(Bitmap image, string? modeOfMultiply, float opacity)
        {
            Image = Funcs.BitmapChangeFormatTo32(image);
            ModeOfMultiply = modeOfMultiply;
            Opacity = opacity;
        }

        public Bitmap Image { get; set; }
        public string? ModeOfMultiply { get; set; }
        public float Opacity { get; set; }
    }
}
