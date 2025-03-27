namespace labaphotoshop
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MainPicture = new PictureBox();
            AddImageButton = new Button();
            FlowPanelImages = new FlowLayoutPanel();
            InfoText = new Label();
            Modes = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)MainPicture).BeginInit();
            SuspendLayout();
            // 
            // MainPicture
            // 
            MainPicture.Location = new Point(6, 6);
            MainPicture.Name = "MainPicture";
            MainPicture.Size = new Size(900, 800);
            MainPicture.TabIndex = 0;
            MainPicture.TabStop = false;
            // 
            // AddImageButton
            // 
            AddImageButton.Location = new Point(1018, 789);
            AddImageButton.Name = "AddImageButton";
            AddImageButton.Size = new Size(260, 36);
            AddImageButton.TabIndex = 2;
            AddImageButton.Text = "Add Image";
            AddImageButton.UseVisualStyleBackColor = true;
            AddImageButton.Click += AddImageButton_Click;
            // 
            // FlowPanelImages
            // 
            FlowPanelImages.Location = new Point(939, 54);
            FlowPanelImages.Name = "FlowPanelImages";
            FlowPanelImages.Size = new Size(400, 700);
            FlowPanelImages.TabIndex = 3;
            // 
            // InfoText
            // 
            InfoText.AutoSize = true;
            InfoText.Font = new Font("Segoe UI", 12F);
            InfoText.ForeColor = Color.Chocolate;
            InfoText.Location = new Point(969, 765);
            InfoText.Name = "InfoText";
            InfoText.Size = new Size(362, 21);
            InfoText.TabIndex = 4;
            InfoText.Text = "loremhfghfgddfghdfghghfdgfdhdgfhdfghfdghdfgh";
            InfoText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Modes
            // 
            Modes.Location = new Point(940, 6);
            Modes.Name = "Modes";
            Modes.Size = new Size(400, 42);
            Modes.TabIndex = 5;
            Modes.TabStop = false;
            Modes.Text = "Mode";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1376, 837);
            Controls.Add(Modes);
            Controls.Add(InfoText);
            Controls.Add(FlowPanelImages);
            Controls.Add(AddImageButton);
            Controls.Add(MainPicture);
            Name = "MainForm";
            Text = "MainForm";
            ((System.ComponentModel.ISupportInitialize)MainPicture).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox MainPicture;
        private Button AddImageButton;
        private FlowLayoutPanel FlowPanelImages;
        private Label InfoText;
        private GroupBox Modes;
    }
}
