using System.Drawing.Drawing2D;

namespace AdminTrayTool.UI
{
    public static class HudTheme
    {
        public static readonly Color AccentCyan =
            Color.FromArgb(120, 230, 255);

        public static readonly Color GradientTop =
            Color.FromArgb(28, 40, 90);

        public static readonly Color GradientBottom =
            Color.FromArgb(14, 18, 45);

        public static readonly Color TextWhite =
            Color.White;

        public static readonly Color TextMuted =
            Color.FromArgb(150, 190, 220);

        public const int DefaultChamfer = 14;
        public const int DefaultBorderWidth = 2;

        // ============================================================
        // CHAMFERED PANEL
        // ============================================================

        public static GraphicsPath BuildChamferedPath(
            Rectangle bounds,
            int chamfer)
        {
            var path = new GraphicsPath();

            int x = bounds.X;
            int y = bounds.Y;
            int w = bounds.Width;
            int h = bounds.Height;

            path.AddLine(
                x + chamfer,
                y,
                x + w - chamfer,
                y);

            path.AddLine(
                x + w - chamfer,
                y,
                x + w,
                y + chamfer);

            path.AddLine(
                x + w,
                y + chamfer,
                x + w,
                y + h);

            path.AddLine(
                x + w,
                y + h,
                x + chamfer,
                y + h);

            path.AddLine(
                x + chamfer,
                y + h,
                x,
                y + h - chamfer);

            path.AddLine(
                x,
                y + h - chamfer,
                x,
                y);

            path.CloseFigure();

            return path;
        }

        // ============================================================
        // HUD PANEL
        // ============================================================

        public static void PaintPanel(
            Graphics g,
            Rectangle bounds,
            int chamfer = DefaultChamfer)
        {
            g.SmoothingMode =
                SmoothingMode.AntiAlias;

            var rect =
                new Rectangle(
                    bounds.X,
                    bounds.Y,
                    bounds.Width - 1,
                    bounds.Height - 1);

            using var path =
                BuildChamferedPath(
                    rect,
                    chamfer);

            using var fill =
                new LinearGradientBrush(
                    bounds,
                    GradientTop,
                    GradientBottom,
                    LinearGradientMode.Vertical);

            g.FillPath(
                fill,
                path);

            using var borderPen =
                new Pen(
                    AccentCyan,
                    DefaultBorderWidth);

            g.DrawPath(
                borderPen,
                path);
        }

        // ============================================================
        // 3D RECTANGULAR BUTTON
        // ============================================================

        public static void PaintButton(
            Graphics g,
            Rectangle bounds,
            bool isHovered,
            bool isPressed,
            bool isEnabled)
        {
            if (bounds.Width <= 3 ||
                bounds.Height <= 3)
            {
                return;
            }

            g.SmoothingMode =
                SmoothingMode.AntiAlias;

            g.PixelOffsetMode =
                PixelOffsetMode.HighQuality;

            // --------------------------------------------------------
            // BUTTON GEOMETRY
            // --------------------------------------------------------

            int depth = isPressed ? 1 : 3;

            Rectangle faceRect =
                new(
                    bounds.X + 1,
                    bounds.Y + 1,
                    bounds.Width - 3,
                    bounds.Height - depth - 2);

            if (faceRect.Width <= 2 ||
                faceRect.Height <= 2)
            {
                return;
            }

            // --------------------------------------------------------
            // 3D DEPTH
            // --------------------------------------------------------

            if (depth > 0)
            {
                Rectangle depthRect =
                    new(
                        bounds.X + 2,
                        bounds.Y + depth + 1,
                        bounds.Width - 4,
                        bounds.Height - depth - 2);

                using var depthBrush =
                    new SolidBrush(
                        Color.FromArgb(
                            8,
                            10,
                            22));

                g.FillRectangle(
                    depthBrush,
                    depthRect);
            }

            // --------------------------------------------------------
            // BUTTON COLOURS
            // --------------------------------------------------------

            Color edgeColor;
            Color centreColor;

            if (!isEnabled)
            {
                edgeColor =
                    Color.FromArgb(
                        35,
                        42,
                        60);

                centreColor =
                    Color.FromArgb(
                        22,
                        27,
                        42);
            }
            else if (isPressed)
            {
                edgeColor =
                    Color.FromArgb(
                        55,
                        80,
                        120);

                centreColor =
                    Color.FromArgb(
                        25,
                        38,
                        65);
            }
            else if (isHovered)
            {
                edgeColor =
                    Color.FromArgb(
                        70,
                        105,
                        155);

                centreColor =
                    Color.FromArgb(
                        30,
                        48,
                        80);
            }
            else
            {
                edgeColor =
                    Color.FromArgb(
                        45,
                        62,
                        100);

                centreColor =
                    Color.FromArgb(
                        22,
                        31,
                        55);
            }

            // --------------------------------------------------------
            // BUTTON FACE
            //
            // Use a path gradient so the edges are lighter and the
            // centre gradually becomes darker.
            // --------------------------------------------------------

            using var facePath =
                new GraphicsPath();

            facePath.AddRectangle(
                faceRect);

            using var faceBrush =
                new PathGradientBrush(
                    facePath);

            faceBrush.CenterPoint =
                new PointF(
                    faceRect.Left +
                    (faceRect.Width / 2f),
                    faceRect.Top +
                    (faceRect.Height / 2f));

            faceBrush.CenterColor =
                centreColor;

            faceBrush.SurroundColors =
                [edgeColor];

            g.FillRectangle(
                faceBrush,
                faceRect);

            // --------------------------------------------------------
            // TOP / LEFT HIGHLIGHT
            // --------------------------------------------------------

            if (isEnabled)
            {
                using var highlightPen =
                    new Pen(
                        isHovered
                            ? Color.FromArgb(
                                190,
                                AccentCyan)
                            : Color.FromArgb(
                                120,
                                AccentCyan),
                        1f);

                g.DrawLine(
                    highlightPen,
                    faceRect.Left,
                    faceRect.Top,
                    faceRect.Right,
                    faceRect.Top);

                g.DrawLine(
                    highlightPen,
                    faceRect.Left,
                    faceRect.Top,
                    faceRect.Left,
                    faceRect.Bottom);
            }

            // --------------------------------------------------------
            // BOTTOM / RIGHT BEVEL
            // --------------------------------------------------------

            Color bevelColor =
                isEnabled
                    ? Color.FromArgb(
                        150,
                        5,
                        10,
                        25)
                    : Color.FromArgb(
                        100,
                        5,
                        8,
                        15);

            using var bevelPen =
                new Pen(
                    bevelColor,
                    1f);

            g.DrawLine(
                bevelPen,
                faceRect.Left,
                faceRect.Bottom,
                faceRect.Right,
                faceRect.Bottom);

            g.DrawLine(
                bevelPen,
                faceRect.Right,
                faceRect.Top,
                faceRect.Right,
                faceRect.Bottom);

            // --------------------------------------------------------
            // OUTER EDGE
            // --------------------------------------------------------

            Color borderColor;
            if (!isEnabled)
            {
                borderColor = Color.FromArgb(
                                    60,
                                    80,
                                    100);
            }
            else if (isHovered)
            {
                borderColor = AccentCyan;
            }
            else
            {
                borderColor = Color.FromArgb(
                                        90,
                                        AccentCyan);
            }

            using var borderPen =
                new Pen(
                    borderColor,
                    1f);

            Rectangle borderRect =
                new(
                    bounds.X,
                    bounds.Y,
                    bounds.Width - 1,
                    bounds.Height - 1);

            g.DrawRectangle(
                borderPen,
                borderRect);
        }
    }
}
