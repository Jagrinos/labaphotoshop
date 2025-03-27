using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace labaphotoshop.furier
{
    internal static class FurierVisualize
    {
        public static Bitmap VisualizeFourierWithRings(Complex[,] red, Complex[,] green, Complex[,] blue, int r1, int r2, double brightness)
        {
            Bitmap fourierImage = VisualizeFourier(red, green, blue, brightness);
            using (Graphics g = Graphics.FromImage(fourierImage))
            {
                int centerX = fourierImage.Width / 2;
                int centerY = fourierImage.Height / 2;

                // Рисуем кольца
                Pen pen = new(Color.Red, 2);
                g.DrawEllipse(pen, centerX - r1, centerY - r1, r1 * 2, r1 * 2);
                g.DrawEllipse(pen, centerX - r2, centerY - r2, r2 * 2, r2 * 2);
            }
            return fourierImage;
        }

        public static Bitmap VisualizeFourier(Complex[,] red, Complex[,] green, Complex[,] blue, double brightness)
        {
            int width = red.GetLength(1);
            int height = red.GetLength(0);
            Bitmap output = new(width, height, PixelFormat.Format32bppArgb);

            // Вычисление модулей и логарифмическое масштабирование
            (double[,] rMag, double rMax) = ComputeMagnitudes(red);
            (double[,] gMag, double gMax) = ComputeMagnitudes(green);
            (double[,] bMag, double bMax) = ComputeMagnitudes(blue);

            // Создание изображения
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
                        // Нормализация для каждого канала
                        byte r = FurierMath.ClampByte(ScaleToByte(rMag[y, x], rMax) * brightness);
                        byte g = FurierMath.ClampByte(ScaleToByte(gMag[y, x], gMax) * brightness);
                        byte b = FurierMath.ClampByte(ScaleToByte(bMag[y, x], bMax) * brightness);

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
        private static byte ScaleToByte(double value, double max)
        {
            return (byte)(255 * value / max);
        }

        public static (double[,], double) ComputeMagnitudes(Complex[,] data)
        {
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            double[,] magnitudes = new double[height, width];
            double max = 0;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    magnitudes[y, x] = Math.Log(1 + Complex.Abs(data[y, x]));
                    if (magnitudes[y, x] > max) max = magnitudes[y, x];
                }
            return (magnitudes, max);
        }
    }
}
