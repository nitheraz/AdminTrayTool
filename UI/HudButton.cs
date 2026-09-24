using AdminTrayTool.UI;

namespace AdminTrayTool
{
    public class HudButton : Button
    {
        private bool _isHovered;
        private bool _isPressed;

        public HudButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;

            BackColor = HudTheme.GradientBottom;
            ForeColor = HudTheme.TextWhite;

            Font = new Font(
                "Segoe UI",
                9F,
                FontStyle.Bold);

            Cursor = Cursors.Hand;

            MouseEnter += OnMouseEnterButton;
            MouseLeave += OnMouseLeaveButton;
            MouseDown += OnMouseDownButton;
            MouseUp += OnMouseUpButton;
        }

        private void OnMouseEnterButton(
            object? sender,
            EventArgs e)
        {
            if (!Enabled)
                return;

            _isHovered = true;
            Invalidate();
        }

        private void OnMouseLeaveButton(
            object? sender,
            EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        private void OnMouseDownButton(
            object? sender,
            MouseEventArgs e)
        {
            if (!Enabled || e.Button != MouseButtons.Left)
                return;

            _isPressed = true;
            Invalidate();
        }

        private void OnMouseUpButton(
            object? sender,
            MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            _isPressed = false;
            Invalidate();
        }

        protected override void OnEnabledChanged(
            EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;

            base.OnEnabledChanged(e);

            Invalidate();
        }

        protected override void OnPaintBackground(
            PaintEventArgs pevent)
        {
            using SolidBrush backgroundBrush =
                new SolidBrush(HudTheme.GradientBottom);

            pevent.Graphics.FillRectangle(
                backgroundBrush,
                ClientRectangle);
        }

        protected override void OnPaint(
            PaintEventArgs pevent)
        {
            Rectangle bounds = ClientRectangle;

            if (bounds.Width <= 2 || bounds.Height <= 2)
                return;

            HudTheme.PaintButton(
                pevent.Graphics,
                bounds,
                _isHovered,
                _isPressed,
                Enabled);

            Color textColor = Enabled
                ? ForeColor
                : Color.FromArgb(100, ForeColor);

            TextRenderer.DrawText(
                pevent.Graphics,
                Text,
                Font,
                bounds,
                textColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);
        }
    }
}
