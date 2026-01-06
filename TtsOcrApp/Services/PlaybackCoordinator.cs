using TtsOcrApp.Core.Services;

namespace TtsOcrApp.Services
{
    public class PlaybackCoordinator : IDisposable
    {
        private readonly IScreenCaptureService _screenCapture;
        private readonly IOcrService _ocr;
        private readonly Func<Rectangle> _getRegion;
        private readonly Action<string> _onTextReady;
        private readonly SynchronizationContext? _uiContext;

        private readonly object _gate = new();

        private CancellationTokenSource? _currentCts;
        private Task _worker = Task.CompletedTask;

        private PendingCapture? _pending;
        private bool _isRunning;

        private string? _lastProducedText;

        public PlaybackCoordinator(
            IScreenCaptureService screenCapture,
            IOcrService ocr,
            Func<Rectangle> getRegion,
            Action<string> onTextReady,
            SynchronizationContext? uiContext = null)
        {
            _screenCapture = screenCapture ?? throw new ArgumentNullException(nameof(screenCapture));
            _ocr = ocr ?? throw new ArgumentNullException(nameof(ocr));
            _getRegion = getRegion ?? throw new ArgumentNullException(nameof(getRegion));
            _onTextReady = onTextReady ?? throw new ArgumentNullException(nameof(onTextReady));
            _uiContext = uiContext;
        }

        public void RequestCapture()
        {
            var region = _getRegion();
            if (region.Width <= 0 || region.Height <= 0)
                return;

            Bitmap bmp;
            try
            {
                bmp = _screenCapture.Capture(region);
            }
            catch
            {
                return;
            }

            var fingerprint = ComputeFingerprint(bmp);

            PendingCapture? replaced = null;
            bool shouldStartWorker = false;

            lock (_gate)
            {
                if (!_isRunning)
                {
                    _isRunning = true;
                    shouldStartWorker = true;
                }
                else
                {
                    // Busy: only keep latest pending capture.
                    // Dedup: if user spammed same capture, ignore.
                    if (_pending != null && _pending.Fingerprint == fingerprint)
                    {
                        bmp.Dispose();
                        return;
                    }

                    replaced = _pending;
                }

                _pending = new PendingCapture(bmp, fingerprint);
            }

            replaced?.Dispose();

            if (shouldStartWorker)
            {
                _worker = Task.Run(RunWorkerLoop);
            }
        }

        public void Cancel()
        {
            PendingCapture? pendingToDispose = null;
            CancellationTokenSource? ctsToCancel = null;

            lock (_gate)
            {
                pendingToDispose = _pending;
                _pending = null;

                ctsToCancel = _currentCts;
                _currentCts = null;
            }

            pendingToDispose?.Dispose();
            ctsToCancel?.Cancel();
            ctsToCancel?.Dispose();
        }

        public void Dispose()
        {
            Cancel();
        }

        // ---------------- Worker loop ----------------

        private async Task RunWorkerLoop()
        {
            while (true)
            {
                PendingCapture? capture = null;

                lock (_gate)
                {
                    capture = _pending;
                    _pending = null;

                    if (capture == null)
                    {
                        _isRunning = false;
                        return;
                    }

                    _currentCts?.Dispose();
                    _currentCts = new CancellationTokenSource();
                }

                try
                {
                    using (capture)
                    {
                        var token = GetCurrentToken();

                        string text;
                        try
                        {
                            text = await _ocr.RecognizeTextAsync(capture.Bitmap, token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(text))
                        {
                            continue;
                        }

                        text = text.Trim();

                        // Text-based dedupe: if same as last produced, drop.
                        // This handles "same text from different region".
                        if (IsSameText(text, _lastProducedText))
                            continue;

                        _lastProducedText = text;

                        PostToUi(() => _onTextReady(text));
                    }
                }
                finally
                {
                    CleanupCurrentTokenSource();
                }
            }
        }

        private CancellationToken GetCurrentToken()
        {
            lock (_gate)
            {
                return _currentCts?.Token ?? CancellationToken.None;
            }
        }

        private void CleanupCurrentTokenSource()
        {
            lock (_gate)
            {
                _currentCts?.Dispose();
                _currentCts = null;
            }
        }

        private void PostToUi(Action action)
        {
            if (_uiContext == null)
            {
                action();
                return;
            }

            _uiContext.Post(_ => action(), null);
        }

        // ---------------- helper methods (keep at bottom) ----------------

        private static bool IsSameText(string a, string? b)
        {
            if (b == null) return false;

            var na = NormalizeText(a);
            var nb = NormalizeText(b);

            return string.Equals(na, nb, StringComparison.Ordinal);
        }

        private static string NormalizeText(string text)
        {
            text = text.Trim();

            Span<char> buffer = stackalloc char[text.Length];
            int idx = 0;
            bool prevSpace = false;

            for (int i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (ch == '\r' || ch == '\n' || ch == '\t')
                    ch = ' ';

                if (ch == ' ')
                {
                    if (prevSpace) continue;
                    prevSpace = true;
                    buffer[idx++] = ' ';
                    continue;
                }

                prevSpace = false;
                buffer[idx++] = ch;
            }

            return new string(buffer[..idx]);
        }

        private static ulong ComputeFingerprint(Bitmap bmp)
        {
            unchecked
            {
                ulong hash = 1469598103934665603UL;
                const ulong prime = 1099511628211UL;

                int w = bmp.Width;
                int h = bmp.Height;

                int stepX = Math.Max(1, w / 16);
                int stepY = Math.Max(1, h / 16);

                for (int y = 0; y < h; y += stepY)
                {
                    for (int x = 0; x < w; x += stepX)
                    {
                        var c = bmp.GetPixel(x, y);
                        int gray = (int)((0.299 * c.R) + (0.587 * c.G) + (0.114 * c.B));

                        hash ^= (byte)gray;
                        hash *= prime;
                    }
                }

                hash ^= (ulong)w;
                hash *= prime;
                hash ^= (ulong)h;
                hash *= prime;

                return hash;
            }
        }

        private sealed class PendingCapture : IDisposable
        {
            public PendingCapture(Bitmap bitmap, ulong fingerprint)
            {
                Bitmap = bitmap;
                Fingerprint = fingerprint;
            }

            public Bitmap Bitmap { get; }
            public ulong Fingerprint { get; }

            public void Dispose()
            {
                Bitmap.Dispose();
            }
        }
    }
}
