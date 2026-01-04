using System.Drawing;
using System.Drawing.Imaging;

namespace TtsOcrApp.Core.Services
{
    public sealed class BasicOcrPreprocessor : IImagePreprocessor
    {
        public Bitmap Preprocess(Bitmap source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using var scaled = Scale(source, 2.0f);

            return Binarize(scaled);
        }

        // ---------- helper methods ----------

        private static Bitmap Scale(Bitmap source, float factor)
        {
            Bitmap result = new(
                width: (int)(source.Width * factor),
                height: (int)(source.Height * factor),
                format: PixelFormat.Format24bppRgb);

            using var g = Graphics.FromImage(result);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(source, 0, 0, result.Width, result.Height);

            return result;
        }

        private static Bitmap Binarize(Bitmap source)
        {
            var result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    var c = source.GetPixel(x, y);

                    // Grayscale (luma)
                    int gray = (int)((0.299 * c.R) + (0.587 * c.G) + (0.114 * c.B));

                    // Simple contrast boost
                    gray = gray > 128 ? Math.Min(255, gray + 40) : Math.Max(0, gray - 40);

                    // Threshold
                    var bw = gray > 160 ? Color.White : Color.Black;

                    result.SetPixel(x, y, bw);
                }
            }

            return result;
        }
    }
}
