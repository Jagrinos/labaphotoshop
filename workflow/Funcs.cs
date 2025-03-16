using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace labaphotoshop.workflow
{
    internal class Funcs
    {
        public static Bitmap BitmapChangeFormatTo32(Bitmap originalImage)
        {
            var newImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.DrawImage(originalImage, 0, 0, originalImage.Width, originalImage.Height);
            }
            return newImage;
        }
    }
}
