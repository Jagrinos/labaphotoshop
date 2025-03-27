using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.workflow;

namespace labaphotoshop.Space_filter
{
    internal class SfForm
    {
        private DataGridView _gridMatrix;
        private double[,] _matrix;
        private FlowLayoutPanel _flowPanelImages;
        private Label _infoText;
        private Bitmap _mainImage;
        private PictureBox _mainPicture;

        private string MODE;
        private int _size = 21;
        private double _sigma = 1;
        private int _mWidth = 3;
        private int _mHeight = 3;

        public SfForm(FlowLayoutPanel flowPanelImages, Label infoText, PictureBox mainPicture)
        {
            _flowPanelImages = flowPanelImages;
            _infoText = infoText;
            _mainPicture = mainPicture;
            _mainImage = Funcs.BitmapChangeFormatTo32(new Bitmap(mainPicture.Image!));

            SpaceFilterFormCreate();
        }
        ~SfForm() 
        {
            _mainImage.Dispose();
        }

        private void SpaceFilterFormCreate()
        {
            string[] modes = ["Линейная фильтрация", "Медианная фильтрация", "По Гауссу", "Сбросить"];

            TableLayoutPanel modeLayout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = modes.Length,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            for (int i = 0; i < modes.Length; i++)
            {
                var mode = modes[i];
                RadioButton button = new()
                {
                    Text = mode,
                    AutoSize = true
                   
                };

                button.CheckedChanged += ModeSelectionChangedSpaceFilter!;
                modeLayout.Controls.Add(button, 0, i);
            }
            _flowPanelImages.Controls.Add(modeLayout);

            //matrix
            Panel mxPanel = new()
            {
                AutoSize = true
            };

            _gridMatrix = new()
            {
                AutoSize = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ColumnHeadersVisible = false,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.None,
            };
            _gridMatrix.CellValueChanged += OnValueChanged!;

            for (int i = 0; i < 3; i++)
            {
                AddColumn();
                AddRow();
            }
            UpdateMatrix();

            mxPanel.Controls.Add(_gridMatrix);

            _flowPanelImages.Controls.Add(mxPanel);
            _flowPanelImages.Controls.Add(SetupControlButtons());

            //val medium

            TableLayoutPanel valPanelMed = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            Label medInfo = new()
            {
                Text = "Мед. фильтр",
            };

            Label valHeightTxt = new()
            {
                Text = "Height",
            };
            TextBox valHeight = new()
            {
                Name = "height",
                Text = "3",
            };
            valHeight.TextChanged += onValChange!;

            Label valWidthTxt = new()
            {
                Text = "Width",
            };
            TextBox valWidth = new()
            {
                Name = "width",
                Text = "3",
            };
            valWidth.TextChanged += onValChange!;

            valPanelMed.Controls.Add(medInfo, 0, 0);

            valPanelMed.Controls.Add(valHeightTxt, 0, 1);
            valPanelMed.Controls.Add(valHeight, 0, 2);

            valPanelMed.Controls.Add(valWidthTxt, 1, 1);
            valPanelMed.Controls.Add(valWidth, 1, 2);

            _flowPanelImages.Controls.Add(valPanelMed);

            //val gauss
            TableLayoutPanel valPanel = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            Label gaussInfo = new()
            {
                Text = "По Гауссу",
            };

            Label valRTxt = new()
            {
                Text = "Size",
            };
            TextBox valR = new()
            {
                Name = "size",
                Text = "21",
            };
            valR.TextChanged += onValChange!;

            Label valSigTxt = new()
            {
                Text = "Sigma",
            };
            TextBox valSig = new()
            {
                Name = "sigma",
                Text = "1",
            };
            valSig.TextChanged += onValChange!;

            valPanel.Controls.Add(gaussInfo, 0, 0);

            valPanel.Controls.Add(valRTxt, 0, 1);
            valPanel.Controls.Add(valR, 0, 2);

            valPanel.Controls.Add(valSigTxt, 1, 1);
            valPanel.Controls.Add(valSig, 1, 2);

            _flowPanelImages.Controls.Add(valPanel);


            Button workBut = new()
            {
                Text = "Применить",
                AutoSize = true
            };
            workBut.Click += OnClickWorkBut!;

            _flowPanelImages.Controls.Add(workBut);
        }

        private void onValChange(object s, EventArgs e)
        {
            if (s is TextBox tx)
            {
                switch (tx.Name)
                {
                    case "size":
                        if (int.TryParse(tx.Text, out int resSize) && resSize % 2 != 0)
                        {
                            _size = resSize;
                            _infoText.Text = "size changed";
                            break;
                        }
                        _infoText.Text = "size bad";
                        _size = 21;
                        break;
                    case "sigma":
                        if (double.TryParse(tx.Text, out double resSigma))
                        {
                            _sigma = resSigma;
                            _infoText.Text = "sigma changed";
                            break;
                        }
                        _infoText.Text = "sigma bad";
                        _sigma = 1;
                        break;

                    case "width":
                        if (int.TryParse(tx.Text, out int width) && width % 2 != 0)
                        {
                            _mWidth = width;
                            _infoText.Text = "width changed";
                            break;
                        }
                        _infoText.Text = "width bad";
                        _mWidth = 3;
                        break;
                    case "height":
                        if (int.TryParse(tx.Text, out int height) && height % 2 != 0)
                        {
                            _mHeight = height;
                            _infoText.Text = "height changed";
                            break;
                        }
                        _infoText.Text = "height bad";
                        _mHeight = 3;
                        break;
                }
            }
        }

        private void OnClickWorkBut(object s, EventArgs e)
        {
            RepaintImage(MODE);
        }

        private void ModeSelectionChangedSpaceFilter(object s, EventArgs e)
        {
            if (s is RadioButton button && button.Checked)
            {
                _infoText.Text = $"Выбран режим {button.Text}";
                MODE = button.Text;
                RepaintImage(MODE);
            }
        }

        private void RepaintImage(string MODE)
        {
            Bitmap newImg;
            Stopwatch sw;
            switch (MODE)
            {
                case "Линейная фильтрация":
                    _infoText.Text = "loading...";
                    Application.DoEvents();

                    sw = Stopwatch.StartNew();

                    newImg = SfFuncs.ApplyLinearFilter(_mainImage, _matrix);
                    _mainPicture.Image?.Dispose();
                    _mainPicture.Image = newImg;

                    sw.Stop();
                    _infoText.Text = $"yeeeeey {sw.ElapsedMilliseconds}";
                    break;
                case "Медианная фильтрация":
                    _infoText.Text = "loading...";
                    Application.DoEvents();
                    sw = Stopwatch.StartNew();

                    newImg = SfFuncs.ApplyMedianFilter(_mainImage, _mHeight, _mWidth);
                    _mainPicture.Image?.Dispose();
                    _mainPicture.Image = newImg;
                    
                    sw.Stop();
                    _infoText.Text = $"yeeeeey {sw.ElapsedMilliseconds} {_mHeight} x {_mWidth}";
                    break;

                case "По Гауссу":
                    _infoText.Text = "loading...";
                    Application.DoEvents();
                    sw = Stopwatch.StartNew();

                    var kernel = SfFuncs.GenerateGaussianMatrix(_size, _sigma, out double sum);
                    newImg = SfFuncs.ApplyLinearFilter(_mainImage, kernel);

                    _mainPicture.Image?.Dispose();
                    _mainPicture.Image = newImg;

                    sw.Stop();
                    _infoText.Text = $"do {sw.ElapsedMilliseconds} sum = {double.Round(sum,4)}";
                    break;
                case "Сбросить":
                    newImg = new Bitmap(_mainImage);
                    _mainPicture.Image?.Dispose();
                    _mainPicture.Image = newImg;
                    break;
                default:
                    break;
            }
        }

        private void OnValueChanged(object s, EventArgs e)
        {
            //if (UpdateMatrix())
            //    RepaintImage(MODE);
            UpdateMatrix();
        }

        private bool UpdateMatrix()
        {
            _matrix = new double[_gridMatrix.Rows.Count, _gridMatrix.Columns.Count];
            if (_gridMatrix.Rows.Count % 2 == 0 || _gridMatrix.Columns.Count % 2 == 0) 
                return false;

            for (int i = 0; i < _gridMatrix.Rows.Count; i++)
                for (int j = 0; j < _gridMatrix.Columns.Count; j++)
                {
                    _infoText.Text = "good val";
                    var value = _gridMatrix.Rows[i].Cells[j].Value;
                    var parts = value?.ToString()?.Split('/');

                    if (parts != null && parts.Length == 2 && double.TryParse(parts[0], out double p1) && double.TryParse(parts[1], out double p2))
                    {
                        _matrix[i, j] = p1 / p2;
                    }
                    else
                    {
                        if (int.TryParse(value?.ToString(), out int res))
                        {
                            _matrix[i, j] = res;
                        }
                        else
                        {
                            _infoText.Text = "err in val";
                            return false;
                        }
                       
                    }
                        
                }
            return true;
        }

        //TESTTEST
        private void newValues()
        {
            for (int i = 0; i < _gridMatrix.Rows.Count; i++)
                for (int j = 0; j < _gridMatrix.Columns.Count; j++)
                {
                    _gridMatrix.Rows[i].Cells[j].Value = $"1/{_gridMatrix.Rows.Count * _gridMatrix.Columns.Count}";
                }
        }


        private void AddRow()
        {
            _gridMatrix.Rows.Add(new DataGridViewRow()
            {
                Height = 25,
            });

            for (int i = 0; i < _gridMatrix.Columns.Count; i++)
                _gridMatrix[i, _gridMatrix.Rows.Count - 1].Value = "1/9"; //1.0 / (_gridMatrix.Columns.Count * _gridMatrix.Rows.Count);
        }
        private void DelRow()
        {
            if (_gridMatrix.Rows.Count > 1)
            {
                _gridMatrix.Rows.Remove(_gridMatrix.Rows[^1]);
                _gridMatrix.Rows.Remove(_gridMatrix.Rows[^1]);
            }
        }

        private void AddColumn()
        {
            _gridMatrix.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Width = 50,
                ValueType = typeof(string)
            });

            for (int i = 0; i < _gridMatrix.Rows.Count; i++)
                _gridMatrix[_gridMatrix.Columns.Count - 1, i].Value = "1/9"; // 1.0 / (_gridMatrix.Columns.Count * _gridMatrix.Rows.Count);
        }
        private void DelColumn()
        {
            if (_gridMatrix.Columns.Count > 1)
            {
                _gridMatrix.Columns.Remove(_gridMatrix.Columns[^1]);
                _gridMatrix.Columns.Remove(_gridMatrix.Columns[^1]);
            }
        }

        private void OnRowAdd(object sender, EventArgs e)
        {
            AddRow();
            EnsureOdd();
            newValues();
            UpdateMatrix();
        }
        private void OnRowDel(object sender, EventArgs e)
        {
            DelRow();
            newValues();
            UpdateMatrix();
        }

        private void OnColumnAdd(object sender, EventArgs e)
        {
            AddColumn();
            EnsureOdd();
            newValues();
            UpdateMatrix();
        }

        private void OnColumnDel(object sender, EventArgs e)
        {
            DelColumn();
            newValues();
            UpdateMatrix();
        }

        private void EnsureOdd()
        {
            while (_gridMatrix.Rows.Count % 2 == 0)
            {
                AddRow();
            }
            while (_gridMatrix.Columns.Count % 2 == 0)
            {
                AddColumn();
            }
        }

        private TableLayoutPanel SetupControlButtons()
        {
            var btnPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            var addRowBtn = new Button
            {
                Text = "Add Row",
                AutoSize = true,
                Margin = new Padding(5)
            };
            addRowBtn.Click += OnRowAdd!;

            var delRowBtn = new Button
            {
                Text = "Del Row",
                AutoSize = true,
                Margin = new Padding(5)
            };
            delRowBtn.Click += OnRowDel!;

            var addColBtn = new Button
            {
                Text = "Add Column",
                AutoSize = true,
                Margin = new Padding(5)
            };
            addColBtn.Click += OnColumnAdd!;

            var delColBtn = new Button
            {
                Text = "Del Column",
                AutoSize = true,
                Margin = new Padding(5)
            };
            delColBtn.Click += OnColumnDel!;

            btnPanel.Controls.Add(addRowBtn, 0, 0);
            btnPanel.Controls.Add(addColBtn, 1, 0);

            btnPanel.Controls.Add(delRowBtn, 0, 1);
            btnPanel.Controls.Add(delColBtn, 1, 0);


            return btnPanel;
        }

    }
}
