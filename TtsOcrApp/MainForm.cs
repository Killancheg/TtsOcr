using System.ComponentModel;
using TtsOcrApp.Core.Services;
using TtsOcrApp.Services;

namespace TtsOcrApp
{
    public partial class MainForm : Form
    {
        private GlobalHotkeyService? _hotkeyService;

        private IOcrService _ocrService;
        private Rectangle _selectedRegion = Rectangle.Empty;

        public MainForm()
        {
            InitializeComponent();

            var tessdataPath = Path.Combine(
                AppContext.BaseDirectory,
                "tessdata");

            if (!IsDesignTime())
            {
                _ocrService = new TesseractOcrService(tessdataPath);
            }
        }

        private void btnSelectRegion_Click(object sender, EventArgs e)
        {
            using var selector = new RegionSelectorForm();
            var result = selector.ShowDialog(this);

            if (result != DialogResult.OK)
            {
                txtOcrResult.Text = "Region selection canceled.";
                return;
            }

            _selectedRegion = selector.SelectedRegion;

            txtOcrResult.Text =
                $"Selected region: X={_selectedRegion.X}, Y={_selectedRegion.Y}, W={_selectedRegion.Width}, H={_selectedRegion.Height}";
        }

        private async void btnCapture_Click(object sender, EventArgs e)
        {
            TriggerCapture();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (IsDesignTime())
                return;

            _hotkeyService = new GlobalHotkeyService(
                Handle,
                hotkeyId: 1,
                key: Keys.O,
                ctrl: true,
                shift: true,
                alt: false);

            _hotkeyService.HotkeyPressed += (_, _) =>
            {
                TriggerCapture();
            };
        }

        protected override void WndProc(ref Message m)
        {
            if (_hotkeyService?.HandleMessage(ref m) == true)
                return;

            base.WndProc(ref m);
        }

        private async void TriggerCapture()
        {
            if (_selectedRegion == Rectangle.Empty)
                return;

            using var bmp = CaptureRegion(_selectedRegion);

            try
            {
                var text = await _ocrService.RecognizeTextAsync(bmp);

                BeginInvoke(() =>
                {
                    txtOcrResult.Text = string.IsNullOrWhiteSpace(text)
                        ? "(No text detected)"
                        : text;
                });
            }
            catch (Exception ex)
            {
                BeginInvoke(() =>
                {
                    txtOcrResult.Text = $"OCR error: {ex.Message}";
                });
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _hotkeyService?.Dispose();
            base.OnFormClosed(e);
        }

        //helpers

        private static Bitmap CaptureRegion(Rectangle region)
        {
            var bmp = new Bitmap(region.Width, region.Height);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(region.Location, Point.Empty, region.Size);
            return bmp;
        }

        private static bool IsDesignTime()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }
    }
}
