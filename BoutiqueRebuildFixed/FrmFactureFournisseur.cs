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
    public partial class FrmFactureFournisseur : Form
    {
        private readonly string cs = ConfigSysteme.ConnectionString;

        ComboBox cmbReception;
        TextBox txtNumero;
        DateTimePicker dtFacture, dtEcheance;
        CheckBox chkEcheance;
        Button btnCreer, btnRefresh;
        private Button btnPdf;
        DataGridView dgv;

        private class ReceptionItem
        {
            public int IdReception { get; set; }
            public int IdEntreprise { get; set; }
            public int IdMagasin { get; set; }
            public string Numero { get; set; }
            public override string ToString() => Numero;
        }

        private ReceptionItem ReceptionSel() => cmbReception.SelectedItem as ReceptionItem;

        public FrmFactureFournisseur()
        {
            InitializeComponent();

            Text = "Facture Fournisseur";
            Width = 950;
            Height = 600;

            BuildUI();
            Load += (s, e) =>
            {
                ChargerReceptions();
                ChargerLignesReception();
            };
        }

        private void FrmFactureFournisseur_Load(object sender, EventArgs e)
        {

        }
        void BuildUI()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 115 };
            Controls.Add(top);

            Label L(string t, int l, int tp) => new Label { Left = l, Top = tp, AutoSize = true, Text = t };

            // ===== Ligne 1 =====
            top.Controls.Add(L("Réception", 15, 8));
            cmbReception = new ComboBox
            {
                Left = 15,
                Top = 25,
                Width = 320,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            top.Controls.Add(L("N° Facture", 350, 8));
            txtNumero = new TextBox { Left = 350, Top = 25, Width = 180 };
            txtNumero.Text = "FAC-" + DateTime.Now.ToString("yyyyMMdd-HHmm");

            top.Controls.Add(L("Date facture", 545, 8));
            dtFacture = new DateTimePicker
            {
                Left = 545,
                Top = 25,
                Width = 120,
                Format = DateTimePickerFormat.Short
            };

            chkEcheance = new CheckBox
            {
                Left = 680,
                Top = 27,
                Width = 90,
                Text = "Échéance"
            };

            top.Controls.Add(L("Date échéance", 770, 8));
            dtEcheance = new DateTimePicker
            {
                Left = 770,
                Top = 25,
                Width = 120,
                Format = DateTimePickerFormat.Short,
                Enabled = false
            };

            // ===== Ligne 2 =====
            btnCreer = new Button { Left = 15, Top = 65, Width = 260, Height = 28, Text = "Créer facture depuis réception" };
            btnRefresh = new Button { Left = 285, Top = 65, Width = 120, Height = 28, Text = "Rafraîchir" };
            btnPdf = new Button { Left = 410, Top = 65, Width = 200, Height = 28, Text = "Exporter PDF Facture" };
            var btnVoirBoutique = new Button { Left = 620, Top = 65, Width = 150, Height = 28, Text = "Voir Boutique" };
            top.Controls.Add(btnVoirBoutique);
            btnVoirBoutique.Click += (s, e) => OuvrirBoutiqueDepuisReception();

            top.Controls.AddRange(new Control[]
            {
                cmbReception, txtNumero, dtFacture, chkEcheance, dtEcheance,
                btnCreer, btnRefresh, btnPdf
            });

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(dgv);

            // Events
            chkEcheance.CheckedChanged += (s, e) => dtEcheance.Enabled = chkEcheance.Checked;
            btnCreer.Click += (s, e) => CreerFacture();
            btnRefresh.Click += (s, e) => { ChargerReceptions(); ChargerLignesReception(); };
            btnPdf.Click += (s, e) => ExporterPdfFacture();
            cmbReception.SelectedIndexChanged += (s, e) => ChargerLignesReception();
        }

        private void OuvrirBoutiqueDepuisReception()
        {
            var it = ReceptionSel();
            if (it == null)
            {
                MessageBox.Show("Sélectionne une réception.");
                return;
            }

            // période par défaut : mois courant
            DateTime d1 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime d2 = d1.AddMonths(1); // fin exclusive

            // Récupérer Nom + Adresse magasin
            string nomMagasin = "";
            string adresse = "";

            if (it.IdMagasin > 0)
            {
                try
                {
                    using (var con = new SqlConnection(cs))
                    using (var cmd = new SqlCommand(@"
SELECT TOP 1 Nom, Adresse
FROM dbo.Magasin
WHERE IdMagasin = @m;", con))
                    {
                        cmd.Parameters.Add("@m", SqlDbType.Int).Value = it.IdMagasin;
                        con.Open();

                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                nomMagasin = rd["Nom"]?.ToString() ?? "";
                                adresse = rd["Adresse"]?.ToString() ?? "";
                            }
                        }
                    }
                }
                catch
                {
                    // si erreur, on laisse vide (ça n’empêche pas l’ouverture)
                }
            }

            using (var f = new FrmDetailsBoutique(cs, it.IdEntreprise, it.IdMagasin, d1, d2, nomMagasin, adresse))
            {
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this);
            }
        }



        void ChargerReceptions()
        {
            cmbReception.Items.Clear();

            // ⚠️ ta table Reception doit avoir IdEntreprise, IdMagasin
            var dt = DbHelper.Table(cs,
        @"SELECT ID_Reception, NumeroReception, IdEntreprise, IdMagasin
  FROM Reception
  ORDER BY ID_Reception DESC");

            foreach (DataRow r in dt.Rows)
            {
                int idReception = Convert.ToInt32(r["ID_Reception"]);
                string numero = Convert.ToString(r["NumeroReception"] ?? "").Trim();

                int idEntreprise = (r["IdEntreprise"] == DBNull.Value) ? 0 : Convert.ToInt32(r["IdEntreprise"]);
                int idMagasin = (r["IdMagasin"] == DBNull.Value) ? 0 : Convert.ToInt32(r["IdMagasin"]);

                cmbReception.Items.Add(new ReceptionItem
                {
                    IdReception = idReception,
                    Numero = numero,
                    IdEntreprise = idEntreprise,
                    IdMagasin = idMagasin
                });
            }

            if (cmbReception.Items.Count > 0) cmbReception.SelectedIndex = 0;
        }

        int? ReceptionId() => ReceptionSel()?.IdReception;
        void ChargerLignesReception()
        {
            var r = ReceptionId();
            if (r == null) { dgv.DataSource = null; return; }

            var dt = DbHelper.Table(cs, @"
SELECT
    p.NomProduit,
    rl.QteRecueBase,
    rl.PrixAchat,
    rl.Devise,
    rl.LotNumero,
    rl.DateExpiration
FROM ReceptionLigne rl
JOIN Produit p ON p.ID_Produit=rl.ID_Produit
WHERE rl.ID_Reception=@r",
    new SqlParameter("@r", SqlDbType.Int) { Value = r.Value });

            dgv.DataSource = dt;
        }

        private decimal LireDecimalFR(object value)
        {
            string s = Convert.ToString(value ?? "0").Trim();

            // supprime espaces + NBSP
            s = s.Replace("\u00A0", "").Replace(" ", "");

            // Essai fr-FR
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out var v))
                return v;

            // Secours : 12.5 => 12,5
            s = s.Replace(".", ",");
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out v))
                return v;

            // Secours invariant (rare)
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v))
                return v;

            return 0m;
        }

        private void ExporterPdfFacture()
        {
            var r = ReceptionId();
            if (r == null || dgv.DataSource == null)
            {
                MessageBox.Show("Sélectionne une réception.");
                return;
            }

            string path = PdfExportHelper.AskSavePdfPath($"Facture_{txtNumero.Text}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            if (path == null) return;

            var dt = PdfExportHelper.DataGridViewToDataTable(dgv);

            // ✅ Totaux par devise (important si lignes mix USD/CDF)
            var totDevise = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                decimal q = LireDecimalFR(row.Cells["QteRecueBase"]?.Value);
                decimal p = LireDecimalFR(row.Cells["PrixAchat"]?.Value);

                string dev = Convert.ToString(row.Cells["Devise"]?.Value ?? "N/A").Trim();
                if (string.IsNullOrWhiteSpace(dev)) dev = "N/A";

                decimal sousTotal = q * p;

                if (!totDevise.ContainsKey(dev)) totDevise[dev] = 0m;
                totDevise[dev] += sousTotal;
            }

            // ✅ Texte total propre (si multi-devises on affiche chaque devise)
            string totalTxt;
            if (totDevise.Count == 1)
            {
                var kv = totDevise.First();
                totalTxt = $"{kv.Value.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"))} {kv.Key}";
            }
            else
            {
                totalTxt = string.Join(" | ",
                    totDevise.OrderBy(x => x.Key)
                             .Select(x => x.Value.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")) + " " + x.Key));
            }

            PdfExportHelper.ExportDataTableToPdf(
                path,
                "FACTURE FOURNISSEUR",
                dt,
                $"Réception: {cmbReception.Text} | N° Facture: {txtNumero.Text} | Date: {dtFacture.Value:yyyy-MM-dd} | Total: {totalTxt}"
            );

            MessageBox.Show("PDF Facture généré ✅");
        }

        void CreerFacture()
        {
            var r = ReceptionId();
            if (r == null) { MessageBox.Show("Sélectionne une réception."); return; }

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.sp_Facture_CreateFromReception", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@NumeroFacture", (txtNumero.Text ?? "").Trim());
                cmd.Parameters.AddWithValue("@ID_Reception", r.Value);
                cmd.Parameters.AddWithValue("@DateFacture", dtFacture.Value.Date);
                cmd.Parameters.AddWithValue("@DateEcheance", chkEcheance.Checked ? (object)dtEcheance.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@CreePar", (SessionEmploye.Nom + " " + SessionEmploye.Prenom).Trim());

                con.Open();
                int idFacture = Convert.ToInt32(cmd.ExecuteScalar());
                MessageBox.Show("Facture créée ✅ ID=" + idFacture);
            }
        }
    }
}