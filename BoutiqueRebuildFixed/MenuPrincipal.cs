using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class MenuPrincipal : UserControl
    {
        public MenuPrincipal()
        {
            InitializeComponent();
            InitialiserMenu();
        }

        private void MenuPrincipal_Load(object sender, EventArgs e)
        {

        }
        private void InitialiserMenu()
        {
            menuStripPrincipal.Items.Clear();

            // ===== MENU FICHIER =====
            ToolStripMenuItem mFichier = new ToolStripMenuItem("Fichier");
            mFichier.DropDownItems.Add("Nouveau", null, (s, e) => ApplicationCommands.Instance.Nouveau());
            mFichier.DropDownItems.Add("Ouvrir", null, (s, e) => ApplicationCommands.Instance.Ouvrir());
            mFichier.DropDownItems.Add("Enregistrer", null, (s, e) => ApplicationCommands.Instance.Enregistrer(FindForm()));
            mFichier.DropDownItems.Add("Enregistrer sous", null, (s, e) => ApplicationCommands.Instance.EnregistrerSous(FindForm()));
            mFichier.DropDownItems.Add(new ToolStripSeparator());
            mFichier.DropDownItems.Add("Imprimer", null, (s, e) => ApplicationCommands.Instance.Imprimer(FindForm()));
            mFichier.DropDownItems.Add("Aperçu impression", null, (s, e) => ApplicationCommands.Instance.ApercuImpression());
            mFichier.DropDownItems.Add(new ToolStripSeparator());
            mFichier.DropDownItems.Add("Quitter", null, (s, e) => ApplicationCommands.Instance.Quitter());

            // ===== MENU AFFICHAGE =====
            ToolStripMenuItem mAffichage = new ToolStripMenuItem("Affichage");
            mAffichage.DropDownItems.Add("Accueil", null, (s, e) => ApplicationCommands.Instance.Accueil());
            mAffichage.DropDownItems.Add("Historique", null, (s, e) => ApplicationCommands.Instance.Historique());

            // ===== MENU AIDE =====
            ToolStripMenuItem mAide = new ToolStripMenuItem("Aide");
            mAide.DropDownItems.Add("Informations", null, (s, e) => ApplicationCommands.Instance.Informations());

            menuStripPrincipal.Items.AddRange(new ToolStripItem[]
            {
            mFichier,
            mAffichage,
            mAide
            });
        }
    }
}
