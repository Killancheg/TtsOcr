namespace TtsOcrApp.Services
{
    public interface IScreenCaptureService
    {
        Bitmap Capture(Rectangle region);
    }
}
