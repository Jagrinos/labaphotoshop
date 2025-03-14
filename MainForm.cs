using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace labaphotoshop
{
    public partial class MainForm : Form
    {
        private List<(Bitmap image, string modeOfMultiply, float opacity)> _images = [];
        private string _defImgPath = "D:\\progers\\vsreal\\labaphotoshop\\images\\default.png";

        public MainForm()
        {
            InitializeComponent();
            MainPicture.SizeMode = PictureBoxSizeMode.StretchImage;
            InfoText.Text = "";
            AddImage(_defImgPath);
        }

        private void AddImageButton_Click(object sender, EventArgs e)
        {
            //download files
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (openFileDialog.ShowDialog() == DialogResult.OK) //download files
            {
                AddImage(openFileDialog.FileName);
            }
        }

        private void AddImage(string filename)
        {
            //change format
            var originalImage = new Bitmap(filename);
            var newImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.DrawImage(originalImage, 0, 0, originalImage.Width, originalImage.Height);
            }

            _images.Insert(0, (newImage, "Sum", 1.0f));
            //forming new panel
            Panel imagePanel = new()
            {
                Height = 100,
                Width = FlowPanelImages.Width - 20,
                BorderStyle = BorderStyle.FixedSingle
            };

            PictureBox icon = new()
            {
                Image = new Bitmap(newImage, new Size(80, 80)),
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 80,
                Height = 80,
                Left = 5,
                Top = 10
            };

            ComboBox cmbMode = new()
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "Sum", "Multiply", "None", "Difference" },
                SelectedItem = "Sum",
                Left = 90,
                Top = 10,
                Width = 100
            };

            cmbMode.SelectedIndexChanged += (sender, e) =>
            {
                int index = FlowPanelImages.Controls.IndexOf(imagePanel) + 1;
                _images[^index] = (_images[^index].image, cmbMode.SelectedItem.ToString(), _images[^index].opacity)!;
                InfoText.Text = "loading...";
                RePaint();
            };

            TrackBar trackOpacity = new()
            {
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                TickStyle = TickStyle.None,
                Left = 90,
                Top = 40,
                Width = 100
            };

            trackOpacity.Scroll += (sender, e) =>
            {
                int index = FlowPanelImages.Controls.IndexOf(imagePanel) + 1;
                _images[^index] = (_images[^index].image, _images[^index].modeOfMultiply, trackOpacity.Value / 100f);
                InfoText.Text = "loading...";
                RePaint();
            };

            Button delBut = new()
            {
                Left = imagePanel.Width - 20,
                Width = 20,
                Height = 20,
                Image = System.Drawing.Image.FromFile("D:\\progers\\vsreal\\labaphotoshop\\images\\closeicon.png"),
                ImageAlign = ContentAlignment.MiddleCenter,
            };
            delBut.Click += (sender, e) =>
            {
                int index = FlowPanelImages.Controls.IndexOf(imagePanel) + 1;
                _images[^index].image.Dispose();
                _images.Remove(_images[^index]);
                FlowPanelImages.Controls.Remove(imagePanel);
                InfoText.Text = "loading...";
                RePaint();
            };

            Button downBut = new()
            {
                Left = imagePanel.Width - 60,
                Width = 20,
                Height = 20,
                Top = imagePanel.Height - 30,
                Image = System.Drawing.Image.FromFile("D:\\progers\\vsreal\\labaphotoshop\\images\\downicon.png"),
                ImageAlign = ContentAlignment.MiddleCenter,
            };
            downBut.Click += (sender, e) =>
            {
                int pannelIndex = FlowPanelImages.Controls.IndexOf(imagePanel);
                if (pannelIndex < FlowPanelImages.Controls.Count - 1)
                {
                    (_images[^(pannelIndex + 2)], _images[^(pannelIndex + 1)]) = (_images[^(pannelIndex + 1)], _images[^(pannelIndex + 2)]);

                    FlowPanelImages.Controls.SetChildIndex(imagePanel, pannelIndex + 1);
                    RePaint();
                }
            };

            Button upBut = new()
            {
                Left = imagePanel.Width - 30,
                Width = 20,
                Height = 20,
                Top = imagePanel.Height - 30,
                Image = System.Drawing.Image.FromFile("D:\\progers\\vsreal\\labaphotoshop\\images\\upicon.png"),
                ImageAlign = ContentAlignment.MiddleCenter,
            };
            upBut.Click += (sender, e) =>
            {
                int pannelIndex = FlowPanelImages.Controls.IndexOf(imagePanel);
                if (pannelIndex > 0)
                {
                    (_images[^(pannelIndex)], _images[^(pannelIndex + 1)]) = (_images[^(pannelIndex + 1)], _images[^(pannelIndex)]);

                    FlowPanelImages.Controls.SetChildIndex(imagePanel, pannelIndex - 1);
                    RePaint();
                }
            };

            imagePanel.Controls.Add(icon);
            imagePanel.Controls.Add(cmbMode);
            imagePanel.Controls.Add(trackOpacity);
            imagePanel.Controls.Add(delBut);
            imagePanel.Controls.Add(downBut);
            imagePanel.Controls.Add(upBut);

            FlowPanelImages.Controls.Add(imagePanel);
            RePaint();
        }

        private void RePaint()
        {
            MainPicture.Image?.Dispose();

            if (_images.Count == 0)
            {
                Bitmap defImg = new(_defImgPath);
                MainPicture.Image = defImg;
                return;
            }

            Bitmap formingImage = new(MainPicture.Width, MainPicture.Height, PixelFormat.Format32bppArgb);
           

            foreach (var (image, mode, opacity) in _images)
            {
                switch (mode)
                {
                    case "None":
                        break;
                    default:
                        Image.DrawWithModeByte(formingImage, image, opacity, mode);
                        break;
                }
            }
            MainPicture.Image = formingImage;
            InfoText.Text = "success";
        }


        
        
    }

}

