using System.Drawing.Drawing2D;

namespace AdminTrayTool.UI
{
    public static class HudTheme
    {
        public static readonly Color AccentCyan = Color.FromArgb(120, 230, 255);
        public static readonly Color GradientTop = Color.FromArgb(28, 40, 90);
        public static readonly Color GradientBottom = Color.FromArgb(14, 18, 45);
        public static readonly Color TextWhite = Color.White;
        public static readonly Color TextMuted = Color.FromArgb(150, 190, 220);

        public const int DefaultChamfer = 14;
        public const int DefaultBorderWidth = 2;

        /// <summary>
        /// Builds a chamfered (angled-corner) rectangle path, matching the
        /// reference HUD frame style - top-left and bottom-right corners cut.
        /// </summary>
        public static GraphicsPath BuildChamferedPath(Rectangle bounds, int chamfer)
        {
            var path = new GraphicsPath();
            int x = bounds.X, y = bounds.Y, w = bounds.Width, h = bounds.Height;

            path.AddLine(x + chamfer, y, x + w - chamfer, y);              // top edge
            path.AddLine(x + w - chamfer, y, x + w, y + chamfer);          // top-right cut
            path.AddLine(x + w, y + chamfer, x + w, y + h);                // right edge
            path.AddLine(x + w, y + h, x + chamfer, y + h);                // bottom edge
            path.AddLine(x + chamfer, y + h, x, y + h - chamfer);          // bottom-left cut
            path.AddLine(x, y + h - chamfer, x, y);                        // left edge
            path.CloseFigure();

            return path;
        }

        /// <summary>
        /// Paints a chamfered panel background: vertical gradient fill + glowing border.
        /// Call this from a Panel's Paint event handler.
        /// </summary>
        public static void PaintPanel(Graphics g, Rectangle bounds, int chamfer = DefaultChamfer)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            using var path = BuildChamferedPath(rect, chamfer);

            using var fill = new LinearGradientBrush(
                bounds, GradientTop, GradientBottom, LinearGradientMode.Vertical);
            g.FillPath(fill, path);

            using var borderPen = new Pen(AccentCyan, DefaultBorderWidth);
            g.DrawPath(borderPen, path);
        }

        /// <summary>
        /// Paints a chamfered button: filled background + border, with hover/pressed variants.
        /// </summary>
        public static void PaintButton(Graphics g, Rectangle bounds, bool isHovered, bool isPressed, int chamfer = 8)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            using var path = BuildChamferedPath(rect, chamfer);

            Color fillColor = isPressed
                ? Color.FromArgb(60, 90, 140)
                : isHovered
                    ? Color.FromArgb(45, 70, 115)
                    : Color.FromArgb(30, 45, 75);

            using var fill = new SolidBrush(fillColor);
            g.FillPath(fill, path);

            using var borderPen = new Pen(AccentCyan, 1.5f);
            g.DrawPath(borderPen, path);
        }
    }
}