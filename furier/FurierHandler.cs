using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using labaphotoshop.workflow;

namespace labaphotoshop.furier
{
    internal class FurierHandler
    {
        private FurierForm _furierForm;
        private Label _infoText;

        public FurierHandler(FurierForm furierForm, Label infoText)
        {
            _furierForm = furierForm;
            _infoText = infoText;

            _furierForm.WorkBut.Click += OnClick!;
        }

        private void OnClick(object sender, EventArgs e)
        {
            RepaintAll();
        }

        private async void RepaintAll()
        {
            Stopwatch sw = Stopwatch.StartNew();

            var tokenSourse = new CancellationTokenSource();
            var ct = tokenSourse.Token;

            var loading = Task.Run(async () => {
                ct.ThrowIfCancellationRequested();
                for (; ;)
                {
                    _infoText.Invoke(() =>
                    {
                        _infoText.Text = sw.Elapsed.TotalSeconds.ToString();
                    });

                    await Task.Delay(500);

                    if (ct.IsCancellationRequested)
                    {
                        sw.Stop();
                        ct.ThrowIfCancellationRequested();
                    }
                }
            }, ct);

            await Task.Run(() => 
            {
                var inputImage = _furierForm.GetMainPictureImage();
                var (img, r, g, b) = FurierProcessor.ApplyFilterAndVisualize(inputImage, _furierForm.R1, _furierForm.R2, _furierForm.FurierFilterMode, _furierForm.Brightness);

                _furierForm.SetNewMainImage(Funcs.BitmapChangeFormatTo32(FurierImages.ReconstructImage(r, g, b)));
                _furierForm.SetFurierImage(Funcs.BitmapChangeFormatTo32(img));
            });

            tokenSourse.Cancel();
            tokenSourse.Dispose();
            _infoText.Text = $"ready {sw.Elapsed.TotalSeconds}";
        }
    }
}
