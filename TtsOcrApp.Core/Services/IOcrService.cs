using System.Drawing;

namespace TtsOcrApp.Core.Services
{
    public interface IOcrService
    {
        Task<string> RecognizeTextAsync(
            Bitmap image,
            CancellationToken cancellationToken = default);
    }
}
