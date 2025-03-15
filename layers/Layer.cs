
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace labaphotoshop.layers
{
    internal class Layer
    {
        public Layer(string filepath)
        {
            Image = new Bitmap(filepath);
            ModeOfMultiply = "Sum";
            Opacity = 1.0f; 
        }

        public Layer(string filepath, string? modeOfMultiply, float opacity)
        {
            Image = new Bitmap(filepath);
            ModeOfMultiply = modeOfMultiply;
            Opacity = opacity;
        }

        public Layer(Bitmap image, string? modeOfMultiply, float opacity)
        {
            Image = image;
            ModeOfMultiply = modeOfMultiply;
            Opacity = opacity;
        }

        public Bitmap Image { get; set; }
        public string? ModeOfMultiply { get; set; }
        public float Opacity { get; set; }
    }
}
