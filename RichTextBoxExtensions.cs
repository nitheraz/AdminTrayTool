using System.Runtime.InteropServices;

namespace AdminTrayTool
{
    // Top-level extension methods for RichTextBox to suspend/redraw
    public static class RichTextBoxExtensions
    {
        private const int WM_SETREDRAW = 0x0b;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static void BeginUpdate(this RichTextBox rtb) => SendMessage(rtb.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        public static void EndUpdate(this RichTextBox rtb) { SendMessage(rtb.Handle, WM_SETREDRAW, new IntPtr(1), IntPtr.Zero); rtb.Invalidate(); }
    }
}
