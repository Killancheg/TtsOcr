namespace TtsOcrApp.Services
{
    public class GdiScreenCaptureService : IScreenCaptureService
    {
        public Bitmap Capture(Rectangle region)
        {
            if (region.Width <= 0 || region.Height <= 0)
                throw new ArgumentException("Invalid capture region", nameof(region));

            var bmp = new Bitmap(region.Width, region.Height);

            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(region.Location, Point.Empty, region.Size);

            return bmp;
        }
    }
}
