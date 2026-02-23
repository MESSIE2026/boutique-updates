using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    internal class ApplicationCommands
    {
        private static readonly Lazy<ApplicationCommands> _instance =
          new Lazy<ApplicationCommands>(() => new ApplicationCommands());

        public static ApplicationCommands Instance => _instance.Value;

        private ApplicationCommands() { }

        // ================= COMMANDES =================

        public void Nouveau()
        {
            MessageBox.Show("Nouveau document");
        }

        public void Ouvrir()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Ouverture : " + ofd.FileName);
            }
        }

        public void Enregistrer(Form context)
        {
            MessageBox.Show($"Enregistrement depuis : {context.Name}");
        }

        public void EnregistrerSous(Form context)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Enregistré sous : " + sfd.FileName);
            }
        }

        public void Imprimer(Form context)
        {
            MessageBox.Show($"Impression depuis : {context.Name}");
        }

        public void ApercuImpression()
        {
            MessageBox.Show("Aperçu avant impression");
        }

        public void Accueil()
        {
            MessageBox.Show("Retour à l'accueil");
        }

        public void Historique()
        {
            MessageBox.Show("Historique");
        }

        public void Informations()
        {
            MessageBox.Show("ZAIRE MODE SARL\nVersion 1.0", "Informations");
        }

        public void Quitter()
        {
            Application.Exit();
        }
    }
}
