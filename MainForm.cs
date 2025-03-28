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

            _modes = new(FlowPanelImages, AddImageButton, MainPicture, _layersForm, InfoText, this);

            MainPicture.SizeMode = PictureBoxSizeMode.StretchImage;
            InfoText.Text = "";

            _layersForm.CreateLayer(Config.DefImgPath);
            CreateModeSelectionButtons();
        }
        //wash me 30 45

        //mode
        private void CreateModeSelectionButtons()
        {
            string[] modes = ["C���", "����", "���", "������. ����", "��������� ����"];

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
                    Checked = mode == "C���",
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
                InfoText.Text = $"������ �����: {selectedMode}";
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

