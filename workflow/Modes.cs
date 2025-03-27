using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.furier;
using labaphotoshop.gradation_transformations;
using labaphotoshop.layers;
using labaphotoshop.Space_filter;
using labaphotoshop.thresholding;

namespace labaphotoshop.workflow
{
    internal class Modes(FlowLayoutPanel flowPanelImages, Button addImageButton, PictureBox mainPicture, LayersForm layersForm, Label infoText, MainForm mainForm)
    {
        private FlowLayoutPanel _flowPanelImages = flowPanelImages;
        private LayersForm _layersForm = layersForm;
        private Button _addImageButton = addImageButton;
        private PictureBox _mainPicture = mainPicture;
        private Label _infoText = infoText;
        private MainForm _mainForm = mainForm;

        private FurierForm? _ff = null;

        public void SetProcessingMode(string selectedMode)
        {
            _mainPicture.Width = 900;
            _mainPicture.Height = 800;
            switch (selectedMode)
            {
                case "Cлои":
                    _ff?.Clean();

                    _addImageButton.Visible = true;
                    _flowPanelImages.Controls.Clear();
                    _layersForm.RepaintLayers();
                    _layersForm.RepaintImage();
                    break;
                case "Град":
                    _ff?.Clean();

                    _addImageButton.Visible = false;
                    _flowPanelImages.Controls.Clear();

                    GradForm gradForm = new(_flowPanelImages);
                    Histogram histogram = new(_flowPanelImages, _mainPicture, gradForm.HistogramImg);
                    Curve curve = new(_mainPicture, _infoText, histogram, gradForm.CurveImg);
                    break;
                case "Бин":
                    _ff?.Clean();

                    _addImageButton.Visible = false;
                    _flowPanelImages.Controls.Clear();

                    ThresholdingForm thresholdingForm = new(_flowPanelImages);
                    _ = new ThresholdingFuncs(thresholdingForm, _infoText, _mainPicture);
                    break;
                case "Простр. Флтр":
                    _ff?.Clean();

                    _addImageButton.Visible = true;
                    _flowPanelImages.Controls.Clear();

                    SfForm sfForm = new(_flowPanelImages, _infoText, _mainPicture);
                    break;

                case "Частотная Флтр":
                    _addImageButton.Visible = true;
                    _flowPanelImages.Controls.Clear();

                    _ff = new(_flowPanelImages, _mainPicture, _mainForm, _infoText);
                    _ = new FurierHandler(_ff, _infoText);
                    break;
                default:
                    break;
            }
        }
    }
}
