using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using static BoutiqueRebuildFixed.FormMain;

namespace BoutiqueRebuildFixed
{
    public partial class FormCaisse : FormBase
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;

        public FormCaisse()
        {
            InitializeComponent();
            this.Load += FormCaisse_Load;

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        private void FormCaisse_Load(object sender, EventArgs e)
        {
            // Initialisation basique (clear avant ajout)
            cmbModePaiement.Items.Clear();

            // Items à ajouter : idéalement traduits via ConfigSysteme
            cmbModePaiement.Items.AddRange(new string[] {
        ConfigSysteme.Traduire("cbEspèces"),
        ConfigSysteme.Traduire("cbCarte"),
        ConfigSysteme.Traduire("cbMobileMoney")
    });

            dtpDateTransaction.Value = DateTime.Now;
            numQuantite.Value = 1;

            
            ConfigurerDgvCaisse();
            ChargerCaisse();

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
        private void AppliquerTheme(string theme)
        {
            Color backColor;
            Color foreColor;

            if (theme == "Sombre")
            {
                backColor = Color.FromArgb(30, 30, 30);
                foreColor = Color.White;
            }
            else
            {
                backColor = SystemColors.Control;
                foreColor = Color.Black;
            }

            this.BackColor = backColor;
            this.ForeColor = foreColor;

            // Appliquer aux contrôles enfants
            foreach (Control ctrl in this.Controls)
            {
                if (!(ctrl is Button))  // ou selon ta logique (par exemple, boutons gardent leur style)
                {
                    ctrl.BackColor = backColor;
                    ctrl.ForeColor = foreColor;
                }
            }
        }
        private void InitialiserControles()
        {
            cmbModePaiement.Items.Clear();
            cmbModePaiement.Items.AddRange(new string[]
            {
                ConfigSysteme.Traduire("Espèces"),
                ConfigSysteme.Traduire("Carte"),
                ConfigSysteme.Traduire("Mobile Money")
            });

            dtpDateTransaction.Value = DateTime.Now;
            numQuantite.Value = 1;

            numQuantite.ValueChanged += (_, __) => CalculerMontant();
            txtPrix.TextChanged += (_, __) => CalculerMontant();
        }
        private void ConfigurerDgvCaisse()
        {
            dgvCaisse.ReadOnly = true;
            dgvCaisse.AllowUserToAddRows = false;
            dgvCaisse.AllowUserToDeleteRows = false;

            dgvCaisse.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCaisse.MultiSelect = false;

            dgvCaisse.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvCaisse.ScrollBars = ScrollBars.Both; // ✅ Barres droite & bas

            dgvCaisse.RowHeadersVisible = true;
        }

        // ================== CALCUL ==================
        private void CalculerMontant()
        {
            if (decimal.TryParse(txtPrix.Text, out decimal prix))
            {
                decimal montant = prix * numQuantite.Value;
                txtMontantTotal.Text = montant.ToString("F2");
            }
            else
            {
                txtMontantTotal.Text = "0.00";
            }
        }
        private void numQuantite_ValueChanged(object sender, EventArgs e)
        {
            CalculerMontantTotal();
        }

        private void txtPrix_TextChanged(object sender, EventArgs e)
        {
            CalculerMontantTotal();
        }

        private void CalculerMontantTotal()
        {
            decimal qte = numQuantite.Value;
            if (decimal.TryParse(txtPrix.Text, out decimal prix))
            {
                txtMontantTotal.Text = (qte * prix).ToString("F2");
            }
            else
            {
                txtMontantTotal.Text = "0.00";
            }
        }
        private void ChargerCaisse()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();

                    string sql = @"SELECT 
                ID,
                DateTransaction,
                ReferenceProduit,
                NomProduit,
                Quantite,
                PrixUnitaire,
                MontantTotal,
                ModePaiement,
                NomClient,
                IDEmploye
            FROM Caisse
            ORDER BY DateTransaction DESC";

                    SqlDataAdapter da = new SqlDataAdapter(sql, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvCaisse.DataSource = null;      // 🔴 IMPORTANT
                    dgvCaisse.AutoGenerateColumns = true;
                    dgvCaisse.DataSource = dt;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement Caisse : " + ex.Message);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            decimal montantTotal = 0; // ✅ DÉCLARATION ICI

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();

                    decimal prix = decimal.Parse(txtPrix.Text);
                    int quantite = (int)numQuantite.Value;
                    montantTotal = prix * quantite; // ✅ AFFECTATION

                    string sql = @"
INSERT INTO Caisse
(DateTransaction, ReferenceProduit, NomProduit, Quantite,
 PrixUnitaire, MontantTotal, ModePaiement, NomClient, IDEmploye)
VALUES
(@date, @ref, @nom, @qte, @prix, @total, @mode, @client, @idEmp)";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@date", dtpDateTransaction.Value);
                        cmd.Parameters.AddWithValue("@ref", txtReference.Text);
                        cmd.Parameters.AddWithValue("@nom", txtNomProduit.Text);
                        cmd.Parameters.AddWithValue("@qte", quantite);
                        cmd.Parameters.AddWithValue("@prix", prix);
                        cmd.Parameters.AddWithValue("@total", montantTotal);
                        cmd.Parameters.AddWithValue("@mode", cmbModePaiement.Text);
                        cmd.Parameters.AddWithValue("@client", txtNomClient.Text);
                        cmd.Parameters.AddWithValue("@idEmp", int.Parse(txtIDEmploye.Text));

                        cmd.ExecuteNonQuery();
                    }
                }

                // ✅ AUDIT LOG — SUCCÈS
                ConfigSysteme.AjouterAuditLog(
                    "Transaction Caisse",
                    $"Produit={txtNomProduit.Text} | Ref={txtReference.Text} | Qte={numQuantite.Value} | Total={montantTotal}",
                    "Succès"
                );

                MessageBox.Show("Transaction enregistrée !");
                ChargerCaisse();
            }
            catch (Exception ex)
            {
                // ❌ AUDIT LOG — ERREUR
                ConfigSysteme.AjouterAuditLog(
                    "Transaction Caisse",
                    $"Échec | Produit={txtNomProduit.Text} | Erreur={ex.Message}",
                    "Erreur"
                );

                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (dgvCaisse.CurrentRow == null) return;

            int id = Convert.ToInt32(dgvCaisse.CurrentRow.Cells["ID"].Value);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Caisse WHERE ID=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            ChargerCaisse();
        }

        // ================== LOAD DGV ==================
        

        // ================== DGV CONFIG ==================
       

        // ================== THEME ==================

        // ================== LANGUE ==================
        public void AppliquerLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);
        }

        // ================== UTILS ==================
        private void ViderChamps()
        {
            txtReference.Clear();
            txtNomProduit.Clear();
            txtPrix.Clear();
            txtMontantTotal.Clear();
            txtNomClient.Clear();
            numQuantite.Value = 1;
        }


        private void btnDetails_Click(object sender, EventArgs e)
        {
            {
                MessageBox.Show("Fonction Détails à implémenter");
            }
        }

        private void btnSupprimer_Click_1(object sender, EventArgs e)
        {
            

        }
    }

}
