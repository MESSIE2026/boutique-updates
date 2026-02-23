using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FormGestionImprimantes : FormBase
    {
        private PrintDocument printDocument;
        private string imprimanteATester;
        public FormGestionImprimantes()
        {
            InitializeComponent();
            this.Load += FormGestionImprimantes_Load;

            InitialiserFormulaire();

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }
        private void FormGestionImprimantes_Load(object sender, EventArgs e)
        {
            ConfigSysteme.AppliquerTraductions(this);
            ConfigSysteme.AppliquerTheme(this);

            // ✅ Charger config imprimantes
            ConfigSysteme.LoadPrintersConfig();

            cboTicket.Items.Clear();
            cboA4.Items.Clear();

            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                cboTicket.Items.Add(printer);
                cboA4.Items.Add(printer);
            }

            cboTicket.DropDownStyle = ComboBoxStyle.DropDownList;
            cboA4.DropDownStyle = ComboBoxStyle.DropDownList;

            // ✅ Mettre Ticket sauvegardée en 1ère position + la sélectionner
            MettreImprimanteEnPremierEtSelectionner(cboTicket, ConfigSysteme.ImprimanteTicketNom);

            // ✅ Sélectionner A4 sauvegardée
            SelectionnerImprimanteSiExiste(cboA4, ConfigSysteme.ImprimanteA4Nom);

            // Fallback si rien
            if (cboTicket.SelectedIndex < 0 && cboTicket.Items.Count > 0) cboTicket.SelectedIndex = 0;
            if (cboA4.SelectedIndex < 0 && cboA4.Items.Count > 0) cboA4.SelectedIndex = 0;

            RafraichirLangue();
            RafraichirTheme();
        }

        private void SelectionnerImprimanteSiExiste(ComboBox cbo, string printerName)
        {
            if (cbo == null) return;
            if (string.IsNullOrWhiteSpace(printerName)) return;

            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (string.Equals(cbo.Items[i]?.ToString(), printerName, StringComparison.OrdinalIgnoreCase))
                {
                    cbo.SelectedIndex = i;
                    return;
                }
            }
        }

        private void MettreImprimanteEnPremierEtSelectionner(ComboBox cbo, string printerName)
        {
            if (cbo == null) return;
            if (string.IsNullOrWhiteSpace(printerName)) return;

            int idx = -1;
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (string.Equals(cbo.Items[i]?.ToString(), printerName, StringComparison.OrdinalIgnoreCase))
                {
                    idx = i;
                    break;
                }
            }

            if (idx >= 0)
            {
                object it = cbo.Items[idx];
                cbo.Items.RemoveAt(idx);
                cbo.Items.Insert(0, it);
                cbo.SelectedIndex = 0;
            }
        }


        private void RafraichirLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);
        }

        private void RafraichirTheme()
        {
            ConfigSysteme.AppliquerTheme(this);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 🔥 OBLIGATOIRE : éviter fuite mémoire
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;

            base.OnFormClosed(e);
        }
        private void InitialiserFormulaire()
        {
            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Texte simple pour page test
            string texte = "Test impression sur : " + imprimanteATester;
            Font font = new Font("Arial", 14, FontStyle.Bold);
            e.Graphics.DrawString(texte, font, Brushes.Black, 100, 100);
        }

        private void btnTestTicket_Click(object sender, EventArgs e)
        {
            if (cboTicket.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une imprimante Ticket.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            imprimanteATester = cboTicket.SelectedItem.ToString();
            printDocument.PrinterSettings.PrinterName = imprimanteATester;

            try
            {
                printDocument.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur impression ticket : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTestA4_Click(object sender, EventArgs e)
        {
            if (cboA4.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une imprimante A4.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            imprimanteATester = cboA4.SelectedItem.ToString();
            printDocument.PrinterSettings.PrinterName = imprimanteATester;

            try
            {
                printDocument.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur impression A4 : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cboTicket.SelectedItem == null || cboA4.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner les deux imprimantes avant d'enregistrer.", "Attention",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ConfigSysteme.ImprimanteTicketNom = cboTicket.SelectedItem.ToString();
            ConfigSysteme.ImprimanteA4Nom = cboA4.SelectedItem.ToString();

            ConfigSysteme.SavePrintersConfig();

            MessageBox.Show(
                $"✅ Paramètres enregistrés :\n\nTicket : {ConfigSysteme.ImprimanteTicketNom}\nA4 : {ConfigSysteme.ImprimanteA4Nom}",
                "Enregistré",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
