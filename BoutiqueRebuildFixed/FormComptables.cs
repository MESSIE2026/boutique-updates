using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using PdfDoc = iTextSharp.text.Document;
using PdfFont = iTextSharp.text.Font;
using PdfBaseFont = iTextSharp.text.pdf.BaseFont;
using PdfWriter = iTextSharp.text.pdf.PdfWriter;
using PdfPTable = iTextSharp.text.pdf.PdfPTable;
using PdfPCell = iTextSharp.text.pdf.PdfPCell;
using PdfPhrase = iTextSharp.text.Phrase;
using PdfParagraph = iTextSharp.text.Paragraph;
using PdfPageSize = iTextSharp.text.PageSize;
using PdfElement = iTextSharp.text.Element;
using PdfColor = iTextSharp.text.BaseColor;
using ClosedXML.Excel;
namespace BoutiqueRebuildFixed
{
    public partial class FormComptables : FormBase
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        public Dictionary<string, decimal> RevenusParDevise { get; set; }
        public Dictionary<string, decimal> DepensesParDevise { get; set; }

        // ========================
        // UI
        // ========================
        private TableLayoutPanel root;
        private Panel header;
        private Label lblTitle;
        private Label lblPeriode;

        private FlowLayoutPanel periodTabs;
        private Button btnJour, btnSemaine, btnMois, btnAnnee;

        private TableLayoutPanel kpiRow;
        private Panel kpiRevenus, kpiDepenses, kpiFlux, kpiBenefice;

        private Chart chartCashflow;
        private Chart chartExpensesPie;

        private DataGridView dgvTransactions;
        private DataGridView dgvFactures;
        private DataGridView dgvSalaires;

        private FlowLayoutPanel reportsPanel;
        private Button btnBilan, btnResultat, btnGrandLivre, btnReleve;

        private FlowLayoutPanel bottomActions;
        private Button btnExportPdf, btnGenExcel, btnSimu, btnPrevision;

        // State période
        private DateTime dateDebut;
        private DateTime dateFin;

        // Couleurs (style “grande entreprise”)
        private readonly Color C_Header1 = Color.FromArgb(10, 36, 84);
        private readonly Color C_Header2 = Color.FromArgb(7, 24, 58);
        private readonly Color C_Back = Color.FromArgb(245, 247, 251);
        private readonly Color C_Card = Color.White;
        private readonly Color C_CardBorder = Color.FromArgb(214, 224, 240);
        private readonly Color C_White = Color.White;

        private readonly Color C_Title = Color.FromArgb(18, 27, 44);
        private readonly Color C_SubTitle = Color.FromArgb(80, 92, 110);

        // KPI accents (petit bandeau gauche)
        private readonly Color C_AccentBlue = Color.FromArgb(30, 109, 230);
        private readonly Color C_AccentRed = Color.FromArgb(231, 76, 60);
        private readonly Color C_AccentGreen = Color.FromArgb(46, 204, 113);
        private readonly Color C_AccentPurple = Color.FromArgb(155, 89, 182);

        // Mode de période
        private enum PeriodMode { Jour, Semaine, Mois, Annee }
        private PeriodMode _mode = PeriodMode.Mois;

        // Combo période
        private ComboBox cmbPeriode;
        private Panel scrollHost;

        // Item Combo
        private sealed class PeriodItem
        {
            public DateTime Debut { get; set; }
            public DateTime Fin { get; set; }
            public string Label { get; set; }
            public bool HasData { get; set; }

            public override string ToString()
                => HasData ? Label : (Label + " (vide)");
        }
        public FormComptables()
        {
            InitializeComponent();
            // ✅ Log ouverture ici (ou dans Load, mais pas les deux)
            AuditLogger.Log("VIEW", "Ouverture FormComptables");

            Text = "Tableau de Bord Comptable";
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;

            AutoScaleMode = AutoScaleMode.Font;
            Font = new Font("Segoe UI", 10.5f, FontStyle.Regular);
            BackColor = C_Back;

            // ✅ IMPORTANT : le scroll est géré par scrollHost, pas le Form
            this.AutoScroll = false;

            Size = new Size(1386, 788);
            MinimumSize = new Size(1200, 680);

            BuildUI();
            WireEvents();

            // mode initial : Mois
            _mode = PeriodMode.Mois;
            SetActiveTab(btnMois);
            ChargerListePeriodes(); // sélectionne la période et refresh déjà

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;

            this.Load += FormComptables_Load;

        }



        private void FormComptables_Load(object sender, EventArgs e)
        {
            // Charger traductions dynamiques

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

        private void ApplyPeriod(DateTime d1, DateTime d2, bool forceRefresh = true)
        {
            // Normaliser : début = 00:00:00, fin = 23:59:59.999...
            d1 = d1.Date;
            d2 = d2.Date.AddDays(1).AddTicks(-1);

            bool changed = (dateDebut != d1) || (dateFin != d2);

            dateDebut = d1;
            dateFin = d2;

            UpdatePeriodeLabel();

            if (forceRefresh || changed)
                RefreshAll();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AuditLogger.Log("VIEW", "Fermeture FormComptables");
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }
        private static readonly string[] MOTIFS_TRANSFERT = new[]
{
    "VERSEMENT SIM",
    "VERSEMENT BANK",
    "VERSEMENT SIM OU BANQUE",
    "ENVOI PATRONNE",
    "TOTAL GENERAL ESPECE"
};
        private string FormatMultiDevise(Dictionary<string, decimal> data)
        {
            if (data == null || data.Count == 0)
                return "0";

            StringBuilder sb = new StringBuilder();
            foreach (var kv in data)
                sb.AppendLine($"{kv.Value:N0} {kv.Key}");

            return sb.ToString().Trim();
        }
        private Dictionary<string, decimal> ExecParDeviseKpi(
    SqlConnection con,
    string typeMouvement,
    DateTime d1,
    DateTime d2)
        {
            var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            string sql = @"
SELECT 
    CASE 
        WHEN UPPER(Devise) IN ('USD', '$', 'DOLLAR') THEN 'USD'
        ELSE 'CDF'
    END AS Devise,
    ISNULL(SUM(Montant),0) AS Total
FROM MouvementsCaisse
WHERE TypeMouvement = @type
  AND DateHeure BETWEEN @d1 AND @d2
  AND UPPER(LTRIM(RTRIM(Motif))) NOT IN (" + string.Join(",", MOTIFS_TRANSFERT.Select(m => $"'{m}'")) + @")
GROUP BY 
    CASE 
        WHEN UPPER(Devise) IN ('USD', '$', 'DOLLAR') THEN 'USD'
        ELSE 'CDF'
    END";

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@type", SqlDbType.NVarChar, 20).Value = typeMouvement;
                cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = d1;
                cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = d2;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string devise = Convert.ToString(rd["Devise"]);
                        decimal total = ToDec(rd["Total"]);
                        result[devise] = total;
                    }
                }
            }

            return result;
        }
        private Dictionary<string, decimal> ExecParDevise(
    SqlConnection con,
    string sql,
    DateTime d1,
    DateTime d2)
        {
            var result = new Dictionary<string, decimal>();

            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = d1;
                cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = d2;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string devise = rd["Devise"].ToString();
                        decimal montant = ToDec(rd["Total"]);
                        result[devise] = montant;
                    }
                }
            }
            return result;
        }

        private void InsererEcriture(
    DateTime date,
    string piece,
    string journal,
    string compte,
    string libelle,
    decimal debit,
    decimal credit,
    string devise,
    string sourceTable,
    int sourceId)
        {
            int idEntreprise = GetCurrentEntrepriseId();
            int idMagasin = GetCurrentMagasinId();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
INSERT INTO EcrituresComptables
(DateEcriture,Piece,Journal,Compte,Libelle,Debit,Credit,Devise,SourceTable,SourceId,Utilisateur,IdEntreprise,IdMagasin)
VALUES
(@d,@p,@j,@c,@l,@de,@cr,@dv,@st,@sid,@u,@ie,@im)";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@d", SqlDbType.DateTime).Value = date;
                    cmd.Parameters.Add("@p", SqlDbType.NVarChar, 50).Value = piece ?? "";
                    cmd.Parameters.Add("@j", SqlDbType.NVarChar, 10).Value = journal ?? "";
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 20).Value = compte ?? "";
                    cmd.Parameters.Add("@l", SqlDbType.NVarChar, 255).Value = libelle ?? "";

                    // ✅ décimaux typés (pro)
                    var pDe = cmd.Parameters.Add("@de", SqlDbType.Decimal);
                    pDe.Precision = 18; pDe.Scale = 2;
                    pDe.Value = debit;

                    var pCr = cmd.Parameters.Add("@cr", SqlDbType.Decimal);
                    pCr.Precision = 18; pCr.Scale = 2;
                    pCr.Value = credit;

                    cmd.Parameters.Add("@dv", SqlDbType.NVarChar, 10).Value = (devise ?? "CDF");
                    cmd.Parameters.Add("@st", SqlDbType.NVarChar, 50).Value = sourceTable ?? "";
                    cmd.Parameters.Add("@sid", SqlDbType.Int).Value = sourceId;
                    cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = Environment.UserName;

                    cmd.Parameters.Add("@ie", SqlDbType.Int).Value = idEntreprise;
                    cmd.Parameters.Add("@im", SqlDbType.Int).Value = idMagasin;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private PdfBaseFont GetUnicodeBaseFont()
        {
            // exemple : ...\bin\Debug\Assets\Fonts\segoeui.ttf
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string fontPath = Path.Combine(baseDir, "Assets", "Fonts", "segoeui.ttf");

            if (!File.Exists(fontPath))
            {
                // fallback : Helvetica (moins safe pour accents)
                return PdfBaseFont.CreateFont(PdfBaseFont.HELVETICA, PdfBaseFont.CP1252, false);
            }

            return PdfBaseFont.CreateFont(fontPath, PdfBaseFont.IDENTITY_H, PdfBaseFont.EMBEDDED);
        }

        private void InitialiserPlanComptableOHADA()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string check = "SELECT COUNT(*) FROM PlanComptableOHADA";
                using (SqlCommand cmd = new SqlCommand(check, con))
                {
                    if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                        return;
                }

                string sql = @"
INSERT INTO PlanComptableOHADA (Compte, Libelle, Classe) VALUES
('101','Capital',1),
('108','Compte de l''exploitant (Patronne)',1),
('401','Fournisseurs',4),
('421','Personnel - Salaires dus',4),
('512','Banque',5),
('531','Caisse',5),
('601','Achats',6),
('641','Charges de personnel',6),
('701','Produits',7);";

                using (SqlCommand cmdIns = new SqlCommand(sql, con))
                {
                    cmdIns.ExecuteNonQuery();
                }
            }
        }
        private void GenererEcrituresCaisse(DateTime d1, DateTime d2)
        {
            DataTable dt = FillTable(
                null,
                @"
SELECT 
    IdMouvement,
    DateHeure,
    TypeMouvement,
    Montant,
    Motif,
    Devise
FROM MouvementsCaisse
WHERE DateHeure >= @d1 AND DateHeure <= @d2",
                d1,
                d2
            );

            foreach (DataRow r in dt.Rows)
            {
                int idMv = Convert.ToInt32(r["IdMouvement"]);

                DateTime date = Convert.ToDateTime(r["DateHeure"]);

                decimal montant = ToDec(r["Montant"]);

                string devise = Convert.ToString(r["Devise"]);
                if (string.IsNullOrWhiteSpace(devise)) devise = "CDF";

                // Normaliser devise si tu utilises FC/CDF
                devise = devise.Trim().ToUpperInvariant();
                if (devise == "FC") devise = "CDF";

                string motifRaw = Convert.ToString(r["Motif"]) ?? "";
                string motif = motifRaw.Trim();
                string motifN = motif.ToUpperInvariant();

                bool entree = Convert.ToString(r["TypeMouvement"]) == "Entrée";

                bool isVersementBank = motifN == "VERSEMENT BANK";
                bool isVersementSim = (motifN == "VERSEMENT SIM" || motifN == "VERSEMENT SIM OU BANQUE");
                bool isEnvoiPatronne = motifN == "ENVOI PATRONNE";
                bool isTotalGeneral = motifN == "TOTAL GENERAL ESPECE";

                // ✅ ignorer les lignes "TOTAL..." (résumé)
                if (isTotalGeneral)
                    continue;

                // ✅ Transfert interne : Caisse (531) <-> Banque/SIM (512)
                if (isVersementBank || isVersementSim)
                {
                    if (!entree)
                    {
                        // Sortie: argent quitte la caisse vers banque/sim => Debit 512 / Credit 531
                        InsererEcriture(date, "TRF-" + idMv, "TRF", "512", "Transfert vers Banque/SIM",
                            montant, 0, devise, "MouvementsCaisse", idMv);

                        InsererEcriture(date, "TRF-" + idMv, "TRF", "531", "Transfert vers Banque/SIM",
                            0, montant, devise, "MouvementsCaisse", idMv);
                    }
                    else
                    {
                        // Entrée: argent revient banque/sim vers caisse => Debit 531 / Credit 512
                        InsererEcriture(date, "TRF-" + idMv, "TRF", "531", "Transfert depuis Banque/SIM",
                            montant, 0, devise, "MouvementsCaisse", idMv);

                        InsererEcriture(date, "TRF-" + idMv, "TRF", "512", "Transfert depuis Banque/SIM",
                            0, montant, devise, "MouvementsCaisse", idMv);
                    }
                    continue;
                }

                // ✅ Envoi Patronne : 108 / 531 (sortie)
                if (isEnvoiPatronne)
                {
                    // Si jamais c'est "Entrée" (rare), tu peux inverser, mais généralement c'est Sortie.
                    if (!entree)
                    {
                        InsererEcriture(date, "PAT-" + idMv, "OD", "108", "Retrait/Envoi Patronne",
                            montant, 0, devise, "MouvementsCaisse", idMv);

                        InsererEcriture(date, "PAT-" + idMv, "OD", "531", "Retrait/Envoi Patronne",
                            0, montant, devise, "MouvementsCaisse", idMv);
                    }
                    else
                    {
                        // Optionnel si un jour tu fais un retour patronne -> caisse
                        InsererEcriture(date, "PAT-" + idMv, "OD", "531", "Retour depuis Patronne",
                            montant, 0, devise, "MouvementsCaisse", idMv);

                        InsererEcriture(date, "PAT-" + idMv, "OD", "108", "Retour depuis Patronne",
                            0, montant, devise, "MouvementsCaisse", idMv);
                    }
                    continue;
                }

                // ✅ Sinon : écriture caisse standard (Soustraction SIM reste une dépense normale)
                InsererEcriture(
                    date,
                    "CAI-" + idMv,
                    "CAI",
                    "531",
                    motif,
                    entree ? montant : 0,
                    entree ? 0 : montant,
                    devise,
                    "MouvementsCaisse",
                    idMv
                );
            }
        }
        private void GenererEcrituresAchats(DateTime d1, DateTime d2)
        {
            DataTable dt = FillTable(
                null,
        @"
SELECT 
    ID_Achat,
    DateAchat,
    MontantTotal,
    NomFournisseur,
    Devise
FROM HistoriquesAchat
WHERE DateAchat >= @d1 AND DateAchat <= @d2",
                d1,
                d2
            );

            foreach (DataRow r in dt.Rows)
            {
                int idAchat = Convert.ToInt32(r["ID_Achat"]);
                decimal montant = ToDec(r["MontantTotal"]);
                string devise = Convert.ToString(r["Devise"]) ?? "CDF";

                // Charge d'achat
                InsererEcriture(
                    Convert.ToDateTime(r["DateAchat"]),
                    "ACH-" + idAchat,
                    "ACH",
                    "601",
                    "Achat fournisseur " + r["NomFournisseur"],
                    montant,
                    0,
                    devise,
                    "HistoriquesAchat",
                    idAchat
                );

                // Dette fournisseur
                InsererEcriture(
                    Convert.ToDateTime(r["DateAchat"]),
                    "ACH-" + idAchat,
                    "ACH",
                    "401",
                    "Dette fournisseur",
                    0,
                    montant,
                    devise,
                    "HistoriquesAchat",
                    idAchat
                );
            }
        }
        private void GenererEcrituresSalaires(DateTime d1, DateTime d2)
        {
            DataTable dt = FillTable(
                null,
        @"
SELECT 
    Id,
    DatePaiement,
    Montant,
    NomEmploye,
    Devise
FROM SalairesPaiements
WHERE DatePaiement >= @d1 AND DatePaiement <= @d2",
                d1,
                d2
            );

            foreach (DataRow r in dt.Rows)
            {
                int idSalaire = Convert.ToInt32(r["Id"]);
                decimal montant = ToDec(r["Montant"]);
                string devise = Convert.ToString(r["Devise"]) ?? "CDF";

                // Charge de personnel
                InsererEcriture(
                    Convert.ToDateTime(r["DatePaiement"]),
                    "SAL-" + idSalaire,
                    "SAL",
                    "641",
                    "Salaire " + r["NomEmploye"],
                    montant,
                    0,
                    devise,
                    "SalairesPaiements",
                    idSalaire
                );

                // Paiement / dette personnel
                InsererEcriture(
                    Convert.ToDateTime(r["DatePaiement"]),
                    "SAL-" + idSalaire,
                    "SAL",
                    "531",
                    "Paiement salaire",
                    0,
                    montant,
                    devise,
                    "SalairesPaiements",
                    idSalaire
                );
            }
        }
        private void CloturerPeriode(DateTime d1, DateTime d2)
        {
            InitialiserPlanComptableOHADA();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // 1️⃣ SUPPRIMER les écritures AUTO de la période (recalcul propre)
                string delete = @"
DELETE FROM EcrituresComptables
WHERE DateEcriture >= @d1
  AND DateEcriture <= @d2
  AND SourceTable IN ('MouvementsCaisse','HistoriquesAchat','SalairesPaiements')";

                using (SqlCommand cmd = new SqlCommand(delete, con))
                {
                    cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = d1;
                    cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = d2;
                    cmd.ExecuteNonQuery();
                }
            }

            // 2️⃣ RECRÉER LES ÉCRITURES DEPUIS LES TABLES SOURCES
            GenererEcrituresCaisse(d1, d2);
            GenererEcrituresAchats(d1, d2);
            GenererEcrituresSalaires(d1, d2);
        }
        private void BuildUI()
        {
            SuspendLayout();

            // ✅ 1) Scroll host : c'est LUI qui scrolle, pas le Form
            scrollHost = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = C_Back,
                Padding = new Padding(0, 0, 0, 90) // réserve bas
            };
            Controls.Add(scrollHost);

            // ✅ 2) Root : Dock Top + AutoSize
            root = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = C_Back,
                ColumnCount = 1,

                // ✅ Ici: 5 lignes seulement (plus de rangée boutons vide)
                RowCount = 5,

                Padding = new Padding(16, 12, 16, 8),
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize
            };

            root.ColumnStyles.Clear();
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            root.RowStyles.Clear();
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));  // Header
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));  // Tabs
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));  // KPI
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 320)); // Charts
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 300)); // Bottom section

            scrollHost.Controls.Add(root);

            BuildHeader();
            BuildTabs();
            BuildKpis();
            BuildCharts();
            BuildBottomSection();
            BuildBottomActions();
            ResumeLayout(true);
        }


        private void ExporterExcelDashboardXlsx()
        {
            using (var sfd = new SaveFileDialog { Filter = "Excel|*.xlsx", FileName = "Dashboard_Comptable.xlsx" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                using (var wb = new XLWorkbook())
                {
                    // KPI (simple)
                    var wsKpi = wb.Worksheets.Add("KPI");
                    wsKpi.Cell(1, 1).Value = "Revenus"; wsKpi.Cell(1, 2).Value = GetKpiValue(kpiRevenus);
                    wsKpi.Cell(2, 1).Value = "Dépenses"; wsKpi.Cell(2, 2).Value = GetKpiValue(kpiDepenses);
                    wsKpi.Cell(3, 1).Value = "Flux"; wsKpi.Cell(3, 2).Value = GetKpiValue(kpiFlux);
                    wsKpi.Cell(4, 1).Value = "Bénéfice"; wsKpi.Cell(4, 2).Value = GetKpiValue(kpiBenefice);
                    wsKpi.Columns().AdjustToContents();

                    AddDataGridToSheet(wb, "Transactions", dgvTransactions);
                    AddDataGridToSheet(wb, "Factures", dgvFactures);
                    AddDataGridToSheet(wb, "Salaires", dgvSalaires);

                    wb.SaveAs(sfd.FileName);
                }

                MessageBox.Show("Export Excel (.xlsx) terminé.", "Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddDataGridToSheet(XLWorkbook wb, string name, DataGridView dgv)
        {
            var ws = wb.Worksheets.Add(name);
            var cols = dgv.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToList();

            for (int c = 0; c < cols.Count; c++)
                ws.Cell(1, c + 1).Value = cols[c].HeaderText;

            int row = 2;
            foreach (DataGridViewRow r in dgv.Rows)
            {
                if (r.IsNewRow) continue;
                for (int c = 0; c < cols.Count; c++)
                    ws.Cell(row, c + 1).Value = Convert.ToString(r.Cells[cols[c].Index].Value) ?? "";
                row++;
            }

            ws.RangeUsed().CreateTable();
            ws.Columns().AdjustToContents();
        }
        private void LancerSimulationFinanciere()
        {
            DataTable sim = new DataTable();
            sim.Columns.Add("Mois");
            sim.Columns.Add("Revenus", typeof(decimal));
            sim.Columns.Add("Dépenses", typeof(decimal));
            sim.Columns.Add("Résultat", typeof(decimal));

            int annee = DateTime.Today.Year;
            int moisActuel = DateTime.Today.Month;

            for (int m = 1; m <= moisActuel; m++)
            {
                DateTime d1 = new DateTime(annee, m, 1);
                DateTime d2 = d1.AddMonths(1).AddSeconds(-1);

                // Revenus = entrées économiques (si tu veux exclure certains motifs, mets ton NOT IN ici)
                decimal rev = ExecDecimal(null, @"
SELECT ISNULL(SUM(Montant),0)
FROM MouvementsCaisse
WHERE TypeMouvement = 'Entrée'
  AND DateHeure BETWEEN @d1 AND @d2", d1, d2);

                // Dépenses (dates corrigées partout)
                decimal dep = ExecDecimal(null, @"
SELECT ISNULL(SUM(Montant),0)
FROM (
    SELECT Montant AS Montant
    FROM MouvementsCaisse
    WHERE TypeMouvement='Sortie'
      AND DateHeure BETWEEN @d1 AND @d2

    UNION ALL

    SELECT Montant
    FROM SalairesPaiements
    WHERE DatePaiement BETWEEN @d1 AND @d2

    UNION ALL

    SELECT MontantTotal
    FROM HistoriquesAchat
    WHERE DateAchat BETWEEN @d1 AND @d2
) X", d1, d2);

                sim.Rows.Add(d1.ToString("MMMM yyyy"), rev, dep, rev - dep);
            }

            ShowTable("Simulation Financière", sim);
        }

        private void BuildHeader()
        {
            header = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 8, 12, 8),
                BackColor = Color.White
            };

            // Bord discret
            header.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(214, 224, 240), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, header.Width - 1, header.Height - 1);
            };

            var btnMenu = new Button
            {
                Text = "☰",
                FlatStyle = FlatStyle.Flat,
                ForeColor = C_Title,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                Width = 46,
                Height = 36,
                Location = new Point(10, 12),
                Cursor = Cursors.Hand
            };
            btnMenu.FlatAppearance.BorderSize = 0;

            lblTitle = new Label
            {
                Text = "Tableau de Bord Comptable",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = C_Title,                         // ✅ noir
                Font = new Font("Segoe UI", 14.5f, FontStyle.Bold)
            };

            lblPeriode = new Label
            {
                AutoSize = false,
                Width = 340,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = C_SubTitle,                      // ✅ gris lisible
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Padding = new Padding(0, 0, 8, 0)
            };

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblPeriode);
            header.Controls.Add(btnMenu);

            root.Controls.Add(header, 0, 0);
        }


        private void BuildTabs()
        {
            // --- conteneur principal
            var tabsHost = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(230, 235, 243),
                Padding = new Padding(8, 8, 8, 8),
                Margin = new Padding(0)
            };

            tabsHost.ColumnStyles.Clear();
            tabsHost.RowStyles.Clear();

            // gauche = boutons (prend le reste)
            tabsHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // droite = bloc période (largeur fixe)
            tabsHost.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420));
            tabsHost.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // --- gauche : boutons
            var tabsLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 4, 0, 4),
                Margin = new Padding(0)
            };

            btnJour = CreateTabButton("Jour");
            btnSemaine = CreateTabButton("Semaine");
            btnMois = CreateTabButton("Mois");
            btnAnnee = CreateTabButton("Année");

            tabsLeft.Controls.AddRange(new Control[] { btnJour, btnSemaine, btnMois, btnAnnee });

            // --- droite : période (Label + Combo collés)
            var right = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 6, 0, 0),
                Margin = new Padding(0)
            };

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            rightLayout.ColumnStyles.Clear();
            rightLayout.RowStyles.Clear();

            // Col 0 = label (auto)
            rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            // Col 1 = espace qui pousse tout à droite
            rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // Col 2 = combo (fixe)
            rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lblSelect = new Label
            {
                Text = "Période :",
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = C_Title,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 10, 0)
            };

            cmbPeriode = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                IntegralHeight = false,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 0, 0)
            };

            // Un spacer (vide) au milieu
            var spacer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            rightLayout.Controls.Add(lblSelect, 0, 0);
            rightLayout.Controls.Add(spacer, 1, 0);
            rightLayout.Controls.Add(cmbPeriode, 2, 0);

            right.Controls.Add(rightLayout);

            // --- assembler
            tabsHost.Controls.Add(tabsLeft, 0, 0);
            tabsHost.Controls.Add(right, 1, 0);

            root.Controls.Add(tabsHost, 0, 1);
        }

        private Button CreateTabButton(string text)
        {
            var b = new Button
            {
                Text = text,
                Width = 130,
                Height = 38,
                Margin = new Padding(6, 6, 6, 6),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(31, 79, 142),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;

            ApplyRounded(b, 8);
            b.Resize += (s, e) => ApplyRounded(b, 8);

            return b;
        }

        private void SetActiveTab(Button active)
        {
            foreach (var b in new[] { btnJour, btnSemaine, btnMois, btnAnnee })
                b.BackColor = (b == active) ? Color.FromArgb(25, 99, 180) : Color.FromArgb(31, 79, 142);
        }

        private void BuildKpis()
        {
            kpiRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpiRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            kpiRevenus = CreateKpiCard("Revenus", "0", "+0%", C_AccentBlue);
            kpiDepenses = CreateKpiCard("Dépenses", "0", "+0%", C_AccentRed);
            kpiFlux = CreateKpiCard("Flux de Trésorerie", "0", "+0%", C_AccentPurple);
            kpiBenefice = CreateKpiCard("Bénéfice Net", "0", "+0%", C_AccentGreen);

            kpiRow.Controls.Add(kpiRevenus, 0, 0);
            kpiRow.Controls.Add(kpiDepenses, 1, 0);
            kpiRow.Controls.Add(kpiFlux, 2, 0);
            kpiRow.Controls.Add(kpiBenefice, 3, 0);

            root.Controls.Add(kpiRow, 0, 2);
        }

        private Panel CreateKpiCard(string title, string value, string delta, Color accent)
        {
            Panel p = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                Padding = new Padding(10, 8, 10, 8),
                BackColor = C_Card
            };

            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(C_CardBorder, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };

            var accentBar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 6,
                BackColor = accent
            };

            var lblT = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                ForeColor = C_SubTitle,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Height = 18
            };

            var lblDelta = new Label
            {
                Name = "lblDelta",
                Text = delta,
                Dock = DockStyle.Bottom,
                Height = 18,
                ForeColor = Color.FromArgb(35, 160, 90),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // ✅ valeur en haut (pas en Fill) => elle ne se fait plus cacher par le %
            var lblValue = new Label
            {
                Name = "lblValue",
                Text = value,
                Dock = DockStyle.Top,
                ForeColor = C_Title,
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // petit texte aide (norme pro)
            var lblHint = new Label
            {
                Text = "Montant sur la période",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(120, 130, 145),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft
            };

            p.Controls.Add(lblHint);
            p.Controls.Add(lblDelta);
            p.Controls.Add(lblValue);
            p.Controls.Add(lblT);
            p.Controls.Add(accentBar);

            ApplyRounded(p, 12);
            p.Resize += (s, e) => ApplyRounded(p, 12);

            return p;
        }

        private void BuildCharts()
        {
            var chartsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            chartsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            chartsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

            // ===== LEFT: Cashflow =====
            var leftCard = CreateSectionCard("Flux de Trésorerie (Encaissements / Décaissements)");
            chartCashflow = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };
            chartCashflow.BorderSkin.SkinStyle = BorderSkinStyle.None;

            var area = new ChartArea("cash")
            {
                BackColor = Color.White
            };
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(235, 240, 248);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(235, 240, 248);
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 9.5f);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 9.5f);
            area.AxisX.LineColor = Color.FromArgb(200, 210, 225);
            area.AxisY.LineColor = Color.FromArgb(200, 210, 225);

            // ✅ plus de place réelle pour tracer
            area.Position = new ElementPosition(3, 8, 95, 86);
            area.InnerPlotPosition = new ElementPosition(7, 14, 88, 76);

            chartCashflow.ChartAreas.Clear();
            chartCashflow.ChartAreas.Add(area);

            chartCashflow.Legends.Clear();
            chartCashflow.Legends.Add(new Legend("L1")
            {
                Docking = Docking.Top,                 // ✅ plus en bas
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
                BackColor = Color.Transparent,
                IsDockedInsideChartArea = false         // ✅ n’écrase pas les dates
            });

            // Optionnel : une légende en ligne
            chartCashflow.Legends[0].LegendStyle = LegendStyle.Row;

            leftCard.Body.Controls.Add(chartCashflow);

            // ===== RIGHT: Expenses Pie =====
            var rightCard = CreateSectionCard("Structure des Charges (par Catégorie)");
            chartExpensesPie = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };
            chartExpensesPie.BorderSkin.SkinStyle = BorderSkinStyle.None;

            var area2 = new ChartArea("pie")
            {
                BackColor = Color.White
            };
            area2.AxisX.Enabled = AxisEnabled.False;
            area2.AxisY.Enabled = AxisEnabled.False;

            // ✅ énorme place pour labels dehors
            area2.Position = new ElementPosition(6, 5, 92, 90);
            area2.InnerPlotPosition = new ElementPosition(20, 10, 60, 78);

            chartExpensesPie.ChartAreas.Clear();
            chartExpensesPie.ChartAreas.Add(area2);

            chartExpensesPie.Legends.Clear();
            chartExpensesPie.Legends.Add(new Legend("L2")
            {
                Docking = Docking.Right,
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
                BackColor = Color.Transparent
            });

            rightCard.Body.Controls.Add(chartExpensesPie);

            chartsGrid.Controls.Add(leftCard.Container, 0, 0);
            chartsGrid.Controls.Add(rightCard.Container, 1, 0);

            root.Controls.Add(chartsGrid, 0, 3);
        }

        private void BuildBottomSection()
        {
            var bottom = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0),   // ✅ évite un petit espace par défaut
                Padding = new Padding(0)
            };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19));

            bottom.Margin = new Padding(0);
            bottom.Padding = new Padding(0);

            // Transactions
            var cardTx = CreateSectionCard("Transactions Récentes");
            dgvTransactions = CreateGrid();
            cardTx.Body.Controls.Add(dgvTransactions);

            // Factures
            var cardBills = CreateSectionCard("Factures à Payer");
            dgvFactures = CreateGrid();
            cardBills.Body.Controls.Add(dgvFactures);

            // Salaires
            var cardSal = CreateSectionCard("Suivi des Salaires");
            dgvSalaires = CreateGrid();
            cardSal.Body.Controls.Add(dgvSalaires);

            // Reports
            var cardReports = CreateSectionCard("Rapports Automatisés");

            // ✅ Layout interne : (1) liste des rapports (2) bande des 4 actions avec scroll
            var reportsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            reportsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Rapports (remplit)
            reportsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F)); // Bande boutons (fixe)

            // ========== (1) Liste des rapports ==========
            reportsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10),
                AutoScroll = true
            };

            btnBilan = CreateReportButton("Bilan Comptable", "Assets/report.png");
            btnResultat = CreateReportButton("Compte de Résultat", "Assets/result.png");
            btnGrandLivre = CreateReportButton("Grand Livre", "Assets/book.png");
            btnReleve = CreateReportButton("Relevé Bancaire", "Assets/bank.png");

            reportsPanel.Controls.AddRange(new Control[] { btnBilan, btnResultat, btnGrandLivre, btnReleve });

            reportsPanel.SizeChanged += (s, e) =>
            {
                int w = Math.Max(160, reportsPanel.ClientSize.Width - 25);
                foreach (Control c in reportsPanel.Controls) c.Width = w;

                int totalH = reportsPanel.Padding.Vertical + 20;
                foreach (Control c in reportsPanel.Controls)
                    totalH += c.Height + c.Margin.Top + c.Margin.Bottom;

                reportsPanel.AutoScrollMinSize = new Size(0, totalH);
            };

            // ========== (2) Bande des 4 actions + scroll ==========
            var actionsScroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(6, 6, 6, 6),
                Margin = new Padding(0)
            };

            bottomActions = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,

                // ✅ le scroll est sur le Panel parent
                AutoScroll = false,

                BackColor = Color.Transparent,
                Padding = new Padding(6, 4, 6, 4),
                Margin = new Padding(0),

                Location = new Point(0, 0),
                Height = 64
            };

            // ✅ Tes 4 boutons (mêmes objets que WireEvents utilise)
            btnExportPdf = CreateActionButton("Exporter PDF", "Assets/pdf.png", Color.FromArgb(192, 57, 43));
            btnGenExcel = CreateActionButton("Générer Excel", "Assets/excel.png", Color.FromArgb(39, 174, 96));
            btnSimu = CreateActionButton("Simulations Financières", "Assets/simu.png", Color.FromArgb(44, 62, 80));
            btnPrevision = CreateActionButton("Prévision de Trésorerie", "Assets/forecast.png", Color.FromArgb(127, 140, 141));

            btnExportPdf.Width = 240; btnExportPdf.Height = 56;
            btnGenExcel.Width = 240; btnGenExcel.Height = 56;
            btnSimu.Width = 280; btnSimu.Height = 56;
            btnPrevision.Width = 280; btnPrevision.Height = 56;

            bottomActions.Controls.AddRange(new Control[] { btnExportPdf, btnGenExcel, btnSimu, btnPrevision });

            void UpdateActionsScroll()
            {
                int totalW = bottomActions.Padding.Horizontal + 20;
                foreach (Control c in bottomActions.Controls)
                    totalW += c.Width + c.Margin.Left + c.Margin.Right;

                bottomActions.Width = totalW;
                actionsScroll.AutoScrollMinSize = new Size(totalW, bottomActions.Height);
            }

            actionsScroll.SizeChanged += (s, e) => UpdateActionsScroll();
            bottomActions.ControlAdded += (s, e) => UpdateActionsScroll();
            bottomActions.ControlRemoved += (s, e) => UpdateActionsScroll();

            actionsScroll.Controls.Add(bottomActions);
            UpdateActionsScroll();

            // assembler dans la carte
            reportsLayout.Controls.Add(reportsPanel, 0, 0);
            reportsLayout.Controls.Add(actionsScroll, 0, 1);

            cardReports.Body.Controls.Add(reportsLayout);

            bottom.Controls.Add(cardTx.Container, 0, 0);
            bottom.Controls.Add(cardBills.Container, 1, 0);
            bottom.Controls.Add(cardSal.Container, 2, 0);
            bottom.Controls.Add(cardReports.Container, 3, 0);

            root.Controls.Add(bottom, 0, 4);
        }

        private void BuildBottomActions()
        {
            Panel bottomContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(6, 2, 6, 6),
                Margin = new Padding(0)
            };

            // ✅ Panel qui porte la barre de défilement (UNIQUEMENT ici)
            var scrollButtons = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            bottomActions = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,

                // ✅ IMPORTANT : le scroll est sur le Panel parent
                AutoScroll = false,

                BackColor = Color.Transparent,
                Padding = new Padding(6, 4, 6, 4),
                Margin = new Padding(0),

                Location = new Point(0, 0),
                Height = 64 // hauteur de la bande des boutons
            };

            btnExportPdf = CreateActionButton("Exporter PDF", "Assets/pdf.png", Color.FromArgb(192, 57, 43));
            btnGenExcel = CreateActionButton("Générer Excel", "Assets/excel.png", Color.FromArgb(39, 174, 96));
            btnSimu = CreateActionButton("Simulations Financières", "Assets/simu.png", Color.FromArgb(44, 62, 80));
            btnPrevision = CreateActionButton("Prévision de Trésorerie", "Assets/forecast.png", Color.FromArgb(127, 140, 141));

            btnExportPdf.Width = 240; btnExportPdf.Height = 56;
            btnGenExcel.Width = 240; btnGenExcel.Height = 56;
            btnSimu.Width = 280; btnSimu.Height = 56;
            btnPrevision.Width = 280; btnPrevision.Height = 56;

            bottomActions.Controls.AddRange(new Control[] { btnExportPdf, btnGenExcel, btnSimu, btnPrevision });

            void UpdateButtonsScroll()
            {
                int totalW = bottomActions.Padding.Horizontal + 20;
                foreach (Control c in bottomActions.Controls)
                    totalW += c.Width + c.Margin.Left + c.Margin.Right;

                // ✅ force une largeur réelle > panel => scrollbar horizontale
                bottomActions.Width = totalW;

                // ✅ dit au panel scrollable quelle surface scroller
                scrollButtons.AutoScrollMinSize = new Size(totalW, bottomActions.Height);
            }

            // recalcul au resize (fenêtre) ou si tu changes les boutons
            scrollButtons.SizeChanged += (s, e) => UpdateButtonsScroll();
            bottomActions.ControlAdded += (s, e) => UpdateButtonsScroll();
            bottomActions.ControlRemoved += (s, e) => UpdateButtonsScroll();

            scrollButtons.Controls.Add(bottomActions);
            bottomContainer.Controls.Add(scrollButtons);
            if (root.RowCount < 6)
            {
                root.RowCount = 6;
                root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96)); // hauteur boutons
            }
            root.Controls.Add(bottomContainer); // ✅ auto sur la prochaine ligne libre
            UpdateButtonsScroll();
        }

        private (Panel Container, Panel Body) CreateSectionCard(string title)
        {
            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 2, 6, 6), // ✅ top plus petit => remonte
                BackColor = Color.White
            };

            container.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(214, 224, 240), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, container.Width - 1, container.Height - 1);
            };

            Panel hdr = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,                       // ✅ avant 36
                BackColor = Color.FromArgb(244, 247, 252),
                Padding = new Padding(12, 6, 12, 4) // ✅ avant (12,8,12,6)
            };

            Label lbl = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                ForeColor = C_Title, // ✅ noir
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            hdr.Controls.Add(lbl);

            Panel body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8), // ✅ avant 10
                BackColor = Color.White
            };

            container.Controls.Add(body);
            container.Controls.Add(hdr);

            ApplyRounded(container, 12);
            container.Resize += (s, e) => ApplyRounded(container, 12);

            return (container, body);
        }

        private DataGridView CreateGrid()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            dgv.RowTemplate.Height = 28;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 244, 250);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 40, 60);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Regular);

            return dgv;
        }

        private Button CreateReportButton(string text, string assetPath)
        {
            var btn = new Button
            {
                Text = "  " + text,
                TextAlign = ContentAlignment.MiddleLeft,
                ImageAlign = ContentAlignment.MiddleLeft,
                Width = 240,
                Height = 50,
                Margin = new Padding(0, 0, 0, 10),
                BackColor = Color.FromArgb(31, 79, 142),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Image = LoadAssetImage(assetPath, 22, 22)
            };
            btn.FlatAppearance.BorderSize = 0;

            ApplyRounded(btn, 10);
            btn.Resize += (s, e) => ApplyRounded(btn, 10);

            return btn;
        }

        private Button CreateActionButton(string text, string assetPath, Color color)
        {
            var btn = new Button
            {
                Text = "  " + text,
                TextAlign = ContentAlignment.MiddleLeft,
                ImageAlign = ContentAlignment.MiddleLeft,
                Width = 220,
                Height = 56,
                Margin = new Padding(10, 6, 10, 6),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Image = LoadAssetImage(assetPath, 22, 22)
            };
            btn.FlatAppearance.BorderSize = 0;

            ApplyRounded(btn, 10);
            btn.Resize += (s, e) => ApplyRounded(btn, 10);

            return btn;
        }



        // ========================
        // EVENTS
        // ========================
        private void WireEvents()
        {
            // ========================
            // CHANGEMENT DE PÉRIODE
            // ========================
            btnJour.Click += (s, e) =>
            {
                _mode = PeriodMode.Jour;
                SetActiveTab(btnJour);
                ChargerListePeriodes();

                // ✅ force l’appli du choix actuel
                if (cmbPeriode.SelectedItem is PeriodItem it)
                    ApplyPeriod(it.Debut, it.Fin, forceRefresh: true);
            };

            btnSemaine.Click += (s, e) =>
            {
                _mode = PeriodMode.Semaine;
                SetActiveTab(btnSemaine);
                ChargerListePeriodes();

                // ✅ force l’appli du choix actuel
                if (cmbPeriode.SelectedItem is PeriodItem it)
                    ApplyPeriod(it.Debut, it.Fin, forceRefresh: true);
            };

            btnMois.Click += (s, e) =>
            {
                _mode = PeriodMode.Mois;
                SetActiveTab(btnMois);
                ChargerListePeriodes();

                // ✅ force l’appli du choix actuel
                if (cmbPeriode.SelectedItem is PeriodItem it)
                    ApplyPeriod(it.Debut, it.Fin, forceRefresh: true);
            };

            btnAnnee.Click += (s, e) =>
            {
                _mode = PeriodMode.Annee;
                SetActiveTab(btnAnnee);
                ChargerListePeriodes();

                // ✅ force l’appli du choix actuel
                if (cmbPeriode.SelectedItem is PeriodItem it)
                    ApplyPeriod(it.Debut, it.Fin, forceRefresh: true);
            };

            // ========================
            // EXPORTS & ACTIONS GÉNÉRALES
            // ========================
            btnExportPdf.Click += (s, e) => ExporterPdfDashboard();
            btnGenExcel.Click += (s, e) => ExporterExcelDashboard();
            btnSimu.Click += (s, e) => LancerSimulationFinanciere();
            btnPrevision.Click += (s, e) => LancerPrevisionTresorerie();

            // ========================
            // RAPPORTS COMPTABLES (OBLIGATOIRE)
            // ========================
            btnBilan.Click += (s, e) =>
            {
                // 🔥 Générer les écritures AVANT l’export
                CloturerPeriode(dateDebut, dateFin);
                ExporterPdfBilan();
            };

            btnResultat.Click += (s, e) =>
            {
                // 🔥 Générer les écritures AVANT l’export
                CloturerPeriode(dateDebut, dateFin);
                ExporterPdfCompteResultat();
            };

            btnGrandLivre.Click += (s, e) =>
            {
                // (Optionnel mais cohérent)
                CloturerPeriode(dateDebut, dateFin);
                ExporterPdfGrandLivre();
            };

            btnReleve.Click += (s, e) =>
            {
                // (Optionnel mais cohérent)
                CloturerPeriode(dateDebut, dateFin);
                ExporterPdfReleveBancaire();
            };

            cmbPeriode.SelectedIndexChanged += CmbPeriodeSafeHandler;
        }

        private (DateTime min, DateTime max) GetBornesDonnees()
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
SELECT 
    MIN(Dt) AS MinDt,
    MAX(Dt) AS MaxDt
FROM (
    SELECT DateHeure   AS Dt FROM MouvementsCaisse
    UNION ALL
    SELECT DateAchat   AS Dt FROM HistoriquesAchat
    UNION ALL
    SELECT DatePaiement AS Dt FROM SalairesPaiements
) X;";

                using (var cmd = new SqlCommand(sql, con))
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                        return (DateTime.Today, DateTime.Today);

                    DateTime min = rd["MinDt"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(rd["MinDt"]);
                    DateTime max = rd["MaxDt"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(rd["MaxDt"]);
                    return (min, max);
                }
            }
        }

        private bool HasDataOnPeriod(DateTime d1, DateTime d2)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                string sql = @"
SELECT CASE WHEN EXISTS(
    SELECT 1 FROM MouvementsCaisse WHERE DateHeure BETWEEN @d1 AND @d2
    UNION ALL
    SELECT 1 FROM HistoriquesAchat WHERE DateAchat BETWEEN @d1 AND @d2
    UNION ALL
    SELECT 1 FROM SalairesPaiements WHERE DatePaiement BETWEEN @d1 AND @d2
) THEN 1 ELSE 0 END";

                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = d1;
                    cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = d2;
                    return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                }
            }
        }

        private static DateTime StartOfWeekMonday(DateTime d)
        {
            int diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
            return d.Date.AddDays(-diff);
        }

        private void ChargerListePeriodes()
        {
            // bornes des données existantes
            var (minData, maxData) = GetBornesDonnees();

            // fenêtre affichée : on inclut futur "vide"
            int futurSemaines = 8;
            int futurMois = 6;
            int futurAnnees = 2;

            var items = new List<PeriodItem>();
            var culture = CultureInfo.GetCultureInfo("fr-FR");

            if (_mode == PeriodMode.Jour)
            {
                // Jours : last 30 + next 7
                DateTime start = DateTime.Today.AddDays(-30);
                DateTime end = DateTime.Today.AddDays(7);

                for (DateTime d = start; d <= end; d = d.AddDays(1))
                {
                    DateTime d1 = d.Date;
                    DateTime d2 = d1.AddDays(1).AddSeconds(-1);

                    bool has = HasDataOnPeriod(d1, d2);
                    items.Add(new PeriodItem
                    {
                        Debut = d1,
                        Fin = d2,
                        Label = d1.ToString("dddd dd MMMM yyyy", culture),
                        HasData = has
                    });
                }

                // sélection automatique : aujourd'hui
                SetCombo(items, it => it.Debut.Date == DateTime.Today);
                return;
            }

            if (_mode == PeriodMode.Semaine)
            {
                // de la première semaine des données jusqu'à quelques semaines futures (mais on limite si trop énorme)
                DateTime start = StartOfWeekMonday(minData);
                DateTime end = StartOfWeekMonday(maxData).AddDays(7 * futurSemaines);

                int nbWeeks = (int)((end - start).TotalDays / 7) + 1;

                // garde-fou si ton historique fait 10 ans (520 semaines) : on montre les 156 dernières (≈3 ans) + futur
                if (nbWeeks > 200)
                    start = StartOfWeekMonday(DateTime.Today.AddDays(-7 * 156));

                for (DateTime d = start; d <= end; d = d.AddDays(7))
                {
                    DateTime d1 = d.Date;
                    DateTime d2 = d1.AddDays(7).AddSeconds(-1);

                    bool has = HasDataOnPeriod(d1, d2);
                    string label = $"Semaine du {d1:dd/MM/yyyy} au {d2:dd/MM/yyyy}";
                    items.Add(new PeriodItem { Debut = d1, Fin = d2, Label = label, HasData = has });
                }

                // sélection auto : semaine courante
                DateTime cur = StartOfWeekMonday(DateTime.Today);
                SetCombo(items, it => it.Debut.Date == cur);
                return;
            }

            if (_mode == PeriodMode.Mois)
            {
                DateTime start = new DateTime(minData.Year, minData.Month, 1);
                DateTime end = new DateTime(maxData.Year, maxData.Month, 1).AddMonths(futurMois);

                int nbMonths = ((end.Year - start.Year) * 12 + end.Month - start.Month) + 1;

                // garde-fou si trop grand : 36 derniers mois
                if (nbMonths > 48)
                    start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-36);

                for (DateTime d = start; d <= end; d = d.AddMonths(1))
                {
                    DateTime d1 = new DateTime(d.Year, d.Month, 1);
                    DateTime d2 = d1.AddMonths(1).AddSeconds(-1);

                    bool has = HasDataOnPeriod(d1, d2);
                    string label = d1.ToString("MMMM yyyy", culture);
                    items.Add(new PeriodItem { Debut = d1, Fin = d2, Label = label, HasData = has });
                }

                // sélection auto : mois courant
                var cur = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                SetCombo(items, it => it.Debut.Date == cur);
                return;
            }

            // ANNEE
            {
                int startYear = minData.Year;
                int endYear = maxData.Year + futurAnnees;

                // garde-fou si énorme : 8 dernières années
                if (endYear - startYear > 12)
                    startYear = DateTime.Today.Year - 8;

                for (int y = startYear; y <= endYear; y++)
                {
                    DateTime d1 = new DateTime(y, 1, 1);
                    DateTime d2 = new DateTime(y, 12, 31, 23, 59, 59);

                    bool has = HasDataOnPeriod(d1, d2);
                    items.Add(new PeriodItem { Debut = d1, Fin = d2, Label = $"Année {y}", HasData = has });
                }

                int curY = DateTime.Today.Year;
                SetCombo(items, it => it.Debut.Year == curY);
            }
        }

        private void SetCombo(List<PeriodItem> items, Func<PeriodItem, bool> predicate)
        {
            cmbPeriode.SelectedIndexChanged -= CmbPeriodeSafeHandler;

            cmbPeriode.DataSource = null;
            cmbPeriode.DisplayMember = null;
            cmbPeriode.ValueMember = null;

            cmbPeriode.DataSource = items;

            int idx = items.FindIndex(i => predicate(i));
            cmbPeriode.SelectedIndex = (idx >= 0) ? idx : Math.Max(0, items.Count - 1);

            cmbPeriode.SelectedIndexChanged += CmbPeriodeSafeHandler;

            // ✅ Appliquer la période sélectionnée (sans dépendre d’un event)
            if (cmbPeriode.SelectedItem is PeriodItem it)
                ApplyPeriod(it.Debut, it.Fin, forceRefresh: true);
        }

        private void CmbPeriodeSafeHandler(object sender, EventArgs e)
        {
            if (cmbPeriode.SelectedItem is PeriodItem it)
                ApplyPeriod(it.Debut, it.Fin, forceRefresh: true);
        }
        // ========================
        // PERIOD
        // ========================
        private void SetPeriodeJour()
        {
            dateDebut = DateTime.Today;
            dateFin = DateTime.Today.AddDays(1).AddSeconds(-1);
            UpdatePeriodeLabel();
        }

        private void SetPeriodeSemaine()
        {
            var today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            dateDebut = today.AddDays(-diff);
            dateFin = dateDebut.AddDays(7).AddSeconds(-1);
            UpdatePeriodeLabel();
        }

        private void SetPeriodeMoisCourant()
        {
            var today = DateTime.Today;

            dateDebut = new DateTime(today.Year, today.Month, 1);
            dateFin = dateDebut.AddMonths(1).AddSeconds(-1); // 🔥 TRÈS IMPORTANT

            SetActiveTab(btnMois);
            UpdatePeriodeLabel();
        }

        private void SetPeriodeAnneeCourante()
        {
            var today = DateTime.Today;
            dateDebut = new DateTime(today.Year, 1, 1);
            dateFin = new DateTime(today.Year, 12, 31, 23, 59, 59);
            UpdatePeriodeLabel();
        }

        private void UpdatePeriodeLabel()
        {
            lblPeriode.Text =
                $"Période : {dateDebut:dd/MM/yyyy} - {dateFin:dd/MM/yyyy}";
        }

        private void LancerPrevisionTresorerie()
        {
            DataTable prev = new DataTable();
            prev.Columns.Add("Mois");
            prev.Columns.Add("Encaissements", typeof(decimal));
            prev.Columns.Add("Décaissements", typeof(decimal));
            prev.Columns.Add("Solde Cumulé", typeof(decimal));

            int annee = DateTime.Today.Year;
            int moisActuel = DateTime.Today.Month;

            decimal soldeCumule = 0m;

            for (int m = 1; m <= moisActuel; m++)
            {
                DateTime d1 = new DateTime(annee, m, 1);
                DateTime d2 = d1.AddMonths(1).AddSeconds(-1);

                decimal encaissements = ExecDecimal(
                    null,
                    @"
SELECT ISNULL(SUM(Montant),0)
FROM MouvementsCaisse
WHERE TypeMouvement = 'Entrée'
  AND DateHeure BETWEEN @d1 AND @d2",
                    d1,
                    d2
                );

                decimal decaissements = ExecDecimal(
                    null,
                    @"
SELECT ISNULL(SUM(Montant),0)
FROM (
    SELECT Montant
    FROM MouvementsCaisse
    WHERE TypeMouvement = 'Sortie'
      AND DateHeure BETWEEN @d1 AND @d2

    UNION ALL

    SELECT Montant
    FROM SalairesPaiements
    WHERE DatePaiement BETWEEN @d1 AND @d2

    UNION ALL

    SELECT MontantTotal
    FROM HistoriquesAchat
    WHERE DateAchat BETWEEN @d1 AND @d2
) X",
                    d1,
                    d2
                );

                soldeCumule += encaissements - decaissements;

                prev.Rows.Add(d1.ToString("MMMM yyyy"), encaissements, decaissements, soldeCumule);
            }

            ShowTable("Prévision de Trésorerie (Données Réelles)", prev);
        }

        // ========================
        // REFRESH (SQL)
        // ========================
        private void RefreshAll()
        {
            UpdatePeriodeLabel();

            try
            {
                var model = ChargerDashboardDepuisSql(dateDebut, dateFin);

                SetKpi(kpiRevenus, model.RevenusParDevise, model.DeltaRevenus);
                SetKpi(kpiDepenses, model.DepensesParDevise, model.DeltaDepenses);

                var flux = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
{
    {
        "CDF",
        (model.RevenusParDevise.TryGetValue("CDF", out var rcdf) ? rcdf : 0m)
      - (model.DepensesParDevise.TryGetValue("CDF", out var dcdf) ? dcdf : 0m)
    },
    {
        "USD",
        (model.RevenusParDevise.TryGetValue("USD", out var rusd) ? rusd : 0m)
      - (model.DepensesParDevise.TryGetValue("USD", out var dusd) ? dusd : 0m)
    }
};

                SetKpi(kpiFlux, flux, model.DeltaFlux);
                SetKpi(kpiBenefice, flux, model.DeltaBenefice);

                RemplirCashflowChart(model.CashflowJour);
                RemplirExpensesPie(model.DepensesParCategorie);

                dgvTransactions.DataSource = model.Transactions;
                dgvFactures.DataSource = model.Factures;
                dgvSalaires.DataSource = model.Salaires;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erreur chargement Dashboard : " + ex.Message,
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private DashboardModel ChargerDashboardDepuisSql(DateTime debut, DateTime fin)
        {
            var m = new DashboardModel();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                // KPI PAR DEVISE (FC / USD)
                // =========================
                m.RevenusParDevise = ExecParDeviseKpi(con, "Entrée", debut, fin);
                m.DepensesParDevise = ExecParDeviseKpi(con, "Sortie", debut, fin);

                // Totaux globaux (toutes devises)
                m.Revenus = m.RevenusParDevise.Values.Sum();
                m.Depenses = m.DepensesParDevise.Values.Sum();

                m.FluxTresorerie = m.Revenus - m.Depenses;
                m.BeneficeNet = m.FluxTresorerie;

                // =========================
                // 1) KPI PRINCIPAUX (ÉCONOMIQUES UNIQUEMENT)
                // =========================

                // Revenus = Entrées économiques (TRANSFERTS EXCLUS)
                m.Revenus = ExecDecimal(con, @"
SELECT ISNULL(SUM(Montant),0)
FROM MouvementsCaisse
WHERE TypeMouvement = 'Entrée'
  AND DateHeure BETWEEN @d1 AND @d2
  AND UPPER(LTRIM(RTRIM(Motif))) NOT IN (
        'VERSEMENT SIM',
        'VERSEMENT BANK',
        'SOUSTRACTION SIM',
        'VERSEMENT SIM OU BANQUE',
        'ENVOI PATRONNE',
        'TOTAL GENERAL ESPECE'
  )", debut, fin);

                // Dépenses = Sorties économiques UNIQUEMENT
                m.Depenses = ExecDecimal(con, @"
SELECT ISNULL(SUM(Montant),0)
FROM MouvementsCaisse
WHERE TypeMouvement = 'Sortie'
  AND DateHeure BETWEEN @d1 AND @d2
  AND UPPER(LTRIM(RTRIM(Motif))) NOT IN (
        'VERSEMENT SIM',
        'VERSEMENT BANK',
        'SOUSTRACTION SIM',
        'VERSEMENT SIM OU BANQUE',
        'ENVOI PATRONNE',
        'TOTAL GENERAL ESPECE'
  )", debut, fin);

                // Flux & bénéfice (vision économique)
                m.FluxTresorerie = m.Revenus - m.Depenses;
                m.BeneficeNet = m.FluxTresorerie;

                // =========================
                // 2) DELTA VS PÉRIODE PRÉCÉDENTE (MÊME LOGIQUE)
                // =========================
                var spanDays = Math.Max(1, (fin.Date - debut.Date).TotalDays + 1);
                var prevFin = debut.AddSeconds(-1);
                var prevDeb = prevFin.AddDays(-spanDays).Date;

                decimal revPrev = ExecDecimal(con, @"
SELECT ISNULL(SUM(Montant),0)
FROM MouvementsCaisse
WHERE TypeMouvement = 'Entrée'
  AND DateHeure BETWEEN @d1 AND @d2
  AND UPPER(LTRIM(RTRIM(Motif))) NOT IN (
        'VERSEMENT SIM',
        'VERSEMENT BANK',
        'SOUSTRACTION SIM',
        'VERSEMENT SIM OU BANQUE',
        'ENVOI PATRONNE',
        'TOTAL GENERAL ESPECE'
  )", prevDeb, prevFin);

                decimal depPrev = ExecDecimal(con, @"
SELECT ISNULL(SUM(Montant),0)
FROM MouvementsCaisse
WHERE TypeMouvement = 'Sortie'
  AND DateHeure BETWEEN @d1 AND @d2
  AND UPPER(LTRIM(RTRIM(Motif))) NOT IN (
        'VERSEMENT SIM',
        'VERSEMENT BANK',
        'SOUSTRACTION SIM',
        'VERSEMENT SIM OU BANQUE',
        'ENVOI PATRONNE',
        'TOTAL GENERAL ESPECE'
  )", prevDeb, prevFin);

                m.DeltaRevenus = PctDelta(m.Revenus, revPrev);
                m.DeltaDepenses = PctDelta(m.Depenses, depPrev);
                m.DeltaFlux = PctDelta(m.FluxTresorerie, revPrev - depPrev);
                m.DeltaBenefice = m.DeltaFlux;

                // =========================
                // 3) CASHFLOW PAR JOUR (ÉCONOMIQUE)
                // =========================
                m.CashflowJour = FillTable(con, @"
SELECT 
    CAST(DateHeure AS date) AS [Date],
    SUM(CASE 
            WHEN TypeMouvement = 'Entrée'
             AND UPPER(LTRIM(RTRIM(Motif))) NOT IN (
                'VERSEMENT SIM','VERSEMENT BANK','SOUSTRACTION SIM',
                'VERSEMENT SIM OU BANQUE','ENVOI PATRONNE','TOTAL GENERAL ESPECE'
             )
            THEN Montant ELSE 0 END) AS Entrees,
    SUM(CASE 
            WHEN TypeMouvement = 'Sortie'
             AND UPPER(LTRIM(RTRIM(Motif))) NOT IN (
                'VERSEMENT SIM','VERSEMENT BANK','SOUSTRACTION SIM',
                'VERSEMENT SIM OU BANQUE','ENVOI PATRONNE','TOTAL GENERAL ESPECE'
             )
            THEN Montant ELSE 0 END) AS Sorties
FROM MouvementsCaisse
WHERE DateHeure BETWEEN @d1 AND @d2
GROUP BY CAST(DateHeure AS date)
ORDER BY CAST(DateHeure AS date)", debut, fin);

                if (!m.CashflowJour.Columns.Contains("Solde"))
                    m.CashflowJour.Columns.Add("Solde", typeof(decimal));

                foreach (DataRow r in m.CashflowJour.Rows)
                    r["Solde"] = ToDec(r["Entrees"]) - ToDec(r["Sorties"]);

                // =========================
                // 4) STRUCTURE DES CHARGES (PIE – ÉCONOMIQUE)
                // =========================
                m.DepensesParCategorie = FillTable(con, @"
SELECT 
    CASE 
        WHEN UPPER(LTRIM(RTRIM(Motif))) = 'SOUSTRACTION SIM'
            THEN 'SALAIRES AGENTS'
        ELSE LTRIM(RTRIM(Motif))
    END AS Categorie,
    SUM(Montant) AS Montant
FROM MouvementsCaisse
WHERE TypeMouvement = 'Sortie'
  AND DateHeure BETWEEN @d1 AND @d2
  AND UPPER(LTRIM(RTRIM(Motif))) NOT IN (
        'VERSEMENT SIM',
        'VERSEMENT BANK',
        'VERSEMENT SIM OU BANQUE',
        'ENVOI PATRONNE',
        'TOTAL GENERAL ESPECE'
  )
GROUP BY 
    CASE 
        WHEN UPPER(LTRIM(RTRIM(Motif))) = 'SOUSTRACTION SIM'
            THEN 'SALAIRES AGENTS'
        ELSE LTRIM(RTRIM(Motif))
    END
ORDER BY SUM(Montant) DESC
", debut, fin);

                // =========================
                // 5) TRANSACTIONS (LISTE COMPLÈTE – NON FILTRÉE)
                // =========================
                m.Transactions = FillTable(con, @"
SELECT TOP 50
    DateHeure AS [Date],
    Motif AS [Description],
    CASE WHEN TypeMouvement='Entrée' THEN Montant ELSE -Montant END AS Montant,
    TypeMouvement AS [Type]
FROM MouvementsCaisse
WHERE DateHeure BETWEEN @d1 AND @d2
ORDER BY DateHeure DESC", debut, fin);

                // =========================
                // 6) FACTURES FOURNISSEURS
                // =========================
                m.Factures = FillTable(con, @"
SELECT TOP 50
    NomFournisseur AS Fournisseur,
    CONVERT(varchar(10), DateAchat, 103) AS DateAchat,
    MontantTotal AS Montant,
    Statut
FROM HistoriquesAchat
WHERE DateAchat BETWEEN @d1 AND @d2
  AND ISNULL(Statut,'') NOT IN ('Payé','Paye','Paid')
ORDER BY DateAchat DESC", debut, fin);

                // =========================
                // 7) SALAIRES (SUIVI SEULEMENT)
                // =========================
                m.Salaires = FillTable(con, @"
SELECT TOP 50
    CONVERT(varchar(10), DatePaiement, 103) AS DatePaiement,
    NomEmploye,
    Montant,
    Devise,
    Statut
FROM SalairesPaiements
WHERE DatePaiement BETWEEN @d1 AND @d2
ORDER BY DatePaiement DESC", debut, fin);
            }

            return m;
        }

        private void SetKpi(Panel card, Dictionary<string, decimal> values, decimal deltaPct)
        {
            // 🔒 Stockage des valeurs réelles (SOURCE UNIQUE)
            card.Tag = new Dictionary<string, decimal>(values, StringComparer.OrdinalIgnoreCase);

            var lblValue = card.Controls.Find("lblValue", true).FirstOrDefault() as Label;
            var lblDelta = card.Controls.Find("lblDelta", true).FirstOrDefault() as Label;

            if (lblValue != null)
            {
                decimal cdf = values.TryGetValue("CDF", out var vcdf) ? vcdf : 0m;
                decimal usd = values.TryGetValue("USD", out var vusd) ? vusd : 0m;

                lblValue.Text = $"{FmtMoney(cdf, "CDF")} / {FmtMoney(usd, "USD")}";
            }

            if (lblDelta != null)
            {
                string sign = deltaPct >= 0 ? "+" : "";
                lblDelta.Text = $"{sign}{deltaPct:N1}%";
                lblDelta.ForeColor = deltaPct >= 0
                    ? Color.FromArgb(35, 160, 90)
                    : Color.FromArgb(220, 60, 60);
            }
        }
        private Dictionary<string, decimal> GetKpiRawValues(Panel card)
        {
            return card.Tag as Dictionary<string, decimal>
                ?? new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        private void RemplirCashflowChart(DataTable dt)
        {
            chartCashflow.Series.Clear();

            var sEntrees = new Series("Entrées") { ChartType = SeriesChartType.Column, XValueType = ChartValueType.DateTime, BorderWidth = 1 };
            var sSorties = new Series("Sorties") { ChartType = SeriesChartType.Column, XValueType = ChartValueType.DateTime, BorderWidth = 1 };
            var sSolde = new Series("Solde Net") { ChartType = SeriesChartType.Line, BorderWidth = 3, XValueType = ChartValueType.DateTime };

            foreach (DataRow r in dt.Rows)
            {
                DateTime d = Convert.ToDateTime(r["Date"]);
                decimal entrees = ToDec(r["Entrees"]);
                decimal sorties = ToDec(r["Sorties"]);
                decimal solde = ToDec(r["Solde"]);

                sEntrees.Points.AddXY(d, (double)entrees);
                sSorties.Points.AddXY(d, (double)sorties);
                sSolde.Points.AddXY(d, (double)solde);
            }

            chartCashflow.Series.Add(sEntrees);
            chartCashflow.Series.Add(sSorties);
            chartCashflow.Series.Add(sSolde);

            var area = chartCashflow.ChartAreas[0];
            area.AxisX.LabelStyle.Format = "dd MMM";
            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area.RecalculateAxesScale();


            // ✅ dates plus lisibles
            area.AxisX.LabelStyle.Format = "dd MMM";
            area.AxisX.IsLabelAutoFit = false;
            area.AxisX.LabelStyle.Angle = -45;       // ✅ tourne les dates
            area.AxisX.LabelStyle.IsStaggered = true; // ✅ 2 lignes si besoin

            // ✅ limite le nombre de dates affichées (max ~10-12 labels)
            int n = dt?.Rows.Count ?? 0;
            int interval = (n <= 12) ? 1 : (int)Math.Ceiling(n / 12.0);

            area.AxisX.Interval = interval;
            area.AxisX.IntervalType = DateTimeIntervalType.Days;

            // un peu plus de marge en bas pour les labels inclinés
            area.Position = new ElementPosition(3, 10, 95, 82);
            area.InnerPlotPosition = new ElementPosition(7, 12, 88, 72);

            area.RecalculateAxesScale();
        }

        private void RemplirExpensesPie(DataTable dt)
        {
            chartExpensesPie.Series.Clear();
            chartExpensesPie.Palette = ChartColorPalette.BrightPastel;

            if (dt == null || dt.Rows.Count == 0)
            {
                Series s0 = new Series("Aucune donnée")
                {
                    ChartType = SeriesChartType.Pie
                };
                s0.Points.AddXY("Aucune donnée", 1);
                chartExpensesPie.Series.Add(s0);
                return;
            }

            Series s = new Series("Structure des Charges")
            {
                ChartType = SeriesChartType.Doughnut,
                BorderWidth = 1,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                IsValueShownAsLabel = true
            };

            s["DoughnutRadius"] = "65";
            s["PieLabelStyle"] = "Outside";
            s["PieStartAngle"] = "270";

            decimal total = dt.AsEnumerable().Sum(r => ToDec(r["Montant"]));
            if (total <= 0) total = 1;

            foreach (DataRow r in dt.Rows)
            {
                string cat = Convert.ToString(r["Categorie"]);
                decimal m = ToDec(r["Montant"]);
                if (m <= 0) continue;

                int idx = s.Points.AddXY(cat, (double)m);
                s.Points[idx].LegendText = cat;
                s.Points[idx].Label = $"{(m / total * 100m):N0}%";
                s.Points[idx].ToolTip = $"{cat} : {m:N0}";
            }

            chartExpensesPie.Series.Add(s);
            chartExpensesPie.Legends[0].Docking = Docking.Right;
            chartExpensesPie.Legends[0].Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        }

        // ========================
        // REPORTS (PDF)
        // ========================
        private void ExporterPdfDashboard()
        {
            using (var sfd = new SaveFileDialog { Filter = "Fichier PDF|*.pdf", FileName = "TableauDeBordComptable.pdf" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        using (PdfDoc pdfDoc = new PdfDoc(PdfPageSize.A4, 18f, 18f, 20f, 20f))
                        {
                            PdfWriter.GetInstance(pdfDoc, stream);
                            pdfDoc.Open();

                            PdfBaseFont baseFont = GetUnicodeBaseFont(); PdfFont fontTitle = new PdfFont(baseFont, 16, PdfFont.BOLD);
                            PdfFont fontHeader = new PdfFont(baseFont, 11, PdfFont.BOLD);
                            PdfFont fontRow = new PdfFont(baseFont, 10, PdfFont.NORMAL);

                            pdfDoc.Add(new PdfParagraph("TABLEAU DE BORD COMPTABLE", fontTitle)
                            {
                                Alignment = PdfElement.ALIGN_CENTER,
                                SpacingAfter = 10f
                            });

                            pdfDoc.Add(new PdfParagraph(lblPeriode.Text ?? "", fontRow)
                            {
                                Alignment = PdfElement.ALIGN_CENTER,
                                SpacingAfter = 16f
                            });

                            // KPI
                            PdfPTable kpi = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 14f };
                            kpi.AddCell(KpiCell("Revenus", GetKpiValue(kpiRevenus), fontHeader));
                            kpi.AddCell(KpiCell("Dépenses", GetKpiValue(kpiDepenses), fontHeader));
                            kpi.AddCell(KpiCell("Flux", GetKpiValue(kpiFlux), fontHeader));
                            kpi.AddCell(KpiCell("Bénéfice", GetKpiValue(kpiBenefice), fontHeader));
                            pdfDoc.Add(kpi);

                            pdfDoc.Add(new PdfParagraph("Transactions récentes", fontHeader) { SpacingAfter = 6f });
                            pdfDoc.Add(DataGridToPdfTable(dgvTransactions, fontHeader, fontRow));
                            pdfDoc.Add(new PdfParagraph(" ", fontRow));

                            pdfDoc.Add(new PdfParagraph("Factures à payer", fontHeader) { SpacingAfter = 6f });
                            pdfDoc.Add(DataGridToPdfTable(dgvFactures, fontHeader, fontRow));
                            pdfDoc.Add(new PdfParagraph(" ", fontRow));

                            pdfDoc.Add(new PdfParagraph("Salaires (paiements)", fontHeader) { SpacingAfter = 6f });
                            pdfDoc.Add(DataGridToPdfTable(dgvSalaires, fontHeader, fontRow));

                            pdfDoc.Close();
                        }
                        stream.Close();
                    }

                    MessageBox.Show("Export PDF terminé.", "PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur export PDF : " + ex.Message, "PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private PdfPCell KpiCell(string title, string val, PdfFont font)
        {
            var ph = new PdfPhrase($"{title} : {val}", font);
            return new PdfPCell(ph)
            {
                Padding = 8,
                BackgroundColor = new PdfColor(230, 230, 230),
                HorizontalAlignment = PdfElement.ALIGN_CENTER
            };
        }

        private PdfPTable DataGridToPdfTable(DataGridView dgv, PdfFont fontHeader, PdfFont fontRow)
        {
            var visibleCols = dgv.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible)
                .OrderBy(c => c.DisplayIndex)
                .ToList();

            PdfPTable table = new PdfPTable(Math.Max(1, visibleCols.Count)) { WidthPercentage = 100 };

            foreach (var col in visibleCols)
            {
                PdfPCell cell = new PdfPCell(new PdfPhrase(col.HeaderText, fontHeader))
                {
                    BackgroundColor = new PdfColor(240, 244, 250),
                    HorizontalAlignment = PdfElement.ALIGN_CENTER,
                    Padding = 5
                };
                table.AddCell(cell);
            }

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                foreach (var col in visibleCols)
                {
                    string t = Convert.ToString(row.Cells[col.Index].Value) ?? "";
                    PdfPCell cell = new PdfPCell(new PdfPhrase(t, fontRow))
                    {
                        Padding = 5,
                        HorizontalAlignment = PdfElement.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
            }

            return table;
        }

        private string GetKpiValue(Panel card)
        {
            var values = GetKpiRawValues(card);

            decimal cdf = values.TryGetValue("CDF", out var vcdf) ? vcdf : 0m;
            decimal usd = values.TryGetValue("USD", out var vusd) ? vusd : 0m;

            return $"{FmtMoney(cdf, "CDF")} / {FmtMoney(usd, "USD")}";
        }


        private int GetCurrentEntrepriseId()
        {
          
            return ConfigSysteme.IdEntreprise; 
        }

        private int GetCurrentMagasinId()
        {
          
            return ConfigSysteme.IdMagasin; 
        }


        private void ExporterPdfBilan()
        {
            ExporterPdfDepuisEcritures(
                "BILAN COMPTABLE",
        @"
SELECT 
    e.Compte,
    e.Libelle,
    SUM(e.Debit) AS Debit,
    SUM(e.Credit) AS Credit,
    SUM(e.Debit - e.Credit) AS Solde
FROM EcrituresComptables e
WHERE e.DateEcriture >= @d1
  AND e.DateEcriture <= @d2
GROUP BY e.Compte, e.Libelle
HAVING SUM(e.Debit) <> 0 OR SUM(e.Credit) <> 0
ORDER BY e.Compte"
            );
        }
        private void ExporterPdfReleveBancaire()
        {
            DataTable dt = FillTable(null, @"
SELECT DateHeure AS Date, Motif, 
CASE WHEN TypeMouvement='Entrée' THEN Montant ELSE -Montant END AS Montant
FROM MouvementsCaisse
WHERE DateHeure BETWEEN @d1 AND @d2
ORDER BY DateHeure", dateDebut, dateFin);

            ShowTable("Relevé Bancaire", dt);
        }

        private void ExporterPdfCompteResultat()
        {
            ExporterPdfDepuisEcritures(
                "COMPTE DE RÉSULTAT",
        @"
SELECT 
    e.Compte,
    e.Libelle,
    SUM(e.Debit) AS Charges,
    SUM(e.Credit) AS Produits,
    SUM(e.Credit - e.Debit) AS Resultat
FROM EcrituresComptables e
WHERE e.DateEcriture >= @d1
  AND e.DateEcriture <= @d2
GROUP BY e.Compte, e.Libelle
HAVING SUM(e.Debit) <> 0 OR SUM(e.Credit) <> 0
ORDER BY e.Compte"
            );
        }
        private void ExporterPdfGrandLivre()
        {
            DataTable dt = FillTable(null, @"
SELECT DateHeure AS Date, Motif AS Libellé, 
CASE WHEN TypeMouvement='Entrée' THEN Montant ELSE 0 END AS Débit,
CASE WHEN TypeMouvement='Sortie' THEN Montant ELSE 0 END AS Crédit
FROM MouvementsCaisse
WHERE DateHeure BETWEEN @d1 AND @d2
ORDER BY DateHeure", dateDebut, dateFin);

            ShowTable("Grand Livre", dt);
        }
        private void ShowTable(string titre, DataTable dt)
        {
            Form f = new Form
            {
                Text = titre,
                Width = 900,
                Height = 500
            };

            DataGridView dgv = CreateGrid();
            dgv.DataSource = dt;
            dgv.Dock = DockStyle.Fill;

            f.Controls.Add(dgv);
            f.ShowDialog();
        }

        private void ExporterPdfDepuisEcritures(string titre, string sql)
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = "Fichier PDF|*.pdf",
                FileName = $"{NettoyerNomFichier(titre)}.pdf"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    DataTable dt = new DataTable();

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        {
                            cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = dateDebut;
                            cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = dateFin;

                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                da.Fill(dt);
                        }
                    }

                    using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (PdfDoc pdfDoc = new PdfDoc(PdfPageSize.A4.Rotate(), 18f, 18f, 20f, 20f))
                    {
                        PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();

                        PdfBaseFont baseFont = PdfBaseFont.CreateFont(
                            PdfBaseFont.HELVETICA,
                            PdfBaseFont.CP1252,
                            false
                        );

                        PdfFont fontTitle = new PdfFont(baseFont, 16, PdfFont.BOLD);
                        PdfFont fontHeader = new PdfFont(baseFont, 11, PdfFont.BOLD);
                        PdfFont fontRow = new PdfFont(baseFont, 10, PdfFont.NORMAL);

                        pdfDoc.Add(new PdfParagraph(titre, fontTitle)
                        {
                            Alignment = PdfElement.ALIGN_CENTER,
                            SpacingAfter = 8f
                        });

                        pdfDoc.Add(new PdfParagraph(lblPeriode.Text ?? "", fontRow)
                        {
                            Alignment = PdfElement.ALIGN_CENTER,
                            SpacingAfter = 14f
                        });

                        // ✅ TEST CORRECT
                        if (dt.Rows.Count == 0)
                        {
                            pdfDoc.Add(new PdfParagraph(
                                "Aucune écriture comptable sur la période sélectionnée.",
                                fontRow
                            ));
                            pdfDoc.Close();
                            return;
                        }

                        PdfPTable table = new PdfPTable(dt.Columns.Count)
                        {
                            WidthPercentage = 100
                        };
                        table.HeaderRows = 1;

                        float[] widths = new float[dt.Columns.Count];
                        for (int i = 0; i < widths.Length; i++)
                            widths[i] = 1f;
                        table.SetWidths(widths);

                        // En-têtes
                        foreach (DataColumn col in dt.Columns)
                        {
                            PdfPCell cell = new PdfPCell(new PdfPhrase(col.ColumnName, fontHeader))
                            {
                                BackgroundColor = new PdfColor(230, 230, 230),
                                HorizontalAlignment = PdfElement.ALIGN_CENTER,
                                Padding = 5
                            };
                            table.AddCell(cell);
                        }

                        // Données
                        foreach (DataRow r in dt.Rows)
                        {
                            foreach (DataColumn col in dt.Columns)
                            {
                                string text = FormaterCellule(r[col]);
                                PdfPCell cell = new PdfPCell(new PdfPhrase(text, fontRow))
                                {
                                    Padding = 5,
                                    HorizontalAlignment = PdfElement.ALIGN_LEFT
                                };
                                table.AddCell(cell);
                            }
                        }

                        pdfDoc.Add(table);
                        pdfDoc.Close();
                    }

                    MessageBox.Show(
                        "PDF généré : " + titre,
                        "PDF",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Erreur PDF : " + ex.Message,
                        "PDF",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private string NettoyerNomFichier(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }

        private string FormaterCellule(object value)
        {
            if (value == null || value == DBNull.Value) return "";

            // Formats propres
            if (value is DateTime dt) return dt.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            if (value is decimal dec) return dec.ToString("N2", CultureInfo.InvariantCulture);
            if (value is double dbl) return dbl.ToString("N2", CultureInfo.InvariantCulture);
            if (value is float fl) return fl.ToString("N2", CultureInfo.InvariantCulture);

            return Convert.ToString(value) ?? "";
        }

        // ========================
        // EXCEL (CSV simple)
        // ========================
        private void ExporterExcelDashboard()
        {
            using (var sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "Dashboard_Comptable.csv" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (var sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        sw.WriteLine("SECTION;COL1;COL2;COL3;COL4");

                        sw.WriteLine($"KPI;Revenus;{GetKpiValue(kpiRevenus)};;");
                        sw.WriteLine($"KPI;Dépenses;{GetKpiValue(kpiDepenses)};;");
                        sw.WriteLine($"KPI;Flux;{GetKpiValue(kpiFlux)};;");
                        sw.WriteLine($"KPI;Bénéfice;{GetKpiValue(kpiBenefice)};;");

                        sw.WriteLine();
                        sw.WriteLine("TRANSACTIONS");
                        WriteGridCsv(sw, dgvTransactions);

                        sw.WriteLine();
                        sw.WriteLine("FACTURES");
                        WriteGridCsv(sw, dgvFactures);

                        sw.WriteLine();
                        sw.WriteLine("SALAIRES");
                        WriteGridCsv(sw, dgvSalaires);
                    }

                    MessageBox.Show("Export CSV terminé.", "Excel/CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur export CSV : " + ex.Message, "Excel/CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========================
        // HELPERS (SQL)
        // ========================
        private decimal ExecDecimal(SqlConnection con, string sql, DateTime d1, DateTime d2)
        {
            bool ouvrirConnexion = false;

            if (con == null)
            {
                con = new SqlConnection(connectionString);
                ouvrirConnexion = true;
            }

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = d1;
                    cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = d2;

                    object res = cmd.ExecuteScalar();
                    return ToDec(res);
                }
            }
            finally
            {
                if (ouvrirConnexion && con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private DataTable FillTable(SqlConnection con, string sql, DateTime d1, DateTime d2)
        {
            bool ouvrirConnexion = false;

            if (con == null)
            {
                con = new SqlConnection(connectionString);
                ouvrirConnexion = true;
            }

            try
            {
                if (con.State != ConnectionState.Open)
                    con.Open();

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = d1;
                    cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = d2;

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    return dt;
                }
            }
            finally
            {
                if (ouvrirConnexion && con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private static decimal PctDelta(decimal current, decimal previous)
        {
            if (previous == 0m) return current == 0m ? 0m : 100m;
            return ((current - previous) / previous) * 100m;
        }

        private static decimal ToDec(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;

            // ✅ SQL renvoie souvent déjà un type numérique : ne pas convertir en string
            switch (value)
            {
                case decimal d: return d;
                case double d: return (decimal)d;
                case float f: return (decimal)f;
                case int i: return i;
                case long l: return l;
                case short s: return s;
                case byte b: return b;
            }

            string str = (value as string) ?? Convert.ToString(value, CultureInfo.CurrentCulture);
            return ParseDecimalFlexible(str);
        }
        private static decimal ParseDecimalFlexible(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return 0m;

            str = str.Trim().Replace(" ", "");

            // ✅ Culture courante d'abord (évite "520,00" => 52000)
            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.CurrentCulture, out var r)) return r;

            var fr = CultureInfo.GetCultureInfo("fr-FR");
            if (decimal.TryParse(str, NumberStyles.Any, fr, out r)) return r;

            // cas "4.200.000,50"
            var nfDot = new NumberFormatInfo { NumberGroupSeparator = ".", NumberDecimalSeparator = "," };
            if (decimal.TryParse(str, NumberStyles.Any, nfDot, out r)) return r;

            // fallback invariant
            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out r)) return r;

            return 0m;
        }

        private static readonly NumberFormatInfo NfCDF = new NumberFormatInfo
        {
            NumberGroupSeparator = ".",
            NumberDecimalSeparator = ","
        };

        private string FmtMoney(decimal amount, string devise)
        {
            devise = (devise ?? "").ToUpperInvariant();

            if (devise == "USD")
            {
                // Afficher 520 si entier, sinon 520.50
                if (amount == Math.Truncate(amount)) return amount.ToString("N0", CultureInfo.InvariantCulture) + " USD";
                return amount.ToString("N2", CultureInfo.InvariantCulture) + " USD";
            }

            // FC/CDF : pas de décimales, séparateur milliers en "."
            return amount.ToString("N0", NfCDF) + " CDF";
        }

        // ========================
        // HELPERS (PDF/CSV)
        // ========================

        private void WriteGridCsv(StreamWriter sw, DataGridView dgv)
        {
            var cols = dgv.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToList();
            sw.WriteLine(string.Join(";", cols.Select(c => EscapeCsv(c.HeaderText))));

            foreach (DataGridViewRow r in dgv.Rows)
            {
                if (r.IsNewRow) continue;
                sw.WriteLine(string.Join(";", cols.Select(c => EscapeCsv(Convert.ToString(r.Cells[c.Index].Value)))));
            }
        }

        private string EscapeCsv(string s)
        {
            s = s ?? "";
            s = s.Replace("\"", "\"\"");
            if (s.Contains(";") || s.Contains("\"") || s.Contains("\n"))
                return $"\"{s}\"";
            return s;
        }

        // ========================
        // HELPERS (UI)
        // ========================
       

        private Region RoundedRectRegion(System.Drawing.Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return new Region(path);
        }

        private void ApplyRounded(Control c, int radius)
        {
            if (c.Width <= 0 || c.Height <= 0) return;
            c.Region = RoundedRectRegion(new System.Drawing.Rectangle(0, 0, c.Width, c.Height), radius);
        }

        private System.Drawing.Image LoadAssetImage(string relativePath, int w, int h)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string full = Path.Combine(baseDir, relativePath);

                if (File.Exists(full))
                {
                    using (System.Drawing.Image img = System.Drawing.Image.FromFile(full))
                    {
                        Bitmap resized = new Bitmap(w, h);
                        using (Graphics g = Graphics.FromImage(resized))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.DrawImage(img, new System.Drawing.Rectangle(0, 0, w, h));
                        }
                        return resized;
                    }
                }
            }
            catch { }

            using (Bitmap bmp = SystemIcons.Information.ToBitmap())
            {
                return new Bitmap(bmp, new Size(w, h));
            }
        }

        // ========================
        // MODEL
        // ========================
        private sealed class DashboardModel
        {
            // === KPI globaux (somme toutes devises) ===
            public decimal Revenus { get; set; }
            public decimal Depenses { get; set; }
            public decimal FluxTresorerie { get; set; }
            public decimal BeneficeNet { get; set; }

            // === KPI par devise (OBLIGATOIRE) ===
            public Dictionary<string, decimal> RevenusParDevise { get; set; }
                = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            public Dictionary<string, decimal> DepensesParDevise { get; set; }
                = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            // === Deltas ===
            public decimal DeltaRevenus { get; set; }
            public decimal DeltaDepenses { get; set; }
            public decimal DeltaFlux { get; set; }
            public decimal DeltaBenefice { get; set; }

            // === Données graphiques & tableaux ===
            public DataTable CashflowJour { get; set; }
            public DataTable DepensesParCategorie { get; set; }

            public DataTable Transactions { get; set; }
            public DataTable Factures { get; set; }
            public DataTable Salaires { get; set; }
        }
    }
}
