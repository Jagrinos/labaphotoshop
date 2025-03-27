using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace labaphotoshop.furier
{
    internal static class FurierProcessor
    {
        public static (Bitmap, Complex[,], Complex[,], Complex[,]) ApplyFilterAndVisualize(Bitmap inputImage, int r1, int r2, FilterMode mode, double brightness)
        {
            Complex[,] r = FurierImages.ExtractChannel(inputImage, 2);   // Красный канал (смещение +2)
            Complex[,] g = FurierImages.ExtractChannel(inputImage, 1); // Зеленый канал (смещение +1)
            Complex[,] b = FurierImages.ExtractChannel(inputImage, 0);   // Синий канал (смещение +0)

            // Прямое ДПФ для каждого канала
            FurierMath.FFT2D(r, true);
            FurierMath.FFT2D(g, true);
            FurierMath.FFT2D(b, true);

            Bitmap fv = mode != FilterMode.None ? FurierVisualize.VisualizeFourierWithRings(r, g, b, r1, r2, brightness) : FurierVisualize.VisualizeFourier(r, g, b, brightness);

            // Применяем фильтр ко всем каналам
            ApplyFilter(r, r1, r2, mode);
            ApplyFilter(g, r1, r2, mode);
            ApplyFilter(b, r1, r2, mode);

            // Визуализируем Фурье-образ с радиусами
            return (fv, r, g, b);
        }

        public static void ApplyFilter(Complex[,] data, int r1, int r2, FilterMode mode)
        {
            int width = data.GetLength(1);
            int height = data.GetLength(0);
            int centerX = width / 2;
            int centerY = height / 2;

            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    double dx = x - centerX;
                    double dy = y - centerY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    bool inZone = distance >= r1 && distance <= r2;

                    if ((mode == FilterMode.Reject && inZone) || (mode == FilterMode.BandPass && !inZone))
                    {
                        data[y, x] = Complex.Zero;
                    }
                }
            });
        }

        
    }
}
