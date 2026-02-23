using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public class SmoothScrollPanel : Panel
    {
        public SmoothScrollPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
            AutoScroll = true;
        }

        // ✅ réduit le flicker (attention: peut ralentir si énorme UI, mais en général ça aide)
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            // évite de repaint comme un malade
            this.SuspendLayout();
            base.OnScroll(se);
            this.ResumeLayout();
            this.Invalidate();
        }
    }
}