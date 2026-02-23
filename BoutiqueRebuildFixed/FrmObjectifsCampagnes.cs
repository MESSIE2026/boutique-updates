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
    public partial class FrmObjectifsCampagnes : Form
    {
        private readonly string cs = ConfigSysteme.ConnectionString;
        private int selectedObjectifId = -1;

        // ✅ Contrôles (déclarés ici => plus d'erreur "does not exist")
        private DataGridView dgvObjectifs;
        private ComboBox cmbAgent, cmbPeriode, cmbDevise;
        private DateTimePicker dtDebut, dtFin;
        private TextBox txtObjectifNbVentes, txtObjectifMontant, txtCommentaire;
        private Button btnAjouter, btnModifier, btnSupprimer, btnAnnuler;
        public FrmObjectifsCampagnes()
        {
            InitializeComponent();

            ConstruireUI();        // ✅ crée les contrôles
            HookEvents();          // ✅ branche les events

            this.Load += FrmObjectifsCampagnes_Load;

            // Theme/Langue
            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }

        private void ConstruireUI()
        {
            this.Text = "Objectifs - Campagnes";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(1050, 600);

            // ===== TOP PANEL (Formulaire) =====
            // ✅ plus haut pour éviter que les boutons couvrent Montant/Note
            var top = new Panel { Dock = DockStyle.Top, Height = 190, Padding = new Padding(10) };
            this.Controls.Add(top);

            // Labels
            Label L(string t) => new Label { Text = t, AutoSize = true, ForeColor = Color.Black };

            cmbAgent = new ComboBox { Name = "cmbAgent", DropDownStyle = ComboBoxStyle.DropDownList, Width = 240 };
            cmbPeriode = new ComboBox { Name = "cmbPeriode", DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            cmbDevise = new ComboBox { Name = "cmbDevise", DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };

            dtDebut = new DateTimePicker { Name = "dtDebut", Format = DateTimePickerFormat.Short, Width = 120 };
            dtFin = new DateTimePicker { Name = "dtFin", Format = DateTimePickerFormat.Short, Width = 120 };

            txtObjectifNbVentes = new TextBox { Name = "txtObjectifNbVentes", Width = 120, Text = "0" };
            txtObjectifMontant = new TextBox { Name = "txtObjectifMontant", Width = 160, Text = "0" };
            txtCommentaire = new TextBox { Name = "txtCommentaire", Width = 420 };

            btnAjouter = new Button { Name = "btnAjouter", Text = "Ajouter", Width = 110, Height = 32 };
            btnModifier = new Button { Name = "btnModifier", Text = "Modifier", Width = 110, Height = 32 };
            btnSupprimer = new Button { Name = "btnSupprimer", Text = "Supprimer", Width = 110, Height = 32 };
            btnAnnuler = new Button { Name = "btnAnnuler", Text = "Annuler", Width = 110, Height = 32 };

            // ✅ Layout (TableLayoutPanel) — Dock TOP (pas Fill) pour laisser place aux boutons
            var gridForm = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 6,
                RowCount = 3,
                Height = 110,
                AutoSize = false
            };

            gridForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            gridForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            gridForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            gridForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            gridForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            gridForm.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));

            gridForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            gridForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            gridForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            // Row 0
            gridForm.Controls.Add(L("Agent"), 0, 0);
            gridForm.Controls.Add(cmbAgent, 1, 0);
            gridForm.Controls.Add(L("Période"), 2, 0);
            gridForm.Controls.Add(cmbPeriode, 3, 0);
            gridForm.Controls.Add(L("Devise"), 4, 0);
            gridForm.Controls.Add(cmbDevise, 5, 0);

            // Row 1
            gridForm.Controls.Add(L("Début"), 0, 1);
            gridForm.Controls.Add(dtDebut, 1, 1);
            gridForm.Controls.Add(L("Fin"), 2, 1);
            gridForm.Controls.Add(dtFin, 3, 1);
            gridForm.Controls.Add(L("Ventes"), 4, 1);
            gridForm.Controls.Add(txtObjectifNbVentes, 5, 1);

            // Row 2
            gridForm.Controls.Add(L("Montant"), 0, 2);
            gridForm.Controls.Add(txtObjectifMontant, 1, 2);
            gridForm.Controls.Add(L("Note"), 2, 2);
            gridForm.Controls.Add(txtCommentaire, 3, 2);

            // ✅ Boutons plus bas (padding top)
            var pnlBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                Padding = new Padding(0, 10, 0, 0), // ✅ descendre les boutons
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            pnlBtns.Controls.AddRange(new Control[] { btnAjouter, btnModifier, btnSupprimer, btnAnnuler });

            top.Controls.Add(gridForm);
            top.Controls.Add(pnlBtns);

            // ===== GRID =====
            dgvObjectifs = new DataGridView
            {
                Name = "dgvObjectifs",
                Dock = DockStyle.Fill,
                Visible = true
            };
            this.Controls.Add(dgvObjectifs);

            // config
            ConfigurerGrid();
            InitialiserPeriode();
            InitialiserDevise();

            // ✅ ordre visuel sûr
            top.BringToFront();
            dgvObjectifs.BringToFront(); // le dgv reste sous top (Dock Fill)
        }

        private void HookEvents()
        {
            btnAjouter.Click += btnAjouter_Click;
            btnModifier.Click += btnModifier_Click;
            btnSupprimer.Click += btnSupprimer_Click;
            btnAnnuler.Click += (s, e) => Reinitialiser();

            dgvObjectifs.CellClick += dgvObjectifs_CellClick;

            cmbPeriode.SelectedIndexChanged += (s, e) => AjusterDatesSelonPeriode();
        }

        private void FrmObjectifsCampagnes_Load(object sender, EventArgs e)
        {
            ConfigSysteme.AppliquerTraductions(this);
            ConfigSysteme.AppliquerTheme(this);

            ChargerAgents();
            ChargerObjectifs();
            Reinitialiser();
        }

        private void RafraichirLangue() => ConfigSysteme.AppliquerTraductions(this);
        private void RafraichirTheme() => ConfigSysteme.AppliquerTheme(this);

        // =========================
        // UI INIT
        // =========================
        private void ConfigurerGrid()
        {
            dgvObjectifs.ReadOnly = true;
            dgvObjectifs.AllowUserToAddRows = false;
            dgvObjectifs.AllowUserToDeleteRows = false;
            dgvObjectifs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvObjectifs.MultiSelect = false;
            dgvObjectifs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvObjectifs.RowHeadersVisible = false;
        }

        private void InitialiserPeriode()
        {
            cmbPeriode.Items.Clear();
            cmbPeriode.Items.AddRange(new[] { "Jour", "Semaine", "Mois" });
            cmbPeriode.SelectedIndex = 1; // semaine
            AjusterDatesSelonPeriode();
        }

        private void InitialiserDevise()
        {
            cmbDevise.Items.Clear();
            cmbDevise.Items.AddRange(new[] { "CDF", "USD", "FC", "$" });
            cmbDevise.SelectedIndex = 0;
        }

        private void AjusterDatesSelonPeriode()
        {
            var today = DateTime.Today.Date;

            if (cmbPeriode.Text == "Jour")
            {
                dtDebut.Value = today;
                dtFin.Value = today;
            }
            else if (cmbPeriode.Text == "Semaine")
            {
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var lundi = today.AddDays(-diff);
                dtDebut.Value = lundi;
                dtFin.Value = lundi.AddDays(6);
            }
            else if (cmbPeriode.Text == "Mois")
            {
                var first = new DateTime(today.Year, today.Month, 1);
                dtDebut.Value = first;
                dtFin.Value = first.AddMonths(1).AddDays(-1);
            }
        }

        // =========================
        // DATA
        // =========================
        private void ChargerAgents()
        {
            try
            {
                using (var con = new SqlConnection(cs))
                {
                    con.Open();
                    var dt = new DataTable();

                    using (var cmd = new SqlCommand(@"
SELECT 
    Id = e.ID_Employe,
    NomAgent = LTRIM(RTRIM(ISNULL(e.Nom,''))) + ' ' + LTRIM(RTRIM(ISNULL(e.Prenom,'')))
FROM Employes e
ORDER BY e.Nom, e.Prenom;", con))
                    using (var da = new SqlDataAdapter(cmd))
                        da.Fill(dt);

                    cmbAgent.DisplayMember = "NomAgent";
                    cmbAgent.ValueMember = "Id";
                    cmbAgent.DataSource = dt;

                    // Optionnel : éviter un SelectedValue invalide après refresh
                    if (dt.Rows.Count > 0) cmbAgent.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement agents : " + ex.Message);
            }
        }


        private void ChargerObjectifs()
        {
            try
            {
                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    // ✅ Visualise la table ObjectifsCampagnesAgents + calcule les réalisés
                    string sql = @"
SELECT 
    o.Id,
    o.AgentId,
    o.NomAgent,
    o.PeriodeType,
    o.DateDebut,
    o.DateFin,
    o.ObjectifNbVentes,
    o.ObjectifMontant,
    o.Devise,
    o.CampagneId,
    o.Commentaire,
    o.DateCreation,

    -- ✅ Réalisé : Nb ventes (non annulées)
    (SELECT COUNT(v.ID_Vente)
     FROM Vente v
     WHERE v.DateVente >= o.DateDebut
       AND v.DateVente < DATEADD(DAY, 1, o.DateFin)
       AND v.IDEmploye = o.AgentId
       AND v.DateAnnulation IS NULL
    ) AS RealiseNbVentes,

    -- ✅ Réalisé : MontantTotal (non annulées)
    (SELECT ISNULL(SUM(v.MontantTotal), 0)
     FROM Vente v
     WHERE v.DateVente >= o.DateDebut
       AND v.DateVente < DATEADD(DAY, 1, o.DateFin)
       AND v.IDEmploye = o.AgentId
       AND v.DateAnnulation IS NULL
       AND UPPER(LTRIM(RTRIM(ISNULL(v.Devise,'CDF')))) = UPPER(LTRIM(RTRIM(ISNULL(o.Devise,'CDF'))))
    ) AS RealiseMontant
FROM ObjectifsCampagnesAgents o
ORDER BY o.DateDebut DESC, o.NomAgent;
";

                    var dt = new DataTable();
                    using (var da = new SqlDataAdapter(sql, con))
                        da.Fill(dt);

                    // ✅ Colonnes % atteinte
                    if (!dt.Columns.Contains("AtteinteNbPct"))
                        dt.Columns.Add("AtteinteNbPct", typeof(string));
                    if (!dt.Columns.Contains("AtteinteMontantPct"))
                        dt.Columns.Add("AtteinteMontantPct", typeof(string));

                    foreach (DataRow r in dt.Rows)
                    {
                        int objNb = SafeInt(r["ObjectifNbVentes"]);
                        decimal objMt = SafeDec(r["ObjectifMontant"]);

                        int realNb = SafeInt(r["RealiseNbVentes"]);
                        decimal realMt = SafeDec(r["RealiseMontant"]);

                        r["AtteinteNbPct"] = (objNb <= 0) ? "—" : $"{(realNb * 100m / objNb):F1}%";
                        r["AtteinteMontantPct"] = (objMt <= 0) ? "—" : $"{(realMt * 100m / objMt):F1}%";
                    }

                    dgvObjectifs.DataSource = dt;

                    // ✅ Masquer colonnes techniques si tu veux
                    if (dgvObjectifs.Columns.Contains("Id"))
                        dgvObjectifs.Columns["Id"].Visible = false;

                    if (dgvObjectifs.Columns.Contains("AgentId"))
                        dgvObjectifs.Columns["AgentId"].Visible = false;

                    // ✅ (Optionnel) headers plus propres
                    if (dgvObjectifs.Columns.Contains("NomAgent")) dgvObjectifs.Columns["NomAgent"].HeaderText = "Agent";
                    if (dgvObjectifs.Columns.Contains("PeriodeType")) dgvObjectifs.Columns["PeriodeType"].HeaderText = "Période";
                    if (dgvObjectifs.Columns.Contains("ObjectifNbVentes")) dgvObjectifs.Columns["ObjectifNbVentes"].HeaderText = "Obj. Ventes";
                    if (dgvObjectifs.Columns.Contains("ObjectifMontant")) dgvObjectifs.Columns["ObjectifMontant"].HeaderText = "Obj. Montant";
                    if (dgvObjectifs.Columns.Contains("RealiseNbVentes")) dgvObjectifs.Columns["RealiseNbVentes"].HeaderText = "Réalisé Ventes";
                    if (dgvObjectifs.Columns.Contains("RealiseMontant")) dgvObjectifs.Columns["RealiseMontant"].HeaderText = "Réalisé Montant";
                    if (dgvObjectifs.Columns.Contains("AtteinteNbPct")) dgvObjectifs.Columns["AtteinteNbPct"].HeaderText = "% Ventes";
                    if (dgvObjectifs.Columns.Contains("AtteinteMontantPct")) dgvObjectifs.Columns["AtteinteMontantPct"].HeaderText = "% Montant";

                    dgvObjectifs.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement objectifs : " + ex.Message);
            }
        }


        // =========================
        // CRUD
        // =========================
        private bool Verifier()
        {
            if (cmbAgent.SelectedValue == null) { MessageBox.Show("Sélectionne un agent."); return false; }
            if (dtDebut.Value.Date > dtFin.Value.Date) { MessageBox.Show("Date début > date fin."); return false; }

            if (!int.TryParse(txtObjectifNbVentes.Text.Trim(), out int nb) || nb < 0)
            { MessageBox.Show("Objectif ventes invalide."); return false; }

            if (!decimal.TryParse(txtObjectifMontant.Text.Trim().Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out decimal mt) || mt < 0)
            { MessageBox.Show("Objectif montant invalide."); return false; }

            return true;
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            if (!Verifier()) return;

            try
            {
                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    string sql = @"
INSERT INTO ObjectifsCampagnesAgents
(AgentId, NomAgent, PeriodeType, DateDebut, DateFin, ObjectifNbVentes, ObjectifMontant, Devise, Commentaire)
VALUES
(@AgentId, @NomAgent, @PeriodeType, @DateDebut, @DateFin, @ObjNb, @ObjMontant, @Devise, @Commentaire);
";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@AgentId", Convert.ToInt32(cmbAgent.SelectedValue));
                        cmd.Parameters.AddWithValue("@NomAgent", cmbAgent.Text.Trim());
                        cmd.Parameters.AddWithValue("@PeriodeType", cmbPeriode.Text.Trim());
                        cmd.Parameters.AddWithValue("@DateDebut", dtDebut.Value.Date);
                        cmd.Parameters.AddWithValue("@DateFin", dtFin.Value.Date);
                        cmd.Parameters.AddWithValue("@ObjNb", int.Parse(txtObjectifNbVentes.Text.Trim()));
                        cmd.Parameters.Add("@ObjMontant", SqlDbType.Decimal).Value = SafeDec(txtObjectifMontant.Text);
                        cmd.Parameters.AddWithValue("@Devise", cmbDevise.Text.Trim());
                        cmd.Parameters.AddWithValue("@Commentaire", txtCommentaire.Text.Trim());

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Objectif ajouté.");
                ChargerObjectifs();
                Reinitialiser();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ajout objectif : " + ex.Message);
            }
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            if (selectedObjectifId <= 0) { MessageBox.Show("Sélectionne une ligne à modifier."); return; }
            if (!Verifier()) return;

            try
            {
                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    string sql = @"
UPDATE ObjectifsCampagnesAgents SET
    AgentId = @AgentId,
    NomAgent = @NomAgent,
    PeriodeType = @PeriodeType,
    DateDebut = @DateDebut,
    DateFin = @DateFin,
    ObjectifNbVentes = @ObjNb,
    ObjectifMontant = @ObjMontant,
    Devise = @Devise,
    Commentaire = @Commentaire
WHERE Id = @Id;
";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", selectedObjectifId);
                        cmd.Parameters.AddWithValue("@AgentId", Convert.ToInt32(cmbAgent.SelectedValue));
                        cmd.Parameters.AddWithValue("@NomAgent", cmbAgent.Text.Trim());
                        cmd.Parameters.AddWithValue("@PeriodeType", cmbPeriode.Text.Trim());
                        cmd.Parameters.AddWithValue("@DateDebut", dtDebut.Value.Date);
                        cmd.Parameters.AddWithValue("@DateFin", dtFin.Value.Date);
                        cmd.Parameters.AddWithValue("@ObjNb", int.Parse(txtObjectifNbVentes.Text.Trim()));
                        cmd.Parameters.Add("@ObjMontant", SqlDbType.Decimal).Value = SafeDec(txtObjectifMontant.Text);
                        cmd.Parameters.AddWithValue("@Devise", cmbDevise.Text.Trim());
                        cmd.Parameters.AddWithValue("@Commentaire", txtCommentaire.Text.Trim());

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Objectif modifié.");
                ChargerObjectifs();
                Reinitialiser();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur modification : " + ex.Message);
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (selectedObjectifId <= 0) { MessageBox.Show("Sélectionne une ligne à supprimer."); return; }

            if (MessageBox.Show("Supprimer cet objectif ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using (var con = new SqlConnection(cs))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("DELETE FROM ObjectifsCampagnesAgents WHERE Id=@Id", con))
                    {
                        cmd.Parameters.AddWithValue("@Id", selectedObjectifId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Objectif supprimé.");
                ChargerObjectifs();
                Reinitialiser();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur suppression : " + ex.Message);
            }
        }

        private void dgvObjectifs_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvObjectifs.Rows[e.RowIndex];
            selectedObjectifId = SafeInt(row.Cells["Id"].Value);

            // ✅ AgentId -> SelectedValue
            if (dgvObjectifs.Columns.Contains("AgentId") &&
                row.Cells["AgentId"].Value != null &&
                row.Cells["AgentId"].Value != DBNull.Value)
            {
                cmbAgent.SelectedValue = Convert.ToInt32(row.Cells["AgentId"].Value);
            }

            cmbPeriode.Text = row.Cells["PeriodeType"].Value?.ToString() ?? "";
            dtDebut.Value = Convert.ToDateTime(row.Cells["DateDebut"].Value);
            dtFin.Value = Convert.ToDateTime(row.Cells["DateFin"].Value);

            txtObjectifNbVentes.Text = row.Cells["ObjectifNbVentes"].Value?.ToString() ?? "0";
            txtObjectifMontant.Text = SafeDec(row.Cells["ObjectifMontant"].Value)
                .ToString("0.##", CultureInfo.InvariantCulture);

            cmbDevise.Text = row.Cells["Devise"].Value?.ToString() ?? "CDF";

            // ✅ Commentaire (si la colonne existe)
            if (dgvObjectifs.Columns.Contains("Commentaire"))
                txtCommentaire.Text = row.Cells["Commentaire"].Value?.ToString() ?? "";
            else
                txtCommentaire.Clear();
        }



        private void Reinitialiser()
        {
            selectedObjectifId = -1;
            if (cmbAgent.Items.Count > 0) cmbAgent.SelectedIndex = 0;

            cmbPeriode.SelectedIndex = 1;
            AjusterDatesSelonPeriode();

            txtObjectifNbVentes.Text = "0";
            txtObjectifMontant.Text = "0";
            txtCommentaire.Clear();
            dgvObjectifs.ClearSelection();
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