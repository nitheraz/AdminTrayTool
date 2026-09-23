using AdminTrayTool.UI;

namespace AdminTrayTool
{
    public class HudPanel : Panel
    {
        public HudPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = System.Drawing.Color.Transparent;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Intentionally do nothing.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            HudTheme.PaintPanel(e.Graphics, ClientRectangle);
            base.OnPaint(e);
        }
    }
}