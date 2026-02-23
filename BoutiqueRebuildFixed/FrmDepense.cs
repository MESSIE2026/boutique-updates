using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace BoutiqueRebuildFixed
{
    public partial class FrmDepense : FormBase
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        public FrmDepense()
        {
            InitializeComponent();

            Button btnValidation = new Button
            {
                Name = "btnValidationManager",
                Text = "Validation manager",
                Width = 190,
                Height = 40,
                BackColor = Color.MidnightBlue,
                ForeColor = Color.White,

                // ✅ Décale vers la droite (+ 210 par exemple)
                Left = btnExporterPDF.Left + 210,

                // ✅ Au-dessus
                Top = btnExporterPDF.Top - 45
            };

            btnValidation.Click += (s, e) =>
            {
                using (var f = new FrmValidationDepenses())
                {
                    f.ShowDialog(this);
                    LoadData(); // refresh après validation
                }
            };

            this.Controls.Add(btnValidation);
            btnValidation.BringToFront();

            
           
            LoadDevises();
            LoadData();  // Charger la grille dès le lancement

            // Charger traductions dynamiques

            // Écoute les changements globaux
            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
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
        private void LoadData()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
SELECT
    IdDepense,
    DateDepense,
    Description,
    Montant,
    Categorie,
    Observations,
    Devise,
    Statut,
    ValidePar,
    DateValidation,
    CommentaireValidation,
    RejetePar,
    DateRejet,
    MotifRejet
FROM dbo.Depenses
ORDER BY IdDepense DESC;";

                    using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvDepenses.DataSource = dt;
                    }

                    // ✅ Config Grid PRO (sécurisé)
                    dgvDepenses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvDepenses.RowHeadersVisible = false;
                    dgvDepenses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgvDepenses.MultiSelect = false;

                    // ✅ Headers (si colonnes existent)
                    void H(string col, string text)
                    {
                        if (dgvDepenses.Columns.Contains(col)) dgvDepenses.Columns[col].HeaderText = text;
                    }

                    H("IdDepense", "ID");
                    H("DateDepense", "Date");
                    H("Description", "Description");
                    H("Montant", "Montant");
                    H("Devise", "Devise");
                    H("Categorie", "Catégorie");
                    H("Observations", "Observations");
                    H("Statut", "Statut");
                    H("ValidePar", "Validé par");
                    H("DateValidation", "Date validation");
                    H("CommentaireValidation", "Commentaire validation");
                    H("RejetePar", "Rejeté par");
                    H("DateRejet", "Date rejet");
                    H("MotifRejet", "Motif rejet");

                    // ✅ Formats
                    if (dgvDepenses.Columns.Contains("DateDepense"))
                        dgvDepenses.Columns["DateDepense"].DefaultCellStyle.Format = "dd/MM/yyyy";

                    if (dgvDepenses.Columns.Contains("DateValidation"))
                        dgvDepenses.Columns["DateValidation"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

                    if (dgvDepenses.Columns.Contains("DateRejet"))
                        dgvDepenses.Columns["DateRejet"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

                    if (dgvDepenses.Columns.Contains("Montant"))
                    {
                        dgvDepenses.Columns["Montant"].DefaultCellStyle.Format = "N2";
                        dgvDepenses.Columns["Montant"].DefaultCellStyle.Alignment =
                            DataGridViewContentAlignment.MiddleRight;
                    }

                    // ✅ FillWeight (facultatif mais joli)
                    void W(string col, float w)
                    {
                        if (dgvDepenses.Columns.Contains(col)) dgvDepenses.Columns[col].FillWeight = w;
                    }

                    W("IdDepense", 7);
                    W("DateDepense", 10);
                    W("Description", 20);
                    W("Montant", 10);
                    W("Devise", 7);
                    W("Categorie", 12);
                    W("Observations", 14);
                    W("Statut", 9);
                    W("ValidePar", 10);
                    W("DateValidation", 12);
                    W("CommentaireValidation", 16);
                    W("RejetePar", 10);
                    W("DateRejet", 12);
                    W("MotifRejet", 16);

                    dgvDepenses.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement données : " + ex.Message);
            }
        }

        private void ClearForm()
        {
            txtID.Text = "";
            dtpDate.Value = DateTime.Today;
            txtDescription.Text = "";
            txtMontant.Text = "";
            cmbDevise.SelectedIndex = 0;
            cmbCategorie.SelectedIndex = 0;
            txtObservations.Text = "";
        }
        private void LoadDevises()
        {
            cmbDevise.Items.Clear();

            var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            var devises = cultures
                .Select(c => new RegionInfo(c.LCID).ISOCurrencySymbol)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            foreach (string d in devises)
                cmbDevise.Items.Add(d);

            cmbDevise.SelectedItem = "USD"; // valeur par défaut
        }


        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("La description est obligatoire.");
                return false;
            }

            decimal montant;
            if (!decimal.TryParse(txtMontant.Text, out montant) || montant <= 0)
            {
                MessageBox.Show("Montant invalide.");
                return false;
            }

            if (cmbDevise.SelectedIndex < 0)
            {
                MessageBox.Show("Sélectionnez une devise.");
                return false;
            }

            if (cmbCategorie.SelectedIndex < 0)
            {
                MessageBox.Show("Sélectionnez une catégorie.");
                return false;
            }

            return true;
        }
        private string CleanText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            return input.Replace("\"", "")
                        .Replace("'", "")
                        .Replace(",", "")
                        .Trim();
        }

        private void dgvDepenses_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvDepenses.Rows[e.RowIndex];

                txtID.Text = row.Cells["IdDepense"].Value.ToString();
                dtpDate.Value = Convert.ToDateTime(row.Cells["DateDepense"].Value);
                txtDescription.Text = row.Cells["Description"].Value.ToString();
                txtMontant.Text = row.Cells["Montant"].Value.ToString();
                cmbDevise.SelectedItem = row.Cells["Devise"].Value.ToString();
                cmbCategorie.SelectedItem = row.Cells["Categorie"].Value.ToString();
                txtObservations.Text = row.Cells["Observations"].Value.ToString();
            }
        }
        private void DrawCenteredTextLocal(XGraphics gfx, string text, int y, XFont font, PdfPage page)
        {
            var size = gfx.MeasureString(text, font);
            double x = (page.Width.Point - size.Width) / 2;
            gfx.DrawString(text, font, XBrushes.Black, new XPoint(x, y));
        }
        private void FrmDepense_Load(object sender, EventArgs e)
        {

        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
INSERT INTO dbo.Depenses
(DateDepense, Description, Montant, Devise, Categorie, Observations,
 Statut, ValidePar, DateValidation, CommentaireValidation, RejetePar, DateRejet, MotifRejet)
VALUES
(@DateDepense, @Description, @Montant, @Devise, @Categorie, @Observations,
 N'En attente', NULL, NULL, NULL, NULL, NULL, NULL);";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@DateDepense", SqlDbType.Date).Value = dtpDate.Value.Date;
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = CleanText(txtDescription.Text);

                        var raw = (txtMontant.Text ?? "").Trim().Replace(',', '.');
                        if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal montant) || montant <= 0)
                        {
                            MessageBox.Show("Montant invalide.");
                            return;
                        }

                        var p = cmd.Parameters.Add("@Montant", SqlDbType.Decimal);
                        p.Precision = 18;
                        p.Scale = 2;
                        p.Value = montant;

                        cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = (cmbDevise.SelectedItem?.ToString() ?? "USD");
                        cmd.Parameters.Add("@Categorie", SqlDbType.NVarChar, 100).Value = CleanText(cmbCategorie.Text);
                        cmd.Parameters.Add("@Observations", SqlDbType.NVarChar, 255).Value = CleanText(txtObservations.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog("Depenses",
                    $"Dépense ajoutée : {dtpDate.Value:dd/MM/yyyy}, Montant={txtMontant.Text} {cmbDevise.SelectedItem}, Catégorie={cmbCategorie.Text}",
                    "Succès");

                MessageBox.Show("Dépense ajoutée (En attente de validation).");

                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Depenses", $"Erreur ajout dépense : {ex.Message}", "Échec");
                MessageBox.Show("Erreur ajout : " + ex.Message);
            }
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text))
            {
                MessageBox.Show("Sélectionnez une dépense.");
                return;
            }

            if (!ValidateInputs()) return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
UPDATE dbo.Depenses SET
    DateDepense=@DateDepense,
    Description=@Description,
    Montant=@Montant,
    Devise=@Devise,
    Categorie=@Categorie,
    Observations=@Observations
WHERE IdDepense=@IdDepense
  AND ISNULL(Statut,'En attente') = N'En attente';";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@DateDepense", SqlDbType.Date).Value = dtpDate.Value.Date;
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 255).Value = CleanText(txtDescription.Text);

                        var raw = (txtMontant.Text ?? "").Trim().Replace(',', '.');
                        if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal montant) || montant <= 0)
                        {
                            MessageBox.Show("Montant invalide.");
                            return;
                        }

                        var p = cmd.Parameters.Add("@Montant", SqlDbType.Decimal);
                        p.Precision = 18;
                        p.Scale = 2;
                        p.Value = montant;

                        cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = (cmbDevise.SelectedItem?.ToString() ?? "USD");
                        cmd.Parameters.Add("@Categorie", SqlDbType.NVarChar, 100).Value = CleanText(cmbCategorie.Text);
                        cmd.Parameters.Add("@Observations", SqlDbType.NVarChar, 255).Value = CleanText(txtObservations.Text);
                        cmd.Parameters.Add("@IdDepense", SqlDbType.Int).Value = int.Parse(txtID.Text);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            MessageBox.Show("Impossible : cette dépense n'est plus en attente (déjà validée/rejetée).");
                            LoadData();
                            return;
                        }
                    }
                }

                MessageBox.Show("Dépense modifiée !");
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur modification : " + ex.Message);
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text))
            {
                MessageBox.Show("Sélectionnez une dépense.");
                return;
            }

            if (MessageBox.Show("Confirmer la suppression ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd =
                        new SqlCommand("DELETE FROM Depenses WHERE IdDepense=@Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", int.Parse(txtID.Text));
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Dépense supprimée !");
                AuditLogger.Log("DELETE", "Suppression client ID=45");
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur suppression : " + ex.Message);
            }
        }

        private void btnNouveau_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnExporterPDF_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(dgvDepenses.DataSource is DataTable dt) || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Aucune donnée à exporter.");
                    return;
                }

                DateTime dateFiltre = dtpDate.Value.Date;

                var lignesFiltrees = dt.AsEnumerable()
                    .Where(r => Convert.ToDateTime(r["DateDepense"]).Date == dateFiltre)
                    .ToList();

                if (lignesFiltrees.Count == 0)
                {
                    MessageBox.Show("Aucune dépense trouvée pour la date sélectionnée.");
                    return;
                }

                PdfDocument document = new PdfDocument();
                document.Info.Title = "Dépenses du " + dateFiltre.ToString("dd/MM/yyyy");

                PdfPage page = document.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                page.Orientation = PdfSharpCore.PageOrientation.Landscape;

                XGraphics gfx = XGraphics.FromPdfPage(page);

                int marginLeft = 35;
                int marginTop = 35;
                int y = marginTop;

                // 🔠 POLICES PLUS GRANDES
                XFont titleFont = new XFont("Arial", 16, XFontStyle.Bold);
                XFont headerFont = new XFont("Arial", 12, XFontStyle.Bold);
                XFont dataFont = new XFont("Arial", 11, XFontStyle.Regular);

                // ===== EN-TÊTE =====
                DrawCenteredTextLocal(gfx, "ZAIRE MODE SARL", y, new XFont("Arial", 18, XFontStyle.Bold), page);
                y += 15;

                DrawCenteredTextLocal(gfx, "23, Bld Lumumba, Q1 Masina Sans Fil", y, new XFont("Arial", 14), page);
                y += 15;

                DrawCenteredTextLocal(gfx, "RCCM: 25-B-01497 | ID.NAT: 01-F4300-N73258E", y, new XFont("Arial", 14), page);
                y += 35;

                DrawCenteredTextLocal(gfx, "LISTE DES DÉPENSES DU " + dateFiltre.ToString("dd/MM/yyyy"),
                    y, new XFont("Arial", 18, XFontStyle.Bold), page);

                y += 20;

                // ===== DIMENSIONS DES COLONNES (A4 PAYSAGE OPTIMISÉ) =====
                int rowHeight = 30;

                int colId = 35;
                int colDate = 75;
                int colDesc = 230;
                int colMontant = 80;
                int colDevise = 60;
                int colCat = 150;
                int colObs = 150;

                // ===== DESSIN CELLULE =====
                Action<string, int, int, int> DrawCell = (text, xPos, yCell, width) =>
                {
                    gfx.DrawRectangle(XPens.Black, xPos, yCell, width, rowHeight);
                    gfx.DrawString(text ?? "", dataFont, XBrushes.Black,
                        new XRect(xPos + 5, yCell + 6, width - 10, rowHeight),
                        XStringFormats.TopLeft);
                };

                // ===== ENTÊTE TABLEAU =====
                int x = marginLeft;

                void DrawHeader(string text, int width)
                {
                    gfx.DrawRectangle(XPens.Black, x, y, width, rowHeight);
                    gfx.DrawString(text, headerFont, XBrushes.Black,
                        new XRect(x, y, width, rowHeight),
                        XStringFormats.Center);
                    x += width;
                }

                DrawHeader("ID", colId);
                DrawHeader("Date", colDate);
                DrawHeader("Description", colDesc);
                DrawHeader("Montant", colMontant);
                DrawHeader("Devise", colDevise);
                DrawHeader("Catégorie", colCat);
                DrawHeader("Observations", colObs);

                y += rowHeight;

                // ===== DONNÉES =====
                foreach (var row in lignesFiltrees)
                {
                    if (y + rowHeight > page.Height - 60)
                    {
                        page = document.AddPage();
                        page.Size = PdfSharpCore.PageSize.A4;
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        gfx = XGraphics.FromPdfPage(page);
                        y = marginTop;
                    }

                    x = marginLeft;

                    DrawCell(row["IdDepense"].ToString(), x, y, colId); x += colId;
                    DrawCell(Convert.ToDateTime(row["DateDepense"]).ToString("dd/MM/yyyy"), x, y, colDate); x += colDate;
                    DrawCell(row["Description"].ToString(), x, y, colDesc); x += colDesc;
                    DrawCell(Convert.ToDecimal(row["Montant"]).ToString("N2"), x, y, colMontant); x += colMontant;
                    DrawCell(row["Devise"].ToString(), x, y, colDevise); x += colDevise;
                    DrawCell(row["Categorie"].ToString(), x, y, colCat); x += colCat;
                    DrawCell(row["Observations"].ToString(), x, y, colObs);

                    y += rowHeight;
                }

                using (SaveFileDialog saveDlg = new SaveFileDialog())
                {
                    saveDlg.Filter = "Fichiers PDF (*.pdf)|*.pdf";
                    saveDlg.FileName = "Depenses_" + dateFiltre.ToString("yyyyMMdd") + ".pdf";

                    if (saveDlg.ShowDialog() == DialogResult.OK)
                    {
                        using (FileStream fs = new FileStream(saveDlg.FileName, FileMode.Create))
                        {
                            document.Save(fs);
                        }
                        MessageBox.Show("Export PDF réussi !");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur export PDF : " + ex.Message);
            }
        }

        private void btnQuitter_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous quitter ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
    }
