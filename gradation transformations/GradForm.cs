
using labaphotoshop.workflow;

namespace labaphotoshop.gradation_transformations
{
    internal class GradForm
    {
        private FlowLayoutPanel _flowPanelImages;
        public PictureBox HistogramImg { get; set; }
        public PictureBox CurveImg { get; set; }
        public GradForm(FlowLayoutPanel FlowPanelImages)
        {
            _flowPanelImages = FlowPanelImages;

            //histogram
            Panel histogramPanel = new()
            {
                Height = Config.HistogramHeight,
                Width = _flowPanelImages.Width - 20,
                BorderStyle = BorderStyle.FixedSingle
            };
            HistogramImg = new()
            {
                SizeMode = PictureBoxSizeMode.StretchImage,
                Width = _flowPanelImages.Width - 20,
                Height = Config.HistogramHeight
            };
            histogramPanel.Controls.Add(HistogramImg);
            _flowPanelImages.Controls.Add(histogramPanel);


            //curve
            Panel curvePanel = new()
            {
                Height = Config.HistogramHeight,
                Width = _flowPanelImages.Width - 20,
                BorderStyle = BorderStyle.FixedSingle
            };
            CurveImg = new()
            {
                Top = 32,
                Left = 62,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Width = 255,
                Height = 255
            };


            curvePanel.Controls.Add(CurveImg);
            _flowPanelImages.Controls.Add(curvePanel);
        }
    }
}
