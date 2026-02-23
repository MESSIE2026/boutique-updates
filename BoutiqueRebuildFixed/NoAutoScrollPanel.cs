using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public class NoAutoScrollPanel : Panel
    {
        // ✅ Empêche WinForms de scroller automatiquement pour rendre visible le contrôle focus
        protected override Point ScrollToControl(Control activeControl)
        {
            // Empêche AutoScroll de bouger quand un contrôle reçoit le focus
            return this.DisplayRectangle.Location;
        }
    }
}
