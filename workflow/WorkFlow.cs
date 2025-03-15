using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.layers;

namespace labaphotoshop.workflow
{
    internal class WorkFlow(FlowLayoutPanel flowPanelImages, LayersForm layersForm)
    {
        private FlowLayoutPanel _flowPanelImages = flowPanelImages;
        public LayersForm _layersForm { get; set; } = layersForm;
        public void SetProcessingMode(string selectedMode)
        {
            switch (selectedMode)
            {
                case "Cлои":
                    _layersForm.RepaintLayers();
                    break;
                case "Ч/Б":
                    _flowPanelImages.Controls.Clear();
                    break;
                default:
                    break;
            }
        }
    }
}
