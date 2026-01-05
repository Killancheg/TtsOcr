namespace TtsOcrApp
{
    public partial class RegionSelectorForm : Form
    {
        private Point? _startPoint;
        private Rectangle _currentRect;
        private Point _currentMousePosition;

        public Rectangle SelectedRegion { get; private set; } = Rectangle.Empty;

        public RegionSelectorForm()
        {
            InitializeComponent();
            ConfigureOverlayWindow();
            AutoScaleMode = AutoScaleMode.None;
        }

        private void ConfigureOverlayWindow()
        {
            FormBorderStyle = FormBorderStyle.None;

            var bounds = SystemInformation.VirtualScreen;
            Bounds = bounds;
            StartPosition = FormStartPosition.Manual;

            TopMost = true;
            ShowInTaskbar = false;

            DoubleBuffered = true;
            KeyPreview = true;

            Cursor = Cursors.Cross;

            Opacity = 0.35;

            MouseDown += RegionSelectorForm_MouseDown;
            MouseMove += RegionSelectorForm_MouseMove;
            MouseUp += RegionSelectorForm_MouseUp;
            KeyDown += RegionSelectorForm_KeyDown;
        }

        private void RegionSelectorForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            _startPoint = e.Location;
            _currentRect = Rectangle.Empty;
            Invalidate();
        }

        private void RegionSelectorForm_MouseMove(object sender, MouseEventArgs e)
        {
            _currentMousePosition = e.Location;

            if (_startPoint is not null)
            {
                _currentRect = CreateRect(_startPoint.Value, e.Location);
            }

            Invalidate();
        }

        private void RegionSelectorForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (_startPoint is null) return;

            _currentRect = CreateRect(_startPoint.Value, e.Location);
            _startPoint = null;

            SelectedRegion = RectangleToScreen(_currentRect);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void RegionSelectorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_currentRect.Width <= 0 || _currentRect.Height <= 0)
                return;

            // "Clear" effect by overdrawing lighter area
            using var clearBrush = new SolidBrush(Color.FromArgb(40, 255, 255, 255));
            e.Graphics.FillRectangle(clearBrush, _currentRect);

            using var pen = new Pen(Color.FromArgb(240, 255, 0, 0), 4);
            e.Graphics.DrawRectangle(pen, _currentRect);

            DrawCursorCrosshair(e);
        }

        private static Rectangle CreateRect(Point a, Point b)
        {
            var x = Math.Min(a.X, b.X);
            var y = Math.Min(a.Y, b.Y);
            var w = Math.Abs(a.X - b.X);
            var h = Math.Abs(a.Y - b.Y);
            return new Rectangle(x, y, w, h);
        }

        private void DrawCursorCrosshair(PaintEventArgs e)
        {
            using var guidePen = new Pen(Color.LightGreen, 1)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
            };

            e.Graphics.DrawLine(
                guidePen,
                new Point(_currentMousePosition.X, 0),
                new Point(_currentMousePosition.X, Height)
            );

            e.Graphics.DrawLine(
                guidePen,
                new Point(0, _currentMousePosition.Y),
                new Point(Width, _currentMousePosition.Y)
            );
        }
    }
}
