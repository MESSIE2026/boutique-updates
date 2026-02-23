using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;        // pour Color
using System.Windows.Forms.VisualStyles;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    internal class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColorTable()) { }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = Color.White;   // 🔥 TEXTE FORCÉ
            base.OnRenderItemText(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Color bg = e.Item.Selected
                ? Color.FromArgb(63, 63, 70)
                : Color.FromArgb(45, 45, 48);

            using (SolidBrush brush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // ❌ aucune bordure
        }
    }

    internal class DarkColorTable : ProfessionalColorTable
    {
        public override Color MenuStripGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color MenuStripGradientEnd => Color.FromArgb(45, 45, 48);

        public override Color MenuItemSelected => Color.FromArgb(63, 63, 70);
        public override Color MenuItemBorder => Color.FromArgb(63, 63, 70);

        public override Color MenuItemPressedGradientBegin => Color.FromArgb(28, 28, 28);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(28, 28, 28);
    }
}