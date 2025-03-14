using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace labaphotoshop
{
    internal class Image
    {
        public static void DrawWithModeByte(Bitmap formingImage, Bitmap workingImage, float opacity, string MODE)
        {
            int height = Math.Min(workingImage.Height, formingImage.Height);
            int width = Math.Min(workingImage.Width, formingImage.Width);

            BitmapData formingData = formingImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData workingData = workingImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* formingPtr = (byte*)formingData.Scan0;
                byte* workingPtr = (byte*)workingData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        //Находим начало текущего пикселя в массиве. Stride потому что строка может быть больше.
                        int formingIndex = y * formingData.Stride + x * 4;
                        int workingIndex = y * workingData.Stride + x * 4;

                        byte formingB = formingPtr[formingIndex];
                        byte formingG = formingPtr[formingIndex + 1];
                        byte formingR = formingPtr[formingIndex + 2];
                        //byte formingA = formingPtr[formingIndex + 3];

                        byte workingB = workingPtr[workingIndex];
                        byte workingG = workingPtr[workingIndex + 1];
                        byte workingR = workingPtr[workingIndex + 2];
                        //byte workingA = workingPtr[workingIndex + 3];
                        
                        switch (MODE)
                        {
                            case "Sum":
                                formingPtr[formingIndex] = ClampByte((int)(formingB * (1 - opacity) + workingB * opacity), 0, 255); // B
                                formingPtr[formingIndex + 1] = ClampByte((int)(formingG * (1 - opacity) + workingG * opacity), 0, 255); // G
                                formingPtr[formingIndex + 2] = ClampByte((int)(formingR * (1 - opacity) + workingR * opacity), 0, 255); // R
                                break;
                            case "Multiply":
                                formingPtr[formingIndex] = ClampByte((int)((formingB * workingB * opacity) % 255), 0, 255); // B
                                formingPtr[formingIndex + 1] = ClampByte((int)((formingG * workingG * opacity) % 255), 0, 255); // G
                                formingPtr[formingIndex + 2] = ClampByte((int)((formingR * workingR * opacity) % 255), 0, 255); // R
                                break;
                            case "Difference":
                                formingPtr[formingIndex] = ClampByte((int)(formingB - workingB * opacity), 0, 255); // B
                                formingPtr[formingIndex + 1] = ClampByte((int)(formingG - workingG * opacity), 0, 255); // G
                                formingPtr[formingIndex + 2] = ClampByte((int)(formingR - workingR * opacity), 0, 255); // R
                                break;
                            default:
                                break;
                        }
                        formingPtr[formingIndex + 3] = 255; // alpha
                    }
                }
            }

            // Разблокируем биты изображения
            formingImage.UnlockBits(formingData);
            workingImage.UnlockBits(workingData);
        }

        private static byte ClampByte(int val, int min, int max)
        {
            if (val < min) return (byte) min;
            else if (val > max) return (byte) max;
                 else return (byte) val;
        }


        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
        public static void DrawWithSum(Bitmap formingImage, Bitmap WorkingImage, float opacity)
        {
            int height = Math.Min(WorkingImage.Height, formingImage.Height);
            int width = Math.Min(WorkingImage.Width, formingImage.Width);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = WorkingImage.GetPixel(x, y);

                    if (y < formingImage.Height && x < formingImage.Width)
                    {
                        Color currentPixel = formingImage.GetPixel(x, y);

                        int r = Clamp((int)(currentPixel.R * (1 - opacity) + pixel.R * opacity), 0, 255);
                        int g = Clamp((int)(currentPixel.G * (1 - opacity) + pixel.G * opacity), 0, 255);
                        int b = Clamp((int)(currentPixel.B * (1 - opacity) + pixel.B * opacity), 0, 255);

                        formingImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
            }
        }

        public static void DrawWithDifference(Bitmap formingImage, Bitmap WorkingImage, float opacity)
        {
            int height = Math.Min(WorkingImage.Height, formingImage.Height);
            int width = Math.Min(WorkingImage.Width, formingImage.Width);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = WorkingImage.GetPixel(x, y);

                    if (y < formingImage.Height && x < formingImage.Width)
                    {
                        Color currentPixel = formingImage.GetPixel(x, y);

                        int r = Clamp((int)(currentPixel.R - pixel.R * opacity), 0, 255);
                        int g = Clamp((int)(currentPixel.G - pixel.G * opacity), 0, 255);
                        int b = Clamp((int)(currentPixel.B - pixel.B * opacity), 0, 255);

                        formingImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
            }
        }

        public static void DrawWithMultiply(Bitmap formingImage, Bitmap WorkingImage, float opacity)
        {
            int height = Math.Min(WorkingImage.Height, formingImage.Height);
            int width = Math.Min(WorkingImage.Width, formingImage.Width);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = WorkingImage.GetPixel(x, y);

                    if (y < formingImage.Height && x < formingImage.Width)
                    {
                        Color currentPixel = formingImage.GetPixel(x, y);

                        int r = Clamp((int)(currentPixel.R * pixel.R * opacity) % 255, 0, 255);
                        int g = Clamp((int)(currentPixel.G * pixel.G * opacity) % 255, 0, 255);
                        int b = Clamp((int)(currentPixel.B * pixel.B * opacity) % 255, 0, 255);

                        formingImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
            }
        }
    }
}
