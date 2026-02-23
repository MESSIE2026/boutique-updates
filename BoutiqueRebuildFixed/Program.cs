using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BoutiqueRebuildFixed;

namespace BoutiqueRebuildFixed
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1) Charger config (ProgramData)
            ConfigSysteme.ChargerConfig();

            // 2) Check update AVANT SQL
            try { UpdateLauncher.TryLaunchUpdaterIfNeeded(); }
            catch { /* ignore */ }

            // 3) Tester SQL au démarrage
            string err;
            if (!ConfigSysteme.TryTestConnexion(out err))
            {
                MessageBox.Show(
                    "La base de données n'est pas configurée ou n'est pas accessible.\n\n" +
                    "Détail : " + err + "\n\n" +
                    "Clique OK pour ouvrir la Configuration SQL.",
                    "Configuration SQL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                using (var f = new FormConfigurationSysteme())
                {
                    f.StartPosition = FormStartPosition.CenterScreen;
                    f.ShowDialog();
                }

                // Re-test
                if (!ConfigSysteme.TryTestConnexion(out err))
                {
                    MessageBox.Show(
                        "Connexion SQL toujours impossible.\n\n" + err +
                        "\n\nL'application va se fermer.",
                        "SQL",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            // 4) Initialiser modules
            string errInit;
            if (!ConfigSysteme.InitialiserModulesSiNecessaire(ConfigSysteme.GetModulesFormMain(), out errInit))
            {
                MessageBox.Show(
                    "Impossible d'initialiser les modules.\n\n" + errInit,
                    "SQL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // 5) Démarrage normal
            Application.Run(new FormLogin());
        }
    }
}