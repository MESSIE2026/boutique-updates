using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmDashboardBoss : Form
    {
        private readonly string _cs = ConfigSysteme.ConnectionString;

        // UI
        private Label lblTitre;
        private Panel pnlTop;
        private Panel pnlBottom;
        private DateTimePicker dtDebut;
        private DateTimePicker dtFin;
        private Button btnCharger;
        private Button btnDetails;
        private Button btnFermer;
        private DataGridView dgv;
        private Label lblStatus;

        // Data
        private readonly BindingSource _bs = new BindingSource();
        private DataTable _dt;

        public FrmDashboardBoss()
        {
            // Form
            Text = "Dashboard Boss - Montants par Boutique";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1100, 680);
            MinimumSize = new Size(980, 600);
            KeyPreview = true;

            BuildUI();
            WireEvents();
        }

        private void WireEvents()
        {
            Shown += (s, e) =>
            {
                // période par défaut : mois courant
                var now = DateTime.Today;
                dtDebut.Value = new DateTime(now.Year, now.Month, 1);
                dtFin.Value = dtDebut.Value.AddMonths(1).AddDays(-1);

                BeginInvoke(new Action(Charger));
            };

            btnCharger.Click += (s, e) => Charger();
            btnDetails.Click += (s, e) => OuvrirDetails();
            btnFermer.Click += (s, e) => Close();

            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OuvrirDetails(); };
            dgv.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    OuvrirDetails();
                }
            };

            // UX : sélection ligne
            dgv.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;

                dgv.ClearSelection();
                var row = dgv.Rows[e.RowIndex];
                row.Selected = true;

                // ✅ Choisir une colonne VISIBLE
                int colIndex = GetFirstVisibleColumnIndex(dgv);
                if (colIndex >= 0)
                    dgv.CurrentCell = row.Cells[colIndex];
            };

            // raccourcis
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F5)
                {
                    e.SuppressKeyPress = true;
                    Charger();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    e.SuppressKeyPress = true;
                    Close();
                }
            };
        }

        private int GetFirstVisibleColumnIndex(DataGridView grid)
        {
            foreach (DataGridViewColumn c in grid.Columns)
                if (c.Visible) return c.Index;
            return -1;
        }


        private void BuildUI()
        {
            // ===== Title =====
            lblTitre = new Label
            {
                Dock = DockStyle.Top,
                Height = 46,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Padding = new Padding(12, 0, 0, 0),
                Text = "Montants par Boutique (Entreprise / Magasin)"
            };

            // ===== Top Filters =====
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                Padding = new Padding(12, 10, 12, 10)
            };

            var lblD = new Label { AutoSize = true, Text = "Début :", Top = 18, Left = 12 };
            dtDebut = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Width = 125,
                Left = 70,
                Top = 14
            };

            var lblF = new Label { AutoSize = true, Text = "Fin :", Top = 18, Left = 215 };
            dtFin = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Width = 125,
                Left = 250,
                Top = 14
            };

            btnCharger = new Button { Text = "Charger (F5)", Width = 120, Height = 32, Left = 395, Top = 12 };
            btnDetails = new Button { Text = "Détails", Width = 110, Height = 32, Left = 525, Top = 12 };

            lblStatus = new Label
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Left = 650,
                Top = 16,
                Width = 420,
                Height = 28,
                ForeColor = Color.DimGray,
                Text = "Prêt"
            };

            pnlTop.Controls.AddRange(new Control[] { lblD, dtDebut, lblF, dtFin, btnCharger, btnDetails, lblStatus });

            // ===== Grid =====
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoGenerateColumns = true,

                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,

                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                AllowUserToResizeColumns = true,
                AllowUserToResizeRows = false
            };

            // ✅ anti “headers invisibles / style bug”
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.DefaultCellStyle.SelectionBackColor = Color.Goldenrod;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

            // Binding stable
            dgv.DataSource = _bs;

            // ===== Bottom bar =====
            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                Padding = new Padding(12, 10, 12, 10)
            };

            btnFermer = new Button
            {
                Text = "Fermer (Esc)",
                Width = 130,
                Height = 32,
                Left = 12,
                Top = 10
            };

            pnlBottom.Controls.Add(btnFermer);

            // Order add (Fill first, then top/bottom)
            Controls.Add(dgv);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            Controls.Add(lblTitre);
        }

        private bool EstBoss()
        {
            string role = SessionEmploye.Poste ?? "";
            return role.Equals("Directeur Général", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Superviseur", StringComparison.OrdinalIgnoreCase);
        }

        private void SetLoading(bool loading, string message = null)
        {
            btnCharger.Enabled = !loading;
            btnDetails.Enabled = !loading;
            Cursor = loading ? Cursors.WaitCursor : Cursors.Default;

            if (!string.IsNullOrWhiteSpace(message))
                lblStatus.Text = message;

            Application.DoEvents();
        }

        private void Charger()
        {
            DateTime d1 = dtDebut.Value.Date;
            DateTime d2 = dtFin.Value.Date.AddDays(1); // fin exclusive

            bool boss = EstBoss();
            int idMagasinFiltre = AppContext.IdMagasin;

            try
            {
                SetLoading(true, "Chargement...");

                var dt = new DataTable();

                using (var con = new SqlConnection(_cs))
                using (var cmd = new SqlCommand(@"
SELECT 
    ISNULL(e.IdEntreprise, 0) AS IdEntreprise,
    ISNULL(e.Nom, 'NON AFFECTE') AS Entreprise,
    ISNULL(m.IdMagasin, 0) AS IdMagasin,
    ISNULL(m.Nom, 'NON AFFECTE') AS Magasin,
    ISNULL(m.Adresse,'') AS Adresse,
    ISNULL(m.Ville,'') AS Ville,
    SUM(ISNULL(v.MontantTotal,0)) AS MontantTotal
FROM dbo.Vente v
LEFT JOIN dbo.Magasin    m ON m.IdMagasin = v.IdMagasin
LEFT JOIN dbo.Entreprise e ON e.IdEntreprise = v.IdEntreprise
WHERE v.DateVente >= @d1 AND v.DateVente < @d2
  AND ISNULL(v.Statut,'OK') <> 'ANNULE'
  AND (@isBoss = 1 OR v.IdMagasin = @idMag)
GROUP BY 
    ISNULL(e.IdEntreprise, 0), ISNULL(e.Nom, 'NON AFFECTE'),
    ISNULL(m.IdMagasin, 0), ISNULL(m.Nom, 'NON AFFECTE'),
    ISNULL(m.Adresse,''),
    ISNULL(m.Ville,'')
ORDER BY MontantTotal DESC;", con))
                {
                    cmd.Parameters.Add("@d1", SqlDbType.DateTime2).Value = d1;
                    cmd.Parameters.Add("@d2", SqlDbType.DateTime2).Value = d2;
                    cmd.Parameters.Add("@isBoss", SqlDbType.Int).Value = boss ? 1 : 0;
                    cmd.Parameters.Add("@idMag", SqlDbType.Int).Value = idMagasinFiltre;

                    con.Open();
                    using (var da = new SqlDataAdapter(cmd))
                        da.Fill(dt);
                }

                // ✅ Bind solide via BindingSource (anti-grid vide)
                dgv.SuspendLayout();
                _dt = dt;

                _bs.DataSource = null;
                _bs.DataSource = _dt;

                dgv.ResumeLayout(true);
                dgv.Refresh();
                dgv.Invalidate();

                // Format
                if (dgv.Columns["MontantTotal"] != null)
                {
                    dgv.Columns["MontantTotal"].DefaultCellStyle.Format = "N2";
                    dgv.Columns["MontantTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                // Hide IDs
                if (dgv.Columns["IdEntreprise"] != null) dgv.Columns["IdEntreprise"].Visible = false;
                if (dgv.Columns["IdMagasin"] != null) dgv.Columns["IdMagasin"].Visible = false;

                // Total + title
                decimal total = 0m;
                foreach (DataRow r in dt.Rows)
                    total += Convert.ToDecimal(r["MontantTotal"] ?? 0m);

                lblTitre.Text = $"Montants par Boutique | Rows={dt.Rows.Count} | Total={total:N2} | {d1:dd/MM/yyyy}→{d2.AddDays(-1):dd/MM/yyyy}";
                lblStatus.Text = "OK";

                // Select first row
                if (dgv.Rows.Count > 0)
                {
                    dgv.ClearSelection();
                    dgv.Rows[0].Selected = true;
                    if (dgv.Rows.Count > 0)
                    {
                        dgv.ClearSelection();
                        dgv.Rows[0].Selected = true;

                        int colIndex = dgv.Columns["Magasin"] != null && dgv.Columns["Magasin"].Visible
                            ? dgv.Columns["Magasin"].Index
                            : GetFirstVisibleColumnIndex(dgv);

                        if (colIndex >= 0)
                            dgv.CurrentCell = dgv.Rows[0].Cells[colIndex];
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erreur";
                MessageBox.Show("Erreur chargement dashboard : " + ex.Message);
            }
            finally
            {
                SetLoading(false, "Prêt");
            }
        }

        private void OuvrirDetails()
        {
            if (dgv == null || dgv.Rows.Count == 0)
            {
                MessageBox.Show("Aucune donnée à afficher.");
                return;
            }

            DataGridViewRow row =
                (dgv.SelectedRows != null && dgv.SelectedRows.Count > 0)
                    ? dgv.SelectedRows[0]
                    : (dgv.CurrentRow ?? dgv.Rows[0]);

            if (row == null)
            {
                MessageBox.Show("Sélectionne une ligne.");
                return;
            }

            int idEntreprise = 0, idMagasin = 0;
            int.TryParse(Convert.ToString(row.Cells["IdEntreprise"]?.Value), out idEntreprise);
            int.TryParse(Convert.ToString(row.Cells["IdMagasin"]?.Value), out idMagasin);
            string nomMagasin = Convert.ToString(row.Cells["Magasin"]?.Value) ?? "";
            string adresse = Convert.ToString(row.Cells["Adresse"]?.Value) ?? "";

            DateTime d1 = dtDebut.Value.Date;
            DateTime d2 = dtFin.Value.Date.AddDays(1);

            using (var f = new FrmDetailsBoutique(
    ConfigSysteme.ConnectionString,
    idEntreprise,
    idMagasin,
    d1,
    d2,
    nomMagasin,
    adresse
))
            {
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this);
            }
        }
    }
}