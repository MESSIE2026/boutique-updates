using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmPresenceAbsence : Form
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        public FrmPresenceAbsence()
        {
            CultureInfo fr = new CultureInfo("fr-FR");
            Thread.CurrentThread.CurrentCulture = fr;
            Thread.CurrentThread.CurrentUICulture = fr;

            InitializeComponent();
            InitialiserFormulaire();
            ChargerGrille();

            // Charger traductions dynamiques

            // Écoute les changements globaux
            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        private void RafraichirLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);
        }

        private string GetValideurNomComplet()
        {
            string nom = (SessionEmploye.Nom ?? "").Trim();
            string prenom = (SessionEmploye.Prenom ?? "").Trim();
            string full = (prenom + " " + nom).Trim();
            return string.IsNullOrWhiteSpace(full) ? "ADMIN" : full;
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
            cmbSexe.Items.Clear();
            cmbSexe.Items.Add("Homme");
            cmbSexe.Items.Add("Femme");

            // 📅 Date en français
            dtpJourDate.Format = DateTimePickerFormat.Custom;
            dtpJourDate.CustomFormat = "dddd dd MMMM yyyy";

            // ⏰ Heures en format 24h
            dtpHeureEntree.Format = DateTimePickerFormat.Custom;
            dtpHeureEntree.CustomFormat = "HH:mm";
            dtpHeureEntree.ShowUpDown = true;

            dtpHeureSortie.Format = DateTimePickerFormat.Custom;
            dtpHeureSortie.CustomFormat = "HH:mm";
            dtpHeureSortie.ShowUpDown = true;

            dgvPresenceAbsence.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPresenceAbsence.ReadOnly = true;
            dgvPresenceAbsence.MultiSelect = false;
            dgvPresenceAbsence.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvPresenceAbsence.CellClick += DgvPresenceAbsence_CellClick;
            dgvPresenceAbsence.CellFormatting -= DgvPresenceAbsence_CellFormatting; // si tu l'as déjà
            dgvPresenceAbsence.CellPainting += DgvPresenceAbsence_CellPainting;
            dgvPresenceAbsence.DataError += DgvPresenceAbsence_DataError;
            ChargerEmployesCombo();
            cmbEmploye.SelectedIndexChanged -= cmbEmploye_SelectedIndexChanged;
            cmbEmploye.SelectedIndexChanged += cmbEmploye_SelectedIndexChanged;
        }

        private void cmbEmploye_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbEmploye.SelectedItem is DataRowView drv)
            {
                txtNomPrenom.Text = drv["NomPrenom"].ToString();
                cmbSexe.Text = drv["Sexe"]?.ToString();
            }
        }

        private void ChargerEmployesCombo()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT ID_Employe,
                   LTRIM(RTRIM(Prenom)) + ' ' + LTRIM(RTRIM(Nom)) AS NomPrenom,
                   Sexe
            FROM dbo.Employes
            ORDER BY Prenom, Nom;", con);

                da.Fill(dt);

                cmbEmploye.DataSource = dt;
                cmbEmploye.DisplayMember = "NomPrenom";
                cmbEmploye.ValueMember = "ID_Employe";

                cmbEmploye.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmbEmploye.AutoCompleteSource = AutoCompleteSource.ListItems;
            }
        }

        // ================= CHARGEMENT DATAGRIDVIEW =================
        private void ChargerGrille()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string sql = @"
SELECT 
    pa.Id,
    pa.ID_Employe,
    COALESCE(LTRIM(RTRIM(e.Nom)) + ' ' + LTRIM(RTRIM(e.Prenom)), pa.NomPrenom) AS NomPrenom,
    COALESCE(e.Sexe, pa.Sexe) AS Sexe,
    pa.JourDate,
    pa.HeureEntree,
    pa.HeureSortie,
    pa.Absent,
    pa.Present,
    pa.Retard,
    pa.Repos,
    pa.Observations,
    pa.HeuresTravail,
    pa.StatutValidation,
    pa.DateValidation,
    pa.ValidePar
FROM dbo.PresenceAbsenceAgents pa
LEFT JOIN dbo.Employes e ON e.ID_Employe = pa.ID_Employe
ORDER BY pa.JourDate DESC, pa.Id DESC;";   // ✅ ICI : fermeture du @""; obligatoire

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvPresenceAbsence.DataSource = dt;

                if (dgvPresenceAbsence.Columns.Contains("Id"))
                    dgvPresenceAbsence.Columns["Id"].Visible = false;

                if (dgvPresenceAbsence.Columns.Contains("ID_Employe"))
                    dgvPresenceAbsence.Columns["ID_Employe"].Visible = false;
            }
        }
        private void DrawCenteredTextLocal(XGraphics gfx, string text, double y, XFont font, PdfPage page)
        {
            gfx.DrawString(
                text,
                font,
                XBrushes.Black,
                new XRect(0, y, page.Width, 20),
                XStringFormats.TopCenter
            );
        }
        private void DgvPresenceAbsence_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            string[] boolColumns = { "Absent", "Present", "Retard", "Repos" };
            if (Array.Exists(boolColumns, c => c == dgvPresenceAbsence.Columns[e.ColumnIndex].Name))
            {
                if (e.Value != null && e.Value != DBNull.Value && (bool)e.Value)
                {
                    e.Value = "X";
                    e.FormattingApplied = true;
                }
                else
                {
                    e.Value = "";
                    e.FormattingApplied = true;
                }
            }
        }
        private void DgvPresenceAbsence_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvPresenceAbsence.Rows[e.RowIndex];

            txtNomPrenom.Text = row.Cells["NomPrenom"].Value?.ToString();
            cmbSexe.Text = row.Cells["Sexe"].Value?.ToString();

            if (row.Cells["JourDate"].Value != DBNull.Value)
                dtpJourDate.Value = Convert.ToDateTime(row.Cells["JourDate"].Value);

            if (row.Cells["HeureEntree"].Value != DBNull.Value)
                dtpHeureEntree.Value = DateTime.Today + (TimeSpan)row.Cells["HeureEntree"].Value;

            if (row.Cells["HeureSortie"].Value != DBNull.Value)
                dtpHeureSortie.Value = DateTime.Today + (TimeSpan)row.Cells["HeureSortie"].Value;

            chkAbsent.Checked = row.Cells["Absent"].Value != DBNull.Value && (bool)row.Cells["Absent"].Value;
            chkPresent.Checked = row.Cells["Present"].Value != DBNull.Value && (bool)row.Cells["Present"].Value;
            chkRetard.Checked = row.Cells["Retard"].Value != DBNull.Value && (bool)row.Cells["Retard"].Value;
            chkRepos.Checked = row.Cells["Repos"].Value != DBNull.Value && (bool)row.Cells["Repos"].Value;

            txtObservations.Text = row.Cells["Observations"].Value?.ToString();
        }
        private void NettoyerChamps()
        {
            txtNomPrenom.Clear();
            cmbSexe.SelectedIndex = -1;
            dtpJourDate.Value = DateTime.Today;
            dtpHeureEntree.Value = DateTime.Today;
            dtpHeureSortie.Value = DateTime.Today;
            chkAbsent.Checked = false;
            chkPresent.Checked = false;
            chkRetard.Checked = false;
            chkRepos.Checked = false;
            txtObservations.Clear();
        }
        private void CalculerTotalsMensuels()
        {
            int totalAbsent = 0;
            int totalPresent = 0;
            int totalRetard = 0;
            int totalRepos = 0;

            int mois = dtpJourDate.Value.Month;
            int annee = dtpJourDate.Value.Year;

            foreach (DataGridViewRow row in dgvPresenceAbsence.Rows)
            {
                if (row.IsNewRow) continue;

                // ✅ Date sûre
                if (row.Cells["JourDate"].Value == null || row.Cells["JourDate"].Value == DBNull.Value)
                    continue;

                DateTime dateRow = Convert.ToDateTime(row.Cells["JourDate"].Value);

                if (dateRow.Month != mois || dateRow.Year != annee)
                    continue;

                // ✅ Bool sûr
                bool GetBool(DataGridViewRow r, string col)
                {
                    var v = r.Cells[col].Value;
                    return v != null && v != DBNull.Value && Convert.ToBoolean(v);
                }

                bool absent = GetBool(row, "Absent");
                bool present = GetBool(row, "Present");
                bool retard = GetBool(row, "Retard");
                bool repos = GetBool(row, "Repos");

                if (absent) totalAbsent++;
                if (present) totalPresent++;
                if (retard) totalRetard++;
                if (repos) totalRepos++;
            }

            lblTotalAbsent.Text = $"Absent : {totalAbsent}";
            lblTotalPresent.Text = $"Présent : {totalPresent}";
            lblTotalRetard.Text = $"Retard : {totalRetard}";
            lblTotalRepos.Text = $"Repos : {totalRepos}";
        }
        private void lblDateJour_Click(object sender, EventArgs e)
        {

        }

        private void cmbSexe_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            // 1) Employé obligatoire (ID vient du combo)
            if (cmbEmploye.SelectedValue == null || !int.TryParse(cmbEmploye.SelectedValue.ToString(), out int idEmploye))
            {
                MessageBox.Show("Choisir un employé.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2) Champs affichage (optionnel, mais tu peux stocker en historique)
            string nomPrenom = txtNomPrenom.Text.Trim();
            if (string.IsNullOrWhiteSpace(nomPrenom))
                nomPrenom = cmbEmploye.Text.Trim(); // fallback

            string sexe = cmbSexe.SelectedItem?.ToString()?.Trim() ?? "";
            DateTime jourDate = dtpJourDate.Value.Date;

            // 3) Statuts
            bool absent = chkAbsent.Checked;
            bool present = chkPresent.Checked;
            bool retard = chkRetard.Checked;
            bool repos = chkRepos.Checked;

            // ✅ Anti incohérence
            int checkedCount = (absent ? 1 : 0) + (present ? 1 : 0) + (repos ? 1 : 0);
            if (checkedCount == 0)
            {
                MessageBox.Show("Choisir au moins un statut: Présent / Absent / Repos.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (checkedCount > 1)
            {
                MessageBox.Show("Un agent ne peut pas être Présent et Absent (ou Repos) le même jour.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4) Heures (DateTimePicker.Value n'est jamais null -> on décide nous-mêmes si on utilise ou pas)
            TimeSpan? heureEntree = dtpHeureEntree.Value.TimeOfDay;
            TimeSpan? heureSortie = dtpHeureSortie.Value.TimeOfDay;

            // ✅ Si Absent/Repos : pas d'heures + pas de retard
            if (absent || repos)
            {
                heureEntree = null;
                heureSortie = null;
                retard = false;
            }

            // ✅ Si Présent mais tu ne veux pas forcer la sortie/entrée:
            //    - Si tu veux autoriser “présent sans sortie” => heuresTravail NULL (normal)
            //    - Si tu veux obliger entrée/sortie quand présent => décommente ce bloc
            /*
            if (present)
            {
                if (!heureEntree.HasValue)
                {
                    MessageBox.Show("Heure d'entrée requise quand l'agent est présent.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!heureSortie.HasValue)
                {
                    MessageBox.Show("Heure de sortie requise quand l'agent est présent.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            */

            // 5) ✅ Calcul HeuresTravail (BON MOMENT : après statut + heures)
            decimal? heuresTravail = null;
            if (present && heureEntree.HasValue && heureSortie.HasValue)
            {
                TimeSpan diff = heureSortie.Value - heureEntree.Value;

                // Sortie après minuit
                if (diff.TotalMinutes < 0)
                    diff = diff.Add(TimeSpan.FromHours(24));

                heuresTravail = (decimal)Math.Round(diff.TotalHours, 2);
            }

            string observations = txtObservations.Text.Trim();

            // 6) UPSERT : si déjà pointage (ID_Employe, JourDate) => UPDATE, sinon INSERT
            // (index unique UX_PresenceJour recommandé)
            string sql = @"
IF EXISTS (SELECT 1 FROM dbo.PresenceAbsenceAgents WHERE ID_Employe = @ID_Employe AND JourDate = @JourDate)
BEGIN
    UPDATE dbo.PresenceAbsenceAgents
    SET NomPrenom = @NomPrenom,
        Sexe = @Sexe,
        HeureEntree = @HeureEntree,
        HeureSortie = @HeureSortie,
        Absent = @Absent,
        Present = @Present,
        Retard = @Retard,
        Repos = @Repos,
        Observations = @Observations,
        HeuresTravail = @HeuresTravail
    WHERE ID_Employe = @ID_Employe AND JourDate = @JourDate;
END
ELSE
BEGIN
    INSERT INTO dbo.PresenceAbsenceAgents
    (NomPrenom, Sexe, JourDate, HeureEntree, HeureSortie,
     Absent, Present, Retard, Repos, Observations,
     ID_Employe, HeuresTravail, StatutValidation, DateValidation, ValidePar)
    VALUES
    (@NomPrenom, @Sexe, @JourDate, @HeureEntree, @HeureSortie,
     @Absent, @Present, @Retard, @Repos, @Observations,
     @ID_Employe, @HeuresTravail, N'NonValidé', NULL, NULL);
END;
";

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();

                    cmd.Parameters.AddWithValue("@ID_Employe", idEmploye);
                    cmd.Parameters.AddWithValue("@NomPrenom", nomPrenom);

                    // Si sexe vide => NULL (ou garde vide si tu préfères)
                    cmd.Parameters.AddWithValue("@Sexe", string.IsNullOrWhiteSpace(sexe) ? (object)DBNull.Value : sexe);

                    cmd.Parameters.AddWithValue("@JourDate", jourDate);

                    cmd.Parameters.AddWithValue("@HeureEntree", (object)heureEntree ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HeureSortie", (object)heureSortie ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@Absent", absent);
                    cmd.Parameters.AddWithValue("@Present", present);
                    cmd.Parameters.AddWithValue("@Retard", retard);
                    cmd.Parameters.AddWithValue("@Repos", repos);

                    cmd.Parameters.AddWithValue("@Observations", string.IsNullOrWhiteSpace(observations) ? (object)DBNull.Value : observations);
                    cmd.Parameters.AddWithValue("@HeuresTravail", (object)heuresTravail ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                ChargerGrille();
                CalculerTotalsMensuels();
                NettoyerChamps();

                MessageBox.Show(
                    $"Pointage enregistré.\nHeuresTravail: {(heuresTravail.HasValue ? heuresTravail.Value.ToString("0.00") : "N/A")}",
                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnCalculerTotals_Click(object sender, EventArgs e)
        {
            CalculerTotalsMensuels();
        }

        private void DgvPresenceAbsence_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Empêche la boîte de dialogue “Type incorrect”
            e.ThrowException = false;
            e.Cancel = true;
        }

        private void DgvPresenceAbsence_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // ✅ Protection indices
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.ColumnIndex >= dgvPresenceAbsence.Columns.Count) return;

            var col = dgvPresenceAbsence.Columns[e.ColumnIndex];
            if (col == null) return;

            string colName = col.Name;
            if (colName != "Absent" && colName != "Present" && colName != "Retard" && colName != "Repos")
                return;

            // Paint normal (bordures, fond, etc.)
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            bool isTrue = false;
            if (e.Value != null && e.Value != DBNull.Value)
                isTrue = Convert.ToBoolean(e.Value);

            if (isTrue)
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    "X",
                    new Font(dgvPresenceAbsence.Font, FontStyle.Bold),
                    e.CellBounds,
                    Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }

            e.Handled = true;
        }

        private void btnExporterPDF_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF files (*.pdf)|*.pdf";
            sfd.FileName = $"Presence_Journalier_{DateTime.Today:yyyyMMdd}.pdf";

            if (sfd.ShowDialog() != DialogResult.OK) return;

            PdfDocument document = new PdfDocument();
            document.Info.Title = "Présences et Absences Journalières";

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            double marginLeft = 20;
            double marginRight = 20;
            double y = 20;

            XFont titleFont = new XFont("Arial", 14, XFontStyle.Bold);
            XFont headerFont = new XFont("Arial", 9, XFontStyle.Bold);
            XFont normalFont = new XFont("Arial", 9);
            XFont checkFont = new XFont("Arial", 11, XFontStyle.Bold);

            string dateDuJour = DateTime.Today.ToString("dd/MM/yyyy");

            // ===== ENTETE DOCUMENT =====
            DrawCenteredTextLocal(gfx, "ZAIRE MODE SARL", y, titleFont, page);
            y += 25;

            DrawCenteredTextLocal(gfx, "23, Bld Lumumba, Q1 Masina Sans Fil", y, normalFont, page);
            y += 15;

            DrawCenteredTextLocal(gfx, "RCCM: 25-B-01497 | ID.NAT: 01-F4300-N73258E", y, normalFont, page);
            y += 25;

            DrawCenteredTextLocal(gfx, $"RAPPORT JOURNALIER DES PRESENCES ET ABSENCES – {dateDuJour}",
                y, new XFont("Arial", 12, XFontStyle.Bold), page);

            y += 25;
            gfx.DrawLine(XPens.Black, marginLeft, y, page.Width - marginRight, y);
            y += 15;

            // ===== TABLE =====
            double[] columnWidths = { 120, 35, 70, 55, 55, 45, 45, 45, 45, 60 };
            string[] headers = { "Nom & Prénom", "Sexe", "Date", "Entrée", "Sortie", "Absent", "Présent", "Retard", "Repos", "Observations" };
            string[] colNames = { "NomPrenom", "Sexe", "JourDate", "HeureEntree", "HeureSortie", "Absent", "Present", "Retard", "Repos", "Observations" };

            double rowHeight = 18;

            void DrawTableHeader()
            {
                double xh = marginLeft;
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawRectangle(XPens.Black, xh, y, columnWidths[i], rowHeight);
                    gfx.DrawString(headers[i], headerFont, XBrushes.Black,
                        new XRect(xh, y, columnWidths[i], rowHeight),
                        XStringFormats.Center);
                    xh += columnWidths[i];
                }
                y += rowHeight;
            }

            DrawTableHeader();

            int totalPresent = 0, totalAbsent = 0, totalRetard = 0, totalRepos = 0;

            foreach (DataGridViewRow row in dgvPresenceAbsence.Rows)
            {
                if (row.IsNewRow) continue;

                // filtrer sur aujourd'hui
                if (row.Cells["JourDate"].Value == null || row.Cells["JourDate"].Value == DBNull.Value) continue;
                DateTime dateRow = Convert.ToDateTime(row.Cells["JourDate"].Value);
                if (dateRow.Date != DateTime.Today) continue;

                double x = marginLeft;

                for (int i = 0; i < colNames.Length; i++)
                {
                    string col = colNames[i];
                    string value = "";

                    bool isBool = (col == "Absent" || col == "Present" || col == "Retard" || col == "Repos");

                    if (col == "JourDate")
                    {
                        value = dateDuJour;
                    }
                    else if (isBool)
                    {
                        var v = row.Cells[col].Value;
                        bool b = v != null && v != DBNull.Value && Convert.ToBoolean(v);
                        value = b ? "X" : "";
                    }
                    else if (col == "HeureEntree" || col == "HeureSortie")
                    {
                        var v = row.Cells[col].Value;
                        if (v == null || v == DBNull.Value) value = "";
                        else if (v is TimeSpan ts) value = ts.ToString(@"hh\:mm");
                        else value = v.ToString();
                    }
                    else
                    {
                        value = row.Cells[col].Value?.ToString() ?? "";
                    }

                    gfx.DrawRectangle(XPens.Black, x, y, columnWidths[i], rowHeight);
                    gfx.DrawString(value, isBool ? checkFont : normalFont, XBrushes.Black,
                        new XRect(x, y, columnWidths[i], rowHeight),
                        XStringFormats.Center);

                    x += columnWidths[i];
                }

                // totals
                if (row.Cells["Present"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["Present"].Value)) totalPresent++;
                if (row.Cells["Absent"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["Absent"].Value)) totalAbsent++;
                if (row.Cells["Retard"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["Retard"].Value)) totalRetard++;
                if (row.Cells["Repos"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["Repos"].Value)) totalRepos++;

                y += rowHeight;

                // saut page
                if (y > page.Height - 90)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                    DrawTableHeader();
                }
            }

            // ===== RECAP (APRES la boucle) =====
            y += 20;
            gfx.DrawLine(XPens.Black, marginLeft, y, page.Width - marginRight, y);
            y += 15;

            gfx.DrawString($"Total Présents : {totalPresent}", headerFont, XBrushes.Black, marginLeft, y); y += 15;
            gfx.DrawString($"Total Absents : {totalAbsent}", headerFont, XBrushes.Black, marginLeft, y); y += 15;
            gfx.DrawString($"Total Retards : {totalRetard}", headerFont, XBrushes.Black, marginLeft, y); y += 15;
            gfx.DrawString($"Total Repos : {totalRepos}", headerFont, XBrushes.Black, marginLeft, y); y += 40;

            gfx.DrawString("Signature de l’Administration :", normalFont, XBrushes.Black, marginLeft, y);
            y += 20;
            gfx.DrawString("MESSIE MATALA", new XFont("Arial", 10, XFontStyle.Bold), XBrushes.Black, marginLeft, y);

            document.Save(sfd.FileName);
            document.Close();

            MessageBox.Show("Rapport journalier exporté avec succès ✔", "Succès",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void FrmPresenceAbsence_Load(object sender, EventArgs e)
        {

        }

        private void btnValider_Click(object sender, EventArgs e)
        {
            // ✅ droits au début
            if (string.IsNullOrWhiteSpace(SessionEmploye.Poste) ||
                !(SessionEmploye.Poste.Equals("Directeur", StringComparison.OrdinalIgnoreCase) ||
                  SessionEmploye.Poste.Equals("Superviseur", StringComparison.OrdinalIgnoreCase) ||
                  SessionEmploye.Poste.Equals("RH", StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Vous n'avez pas l'autorisation de valider.", "Accès refusé",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvPresenceAbsence.CurrentRow == null)
            {
                MessageBox.Show("Sélectionne une ligne.");
                return;
            }

            // ✅ Id existe dans le DataSource (même si colonne cachée)
            int id = Convert.ToInt32(dgvPresenceAbsence.CurrentRow.Cells["Id"].Value);

            var statut = dgvPresenceAbsence.CurrentRow.Cells["StatutValidation"].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(statut) && statut.Equals("Validé", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Cette ligne est déjà validée.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string validePar = GetValideurNomComplet();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.PresenceAbsenceAgents
SET StatutValidation = N'Validé',
    DateValidation = GETDATE(),
    ValidePar = @ValidePar
WHERE Id = @Id;", con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ValidePar", validePar);
                    cmd.ExecuteNonQuery();
                }
            }

            ChargerGrille();
            MessageBox.Show($"Validé ✅ par {validePar}");
        }
    }
}
