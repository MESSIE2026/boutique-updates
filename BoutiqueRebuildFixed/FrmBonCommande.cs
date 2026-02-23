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
    public partial class FrmBonCommande : FormBase
    {
        private readonly string cs = ConfigSysteme.ConnectionString;

        ComboBox cmbFournisseur, cmbProduit, cmbDevise;
        TextBox txtNumero;
        DateTimePicker dtDate;
        NumericUpDown nudQte, nudPrix;
        Button btnCreer, btnAjouterLigne, btnEnvoyer, btnRefresh;
        DataGridView dgv;
        private Button btnPdf;

        int _idBC = 0;
        private bool _loadedOnce = false;
        ComboBox cmbMagasin;

        public FrmBonCommande()
        {
            InitializeComponent();

            Text = "Bon de Commande (BC)";
            Width = 1100;
            Height = 650;

            BuildUI();

            // ✅ Sécurise : éviter double Load si Designer + code
            Load -= FrmBonCommande_Load;
            Load += FrmBonCommande_Load;
        }

        private void FrmBonCommande_Load(object sender, EventArgs e)
        {
            if (_loadedOnce) return;
            _loadedOnce = true;

            ChargerFournisseurs();
            ChargerMagasins();
            ChargerProduits();

            if (cmbDevise.Items.Count > 0 && cmbDevise.SelectedIndex < 0)
                cmbDevise.SelectedIndex = 0;
        }

        void BuildUI()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 140 };
            Controls.Add(top);

            Label L(string t, int l, int tp) => new Label { Left = l, Top = tp, AutoSize = true, Text = t };

            // ===================== LIGNE 1 =====================
            top.Controls.Add(L("Fournisseur", 15, 8));
            cmbFournisseur = new ComboBox { Left = 15, Top = 25, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
            top.Controls.Add(cmbFournisseur);

            top.Controls.Add(L("Magasin", 285, 8));
            cmbMagasin = new ComboBox { Left = 285, Top = 25, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            top.Controls.Add(cmbMagasin);

            top.Controls.Add(L("N° Bon de commande", 515, 8));
            txtNumero = new TextBox { Left = 515, Top = 25, Width = 180 };
            txtNumero.Text = "BC-" + DateTime.Now.ToString("yyyyMMdd-HHmm");
            top.Controls.Add(txtNumero);

            top.Controls.Add(L("Date BC", 705, 8));
            dtDate = new DateTimePicker { Left = 705, Top = 25, Width = 130, Format = DateTimePickerFormat.Short };
            top.Controls.Add(dtDate);

            btnCreer = new Button { Left = 845, Top = 23, Width = 120, Height = 28, Text = "Créer BC" };
            btnEnvoyer = new Button { Left = 970, Top = 23, Width = 120, Height = 28, Text = "Statut: ENVOYE" };
            btnRefresh = new Button { Left = 845, Top = 55, Width = 245, Height = 28, Text = "Rafraîchir" };

            top.Controls.AddRange(new Control[] { btnCreer, btnEnvoyer, btnRefresh });

            // ===================== LIGNE 2 =====================
            top.Controls.Add(L("Produit", 15, 70));
            cmbProduit = new ComboBox { Left = 15, Top = 87, Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
            top.Controls.Add(cmbProduit);

            top.Controls.Add(L("Quantité", 365, 70));
            nudQte = new NumericUpDown { Left = 365, Top = 87, Width = 80, Minimum = 1, Maximum = 999999, Value = 1 };
            top.Controls.Add(nudQte);

            top.Controls.Add(L("Prix achat", 455, 70));
            nudPrix = new NumericUpDown { Left = 455, Top = 87, Width = 120, Minimum = 0, Maximum = 999999999, DecimalPlaces = 2, Value = 0 };
            top.Controls.Add(nudPrix);

            top.Controls.Add(L("Devise", 585, 70));
            cmbDevise = new ComboBox { Left = 585, Top = 87, Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDevise.Items.AddRange(new object[] { "CDF", "USD" });
            cmbDevise.SelectedIndex = 0;
            top.Controls.Add(cmbDevise);

            btnAjouterLigne = new Button { Left = 675, Top = 85, Width = 160, Height = 28, Text = "Ajouter ligne" };
            btnPdf = new Button { Left = 845, Top = 85, Width = 245, Height = 28, Text = "Exporter PDF BC" };
            top.Controls.AddRange(new Control[] { btnAjouterLigne, btnPdf });

            // ===================== GRID =====================
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };
            Controls.Add(dgv);

            // Events
            btnCreer.Click += (s, e) => CreerBC();
            btnAjouterLigne.Click += (s, e) => AjouterLigne();
            btnEnvoyer.Click += (s, e) => EnvoyerBC();
            btnRefresh.Click += (s, e) => RefreshLines();
            btnPdf.Click += (s, e) => ExporterPdfBC();
        }


        void ChargerMagasins()
        {
            cmbMagasin.Items.Clear();

            var dt = DbHelper.Table(cs, "SELECT IdMagasin, Nom AS NomMagasin FROM dbo.Magasin WHERE Actif=1 ORDER BY Nom");

            foreach (DataRow r in dt.Rows)
            {
                string nom = Convert.ToString(r["NomMagasin"] ?? "").Trim();
                int id = Convert.ToInt32(r["IdMagasin"]);

                cmbMagasin.Items.Add(new ComboboxItem(nom, id));
            }

            if (cmbMagasin.Items.Count > 0) cmbMagasin.SelectedIndex = 0;
        }


        int? MagasinId() => cmbMagasin.SelectedItem is ComboboxItem it ? it.Value : (int?)null;


        void ChargerFournisseurs()
        {
            cmbFournisseur.Items.Clear();

            var dt = DbHelper.Table(cs,
    "SELECT ID_Fournisseur, Nom FROM Fournisseur WHERE Actif=1 ORDER BY Nom");

            foreach (DataRow r in dt.Rows)
                cmbFournisseur.Items.Add(new ComboboxItem(r["Nom"].ToString(), Convert.ToInt32(r["ID_Fournisseur"])));

            if (cmbFournisseur.Items.Count > 0) cmbFournisseur.SelectedIndex = 0;
        }

        void ChargerProduits()
        {
            cmbProduit.Items.Clear();

            var dt = DbHelper.Table(cs,
    "SELECT ID_Produit, NomProduit FROM Produit ORDER BY NomProduit");

            foreach (DataRow r in dt.Rows)
                cmbProduit.Items.Add(new ComboboxItem(r["NomProduit"].ToString(), Convert.ToInt32(r["ID_Produit"])));

            if (cmbProduit.Items.Count > 0) cmbProduit.SelectedIndex = 0;
        }

        int? FournisseurId() => cmbFournisseur.SelectedItem is ComboboxItem it ? it.Value : (int?)null;
        int? ProduitId() => cmbProduit.SelectedItem is ComboboxItem it ? it.Value : (int?)null;

        void CreerBC()
        {
            var f = FournisseurId();
            if (f == null) { MessageBox.Show("Choisis un fournisseur."); return; }

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.sp_BC_Create", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NumeroBC", SqlDbType.NVarChar, 50).Value = (txtNumero.Text ?? "").Trim();
                cmd.Parameters.Add("@ID_Fournisseur", SqlDbType.Int).Value = f.Value;
                var m = MagasinId();
                if (m == null) { MessageBox.Show("Choisis un magasin."); return; }

                cmd.Parameters.Add("@IdMagasin", SqlDbType.Int).Value = m.Value;
                cmd.Parameters.Add("@DateBC", SqlDbType.Date).Value = dtDate.Value.Date;
                cmd.Parameters.Add("@CreePar", SqlDbType.NVarChar, 150).Value = (SessionEmploye.Nom + " " + SessionEmploye.Prenom).Trim();

                con.Open();
                _idBC = Convert.ToInt32(cmd.ExecuteScalar());
            }

            MessageBox.Show("BC créé ✅ ID=" + _idBC);
            RefreshLines();
        }

        void AjouterLigne()
        {
            if (_idBC <= 0) { MessageBox.Show("Crée le BC d'abord."); return; }
            var p = ProduitId();
            if (p == null) { MessageBox.Show("Choisis un produit."); return; }

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.sp_BC_AddLine", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ID_BC", SqlDbType.Int).Value = _idBC;
                    cmd.Parameters.Add("@ID_Produit", SqlDbType.Int).Value = p.Value;
                    cmd.Parameters.Add("@ID_Variante", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@QteCommandeeBase", SqlDbType.Int).Value = (int)nudQte.Value;

                    var pr = cmd.Parameters.Add("@PrixAchat", SqlDbType.Decimal);
                    pr.Precision = 18; pr.Scale = 2; pr.Value = nudPrix.Value;

                    cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = cmbDevise.SelectedItem?.ToString() ?? "CDF";

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                RefreshLines();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ajout ligne BC : " + ex.Message);
            }
        }

        void EnvoyerBC()
        {
            if (_idBC <= 0) { MessageBox.Show("Crée un BC d'abord."); return; }

            try
            {
                DbHelper.NonQuery(cs, "dbo.sp_BC_SetStatus", CommandType.StoredProcedure,
                    new SqlParameter("@ID_BC", _idBC),
                    new SqlParameter("@Statut", "ENVOYE")
                );

                MessageBox.Show("BC passé à ENVOYE ✅");
                btnEnvoyer.Text = "Statut: ENVOYE";
                btnEnvoyer.Enabled = false; // optionnel, si tu veux empêcher re-click
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur changement statut BC : " + ex.Message);
            }
        }

        void RefreshLines()
        {
            if (_idBC <= 0) { dgv.DataSource = null; return; }

            var dt = DbHelper.Table(cs, @"
SELECT
    l.ID_Ligne,
    p.NomProduit,
    l.QteCommandeeBase,
    l.PrixAchat,
    l.Devise
FROM dbo.BonCommandeLigne l
JOIN dbo.Produit p ON p.ID_Produit=l.ID_Produit
WHERE l.ID_BC=@bc
ORDER BY l.ID_Ligne DESC",
                new SqlParameter("@bc", SqlDbType.Int) { Value = _idBC });

            dgv.DataSource = dt;
            if (dgv.Columns.Contains("ID_Ligne")) dgv.Columns["ID_Ligne"].Visible = false;
        }

        private void ExporterPdfBC()
        {
            if (_idBC <= 0 || dgv.DataSource == null)
            {
                MessageBox.Show("Crée un BC et ajoute au moins une ligne.");
                return;
            }

            string path = PdfExportHelper.AskSavePdfPath($"BC_{_idBC}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            if (path == null) return;

            var dt = PdfExportHelper.DataGridViewToDataTable(dgv);

            PdfExportHelper.ExportDataTableToPdf(path,
                "BON DE COMMANDE (BC)",
                dt,
                $"BC ID: {_idBC} | Fournisseur: {cmbFournisseur.Text} | Date: {dtDate.Value:yyyy-MM-dd}");

            MessageBox.Show("PDF BC généré ✅");
        }
    }
}