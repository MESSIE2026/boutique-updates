using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmPaiementsFournisseur : Form
    {
        private readonly string cs = ConfigSysteme.ConnectionString;

        DataGridView dgv;
        Button btnRefresh, btnPayer;
        Button btnVoirBoutique; // ✅ AJOUTE ÇA
        NumericUpDown nudMontant;
        ComboBox cmbDevise, cmbMode;
        TextBox txtRef;
        DateTimePicker dtPay;
        private Button btnPdfDettes, btnPdfPaiements;

        // Colonnes attendues dans dgv (retournées par sp_GetDettesFournisseurs)
        private const string COL_ID_FACTURE = "ID_Facture";
        private const string COL_ID_ENTREPRISE = "IdEntreprise";
        private const string COL_ID_MAGASIN = "IdMagasin";
        private const string COL_NOM_MAGASIN = "NomMagasin";
        private const string COL_RESTE = "Reste";
        private const string COL_DEVISE_FACTURE = "DeviseFacture"; // ou "Devise"

        public FrmPaiementsFournisseur()
        {
            InitializeComponent();

            Text = "Paiements Fournisseurs (dettes)";
            Width = 1100;
            Height = 650;

            BuildUI();
            Load += (s, e) => RefreshDettes();
        }

        void BuildUI()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 120 };
            Controls.Add(top);

            Label L(string t, int l, int tp) => new Label { Left = l, Top = tp, AutoSize = true, Text = t };

            btnRefresh = new Button { Left = 15, Top = 25, Width = 120, Height = 28, Text = "Rafraîchir" };

            top.Controls.Add(L("Montant", 150, 8));
            nudMontant = new NumericUpDown
            {
                Left = 150,
                Top = 25,
                Width = 130,
                DecimalPlaces = 2,
                Maximum = 999999999,
                Minimum = 0,
                Value = 0
            };

            top.Controls.Add(L("Devise", 290, 8));
            cmbDevise = new ComboBox { Left = 290, Top = 25, Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDevise.Items.AddRange(new object[] { "CDF", "USD" });
            cmbDevise.SelectedIndex = 0;

            top.Controls.Add(L("Mode paiement", 380, 8));
            cmbMode = new ComboBox { Left = 380, Top = 25, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMode.Items.AddRange(new object[] { "Cash", "MobileMoney", "Banque", "Autre" });
            cmbMode.SelectedIndex = 0;

            top.Controls.Add(L("Référence", 530, 8));
            txtRef = new TextBox { Left = 530, Top = 25, Width = 160 };

            top.Controls.Add(L("Date paiement", 700, 8));
            dtPay = new DateTimePicker { Left = 700, Top = 25, Width = 120, Format = DateTimePickerFormat.Short };

            btnPayer = new Button { Left = 830, Top = 23, Width = 240, Height = 28, Text = "Enregistrer paiement (facture)" };

            btnPdfDettes = new Button { Left = 15, Top = 70, Width = 150, Height = 28, Text = "PDF Dettes" };
            btnPdfPaiements = new Button { Left = 175, Top = 70, Width = 240, Height = 28, Text = "PDF Paiements (facture)" };
            btnVoirBoutique = new Button { Left = 425, Top = 70, Width = 170, Height = 28, Text = "Voir Boutique" };
            top.Controls.Add(btnVoirBoutique);

            top.Controls.AddRange(new Control[]
            {
                btnRefresh, nudMontant, cmbDevise, cmbMode, txtRef, dtPay, btnPayer,
                btnPdfDettes, btnPdfPaiements, btnVoirBoutique,
            });

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(dgv);

            btnRefresh.Click += (s, e) => RefreshDettes();
            btnPayer.Click += (s, e) => PayerSelection();
            btnPdfDettes.Click += (s, e) => ExporterPdfDettes();
            btnPdfPaiements.Click += (s, e) => ExporterPdfPaiementsFacture();
            btnVoirBoutique.Click += (s, e) => OuvrirDetailsBoutiqueDepuisLigne();

            // Double-clic sur une ligne = ouvrir fiche paiements
            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0) ExporterPdfPaiementsFacture();
            };

            // Quand tu changes de facture sélectionnée => forcer devise + max montant
            dgv.SelectionChanged += (s, e) => SyncUiSelonFactureSelectionnee();
        }

        void RefreshDettes()
        {
            var dt = DbHelper.TableSp(cs, "dbo.sp_GetDettesFournisseurs");
            dgv.DataSource = dt;

            // Pro : cacher colonnes techniques si elles existent
            if (dgv.Columns.Contains(COL_ID_ENTREPRISE)) dgv.Columns[COL_ID_ENTREPRISE].Visible = false;
            if (dgv.Columns.Contains(COL_ID_MAGASIN)) dgv.Columns[COL_ID_MAGASIN].Visible = false;

            SyncUiSelonFactureSelectionnee();
        }

        private void SyncUiSelonFactureSelectionnee()
        {
            if (dgv.CurrentRow == null) return;

            // 1) Forcer devise selon facture
            string dev = null;

            if (dgv.Columns.Contains(COL_DEVISE_FACTURE))
                dev = Convert.ToString(dgv.CurrentRow.Cells[COL_DEVISE_FACTURE].Value)?.Trim();

            // fallback si ta colonne s'appelle "Devise"
            if (string.IsNullOrWhiteSpace(dev) && dgv.Columns.Contains("Devise"))
                dev = Convert.ToString(dgv.CurrentRow.Cells["Devise"].Value)?.Trim();

            if (!string.IsNullOrWhiteSpace(dev))
            {
                int idx = cmbDevise.FindStringExact(dev);
                if (idx >= 0) cmbDevise.SelectedIndex = idx;
                else
                {
                    // si devise inconnue, on laisse mais on avertit
                    // MessageBox.Show("Devise facture inconnue : " + dev);
                }
            }

            // 2) Limiter montant au reste
            if (dgv.Columns.Contains(COL_RESTE))
            {
                decimal reste = 0m;
                try { reste = Convert.ToDecimal(dgv.CurrentRow.Cells[COL_RESTE].Value); } catch { reste = 0m; }
                if (reste < 0) reste = 0;

                nudMontant.Maximum = (decimal)Math.Min((double)reste, 999999999);
                if (nudMontant.Value > nudMontant.Maximum) nudMontant.Value = nudMontant.Maximum;
            }
            else
            {
                // si tu n'as pas la colonne Reste, on ne peut pas limiter proprement
                nudMontant.Maximum = 999999999;
            }
        }

        private void OuvrirDetailsBoutiqueDepuisLigne()
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Sélectionne une facture.");
                return;
            }

            if (!dgv.Columns.Contains(COL_ID_ENTREPRISE) || !dgv.Columns.Contains(COL_ID_MAGASIN))
            {
                MessageBox.Show("Le SP doit retourner IdEntreprise et IdMagasin.");
                return;
            }

            object vEnt = dgv.CurrentRow.Cells[COL_ID_ENTREPRISE].Value;
            object vMag = dgv.CurrentRow.Cells[COL_ID_MAGASIN].Value;

            int idEnt = (vEnt == null || vEnt == DBNull.Value) ? 0 : Convert.ToInt32(vEnt);
            int idMag = (vMag == null || vMag == DBNull.Value) ? 0 : Convert.ToInt32(vMag);

            if (idEnt <= 0 || idMag <= 0)
            {
                MessageBox.Show("Cette facture est NON AFFECTÉE (IdEntreprise/IdMagasin manquant).");
                return;
            }

            // période par défaut : mois courant
            DateTime d1 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime d2 = d1.AddMonths(1); // fin exclusive

            // Récupérer Nom + Adresse magasin
            string nomMagasin = "";
            string adresse = "";

            try
            {
                using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 Nom, Adresse
FROM dbo.Magasin
WHERE IdMagasin = @m;", con))
                {
                    cmd.Parameters.Add("@m", SqlDbType.Int).Value = idMag;
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
                // si erreur, on laisse vide
            }

            using (var f = new FrmDetailsBoutique(ConfigSysteme.ConnectionString, idEnt, idMag, d1, d2, nomMagasin, adresse))
            {
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this);
            }
        }




        private void ExporterPdfDettes()
        {
            if (dgv.DataSource == null) { MessageBox.Show("Aucune dette à exporter."); return; }

            string path = PdfExportHelper.AskSavePdfPath($"Dettes_Fournisseurs_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            if (path == null) return;

            var dt = PdfExportHelper.DataGridViewToDataTable(dgv);
            PdfExportHelper.ExportDataTableToPdf(path, "DETTES FOURNISSEURS", dt);
            MessageBox.Show("PDF Dettes généré ✅");
        }

        private void ExporterPdfPaiementsFacture()
        {
            if (dgv.CurrentRow == null || !dgv.Columns.Contains("ID_Facture"))
            {
                MessageBox.Show("Sélectionne une facture.");
                return;
            }

            int idFacture = Convert.ToInt32(dgv.CurrentRow.Cells["ID_Facture"].Value);

            string path = PdfExportHelper.AskSavePdfPath($"Paiements_Facture_{idFacture}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            if (path == null) return;

            // ✅ TEXT
            var dt = DbHelper.Table(cs, @"
SELECT DatePaiement, Montant, Devise, ModePaiement, Reference, CreePar, DateCreation
FROM PaiementFournisseur
WHERE ID_Facture=@f
ORDER BY DatePaiement DESC",
                new SqlParameter("@f", SqlDbType.Int) { Value = idFacture });

            PdfExportHelper.ExportDataTableToPdf(path, "PAIEMENTS FACTURE FOURNISSEUR", dt, $"Facture ID: {idFacture}");
            MessageBox.Show("PDF Paiements généré ✅");
        }

        void PayerSelection()
        {
            if (dgv.CurrentRow == null) { MessageBox.Show("Sélectionne une facture."); return; }
            if (!dgv.Columns.Contains("ID_Facture")) { MessageBox.Show("Colonne ID_Facture manquante."); return; }

            int idFacture = Convert.ToInt32(dgv.CurrentRow.Cells["ID_Facture"].Value);

            decimal montant = nudMontant.Value;
            // ✅ Bloquer paiement > reste (si colonne Reste dispo)
            if (dgv.Columns.Contains(COL_RESTE))
            {
                decimal reste = 0m;
                try { reste = Convert.ToDecimal(dgv.CurrentRow.Cells[COL_RESTE].Value); } catch { reste = 0m; }

                if (montant > reste)
                {
                    MessageBox.Show($"Montant trop grand. Reste = {reste:N2}");
                    return;
                }
            }
            if (montant <= 0) { MessageBox.Show("Montant invalide."); return; }

            try
            {
                string user = (SessionEmploye.Nom + " " + SessionEmploye.Prenom).Trim();

                // ✅ SP
                DbHelper.ExecSp(cs, "dbo.sp_Paiement_Add",
                    new SqlParameter("@ID_Facture", SqlDbType.Int) { Value = idFacture },
                    new SqlParameter("@DatePaiement", SqlDbType.Date) { Value = dtPay.Value.Date },
                    new SqlParameter("@Montant", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = montant },
                    new SqlParameter("@Devise", SqlDbType.NVarChar, 10) { Value = cmbDevise.SelectedItem?.ToString() ?? "CDF" },
                    new SqlParameter("@ModePaiement", SqlDbType.NVarChar, 40) { Value = cmbMode.SelectedItem?.ToString() ?? "Cash" },
                    new SqlParameter("@Reference", SqlDbType.NVarChar, 200) { Value = string.IsNullOrWhiteSpace(txtRef.Text) ? (object)DBNull.Value : txtRef.Text.Trim() },
                    new SqlParameter("@CreePar", SqlDbType.NVarChar, 200) { Value = user }
                );

                MessageBox.Show("Paiement enregistré ✅");
                nudMontant.Value = 0;
                txtRef.Text = "";
                RefreshDettes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur paiement : " + ex.Message);
            }
        }
    }
}