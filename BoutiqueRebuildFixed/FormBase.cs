using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BoutiqueRebuildFixed.Localization;

namespace BoutiqueRebuildFixed
{
    public class FormBase : Form
    {
        protected virtual string NomModule
        {
            get { return string.IsNullOrWhiteSpace(Text) ? Name : Text; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                BoutiqueRebuildFixed.Localization.LangManager.ApplyCulture(ConfigSysteme.Langue);
                BoutiqueRebuildFixed.Localization.TraductionManager.LoadFromCulture(ConfigSysteme.Langue);
                BoutiqueRebuildFixed.Localization.TraductionManager.ApplyToForm(this);
            }
            catch { }

            ConfigSysteme.OnLangueChange -= HandleLangueChange;
            ConfigSysteme.OnLangueChange += HandleLangueChange;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            AuditLogger.Log("VIEW", "Ouverture " + NomModule);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // ✅ 3) Se désabonner (important)
            ConfigSysteme.OnLangueChange -= HandleLangueChange;
            ConfigSysteme.OnThemeChange -= HandleThemeChange;

            AuditLogger.Log("VIEW", "Fermeture " + NomModule);
            base.OnFormClosed(e);
        }

        private void HandleLangueChange()
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(HandleLangueChange));
                return;
            }

            try
            {
                BoutiqueRebuildFixed.Localization.LangManager.ApplyCulture(ConfigSysteme.Langue);

                // ✅ RELOAD JSON
                BoutiqueRebuildFixed.Localization.TraductionManager.LoadFromCulture(ConfigSysteme.Langue);

                // ✅ APPLY sur tous les forms
                BoutiqueRebuildFixed.Localization.TraductionManager.ApplyToAllOpenForms();
            }
            catch { }
        }


        private void HandleThemeChange()
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(HandleThemeChange));
                return;
            }

            try
            {
                // ✅ Ici, branche ton système de thème
                // Exemple si tu as une méthode globale :
                // ThemeManager.ApplyTheme(this, ConfigSysteme.Theme);

                // Si tu n'as pas encore ThemeManager, laisse vide pour l'instant.
            }
            catch { /* ignore */ }
        }
    }
}