using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.workflow;

namespace labaphotoshop.gradation_transformations
{
    internal class Histogram
    {
        private FlowLayoutPanel _flowPanelImages;
        private PictureBox _mainPicture;

        private PictureBox _histogramImg;

        public Histogram(FlowLayoutPanel flowPanelImages, PictureBox MainPicture, PictureBox histogramImg)
        {
            _flowPanelImages = flowPanelImages;
            _mainPicture = MainPicture;
            _histogramImg = histogramImg;

            UpdateHistogram(); 
        }

        public void UpdateHistogram()
        {
            if (_mainPicture.Image == null)
                return;

            var h = CalculateHistogram(Funcs.BitmapChangeFormatTo32(new Bitmap(_mainPicture.Image)));
            _histogramImg.Image = DrawHistogram(h, _flowPanelImages.Width - 20, Config.HistogramHeight);
        }

        private int[] CalculateHistogram(Bitmap img)
        {
            int[] histogram = new int[256];

            int height = img.Height;
            int width = img.Width;

            BitmapData data = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        int workingIndex = y * data.Stride + x * 4;

                        byte B = ptr[workingIndex];
                        byte G = ptr[workingIndex + 1];
                        byte R = ptr[workingIndex + 2];

                        int brigtness = (R+ G + B) / 3;

                        histogram[brigtness]++;
                    }
            }
            return histogram;
        }

        private Bitmap DrawHistogram(int[] histogram, int width, int height)
        {
            Bitmap histogramImg = new(width, height, PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(histogramImg);
            g.Clear(Color.White);

            float scale = (float)height / histogram.Max();
            float step = (float)width / (float)260.0;

            for (int i = 0; i < 256; i++)
            {
                int nowheight = (int)(histogram[i] * scale);
                g.DrawLine(Pens.Black, (i + 1) * step, height - 1, (i + 1) * step, height - 1 - nowheight);
            }
            return histogramImg;
        }
    }
}
