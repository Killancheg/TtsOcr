using System.Drawing;
using Tesseract;

namespace TtsOcrApp.Core.Services
{
    public sealed class TesseractOcrService : IOcrService, IDisposable
    {
        private readonly TesseractEngine _engine;

        public TesseractOcrService(string tessdataPath, string language = "eng")
        {
            if (string.IsNullOrWhiteSpace(tessdataPath))
                throw new ArgumentException("tessdataPath is required", nameof(tessdataPath));

            _engine = new TesseractEngine(
                tessdataPath,
                language,
                EngineMode.Default);
        }

        public Task<string> RecognizeTextAsync(
            Bitmap image,
            CancellationToken cancellationToken = default)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var pix = PixConverter.ToPix(image);
                using var page = _engine.Process(pix);

                return page.GetText()?.Trim() ?? string.Empty;
            }, cancellationToken);
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}
