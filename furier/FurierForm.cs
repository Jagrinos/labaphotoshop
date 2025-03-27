using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.layers;
using labaphotoshop.workflow;

namespace labaphotoshop.furier
{
    internal class FurierForm
    {
        private FlowLayoutPanel _flowPanelImages;
        private PictureBox _mainPicture;
        private MainForm _mainForm;
        private Label _infoText;

        private PictureBox _newMain;
        private PictureBox _furierBox;
        public Button WorkBut {  get; set; }
        public int R1 { get; set; } = 10;
        public int R2 { get; set; } = 30;
        public FilterMode FurierFilterMode { get; set; } = FilterMode.Reject;
        public double Brightness { get; set; } = 2;
       

        public FurierForm(FlowLayoutPanel flowLayoutPanel, PictureBox mainPicture, MainForm mainForm, Label infoText)
        {
            _flowPanelImages = flowLayoutPanel;
            _mainPicture = mainPicture;
            _mainForm = mainForm;

            Create();
            _infoText = infoText;
        }

        public void Clean()
        {
            _mainPicture.Image = Funcs.BitmapChangeFormatTo32(new Bitmap(_newMain.Image!));
            _newMain.Dispose();
            _furierBox.Dispose();
            _mainPicture.Visible = true;
        }

        private void Create()
        {
            _mainPicture.Visible = false;

            _newMain = new()
            {
                Width = 450,
                Height = 400,
                Location = new Point()
                {
                    X = 6,
                    Y = 196
                },
                Image = Funcs.BitmapChangeFormatTo32(new Bitmap(_mainPicture.Image!, new Size(450, 400)))
            };

            var std = new Bitmap(Config.Close);
            _furierBox = new()
            {
                Width = 450,
                Height = 400,
                Location = new Point()
                {
                    X = 456,
                    Y = 196
                },
                Image = new Bitmap(std, new Size(450, 400)),
            };

            _mainForm.Controls.Add(_newMain);
            _mainForm.Controls.Add(_furierBox);


            //val
            TableLayoutPanel valPanel = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            Label valbrightnessTxt = new()
            {
                Text = "Яркость",
            };
            TextBox valbrightness = new()
            {
                Name = "яркость",
                Text = "2",
            };
            valbrightness.TextChanged += onValueChanged!;

            Label valR1Txt = new()
            {
                Text = "R1",
            };
            TextBox valR1 = new()
            {
                Name = "r1",
                Text = "10",
            };
            valR1.TextChanged += onValueChanged!;

            Label valR2Txt = new()
            {
                Text = "R2",
            };
            TextBox valR2 = new()
            {
                Name = "r2",
                Text = "30",
            };
            valR2.TextChanged += onValueChanged!;

            ComboBox cmbMode = new()
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Items = { "Режектор", "Полосной", "Без фильтра"},
                Width = 100,
                Text = "Режектор"
            };

            cmbMode.SelectedIndexChanged += (s, e) =>
            {
                switch (cmbMode.SelectedItem?.ToString()) 
                {
                    case "Режектор":
                        FurierFilterMode = FilterMode.Reject;
                        _infoText.Text = "Выбран мод режектор";
                        break;
                    case "Полосной":
                        FurierFilterMode = FilterMode.BandPass;
                        _infoText.Text = "Выбран мод полосной";
                        break;
                    case "Без фильтра":
                        FurierFilterMode = FilterMode.None;
                        _infoText.Text = "Без фильтра";
                        break;
                    default:
                        break;
                }
            };

            WorkBut = new()
            {
                AutoSize = true,
                Text = "Применить"
            };

            valPanel.Controls.Add(valR1Txt, 0, 0);
            valPanel.Controls.Add(valR1, 1, 0);

            valPanel.Controls.Add(valR2Txt, 0, 1);
            valPanel.Controls.Add(valR2, 1, 1);

            valPanel.Controls.Add(cmbMode, 0, 2);

            valPanel.Controls.Add(valbrightnessTxt, 0, 3);
            valPanel.Controls.Add(valbrightness, 1, 3);

            valPanel.Controls.Add(WorkBut, 0, 4);


            _flowPanelImages.Controls.Add(valPanel);
        }

        private void onValueChanged(object sender, EventArgs e)
        {
            if (sender is TextBox tx) 
            {
                switch (tx.Name) 
                {
                    case "r1":
                        if (int.TryParse(tx.Text, out int r1) && r1 <= 200)
                        {
                            R1 = r1;
                            _infoText.Text = "r1 changed";
                            break;
                        }
                        _infoText.Text = "bad r1";
                        break;

                    case "r2":
                        if (int.TryParse(tx.Text, out int r2) && r2 <= 200)
                        {
                            R2 = r2;
                            _infoText.Text = "r2 changed";
                            break;
                        }
                        _infoText.Text = "bad r2";
                        break;
                    case "яркость":
                        if (double.TryParse(tx.Text, out double b))
                        {
                            Brightness = b;
                            _infoText.Text = "Brightness changed";
                            break;
                        }
                        _infoText.Text = "bad brightness";
                        break;
                    default:
                        break;
                }
            }
        }

        public Bitmap GetMainPictureImage()
        {
            return Funcs.BitmapChangeFormatTo32(new Bitmap(_mainPicture.Image!, new Size(450, 400)));
        }
        public void SetNewMainImage(Bitmap img)
        {
            _newMain.Image = img;
        }
        public void SetFurierImage(Bitmap img)
        {
            _furierBox.Image = img;
        }
    }
}
