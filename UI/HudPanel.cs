using AdminTrayTool.UI;

namespace AdminTrayTool
{
    public class HudPanel : Panel
    {
        public HudPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = Color.Transparent;
        }

        protected override void OnPaintBackground(
            PaintEventArgs e)
        {
            // Paint the parent background first so the antialiased
            // chamfered edges blend into the actual UI background
            // instead of showing black pixels.
            base.OnPaintBackground(e);
        }

        protected override void OnPaint(
            PaintEventArgs e)
        {
            HudTheme.PaintPanel(
                e.Graphics,
                ClientRectangle);

            // Paint child controls normally.
            base.OnPaint(e);
        }
    }
}
