using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.workflow;

namespace labaphotoshop.furier
{
    internal static class FurierImages
    {
        public static Complex[,] ExtractChannel(Bitmap image, int channelOffset)
        {
            int width = image.Width;
            int height = image.Height;
            Complex[,] data = new Complex[height, width];

            BitmapData bmpData = image.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb
            );

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                Parallel.For(0, height, y =>
                {
                    byte* row = ptr + y * bmpData.Stride;
                    for (int x = 0; x < width; x++)
                    {
                        double factor = ((x + y) % 2 == 0) ? 1 : -1;
                        double value = row[x * 4 + channelOffset] * factor;
                        data[y, x] = new Complex(value, 0);
                    }
                });
            }
            image.UnlockBits(bmpData);
            return data;
        }
        public static Bitmap CombineChannels(Complex[,] red, Complex[,] green, Complex[,] blue)
        {
            int width = red.GetLength(1);
            int height = red.GetLength(0);
            Bitmap output = new Bitmap(width, height);

            BitmapData bmpData = output.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb
            );

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                Parallel.For(0, height, y =>
                {
                    byte* row = ptr + y * bmpData.Stride;
                    for (int x = 0; x < width; x++)
                    {
                        // Ограничение значений до [0, 255]
                        byte r = FurierMath.ClampByte(red[y, x].Real);
                        byte g = FurierMath.ClampByte(green[y, x].Real);
                        byte b = FurierMath.ClampByte(blue[y, x].Real);

                        row[x * 4] = b;      // Синий
                        row[x * 4 + 1] = g;  // Зеленый
                        row[x * 4 + 2] = r;  // Красный
                        row[x * 4 + 3] = 255; // Альфа
                    }
                });
            }
            output.UnlockBits(bmpData);
            return output;
        }

        public static Bitmap ReconstructImage(Complex[,] redFourier, Complex[,] greenFourier, Complex[,] blueFourier)
        {
            // Обратное ДПФ для каждого канала
            FurierMath.FFT2D(redFourier, false);
            FurierMath.FFT2D(greenFourier, false);
            FurierMath.FFT2D(blueFourier, false);

            // Объединение каналов в изображение
            return CombineChannels(redFourier, greenFourier, blueFourier);
        }

    }
}
