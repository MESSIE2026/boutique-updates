using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace BoutiqueRebuildFixed
{
    public partial class FrmEntreesSortiesCaisse : Form
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;

        public FrmEntreesSortiesCaisse()
        {
            InitializeComponent();

            ChargerType();
            ChargerMotifs();

            dgvMouvements.AutoGenerateColumns = true;

            LoadMouvements();
            UpdateResume();
            ClearInputs();

            TimerDateHeure.Enabled = true;
            cmbDevise.Items.Add("FC");
            cmbDevise.Items.Add("USD");
            cmbDevise.SelectedIndex = 0;

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
        private void ChargerType()
        {
            cmbType.Items.Clear();
            cmbType.Items.AddRange(new string[] { "Entrée", "Sortie", "Autres", });
        }
        private void CalculerTotauxSemaine(
    out decimal eFC,
    out decimal sFC,
    out decimal eUSD,
    out decimal sUSD)
        {
            eFC = sFC = eUSD = sUSD = 0;

            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime lundi = today.AddDays(-diff);
            DateTime dimanche = lundi.AddDays(7);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string q = @"
        SELECT TypeMouvement, Montant, Devise
        FROM MouvementsCaisse
        WHERE DateHeure >= @Debut AND DateHeure < @Fin";

                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@Debut", lundi);
                cmd.Parameters.AddWithValue("@Fin", dimanche);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string type = dr["TypeMouvement"].ToString();
                    string devise = dr["Devise"].ToString();
                    decimal montant = Convert.ToDecimal(dr["Montant"]);

                    if (devise == "FC")
                    {
                        if (type == "Entrée") eFC += montant;
                        else if (type == "Sortie") sFC += montant;
                    }
                    else if (devise == "USD")
                    {
                        if (type == "Entrée") eUSD += montant;
                        else if (type == "Sortie") sUSD += montant;
                    }
                }
            }
        }
        private void CalculerTotauxMois(
    out decimal eFC,
    out decimal sFC,
    out decimal eUSD,
    out decimal sUSD)
        {
            eFC = sFC = eUSD = sUSD = 0;

            DateTime debutMois = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime finMois = debutMois.AddMonths(1);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string q = @"
        SELECT TypeMouvement, Montant, Devise
        FROM MouvementsCaisse
        WHERE DateHeure >= @Debut AND DateHeure < @Fin";

                SqlCommand cmd = new SqlCommand(q, con);
                cmd.Parameters.AddWithValue("@Debut", debutMois);
                cmd.Parameters.AddWithValue("@Fin", finMois);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string type = dr["TypeMouvement"].ToString();
                    string devise = dr["Devise"].ToString();
                    decimal montant = Convert.ToDecimal(dr["Montant"]);

                    if (devise == "FC")
                    {
                        if (type == "Entrée") eFC += montant;
                        else if (type == "Sortie") sFC += montant;
                    }
                    else if (devise == "USD")
                    {
                        if (type == "Entrée") eUSD += montant;
                        else if (type == "Sortie") sUSD += montant;
                    }
                }
            }
        }
        public decimal CalculerTotalEspece(string devise)
        {
            decimal total = 0;

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                string sql = @"
            SELECT 
                SUM(
                    CASE 
                        WHEN TypeOperation = 'Entree' THEN Montant
                        WHEN TypeOperation = 'Sortie' THEN -Montant
                        ELSE 0
                    END
                ) AS Total
            FROM EntreesSortiesCaisse
            WHERE Devise = @Devise
        ";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Devise", devise);

                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                        total = Convert.ToDecimal(result);
                }
            }

            return total;
        }
        private void DrawCenteredText(XGraphics gfx, string text, XFont font, int y, PdfPage page)
        {
            gfx.DrawString(text, font, XBrushes.Black,
                new XRect(0, y, page.Width, 20),
                XStringFormats.TopCenter);
        }
        private void CalculTotauxJour(
    out decimal eFC,
    out decimal sFC,
    out decimal eUSD,
    out decimal sUSD)
        {
            eFC = sFC = eUSD = sUSD = 0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string q = @"
        SELECT TypeMouvement, Montant, Devise
        FROM MouvementsCaisse
        WHERE CAST(DateHeure AS DATE) = CAST(GETDATE() AS DATE)";

                SqlCommand cmd = new SqlCommand(q, con);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string type = dr["TypeMouvement"].ToString();
                    string devise = dr["Devise"].ToString();
                    decimal montant = Convert.ToDecimal(dr["Montant"]);

                    if (devise == "FC")
                    {
                        if (type == "Entrée") eFC += montant;
                        else if (type == "Sortie") sFC += montant;
                    }
                    else if (devise == "USD")
                    {
                        if (type == "Entrée") eUSD += montant;
                        else if (type == "Sortie") sUSD += montant;
                    }
                }
            }
        }
        string NormaliserType(object valeur)
        {
            if (valeur == null) return "";

            string t = valeur.ToString().Trim().ToUpper();

            t = t.Replace("É", "E")
                 .Replace("È", "E")
                 .Replace("Ê", "E")
                 .Replace("Ë", "E");

            return t;
        }


        private void ChargerMotifs()
        {
            cmbMotif.Items.Clear();
            cmbMotif.Items.AddRange(new string[]
            {
                "Fonds de caisse",
                "Vente",
                "Dépense divers",
                "Approvisionnement",
                "Paiement fournisseur",
                "Paiement Agent",
                "Location Voile",
                "Imprimerie",
                "Service Evenement Complet",
                "Remboursement",
                "Argent Photos",
                "Restauration",
                "Transport",
                "Poubelle",
                "Eau Fontaine",
                "Achat Outils de travail",
                "Eau Fontaine",
                "Versement SIM",
                "Versement BANK",
                "Soustraction SIM",
                "Envoi Patronne",
                "TOTAL GENERAL ESPECE",
                "Versement Sim ou banque"
            });
        }
        private void LoadMouvements()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT DateHeure, TypeMouvement, Montant, Motif, Description, IdCaissier, NomCaissier, AutorisePar, Devise
                                   FROM MouvementsCaisse ORDER BY DateHeure DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // ⚠ SUPPRIME TOUTES LES COLONNES AVANT DE RECHARGER
                    dgvMouvements.Columns.Clear();

                    dgvMouvements.AutoGenerateColumns = true;
                    dgvMouvements.DataSource = dt;

                    FormatDataGridView();
                    UpdateResume();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement mouvements : " + ex.Message);
            }
        }
        public decimal TotalParMotif(string motif, string devise)
        {
            decimal total = 0;

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                string sql = @"
            SELECT ISNULL(SUM(Montant), 0)
            FROM EntreesSortiesCaisse
            WHERE Motif = @Motif AND Devise = @Devise
        ";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Motif", motif);
                    cmd.Parameters.AddWithValue("@Devise", devise);

                    total = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }

            return total;
        }
        public void UpdateResume()
        {
            if (!(dgvMouvements.DataSource is DataTable dt)) return;

            decimal eFC = 0, sFC = 0, eUSD = 0, sUSD = 0;
            DateTime today = DateTime.Today;

            foreach (DataRow row in dt.Rows)
            {
                if (!DateTime.TryParse(row["DateHeure"]?.ToString(), out DateTime d))
                    continue;

                if (d.Date != today)
                    continue;

                string type = row["TypeMouvement"]?.ToString();
                string devise = row["Devise"]?.ToString();
                decimal.TryParse(row["Montant"]?.ToString(), out decimal montant);

                if (devise == "FC")
                {
                    if (type == "Entrée") eFC += montant;
                    else if (type == "Sortie") sFC += montant;
                }
                else if (devise == "USD")
                {
                    if (type == "Entrée") eUSD += montant;
                    else if (type == "Sortie") sUSD += montant;
                }
            }

            lblTotalEntrees.Text = $"Entrées : {eFC:N2} FC | {eUSD:N2} USD";
            lblTotalSorties.Text = $"Sorties : {sFC:N2} FC | {sUSD:N2} USD";

            lblBalanceNette.Text =
                $"Balance FC : {(eFC - sFC):N2} FC\n" +
                $"Balance USD : {(eUSD - sUSD):N2} USD";
        }
        private void FormatDataGridView()
        {
        }
        private void ClearInputs()
        {
            cmbType.SelectedIndex = -1;
            txtMontant.Clear();
            cmbMotif.SelectedIndex = -1;
            txtDescription.Clear();
        }
        private void TimerDateHeure_Tick(object sender, EventArgs e)
        {
            lblDateHeure.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void FrmEntreesSortiesCaisse_Load(object sender, EventArgs e)
        {

        }

        private void BtnEnregistrer_Click(object sender, EventArgs e)
        {
            // ===== VALIDATIONS =====
            if (cmbType.SelectedIndex == -1)
            {
                MessageBox.Show("Veuillez sélectionner le type (Entrée / Sortie).");
                return;
            }

            if (cmbDevise.SelectedIndex == -1)
            {
                MessageBox.Show("Veuillez sélectionner la devise (FC / USD).");
                return;
            }

            if (!decimal.TryParse(
                    txtMontant.Text.Trim().Replace(',', '.'),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal montant) || montant <= 0)
            {
                MessageBox.Show("Veuillez entrer un montant valide.");
                return;
            }

            if (cmbMotif.SelectedIndex == -1)
            {
                MessageBox.Show("Veuillez sélectionner un motif.");
                return;
            }

            if (SessionEmploye.ID_Employe <= 0)
            {
                MessageBox.Show("ID employé non valide. Veuillez vous reconnecter.");
                return;
            }

            // ===== INSERTION =====
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    string query = @"
        INSERT INTO MouvementsCaisse
        (DateHeure, TypeMouvement, Montant, Devise,
         Motif, Description,
         IdCaissier, NomCaissier, AutorisePar)
        VALUES
        (@DateHeure, @TypeMouvement, @Montant, @Devise,
         @Motif, @Description,
         @IdCaissier, @NomCaissier, @AutorisePar)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@DateHeure", DateTime.Now);
                        cmd.Parameters.AddWithValue("@TypeMouvement", cmbType.SelectedItem.ToString()); // Entrée / Sortie
                        cmd.Parameters.AddWithValue("@Montant", montant);
                        cmd.Parameters.AddWithValue("@Devise", cmbDevise.SelectedItem.ToString());     // FC / USD
                        cmd.Parameters.AddWithValue("@Motif", cmbMotif.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@IdCaissier", SessionEmploye.ID_Employe);
                        cmd.Parameters.AddWithValue("@NomCaissier", SessionEmploye.Prenom);
                        cmd.Parameters.AddWithValue("@AutorisePar", "MESSIE MATALA");

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Audit log - insertion réussie
                ConfigSysteme.AjouterAuditLog(
                    "MouvementsCaisse",
                    $"Mouvement '{cmbType.SelectedItem}' de {montant} {cmbDevise.SelectedItem} ajouté par employé ID={SessionEmploye.ID_Employe}, Nom={SessionEmploye.Prenom}. Motif: {cmbMotif.SelectedItem}, Description: {txtDescription.Text.Trim()}",
                    "Succès"
                );

                MessageBox.Show("Mouvement ajouté avec succès ✅");

                ClearInputs();
                LoadMouvements();
                UpdateResume();
            }
            catch (Exception ex)
            {
                // Audit log - erreur lors de l'insertion
                ConfigSysteme.AjouterAuditLog(
                    "MouvementsCaisse",
                    $"Erreur lors de l'ajout du mouvement par employé ID={SessionEmploye.ID_Employe}: {ex.Message}",
                    "Échec"
                );

                MessageBox.Show("Erreur lors de l'ajout : " + ex.Message);
            }
        }

        private void BtnAnnuler_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        private static string Truncate(string txt, int max)
        {
            if (string.IsNullOrEmpty(txt)) return "";
            if (max <= 3) return txt.Length <= max ? txt : txt.Substring(0, max); // sécurité
            return txt.Length <= max ? txt : txt.Substring(0, max - 3) + "...";
        }

        private void BtnExporterPDF_Click(object sender, EventArgs e)
        {
            // ✅ 1) Date de référence (une date quelconque dans la semaine à exporter)
            // Idéal: prends la valeur d’un DateTimePicker (ex: dtpJour.Value.Date)
            DateTime dateReference = DateTime.Today; // ou dtpJour.Value.Date

            // ✅ 2) Calcul Lundi 00:00 → Dimanche 23:59:59.999
            DateTime debutSemaine = dateReference.Date;
            // Lundi = 0, Mardi=1 ... Dimanche=6
            int diff = ((int)debutSemaine.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            debutSemaine = debutSemaine.AddDays(-diff);

            DateTime finSemaine = debutSemaine.AddDays(7).AddTicks(-1); // dimanche 23:59:59.9999999

            string Truncate(string txt, int max)
            {
                if (string.IsNullOrEmpty(txt)) return "";
                return txt.Length <= max ? txt : txt.Substring(0, max - 3) + "...";
            }

            List<string> WrapText(XGraphics gfx, string text, XFont font, double maxWidth)
            {
                var lines = new List<string>();
                if (string.IsNullOrWhiteSpace(text))
                {
                    lines.Add("");
                    return lines;
                }

                text = text.Replace("\r", " ").Replace("\n", " ");
                string[] words = text.Split(' ');
                string line = "";

                foreach (string word in words)
                {
                    string test = string.IsNullOrEmpty(line) ? word : line + " " + word;
                    if (gfx.MeasureString(test, font).Width <= maxWidth)
                        line = test;
                    else
                    {
                        lines.Add(line);
                        line = word;
                    }
                }

                if (!string.IsNullOrEmpty(line))
                    lines.Add(line);

                return lines;
            }

            try
            {
                // ✅ 3) Filtre SEMAINE (Lundi → Dimanche)
                var lignesFiltrees = dgvMouvements.Rows.Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow
                        && DateTime.TryParse(r.Cells["DateHeure"].Value?.ToString(), out DateTime d)
                        && d >= debutSemaine
                        && d <= finSemaine)
                    .ToList();

                if (lignesFiltrees.Count == 0)
                {
                    MessageBox.Show($"Aucun mouvement du {debutSemaine:dd/MM/yyyy} au {finSemaine:dd/MM/yyyy}.");
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "PDF (*.pdf)|*.pdf";
                    sfd.FileName = $"MouvementsCaisse_{debutSemaine:yyyyMMdd}_au_{finSemaine:yyyyMMdd}.pdf";
                    if (sfd.ShowDialog() != DialogResult.OK) return;

                    PdfDocument doc = new PdfDocument();

                    XFont titleFont = new XFont("Arial", 16, XFontStyle.Bold);
                    XFont headerFont = new XFont("Arial", 11, XFontStyle.Bold);
                    XFont cellFont = new XFont("Arial", 10);
                    XFont normalFont = new XFont("Arial", 10);

                    int[] colWidths = { 110, 65, 90, 100, 280, 90, 100 };
                    int totalWidth = colWidths.Sum();

                    int baseRowHeight = 22;
                    int pad = 6;
                    int margeBas = 70;

                    PdfPage page = null;
                    XGraphics gfx = null;
                    int currentY = 0;
                    bool premierePage = true;

                    decimal totalEntreeFC = 0, totalSortieFC = 0;
                    decimal totalEntreeUSD = 0, totalSortieUSD = 0;

                    void DrawCentered(string text, XFont font, int y)
                    {
                        gfx.DrawString(text, font, XBrushes.Black,
                            new XRect(0, y, page.Width, 20),
                            XStringFormats.TopCenter);
                    }

                    void NouvellePage()
                    {
                        gfx?.Dispose();
                        page = doc.AddPage();
                        page.Size = PdfSharpCore.PageSize.A4;
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        gfx = XGraphics.FromPdfPage(page);

                        int tableX = (int)((page.Width - totalWidth) / 2);

                        if (premierePage)
                        {
                            int y = 25;
                            DrawCentered("ZAIRE MODE SARL", titleFont, y); y += 22;
                            DrawCentered("23, Bld Lumumba, Q1 Masina Sans Fil", normalFont, y); y += 15;
                            DrawCentered("RCCM: 25-B-01497 | ID.NAT: 01-F4300-N73258E", normalFont, y); y += 15;
                            DrawCentered("Référence : Immeuble Masina Plaza", normalFont, y); y += 18;

                            gfx.DrawLine(XPens.Black, tableX, y, tableX + totalWidth, y);
                            y += 12;

                            // ✅ Titre semaine
                            DrawCentered($"MOUVEMENTS DE CAISSE – SEMAINE DU {debutSemaine:dd/MM/yyyy} AU {finSemaine:dd/MM/yyyy}",
                                headerFont, y);
                            y += 25;

                            currentY = y;

                            string[] headers = { "Date", "Type", "Montant", "Motif", "Description", "Caissier", "Autorisé" };
                            int x = tableX;
                            for (int i = 0; i < headers.Length; i++)
                            {
                                gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, x, currentY, colWidths[i], baseRowHeight);
                                gfx.DrawString(headers[i], headerFont, XBrushes.Black,
                                    new XRect(x, currentY + 4, colWidths[i], baseRowHeight),
                                    XStringFormats.TopCenter);
                                x += colWidths[i];
                            }
                            currentY += baseRowHeight;
                            premierePage = false;
                        }
                        else
                        {
                            currentY = 40;
                        }
                    }

                    NouvellePage();
                    int tableX2 = (int)((page.Width - totalWidth) / 2);

                    foreach (var r in lignesFiltrees)
                    {
                        DateTime d = DateTime.Parse(r.Cells["DateHeure"].Value.ToString());
                        string type = r.Cells["TypeMouvement"].Value.ToString().ToUpper();
                        string devise = r.Cells["Devise"].Value.ToString().ToUpper();
                        decimal montant = Convert.ToDecimal(r.Cells["Montant"].Value);

                        if (type.Contains("ENT"))
                        {
                            if (devise == "FC") totalEntreeFC += montant;
                            if (devise == "USD") totalEntreeUSD += montant;
                        }
                        else if (type.Contains("SORT"))
                        {
                            if (devise == "FC") totalSortieFC += montant;
                            if (devise == "USD") totalSortieUSD += montant;
                        }

                        string description = r.Cells["Description"].Value?.ToString();
                        var descLines = WrapText(gfx, description, cellFont, colWidths[4] - pad * 2);
                        int rowHeight = Math.Max(baseRowHeight, descLines.Count * 12 + pad * 2);

                        if (currentY + rowHeight > page.Height - margeBas)
                            NouvellePage();

                        string[] vals =
                        {
                    d.ToString("dd/MM/yyyy HH:mm"),
                    type,
                    $"{montant:N2} {devise}",
                    r.Cells["Motif"].Value?.ToString(),
                    "",
                    r.Cells["NomCaissier"].Value?.ToString(),
                    r.Cells["AutorisePar"].Value?.ToString()
                };

                        int x = tableX2;
                        for (int i = 0; i < vals.Length; i++)
                        {
                            gfx.DrawRectangle(XPens.Black, x, currentY, colWidths[i], rowHeight);

                            if (i == 4)
                            {
                                double yText = currentY + pad;
                                foreach (var line in descLines)
                                {
                                    gfx.DrawString(line, cellFont, XBrushes.Black,
                                        new XRect(x + pad, yText, colWidths[i] - pad * 2, 12),
                                        XStringFormats.TopLeft);
                                    yText += 12;
                                }
                            }
                            else
                            {
                                gfx.DrawString(vals[i], cellFont, XBrushes.Black,
                                    new XRect(x + pad, currentY + pad, colWidths[i], rowHeight),
                                    (i == 1 || i == 2) ? XStringFormats.Center : XStringFormats.TopLeft);
                            }
                            x += colWidths[i];
                        }
                        currentY += rowHeight;
                    }

                    // ... (tes totaux/balances comme avant)

                    gfx.Dispose();
                    doc.Save(sfd.FileName);
                    Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    MessageBox.Show("PDF généré (semaine Lundi → Dimanche) ✔️");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur export PDF : " + ex.Message);
            }
        }
    }
}