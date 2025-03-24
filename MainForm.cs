using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using labaphotoshop.gradation_transformations;
using labaphotoshop.layers;
using labaphotoshop.workflow;
using Microsoft.VisualBasic.Devices;

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

        //filter

        

        //mode
        private void CreateModeSelectionButtons()
        {
            string[] modes = ["Cлои", "Град", "Бин", "Пр Фильтр"];

            TableLayoutPanel tableLayout = new()
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
                tableLayout.Controls.Add(button, i,0);                
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

