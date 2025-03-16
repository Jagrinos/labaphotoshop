using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.workflow;

namespace labaphotoshop.gradation_transformations
{
    internal class Curve
    {
        private PictureBox _mainPicture;
        private Histogram _histogram;

        private List<PointF> _controlPoints = [new PointF(0, 0), new PointF(255, 255)];
        private PictureBox _curveImg;
        private bool _isDragging = false;
        private PointF _draggingPoint;
        public Bitmap OriginImage;
        private Label _infoText;

        ~Curve()
        {
            OriginImage.Dispose();
        }

        public Curve(PictureBox mainPicture, Label infotext, Histogram histogram, PictureBox curveImg)
        {
            _mainPicture = mainPicture;
            _histogram = histogram;
            _curveImg = curveImg;
            _infoText = infotext;

            _curveImg.MouseClick += CurveImg_MouseClick!;
            _curveImg.MouseDown += CurveImg_MouseDown!;
            _curveImg.MouseMove += CurveImg_MouseMove!;
            _curveImg.MouseUp += CurveImg_MouseUp!;

            OriginImage = Funcs.BitmapChangeFormatTo32(new Bitmap(_mainPicture.Image!));
            UpdateCurve();
        }
        public void UpdateCurve()
        {
            int width = _curveImg.Width;
            int height = _curveImg.Height;
            Bitmap curveimg = new(width, height, PixelFormat.Format32bppArgb);

            using Graphics g = Graphics.FromImage(curveimg);
            g.Clear(Color.White);

            _controlPoints = [.. _controlPoints.OrderBy(p => p.X)];

            for (int i = 0; i < _controlPoints.Count - 1; i++)
            {
                var p1 = _controlPoints[i];
                var p2 = _controlPoints[i + 1];
                g.DrawLine(Pens.Black, p1.X, height - p1.Y, p2.X, height - p2.Y);
            }

            foreach (var p in _controlPoints)
            {
                g.FillEllipse(Brushes.Red, p.X - 8, height - p.Y - 8, 16, 16); //Рисует внутри прямоугольника, координаты обозначают угол
            }

            _curveImg.Image = curveimg;
            CurveTransformation();
        }

        private void CurveTransformation()
        {
            var map = GenerateTransformMap();

            int height = OriginImage.Height;
            int width = OriginImage.Width;
            var newImg = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData formingData = newImg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData workingData = OriginImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* formingPtr = (byte*)formingData.Scan0;
                byte* workingPtr = (byte*)workingData.Scan0;

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        int formingIndex = y * formingData.Stride + x * 4;
                        int workingIndex = y * workingData.Stride + x * 4;

                        byte workingB = workingPtr[workingIndex];
                        byte workingG = workingPtr[workingIndex + 1];
                        byte workingR = workingPtr[workingIndex + 2];

                        var newB = map[workingB];
                        var newG = map[workingG];
                        var newR = map[workingR];


                        formingPtr[formingIndex] = (byte)newB;
                        formingPtr[formingIndex + 1] = (byte)newG;
                        formingPtr[formingIndex + 2] = (byte)newR;
                        formingPtr[formingIndex + 3] = 255;
                    }
            }

            newImg.UnlockBits(formingData);
            OriginImage.UnlockBits(workingData);

            _mainPicture.Image = newImg;
            _histogram.UpdateHistogram();
        }

        private Dictionary<int, int> GenerateTransformMap()
        {
            Dictionary<int, int> map = new(256);
            _controlPoints = [.. _controlPoints.OrderBy(p => p.X)];

            for (int i = 0; i < _controlPoints.Count - 1; i++)
            {
                var p1 = _controlPoints[i];
                var p2 = _controlPoints[i + 1];

                for (int x = (int)p1.X; x <= (int)p2.X; x++)
                {
                    map[x] = (int)(p1.Y + (p2.Y - p1.Y) / (p2.X - p1.X) * (x - p1.X)); // f(x0) + (f(x1) - f(x0))/(x1 - x0) * (x - x0)
                }
            }
            return map;
        }


     private void CurveImg_MouseClick(object sender, MouseEventArgs e)
        {
            _controlPoints.Add(new PointF(e.X, _curveImg.Height - e.Y));
            _infoText.Text = "Create new point";
            UpdateCurve();
        }

        private void CurveImg_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (var p in _controlPoints)
            {
                var h = _curveImg.Height;
                if (Math.Abs(p.X - e.X) <= 16 && Math.Abs((h - p.Y) - (e.Y)) <= 16 && (p.X != 0 && p.Y != 0) && (p.X != 255 && p.Y != 255))
                {
                    _isDragging = true;
                    _draggingPoint = p;
                    _infoText.Text = "Point selected";
                    break;
                }
            }
        }
        private void CurveImg_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.X >= 0 && e.X <= _curveImg.Width && e.Y >= 0 && e.Y <= _curveImg.Height)
            {
                _controlPoints.Remove(_draggingPoint);
                _draggingPoint = new PointF(e.X, _curveImg.Height - e.Y);
                _controlPoints.Add(_draggingPoint);
                UpdateCurve();
            }
        }

        private void CurveImg_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }
    }
}