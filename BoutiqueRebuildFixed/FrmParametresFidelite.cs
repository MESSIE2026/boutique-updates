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
    public partial class FrmParametresFidelite : Form
    {
        private readonly string _cs;

        private NumericUpDown nudFenetreJours;
        private NumericUpDown nudMinVentesPromo;
        private NumericUpDown nudMinTotalPromo;
        private NumericUpDown nudMinVentesRetro;
        private NumericUpDown nudMinTotalRetro;

        private Label lblInfo;
        private Button btnSave;
        private Button btnCancel;

        public FrmParametresFidelite(string connectionString)
        {
            _cs = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            BuildUI();

            Shown += (s, e) => LoadRegles();
        }

        private void BuildUI()
        {
            Text = "Paramètres Fidélité (Responsable)";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            Font = new Font("Segoe UI", 10f);
            Width = 560;
            Height = 420;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 2,
                RowCount = 9,
                AutoSize = false
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));

            lblInfo = new Label
            {
                Text = "Règles utilisées pour basculer OCCASIONNEL ↔ FIDELE.\nVIP/ENTREPRISE restent manuels.",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Dock = DockStyle.Fill
            };

            nudFenetreJours = MakeNud(1, 3650, 90);
            nudMinVentesPromo = MakeNud(0, 100000, 10);
            nudMinTotalPromo = MakeMoneyNud(0, 1000000000, 200);
            nudMinVentesRetro = MakeNud(0, 100000, 3);
            nudMinTotalRetro = MakeMoneyNud(0, 1000000000, 50);

            btnSave = new Button { Text = "Enregistrer", Width = 140, Height = 36 };
            btnCancel = new Button { Text = "Annuler", Width = 120, Height = 36 };

            btnSave.Click += (s, e) => SaveRegles();
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            // Rows
            root.Controls.Add(lblInfo, 0, 0);
            root.SetColumnSpan(lblInfo, 2);

            AddRow(root, 1, "Fenêtre (jours) :", nudFenetreJours);
            AddRow(root, 2, "Devenir FIDELE : Min ventes :", nudMinVentesPromo);
            AddRow(root, 3, "Devenir FIDELE : Min total achats :", nudMinTotalPromo);
            AddRow(root, 4, "Rester FIDELE : Min ventes :", nudMinVentesRetro);
            AddRow(root, 5, "Rester FIDELE : Min total achats :", nudMinTotalRetro);

            // Buttons panel
            var pnlBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };
            pnlBtns.Controls.Add(btnCancel);
            pnlBtns.Controls.Add(btnSave);

            root.Controls.Add(new Label { AutoSize = true }, 0, 6);
            root.Controls.Add(new Label { AutoSize = true }, 1, 6);
            root.Controls.Add(pnlBtns, 0, 7);
            root.SetColumnSpan(pnlBtns, 2);

            Controls.Add(root);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private NumericUpDown MakeNud(int min, int max, int value)
        {
            return new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Value = value,
                Dock = DockStyle.Fill
            };
        }

        private NumericUpDown MakeMoneyNud(decimal min, decimal max, decimal value)
        {
            return new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                DecimalPlaces = 2,
                Increment = 1,
                ThousandsSeparator = true,
                Value = value,
                Dock = DockStyle.Fill
            };
        }

        private void AddRow(TableLayoutPanel p, int row, string label, Control input)
        {
            p.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
            p.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, row);
            p.Controls.Add(input, 1, row);
        }

        private void LoadRegles()
        {
            try
            {
                using (var con = new SqlConnection(_cs))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("dbo.LoyaltyRegles_Get", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) throw new Exception("Aucune règle retournée.");

                            nudFenetreJours.Value = Convert.ToInt32(r["FenetreJours"]);
                            nudMinVentesPromo.Value = Convert.ToInt32(r["MinNbVentes_Promo"]);
                            nudMinTotalPromo.Value = Convert.ToDecimal(r["MinTotal_Promo"], CultureInfo.InvariantCulture);
                            nudMinVentesRetro.Value = Convert.ToInt32(r["MinNbVentes_Retro"]);
                            nudMinTotalRetro.Value = Convert.ToDecimal(r["MinTotal_Retro"], CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement règles : " + ex.Message);
            }
        }

        private void SaveRegles()
        {
            try
            {
                using (var con = new SqlConnection(_cs))
                {
                    con.Open();
                    using (var trans = con.BeginTransaction())
                    using (var cmd = new SqlCommand("dbo.LoyaltyRegles_Save", con, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@FenetreJours", SqlDbType.Int).Value = (int)nudFenetreJours.Value;
                        cmd.Parameters.Add("@MinNbVentes_Promo", SqlDbType.Int).Value = (int)nudMinVentesPromo.Value;

                        var p1 = cmd.Parameters.Add("@MinTotal_Promo", SqlDbType.Decimal);
                        p1.Precision = 18; p1.Scale = 2; p1.Value = nudMinTotalPromo.Value;

                        cmd.Parameters.Add("@MinNbVentes_Retro", SqlDbType.Int).Value = (int)nudMinVentesRetro.Value;

                        var p2 = cmd.Parameters.Add("@MinTotal_Retro", SqlDbType.Decimal);
                        p2.Precision = 18; p2.Scale = 2; p2.Value = nudMinTotalRetro.Value;

                        cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }

                MessageBox.Show("✅ Règles enregistrées.");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur enregistrement règles : " + ex.Message);
            }
        }
    }
}
