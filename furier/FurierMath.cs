using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace labaphotoshop.furier
{
    internal static class FurierMath
    {
        public static void FFT1D(Complex[] data, bool forward = true)
        {
            int n = data.Length;
            Complex[] result = new Complex[n];
            double sign = forward ? -1.0 : 1.0;

            Parallel.For(0, n, u =>
            {
                Complex sum = Complex.Zero;
                for (int m = 0; m < n; m++)
                {
                    double angle = sign * 2 * Math.PI * u * m / n;
                    sum += data[m] * new Complex(Math.Cos(angle), Math.Sin(angle));
                }
                result[u] = forward ? sum / n : sum;
            });
            for (int i = 0; i < result.Length; i++) data[i] = result[i];
        }

        public static void FFT2D(Complex[,] data, bool forward)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);

            Complex[,] tmp = new Complex[height, width];

            // Обработка строк
            Parallel.For(0, height, y =>
            {
                Complex[] row = new Complex[width];
                for (int x = 0; x < width; x++) row[x] = data[y, x];
                FFT1D(row, forward);
                for (int x = 0; x < width; x++) tmp[y, x] = row[x];
            });

            // Обработка столбцов
            Parallel.For(0, width, x =>
            {
                Complex[] col = new Complex[height];
                for (int y = 0; y < height; y++) col[y] = tmp[y, x];
                FFT1D(col, forward);
                for (int y = 0; y < height; y++) data[y, x] = col[y];
            });

            if (!forward)
            {
                double scale = 1.0 / (width * height);
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        data[y, x] *= ((x + y) % 2 == 0) ? 1 : -1; // Центрирование
                    }
                });
            }
        }

        public static byte ClampByte(double value)
        {
            return (byte)Math.Clamp(value, 0, 255);
        }
    }
}
