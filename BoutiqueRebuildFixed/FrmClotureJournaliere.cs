using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmClotureJournaliere : FormBase
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;

        public FrmClotureJournaliere()
        {
            InitializeComponent();
            
            ChargerInfos();



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
        private void ChargerInfos()
        {
            CultureInfo fr = new CultureInfo("fr-FR");

            // ===== INFOS GÉNÉRALES =====
            txtDate.Text = DateTime.Now.ToString("dddd dd MMMM yyyy", fr);
            txtCaissier.Text = SessionEmploye.Prenom;

            txtDate.TextAlign = HorizontalAlignment.Center;
            txtCaissier.TextAlign = HorizontalAlignment.Center;

            // =========================
            // ===== JOUR =====
            // =========================
            CalculTotauxJour(out decimal eFC, out decimal sFC, out decimal eUSD, out decimal sUSD);

            decimal bFC = eFC - sFC;
            decimal bUSD = eUSD - sUSD;

            txtEntreesFC.Text = eFC.ToString("N2");
            txtSortiesFC.Text = sFC.ToString("N2");
            txtBalanceFC.Text = bFC.ToString("N2");

            txtEntreesUSD.Text = eUSD.ToString("N2");
            txtSortiesUSD.Text = sUSD.ToString("N2");
            txtBalanceUSD.Text = bUSD.ToString("N2");

            // =========================
            // ===== SEMAINE =====
            // =========================
            CalculerTotauxSemaine(out decimal eFCS, out decimal sFCS, out decimal eUSDS, out decimal sUSDS);

            decimal bFCS = eFCS - sFCS;
            decimal bUSDS = eUSDS - sUSDS;

            txtEntreesFC_Semaine.Text = eFCS.ToString("N2");
            txtSortiesFC_Semaine.Text = sFCS.ToString("N2");
            txtBalanceFC_Semaine.Text = bFCS.ToString("N2");

            txtEntreesUSD_Semaine.Text = eUSDS.ToString("N2");
            txtSortiesUSD_Semaine.Text = sUSDS.ToString("N2");
            txtBalanceUSD_Semaine.Text = bUSDS.ToString("N2");

            // =========================
            // ===== MOIS =====
            // =========================
            CalculerTotauxMois(out decimal eFCM, out decimal sFCM, out decimal eUSDM, out decimal sUSDM);

            decimal bFCM = eFCM - sFCM;
            decimal bUSDM = eUSDM - sUSDM;

            txtEntreesFC_Mois.Text = eFCM.ToString("N2");
            txtSortiesFC_Mois.Text = sFCM.ToString("N2");
            txtBalanceFC_Mois.Text = bFCM.ToString("N2");

            txtEntreesUSD_Mois.Text = eUSDM.ToString("N2");
            txtSortiesUSD_Mois.Text = sUSDM.ToString("N2");
            txtBalanceUSD_Mois.Text = bUSDM.ToString("N2");
        }
        private decimal TotalVersementSimNet(string devise)
        {
            decimal versement = TotalParMotif("VERSEMENT SIM", devise);
            decimal soustraction = TotalParMotif("SOUSTRACTION SIM", devise);

            return versement - soustraction;
        }
        private void CalculTotauxJour(out decimal eFC, out decimal sFC, out decimal eUSD, out decimal sUSD)
        {
            eFC = sFC = eUSD = sUSD = 0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TypeMouvement, Montant, Devise
                    FROM MouvementsCaisse
                    WHERE CAST(DateHeure AS DATE) = CAST(GETDATE() AS DATE)";

                SqlCommand cmd = new SqlCommand(query, con);
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
        private void CalculerTotauxSemaine(out decimal eFC, out decimal sFC, out decimal eUSD, out decimal sUSD)
        {
            eFC = sFC = eUSD = sUSD = 0;

            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime lundi = today.AddDays(-diff);
            DateTime dimanche = lundi.AddDays(7);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TypeMouvement, Montant, Devise
                    FROM MouvementsCaisse
                    WHERE DateHeure >= @Debut AND DateHeure < @Fin";

                SqlCommand cmd = new SqlCommand(query, con);
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
                string query = @"
        SELECT TypeMouvement, Montant, Devise
        FROM MouvementsCaisse
        WHERE DateHeure >= @Debut AND DateHeure < @Fin";

                SqlCommand cmd = new SqlCommand(query, con);
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
        private decimal TotalMensuelParMotif(string motif, string devise)
        {
            decimal total = 0;

            DateTime debutMois = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime finMois = debutMois.AddMonths(1);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT SUM(Montant)
        FROM MouvementsCaisse
        WHERE Motif = @Motif
          AND Devise = @Devise
          AND TypeMouvement = 'Entrée'
          AND DateHeure >= @Debut
          AND DateHeure < @Fin";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Motif", motif);
                cmd.Parameters.AddWithValue("@Devise", devise);
                cmd.Parameters.AddWithValue("@Debut", debutMois);
                cmd.Parameters.AddWithValue("@Fin", finMois);

                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                    total = Convert.ToDecimal(result);
            }

            return total;
        }
        private string ChargerObservationsDuJour()
        {
            string observation = "";

            string connectionString = ConfigSysteme.ConnectionString;

            string query = @"
        SELECT TOP 1 Observation
        FROM ClotureJournaliere
        WHERE CAST(DateCloture AS DATE) = CAST(GETDATE() AS DATE)
        ORDER BY Id DESC";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        observation = result.ToString();
                    }
                }
            }

            return observation.Trim();
        }
        private void DrawCenteredText(
    XGraphics gfx,
    string text,
    XFont font,
    int y,
    PdfPage page)
        {
            gfx.DrawString(
                text,
                font,
                XBrushes.Black,
                new XRect(0, y, page.Width, 20),
                XStringFormats.TopCenter
            );
        }
        private decimal CalculerTotalEspece(string devise)
        {
            decimal total = 0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
        SELECT 
            SUM(
                CASE 
                    WHEN Motif = 'SOUSTRACTION SIM' THEN 0
                    WHEN TypeMouvement = 'Entrée' THEN Montant
                    WHEN TypeMouvement = 'Sortie' THEN -Montant
                    ELSE 0
                END
            ) AS Total
        FROM MouvementsCaisse
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

        private decimal TotalParMotif(string motif, string devise)
        {
            decimal total = 0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
            SELECT ISNULL(SUM(Montant), 0)
            FROM MouvementsCaisse
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

        private void FrmClotureJournaliere_Load(object sender, EventArgs e)
        {

        }

        private void BtnValiderCloture_Click(object sender, EventArgs e)
        {
            CalculTotauxJour(out decimal eFC, out decimal sFC, out decimal eUSD, out decimal sUSD);

            decimal bFC = sFC - eFC;
            decimal bUSD = sUSD - eUSD;

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    string query = @"
INSERT INTO ClotureJournaliere
(DateCloture, IdCaissier, NomCaissier,
 EntreesFC, SortiesFC, PhotoFC, VenteFC, BalanceFC,
 EntreesUSD, SortiesUSD, PhotoUSD, VenteUSD, BalanceUSD,
 Observation)
VALUES
(@Date, @Id, @Nom,
 @eFC, @sFC, @pFC, @vFC, @bFC,
 @eUSD, @sUSD, @pUSD, @vUSD, @bUSD,
 @Obs)";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Date", DateTime.Today);
                    cmd.Parameters.AddWithValue("@Id", SessionEmploye.ID_Employe);
                    cmd.Parameters.AddWithValue("@Nom", SessionEmploye.Prenom);

                    cmd.Parameters.AddWithValue("@eFC", eFC);
                    cmd.Parameters.AddWithValue("@sFC", sFC);
                    cmd.Parameters.AddWithValue("@pFC", 0m);  // à adapter selon ta logique
                    cmd.Parameters.AddWithValue("@vFC", 0m);  // idem
                    cmd.Parameters.AddWithValue("@bFC", bFC);

                    cmd.Parameters.AddWithValue("@eUSD", eUSD);
                    cmd.Parameters.AddWithValue("@sUSD", sUSD);
                    cmd.Parameters.AddWithValue("@pUSD", 0m);  // à adapter
                    cmd.Parameters.AddWithValue("@vUSD", 0m);  // à adapter
                    cmd.Parameters.AddWithValue("@bUSD", bUSD);

                    cmd.Parameters.AddWithValue("@Obs", txtObservation.Text.Trim());

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                // Audit log - Clôture journalière réussie
                ConfigSysteme.AjouterAuditLog(
                    "ClotureJournaliere",
                    $"Clôture journalière validée par l'employé ID={SessionEmploye.ID_Employe}, Nom={SessionEmploye.Prenom}. " +
                    $"Balance FC={bFC}, Balance USD={bUSD}, Observation={txtObservation.Text.Trim()}",
                    "Succès"
                );

                MessageBox.Show("Clôture journalière validée avec succès ✅");
                Close();
            }
            catch (Exception ex)
            {
                // Audit log - Erreur lors de la clôture
                ConfigSysteme.AjouterAuditLog(
                    "ClotureJournaliere",
                    $"Erreur lors de la validation de la clôture journalière par employé ID={SessionEmploye.ID_Employe}: {ex.Message}",
                    "Échec"
                );

                MessageBox.Show("Erreur lors de la validation : " + ex.Message);
            }
        }

        private void BtnAnnuler_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnExporterPDF_Click(object sender, EventArgs e)
        {
            // ===== JOUR =====
            CalculTotauxJour(out decimal eFC, out decimal sFC, out decimal eUSD, out decimal sUSD);
            decimal bFC = eFC - sFC;   // Entrées - Sorties
            decimal bUSD = eUSD - sUSD;

            // ===== SEMAINE =====
            CalculerTotauxSemaine(out decimal eFCS, out decimal sFCS, out decimal eUSDS, out decimal sUSDS);
            decimal bFCS = eFCS - sFCS;
            decimal bUSDS = eUSDS - sUSDS;

            // ===== MOIS =====
            CalculerTotauxMois(out decimal eFCM, out decimal sFCM, out decimal eUSDM, out decimal sUSDM);
            decimal bFCM = eFCM - sFCM;
            decimal bUSDM = eUSDM - sUSDM;

            decimal totalEspeceFC = CalculerTotalEspece("FC");
            decimal totalEspeceUSD = CalculerTotalEspece("USD");

            decimal versementBankFC = TotalParMotif("VERSEMENT BANK", "FC");
            decimal versementBankUSD = TotalParMotif("VERSEMENT BANK", "USD");

            decimal versementSimFC = TotalVersementSimNet("FC");
            decimal versementSimUSD = TotalVersementSimNet("USD");

            decimal envoiPatronneFC = TotalParMotif("ENVOI PATRONNE", "FC");
            decimal envoiPatronneUSD = TotalParMotif("ENVOI PATRONNE", "USD");

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Fichier PDF (*.pdf)|*.pdf",
                FileName = $"Cloture_{DateTime.Today:dd_MM_yyyy}.pdf"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            // ================= PDF =================
            PdfDocument doc = new PdfDocument();
            doc.Info.Title = $"Clôture Caisse - {DateTime.Today:dd/MM/yyyy}";

            PdfPage page = doc.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            page.Orientation = PdfSharpCore.PageOrientation.Portrait;

            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Marges A4
            const int MLeft = 50;
            const int MRight = 50;
            const int MTop = 40;
            const int MBottom = 50;

            double contentWidth = page.Width.Point - MLeft - MRight;

            // Fonts
            XFont fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            XFont fontSubTitle = new XFont("Arial", 11, XFontStyle.Bold);
            XFont fontHeader = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontText = new XFont("Arial", 10, XFontStyle.Regular);
            XFont fontSmall = new XFont("Arial", 9, XFontStyle.Regular);

            // Couleurs (sobre)
            XBrush brushBlack = XBrushes.Black;

            XPen penBorder = new XPen(XColors.Black, 0.8);
            XPen penGrid = new XPen(XColors.Black, 0.5);
            XBrush brushHeaderBg = new XSolidBrush(XColor.FromArgb(245, 245, 245));

            int y = MTop;

            // ============ Helpers ============
            void EnsureSpace(int needHeight)
            {
                if (y + needHeight > page.Height.Point - MBottom)
                {
                    page = doc.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    page.Orientation = PdfSharpCore.PageOrientation.Portrait;
                    gfx = XGraphics.FromPdfPage(page);
                    y = MTop;
                }
            }

            void DrawCentered(string text, XFont font, int h = 18)
            {
                gfx.DrawString(text ?? "", font, brushBlack,
                    new XRect(MLeft, y, contentWidth, h),
                    XStringFormats.TopCenter);
                y += h;
            }

            void DrawLeftRight(string left, string right, XFont font, int h = 18)
            {
                gfx.DrawString(left ?? "", font, brushBlack,
                    new XRect(MLeft, y, contentWidth * 0.6, h),
                    XStringFormats.TopLeft);

                gfx.DrawString(right ?? "", font, brushBlack,
                    new XRect(MLeft + contentWidth * 0.6, y, contentWidth * 0.4, h),
                    XStringFormats.TopRight);

                y += h;
            }

            void DrawHr(int spaceBefore = 8, int spaceAfter = 10)
            {
                y += spaceBefore;
                gfx.DrawLine(penBorder, MLeft, y, MLeft + contentWidth, y);
                y += spaceAfter;
            }

            void DrawBox(string title, int innerPaddingTop, Action drawContent)
            {
                EnsureSpace(120);

                int startY = y;

                gfx.DrawString(title, fontSubTitle, brushBlack,
                    new XRect(MLeft, y, contentWidth, 18), XStringFormats.TopLeft);
                y += 20;

                y += innerPaddingTop;

                drawContent?.Invoke();

                int endY = y;

                int boxPadding = 10;
                int top = startY - 6;
                int height = (endY - top) + boxPadding;

                gfx.DrawRectangle(penBorder, MLeft - 6, top, contentWidth + 12, height);

                y += 18;
            }

            // ================= TABLE (GRID PRO) =================
            // Colonnes fixes (mêmes largeurs partout)
            double colDevW = contentWidth * 0.18;
            double colEntW = contentWidth * 0.27;
            double colSorW = contentWidth * 0.27;
            double colBalW = contentWidth * 0.28;

            double xDev = MLeft;
            double xEnt = xDev + colDevW;
            double xSor = xEnt + colEntW;
            double xBal = xSor + colSorW;

            int rowH = 20;

            void DrawCell(string text, XFont font, double x, double w, int yTop, XStringFormat fmt, bool header = false)
            {
                if (header)
                    gfx.DrawRectangle(brushHeaderBg, x, yTop, w, rowH);

                gfx.DrawString(text ?? "", font, brushBlack,
                    new XRect(x + 6, yTop + 4, w - 12, rowH),
                    fmt);
            }

            void DrawTableHeader()
            {
                EnsureSpace(50);

                int top = y;

                // Fond header + bordure
                gfx.DrawRectangle(penGrid, xDev, top, contentWidth, rowH);

                DrawCell("DEV.", fontHeader, xDev, colDevW, top, XStringFormats.TopLeft, true);
                DrawCell("ENTRÉES", fontHeader, xEnt, colEntW, top, XStringFormats.TopRight, true);
                DrawCell("SORTIES", fontHeader, xSor, colSorW, top, XStringFormats.TopRight, true);
                DrawCell("BALANCE", fontHeader, xBal, colBalW, top, XStringFormats.TopRight, true);

                // Traits verticaux
                gfx.DrawLine(penGrid, xEnt, top, xEnt, top + rowH);
                gfx.DrawLine(penGrid, xSor, top, xSor, top + rowH);
                gfx.DrawLine(penGrid, xBal, top, xBal, top + rowH);

                y += rowH;
            }

            void DrawTableRow(string dev, decimal ent, decimal sor, decimal bal)
            {
                EnsureSpace(40);

                int top = y;

                // Bordure de ligne
                gfx.DrawRectangle(penGrid, xDev, top, contentWidth, rowH);

                // Cellules
                DrawCell(dev, fontText, xDev, colDevW, top, XStringFormats.TopLeft);
                DrawCell(ent.ToString("N2"), fontText, xEnt, colEntW, top, XStringFormats.TopRight);
                DrawCell(sor.ToString("N2"), fontText, xSor, colSorW, top, XStringFormats.TopRight);
                DrawCell(bal.ToString("N2"), fontText, xBal, colBalW, top, XStringFormats.TopRight);

                // Traits verticaux
                gfx.DrawLine(penGrid, xEnt, top, xEnt, top + rowH);
                gfx.DrawLine(penGrid, xSor, top, xSor, top + rowH);
                gfx.DrawLine(penGrid, xBal, top, xBal, top + rowH);

                y += rowH;
            }

            void DrawMotifLine(string libelle, decimal fc, decimal usd)
            {
                EnsureSpace(20);
                string txt = $"{libelle} : FC {fc:N2} | USD {usd:N2}";
                gfx.DrawString(txt, fontText, brushBlack,
                    new XRect(MLeft, y, contentWidth, 16), XStringFormats.TopLeft);
                y += 16;
            }

            // ================= ENTÊTE PRO =================
            DrawCentered("ZAIRE MODE SARL", fontTitle, 22);
            DrawCentered("23, Bld Lumumba, Q1 Masina Sans Fil", fontSmall, 14);
            DrawCentered("RCCM: 25-B-01497 | ID.NAT: 01-F4300-N73258E", fontSmall, 18);

            DrawHr(6, 10);

            DrawCentered("MOUVEMENTS DE CAISSE - CLÔTURE JOURNALIÈRE", fontHeader, 20);

            DrawHr(0, 12);

            // Infos
            EnsureSpace(30);
            DrawLeftRight($"Date : {DateTime.Now:dddd dd MMMM yyyy}", $"Caissier : {SessionEmploye.Prenom}", fontText, 18);
            DrawHr(4, 10);

            // ================= BLOC JOUR =================
            DrawBox("RÉCAPITULATIF DU JOUR", 0, () =>
            {
                EnsureSpace(80);
                DrawTableHeader();
                DrawTableRow("FC", eFC, sFC, bFC);
                DrawTableRow("USD", eUSD, sUSD, bUSD);
            });

            // ================= OBSERVATIONS =================
            string observations = ChargerObservationsDuJour();
            if (!string.IsNullOrWhiteSpace(observations))
            {
                DrawBox("OBSERVATIONS DU JOUR", 0, () =>
                {
                    EnsureSpace(40);
                    gfx.DrawLine(penBorder, MLeft, y, MLeft + contentWidth, y);
                    y += 10;

                    foreach (string line in observations.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        EnsureSpace(16);
                        gfx.DrawString(line, fontText, brushBlack, new XRect(MLeft, y, contentWidth, 16), XStringFormats.TopLeft);
                        y += 14;
                    }
                });
            }

            // ================= SEMAINE =================
            DrawBox("RÉCAPITULATIF HEBDOMADAIRE", 0, () =>
            {
                EnsureSpace(80);
                DrawTableHeader();
                DrawTableRow("FC", eFCS, sFCS, bFCS);
                DrawTableRow("USD", eUSDS, sUSDS, bUSDS);
            });

            // ================= MOIS =================
            DrawBox("RÉCAPITULATIF MENSUEL", 0, () =>
            {
                EnsureSpace(80);
                DrawTableHeader();
                DrawTableRow("FC", eFCM, sFCM, bFCM);
                DrawTableRow("USD", eUSDM, sUSDM, bUSDM);
            });

            // ================= DETAILS MENSUELS =================
            DrawBox("DÉTAIL DES RECETTES MENSUELLES", 0, () =>
            {
                EnsureSpace(130);

                DrawMotifLine("Argent Photos",
                    TotalMensuelParMotif("Argent Photos", "FC"),
                    TotalMensuelParMotif("Argent Photos", "USD"));

                DrawMotifLine("Vente",
                    TotalMensuelParMotif("Vente", "FC"),
                    TotalMensuelParMotif("Vente", "USD"));

                y += 8;

                DrawMotifLine("TOTAL GÉNÉRAL ESPÈCE (ARGENT EN MAIN)", totalEspeceFC, totalEspeceUSD);

                y += 8;

                DrawMotifLine("VERSEMENT BANK (Information)", versementBankFC, versementBankUSD);
                DrawMotifLine("VERSEMENT SIM (Information)", versementSimFC, versementSimUSD);
                DrawMotifLine("ENVOI PATRONNE (Information)", envoiPatronneFC, envoiPatronneUSD);
            });

            // ================= SIGNATURES =================
            EnsureSpace(40);

            gfx.DrawLine(penBorder, MLeft, y, MLeft + contentWidth, y);
            y += 18;

            string sigCaissier = "Signature du Caissier : " + SessionEmploye.Prenom + " " + SessionEmploye.Nom;
            string sigAdmin = "Signature Administration : MESSIE MATALA";

            gfx.DrawString(sigCaissier, fontText, brushBlack,
                new XRect(MLeft, y, contentWidth * 0.5, 18), XStringFormats.TopLeft);

            gfx.DrawString(sigAdmin, fontText, brushBlack,
                new XRect(MLeft + contentWidth * 0.5, y, contentWidth * 0.5, 18), XStringFormats.TopRight);

            // ================= FOOTER =================
            y += 25;
            EnsureSpace(20);
            gfx.DrawString($"Document généré le {DateTime.Now:dd/MM/yyyy HH:mm}", fontSmall, brushBlack,
                new XRect(MLeft, y, contentWidth, 16), XStringFormats.TopLeft);

            // Sauvegarde
            doc.Save(sfd.FileName);
            doc.Close();

            MessageBox.Show("PDF exporté avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtEntreesFC_Mois_TextChanged(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }
    }
}
