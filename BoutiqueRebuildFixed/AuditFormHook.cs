using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public static class AuditFormHook
    {
        public static void Attach(Form f, string module = null)
        {
            if (f == null) return;

            string mod = module ?? (string.IsNullOrWhiteSpace(f.Text) ? f.Name : f.Text);

            f.Shown += (_, __) =>
                AuditLogger.Log("VIEW", $"Ouverture {mod}");

            f.FormClosed += (_, __) =>
                AuditLogger.Log("VIEW", $"Fermeture {mod}");
        }
    }
}