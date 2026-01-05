using System.Runtime.InteropServices;

namespace TtsOcrApp.Services
{
    public class GlobalHotkeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        private readonly IntPtr _windowHandle;
        private readonly int _hotkeyId;

        public event EventHandler? HotkeyPressed;

        public GlobalHotkeyService(
            IntPtr windowHandle,
            int hotkeyId,
            Keys key,
            bool ctrl,
            bool shift,
            bool alt)
        {
            _windowHandle = windowHandle;
            _hotkeyId = hotkeyId;

            uint modifiers = 0;
            if (ctrl) modifiers |= MOD_CONTROL;
            if (shift) modifiers |= MOD_SHIFT;
            if (alt) modifiers |= MOD_ALT;

            bool registered = RegisterHotKey(
                _windowHandle,
                _hotkeyId,
                modifiers,
                (uint)key);

            if (!registered)
            {
                throw new InvalidOperationException(
                    $"Failed to register hotkey: {modifiers}+{key}");
            }
        }

        public bool HandleMessage(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == _hotkeyId)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            UnregisterHotKey(_windowHandle, _hotkeyId);
        }

        // ---------- Win32 ----------

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            uint fsModifiers,
            uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(
            IntPtr hWnd,
            int id);
    }
}
