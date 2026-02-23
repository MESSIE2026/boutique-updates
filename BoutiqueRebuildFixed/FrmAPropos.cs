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
    public partial class FrmAPropos : Form
    {
        // ============================
        // CONTROLES
        // ============================
        private Label lblTitre;
        private Label lblDescription;
        private GroupBox grpInfos;
        private Label lblNom;
        private Label lblTelephone;
        private Label lblEmail;
        private Label lblVille;
        private Label lblPays;
        private Label lblReseaux;
        private Button btnFermer;

        public FrmAPropos()
        {
            InitialiserFormulaire();
            ConstruireUI();
        }

        // ============================
        // CONFIGURATION DU FORMULAIRE
        // ============================
        private void InitialiserFormulaire()
        {
            Text = "À propos - MESSIE MATALA POS";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Size = new Size(520, 460); // ✅ un peu plus haut (réseaux + site)
            BackColor = Color.White;
        }

        // ============================
        // CONSTRUCTION DE L'INTERFACE
        // ============================
        private void ConstruireUI()
        {
            // ----- TITRE -----
            lblTitre = new Label
            {
                Text = "MESSIE MATALA POS",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };
            Controls.Add(lblTitre);

            // ----- DESCRIPTION -----
            lblDescription = new Label
            {
                Text = "Logiciel de gestion de caisse et de vente.\n" +
                       "Conçu pour les boutiques, pharmacies, supermarchés et entreprises.",
                Font = new Font("Segoe UI", 10),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };
            Controls.Add(lblDescription);

            // ----- GROUPE INFOS -----
            grpInfos = new GroupBox
            {
                Text = "Informations du développeur",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Width = 460,
                Height = 260,  // ✅ plus haut pour contenir Réseaux + Site
                Left = 20,
                Top = 120
            };
            Controls.Add(grpInfos);

            // ✅ Placement dynamique (évite superposition)
            int y = 30;

            lblNom = CreerLabel("Nom du projet : MESSIE MATALA POS", y);
            grpInfos.Controls.Add(lblNom);
            y += lblNom.Height + 8;

            lblTelephone = CreerLabel("Téléphone / WhatsApp :\n+243 081 858 4345  |  +243 098 654 9293", y);
            grpInfos.Controls.Add(lblTelephone);
            y += lblTelephone.Height + 8;

            lblEmail = CreerLabel("E-mail : messiematala77@gmail.com", y);
            grpInfos.Controls.Add(lblEmail);
            y += lblEmail.Height + 8;

            lblVille = CreerLabel("Ville : Kinshasa", y);
            grpInfos.Controls.Add(lblVille);
            y += lblVille.Height + 8;

            lblPays = CreerLabel("Pays : République Démocratique du Congo", y);
            grpInfos.Controls.Add(lblPays);
            y += lblPays.Height + 8;

            // ✅ Réseaux + Site (mis à jour)
            lblReseaux = CreerLabel(
                "Réseaux sociaux :\n" +
                "WhatsApp Business : 0810742730\n" +
                "Facebook : Automarket Rdc\n" +
                "TikTok : Automarket_Rdc\n" +
                "Instagram : Messie Matala\n" +
                "Site : www.automarket-rdc.site",
                y);
            grpInfos.Controls.Add(lblReseaux);
            y += lblReseaux.Height + 8;

            // ----- BOUTON FERMER -----
            btnFermer = new Button
            {
                Text = "Fermer",
                Width = 110,
                Height = 34,
                Left = (ClientSize.Width - 110) / 2,
                Top = grpInfos.Bottom + 12,
                BackColor = Color.FromArgb(220, 220, 220)
            };
            btnFermer.Click += (s, e) => Close();
            Controls.Add(btnFermer);
        }

        // ============================
        // MÉTHODE UTILITAIRE (PRO)
        // AutoSize + MaximumSize => pas de superposition, texte multi-lignes propre
        // ============================
        private Label CreerLabel(string texte, int top)
        {
            return new Label
            {
                Text = texte,
                Left = 12,
                Top = top,
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                MaximumSize = new Size(430, 0) // ✅ largeur fixe, hauteur auto
            };
        }
    }
}