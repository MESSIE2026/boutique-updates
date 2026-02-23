using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed.Localization
{
    public static class LangManager
    {
        public static void ApplyCulture(string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
                cultureName = "fr-FR";

            CultureInfo culture;
            try { culture = new CultureInfo(cultureName); }
            catch { culture = new CultureInfo("fr-FR"); }

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public static void RefreshUI(Form f)
        {
            if (f == null || f.IsDisposed) return;

            // RTL auto (Arabe/Hébreu, etc.)
            ApplyRTLIfNeeded(f);

            var resources = new ComponentResourceManager(f.GetType());
            resources.ApplyResources(f, "$this");
            ApplyResourcesRecursive(resources, f.Controls);

            RefreshToolStrips(resources, f);
        }

        private static void ApplyResourcesRecursive(ComponentResourceManager res, Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                res.ApplyResources(c, c.Name);
                if (c.HasChildren) ApplyResourcesRecursive(res, c.Controls);
            }
        }

        private static void RefreshToolStrips(ComponentResourceManager res, Form f)
        {
            foreach (Control c in f.Controls)
            {
                if (c is MenuStrip ms)
                {
                    foreach (ToolStripItem it in ms.Items)
                        ApplyToolStripItem(res, it);
                }
                else if (c is ToolStrip ts)
                {
                    foreach (ToolStripItem it in ts.Items)
                        ApplyToolStripItem(res, it);
                }
            }
        }

        private static void ApplyToolStripItem(ComponentResourceManager res, ToolStripItem item)
        {
            if (item == null) return;

            res.ApplyResources(item, item.Name);

            if (item is ToolStripDropDownItem dd && dd.DropDownItems != null)
            {
                foreach (ToolStripItem sub in dd.DropDownItems)
                    ApplyToolStripItem(res, sub);
            }
        }

        private static void ApplyRTLIfNeeded(Form f)
        {
            var ui = Thread.CurrentThread.CurrentUICulture;
            bool rtl = ui.TextInfo.IsRightToLeft;

            f.RightToLeft = rtl ? RightToLeft.Yes : RightToLeft.No;

            // Important : certains forms ont cette propriété
            try
            {
                f.RightToLeftLayout = rtl;
            }
            catch { /* certains types de form n'ont pas */ }
        }
    }
}