using System.Drawing;
using Tesseract;

namespace TtsOcrApp.Core.Services
{
    public sealed class TesseractOcrService : IOcrService, IDisposable
    {
        private readonly TesseractEngine _engine;
        private readonly IImagePreprocessor _preprocessor;

        public TesseractOcrService(string tessdataPath, string language = "eng", IImagePreprocessor? preprocessor = null)
        {
            if (string.IsNullOrWhiteSpace(tessdataPath))
                throw new ArgumentException("tessdataPath is required", nameof(tessdataPath));

            _engine = new TesseractEngine(
                tessdataPath,
                language,
                EngineMode.Default);

            _preprocessor = preprocessor ?? new BasicOcrPreprocessor();
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

                using var processed = _preprocessor.Preprocess(image);
                using var pix = PixConverter.ToPix(processed);
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
