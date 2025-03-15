using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.workflow;

namespace labaphotoshop.layers
{
    internal class LayersForm(List<(Bitmap image, string modeOfMultiply, float opacity)> images, FlowLayoutPanel flowPanelImages, Label infoText, PictureBox mainPicture)
    {
        //private List<(Bitmap image, string modeOfMultiply, float opacity)> layers = images;
        private FlowLayoutPanel _flowPanelImages = flowPanelImages;
        private Label _infoText = infoText;
        private PictureBox _mainPicture = mainPicture;

        private List<Layer> layers = [];

        public void RepaintLayers()
        {
            foreach (Layer layer in layers)
            {
                DrowLayer(layer);
            }
        }

        public void RepaintImage()
        {
            _mainPicture.Image?.Dispose();

            if (layers.Count == 0)
            {
                Bitmap defImg = new(Config.DefImgPath);
                _mainPicture.Image = defImg;
                return;
            }

            Bitmap formingImage = new(_mainPicture.Width, _mainPicture.Height, PixelFormat.Format32bppArgb);


            foreach (var layer in layers)
            {
                switch (layer.ModeOfMultiply)
                {
                    case "None":
                        break;
                    default:
                        LayersImage.DrawWithModeByte(formingImage, layer.Image, layer.Opacity, layer.ModeOfMultiply!);
                        break;
                }
            }
            _mainPicture.Image = formingImage;
            _infoText.Text = "success";
        }

        public void DrowLayer(Layer layer)
        {
            Panel imagePanel = new()
            {
                Height = 100,
                Width = _flowPanelImages.Width - 20,
                BorderStyle = BorderStyle.FixedSingle
            };

            PictureBox icon = new()
            {
                Image = new Bitmap(layer.Image, new Size(80, 80)),
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
                SelectedItem = layer.ModeOfMultiply,
                Left = 90,
                Top = 10,
                Width = 100
            };

            cmbMode.SelectedIndexChanged += (sender, e) =>
            {
                int index = _flowPanelImages.Controls.IndexOf(imagePanel) + 1;
                layers[^index] = new Layer(layers[^index].Image, cmbMode.SelectedItem?.ToString()!, layers[^index].Opacity);
                _infoText.Text = "loading...";
                RepaintImage();
            };

            TrackBar trackOpacity = new()
            {
                Minimum = 0,
                Maximum = 100,
                Value = (int) (layer.Opacity * 100),
                TickStyle = TickStyle.None,
                Left = 90,
                Top = 40,
                Width = 100
            };


            trackOpacity.Scroll += (sender, e) =>
            {
                int index = _flowPanelImages.Controls.IndexOf(imagePanel) + 1;
                layers[^index] = new(layers[^index].Image, layers[^index].ModeOfMultiply, trackOpacity.Value / 100f);
                _infoText.Text = "loading...";
                RepaintImage();
            };

            Button delBut = new()
            {
                Left = imagePanel.Width - 20,
                Width = 20,
                Height = 20,
                Image = Image.FromFile(Config.CloseIcon),
                ImageAlign = ContentAlignment.MiddleCenter,
            };
            delBut.Click += (sender, e) =>
            {
                int index = _flowPanelImages.Controls.IndexOf(imagePanel) + 1;
                layers[^index].Image.Dispose();
                layers.Remove(layers[^index]);
                _flowPanelImages.Controls.Remove(imagePanel);
                _infoText.Text = "loading...";
                RepaintImage();
            };

            Button downBut = new()
            {
                Left = imagePanel.Width - 60,
                Width = 20,
                Height = 20,
                Top = imagePanel.Height - 30,
                Image = Image.FromFile(Config.DownIcon),
                ImageAlign = ContentAlignment.MiddleCenter,
            };
            downBut.Click += (sender, e) =>
            {
                int pannelIndex = _flowPanelImages.Controls.IndexOf(imagePanel);
                if (pannelIndex < _flowPanelImages.Controls.Count - 1)
                {
                    (layers[^(pannelIndex + 2)], layers[^(pannelIndex + 1)]) = (layers[^(pannelIndex + 1)], layers[^(pannelIndex + 2)]);

                    _flowPanelImages.Controls.SetChildIndex(imagePanel, pannelIndex + 1);
                    RepaintImage();
                }
            };

            Button upBut = new()
            {
                Left = imagePanel.Width - 30,
                Width = 20,
                Height = 20,
                Top = imagePanel.Height - 30,
                Image = Image.FromFile(Config.UpIcon),
                ImageAlign = ContentAlignment.MiddleCenter,
            };
            upBut.Click += (sender, e) =>
            {
                int pannelIndex = _flowPanelImages.Controls.IndexOf(imagePanel);
                if (pannelIndex > 0)
                {
                    (layers[^(pannelIndex)], layers[^(pannelIndex + 1)]) = (layers[^(pannelIndex + 1)], layers[^(pannelIndex)]);

                    _flowPanelImages.Controls.SetChildIndex(imagePanel, pannelIndex - 1);
                    RepaintImage();
                }
            };

            imagePanel.Controls.Add(icon);
            imagePanel.Controls.Add(cmbMode);
            imagePanel.Controls.Add(trackOpacity);
            imagePanel.Controls.Add(delBut);
            imagePanel.Controls.Add(downBut);
            imagePanel.Controls.Add(upBut);

            _flowPanelImages.Controls.Add(imagePanel);
        }
        public void CreateLayer(string filename)
        {
            //change format
            var originalImage = new Bitmap(filename);
            var newImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.DrawImage(originalImage, 0, 0, originalImage.Width, originalImage.Height);
            }

            layers.Insert(0, new Layer(filename));

            DrowLayer(layers[0]);
            RepaintImage();
        }
    }
}
