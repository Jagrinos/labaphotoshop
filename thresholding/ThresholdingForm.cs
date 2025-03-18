using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace labaphotoshop.thresholding
{
    internal class ThresholdingForm
    {
        private FlowLayoutPanel _flowPanelImages;
        public List<RadioButton> Buttons = [];

        public ThresholdingForm(FlowLayoutPanel flowPanelImages)
        {
            _flowPanelImages = flowPanelImages;

            string[] modes = ["Градации серого", "Критерий Гаврилова", "Критерий Отсо", "Критерий Ниблека", "Критерий Сауволы", "Критерий Кристиана Вульфа", "Критерий Брэдли-Рота"];

            TableLayoutPanel tableLayout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = modes.Length,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            for (int i = 0; i < modes.Length; i++)
            {
                var mode = modes[i];
                RadioButton button = new()
                {
                    Text = mode,
                    AutoSize = true,
                };

                Buttons.Add(button);

                tableLayout.Controls.Add(button, 0, i);
            }
            _flowPanelImages.Controls.Add(tableLayout);
        }
    }
}
