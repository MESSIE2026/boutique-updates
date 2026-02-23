using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmFideliteRetrait : Form

    {
        private int _idClientCourant = 0;
        private string _codeCarteCourant = "";
        // ✅ Sécurité : on exige une validation manager avant actions sensibles
        private bool _signatureOk = false;
        private int _managerId = 0;
        private string _managerNom = "";
        private string _managerPoste = "";

        private void ResetSignatureCache()
        {
            _signatureOk = false;
            _managerId = 0;
            _managerNom = "";
            _managerPoste = "";
        }

        private enum CarteType { None, Client, Partenaire }
        private CarteType _typeCourant = CarteType.None;

        private int _idPartenaireCourant = 0;
        private string _nomCourant = "";


        public FrmFideliteRetrait()
        {
            InitializeComponent();

            this.Load += FrmFideliteRetrait_Load;
            txtScanCarte.KeyDown += txtScanCarte_KeyDown;
            btnAppliquer.Click += btnAppliquer_Click;
        }

        private void FrmFideliteRetrait_Load(object sender, EventArgs e)
        {
            // Combo devise
            cmbDevise.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDevise.Items.Clear();
            cmbDevise.Items.AddRange(new[] { "CDF", "USD" });
            cmbDevise.SelectedIndex = 0;

            // DGV historique PRO
            InitialiserDgvHistorique();

            ResetUI();
            SetMessage("Prêt. Scannez la carte fidélité.", true);
            ResetSignatureCache();
            txtScanCarte.Focus();
        }

        // ========================= UI HELPERS =========================

        private void ResetUI()
        {
            _typeCourant = CarteType.None;

            _idClientCourant = 0;
            _idPartenaireCourant = 0;

            _codeCarteCourant = "";
            _nomCourant = "";

            lblClientNom.Text = "-";
            lblSoldeCDF.Text = "0,00";
            lblSoldeUSD.Text = "0,00";

            txtMontantAUtiliser.Text = "";
            txtScanCarte.Text = "";

            if (dgvHistorique != null)
                dgvHistorique.DataSource = null;
        }

        private void SetMessage(string text, bool ok)
        {
            if (lblMessage == null) return;

            lblMessage.Text = text ?? "";
            lblMessage.ForeColor = ok ? Color.ForestGreen : Color.Firebrick;
        }

        private decimal ParseMoney(string s)
        {
            s = (s ?? "").Trim();
            if (s.Length == 0) return 0m;

            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out decimal v)) return v;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v)) return v;

            return 0m;
        }

        // ========================= DGV HISTORIQUE =========================

        private void InitialiserDgvHistorique()
        {
            if (dgvHistorique == null) return;

            dgvHistorique.DataSource = null;
            dgvHistorique.Columns.Clear();

            dgvHistorique.AllowUserToAddRows = false;
            dgvHistorique.AllowUserToDeleteRows = false;
            dgvHistorique.ReadOnly = true;
            dgvHistorique.MultiSelect = false;
            dgvHistorique.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvHistorique.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistorique.RowHeadersVisible = false;
            dgvHistorique.AllowUserToResizeRows = false;

            dgvHistorique.EnableHeadersVisualStyles = false;
            dgvHistorique.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);
            dgvHistorique.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvHistorique.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvHistorique.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            // Colonnes
            dgvHistorique.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DateMvt",
                HeaderText = "Date",
                DataPropertyName = "DateMvt",
                FillWeight = 30f,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });

            dgvHistorique.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TypeMvt",
                HeaderText = "Type",
                DataPropertyName = "TypeMvt",
                FillWeight = 20f
            });

            dgvHistorique.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Devise",
                HeaderText = "Devise",
                DataPropertyName = "Devise",
                FillWeight = 15f
            });

            dgvHistorique.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MontantPoints",
                HeaderText = "Montant",
                DataPropertyName = "MontantPoints",
                FillWeight = 20f,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "N2"
                }
            });

            dgvHistorique.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Commentaire",
                HeaderText = "Commentaire",
                DataPropertyName = "Commentaire",
                FillWeight = 35f
            });
        }

        private bool DemanderSignatureManager(string typeAction, string reference, string details)
        {
            try
            {
                using (var frm = new FrmSignatureManager(
                    ConfigSysteme.ConnectionString,
                    typeAction: typeAction,
                    permissionCode: "",        // ici on ne contrôle pas par module, juste une validation manager
                    reference: reference,
                    details: details,
                    idEmployeDemandeur: SessionEmploye.ID_Employe,     // si tu veux tracer qui demande
                    roleDemandeur: SessionEmploye.Poste                // rôle/poste du demandeur
                ))
                {
                    // ✅ ici on veut autoriser l'action : donc fermeture auto si OK
                    frm.CloseOnApproved = true;

                    // (Optionnel) si tu veux démarrer direct sur empreinte si configurée :
                    frm.StartOnEmpreinteTab = true;

                    var dr = frm.ShowDialog(this);

                    if (dr == DialogResult.OK && frm.Approved)
                    {
                        _signatureOk = true;
                        _managerId = frm.ManagerId;
                        _managerNom = frm.ManagerNom;
                        _managerPoste = frm.ManagerPoste;
                        return true;
                    }

                    _signatureOk = false;
                    _managerId = 0;
                    _managerNom = "";
                    _managerPoste = "";
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur Signature Manager : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void ChargerHistorique(SqlConnection con, int idClient)
        {
            if (dgvHistorique == null) return;

            using (var da = new SqlDataAdapter(@"
SELECT TOP 30
    DateMvt,
    TypeMvt,
    Devise,
    MontantPoints,
    Commentaire
FROM ClientFideliteMouvements
WHERE IdClient=@id
ORDER BY DateMvt DESC;", con))
            {
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = idClient;

                var dt = new DataTable();
                da.Fill(dt);

                dgvHistorique.AutoGenerateColumns = false;
                dgvHistorique.DataSource = dt;
                dgvHistorique.ClearSelection();
            }
        }

        // ========================= SCAN CARTE =========================

        private void txtScanCarte_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            string code = (txtScanCarte.Text ?? "").Trim();
            if (code.Length == 0)
            {
                txtScanCarte.Focus();
                return;
            }

            ChargerCompteParCarte(code);
        }

        private void ChargerClientParCarte(string codeCarte)
        {
            try
            {
                int idClient = 0;
                string nom = "";
                decimal soldeUSD = 0m;

                using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();

                    // 1) client via CodeCarte
                    using (var cmd = new SqlCommand(@"
SELECT TOP 1 
    ID_Clients,
    LTRIM(RTRIM(ISNULL(Nom,''))) 
    + CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(Prenom,''))), '') IS NULL THEN '' 
           ELSE ' ' + LTRIM(RTRIM(ISNULL(Prenom,''))) END AS NomComplet
FROM dbo.Clients
WHERE CodeCarte=@Code;", con))
                    {
                        cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 50).Value = (codeCarte ?? "").Trim();

                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            {
                                SetMessage("Carte inconnue.", false);
                                txtScanCarte.Clear();
                                txtScanCarte.Focus();
                                return;
                            }
                            idClient = Convert.ToInt32(r["ID_Clients"]);
                            nom = (r["NomComplet"]?.ToString() ?? "").Trim();
                        }
                    }

                    if (idClient <= 0)
                    {
                        SetMessage("Client invalide.", false);
                        txtScanCarte.Clear();
                        txtScanCarte.Focus();
                        return;
                    }

                    // 2) Ensure + Lire LoyaltyCompte (⚠️ param = @IdClient, pas @id)
                    using (var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.LoyaltyCompte WHERE IdClient=@IdClient)
BEGIN
    INSERT INTO dbo.LoyaltyCompte(IdClient, Points, CashbackSolde, Statut, DateMaj)
    VALUES(@IdClient, 0, 0, 'BRONZE', GETDATE());
END

SELECT TOP 1 ISNULL(CashbackSolde,0) AS CashbackSolde
FROM dbo.LoyaltyCompte
WHERE IdClient=@IdClient;", con))
                    {
                        cmd.Parameters.Add("@IdClient", SqlDbType.Int).Value = idClient;

                        var o = cmd.ExecuteScalar();
                        soldeUSD = (o == null || o == DBNull.Value) ? 0m : Convert.ToDecimal(o);
                    }

                    // 3) Historique depuis LoyaltyMouvement
                    ChargerHistoriqueLoyalty(con, idClient);
                }

                _typeCourant = CarteType.Client;
                _idClientCourant = idClient;
                _codeCarteCourant = (codeCarte ?? "").Trim();
                _nomCourant = nom;

                lblClientNom.Text = "CLIENT : " + nom;
                lblSoldeUSD.Text = soldeUSD.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"));
                lblSoldeCDF.Text = "0,00"; // mono-solde USD

                SetMessage("Client chargé. Vous pouvez appliquer un retrait.", true);

                txtScanCarte.Clear();
                txtMontantAUtiliser.Focus();
                txtMontantAUtiliser.SelectAll();
            }
            catch (Exception ex)
            {
                SetMessage("Erreur scan : " + ex.Message, false);
                txtScanCarte.Focus();
                txtScanCarte.SelectAll();
            }
        }

        private void ChargerHistoriqueLoyalty(SqlConnection con, int idClient)
        {
            if (dgvHistorique == null) return;

            using (var da = new SqlDataAdapter(@"
SELECT TOP 30
    DateMvt,
    Type AS TypeMvt,
    CAST('USD' AS nvarchar(10)) AS Devise,
    CashbackDelta AS MontantPoints,
    ISNULL(Note,'') AS Commentaire
FROM dbo.LoyaltyMouvement
WHERE IdClient=@IdClient
ORDER BY DateMvt DESC;", con))
            {
                da.SelectCommand.Parameters.Add("@IdClient", SqlDbType.Int).Value = idClient;

                var dt = new DataTable();
                da.Fill(dt);

                dgvHistorique.AutoGenerateColumns = false;
                dgvHistorique.DataSource = dt;
                dgvHistorique.ClearSelection();
            }
        }

        private void ChargerCompteParCarte(string codeCarte)
        {
            codeCarte = (codeCarte ?? "").Trim();
            ResetSignatureCache(); // ✅ ici

            bool isClient = codeCarte.StartsWith("FID-", StringComparison.OrdinalIgnoreCase);
            bool isPart = codeCarte.StartsWith("PART-", StringComparison.OrdinalIgnoreCase);

            if (!isClient && !isPart)
            {
                SetMessage("Préfixe inconnu. Utilise FID-xxxxxx ou PART-xxxxxx.", false);
                MessageBox.Show("Carte inconnue (préfixe).");
                txtScanCarte.SelectAll();
                txtScanCarte.Focus();
                return;
            }

            // ✅ 1) Signature manager avant toute opération
            // (Tu peux mettre une permissionCode si tu veux, ex: "btnFideliteRetrait")
            string who = isClient ? "CLIENT" : "PARTENAIRE";
            string details = $"Accès retrait fidélité | {who} | Carte={codeCarte}";

            bool ok = DemanderSignatureManager(
                typeAction: "FIDELITE_RETRAIT_AUTH",
                reference: "FrmFideliteRetrait",
                details: details
            );

            if (!ok)
            {
                SetMessage("Accès refusé : signature manager annulée ou invalide.", false);
                txtScanCarte.SelectAll();
                txtScanCarte.Focus();
                return;
            }

            // ✅ 2) Charger le compte après validation
            if (isClient)
                ChargerClientParCarte(codeCarte);
            else
                ChargerPartenaireParCarte(codeCarte);

            // ✅ On peut afficher le manager validateur
            SetMessage($"✅ Autorisé par {_managerNom} ({_managerPoste}).", true);
        }

        private void ChargerHistoriquePartenaire(SqlConnection con, int idPartenaire)
        {
            if (dgvHistorique == null) return;

            using (var da = new SqlDataAdapter(@"
SELECT TOP 30
    DateMvt,
    TypeMvt,
    Devise,
    MontantPoints,
    Commentaire
FROM PartenaireFideliteMouvements
WHERE IdPartenaire=@id
ORDER BY DateMvt DESC;", con))
            {
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = idPartenaire;

                var dt = new DataTable();
                da.Fill(dt);

                dgvHistorique.AutoGenerateColumns = false;
                dgvHistorique.DataSource = dt;
                dgvHistorique.ClearSelection();
            }
        }

        private void ChargerPartenaireParCarte(string codeCarte)
        {
            try
            {
                int idPart = 0;
                string nom = "";
                decimal soldeCDF = 0m, soldeUSD = 0m;

                using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();

                    // 1) Partenaire via CodeCarte
                    using (var cmd = new SqlCommand(@"
SELECT TOP 1
    IdPartenaire,
    LTRIM(RTRIM(ISNULL(Nom,''))) AS NomPartenaire
FROM Partenaire
WHERE CodeCarte=@c;", con))
                    {
                        cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = (codeCarte ?? "").Trim();

                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                            {
                                SetMessage("Carte partenaire inconnue.", false);
                                MessageBox.Show("Carte partenaire inconnue.");

                                txtScanCarte.Clear();
                                txtScanCarte.Focus();
                                return;
                            }

                            idPart = r.GetInt32(0);
                            nom = r.IsDBNull(1) ? "" : r.GetString(1);
                        }
                    }

                    if (idPart <= 0)
                    {
                        SetMessage("Partenaire invalide.", false);
                        txtScanCarte.Clear();
                        txtScanCarte.Focus();
                        return;
                    }

                    // 2) Solde fidélité partenaire (auto-crée si absent)
                    using (var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM PartenaireFidelite WHERE IdPartenaire=@id)
    INSERT INTO PartenaireFidelite(IdPartenaire) VALUES(@id);

SELECT SoldeCDF, SoldeUSD
FROM PartenaireFidelite
WHERE IdPartenaire=@id;", con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPart;

                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                soldeCDF = r.IsDBNull(0) ? 0m : r.GetDecimal(0);
                                soldeUSD = r.IsDBNull(1) ? 0m : r.GetDecimal(1);
                            }
                        }
                    }

                    // 3) Historique
                    ChargerHistoriquePartenaire(con, idPart);
                }

                // UI + mémoire
                _typeCourant = CarteType.Partenaire;
                _idPartenaireCourant = idPart;
                _idClientCourant = 0;

                _codeCarteCourant = (codeCarte ?? "").Trim();
                _nomCourant = nom;

                lblClientNom.Text = "PARTENAIRE : " + nom;
                lblSoldeCDF.Text = soldeCDF.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"));
                lblSoldeUSD.Text = soldeUSD.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"));

                SetMessage("Partenaire chargé. Vous pouvez appliquer un retrait.", true);

                txtScanCarte.Clear();
                txtMontantAUtiliser.Focus();
                txtMontantAUtiliser.SelectAll();
            }
            catch (Exception ex)
            {
                SetMessage("Erreur scan partenaire : " + ex.Message, false);
                MessageBox.Show("Erreur scan partenaire : " + ex.Message);

                txtScanCarte.Focus();
                txtScanCarte.SelectAll();
            }
        }

        private void btnAppliquer_Click(object sender, EventArgs e)
        {
            if (_typeCourant == CarteType.None || string.IsNullOrWhiteSpace(_codeCarteCourant))
            {
                SetMessage("Scannez d'abord une carte.", false);
                MessageBox.Show("Scannez d'abord une carte.");
                txtScanCarte.Focus();
                return;
            }

            // ✅ AJOUT ICI
            if (!_signatureOk || _managerId <= 0)
            {
                SetMessage("⚠️ Signature manager requise avant retrait.", false);
                MessageBox.Show("Signature manager requise avant retrait.");
                txtScanCarte.Focus();
                return;

            }

            string devise = (cmbDevise.Text ?? "CDF").Trim().ToUpperInvariant();
            if (devise != "CDF" && devise != "USD") devise = "CDF";
            decimal montant = ParseMoney(txtMontantAUtiliser.Text);

            if (montant <= 0)
            {
                SetMessage("Montant invalide.", false);
                MessageBox.Show("Montant invalide.");
                txtMontantAUtiliser.Focus();
                txtMontantAUtiliser.SelectAll();
                return;
            }

            try
            {
                using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();
                    using (var trans = con.BeginTransaction())
                    {
                        if (_typeCourant == CarteType.Client)
                        {
                            using (var cmd = new SqlCommand("dbo.Loyalty_Utiliser", con, trans))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.Add("@IdClient", SqlDbType.Int).Value = _idClientCourant;

                                var p = cmd.Parameters.Add("@Montant", SqlDbType.Decimal);
                                p.Precision = 18; p.Scale = 2; p.Value = montant;

                                cmd.Parameters.Add("@RefVente", SqlDbType.Int).Value = DBNull.Value;
                                cmd.Parameters.Add("@Note", SqlDbType.NVarChar, 200).Value =
                                    $"Retrait fidélité | Carte={_codeCarteCourant} | Autorisé par {_managerNom}({_managerId})";

                                cmd.ExecuteNonQuery();
                            }
                        }
                        else if (_typeCourant == CarteType.Partenaire)
                        {
                            using (var cmd = new SqlCommand("dbo.PartenairesFidelite_Utiliser", con, trans))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.Add("@IdPartenaire", SqlDbType.Int).Value = _idPartenaireCourant;
                                cmd.Parameters.Add("@IdVente", SqlDbType.Int).Value = DBNull.Value;
                                cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = devise;

                                var p = cmd.Parameters.Add("@MontantUtilise", SqlDbType.Decimal);
                                p.Precision = 18;
                                p.Scale = 2;
                                p.Value = montant;

                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                    }
                }

                    ConfigSysteme.AjouterAuditLog(
                    "Fidélité Retrait",
                     $"{_typeCourant} | Carte={_codeCarteCourant} | {montant} {devise} | Autorisé par Manager={_managerNom}({_managerId})",
                     "Succès"
                       );

                    SetMessage("✅ Retrait appliqué avec succès.", true);
                MessageBox.Show("✅ Retrait appliqué.");

                // Recharge selon type
                if (_typeCourant == CarteType.Client)
                    ChargerClientParCarte(_codeCarteCourant);
                else
                    ChargerPartenaireParCarte(_codeCarteCourant);

                txtMontantAUtiliser.Clear();
                txtScanCarte.Clear();
                ResetSignatureCache();
                txtScanCarte.Focus();
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog(
                    "Fidélité Retrait",
                    $"Erreur retrait fidélité | {_typeCourant} | {ex.Message}",
                    "Échec"
                );

                SetMessage("Erreur : " + ex.Message, false);
                MessageBox.Show("Erreur : " + ex.Message);

                txtMontantAUtiliser.Focus();
                txtMontantAUtiliser.SelectAll();
            }
        }
    }
}
