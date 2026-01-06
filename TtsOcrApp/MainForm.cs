using System.ComponentModel;
using TtsOcrApp.Core.Services;
using TtsOcrApp.Services;

namespace TtsOcrApp
{
    public partial class MainForm : Form
    {
        private GlobalHotkeyService? _captureHotkeyService;
        private GlobalHotkeyService? _cancelHotkeyService;
        private IScreenCaptureService _screenCaptureService;
        private PlaybackCoordinator? _playbackCoordinator;

        private IOcrService _ocrService;
        private Rectangle _selectedRegion = Rectangle.Empty;

        public MainForm()
        {
            InitializeComponent();

            if (IsDesignTime()) return;

            var tessdataPath = Path.Combine(
            AppContext.BaseDirectory,
            "tessdata");

            _ocrService = new TesseractOcrService(tessdataPath);

            _screenCaptureService = new GdiScreenCaptureService();

            _playbackCoordinator = new PlaybackCoordinator(
                screenCapture: _screenCaptureService,
                ocr: _ocrService,
                getRegion: () => _selectedRegion,
                onTextReady: text => txtOcrResult.Text = text,
                uiContext: SynchronizationContext.Current);
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

        private void btnCapture_Click(object sender, EventArgs e)
        {
            _playbackCoordinator?.RequestCapture();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (IsDesignTime())
                return;

            _captureHotkeyService = new GlobalHotkeyService(
                Handle,
                hotkeyId: 1,
                key: Keys.O,
                ctrl: true,
                shift: true,
                alt: false);

            _cancelHotkeyService = new GlobalHotkeyService(
                Handle,
                hotkeyId: 2,
                key: Keys.P,
                ctrl: true,
                shift: true,
                alt: false);

            _captureHotkeyService.HotkeyPressed += (_, _) =>
            {
                _playbackCoordinator?.RequestCapture();
            };

            _cancelHotkeyService.HotkeyPressed += (_, _) =>
            {
                _playbackCoordinator?.Cancel();
            };
        }

        protected override void WndProc(ref Message m)
        {
            if (_captureHotkeyService?.HandleMessage(ref m) == true)
                return;

            if (_cancelHotkeyService?.HandleMessage(ref m) == true)
                return;

            base.WndProc(ref m);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _captureHotkeyService?.Dispose();
            _cancelHotkeyService?.Dispose();
            _playbackCoordinator?.Dispose();
            _playbackCoordinator = null;
            base.OnFormClosed(e);
        }

        //helpers

        private static bool IsDesignTime()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }
    }
}
