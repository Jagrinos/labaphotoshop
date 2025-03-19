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
        public int WindowSize { get; set; } = 3;
        public double A { get; set; } = 0.5;
        public double K { get; set; } = 0.2;



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

            TableLayoutPanel valuesLayout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = modes.Length,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            Label valWindowSizeTxt = new()
            {
                Text = "Window Size",
            };
            TextBox valWindowSize = new()
            {
                Name = "windowSize",
                Text = "3",
            };
            valWindowSize.TextChanged += valChange!;

            Label valATxt = new()
            {
                Text = "a",
            };
            TextBox valA = new()
            {
                Name = "a",
                Text = "0,5",
            };
            valA.TextChanged += valChange!;

            Label valKTxt = new()
            {
                Text = "k",
            };
            TextBox valK = new()
            {
                Name = "k",
                Text = "0,2",
            };
            valK.TextChanged += valChange!;

            valuesLayout.Controls.Add(valWindowSizeTxt, 0, 0);
            valuesLayout.Controls.Add(valWindowSize, 1, 0);

            valuesLayout.Controls.Add(valATxt, 0, 1);
            valuesLayout.Controls.Add(valA, 1, 1);

            valuesLayout.Controls.Add(valKTxt, 0, 2);
            valuesLayout.Controls.Add(valK, 1, 2);

            _flowPanelImages.Controls.Add(tableLayout);
            _flowPanelImages.Controls.Add(valuesLayout);
        }

        private void valChange(object sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                switch (tb.Name)
                {
                    case "windowSize":
                        try
                        {
                            WindowSize = int.Parse(tb.Text);
                        }
                        catch
                        {
                            WindowSize = 3;
                        }
                        break;
                    case "a":
                        try
                        {
                            A = double.Parse(tb.Text);
                        }
                        catch
                        {
                            A = 0.5;
                        }
                        break;
                    case "k":
                        try
                        {
                            K = double.Parse(tb.Text);
                        }
                        catch
                        {
                            K = 0.2;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
