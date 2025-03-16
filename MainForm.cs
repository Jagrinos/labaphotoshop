using System.Drawing.Imaging;
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
        private GradForm _gradForm;
        private Histogram _histogram;
        private Curve _curve;
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
        

        //mode
        private void CreateModeSelectionButtons()
        {
            string[] modes = ["Cлои", "Град. Преоб."];
            int xOffset = 20;

            foreach (var mode in modes)
            {
                RadioButton button = new()
                {
                    Text = mode,
                    Left = xOffset,
                    Top = 20,
                    AutoSize = true,
                    Checked = mode == "Cлои",
                };
                
                button.CheckedChanged += ModeSelectionChanged!;
                Modes.Controls.Add(button);
                xOffset += 70;
            }
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

