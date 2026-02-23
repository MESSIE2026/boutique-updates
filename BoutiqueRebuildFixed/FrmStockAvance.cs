using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.IO;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmStockAvance : FormBase
    {
        // 🔐 Connexion centralisée (PROPRE)
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        private bool modeEdition = false;
        private int idSelectionne = 0;
        public FrmStockAvance()
        {
            InitializeComponent();
            

            // Écoute les changements globaux
            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        private void FrmStockAvance_Load(object sender, EventArgs e)
        {
            ChargerStocks();
            ConfigurerDataGridView();
            ChargerProduits();
            // Charger traductions dynamiques

            // Appliquer AU CHARGEMENT
            RafraichirLangue();
            RafraichirTheme();
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
        private void ChargerProduits()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(
                        "SELECT ID_Produit, NomProduit FROM Produit ORDER BY NomProduit", con);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbProduit.DataSource = dt;
                    cmbProduit.DisplayMember = "NomProduit";   // ce que l’utilisateur voit
                    cmbProduit.ValueMember = "ID_Produit";     // clé interne
                    cmbProduit.SelectedIndex = -1;              // aucun sélectionné au départ
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Erreur chargement produits :\n" + ex.Message,
                        "Erreur SQL",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void ChargerStocks()
        {
            string query = @"
                SELECT 
                    ID,
                    Produit,
                    Quantite,
                    DateEntree,
                    DateSortie,
                    Fournisseurs,
                    Gerant,
                    DateCreation
                FROM StockAvance
                ORDER BY DateCreation DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvStocks.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Erreur lors du chargement du stock avancé :\n" + ex.Message,
                        "Erreur SQL",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }
        private void DrawCenteredTextLocal(XGraphics gfx, string text, double y, XFont font)
        {
            gfx.DrawString(text, font, XBrushes.Black,
                new XRect(0, y, gfx.PageSize.Width, 20),
                XStringFormats.Center);
        }
        private void dgvStocks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow r = dgvStocks.Rows[e.RowIndex];

            idSelectionne = Convert.ToInt32(r.Cells["ID"].Value);

            cmbProduit.Text = r.Cells["Produit"].Value?.ToString();
            txtFournisseur.Text = r.Cells["Fournisseurs"].Value?.ToString();
            txtGerant.Text = r.Cells["Gerant"].Value?.ToString();
        }

        // 🎨 Configuration propre du DataGridView
        private void ConfigurerDataGridView()
        {
            dgvStocks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvStocks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvStocks.MultiSelect = false;
            dgvStocks.ReadOnly = true;
            dgvStocks.AllowUserToAddRows = false;
            dgvStocks.AllowUserToDeleteRows = false;
            dgvStocks.RowHeadersVisible = false;

            dgvStocks.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvStocks.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            modeEdition = false;
            idSelectionne = 0;
            MessageBox.Show("Mode ajout activé.\nSélectionne une ligne vide ou utilise la ligne actuelle.",
                "Ajout", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnModifier_Click(object sender, EventArgs e)
        {
            if (idSelectionne == 0)
            {
                MessageBox.Show("Sélectionne une ligne à modifier.");
                return;
            }

            modeEdition = true;
            MessageBox.Show("Mode modification activé.", "Modification",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (idSelectionne == 0)
            {
                MessageBox.Show("Sélectionne une ligne à supprimer.");
                return;
            }

            if (MessageBox.Show("Confirmer la suppression ?", "Suppression",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM StockAvance WHERE ID=@ID", con);
                cmd.Parameters.AddWithValue("@ID", idSelectionne);
                cmd.ExecuteNonQuery();
            }

            ChargerStocks();
            idSelectionne = 0;

            MessageBox.Show("Ligne supprimée avec succès.");
            AuditLogger.Log("DELETE", "Suppression client ID=45");
        }

        private void BtnValider_Click(object sender, EventArgs e)
        {
            if (dgvStocks.CurrentRow == null)
            {
                MessageBox.Show("Aucune ligne sélectionnée.");
                return;
            }

            DataGridViewRow r = dgvStocks.CurrentRow;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd;

                    string action;
                    if (modeEdition)
                    {
                        cmd = new SqlCommand(@"
                    UPDATE StockAvance SET
                        Produit = @Produit,
                        Quantite = @Quantite,
                        DateEntree = @DateEntree,
                        DateSortie = @DateSortie,
                        Fournisseurs = @Fournisseurs,
                        Gerant = @Gerant
                    WHERE ID = @ID", con);

                        cmd.Parameters.AddWithValue("@ID", idSelectionne);
                        action = $"Modification de StockAvance ID={idSelectionne}";
                    }
                    else
                    {
                        cmd = new SqlCommand(@"
                    INSERT INTO StockAvance
                        (Produit, Quantite, DateEntree, DateSortie, Fournisseurs, Gerant)
                    VALUES
                        (@Produit, @Quantite, @DateEntree, @DateSortie, @Fournisseurs, @Gerant)", con);
                        action = "Nouvelle insertion dans StockAvance";
                    }

                    cmd.Parameters.AddWithValue("@Produit", cmbProduit.Text);
                    cmd.Parameters.AddWithValue("@Quantite", r.Cells["Quantite"].Value ?? 0);
                    cmd.Parameters.AddWithValue("@DateEntree", r.Cells["DateEntree"].Value ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DateSortie", r.Cells["DateSortie"].Value ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fournisseurs", string.IsNullOrWhiteSpace(txtFournisseur.Text) ? "" : txtFournisseur.Text.Trim());
                    cmd.Parameters.AddWithValue("@Gerant", string.IsNullOrWhiteSpace(txtGerant.Text) ? "" : txtGerant.Text.Trim());

                    cmd.ExecuteNonQuery();

                    // --- Audit Log ---
                    string details = $"Produit={cmbProduit.Text}, Quantite={r.Cells["Quantite"].Value}, " +
                                     $"DateEntree={r.Cells["DateEntree"].Value}, DateSortie={r.Cells["DateSortie"].Value}, " +
                                     $"Fournisseurs={txtFournisseur.Text.Trim()}, Gerant={txtGerant.Text.Trim()}";

                    ConfigSysteme.AjouterAuditLog("StockAvance", action + " : " + details, "Succès");
                }

                ChargerStocks();
                MessageBox.Show("Opération enregistrée avec succès.");
            }
            catch (Exception ex)
            {
                // Audit Log en cas d'erreur
                string errorMsg = $"Erreur lors de l'enregistrement StockAvance : {ex.Message}";
                ConfigSysteme.AjouterAuditLog("StockAvance", errorMsg, "Échec");

                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void BtnExporterPDF_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = "StockAvance.pdf"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            PdfDocument doc = new PdfDocument();
            PdfPage page = doc.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont titleFont = new XFont("Arial", 14, XFontStyle.Bold);
            XFont headerFont = new XFont("Arial", 12, XFontStyle.Bold);
            XFont textFont = new XFont("Arial", 9);

            double y = 40;

            DrawCenteredTextLocal(gfx, "ZAIRE MODE SARL", y, titleFont); y += 25;
            DrawCenteredTextLocal(gfx, "23, Bld Lumumba, Q1 Masina Sans Fil", y, textFont); y += 15;
            DrawCenteredTextLocal(gfx,
                "RCCM: 25-B-01497 | ID.NAT: 01-F4300-N73258E", y, textFont); y += 25;

            DrawCenteredTextLocal(gfx,
                "MOUVEMENTS DE STOCK - STOCK AVANCÉ", y, headerFont); y += 30;

            gfx.DrawLine(XPens.Black, 40, y, page.Width - 40, y);
            y += 20;

            double colWidth = (page.Width - 80) / dgvStocks.Columns.Count;

            foreach (DataGridViewColumn col in dgvStocks.Columns)
            {
                gfx.DrawString(col.HeaderText, textFont, XBrushes.Black,
                    new XRect(40 + col.Index * colWidth, y, colWidth, 20),
                    XStringFormats.Center);
            }

            y += 20;

            foreach (DataGridViewRow row in dgvStocks.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    gfx.DrawString(cell.Value?.ToString() ?? "",
                        textFont, XBrushes.Black,
                        new XRect(40 + cell.ColumnIndex * colWidth, y, colWidth, 20),
                        XStringFormats.Center);
                }
                y += 20;
            }

            doc.Save(sfd.FileName);
            MessageBox.Show("PDF exporté avec succès.");
        }
    }
}