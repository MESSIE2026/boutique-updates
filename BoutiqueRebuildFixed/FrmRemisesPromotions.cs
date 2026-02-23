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
    public partial class FrmRemisesPromotions : FormBase
    {
        public FrmRemisesPromotions()
        {
            InitializeComponent();
            
            ChargerTypesRemise();
            ChargerProduits();
            ChargerPromotions();


            // Écoute les changements globaux
            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        private void FrmRemisesPromotions_Load(object sender, EventArgs e)
        {
            // Charger traductions dynamiques

            // Appliquer AU CHARGEMENT
            RafraichirLangue();
            RafraichirTheme();
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
        private void ChargerProduits()
        {
            cboProduits.Items.Clear();

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(
                    "SELECT NomProduit FROM Produit ORDER BY NomProduit", con);

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    cboProduits.Items.Add(dr["NomProduit"].ToString());
                }
            }
        }
        private void dgvPromotions_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            txtNomPromotion.Text = dgvPromotions.CurrentRow.Cells["NomPromotion"].Value.ToString();
            cboTypeRemise.Text = dgvPromotions.CurrentRow.Cells["TypeRemise"].Value.ToString();
            nudValeur.Value = Convert.ToDecimal(dgvPromotions.CurrentRow.Cells["ValeurRemise"].Value);
            dtDebut.Value = Convert.ToDateTime(dgvPromotions.CurrentRow.Cells["DateDebut"].Value);
            dtFin.Value = Convert.ToDateTime(dgvPromotions.CurrentRow.Cells["DateFin"].Value);
            txtCreerPar.Text = dgvPromotions.CurrentRow.Cells["CreePar"].Value.ToString();
        }
        private void ChargerPromotions()
        {
            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                SqlDataAdapter da = new SqlDataAdapter(@"
SELECT TOP (1000)
    IdPromotion,
    NomPromotion,
    TypeRemise,
    ValeurRemise,
    DateDebut,
    DateFin,
    CreePar,
    DateCreation
FROM Promotions
ORDER BY DateCreation DESC", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvPromotions.DataSource = dt;
            }

            // Mise en forme colonnes
            dgvPromotions.Columns["IdPromotion"].Visible = false;

            dgvPromotions.Columns["NomPromotion"].HeaderText = "Promotion";
            dgvPromotions.Columns["TypeRemise"].HeaderText = "Type";
            dgvPromotions.Columns["ValeurRemise"].HeaderText = "Valeur";
            dgvPromotions.Columns["DateDebut"].HeaderText = "Début";
            dgvPromotions.Columns["DateFin"].HeaderText = "Fin";
            dgvPromotions.Columns["CreePar"].HeaderText = "Créé par";
            dgvPromotions.Columns["DateCreation"].HeaderText = "Création";

            dgvPromotions.Columns["ValeurRemise"].DefaultCellStyle.Format = "N2";
            dgvPromotions.Columns["DateDebut"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dgvPromotions.Columns["DateFin"].DefaultCellStyle.Format = "dd/MM/yyyy";
            dgvPromotions.Columns["DateCreation"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }
        private void ChargerTypesRemise()
        {
            cboTypeRemise.Items.Add("Remise en Pourcentage");
            cboTypeRemise.Items.Add("Montant Fixe");
            cboTypeRemise.SelectedIndex = 0;
        }
        private void ViderFormulaire()
        {
            txtNomPromotion.Clear();
            cboTypeRemise.SelectedIndex = -1;
            nudValeur.Value = 0;
            dtDebut.Value = DateTime.Today;
            dtFin.Value = DateTime.Today;
            txtCreerPar.Clear();
            lstProduits.Items.Clear();
        }


        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            // VALIDATIONS
            if (string.IsNullOrWhiteSpace(txtNomPromotion.Text)) return;
            if (string.IsNullOrWhiteSpace(cboTypeRemise.Text)) return;
            if (nudValeur.Value <= 0) return;
            if (dtFin.Value.Date < dtDebut.Value.Date) return;
            if (lstProduits.Items.Count == 0) return;
            if (string.IsNullOrWhiteSpace(txtCreerPar.Text)) return;

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                SqlTransaction tr = con.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand(@"
INSERT INTO Promotions
(NomPromotion, TypeRemise, ValeurRemise, DateDebut, DateFin, CreePar, DateCreation)
VALUES
(@Nom, @Type, @Valeur, @Debut, @Fin, @CreePar, GETDATE());
SELECT SCOPE_IDENTITY();", con, tr);

                    cmd.Parameters.AddWithValue("@Nom", txtNomPromotion.Text);
                    cmd.Parameters.AddWithValue("@Type", cboTypeRemise.Text);
                    cmd.Parameters.AddWithValue("@Valeur", nudValeur.Value);
                    cmd.Parameters.AddWithValue("@Debut", dtDebut.Value);
                    cmd.Parameters.AddWithValue("@Fin", dtFin.Value);
                    cmd.Parameters.AddWithValue("@CreePar", txtCreerPar.Text);

                    int idPromotion = Convert.ToInt32(cmd.ExecuteScalar());

                    foreach (string produit in lstProduits.Items)
                    {
                        SqlCommand cmdP = new SqlCommand(@"
INSERT INTO PromotionProduits
(IdPromotion, NomProduit)
VALUES (@IdPromotion, @NomProduit)", con, tr);

                        cmdP.Parameters.AddWithValue("@IdPromotion", idPromotion);
                        cmdP.Parameters.AddWithValue("@NomProduit", produit);
                        cmdP.ExecuteNonQuery();
                    }

                    tr.Commit();

                    MessageBox.Show("Promotion enregistrée avec succès");

                    ChargerPromotions();
                    ViderFormulaire();
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    MessageBox.Show("Erreur lors de l'enregistrement.\n" + ex.Message);
                }
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            txtNomPromotion.Clear();
            nudValeur.Value = 0;
            lstProduits.Items.Clear();
            cboTypeRemise.SelectedIndex = 0;
        }

        private void btnRetirer_Click(object sender, EventArgs e)
        {
            if (lstProduits.SelectedItem != null)
                lstProduits.Items.Remove(lstProduits.SelectedItem);
            AuditLogger.Log("DELETE", "Suppression client ID=45");
        }

        private void btnEffacer_Click(object sender, EventArgs e)
        {
            lstProduits.Items.Clear();
            AuditLogger.Log("DELETE", "Suppression client ID=45");
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            if (cboProduits.SelectedItem == null)
                return;

            string produit = cboProduits.SelectedItem.ToString();

            if (!lstProduits.Items.Contains(produit))
                lstProduits.Items.Add(produit);
        }
    }
}
