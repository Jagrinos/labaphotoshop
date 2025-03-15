using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using labaphotoshop.layers;
using labaphotoshop.workflow;

namespace labaphotoshop
{
    public partial class MainForm : Form
    {
        private List<(Bitmap image, string modeOfMultiply, float opacity)> _images = [];
        private LayersForm _layersForm;
        private WorkFlow _workFlow;
        
        public MainForm()
        {
            InitializeComponent();
            _layersForm = new(_images, FlowPanelImages, InfoText, MainPicture);
            _workFlow = new(FlowPanelImages, _layersForm);

            MainPicture.SizeMode = PictureBoxSizeMode.StretchImage;
            InfoText.Text = "";
            _layersForm.CreateLayer(Config.DefImgPath);
            CreateModeSelectionButtons();
        }

        private void CreateModeSelectionButtons()
        {
            string[] modes = ["Cлои", "Ч/Б"];
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
                _workFlow.SetProcessingMode(selectedMode);
                InfoText.Text = $"Выбран режим: {selectedMode}";
            }
        }

        private void AddImageButton_Click(object sender, EventArgs e)
        {
            //download files
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (openFileDialog.ShowDialog() == DialogResult.OK) //download files
            {
                _layersForm.CreateLayer(openFileDialog.FileName);
            }
        }
       
    }

}

