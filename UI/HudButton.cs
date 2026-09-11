using AdminTrayTool.UI;
using System.Drawing;
using System.Windows.Forms;

namespace AdminTrayTool
{
    public class HudButton : Button
    {
        private bool _isHovered;

        public HudButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = HudTheme.TextWhite;
            Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Cursor = Cursors.Hand;

            MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
            MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT - forces real background compositing
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Intentionally do nothing - prevents WinForms' default background
            // fill from fighting with the transparent compositing above.
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            HudTheme.PaintButton(pevent.Graphics, ClientRectangle, _isHovered, false);

            TextRenderer.DrawText(
                pevent.Graphics, Text, Font, ClientRectangle, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}