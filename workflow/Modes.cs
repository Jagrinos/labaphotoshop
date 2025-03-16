using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.gradation_transformations;
using labaphotoshop.layers;

namespace labaphotoshop.workflow
{
    internal class Modes(FlowLayoutPanel flowPanelImages, Button addImageButton, PictureBox mainPicture, LayersForm layersForm, Label infoText)
    {
        private FlowLayoutPanel _flowPanelImages = flowPanelImages;
        private LayersForm _layersForm = layersForm;
        private Button _addImageButton = addImageButton;
        private PictureBox _mainPicture = mainPicture;
        private Label _infoText = infoText; 

        public void SetProcessingMode(string selectedMode)
        {
            switch (selectedMode)
            {
                case "Cлои":
                    _addImageButton.Visible = true;
                    _flowPanelImages.Controls.Clear();
                    _layersForm.RepaintLayers();
                    _layersForm.RepaintImage();
                    break;
                case "Град. Преоб.":
                    _addImageButton.Visible = false;
                    _flowPanelImages.Controls.Clear();

                    GradForm _gradForm = new(_flowPanelImages);
                    Histogram _histogram = new(_flowPanelImages, _mainPicture, _gradForm.HistogramImg);
                    Curve _curve = new(_mainPicture, _infoText, _histogram, _gradForm.CurveImg);
                    break;
                default:
                    break;
            }
        }
    }
}
