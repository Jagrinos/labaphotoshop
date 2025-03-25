using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace labaphotoshop.Space_filter
{
    internal class SfFuncs
    {
        public static Bitmap ApplyLinearFilter(Bitmap source, double[,] matrix)
        {
            int width = source.Width;
            int height = source.Height;
            int mWidth = matrix.GetLength(1);
            int mHeight = matrix.GetLength(0);
            int mCenterX = mWidth / 2;
            int mCenterY = mHeight / 2;

            Bitmap res = new(width, height, PixelFormat.Format32bppArgb);

            var srcData = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var resData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* resPtr = (byte*)resData.Scan0;

                Parallel.For(0, height, y => 
                {
                    for (int x = 0; x < width; x++) 
                    {
                        double sumB = 0, sumG = 0, sumR = 0;

                        for (int winY = -mHeight / 2; winY <= mHeight / 2; winY++)
                            for (int winX = -mWidth / 2; winX <= mWidth / 2; winX++)
                            {
                                int nX = x + winX >= 0 && x + winX < width ? x + winX : x - winX;
                                int nY = y + winY >= 0 && y + winY < height ? y + winY : y - winY;
                                
                                var srcIndex = nY * srcData.Stride + nX * 4;
                                var mxel = matrix[winY + mHeight / 2, winX + mWidth / 2];

                                sumB += srcPtr[srcIndex] * mxel;
                                sumG += srcPtr[srcIndex + 1] * mxel;
                                sumR += srcPtr[srcIndex + 2] * mxel;
                            }

                        var resIndex = y * resData.Stride + x * 4;
                        resPtr[resIndex] =     (byte)Math.Clamp(sumB, 0, 255);
                        resPtr[resIndex + 1] = (byte)Math.Clamp(sumG, 0, 255);
                        resPtr[resIndex + 2] = (byte)Math.Clamp(sumR, 0, 255);
                        resPtr[resIndex + 3] = 255;
                    }
                });

            }

            source.UnlockBits(srcData);
            res.UnlockBits(resData);


            return res;
        }

        public static Bitmap ApplyMedianFilter(Bitmap source, int mHeight, int mWidth)
        {
            int width = source.Width;
            int height = source.Height;
           
            int mSize = mWidth * mHeight;
           
            Bitmap res = new(width, height, PixelFormat.Format32bppArgb);
            if (mWidth > width || mHeight > height)
            {
                res.Dispose();
                res = new Bitmap(source);
                return res;
            }

            var srcData = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var resData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* resPtr = (byte*)resData.Scan0;
               
                Parallel.For(0, height, y =>
                {
                    byte[] neighborhoodR = new byte[mSize];
                    byte[] neighborhoodG = new byte[mSize];
                    byte[] neighborhoodB = new byte[mSize];
                    for (int x = 0; x < width; x++)
                    {
                        int index = 0;

                        for (int winY = -mHeight / 2; winY <= mHeight / 2; winY++)
                            for (int winX = -mWidth / 2; winX <= mWidth / 2; winX++)
                            {
                                int nX = x + winX >= 0 && x + winX < width ? x + winX : x - winX;
                                int nY = y + winY >= 0 && y + winY < height ? y + winY : y - winY;

                                var srcIndex = nY * srcData.Stride + nX * 4;

                                neighborhoodB[index] = srcPtr[srcIndex];
                                neighborhoodG[index] = srcPtr[srcIndex + 1];
                                neighborhoodR[index] = srcPtr[srcIndex + 2];
                                index++;
                            }

                        Task[] tasks =
                        [
                            Task.Run(() => Array.Sort(neighborhoodR)),
                            Task.Run(() => Array.Sort(neighborhoodG)),
                            Task.Run(() => Array.Sort(neighborhoodB)),
                        ];
                        Task.WaitAll(tasks);

                        var resIndex = y * resData.Stride + x * 4;
                        resPtr[resIndex] = neighborhoodB[mSize/2];
                        resPtr[resIndex + 1] = neighborhoodG[mSize / 2];
                        resPtr[resIndex + 2] = neighborhoodR[mSize / 2];
                        resPtr[resIndex + 3] = 255;
                    }
                }
                );

            }

            source.UnlockBits(srcData);
            res.UnlockBits(resData);


            return res;
        }

        public static double[,] GenerateGaussianMatrix(int size, double sigma, out double sum)
        {
            double[,] kernel = new double[size, size];
            double sigSqr2 = sigma * sigma * 2;
            double sigSqrPi = sigSqr2 * Math.PI;
            int r = size / 2;
            sum = 0;

            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                {
                    double value = Math.Exp(-(x * x + y * y) / sigSqr2) / sigSqrPi;
                    kernel[y + r, x + r] = value;
                    sum += value;
                }

            return kernel;
        }
    }
}
