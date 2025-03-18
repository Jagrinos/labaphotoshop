using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using labaphotoshop.gradation_transformations;
using labaphotoshop.layers;
using labaphotoshop.workflow;

namespace labaphotoshop
{
    public partial class MainForm : Form
    {
        private LayersForm _layersForm;
        private Modes _modes;
        public MainForm()
        {
            InitializeComponent();
            //layers
            _layersForm = new(FlowPanelImages, InfoText, MainPicture);

            _modes = new(FlowPanelImages, AddImageButton, MainPicture, _layersForm, InfoText);

            MainPicture.SizeMode = PictureBoxSizeMode.StretchImage;
            InfoText.Text = "";

            _layersForm.CreateLayer(Config.DefImgPath);
            CreateModeSelectionButtons();
        }

        //bin
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

                        byte bin = gray <= threshold ? (byte) 0 : (byte) 255;
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
            //for (int i = 0; i < 256; i++)
            //{
            //    histogram[i] /= sizeImg;
            //}

            var uT = 0;
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] == histogram.Max())
                    break;

                uT += i * histogram[i];
            }

            //calculation threshold
            var delta2 = -int.MaxValue;
            var threshold = 0;
            int w1, w2, u1, u2, delta2new;

            for (int t = 0; t < 256; t++)
            {
                if (histogram[t] == histogram.Max())
                    break;

                if (t == 0 || t == 1)
                {
                    w1 = histogram[t];
                    w2 = 1 - w1;

                    u1 = 1;
                    u2 = (uT - w1) / w2;

                    delta2new = w1 * w2 * (int)Math.Pow(u1 - u2, 2);
                    if (delta2new > delta2)
                    {
                        delta2 = delta2new;
                        threshold = t;
                    }
                    continue;
                }

                w1 = 0;
                for (int i = 0; i < t-1; i++)
                {
                    w1 += histogram[i];
                }
                w2 = 1 - w1;

                var uNi = 0;
                for (int i = 0; i < t - 1; i++)
                {
                    uNi += i * histogram[i];
                }
                u1 = uNi / w1;
                u2 = (uT - u1 * w1) / w2;

                delta2new = w1 * w2 * (int)Math.Pow(u1 - u2, 2);
                if (delta2new > delta2)
                {
                    delta2 = delta2new;
                    threshold = t;
                }
            }
            InfoText.Text = threshold.ToString();
            
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

        private void BinModes(string MODE)
        {
            switch (MODE) 
            {
                case "Градации серого":
                    MainPicture.Image = Grayscale(Funcs.BitmapChangeFormatTo32(new Bitmap(MainPicture.Image!)));
                    break;
                case "Критерий Гаврилова":
                    MainPicture.Image = GavrilovThresholding(Funcs.BitmapChangeFormatTo32(new Bitmap(MainPicture.Image!)));
                    break;
                case "Критерий Отсо":
                    MainPicture.Image = OtsuThresholding(Funcs.BitmapChangeFormatTo32(new Bitmap(MainPicture.Image!)));
                    break;
                default:
                    break;
            }
        }
        private void BinForm()
        {
            string[] modes = ["Градации серого", "Критерий Гаврилова", "Критерий Отсо"];

            TableLayoutPanel tableLayout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = modes.Length,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            for (int i = 0; i < modes.Length; i++)
            {
                var mode = modes[i];
                RadioButton button = new()
                {
                    Text = mode,
                    AutoSize = true,
                };
                button.CheckedChanged += BinModeChange!;
                tableLayout.Controls.Add(button, 0, i);
            }
            FlowPanelImages.Controls.Add(tableLayout);    
        }

        private void BinModeChange(object sender, EventArgs e)
        {
            if (sender is RadioButton button && button.Checked) 
            {
                var MODE = button.Text;
                InfoText.Text = $"Выбран режим: {MODE}";
                BinModes(MODE);
            }
        }

        //mode
        private void CreateModeSelectionButtons()
        {
            string[] modes = ["Cлои", "Град. Преоб.", "Бинаризация"];
            

            TableLayoutPanel tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, 
                ColumnCount = modes.Length,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            for (int i = 0; i < modes.Length; i++) {
                var mode = modes[i];
                RadioButton button = new()
                {
                    Text = mode,
                    AutoSize = true,
                    Checked = mode == "Cлои",
                };

                button.CheckedChanged += ModeSelectionChanged!;
                tableLayout.Controls.Add(button, i, 0);
                //xOffset += 100 - mode.Length;
                
            }
            Modes.Controls.Add(tableLayout);
        }
        private void ModeSelectionChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                string selectedMode = radioButton.Text;
                _modes.SetProcessingMode(selectedMode);
                InfoText.Text = $"Выбран режим: {selectedMode}";

                //bin test
                if (selectedMode == "Бинаризация")
                {
                    AddImageButton.Visible = false;
                    FlowPanelImages.Controls.Clear();
                    BinForm();
                }
            }
        }
        
        //layers
        private void AddImageButton_Click(object sender, EventArgs e)
        {
            //download files
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _layersForm.CreateLayer(openFileDialog.FileName);
            }
        }
       
    }

}

