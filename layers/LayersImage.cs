using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace labaphotoshop.layers
{
    internal class LayersImage
    {
        public static void DrawWithModeByte(Bitmap formingImage, Layer layer)
        {
            int height = Math.Min(layer.Image.Height, formingImage.Height);
            int width = Math.Min(layer.Image.Width, formingImage.Width);

            BitmapData formingData = formingImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData workingData = layer.Image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

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
                        
                        switch (layer.ModeOfMultiply)
                        {
                            case "Sum":
                                formingPtr[formingIndex] = ClampByte((int)(formingB * (1 - layer.Opacity) + workingB * layer.Opacity), 0, 255); // B
                                formingPtr[formingIndex + 1] = ClampByte((int)(formingG * (1 - layer.Opacity) + workingG * layer.Opacity), 0, 255); // G
                                formingPtr[formingIndex + 2] = ClampByte((int)(formingR * (1 - layer.Opacity) + workingR * layer.Opacity), 0, 255); // R
                                break;
                            case "Multiply":
                                formingPtr[formingIndex] = ClampByte((int)(formingB * workingB * layer.Opacity % 255), 0, 255); // B
                                formingPtr[formingIndex + 1] = ClampByte((int)(formingG * workingG * layer.Opacity % 255), 0, 255); // G
                                formingPtr[formingIndex + 2] = ClampByte((int)(formingR * workingR * layer.Opacity % 255), 0, 255); // R
                                break;
                            case "Difference":
                                formingPtr[formingIndex] = ClampByte((int)(formingB - workingB * layer.Opacity), 0, 255); // B
                                formingPtr[formingIndex + 1] = ClampByte((int)(formingG - workingG * layer.Opacity), 0, 255); // G
                                formingPtr[formingIndex + 2] = ClampByte((int)(formingR - workingR * layer.Opacity), 0, 255); // R
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
            layer.Image.UnlockBits(workingData);
        }

        private static byte ClampByte(int val, int min, int max)
        {
            if (val < min) return (byte)min;
            else if (val > max) return (byte)max;
            else return (byte)val;
        }

    }
}
