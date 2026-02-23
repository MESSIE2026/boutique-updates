using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmTauxChange : Form
    {
        private readonly string _cs;

        private ComboBox cboFrom, cboTo;
        private TextBox txtTaux, txtNote;
        private Button btnSave, btnRefresh, btnClose;
        private DataGridView dgv;

        // ✅ Labels UI “1 USD = 2300 CDF”
        private Label lblFromDev;
        private Label lblToDev;
        private Label lblInverse;

        public FrmTauxChange(string connectionString)
        {
            _cs = connectionString;
            BuildUI();
            Load += (s, e) => RefreshGrid();
        }

        private void BuildUI()
        {
            Text = "Paramètres - Taux de change (simple)";
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10f);
            Width = 900;
            Height = 560;
            MinimizeBox = false;
            MaximizeBox = false;

            var lblFrom = new Label { Text = "Devise Base", Left = 20, Top = 20, AutoSize = true };
            cboFrom = new ComboBox { Left = 20, Top = 45, Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblTo = new Label { Text = "Devise Cible", Left = 200, Top = 20, AutoSize = true };
            cboTo = new ComboBox { Left = 200, Top = 45, Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };

            // ✅ UI : 1 [From] = [txtTaux] [To]
            var lblTaux = new Label { Text = "Taux simple", Left = 380, Top = 20, AutoSize = true };

            var lblOne = new Label { Text = "1", Left = 380, Top = 49, AutoSize = true };
            lblFromDev = new Label { Text = "USD", Left = 400, Top = 49, AutoSize = true };
            var lblEq = new Label { Text = "=", Left = 445, Top = 49, AutoSize = true };

            txtTaux = new TextBox { Left = 465, Top = 45, Width = 120 }; // ex: 2300
            lblToDev = new Label { Text = "CDF", Left = 595, Top = 49, AutoSize = true };

            lblInverse = new Label
            {
                Text = "",
                Left = 380,
                Top = 75,
                AutoSize = true,
                ForeColor = Color.DimGray
            };

            var lblNote = new Label { Text = "Note (optionnel)", Left = 560, Top = 20, AutoSize = true };
            txtNote = new TextBox { Left = 560, Top = 45, Width = 300 };

            btnSave = new Button { Text = "Enregistrer / Activer", Left = 20, Top = 90, Width = 200, Height = 34 };
            btnRefresh = new Button { Text = "Rafraîchir", Left = 230, Top = 90, Width = 120, Height = 34 };
            btnClose = new Button { Text = "Fermer", Left = 360, Top = 90, Width = 120, Height = 34 };

            dgv = new DataGridView
            {
                Left = 20,
                Top = 140,
                Width = 840,
                Height = 360,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            cboFrom.Items.AddRange(new object[] { "CDF", "USD", "EUR" });
            cboTo.Items.AddRange(new object[] { "CDF", "USD", "EUR" });
            cboFrom.SelectedItem = "USD";
            cboTo.SelectedItem = "CDF";

            // ✅ Events : mise à jour labels + inverse
            cboFrom.SelectedIndexChanged += (s, e) => UpdateRateLabels();
            cboTo.SelectedIndexChanged += (s, e) => UpdateRateLabels();
            txtTaux.TextChanged += (s, e) => UpdateInverseLabel();

            // ✅ Optionnel mais recommandé : éviter les lettres dans le taux
            txtTaux.KeyPress += (s, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;               // backspace etc.
                if (char.IsDigit(e.KeyChar)) return;                 // chiffres
                if (e.KeyChar == '.' || e.KeyChar == ',') return;    // séparateur
                if (e.KeyChar == ' ') return;                        // espace (ex: 2 300)
                e.Handled = true;
            };

            btnSave.Click += (s, e) => SaveRate();
            btnRefresh.Click += (s, e) => RefreshGrid();
            btnClose.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                lblFrom, cboFrom, lblTo, cboTo,
                lblTaux, lblOne, lblFromDev, lblEq, txtTaux, lblToDev, lblInverse,
                lblNote, txtNote,
                btnSave, btnRefresh, btnClose,
                dgv
            });

            UpdateRateLabels();
        }

        private void UpdateRateLabels()
        {
            lblFromDev.Text = (cboFrom.Text ?? "").Trim().ToUpperInvariant();
            lblToDev.Text = (cboTo.Text ?? "").Trim().ToUpperInvariant();
            UpdateInverseLabel();
        }

        private void UpdateInverseLabel()
        {
            decimal t = ParseDecimal(txtTaux.Text);
            string from = (cboFrom.Text ?? "").Trim().ToUpperInvariant();
            string to = (cboTo.Text ?? "").Trim().ToUpperInvariant();

            if (t > 0m && !string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                decimal inv = 1m / t;

                // ✅ inv avec plus de décimales pour éviter afficher "0"
                lblInverse.Text =
                    $"Donc 1 {from} = {t:0.########} {to}   |   1 {to} = {inv:0.##############} {from}";
            }
            else
            {
                lblInverse.Text = "";
            }
        }

        private decimal ParseDecimal(string s)
        {
            s = (s ?? "").Trim();

            // Autoriser "2 300" en supprimant les espaces
            s = s.Replace(" ", "");

            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)) return v;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out v)) return v;

            return 0m;
        }

        private void SaveRate()
        {
            string from = (cboFrom.Text ?? "").Trim().ToUpperInvariant();
            string to = (cboTo.Text ?? "").Trim().ToUpperInvariant();
            decimal taux = ParseDecimal(txtTaux.Text);
            string note = (txtNote.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            {
                MessageBox.Show("Veuillez choisir les deux devises.");
                return;
            }

            if (from == to)
            {
                MessageBox.Show("Devise Base et Cible ne peuvent pas être identiques.");
                return;
            }

            if (taux <= 0m)
            {
                MessageBox.Show("Taux invalide. Exemple: 1 USD = 2300 CDF");
                txtTaux.Focus();
                txtTaux.SelectAll();
                return;
            }

            try
            {
                using (var con = new SqlConnection(_cs))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("dbo.TauxChange_SetActif", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@DeviseFrom", SqlDbType.NVarChar, 10).Value = from;
                        cmd.Parameters.Add("@DeviseTo", SqlDbType.NVarChar, 10).Value = to;

                        var p = cmd.Parameters.Add("@Taux", SqlDbType.Decimal);
                        p.Precision = 18;
                        p.Scale = 8;
                        p.Value = taux;

                        cmd.Parameters.Add("@Note", SqlDbType.NVarChar, 200).Value =
                            string.IsNullOrWhiteSpace(note) ? (object)DBNull.Value : note;

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"✅ Taux enregistré : 1 {from} = {taux} {to}");
                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur enregistrement taux: " + ex.Message);
            }
        }

        private void RefreshGrid()
        {
            try
            {
                using (var con = new SqlConnection(_cs))
                {
                    con.Open();
                    using (var da = new SqlDataAdapter(@"
SELECT TOP 200
    DateEffet,
    DeviseFrom,
    DeviseTo,
    Taux,
    Actif,
    ISNULL(Note,'') AS Note
FROM dbo.TauxChange
ORDER BY DateEffet DESC;", con))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        dgv.DataSource = dt;
                    }
                }

                // ✅ Format colonne après bind (si elles existent)
                if (dgv.Columns["DateEffet"] != null)
                    dgv.Columns["DateEffet"].DefaultCellStyle.Format = "g";

                if (dgv.Columns["Taux"] != null)
                    dgv.Columns["Taux"].DefaultCellStyle.Format = "N8";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement taux: " + ex.Message);
            }
        }
    }
}