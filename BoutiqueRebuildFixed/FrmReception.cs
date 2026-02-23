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
    public partial class FrmReception : Form
    {
        private readonly string cs = ConfigSysteme.ConnectionString;

        ComboBox cmbFournisseur, cmbDepot, cmbProduit, cmbDevise, cmbBC;
        TextBox txtNumeroRec, txtLot;
        DateTimePicker dtDate, dtExp;
        CheckBox chkExp;
        NumericUpDown nudQte, nudPrix;
        Button btnCreer, btnAddLine, btnValider, btnRefresh;
        DataGridView dgv;
        private Button btnPdf;

        int _idRec = 0;
        public FrmReception()
        {
            InitializeComponent();

            // ✅ IMPORTANT : si ton Designer a déjà mis un Panel Dock=Fill, ça cache ton DGV.
            // On nettoie tout pour être sûr que ton UI dynamique est visible.
            Controls.Clear();

            Text = "Réception Fournisseur (ENTREE stock + lots)";
            Width = 1150;
            Height = 680;

            BuildUI();

            Load += (s, e) =>
            {
                ChargerFournisseurs();
                ChargerDepots();
                ChargerBC();
                ChargerProduits();

                if (cmbDevise.Items.Count > 0 && cmbDevise.SelectedIndex < 0)
                    cmbDevise.SelectedIndex = 0;

                dtExp.Enabled = false;

                // ✅ Force le DGV devant
                dgv?.BringToFront();
                dgv?.Invalidate();
            };
        }

        private void FrmReception_Load(object sender, EventArgs e)
        {
            
        }
        void BuildUI()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 160 };
            Controls.Add(top);

            Label L(string t, int left, int topPos)
            {
                var lb = new Label { Left = left, Top = topPos, AutoSize = true, Text = t };
                top.Controls.Add(lb);
                return lb;
            }

            // ===== Ligne 1 =====
            L("Fournisseur", 15, 8);
            cmbFournisseur = new ComboBox { Left = 15, Top = 25, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };

            L("Dépôt", 285, 8);
            cmbDepot = new ComboBox { Left = 285, Top = 25, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            L("Bon de Commande", 500, 8);
            cmbBC = new ComboBox { Left = 500, Top = 25, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            L("N° Réception", 710, 8);
            txtNumeroRec = new TextBox { Left = 710, Top = 25, Width = 140 };
            txtNumeroRec.Text = "REC-" + DateTime.Now.ToString("yyyyMMdd-HHmm");

            L("Date réception", 860, 8);
            dtDate = new DateTimePicker { Left = 860, Top = 25, Width = 120, Format = DateTimePickerFormat.Short };

            btnCreer = new Button { Left = 990, Top = 23, Width = 140, Height = 28, Text = "Créer réception" };
            btnRefresh = new Button { Left = 990, Top = 55, Width = 140, Height = 28, Text = "Rafraîchir" };

            // ===== Ligne 2 =====
            L("Produit", 15, 60);
            cmbProduit = new ComboBox { Left = 15, Top = 77, Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };

            L("Quantité reçue", 345, 60);
            nudQte = new NumericUpDown { Left = 345, Top = 77, Width = 80, Minimum = 1, Maximum = 999999, Value = 1 };

            L("Prix achat", 435, 60);
            nudPrix = new NumericUpDown { Left = 435, Top = 77, Width = 120, Minimum = 0, Maximum = 999999999, DecimalPlaces = 2, Value = 0 };

            L("Devise", 565, 60);
            cmbDevise = new ComboBox { Left = 565, Top = 77, Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDevise.Items.AddRange(new object[] { "CDF", "USD" });

            L("Lot / Série", 655, 60);
            txtLot = new TextBox { Left = 655, Top = 77, Width = 150 };

            chkExp = new CheckBox { Left = 815, Top = 79, Width = 100, Text = "Expiration" };

            L("Date expiration", 920, 60);
            dtExp = new DateTimePicker { Left = 920, Top = 77, Width = 140, Format = DateTimePickerFormat.Short, Enabled = false };

            // ===== Ligne 3 =====
            btnAddLine = new Button { Left = 15, Top = 115, Width = 180, Height = 28, Text = "Ajouter ligne" };
            btnValider = new Button { Left = 205, Top = 115, Width = 260, Height = 28, Text = "Valider réception (ENTREE)" };
            btnPdf = new Button { Left = 480, Top = 115, Width = 220, Height = 28, Text = "Exporter PDF Réception" };

            top.Controls.AddRange(new Control[]
            {
                cmbFournisseur, cmbDepot, cmbBC, txtNumeroRec, dtDate, btnCreer, btnRefresh,
                cmbProduit, nudQte, nudPrix, cmbDevise, txtLot, chkExp, dtExp,
                btnAddLine, btnValider, btnPdf
            });

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                Visible = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.Both,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.False }
            };
            Controls.Add(dgv);
            dgv.BringToFront();

            // ===== Events =====
            chkExp.CheckedChanged += (s, e) => dtExp.Enabled = chkExp.Checked;
            btnCreer.Click += (s, e) => CreerReception();
            btnAddLine.Click += (s, e) => AjouterLigne();
            btnValider.Click += (s, e) => Valider();
            btnRefresh.Click += (s, e) => RefreshLines();
            btnPdf.Click += (s, e) => ExporterPdfReception();
        }

        void ChargerFournisseurs()
        {
            cmbFournisseur.Items.Clear();

            var dt = DbHelper.Table(cs,
    "SELECT ID_Fournisseur, Nom FROM Fournisseur WHERE Actif=1 ORDER BY Nom");

            foreach (DataRow r in dt.Rows)
                cmbFournisseur.Items.Add(new ComboboxItem(r["Nom"].ToString(), Convert.ToInt32(r["ID_Fournisseur"])));

            if (cmbFournisseur.Items.Count > 0) cmbFournisseur.SelectedIndex = 0;
        }

        void ChargerDepots()
        {
            cmbDepot.Items.Clear();

            // ✅ Dépôt = Magasin
            var dt = DbHelper.Table(cs,
                "SELECT IdMagasin AS ID_Depot, Nom AS NomDepot FROM dbo.Magasin WHERE Actif=1 ORDER BY Nom");

            foreach (DataRow r in dt.Rows)
                cmbDepot.Items.Add(new ComboboxItem(r["NomDepot"].ToString(), Convert.ToInt32(r["ID_Depot"])));

            if (cmbDepot.Items.Count > 0) cmbDepot.SelectedIndex = 0;
        }


        void ChargerBC()
        {
            cmbBC.Items.Clear();

            var dt = DbHelper.Table(cs, @"
SELECT TOP 200 ID_BC, NumeroBC
FROM BonCommande
ORDER BY ID_BC DESC");

            cmbBC.Items.Add(new ComboboxItem("— Réception sans BC —", 0));

            foreach (DataRow r in dt.Rows)
                cmbBC.Items.Add(new ComboboxItem(r["NumeroBC"].ToString(), Convert.ToInt32(r["ID_BC"])));

            cmbBC.SelectedIndex = 0;
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
        int? DepotId() => cmbDepot.SelectedItem is ComboboxItem it ? it.Value : (int?)null;
        int? ProduitId() => cmbProduit.SelectedItem is ComboboxItem it ? it.Value : (int?)null;

        int? BCId()
        {
            if (cmbBC.SelectedItem is ComboboxItem it)
                return it.Value <= 0 ? (int?)null : it.Value;

            return null;
        }

        private void CreerReception()
        {
            var f = FournisseurId();
            var d = DepotId();

            if (f == null || d == null)
            {
                MessageBox.Show("Fournisseur et dépôt requis.");
                return;
            }

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.sp_Reception_Create", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@NumeroReception", SqlDbType.NVarChar, 80).Value =
                        (txtNumeroRec.Text ?? "").Trim();

                    var bc = BCId();
                    cmd.Parameters.Add("@ID_BC", SqlDbType.Int).Value = (object)bc ?? DBNull.Value;

                    cmd.Parameters.Add("@ID_Fournisseur", SqlDbType.Int).Value = f.Value;
                    cmd.Parameters.Add("@ID_Depot", SqlDbType.Int).Value = d.Value;

                    cmd.Parameters.Add("@DateReception", SqlDbType.Date).Value = dtDate.Value.Date;

                    string user = (SessionEmploye.Nom + " " + SessionEmploye.Prenom).Trim();
                    if (string.IsNullOrWhiteSpace(user)) user = Environment.UserName;

                    cmd.Parameters.Add("@CreePar", SqlDbType.NVarChar, 200).Value = user;

                    con.Open();
                    object res = cmd.ExecuteScalar();

                    _idRec = Convert.ToInt32(res);
                }

                MessageBox.Show("Réception créée ✅ ID=" + _idRec);
                RefreshLines();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur création réception: " + ex.Message);
            }
        }

        private void AjouterLigne()
        {
            if (_idRec <= 0)
            {
                MessageBox.Show("Crée la réception d'abord.");
                return;
            }

            var p = ProduitId();
            if (p == null)
            {
                MessageBox.Show("Produit requis.");
                return;
            }

            // Interdire si validée
            var dtStat = DbHelper.Table(cs,
    "SELECT ISNULL(Statut,'') AS Statut FROM dbo.Reception WHERE ID_Reception=@r",
    CommandType.Text,
    60,
    new SqlParameter("@r", SqlDbType.Int) { Value = _idRec });

            string statut = (dtStat.Rows.Count > 0 ? (dtStat.Rows[0]["Statut"]?.ToString() ?? "") : "").Trim();
            if (string.Equals(statut, "VALIDEE", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Réception déjà VALIDÉE. Impossible d'ajouter une ligne.");
                return;
            }

            string lot = (txtLot.Text ?? "").Trim();

            try
            {
                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.sp_Reception_AddLine", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ID_Reception", SqlDbType.Int).Value = _idRec;
                    cmd.Parameters.Add("@ID_Produit", SqlDbType.Int).Value = p.Value;

                    // Variante facultative
                    var variante = VarianteId();
                    cmd.Parameters.Add("@ID_Variante", SqlDbType.Int).Value = (object)variante ?? DBNull.Value;

                    cmd.Parameters.Add("@QteRecueBase", SqlDbType.Int).Value = (int)nudQte.Value;

                    // Decimal (très important)
                    var pr = cmd.Parameters.Add("@PrixAchat", SqlDbType.Decimal);
                    pr.Precision = 18;
                    pr.Scale = 2;
                    pr.Value = nudPrix.Value;

                    cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value =
                        (cmbDevise.SelectedItem?.ToString() ?? "CDF");

                    cmd.Parameters.Add("@LotNumero", SqlDbType.NVarChar, 80).Value =
                        string.IsNullOrWhiteSpace(lot) ? (object)DBNull.Value : lot;

                    cmd.Parameters.Add("@DateExpiration", SqlDbType.Date).Value =
                        chkExp.Checked ? (object)dtExp.Value.Date : DBNull.Value;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                txtLot.Text = "";
                chkExp.Checked = false;

                RefreshLines();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ajout ligne: " + ex.Message);
            }
        }

        private int? VarianteId()
        {
            return null; // pas de variantes => NULL en base
        }


        private void Valider()
        {
            if (_idRec <= 0)
            {
                MessageBox.Show("Aucune réception.");
                return;
            }

            try
            {
                string user = (SessionEmploye.Nom + " " + SessionEmploye.Prenom).Trim();
                if (string.IsNullOrWhiteSpace(user)) user = Environment.UserName;

                using (var con = new SqlConnection(cs))
                using (var cmd = new SqlCommand("dbo.sp_Reception_Validate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID_Reception", SqlDbType.Int).Value = _idRec;
                    cmd.Parameters.Add("@Utilisateur", SqlDbType.NVarChar, 200).Value = user;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Réception validée + Stock mis à jour ✅");
                RefreshLines();

                btnAddLine.Enabled = false;
                btnValider.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur validation: " + ex.Message);
            }
        }
        private void AppliquerEntreeStockDepuisReception(int idReception, string utilisateur)
        {
            using (var con = new SqlConnection(cs))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    // 1) Charger l'entête réception
                    int idDepot;
                    string statut;

                    using (var cmd = new SqlCommand(@"
SELECT ID_Depot, ISNULL(Statut,'') 
FROM dbo.Reception 
WHERE ID_Reception=@r;", con, tx))
                    {
                        cmd.Parameters.Add("@r", SqlDbType.Int).Value = idReception;
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read())
                                throw new Exception("Réception introuvable.");

                            idDepot = Convert.ToInt32(rd.GetValue(0));
                            statut = (rd.GetValue(1)?.ToString() ?? "").Trim();
                        }
                    }

                    if (string.Equals(statut, "VALIDEE", StringComparison.OrdinalIgnoreCase))
                        return; // déjà validée, on ne double pas le stock

                    // 2) Marquer réception validée
                    using (var cmd = new SqlCommand(@"
UPDATE dbo.Reception
SET Statut='VALIDEE', DateValidation=GETDATE(), ValidePar=@u
WHERE ID_Reception=@r;", con, tx))
                    {
                        cmd.Parameters.Add("@u", SqlDbType.NVarChar, 200).Value = utilisateur ?? "";
                        cmd.Parameters.Add("@r", SqlDbType.Int).Value = idReception;
                        cmd.ExecuteNonQuery();
                    }

                    // 3) Charger lignes
                    var lignes = new List<(int idProduit, int? idVariante, int qte, string lot, DateTime? exp)>();

                    using (var cmd = new SqlCommand(@"
SELECT ID_Produit, ID_Variante, QteRecueBase, LotNumero, DateExpiration
FROM dbo.ReceptionLigne
WHERE ID_Reception=@r;", con, tx))
                    {
                        cmd.Parameters.Add("@r", SqlDbType.Int).Value = idReception;
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                int idP = Convert.ToInt32(rd["ID_Produit"]);
                                int? idV = rd["ID_Variante"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["ID_Variante"]);
                                int qte = Convert.ToInt32(rd["QteRecueBase"]);
                                string lot = rd["LotNumero"] == DBNull.Value ? null : rd["LotNumero"].ToString();
                                DateTime? exp = rd["DateExpiration"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["DateExpiration"]);

                                if (qte <= 0) continue;
                                lignes.Add((idP, idV, qte, lot, exp));
                            }
                        }
                    }

                    if (lignes.Count == 0)
                        throw new Exception("Aucune ligne de réception. Ajoute d'abord des produits.");

                    // 4) Appliquer stock par ligne
                    foreach (var l in lignes)
                    {
                        // 4.1 StockDepot (par produit)
                        UpsertStockDepot(con, tx, idDepot, l.idProduit, l.qte);

                        // 4.2 Produit (global)
                        UpdateProduitStock(con, tx, l.idProduit, l.qte);

                        // 4.3 Variantes (si applicable)
                        if (l.idVariante.HasValue)
                            UpsertStockDepotVariante(con, tx, idDepot, l.idVariante.Value, l.qte);

                        // 4.4 Lots (si lot fourni OU expiration)
                        if (!string.IsNullOrWhiteSpace(l.lot) || l.exp.HasValue)
                            UpsertStockLot(con, tx, idDepot, l.idProduit, l.idVariante, l.lot, l.exp, l.qte);
                    }

                    tx.Commit();
                }
            }
        }

        private void UpsertStockDepot(SqlConnection con, SqlTransaction tx, int idDepot, int idProduit, int qte)
        {
            // ✅ Ton besoin demandé : augmenter StockInitial ET StockActuel
            using (var cmd = new SqlCommand(@"
IF EXISTS(SELECT 1 FROM dbo.StockDepot WHERE ID_Depot=@d AND ID_Produit=@p)
BEGIN
    UPDATE dbo.StockDepot
    SET StockInitial = ISNULL(StockInitial,0) + @q,
        StockActuel  = ISNULL(StockActuel,0) + @q
    WHERE ID_Depot=@d AND ID_Produit=@p;
END
ELSE
BEGIN
    INSERT INTO dbo.StockDepot(ID_Depot, ID_Produit, StockInitial, StockActuel, SeuilMin)
    VALUES(@d, @p, @q, @q, 0);
END", con, tx))
            {
                cmd.Parameters.Add("@d", SqlDbType.Int).Value = idDepot;
                cmd.Parameters.Add("@p", SqlDbType.Int).Value = idProduit;
                cmd.Parameters.Add("@q", SqlDbType.Int).Value = qte;
                cmd.ExecuteNonQuery();
            }

            // 🔁 Si tu veux PLUS TARD que StockInitial ne change PAS après initial,
            // remplace StockInitial = StockInitial + @q par StockInitial = StockInitial (ne rien faire)
        }

        private void UpsertStockDepotVariante(SqlConnection con, SqlTransaction tx, int idDepot, int idVariante, int qte)
        {
            using (var cmd = new SqlCommand(@"
IF EXISTS(SELECT 1 FROM dbo.StockDepotVariante WHERE ID_Depot=@d AND ID_Variante=@v)
BEGIN
    UPDATE dbo.StockDepotVariante
    SET StockInitial = ISNULL(StockInitial,0) + @q,
        StockActuel  = ISNULL(StockActuel,0) + @q
    WHERE ID_Depot=@d AND ID_Variante=@v;
END
ELSE
BEGIN
    INSERT INTO dbo.StockDepotVariante(ID_Depot, ID_Variante, StockInitial, StockActuel, SeuilMin)
    VALUES(@d, @v, @q, @q, 0);
END", con, tx))
            {
                cmd.Parameters.Add("@d", SqlDbType.Int).Value = idDepot;
                cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVariante;
                cmd.Parameters.Add("@q", SqlDbType.Int).Value = qte;
                cmd.ExecuteNonQuery();
            }
        }

        private void UpsertStockLot(SqlConnection con, SqlTransaction tx, int idDepot, int idProduit, int? idVariante, string lotNumero, DateTime? dateExp, int qte)
        {
            using (var cmd = new SqlCommand(@"
DECLARE @idLot INT;

SELECT TOP 1 @idLot = ID_Lot
FROM dbo.StockLot
WHERE ID_Depot=@d
  AND ID_Produit=@p
  AND ((@v IS NULL AND ID_Variante IS NULL) OR (ID_Variante=@v))
  AND (ISNULL(LotNumero,'') = ISNULL(@lot,''))
  AND ((@exp IS NULL AND DateExpiration IS NULL) OR (DateExpiration=@exp));

IF @idLot IS NULL
BEGIN
    INSERT INTO dbo.StockLot(ID_Depot, ID_Produit, ID_Variante, LotNumero, DateExpiration, QuantiteBase, DateCreation)
    VALUES(@d, @p, @v, @lot, @exp, @q, GETDATE());
END
ELSE
BEGIN
    UPDATE dbo.StockLot
    SET QuantiteBase = ISNULL(QuantiteBase,0) + @q
    WHERE ID_Lot=@idLot;
END", con, tx))
            {
                cmd.Parameters.Add("@d", SqlDbType.Int).Value = idDepot;
                cmd.Parameters.Add("@p", SqlDbType.Int).Value = idProduit;
                cmd.Parameters.Add("@v", SqlDbType.Int).Value = (object)idVariante ?? DBNull.Value;
                cmd.Parameters.Add("@lot", SqlDbType.NVarChar, 100).Value = (object)lotNumero ?? DBNull.Value;
                cmd.Parameters.Add("@exp", SqlDbType.Date).Value = (object)dateExp ?? DBNull.Value;
                cmd.Parameters.Add("@q", SqlDbType.Int).Value = qte;
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateProduitStock(SqlConnection con, SqlTransaction tx, int idProduit, int qte)
        {
            // ✅ Table Produit : Quantite, StockInitial, StockActuel
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Produit
SET Quantite     = ISNULL(Quantite,0) + @q,
    StockInitial = ISNULL(StockInitial,0) + @q,
    StockActuel  = ISNULL(StockActuel,0) + @q
WHERE ID_Produit=@p;", con, tx))
            {
                cmd.Parameters.Add("@q", SqlDbType.Int).Value = qte;
                cmd.Parameters.Add("@p", SqlDbType.Int).Value = idProduit;
                cmd.ExecuteNonQuery();
            }
        }


        void RefreshLines()
        {
            if (_idRec <= 0) { dgv.DataSource = null; return; }

            var dt = DbHelper.Table(cs, @"
SELECT
    r.ID_Reception,
    r.NumeroReception,
    r.DateReception,
    r.Statut,
    r.DateValidation,
    r.ValidePar,
    r.ID_Depot,

    rl.ID_Ligne,
    rl.ID_Produit,
    rl.ID_Variante,
    p.NomProduit,
    rl.QteRecueBase,
    rl.PrixAchat,
    rl.Devise,
    rl.LotNumero,
    rl.DateExpiration
FROM dbo.Reception r
INNER JOIN dbo.ReceptionLigne rl ON rl.ID_Reception = r.ID_Reception
LEFT JOIN dbo.Produit p ON p.ID_Produit = rl.ID_Produit
WHERE r.ID_Reception = @r
ORDER BY rl.ID_Ligne DESC;",
                new SqlParameter("@r", SqlDbType.Int) { Value = _idRec });

            dgv.DataSource = dt;

            // Masquer ids inutiles à l’utilisateur
            if (dgv.Columns.Contains("ID_Reception")) dgv.Columns["ID_Reception"].Visible = false;
            if (dgv.Columns.Contains("ID_Ligne")) dgv.Columns["ID_Ligne"].Visible = false;
            if (dgv.Columns.Contains("ID_Produit")) dgv.Columns["ID_Produit"].Visible = false;
            if (dgv.Columns.Contains("ID_Variante")) dgv.Columns["ID_Variante"].Visible = false;
            if (dgv.Columns.Contains("ID_Depot")) dgv.Columns["ID_Depot"].Visible = false;

            if (dgv.Columns.Contains("NomProduit")) dgv.Columns["NomProduit"].Width = 250;
            if (dgv.Columns.Contains("NumeroReception")) dgv.Columns["NumeroReception"].Width = 160;
            if (dgv.Columns.Contains("Statut")) dgv.Columns["Statut"].Width = 90;
            if (dgv.Columns.Contains("ValidePar")) dgv.Columns["ValidePar"].Width = 180;

            if (dgv.Columns.Contains("DateExpiration"))
                dgv.Columns["DateExpiration"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgv.Columns.Contains("DateReception"))
                dgv.Columns["DateReception"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (dgv.Columns.Contains("DateValidation"))
                dgv.Columns["DateValidation"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

            dgv.Visible = true;
            dgv.BringToFront();
        }

        private void ExporterPdfReception()
        {
            if (_idRec <= 0 || dgv.DataSource == null)
            {
                MessageBox.Show("Crée une réception et ajoute des lignes.");
                return;
            }

            string path = PdfExportHelper.AskSavePdfPath($"Reception_{_idRec}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            if (path == null) return;

            var dt = PdfExportHelper.DataGridViewToDataTable(dgv);

            PdfExportHelper.ExportDataTableToPdf(path,
                "RECEPTION FOURNISSEUR",
                dt,
                $"Réception ID: {_idRec} | Fournisseur: {cmbFournisseur.Text} | Dépôt: {cmbDepot.Text} | Date: {dtDate.Value:yyyy-MM-dd}");

            MessageBox.Show("PDF Réception généré ✅");
        }
    }
}
