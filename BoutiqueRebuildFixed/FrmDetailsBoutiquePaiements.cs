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
    public partial class FrmDetailsBoutiquePaiements : Form
    {
        
            private readonly string _cs = ConfigSysteme.ConnectionString;
            private readonly int _idMagasin;
            private readonly string _devise;
            private readonly DateTime _d1;
            private readonly DateTime _d2;

            private DataGridView dgv;
            private Label lbl;
            private Button btnFermer;

            public FrmDetailsBoutiquePaiements(int idMagasin, string devise, DateTime d1, DateTime d2)
            {
                _idMagasin = idMagasin;
                _devise = devise ?? "CDF";
                _d1 = d1;
                _d2 = d2;

                Text = "Détails Boutique - Paiements";
                StartPosition = FormStartPosition.CenterParent;
                Size = new Size(1100, 600);
                MinimumSize = new Size(980, 520);

                BuildUI();

                Load += (s, e) => Charger();
                btnFermer.Click += (s, e) => Close();
            }

            private void BuildUI()
            {
                lbl = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 44,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Text = "  Paiements de la boutique"
                };
                Controls.Add(lbl);

                dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };
                Controls.Add(dgv);

                var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 54, Padding = new Padding(12, 10, 12, 10) };
                btnFermer = new Button { Text = "Fermer", Width = 120, Height = 32, Left = 12, Top = 10 };
                pnlBottom.Controls.Add(btnFermer);
                Controls.Add(pnlBottom);
            }

            private void Charger()
            {
                try
                {
                    string nomMag = "";
                    using (var con = new SqlConnection(_cs))
                    {
                        con.Open();
                        using (var cmdMag = new SqlCommand("SELECT TOP 1 Nom FROM dbo.Magasin WHERE IdMagasin=@id", con))
                        {
                            cmdMag.Parameters.AddWithValue("@id", _idMagasin);
                            nomMag = Convert.ToString(cmdMag.ExecuteScalar() ?? "");
                        }
                    }

                    lbl.Text = $"  Paiements : {nomMag} | Devise: {_devise} | {_d1:dd/MM/yyyy} - {_d2.AddDays(-1):dd/MM/yyyy}";

                    var dt = new DataTable();

                    using (var con = new SqlConnection(_cs))
                    using (var cmd = new SqlCommand(@"
SELECT 
    pv.DatePaiement,
    pv.IdPaiement,
    pv.IdVente,
    pv.ModePaiement,
    pv.Devise,
    pv.Montant,
    pv.ReferenceTransaction,
    pv.Statut
FROM dbo.PaiementsVente pv
WHERE pv.IdMagasin = @idMag
  AND pv.Devise = @dev
  AND pv.DatePaiement >= @d1 AND pv.DatePaiement < @d2
  AND ISNULL(pv.Statut,'OK') <> 'ANNULE'
ORDER BY pv.DatePaiement DESC;
", con))
                    {
                        cmd.Parameters.AddWithValue("@idMag", _idMagasin);
                        cmd.Parameters.AddWithValue("@dev", _devise);
                        cmd.Parameters.AddWithValue("@d1", _d1);
                        cmd.Parameters.AddWithValue("@d2", _d2);

                        var da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }

                    dgv.DataSource = dt;

                    if (dgv.Columns["Montant"] != null)
                    {
                        dgv.Columns["Montant"].DefaultCellStyle.Format = "N2";
                        dgv.Columns["Montant"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur détails paiements : " + ex.Message);
                }
            }
        }
}
