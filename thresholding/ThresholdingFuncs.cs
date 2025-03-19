using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.workflow;

namespace labaphotoshop.thresholding
{
    internal class ThresholdingFuncs
    {
        private Label _infoText;
        private PictureBox _mainPicture;
        private Bitmap? _originalImg;
        private ThresholdingForm _thresholdingForm;

        ~ThresholdingFuncs() 
        {
            _originalImg?.Dispose();
        }

        public ThresholdingFuncs(ThresholdingForm thresholdingForm, Label infoText, PictureBox mainPicture)
        {
            _thresholdingForm = thresholdingForm;
            _infoText = infoText;
            _mainPicture = mainPicture;
            if (mainPicture.Image != null) 
            {
                _originalImg = Funcs.BitmapChangeFormatTo32(new Bitmap(mainPicture.Image));
            }
            foreach (RadioButton button in _thresholdingForm.Buttons) 
                button.CheckedChanged += BinModeChange!;
        }

        private void BinModes(string MODE)
        {
            if (_originalImg == null)
            {
                MODE = "NULLIMAGE";
            }
            Bitmap img;
            switch (MODE)
            {
                case "Градации серого":
                    img = Grayscale(_originalImg!);
                    _mainPicture.Image!.Dispose();
                    _mainPicture.Image = img;
                    break;
                case "Критерий Гаврилова":
                    img = GavrilovThresholding(_originalImg!);
                    _mainPicture.Image!.Dispose();
                    _mainPicture.Image = img;
                    break;
                case "Критерий Отсо":
                    img = OtsuThresholding(_originalImg!);
                    _mainPicture.Image!.Dispose();
                    _mainPicture.Image = img;
                    break;
                case "Критерий Ниблека":
                    img = NiblackThresholding(_originalImg!, _thresholdingForm.WindowSize, _thresholdingForm.K);
                    _mainPicture.Image!.Dispose();
                    _mainPicture.Image = img;
                    break;
                case "Критерий Сауволы":
                    img = SauvolaThresholding(_originalImg!, _thresholdingForm.WindowSize, _thresholdingForm.K);
                    _mainPicture.Image!.Dispose();
                    _mainPicture.Image = img;
                    break;
                case "Критерий Кристиана Вульфа":
                    img = WolfThresholding(_originalImg!, _thresholdingForm.WindowSize, _thresholdingForm.A);
                    _mainPicture.Image!.Dispose();
                    _mainPicture.Image = img;
                    break;
                //case "Критерий Брэдли-Рота":
                //    img = BradleyRothThresholding(_originalImg!, 3, 0.2);
                //    _mainPicture.Image!.Dispose();
                //    _mainPicture.Image = img;
                //    break;
                default:
                    break;
            }
        }
        private void BinModeChange(object sender, EventArgs e)
        {
            if (sender is RadioButton button && button.Checked)
            {
                var MODE = button.Text;
                _infoText.Text = $"{MODE}, WS: {_thresholdingForm.WindowSize}, k: {_thresholdingForm.K}, a {_thresholdingForm.A}";
                BinModes(MODE);
            }
        }

        private Bitmap Grayscale(Bitmap img)
        {
            int width = img.Width;
            int height = img.Height;
            Bitmap res = new(width, height, PixelFormat.Format32bppArgb);

            BitmapData srcData = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* resPtr = (byte*)resData.Scan0;

                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        var srcIndex = y * srcData.Stride + x * 4;
                        var resIndex = y * resData.Stride + x * 4;

                        byte b = srcPtr[srcIndex];
                        byte g = srcPtr[srcIndex + 1];
                        byte r = srcPtr[srcIndex + 2];
                        byte gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                        resPtr[resIndex] = gray;
                        resPtr[resIndex + 1] = gray;
                        resPtr[resIndex + 2] = gray;
                        resPtr[resIndex + 3] = 255;
                    }
                });
            }

            img.UnlockBits(srcData);
            res.UnlockBits(resData);

            return res;
        }

        private Bitmap GavrilovThresholding(Bitmap img)
        {

            int width = img.Width;
            int height = img.Height;
            Bitmap res = new(width, height, PixelFormat.Format32bppArgb);

            BitmapData srcData = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            //threshold calculation
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* resPtr = (byte*)resData.Scan0;

                long sum = 0;
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        var srcIndex = y * srcData.Stride + x * 4;

                        byte b = srcPtr[srcIndex];
                        byte g = srcPtr[srcIndex + 1];
                        byte r = srcPtr[srcIndex + 2];
                        byte gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                        Interlocked.Add(ref sum, gray); //Самый эффективный способ синхронизации, так как ваще нет блокировки, а атомарность гарантирована
                    }
                });

                byte threshold = (byte)(sum / (width * height));
                //overlay
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        var srcIndex = y * srcData.Stride + x * 4;
                        var resIndex = y * resData.Stride + x * 4;

                        byte b = srcPtr[srcIndex];
                        byte g = srcPtr[srcIndex + 1];
                        byte r = srcPtr[srcIndex + 2];
                        byte gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                        byte bin = gray <= threshold ? (byte)0 : (byte)255;
                        resPtr[resIndex] = resPtr[resIndex + 1] = resPtr[resIndex + 2] = bin;
                        resPtr[resIndex + 3] = 255;
                    }
                });
            }

            img.UnlockBits(srcData);
            res.UnlockBits(resData);

            return res;
        }

        private Bitmap OtsuThresholding(Bitmap img)
        {
            int width = img.Width;
            int height = img.Height;
            int sizeImg = width * height;

            Bitmap res = new(width, height, PixelFormat.Format32bppArgb);

            BitmapData srcData = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            //calculation histogram
            int[] histogram = new int[256];
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;

                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        var srcIndex = y * srcData.Stride + x * 4;

                        byte b = srcPtr[srcIndex];
                        byte g = srcPtr[srcIndex + 1];
                        byte r = srcPtr[srcIndex + 2];

                        byte gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);
                        histogram[gray]++;
                    }
                });
            }
            //normalization
            //for (int winX = 0; winX < 256; winX++)
            //{
            //    histogram[winX] /= sizeImg;
            //}

            var uT = 0;
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] == histogram.Max())
                    break;

                uT += i * histogram[i];
            }

            //calculation threshold
            //var delta2 = -int.MaxValue;
            //var threshold = 0;
            //int w1, w2, u1, u2, delta2new;

            //for (int t = 0; t < 256; t++)
            //{
            //    if (histogram[t] == histogram.Max())
            //        break;

            //    //if (t == 0 || t == 1)
            //    //{
            //    //    w1 = histogram[t];
            //    //    w2 = 1 - w1;

            //    //    u1 = 1;
            //    //    u2 = (uT - w1) / w2;

            //    //    delta2new = w1 * w2 * (int)Math.Pow(u1 - u2, 2);
            //    //    if (delta2new > delta2)
            //    //    {
            //    //        delta2 = delta2new;
            //    //        threshold = t;
            //    //    }
            //    //    continue;
            //    //}

            //    w1 = 0;
            //    for (int winX = 0; winX < t-1; winX++)
            //    {
            //        w1 += histogram[winX];
            //    }
            //    if (w1 == 0)
            //        continue;

            //    w2 = sizeImg - w1;
            //    if (w2 == 0)
            //        break;

            //    var iNi = 0;
            //    for (int winX = 0; winX < t - 1; winX++)
            //    {
            //        iNi += winX * histogram[winX];
            //    }

            //    u1 = iNi / w1;
            //    u2 = (uT - u1 * w1) / w2;

            //    delta2new = w1 * w2 * (int)Math.Pow(u1 - u2, 2);
            //    if (delta2new > delta2)
            //    {
            //        delta2 = delta2new;
            //        threshold = t;
            //    }
            //}

            float sum = 0;
            for (int i = 0; i < 256; i++)
                sum += i * histogram[i];

            float sumB = 0, wB = 0, wF = 0;
            float maxVariance = 0;
            int threshold = 0;

            for (int t = 0; t < 256; t++)
            {
                wB += histogram[t];
                if (wB == 0) continue;

                wF = sizeImg - wB;
                if (wF == 0) break;

                sumB += t * histogram[t];
                float mB = sumB / wB;
                float mF = (sum - sumB) / wF;

                float variance = wB * wF * (mB - mF) * (mB - mF);
                if (variance > maxVariance)
                {
                    maxVariance = variance;
                    threshold = t;
                }
            }


            //overlay
            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* resPtr = (byte*)resData.Scan0;

                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        var srcIndex = y * srcData.Stride + x * 4;
                        var resIndex = y * resData.Stride + x * 4;

                        byte b = srcPtr[srcIndex];
                        byte g = srcPtr[srcIndex + 1];
                        byte r = srcPtr[srcIndex + 2];
                        byte gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                        byte bin = gray <= threshold ? (byte)0 : (byte)255;
                        resPtr[resIndex] = resPtr[resIndex + 1] = resPtr[resIndex + 2] = bin;
                        resPtr[resIndex + 3] = 255;
                    }
                });
            }

            img.UnlockBits(srcData);
            res.UnlockBits(resData);
            return res;
        }

        private Bitmap NiblackThresholding(Bitmap img, int windowSize, double k)
        {
            if (windowSize % 2 == 0)
            {
                return img;
            }

            int width = img.Width;
            int height = img.Height;
            int sizeImg = width * height;

            Bitmap res = new(width, height, PixelFormat.Format32bppArgb);

            BitmapData srcData = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* resPtr = (byte*)resData.Scan0;

                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIndex, resIndex;
                        byte b, g, r, gray;

                        //calculate M(x) and M(x^2)
                        int winSum = 0;
                        int winSumSquare = 0;
                        int count = 0;

                        for (int winY = -windowSize / 2; winY < windowSize / 2; winY++)
                        {
                            for (int winX = -windowSize / 2; winX < windowSize / 2; winX++)
                            {
                                int nX = x + winX, nY = y + winY;
                                if (nX >= 0 && nY >= 0 && nX < width && nY < height)
                                {
                                    srcIndex = nY * srcData.Stride + nX * 4;
                                    b = srcPtr[srcIndex];
                                    g = srcPtr[srcIndex + 1];
                                    r = srcPtr[srcIndex + 2];

                                    gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                                    winSum += gray;
                                    winSumSquare += gray * gray;
                                    count++;
                                }
                            }
                        }
                        //calculate threshold
                        double MX = winSum / count;
                        double DX = winSumSquare / count - Math.Pow(MX, 2);

                        double delta = Math.Sqrt(DX);
                        double threshold = MX + k * delta;

                        //layout
                        resIndex = y * resData.Stride + x * 4;
                        srcIndex = y * srcData.Stride + x * 4;

                        b = srcPtr[srcIndex];
                        g = srcPtr[srcIndex + 1];
                        r = srcPtr[srcIndex + 2];

                        gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                        byte bin = gray < threshold ? (byte)0 : (byte)255;

                        resPtr[resIndex] = resPtr[resIndex + 1] = resPtr[resIndex + 2] = bin;
                        resPtr[resIndex + 3] = 255;
                    }
                });
            }


            img.UnlockBits(srcData);
            res.UnlockBits(resData);
            return res;
        }

        private Bitmap SauvolaThresholding(Bitmap img, int windowSize, double k)
        {
            if (windowSize % 2 == 0)
            {
                return img;
            }

            int width = img.Width;
            int height = img.Height;
            int sizeImg = width * height;

            Bitmap res = new(width, height, PixelFormat.Format32bppArgb);

            BitmapData srcData = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* resPtr = (byte*)resData.Scan0;

                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIndex, resIndex;
                        byte b, g, r, gray;

                        //calculate M(x) and M(x^2)
                        int winSum = 0;
                        int winSumSquare = 0;
                        int count = 0;

                        for (int winY = -windowSize / 2; winY < windowSize / 2; winY++)
                        {
                            for (int winX = -windowSize / 2; winX < windowSize / 2; winX++)
                            {
                                int nX = x + winX, nY = y + winY;
                                if (nX >= 0 && nY >= 0 && nX < width && nY < height)
                                {
                                    srcIndex = nY * srcData.Stride + nX * 4;
                                    b = srcPtr[srcIndex];
                                    g = srcPtr[srcIndex + 1];
                                    r = srcPtr[srcIndex + 2];

                                    gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                                    winSum += gray;
                                    winSumSquare += gray * gray;
                                    count++;
                                }
                            }
                        }
                        //calculate threshold
                        double MX = winSum / count;
                        double DX = winSumSquare / count - Math.Pow(MX, 2);

                        double delta = Math.Sqrt(DX);
                        double threshold = MX * (1 + k * (delta / 128 - 1));

                        //layout
                        resIndex = y * resData.Stride + x * 4;
                        srcIndex = y * srcData.Stride + x * 4;

                        b = srcPtr[srcIndex];
                        g = srcPtr[srcIndex + 1];
                        r = srcPtr[srcIndex + 2];

                        gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                        byte bin = gray < threshold ? (byte)0 : (byte)255;

                        resPtr[resIndex] = resPtr[resIndex + 1] = resPtr[resIndex + 2] = bin;
                        resPtr[resIndex + 3] = 255;
                    }
                });
            }


            img.UnlockBits(srcData);
            res.UnlockBits(resData);
            return res;
        }

        private Bitmap WolfThresholding(Bitmap img, int windowSize, double a)
        {
            if (windowSize % 2 == 0)
            {
                return img;
            }

            int width = img.Width;
            int height = img.Height;
            int sizeImg = width * height;

            Bitmap res = new(width, height, PixelFormat.Format32bppArgb);

            BitmapData srcData = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            byte minI = 255;
            double maxDelta = -double.MinValue;

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* resPtr = (byte*)resData.Scan0;

                //searh maxDelta and minI
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIndex, resIndex;
                        byte b, g, r, gray;

                        //calculate M(x) and M(x^2)
                        int winSum = 0;
                        int winSumSquare = 0;
                        int count = 0;

                        for (int winY = -windowSize / 2; winY < windowSize / 2; winY++)
                        {
                            for (int winX = -windowSize / 2; winX < windowSize / 2; winX++)
                            {
                                int nX = x + winX, nY = y + winY;
                                if (nX >= 0 && nY >= 0 && nX < width && nY < height)
                                {
                                    srcIndex = nY * srcData.Stride + nX * 4;
                                    b = srcPtr[srcIndex];
                                    g = srcPtr[srcIndex + 1];
                                    r = srcPtr[srcIndex + 2];

                                    gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                                    winSum += gray;
                                    winSumSquare += gray * gray;
                                    count++;
                                }
                            }
                        }
                        //calculate threshold
                        double MX = winSum / count;
                        double DX = winSumSquare / count - Math.Pow(MX, 2);

                        double delta = Math.Sqrt(DX);

                        srcIndex = y * srcData.Stride + x * 4;
                        b = srcPtr[srcIndex];
                        g = srcPtr[srcIndex + 1];
                        r = srcPtr[srcIndex + 2];
                        gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);
                        lock (this)
                        {
                            minI = Math.Min(minI, gray);
                            maxDelta = Math.Max(delta, maxDelta);
                        }
                    }
                });

                //calc + layout
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcIndex, resIndex;
                        byte b, g, r, gray;

                        //calculate M(x) and M(x^2)
                        int winSum = 0;
                        int winSumSquare = 0;
                        int count = 0;

                        for (int winY = -windowSize / 2; winY < windowSize / 2; winY++)
                        {
                            for (int winX = -windowSize / 2; winX < windowSize / 2; winX++)
                            {
                                int nX = x + winX, nY = y + winY;
                                if (nX >= 0 && nY >= 0 && nX < width && nY < height)
                                {
                                    srcIndex = nY * srcData.Stride + nX * 4;
                                    b = srcPtr[srcIndex];
                                    g = srcPtr[srcIndex + 1];
                                    r = srcPtr[srcIndex + 2];

                                    gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                                    winSum += gray;
                                    winSumSquare += gray * gray;
                                    count++;
                                }
                            }
                        }
                        //calculate threshold
                        double MX = winSum / count;
                        double DX = winSumSquare / count - Math.Pow(MX, 2);

                        double delta = Math.Sqrt(DX);

                        double threshold = (1 - a) * MX + a * minI + a * delta / maxDelta * (MX - minI);

                        //layout
                        resIndex = y * resData.Stride + x * 4;
                        srcIndex = y * srcData.Stride + x * 4;

                        b = srcPtr[srcIndex];
                        g = srcPtr[srcIndex + 1];
                        r = srcPtr[srcIndex + 2];

                        gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                        byte bin = gray < threshold ? (byte)0 : (byte)255;

                        resPtr[resIndex] = resPtr[resIndex + 1] = resPtr[resIndex + 2] = bin;
                        resPtr[resIndex + 3] = 255;
                    }
                });
            }


            img.UnlockBits(srcData);
            res.UnlockBits(resData);
            return res;
        }

        //public static Bitmap BradleyRothThresholding(Bitmap source, int windowSize, double k)
        //{
        //    int width = source.Width;
        //    int height = source.Height;
        //    Bitmap result = new(width, height, PixelFormat.Format32bppArgb);

        //    BitmapData srcData = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        //    BitmapData resData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        //    int bytesPerPixel = 4;
        //    int stride = srcData.Stride;
        //    int[,] integralImage = new int[width, height];

        //    unsafe
        //    {
        //        byte* srcPtr = (byte*)srcData.Scan0;

        //        Parallel.For(0, height, y =>
        //        {
        //            int sum = 0;
        //            for (int x = 0; x < width; x++)
        //            {
        //                byte* pixel = srcPtr + y * stride + x * bytesPerPixel;
        //                byte gray = (byte)(0.2125 * pixel[2] + 0.7154 * pixel[1] + 0.0721 * pixel[0]);
        //                sum += gray;
        //                integralImage[x, y] = sum + (y > 0 ? integralImage[x, y - 1] : 0);
        //            }
        //        });
        //    }

        //    unsafe
        //    {
        //        byte* srcPtr = (byte*)srcData.Scan0;
        //        byte* resPtr = (byte*)resData.Scan0;

        //        Parallel.For(0, height, y =>
        //        {
        //            for (int x = 0; x < width; x++)
        //            {
        //                int x1 = Math.Max(0, x - windowSize / 2);
        //                int y1 = Math.Max(0, y - windowSize / 2);
        //                int x2 = Math.Min(width - 1, x + windowSize / 2);
        //                int y2 = Math.Min(height - 1, y + windowSize / 2);

        //                int area = (x2 - x1 + 1) * (y2 - y1 + 1);
        //                int sum = integralImage[x2, y2] - (x1 > 0 ? integralImage[x1 - 1, y2] : 0) -
        //                          (y1 > 0 ? integralImage[x2, y1 - 1] : 0) +
        //                          ((x1 > 0 && y1 > 0) ? integralImage[x1 - 1, y1 - 1] : 0);

        //                int threshold = (int)(sum / area * (1.0 - k));

        //                byte* srcPixel = srcPtr + y * stride + x * bytesPerPixel;
        //                byte grayVal = (byte)(0.2125 * srcPixel[2] + 0.7154 * srcPixel[1] + 0.0721 * srcPixel[0]);
        //                byte bin = (grayVal <= threshold) ? (byte)0 : (byte)255;

        //                byte* resPixel = resPtr + y * stride + x * bytesPerPixel;
        //                resPixel[0] = resPixel[1] = resPixel[2] = bin;
        //                resPixel[3] = 255;
        //            }
        //        });
        //    }

        //    source.UnlockBits(srcData);
        //    result.UnlockBits(resData);
        //    return result;
        //}
    }
}
