using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FormGestionFournisseursAchats : Form
    {
        private readonly string _cs = ConfigSysteme.ConnectionString;
        private bool _fillingFournisseur = false; // évite boucle SelectedIndexChanged
        public FormGestionFournisseursAchats()
        {
            InitializeComponent();

            ConfigurerDataGridViews();

            dgvHistoriqueAchats.SelectionChanged -= dgvHistoriqueAchats_SelectionChanged;
            dgvHistoriqueAchats.SelectionChanged += dgvHistoriqueAchats_SelectionChanged;

            // ✅ Fournisseur combo
            cmbNomFournisseur.SelectedIndexChanged -= cmbNomFournisseur_SelectedIndexChanged;
            cmbNomFournisseur.SelectedIndexChanged += cmbNomFournisseur_SelectedIndexChanged;

            ChargerHistoriqueAchats();

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        private void FormGestionFournisseursAchats_Load(object sender, EventArgs e)
        {
            cmbDevise.Items.Clear();
            cmbDevise.Items.AddRange(new object[] { "CDF", "USD", "EUR", "ZAR" });
            cmbDevise.SelectedIndex = 0;

            ConfigSysteme.AppliquerTraductions(this);
            ConfigSysteme.AppliquerTheme(this);

            RafraichirLangue();
            RafraichirTheme();

            // ✅ Empêche modification manuelle des infos fournisseur (car auto)
            txtContact.ReadOnly = true;
            txtTelephone.ReadOnly = true;
            txtEmail.ReadOnly = true;
            txtAdresse.ReadOnly = true;

            // ✅ Charger fournisseurs dans le ComboBox
            ChargerComboFournisseurs();
        }

        private void RafraichirLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);
        }

        private void RafraichirTheme()
        {
            ConfigSysteme.AppliquerTheme(this);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 🔥 OBLIGATOIRE : éviter fuite mémoire
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;

            base.OnFormClosed(e);
        }

        private void ChargerComboFournisseurs()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_cs))
                {
                    string sql = @"
SELECT ID_Fournisseur, Nom, Contact, Telephone, Email, Adresse
FROM dbo.Fournisseur
WHERE Actif = 1
ORDER BY Nom;";

                    using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        _fillingFournisseur = true;

                        cmbNomFournisseur.DisplayMember = "Nom";
                        cmbNomFournisseur.ValueMember = "ID_Fournisseur";
                        cmbNomFournisseur.DataSource = dt;

                        cmbNomFournisseur.SelectedIndex = -1; // rien sélectionné au départ
                    }
                }
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Chargement Fournisseurs (Combo)", ex.Message, "Échec");
                MessageBox.Show("Erreur chargement fournisseurs : " + ex.Message);
            }
            finally
            {
                _fillingFournisseur = false;
            }
        }

        private void cmbNomFournisseur_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_fillingFournisseur) return;
            if (cmbNomFournisseur.SelectedIndex < 0) return;

            if (!(cmbNomFournisseur.SelectedItem is DataRowView drv)) return;

            txtContact.Text = drv["Contact"]?.ToString() ?? "";
            txtTelephone.Text = drv["Telephone"]?.ToString() ?? "";
            txtEmail.Text = drv["Email"]?.ToString() ?? "";
            txtAdresse.Text = drv["Adresse"]?.ToString() ?? "";
        }

        private void ConfigurerDataGridViews()
        {
            // HISTORIQUE ACHATS
            dgvHistoriqueAchats.AutoGenerateColumns = true;
            dgvHistoriqueAchats.ReadOnly = true;
            dgvHistoriqueAchats.AllowUserToAddRows = false;
            dgvHistoriqueAchats.AllowUserToDeleteRows = false;
            dgvHistoriqueAchats.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistoriqueAchats.MultiSelect = false;
            dgvHistoriqueAchats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvHistoriqueAchats.ScrollBars = ScrollBars.Both;
            dgvHistoriqueAchats.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvHistoriqueAchats.RowTemplate.Height = 22;  // ligne hauteur standard

            // DETAILS COMMANDE
            dgvDetailsCommande.AutoGenerateColumns = true;
            dgvDetailsCommande.ReadOnly = true;
            dgvDetailsCommande.AllowUserToAddRows = false;
            dgvDetailsCommande.AllowUserToDeleteRows = false;
            dgvDetailsCommande.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetailsCommande.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDetailsCommande.ScrollBars = ScrollBars.Both;
            dgvDetailsCommande.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvDetailsCommande.RowTemplate.Height = 22;  // ligne hauteur standard
        }
        // =========================
        // CHARGEMENT HISTORIQUE
        // =========================
        // Chargement de l'historique des achats dans dgvHistoriqueAchats
        private void ChargerHistoriqueAchats()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    string sql = @"
SELECT 
    ID_Achat,
    ReferenceCommande,
    NomFournisseur,
    Contact,
    Telephone,
    Email,
    Adresse,
    DateAchat,
    ModePaiement,
    MontantTotal,
    Devise,
    Statut,
    DateCreation
FROM HistoriquesAchat
ORDER BY DateAchat DESC";

                    SqlDataAdapter da = new SqlDataAdapter(sql, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvHistoriqueAchats.DataSource = null;
                    dgvHistoriqueAchats.DataSource = dt;

                    if (dgvHistoriqueAchats.Columns.Contains("ID_Achat"))
                        dgvHistoriqueAchats.Columns["ID_Achat"].Visible = false;

                    if (dgvHistoriqueAchats.Columns.Contains("Adresse"))
                        dgvHistoriqueAchats.Columns["Adresse"].Width = 200;

                    if (dgvHistoriqueAchats.Rows.Count > 0)
                    {
                        dgvHistoriqueAchats.ClearSelection();
                        dgvHistoriqueAchats.Rows[0].Selected = true; // déclenche SelectionChanged -> charge détails
                    }
                    else
                    {
                        dgvDetailsCommande.DataSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Chargement Historique Achats", ex.Message, "Échec");
                MessageBox.Show("Erreur chargement historique : " + ex.Message);
            }
        }

        private void ChargerDetailsCommande(int idAchat)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    string sql = @"
SELECT 
    ID_Detail,
    Produit,
    Quantite,
    PrixUnitaire,
    Total,
    Devise,
    DateCreation
FROM DetailsCommande
WHERE ID_Achat = @id
ORDER BY DateCreation";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idAchat;

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvDetailsCommande.DataSource = null;
                        dgvDetailsCommande.DataSource = dt;

                        if (dgvDetailsCommande.Columns.Contains("ID_Detail"))
                            dgvDetailsCommande.Columns["ID_Detail"].Visible = false;

                        if (dgvDetailsCommande.Rows.Count > 0)
                        {
                            dgvDetailsCommande.ClearSelection();
                            dgvDetailsCommande.Rows[0].Selected = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Chargement Détails Commande", ex.Message, "Échec");
                MessageBox.Show("Erreur chargement détails : " + ex.Message);
            }
        }
        // Gestion de la sélection dans dgvHistoriqueAchats pour charger les détails associés
        private void dgvHistoriqueAchats_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvHistoriqueAchats.SelectedRows.Count == 0) return;

            var cell = dgvHistoriqueAchats.SelectedRows[0].Cells["ID_Achat"].Value;
            if (cell == null || cell == DBNull.Value) return;

            int idAchat = Convert.ToInt32(cell);
            ChargerDetailsCommande(idAchat);
        }

        // Dans le constructeur du formulaire ou dans l'initialisation, abonne l'événement

        private void btnAjouterAchat_Click(object sender, EventArgs e)
        {
            // ===== VALIDATIONS =====
            if (string.IsNullOrWhiteSpace(txtRefCommande.Text) ||
    cmbNomFournisseur.SelectedIndex < 0 ||
    string.IsNullOrWhiteSpace(txtMontantTotal.Text) ||
    string.IsNullOrWhiteSpace(txtNomProduit.Text) ||
    string.IsNullOrWhiteSpace(txtQuantite.Text) ||
    string.IsNullOrWhiteSpace(txtPrixUnitaire.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.");
                return;
            }

            if (cmbDevise.SelectedIndex == -1)
            {
                MessageBox.Show("Veuillez sélectionner une devise.");
                return;
            }

            if (!decimal.TryParse(txtMontantTotal.Text, out decimal montant))
            {
                MessageBox.Show("Montant total invalide.");
                return;
            }

            if (!int.TryParse(txtQuantite.Text, out int quantite) || quantite <= 0)
            {
                MessageBox.Show("Quantité invalide.");
                return;
            }

            if (!decimal.TryParse(txtPrixUnitaire.Text, out decimal prixUnitaire) || prixUnitaire <= 0)
            {
                MessageBox.Show("Prix unitaire invalide.");
                return;
            }

            // Calcul total ligne (utile pour affichage ou audit)
            decimal totalLigne = quantite * prixUnitaire;

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // ======================
                    // 1️⃣ INSERT ACHAT
                    // ======================
                    string sqlAchat = @"
INSERT INTO HistoriquesAchat
(ReferenceCommande, NomFournisseur, Contact, Telephone, Email, Adresse,
 DateAchat, ModePaiement, MontantTotal, Devise, Statut, DateCreation)
OUTPUT INSERTED.ID_Achat
VALUES
(@Ref,@Nom,@Contact,@Tel,@Email,@Adresse,
 @Date,@Mode,@Montant,@Devise,@Statut,GETDATE())";

                    using (SqlCommand cmdAchat = new SqlCommand(sqlAchat, con, tran))
                    {
                        cmdAchat.Parameters.AddWithValue("@Ref", txtRefCommande.Text.Trim());
                        cmdAchat.Parameters.Add("@Nom", SqlDbType.NVarChar, 200).Value = cmbNomFournisseur.Text.Trim();
                        cmdAchat.Parameters.AddWithValue("@Contact", txtContact.Text.Trim());
                        cmdAchat.Parameters.AddWithValue("@Tel", txtTelephone.Text.Trim());
                        cmdAchat.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmdAchat.Parameters.AddWithValue("@Adresse", txtAdresse.Text.Trim());
                        cmdAchat.Parameters.AddWithValue("@Date", dtpDateAchat.Value);
                        cmdAchat.Parameters.AddWithValue("@Mode", cmbModePaiement.Text);
                        cmdAchat.Parameters.AddWithValue("@Montant", montant);
                        cmdAchat.Parameters.AddWithValue("@Devise", cmbDevise.Text);
                        cmdAchat.Parameters.AddWithValue("@Statut", cmbStatut.Text);

                        int idAchat = Convert.ToInt32(cmdAchat.ExecuteScalar());

                        // ======================
                        // 2️⃣ INSERT DETAIL COMMANDE
                        // ======================
                        string sqlDetail = @"
INSERT INTO DetailsCommande
(ID_Achat, Produit, Quantite, PrixUnitaire, Total, Devise, DateCreation)
VALUES
(@ID, @Produit, @Qte, @Prix, (@Qte * @Prix), @Devise, GETDATE())";

                        using (SqlCommand cmdDetail = new SqlCommand(sqlDetail, con, tran))
                        {
                            cmdDetail.Parameters.Add("@ID", SqlDbType.Int).Value = idAchat;
                            cmdDetail.Parameters.Add("@Produit", SqlDbType.NVarChar, 200).Value = txtNomProduit.Text.Trim();
                            cmdDetail.Parameters.Add("@Qte", SqlDbType.Int).Value = quantite;

                            // ⚠️ Decimal: précision/scale selon ta colonne (ex: decimal(18,2))
                            var pPrix = cmdDetail.Parameters.Add("@Prix", SqlDbType.Decimal);
                            pPrix.Precision = 18;
                            pPrix.Scale = 2;
                            pPrix.Value = prixUnitaire;

                            cmdDetail.Parameters.Add("@Devise", SqlDbType.VarChar, 10).Value = cmbDevise.Text;

                            cmdDetail.ExecuteNonQuery();
                        }

                        // ======================
                        // COMMIT
                        // ======================
                        tran.Commit();

                        // AuditLog après commit
                        ConfigSysteme.AjouterAuditLog(
                            "Achat Fournisseur",
                            $"Achat {txtRefCommande.Text} | Produit={txtNomProduit.Text} | Quantité={quantite} | Prix Unitaire={prixUnitaire} | Total Ligne={totalLigne} | Devise={cmbDevise.Text} | Montant={montant}",
                            "Succès");

                        MessageBox.Show("Achat enregistré avec succès ✅");

                        // ======================
                        // RAFRAÎCHISSEMENT UI
                        // ======================
                        ChargerHistoriqueAchats();

                        if (dgvHistoriqueAchats.Rows.Count > 0)
                        {
                            dgvHistoriqueAchats.Rows[0].Selected = true;
                        }

                        ReinitialiserFormulaire();
                    }
                }
                catch (Exception ex)
                {
                    tran.Rollback();

                    ConfigSysteme.AjouterAuditLog(
                        "Achat Fournisseur",
                        ex.Message,
                        "Échec");

                    MessageBox.Show("Erreur : " + ex.Message);
                }
            }
        }

        // =========================
        // UTILS
        // =========================
        private void ReinitialiserFormulaire()
        {
            txtRefCommande.Clear();

            // ✅ fournisseur
            cmbNomFournisseur.SelectedIndex = -1;
            txtContact.Clear();
            txtTelephone.Clear();
            txtEmail.Clear();
            txtAdresse.Clear();

            txtMontantTotal.Clear();

            // ✅ champs produit
            txtNomProduit.Clear();
            txtQuantite.Clear();
            txtPrixUnitaire.Clear();

            cmbModePaiement.SelectedIndex = -1;
            cmbStatut.SelectedIndex = -1;

            if (cmbDevise.Items.Count > 0) cmbDevise.SelectedIndex = 0;

            dtpDateAchat.Value = DateTime.Today;
        }


        private void btnAnnulerAchat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            if (dgvHistoriqueAchats.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un achat à modifier.");
                return;
            }

            var cell = dgvHistoriqueAchats.SelectedRows[0].Cells["ID_Achat"].Value;
            if (cell == null || cell == DBNull.Value)
            {
                MessageBox.Show("ID Achat invalide.");
                return;
            }

            int idAchat = Convert.ToInt32(cell);

            // ✅ Entête seulement (pas les lignes détails)
            txtNomProduit.ReadOnly = true;
            txtQuantite.ReadOnly = true;
            txtPrixUnitaire.ReadOnly = true;

            // ✅ Valider fournisseur sélectionné
            if (cmbNomFournisseur.SelectedIndex < 0 || string.IsNullOrWhiteSpace(cmbNomFournisseur.Text))
            {
                MessageBox.Show("Sélectionnez un fournisseur.");
                return;
            }

            // ✅ Validation montant
            if (!decimal.TryParse(txtMontantTotal.Text, out decimal montant) || montant < 0)
            {
                MessageBox.Show("Montant total invalide.");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    string sql = @"
UPDATE HistoriquesAchat SET
    NomFournisseur = @Nom,
    Contact = @Contact,
    Telephone = @Tel,
    Email = @Email,
    Adresse = @Adresse,
    DateAchat = @Date,
    ModePaiement = @Mode,
    MontantTotal = @Montant,
    Devise = @Devise,
    Statut = @Statut
WHERE ID_Achat = @ID;";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = idAchat;

                        // ✅ Nom fournisseur depuis ComboBox
                        cmd.Parameters.Add("@Nom", SqlDbType.NVarChar, 200).Value = cmbNomFournisseur.Text.Trim();

                        // ✅ Champs auto (chargés depuis fournisseur)
                        cmd.Parameters.Add("@Contact", SqlDbType.NVarChar, 200).Value = (txtContact.Text ?? "").Trim();
                        cmd.Parameters.Add("@Tel", SqlDbType.NVarChar, 50).Value = (txtTelephone.Text ?? "").Trim();
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = (txtEmail.Text ?? "").Trim();
                        cmd.Parameters.Add("@Adresse", SqlDbType.NVarChar, 300).Value = (txtAdresse.Text ?? "").Trim();

                        cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = dtpDateAchat.Value;
                        cmd.Parameters.Add("@Mode", SqlDbType.NVarChar, 50).Value = cmbModePaiement.Text;

                        var pMontant = cmd.Parameters.Add("@Montant", SqlDbType.Decimal);
                        pMontant.Precision = 18;
                        pMontant.Scale = 2;
                        pMontant.Value = montant;

                        // ✅ Devise (si ta table a Devise)
                        cmd.Parameters.Add("@Devise", SqlDbType.VarChar, 10).Value = cmbDevise.Text;

                        cmd.Parameters.Add("@Statut", SqlDbType.NVarChar, 50).Value = cmbStatut.Text;

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog(
                    "Modification Achat",
                    $"Achat ID={idAchat} modifié | Fournisseur={cmbNomFournisseur.Text.Trim()} | Montant={montant} {cmbDevise.Text}",
                    "Succès"
                );

                MessageBox.Show("Achat modifié avec succès ✅");
                ChargerHistoriqueAchats();
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Modification Achat", ex.Message, "Échec");
                MessageBox.Show("Erreur modification : " + ex.Message);
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (dgvHistoriqueAchats.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un achat à supprimer.");
                return;
            }

            if (MessageBox.Show("Confirmer la suppression ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            int idAchat = Convert.ToInt32(
                dgvHistoriqueAchats.SelectedRows[0].Cells["ID_Achat"].Value);

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();

                    // Supprimer détails commande
                    SqlCommand cmd1 = new SqlCommand(
                        "DELETE FROM DetailsCommande WHERE ID_Achat=@id", con);
                    cmd1.Parameters.AddWithValue("@id", idAchat);
                    cmd1.ExecuteNonQuery();

                    // Supprimer achat
                    SqlCommand cmd2 = new SqlCommand(
                        "DELETE FROM HistoriquesAchat WHERE ID_Achat=@id", con);
                    cmd2.Parameters.AddWithValue("@id", idAchat);
                    cmd2.ExecuteNonQuery();
                }

                ConfigSysteme.AjouterAuditLog(
                    "Suppression Achat",
                    $"Achat ID={idAchat} supprimé",
                    "Succès");

                MessageBox.Show("Achat supprimé 🗑️");
                ChargerHistoriqueAchats();
                dgvDetailsCommande.DataSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur suppression : " + ex.Message);
            }
        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            if (dgvHistoriqueAchats.Rows.Count == 0)
            {
                MessageBox.Show("Aucune donnée à exporter.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Fichier CSV (*.csv)|*.csv",
                FileName = "HistoriqueAchats.csv"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            StringBuilder sb = new StringBuilder();

            // En-têtes
            foreach (DataGridViewColumn col in dgvHistoriqueAchats.Columns)
                sb.Append(col.HeaderText + ";");
            sb.AppendLine();

            // Lignes
            foreach (DataGridViewRow row in dgvHistoriqueAchats.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                    sb.Append(cell.Value?.ToString() + ";");
                sb.AppendLine();
            }

            System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);

            MessageBox.Show("Export terminé 📤");
        }

        private void btnFermer1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnFermer2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

