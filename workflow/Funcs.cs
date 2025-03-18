using System;
using System.Collections.Generic;
using System.Drawing.Imaging;


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

        public static List<Bitmap> SplitImg(Bitmap originalImage, int countOfParts) 
        {
            Bitmap[] parts = new Bitmap[countOfParts];

            int partHeight = originalImage.Height / countOfParts;
            int width = originalImage.Width;

            var origData = originalImage.LockBits(new Rectangle(0, 0, width, originalImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Parallel.For(0, countOfParts, i =>
            {
                int y = i * partHeight;
                int height = partHeight;
                parts[i] = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                var createData = parts[i].LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* origPtr = (byte*)origData.Scan0;
                    byte* createPtr = (byte*)createData.Scan0;

                    Parallel.For(0, partHeight, row =>
                    {
                        for (int col = 0; col < width * 4; col++)
                        {
                            var origIndex = 
                            createPtr[row * createData.Stride + col] = origPtr[(row + y) * origData.Stride+ col];
                        }
                    });

                    
                    parts[i].UnlockBits(createData);
                }
            });
            originalImage.UnlockBits(origData);
            return [..parts];
        }

        public static Bitmap CombineImgs(List<Bitmap> parts)
        {
            int width = parts[0].Width;
            int height = parts[0].Height * parts.Count;
            int partHeight = parts[0].Height;

            Bitmap res = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            var createData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Parallel.For(0, parts.Count, i =>
            {
                int y = i * partHeight;

                var origData = parts[i].LockBits(new Rectangle(0, 0, width, partHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* origPtr = (byte*)origData.Scan0;
                    byte* createPtr = (byte*)createData.Scan0;

                    Parallel.For(0, partHeight, row =>
                    {
                        for (int col = 0; col < width * 4; col++)
                        {
                            createPtr[(row + y) * origData.Stride + col] = origPtr[row * createData.Stride + col]; 
                        }
                    });
                }
                
                parts[i].UnlockBits(origData);
            });
            res.UnlockBits(createData);
            return res;
        }
    }
}
