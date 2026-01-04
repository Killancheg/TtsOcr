using TtsOcrApp.Core.Services;

namespace TtsOcrApp
{
    public partial class MainForm : Form
    {
        private IOcrService _ocrService;
        private Rectangle _selectedRegion = Rectangle.Empty;

        public MainForm()
        {
            InitializeComponent();

            var tessdataPath = Path.Combine(
                AppContext.BaseDirectory,
                "tessdata");

            _ocrService = new TesseractOcrService(tessdataPath);
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
            if (_selectedRegion == Rectangle.Empty || _selectedRegion.Width <= 0 || _selectedRegion.Height <= 0)
            {
                txtOcrResult.Text = "No region selected. Click 'Select region' first.";
                return;
            }

            using var bmp = CaptureRegion(_selectedRegion);

            txtOcrResult.Text = "Recognizing text...";

            try
            {
                var text = await _ocrService.RecognizeTextAsync(bmp);
                txtOcrResult.Text = string.IsNullOrWhiteSpace(text)
                    ? "(No text detected)"
                    : text;
            }
            catch (Exception ex)
            {
                txtOcrResult.Text = $"OCR error: {ex.Message}";
            }
        }

        private static Bitmap CaptureRegion(Rectangle region)
        {
            var bmp = new Bitmap(region.Width, region.Height);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(region.Location, Point.Empty, region.Size);
            return bmp;
        }
    }
}
