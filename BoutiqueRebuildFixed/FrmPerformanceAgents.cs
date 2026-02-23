using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmPerformanceAgents : Form
    {
        private readonly string cs = ConfigSysteme.ConnectionString;

        private ComboBox cmbEmploye, cmbDevise;
        private DateTimePicker dtDebut, dtFin;
        private Button btnActualiser, btnFermer;
        private DataGridView dgvPerf;
        private Label lblResume;
        public FrmPerformanceAgents()
        {
            InitializeComponent();

            this.Load += FrmPerformanceAgents_Load;

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }

        private void RafraichirLangue() => ConfigSysteme.AppliquerTraductions(this);
        private void RafraichirTheme() => ConfigSysteme.AppliquerTheme(this);

        private void FrmPerformanceAgents_Load(object sender, EventArgs e)
        {
            InitializeUi();

            // ✅ Branche les events après création
            HookEvents();

            RafraichirLangue();
            RafraichirTheme();

            ChargerEmployes();
            ChargerPerformance();
        }

        private void InitializeUi()
        {
            this.Text = "Performance des agents";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(1100, 650);

            // ===== TOP FILTER BAR =====
            var top = new Panel { Dock = DockStyle.Top, Height = 85, Padding = new Padding(10) };

            Label L(string t) => new Label { Text = t, AutoSize = true, ForeColor = Color.Black };

            cmbEmploye = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, Name = "cmbEmploye" };
            cmbDevise = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90, Name = "cmbDevise" };

            dtDebut = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, Name = "dtDebut" };
            dtFin = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, Name = "dtFin" };

            btnActualiser = new Button { Text = "Actualiser", Width = 120, Height = 32, Name = "btnActualiser" };
            btnFermer = new Button { Text = "Fermer", Width = 120, Height = 32, Name = "btnFermer" };

            // dates défaut : semaine en cours
            var today = DateTime.Today.Date;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var lundi = today.AddDays(-diff);
            dtDebut.Value = lundi;
            dtFin.Value = lundi.AddDays(6);

            cmbDevise.Items.Clear();
            cmbDevise.Items.AddRange(new object[] { "Toutes", "CDF", "USD" });
            cmbDevise.SelectedIndex = 0;

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true
            };

            flow.Controls.Add(L("Employé"));
            flow.Controls.Add(cmbEmploye);
            flow.Controls.Add(new Label { Width = 12 });

            flow.Controls.Add(L("Début"));
            flow.Controls.Add(dtDebut);
            flow.Controls.Add(new Label { Width = 12 });

            flow.Controls.Add(L("Fin"));
            flow.Controls.Add(dtFin);
            flow.Controls.Add(new Label { Width = 12 });

            flow.Controls.Add(L("Devise"));
            flow.Controls.Add(cmbDevise);
            flow.Controls.Add(new Label { Width = 12 });

            flow.Controls.Add(btnActualiser);
            flow.Controls.Add(btnFermer);

            top.Controls.Add(flow);

            // ===== RESUME =====
            lblResume = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(10, 6, 10, 6),
                Text = "—",
                AutoEllipsis = true
            };

            // ===== GRID =====
            dgvPerf = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };

            // ✅ Très important: clear si designer a déjà des controls
            this.Controls.Clear();

            // ✅ Ordre Dock
            this.Controls.Add(dgvPerf);
            this.Controls.Add(lblResume);
            this.Controls.Add(top);
        }

        private void HookEvents()
        {
            btnActualiser.Click += (s, e) =>
            {
                GenererEtEnregistrerPerformance();
                ChargerDepuisTable();
            };
            btnFermer.Click += (s, e) => this.Close();
        }

        private void GenererEtEnregistrerPerformance()
        {
            try
            {
                int employeId = Convert.ToInt32(cmbEmploye.SelectedValue);
                string devise = (cmbDevise.Text ?? "Toutes").Trim();

                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    string sql = @"
DECLARE @Debut date = @pDebut;
DECLARE @Fin   date = @pFin;
DECLARE @Emp   int  = @pEmp;
DECLARE @Dev   nvarchar(10) = @pDev;

;WITH V AS
(
    SELECT
        v.ID_Vente,
        v.DateVente,
        v.IDEmploye,
        v.MontantTotal,
        Devise = UPPER(LTRIM(RTRIM(ISNULL(v.Devise,'CDF'))))
    FROM Vente v
    WHERE v.DateVente >= @Debut
      AND v.DateVente < DATEADD(DAY,1,@Fin)
      AND (@Emp = 0 OR v.IDEmploye = @Emp)
      AND (@Dev = 'Toutes' OR UPPER(LTRIM(RTRIM(ISNULL(v.Devise,'CDF')))) = UPPER(LTRIM(RTRIM(@Dev))))
),
D AS
(
    SELECT d.ID_Vente, Qte = CAST(ISNULL(d.Quantite,0) AS DECIMAL(18,2))
    FROM DetailsVente d
),
A AS
(
    SELECT a.IdVente, a.ManagerValide
    FROM AnnulationVente a
)
INSERT INTO dbo.PerformanceAgents
(
    DateDebut, DateFin,
    IDEmploye, NomEmploye,
    Devise,
    NbTickets, CA, QteArticles, PanierMoyen, ArticlesParTicket,
    NbAnnulations, CAAnnule, NbAnnulationsValidees,
    GenerePar, Motif
)
SELECT
    @Debut, @Fin,
    e.ID_Employe,
    LTRIM(RTRIM(ISNULL(e.Nom,''))) + ' ' + LTRIM(RTRIM(ISNULL(e.Prenom,''))) AS NomEmploye,
    @Dev,
    COUNT(DISTINCT V.ID_Vente) AS NbTickets,
    ISNULL(SUM(V.MontantTotal),0) AS CA,
    ISNULL(SUM(D.Qte),0) AS QteArticles,
    CASE WHEN COUNT(DISTINCT V.ID_Vente)=0 THEN 0 ELSE ISNULL(SUM(V.MontantTotal),0)/COUNT(DISTINCT V.ID_Vente) END,
    CASE WHEN COUNT(DISTINCT V.ID_Vente)=0 THEN 0 ELSE ISNULL(SUM(D.Qte),0)/COUNT(DISTINCT V.ID_Vente) END,
    ISNULL(SUM(CASE WHEN A.IdVente IS NOT NULL THEN 1 ELSE 0 END),0) AS NbAnnulations,
    ISNULL(SUM(CASE WHEN A.IdVente IS NOT NULL THEN V.MontantTotal ELSE 0 END),0) AS CAAnnule,
    ISNULL(SUM(CASE WHEN A.IdVente IS NOT NULL AND ISNULL(NULLIF(LTRIM(RTRIM(A.ManagerValide)),''),'')<>'' THEN 1 ELSE 0 END),0) AS NbAnnulationsValidees,
    @pGenerePar,
    'Génération automatique performance'
FROM Employes e
LEFT JOIN V ON V.IDEmploye = e.ID_Employe
LEFT JOIN D ON D.ID_Vente = V.ID_Vente
LEFT JOIN A ON A.IdVente  = V.ID_Vente
WHERE (@Emp = 0 OR e.ID_Employe = @Emp)
GROUP BY e.ID_Employe, e.Nom, e.Prenom;
";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@pDebut", SqlDbType.Date).Value = dtDebut.Value.Date;
                        cmd.Parameters.Add("@pFin", SqlDbType.Date).Value = dtFin.Value.Date;
                        cmd.Parameters.Add("@pEmp", SqlDbType.Int).Value = employeId;
                        cmd.Parameters.Add("@pDev", SqlDbType.NVarChar, 10).Value = devise;
                        cmd.Parameters.Add("@pGenerePar", SqlDbType.NVarChar, 80).Value =
                            (SessionEmploye.Nom + " " + SessionEmploye.Prenom).Trim();

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur génération performance : " + ex.Message);
            }
        }

        private void ChargerDepuisTable()
        {
            try
            {
                int employeId = Convert.ToInt32(cmbEmploye.SelectedValue);
                string devise = (cmbDevise.Text ?? "Toutes").Trim();

                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    string sql = @"
SELECT TOP (500)
    IdPerformance,
    DateDebut, DateFin,
    IDEmploye, NomEmploye,
    Devise,
    NbTickets, CA, QteArticles,
    PanierMoyen, ArticlesParTicket,
    NbAnnulations, CAAnnule, NbAnnulationsValidees,
    GenerePar, DateGeneration,
    ManagerValide, DateValidation
FROM dbo.PerformanceAgents
WHERE DateDebut = @pDebut AND DateFin = @pFin
  AND (@pEmp = 0 OR IDEmploye = @pEmp)
  AND (@pDev = 'Toutes' OR Devise = @pDev)
ORDER BY DateGeneration DESC, CA DESC;
";

                    var dt = new DataTable();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@pDebut", SqlDbType.Date).Value = dtDebut.Value.Date;
                        cmd.Parameters.Add("@pFin", SqlDbType.Date).Value = dtFin.Value.Date;
                        cmd.Parameters.Add("@pEmp", SqlDbType.Int).Value = employeId;
                        cmd.Parameters.Add("@pDev", SqlDbType.NVarChar, 10).Value = devise;

                        using (var da = new SqlDataAdapter(cmd))
                            da.Fill(dt);
                    }

                    dgvPerf.DataSource = dt;

                    if (dgvPerf.Columns.Contains("CA"))
                    {
                        dgvPerf.Columns["CA"].DefaultCellStyle.Format = "N2";
                        dgvPerf.Columns["CA"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }

                    lblResume.Text = $"Snapshots: {dt.Rows.Count} | Période: {dtDebut.Value:dd/MM/yyyy}-{dtFin.Value:dd/MM/yyyy} | Devise: {devise}";
                    dgvPerf.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement table performance : " + ex.Message);
            }
        }

        private void ChargerEmployes()
        {
            try
            {
                // ✅ Sécurité : cmbEmploye doit exister
                if (cmbEmploye == null)
                {
                    cmbEmploye = this.Controls.Find("cmbEmploye", true)
                                              .OfType<ComboBox>()
                                              .FirstOrDefault();
                }

                if (cmbEmploye == null)
                {
                    MessageBox.Show("Le contrôle 'cmbEmploye' est introuvable. Vérifie son Name dans le Designer ou sa création dynamique.",
                        "Erreur UI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    var dt = new DataTable();

                    using (var cmd = new SqlCommand(@"
SELECT
    Id = e.ID_Employe,
    NomEmploye = LTRIM(RTRIM(ISNULL(e.Nom,''))) + ' ' + LTRIM(RTRIM(ISNULL(e.Prenom,'')))
FROM Employes e
ORDER BY e.Nom, e.Prenom;", con))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    // ✅ Ajouter "Tous" en première ligne
                    var dt2 = dt.Clone();
                    dt2.Rows.Add(0, "Tous");
                    foreach (DataRow r in dt.Rows) dt2.ImportRow(r);

                    // ✅ DataBind propre (évite bugs de SelectedValue)
                    cmbEmploye.DataSource = null;
                    cmbEmploye.DisplayMember = "NomEmploye";
                    cmbEmploye.ValueMember = "Id";
                    cmbEmploye.DataSource = dt2;

                    // ✅ Sélection par défaut : employé connecté sinon Tous
                    int sessionId = SessionEmploye.ID_Employe;

                    bool existe = dt2.AsEnumerable().Any(x => x.Field<int>("Id") == sessionId);

                    cmbEmploye.SelectedValue = (sessionId > 0 && existe) ? sessionId : 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement employés : " + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        private void ChargerPerformance()
        {
            try
            {
                // ✅ Sécurité UI
                if (cmbEmploye == null || cmbDevise == null || dtDebut == null || dtFin == null || dgvPerf == null || lblResume == null)
                {
                    MessageBox.Show(
                        "UI non initialisée (cmbEmploye/dgvPerf...). Appelle InitializeUi() avant.",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dtDebut.Value.Date > dtFin.Value.Date)
                {
                    MessageBox.Show("Date début > date fin.");
                    return;
                }

                if (cmbEmploye.SelectedValue == null)
                {
                    MessageBox.Show("Aucun employé sélectionné.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int employeId = Convert.ToInt32(cmbEmploye.SelectedValue);
                string devise = (cmbDevise.Text ?? "Toutes").Trim();

                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    string sql = @"
DECLARE @Debut date = @pDebut;
DECLARE @Fin   date = @pFin;
DECLARE @Emp   int  = @pEmp;
DECLARE @Dev   nvarchar(10) = @pDev;

;WITH V AS
(
    SELECT 
        v.ID_Vente,
        v.DateVente,
        v.IDEmploye,
        v.MontantTotal,
        Devise = UPPER(LTRIM(RTRIM(ISNULL(v.Devise,'CDF'))))
    FROM Vente v
    WHERE v.DateVente >= @Debut
      AND v.DateVente < DATEADD(DAY, 1, @Fin)
      AND v.DateAnnulation IS NULL
      AND (@Emp = 0 OR v.IDEmploye = @Emp)
      AND (@Dev = 'Toutes' OR UPPER(LTRIM(RTRIM(ISNULL(v.Devise,'CDF')))) = UPPER(LTRIM(RTRIM(@Dev))))
),
D AS
(
    SELECT 
        d.ID_Vente,
        Quantite = ISNULL(d.Quantite,0),
        Montant  = ISNULL(d.Montant,0),
        Devise   = UPPER(LTRIM(RTRIM(ISNULL(d.Devise,'CDF'))))
    FROM DetailsVente d
)
SELECT
    e.ID_Employe,
    NomEmploye = LTRIM(RTRIM(ISNULL(e.Nom,''))) + ' ' + LTRIM(RTRIM(ISNULL(e.Prenom,''))),

    NbTickets = COUNT(DISTINCT V.ID_Vente),

    CA = ISNULL(SUM(V.MontantTotal), 0),

    QteArticles = ISNULL(SUM(D.Quantite), 0),

    PanierMoyen = CASE WHEN COUNT(DISTINCT V.ID_Vente) = 0 THEN 0
                      ELSE ISNULL(SUM(V.MontantTotal),0) * 1.0 / COUNT(DISTINCT V.ID_Vente) END,

    ArticlesParTicket = CASE WHEN COUNT(DISTINCT V.ID_Vente) = 0 THEN 0
                             ELSE ISNULL(SUM(D.Quantite),0) * 1.0 / COUNT(DISTINCT V.ID_Vente) END
FROM Employes e
LEFT JOIN V ON V.IDEmploye = e.ID_Employe
LEFT JOIN D ON D.ID_Vente = V.ID_Vente
WHERE (@Emp = 0 OR e.ID_Employe = @Emp)
GROUP BY e.ID_Employe, e.Nom, e.Prenom
ORDER BY CA DESC, NbTickets DESC;
";

                    var dt = new DataTable();
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@pDebut", SqlDbType.Date).Value = dtDebut.Value.Date;
                        cmd.Parameters.Add("@pFin", SqlDbType.Date).Value = dtFin.Value.Date;
                        cmd.Parameters.Add("@pEmp", SqlDbType.Int).Value = employeId;
                        cmd.Parameters.Add("@pDev", SqlDbType.NVarChar, 10).Value = devise;

                        using (var da = new SqlDataAdapter(cmd))
                            da.Fill(dt);
                    }

                    dgvPerf.DataSource = dt;

                    // ✅ Formatage PRO
                    if (dgvPerf.Columns.Contains("ID_Employe"))
                        dgvPerf.Columns["ID_Employe"].Visible = false;

                    if (dgvPerf.Columns.Contains("CA"))
                    {
                        dgvPerf.Columns["CA"].DefaultCellStyle.Format = "N2";
                        dgvPerf.Columns["CA"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }

                    if (dgvPerf.Columns.Contains("PanierMoyen"))
                    {
                        dgvPerf.Columns["PanierMoyen"].DefaultCellStyle.Format = "N2";
                        dgvPerf.Columns["PanierMoyen"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }

                    if (dgvPerf.Columns.Contains("QteArticles"))
                    {
                        dgvPerf.Columns["QteArticles"].DefaultCellStyle.Format = "N0";
                        dgvPerf.Columns["QteArticles"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }

                    if (dgvPerf.Columns.Contains("ArticlesParTicket"))
                    {
                        dgvPerf.Columns["ArticlesParTicket"].DefaultCellStyle.Format = "N2";
                        dgvPerf.Columns["ArticlesParTicket"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }

                    // ✅ Résumé
                    decimal totalCA = 0m;
                    int totalTickets = 0;
                    decimal totalQte = 0m;

                    foreach (DataRow r in dt.Rows)
                    {
                        totalCA += SafeDec(r["CA"]);
                        totalTickets += SafeInt(r["NbTickets"]);
                        totalQte += SafeDec(r["QteArticles"]);
                    }

                    lblResume.Text =
                        $"Période: {dtDebut.Value:dd/MM/yyyy} - {dtFin.Value:dd/MM/yyyy} | " +
                        $"Devise: {devise} | Total CA: {totalCA:N2} | Tickets: {totalTickets} | Articles: {totalQte:N0}";

                    dgvPerf.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement performance : " + ex.Message);
            }
        }

        private int SafeInt(object o)
        {
            if (o == null || o == DBNull.Value) return 0;
            var s = o.ToString().Trim().Replace(" ", "").Replace("\u00A0", "");
            return int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out int r) ? r : 0;
        }

        private decimal SafeDec(object o)
        {
            if (o == null || o == DBNull.Value) return 0m;
            if (o is decimal dd) return dd;

            var s = o.ToString().Trim().Replace("\u00A0", "").Replace(" ", "");
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out var r)) return r;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out r)) return r;
            return 0m;
        }
    }
}
