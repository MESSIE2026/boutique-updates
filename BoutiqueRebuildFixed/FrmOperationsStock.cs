using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace BoutiqueRebuildFixed
{
    public partial class FrmOperationsStock : Form
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        public FrmOperationsStock()
        {
            InitializeComponent();

            txtUtilisateur.Text = $"{SessionEmploye.Nom} {SessionEmploye.Prenom}".Trim();
            txtUtilisateur.ReadOnly = true;

            ChargerProduits();
            ChargerDepots();

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;

            cmbTypeOperation.Items.Clear();
            cmbTypeOperation.Items.AddRange(new[] { "ENTREE", "SORTIE", "AJUSTEMENT", "TRANSFERT", "PERTE" });
            cmbTypeOperation.SelectedIndexChanged -= cmbTypeOperation_SelectedIndexChanged;
            cmbTypeOperation.SelectedIndexChanged += cmbTypeOperation_SelectedIndexChanged;
            cmbTypeOperation.SelectedIndex = 0;

            Load += FrmOperationsStock_Load;
        }

        private void RafraichirLangue() => ConfigSysteme.AppliquerTraductions(this);
        private void RafraichirTheme() => ConfigSysteme.AppliquerTheme(this);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }
        private void cmbTypeOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = (cmbTypeOperation.SelectedItem?.ToString() ?? "").Trim().ToUpperInvariant();

            bool motifVisible = (selected == "AJUSTEMENT" || selected == "PERTE");
            CacherMotif(motifVisible);
            if (!motifVisible) txtMotif.Clear();

            // ENTREE => destination obligatoire, source non requise
            cmbDepotSource.Enabled = (selected != "ENTREE");
            cmbDepotDestination.Enabled = (selected == "ENTREE" || selected == "TRANSFERT");

            if (selected == "ENTREE")
                cmbDepotSource.SelectedIndex = -1;

            if (selected != "ENTREE" && cmbDepotSource.SelectedIndex < 0 && cmbDepotSource.Items.Count > 0)
                cmbDepotSource.SelectedIndex = 0;

            if ((selected == "ENTREE" || selected == "TRANSFERT") && cmbDepotDestination.SelectedIndex < 0 && cmbDepotDestination.Items.Count > 0)
                cmbDepotDestination.SelectedIndex = 0;
        }

        private void ChargerProduits()
        {
            cmbProduit.Items.Clear();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                const string sql = "SELECT ID_Produit, NomProduit FROM Produit ORDER BY NomProduit";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cmbProduit.Items.Add(new ComboboxItem(
                            reader["NomProduit"].ToString(),
                            Convert.ToInt32(reader["ID_Produit"])
                        ));
                    }
                }
            }

            if (cmbProduit.Items.Count > 0)
                cmbProduit.SelectedIndex = 0;
        }

        private void ChargerDepots()
        {
            cmbDepotSource.Items.Clear();
            cmbDepotDestination.Items.Clear();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ID_Depot, NomDepot FROM Depot ORDER BY NomDepot", con))
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var item = new ComboboxItem(rd["NomDepot"].ToString(), Convert.ToInt32(rd["ID_Depot"]));
                        cmbDepotSource.Items.Add(item);
                        cmbDepotDestination.Items.Add(new ComboboxItem(item.Text, item.Value));
                    }
                }
            }

            if (cmbDepotSource.Items.Count > 0) cmbDepotSource.SelectedIndex = 0;
            if (cmbDepotDestination.Items.Count > 0) cmbDepotDestination.SelectedIndex = 0;
        }

        private int? GetDepotId(ComboBox cmb) => cmb.SelectedItem is ComboboxItem it ? it.Value : (int?)null;

        /// <summary>
        /// Unité de base robuste:
        /// 1) si Produit.ID_UniteBase existe => utilisé
        /// 2) sinon fallback ProduitUnite.IsBase=1
        /// (Adapte si ton schéma est différent)
        /// </summary>
        private int GetUniteBaseIdForProduit(int idProduit)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP (1) pu.ID_Unite
FROM dbo.ProduitUnite pu
WHERE pu.ID_Produit = @p
  AND pu.IsBase = 1;", con))
                {
                    cmd.Parameters.Add("@p", SqlDbType.Int).Value = idProduit;

                    object v = cmd.ExecuteScalar();
                    if (v == null || v == DBNull.Value)
                        throw new Exception("Unité de base introuvable pour ce produit (ProduitUnite.IsBase=1).");

                    return Convert.ToInt32(v);
                }
            }
        }
        private void CacherMotif(bool visible)
        {
            lblMotif.Visible = visible;
            txtMotif.Visible = visible;
        }

        private void EffacerFormulaire()
        {
            if (cmbProduit.Items.Count > 0) cmbProduit.SelectedIndex = 0;
            cmbTypeOperation.SelectedIndex = 0;

            nudQuantite.Value = 1;
            dtpDateOperation.Value = DateTime.Now;

            txtUtilisateur.Text = $"{SessionEmploye.Nom} {SessionEmploye.Prenom}".Trim();
            txtMotif.Clear();
            txtReference.Clear();
            txtEmplacement.Clear();
            txtRemarques.Clear();

            CacherMotif(false);
        }

        private void ChargerOperations()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                const string sql = @"
SELECT 
    op.ID_Operation,
    p.NomProduit,
    op.TypeOperation,
    op.Quantite,
    op.QuantiteBase,
    op.TypeMouvement,
    ds.NomDepot AS DepotSource,
    dd.NomDepot AS DepotDestination,
    op.LotNumero,
    op.DateExpiration,
    op.DateOperation,
    op.Utilisateur,
    op.Motif,
    op.Reference,
    op.Emplacement,
    op.Remarques
FROM OperationsStock op
INNER JOIN Produit p ON op.ID_Produit = p.ID_Produit
LEFT JOIN Depot ds ON ds.ID_Depot = op.ID_DepotSource
LEFT JOIN Depot dd ON dd.ID_Depot = op.ID_DepotDestination
ORDER BY op.DateOperation DESC;";

                using (SqlDataAdapter adapter = new SqlDataAdapter(sql, con))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvOperations.DataSource = null;
                    dgvOperations.AutoGenerateColumns = true;
                    dgvOperations.DataSource = dt;

                    void SetHeader(string col, string header)
                    {
                        if (dgvOperations.Columns.Contains(col))
                            dgvOperations.Columns[col].HeaderText = header;
                    }

                    SetHeader("ID_Operation", "ID");
                    SetHeader("NomProduit", "Produit");
                    SetHeader("TypeOperation", "Type");
                    SetHeader("Quantite", "Qté");
                    SetHeader("QuantiteBase", "Qté Base");
                    SetHeader("TypeMouvement", "Mouvement");
                    SetHeader("DepotSource", "Dépôt Source");
                    SetHeader("DepotDestination", "Dépôt Destination");
                    SetHeader("LotNumero", "Lot");
                    SetHeader("DateExpiration", "Expiration");
                    SetHeader("DateOperation", "Date");
                    SetHeader("Utilisateur", "Utilisateur");
                    SetHeader("Motif", "Motif");
                    SetHeader("Reference", "Référence");
                    SetHeader("Emplacement", "Emplacement");
                    SetHeader("Remarques", "Remarques");

                    if (dgvOperations.Columns.Contains("ID_Operation"))
                        dgvOperations.Columns["ID_Operation"].Visible = false;

                    if (dgvOperations.Columns.Contains("DateOperation"))
                        dgvOperations.Columns["DateOperation"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                    if (dgvOperations.Columns.Contains("DateExpiration"))
                        dgvOperations.Columns["DateExpiration"].DefaultCellStyle.Format = "dd/MM/yyyy";

                    dgvOperations.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
        }
        
        private void FrmOperationsStock_Load(object sender, EventArgs e)
        {
            ChargerOperations();
            cmbTypeOperation_SelectedIndexChanged(null, EventArgs.Empty);
        }


        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            if (cmbProduit.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un produit.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (nudQuantite.Value <= 0)
            {
                MessageBox.Show("La quantité doit être supérieure à zéro.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string type = (cmbTypeOperation.SelectedItem?.ToString() ?? "").Trim().ToUpperInvariant();

            if ((type == "AJUSTEMENT" || type == "PERTE") && string.IsNullOrWhiteSpace(txtMotif.Text))
            {
                MessageBox.Show("Veuillez saisir un motif pour l'ajustement/perte.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var produit = (ComboboxItem)cmbProduit.SelectedItem;
            int idProduit = produit.Value;
            decimal qte = nudQuantite.Value;

            int? depotSource = GetDepotId(cmbDepotSource);
            int? depotDest = GetDepotId(cmbDepotDestination);

            if (type == "ENTREE" && depotDest == null)
            {
                MessageBox.Show("Dépôt destination requis pour ENTREE.");
                return;
            }

            if ((type == "SORTIE" || type == "PERTE" || type == "AJUSTEMENT") && depotSource == null)
            {
                MessageBox.Show("Dépôt source requis pour cette opération.");
                return;
            }

            if (type == "TRANSFERT")
            {
                if (depotSource == null || depotDest == null)
                {
                    MessageBox.Show("Dépôt source et destination requis pour TRANSFERT.");
                    return;
                }
                if (depotSource.Value == depotDest.Value)
                {
                    MessageBox.Show("Dépôt source et destination doivent être différents.");
                    return;
                }
            }

            try
            {
                int uniteBaseId = GetUniteBaseIdForProduit(idProduit);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("dbo.sp_ApplyStockOperationV2", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@ID_Produit", SqlDbType.Int).Value = idProduit;
                        cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 20).Value = type;

                        // ✅ Quantité typée (évite AddWithValue)
                        var pQte = cmd.Parameters.Add("@Quantite", SqlDbType.Decimal);
                        pQte.Precision = 18;
                        pQte.Scale = 3; // adapte selon ton besoin
                        pQte.Value = qte;

                        cmd.Parameters.Add("@ID_Unite", SqlDbType.Int).Value = uniteBaseId;

                        cmd.Parameters.Add("@ID_DepotSource", SqlDbType.Int).Value = (object)depotSource ?? DBNull.Value;
                        cmd.Parameters.Add("@ID_DepotDestination", SqlDbType.Int).Value = (object)depotDest ?? DBNull.Value;

                        cmd.Parameters.Add("@ID_Variante", SqlDbType.Int).Value = DBNull.Value;

                        // ✅ user fallback
                        string user = (txtUtilisateur.Text ?? "").Trim();
                        if (string.IsNullOrWhiteSpace(user))
                            user = Environment.UserName;

                        cmd.Parameters.Add("@Utilisateur", SqlDbType.NVarChar, 200).Value = user;

                        cmd.Parameters.Add("@Motif", SqlDbType.NVarChar, 250).Value =
                            string.IsNullOrWhiteSpace(txtMotif.Text) ? (object)DBNull.Value : txtMotif.Text.Trim();

                        cmd.Parameters.Add("@Reference", SqlDbType.NVarChar, 120).Value =
                            string.IsNullOrWhiteSpace(txtReference.Text) ? (object)DBNull.Value : txtReference.Text.Trim();

                        cmd.Parameters.Add("@Emplacement", SqlDbType.NVarChar, 120).Value =
                            string.IsNullOrWhiteSpace(txtEmplacement.Text) ? (object)DBNull.Value : txtEmplacement.Text.Trim();

                        cmd.Parameters.Add("@Remarques", SqlDbType.NVarChar, -1).Value =
                            string.IsNullOrWhiteSpace(txtRemarques.Text) ? (object)DBNull.Value : txtRemarques.Text.Trim();

                        cmd.Parameters.Add("@LotNumero", SqlDbType.NVarChar, 80).Value = DBNull.Value;
                        cmd.Parameters.Add("@DateExpiration", SqlDbType.Date).Value = DBNull.Value;

                        cmd.Parameters.Add("@DateOperation", SqlDbType.DateTime).Value = dtpDateOperation.Value;

                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog("OperationsStock",
                    $"Opération '{type}' ProduitID={idProduit}, Qte={qte}, Source={depotSource}, Dest={depotDest}, User={txtUtilisateur.Text.Trim()}",
                    "Succès");

                MessageBox.Show("Opération enregistrée avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                EffacerFormulaire();
                ChargerOperations();
            }
            catch (SqlException ex)
            {
                ConfigSysteme.AjouterAuditLog("OperationsStock", ex.Message, "Échec");
                MessageBox.Show($"Erreur SQL : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("OperationsStock", ex.Message, "Échec");
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnEffacer_Click(object sender, EventArgs e)
        {
            {
                EffacerFormulaire();
            }
        }

        private void btnExporterPDF_Click(object sender, EventArgs e)
        {
            if (dgvOperations.Rows.Count == 0)
            {
                MessageBox.Show("Aucune donnée à exporter.");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Fichiers PDF (*.pdf)|*.pdf";
                sfd.FileName = $"OperationsStock_{DateTime.Now:yyyyMMdd}.pdf";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = "Export opérations stock";

                    PdfPage page = document.AddPage();
                    page.Size = PageSize.A4;
                    page.Orientation = PageOrientation.Landscape;

                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    int marginLeft = 30;
                    int marginRight = 30;
                    int marginTop = 30;
                    int marginBottom = 30;

                    double y = marginTop;

                    XFont titleFont = new XFont("Arial", 14, XFontStyle.Bold);
                    XFont subFont = new XFont("Arial", 10, XFontStyle.Regular);
                    XFont sectionFont = new XFont("Arial", 11, XFontStyle.Bold);
                    XFont headerFont = new XFont("Arial", 9, XFontStyle.Bold);
                    XFont contentFont = new XFont("Arial", 9, XFontStyle.Regular);

                    var cols = dgvOperations.Columns
                        .Cast<DataGridViewColumn>()
                        .Where(c => c.Visible && !(c is DataGridViewButtonColumn))
                        .OrderBy(c => c.DisplayIndex)
                        .ToList();

                    if (cols.Count == 0)
                    {
                        MessageBox.Show("Aucune colonne visible à exporter.");
                        return;
                    }

                    var rows = dgvOperations.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => !r.IsNewRow)
                        .ToList();

                    var groups = rows
                        .GroupBy(r => GetDateFromRow(r, "DateOperation").Date)
                        .OrderBy(g => g.Key)
                        .ToList();

                    double pageWidth = page.Width.Point - marginLeft - marginRight;
                    double pageHeight = page.Height.Point - marginTop - marginBottom;

                    void NewPage()
                    {
                        page = document.AddPage();
                        page.Size = PageSize.A4;
                        page.Orientation = PageOrientation.Landscape;
                        gfx = XGraphics.FromPdfPage(page);
                        y = marginTop;
                        DrawHeader();
                    }

                    void DrawCenteredText(string text, double yPos, XFont font)
                    {
                        var w = gfx.MeasureString(text, font).Width;
                        gfx.DrawString(text, font, XBrushes.Black,
                            new XPoint((page.Width.Point - w) / 2, yPos));
                    }

                    void DrawHeader()
                    {
                        string dateDuJour = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                        DrawCenteredText("ZAIRE MODE SARL", y, titleFont);
                        y += 22;

                        DrawCenteredText("23, Bld Lumumba, Q1 Masina Sans Fil", y, subFont);
                        y += 14;

                        DrawCenteredText("RCCM: 25-B-01497 | ID.NAT: 01-F4300-N73258E", y, subFont);
                        y += 18;

                        DrawCenteredText($"OPERATIONS STOCKS – Export du {dateDuJour}", y, new XFont("Arial", 11, XFontStyle.Bold));
                        y += 18;

                        gfx.DrawLine(XPens.Black, marginLeft, y, page.Width.Point - marginRight, y);
                        y += 12;
                    }

                    double[] ComputeColumnWidths(System.Collections.Generic.List<DataGridViewRow> sampleRows)
                    {
                        int colCount = cols.Count;
                        double[] widths = new double[colCount];

                        for (int i = 0; i < colCount; i++)
                        {
                            double maxW = gfx.MeasureString(cols[i].HeaderText, headerFont).Width;

                            foreach (var r in sampleRows)
                            {
                                string cellText = (r.Cells[cols[i].Name].Value?.ToString() ?? "").Trim();
                                double w = gfx.MeasureString(cellText, contentFont).Width;
                                if (w > maxW) maxW = w;
                            }

                            widths[i] = maxW + 10;
                        }

                        double total = widths.Sum();
                        if (total > pageWidth)
                        {
                            double scale = pageWidth / total;
                            for (int i = 0; i < colCount; i++) widths[i] *= scale;
                        }

                        for (int i = 0; i < colCount; i++)
                            widths[i] = Math.Max(widths[i], 45);

                        double total2 = widths.Sum();
                        if (total2 > pageWidth)
                        {
                            double scale = pageWidth / total2;
                            for (int i = 0; i < colCount; i++) widths[i] *= scale;
                        }

                        return widths;
                    }

                    void DrawTableHeader(double[] colWidths)
                    {
                        double x = marginLeft;
                        double rowH = 18;

                        gfx.DrawRectangle(XBrushes.LightGray, marginLeft, y - rowH + 4, pageWidth, rowH);

                        for (int i = 0; i < cols.Count; i++)
                        {
                            gfx.DrawString(cols[i].HeaderText, headerFont, XBrushes.Black,
                                new XRect(x + 3, y - rowH + 6, colWidths[i] - 6, rowH),
                                XStringFormats.TopLeft);

                            gfx.DrawLine(XPens.Gray, x, y - rowH + 4, x, y - rowH + 4 + rowH);
                            x += colWidths[i];
                        }

                        gfx.DrawLine(XPens.Gray, marginLeft + pageWidth, y - rowH + 4, marginLeft + pageWidth, y - rowH + 4 + rowH);
                        gfx.DrawLine(XPens.Gray, marginLeft, y + 4, marginLeft + pageWidth, y + 4);

                        y += 18;
                    }

                    void DrawRow(DataGridViewRow r, double[] colWidths)
                    {
                        double x = marginLeft;
                        double rowH = 16;

                        for (int i = 0; i < cols.Count; i++)
                        {
                            string cellText = (r.Cells[cols[i].Name].Value?.ToString() ?? "").Trim();

                            gfx.DrawString(cellText, contentFont, XBrushes.Black,
                                new XRect(x + 3, y - rowH + 3, colWidths[i] - 6, rowH),
                                XStringFormats.TopLeft);

                            gfx.DrawLine(XPens.LightGray, x, y - rowH + 2, x, y - rowH + 2 + rowH);
                            x += colWidths[i];
                        }

                        gfx.DrawLine(XPens.LightGray, marginLeft + pageWidth, y - rowH + 2, marginLeft + pageWidth, y - rowH + 2 + rowH);
                        gfx.DrawLine(XPens.LightGray, marginLeft, y + 2, marginLeft + pageWidth, y + 2);

                        y += rowH;
                    }

                    void EnsureSpace(double needed)
                    {
                        if (y + needed > marginTop + pageHeight)
                            NewPage();
                    }

                    DrawHeader();

                    foreach (var g in groups)
                    {
                        EnsureSpace(30);
                        gfx.DrawString($"JOURNÉE : {g.Key:dd/MM/yyyy}", sectionFont, XBrushes.Black, new XPoint(marginLeft, y));
                        y += 16;

                        gfx.DrawLine(XPens.Black, marginLeft, y, marginLeft + pageWidth, y);
                        y += 10;

                        var colWidths = ComputeColumnWidths(g.Take(50).ToList());
                        DrawTableHeader(colWidths);

                        decimal totalEntree = 0m;
                        decimal totalSortie = 0m;

                        foreach (var r in g)
                        {
                            EnsureSpace(22);
                            DrawRow(r, colWidths);

                            string typeOp = (r.Cells["TypeOperation"]?.Value?.ToString() ?? "").Trim().ToUpperInvariant();
                            decimal q = 0m;
                            decimal.TryParse((r.Cells["Quantite"]?.Value?.ToString() ?? "0").Trim(),
                                NumberStyles.Any, CultureInfo.InvariantCulture, out q);

                            if (typeOp == "ENTREE") totalEntree += q;
                            else if (typeOp == "SORTIE") totalSortie += q;

                            if (y + 22 > marginTop + pageHeight)
                            {
                                NewPage();
                                EnsureSpace(40);
                                gfx.DrawString($"JOURNÉE : {g.Key:dd/MM/yyyy} (suite)", sectionFont, XBrushes.Black, new XPoint(marginLeft, y));
                                y += 16;
                                gfx.DrawLine(XPens.Black, marginLeft, y, marginLeft + pageWidth, y);
                                y += 10;
                                DrawTableHeader(colWidths);
                            }
                        }

                        EnsureSpace(30);
                        y += 8;
                        gfx.DrawString($"TOTAL ENTREE : {totalEntree:N0} | TOTAL SORTIE : {totalSortie:N0}",
                            new XFont("Arial", 10, XFontStyle.Bold), XBrushes.Black, new XPoint(marginLeft, y));
                        y += 18;

                        gfx.DrawLine(XPens.Black, marginLeft, y, marginLeft + pageWidth, y);
                        y += 12;
                    }

                    document.Save(sfd.FileName);
                    MessageBox.Show("Export PDF réussi !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur export PDF : " + ex.Message);
                }
            }
        }

        private DateTime GetDateFromRow(DataGridViewRow row, string colName)
        {
            try
            {
                if (row.Cells[colName]?.Value == null) return DateTime.Today;
                return Convert.ToDateTime(row.Cells[colName].Value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return DateTime.Today;
            }
        }

        public class ComboboxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public ComboboxItem(string text, int value) { Text = text; Value = value; }
            public override string ToString() => Text;
        }
    }
}