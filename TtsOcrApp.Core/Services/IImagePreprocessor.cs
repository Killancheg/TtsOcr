using System.Drawing;

namespace TtsOcrApp.Core.Services
{
    public interface IImagePreprocessor
    {
        Bitmap Preprocess(Bitmap source);
    }
}
