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
    public partial class FormFournisseurs : Form
    {
        private readonly string _cs = ConfigSysteme.ConnectionString;

        // évite boucle SelectionChanged quand on remplit les champs
        private bool _filling = false;

        public FormFournisseurs()
        {
            InitializeComponent(); // ✅ UNE SEULE FOIS

            // Norme "FormBase"
            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;

            Load += FormFournisseurs_Load;

            // ✅ UX : clic + clavier
            dgvFournisseurs.CellClick += dgvFournisseurs_CellClick;
            dgvFournisseurs.SelectionChanged += dgvFournisseurs_SelectionChanged;

            // ✅ Filtre liste
            chkAfficherActifs.CheckedChanged += (s, e) => ChargerFournisseurs();

            // Actions
            btnAjouter.Click += btnAjouter_Click;
            btnModifier.Click += btnModifier_Click;
            btnSupprimer.Click += btnSupprimer_Click;
            btnNouveau.Click += btnNouveau_Click;

            // ✅ Fermer : un seul branchement (supprime btnFermer_Click vide)
            btnFermer.Click += (s, e) => Close();
        }

        private void FormFournisseurs_Load(object sender, EventArgs e)
        {
            ConfigurerDgv();

            // ✅ Valeurs par défaut
            chkActif.Checked = true;          // état du fournisseur (édition)
            chkAfficherActifs.Checked = true; // filtre liste

            ChargerFournisseurs();

            RafraichirLangue();
            RafraichirTheme();
        }

        private void RafraichirLangue() => ConfigSysteme.AppliquerTraductions(this);
        private void RafraichirTheme() => ConfigSysteme.AppliquerTheme(this);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }

        private void ConfigurerDgv()
        {
            dgvFournisseurs.AutoGenerateColumns = true;
            dgvFournisseurs.ReadOnly = true;
            dgvFournisseurs.AllowUserToAddRows = false;
            dgvFournisseurs.AllowUserToDeleteRows = false;
            dgvFournisseurs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFournisseurs.MultiSelect = false;
            dgvFournisseurs.ScrollBars = ScrollBars.Both;
            dgvFournisseurs.RowHeadersVisible = false;
        }

        // ✅ C) ChargerFournisseurs avec filtre
        private void ChargerFournisseurs()
        {
            bool onlyActifs = chkAfficherActifs.Checked;

            using (SqlConnection con = new SqlConnection(_cs))
            {
                string sql = @"
SELECT
    ID_Fournisseur,
    Nom,
    Contact,
    Telephone,
    Email,
    Adresse,
    Actif,
    DateCreation
FROM dbo.Fournisseur
WHERE (@onlyActifs = 0 OR Actif = 1)
ORDER BY Nom;";

                using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
                {
                    da.SelectCommand.Parameters.Add("@onlyActifs", SqlDbType.Bit).Value = onlyActifs;

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvFournisseurs.DataSource = null;
                    dgvFournisseurs.DataSource = dt;
                }
            }

            if (dgvFournisseurs.Columns.Contains("ID_Fournisseur"))
                dgvFournisseurs.Columns["ID_Fournisseur"].Visible = false;

            if (dgvFournisseurs.Columns.Contains("Adresse"))
                dgvFournisseurs.Columns["Adresse"].Width = 220;

            if (dgvFournisseurs.Rows.Count > 0)
            {
                dgvFournisseurs.ClearSelection();
                dgvFournisseurs.Rows[0].Selected = true;

                // ✅ place un CurrentCell visible pour déclencher selection et navigation
                var firstVisibleCell = dgvFournisseurs.Rows[0].Cells.Cast<DataGridViewCell>()
                    .FirstOrDefault(c => c.Visible);
                if (firstVisibleCell != null)
                    dgvFournisseurs.CurrentCell = firstVisibleCell;

                RemplirDepuisSelection();
            }
            else
            {
                btnNouveau_Click(null, null);
            }
        }

        private int GetSelectedFournisseurId()
        {
            if (dgvFournisseurs.CurrentRow == null)
                return 0;

            object v = dgvFournisseurs.CurrentRow.Cells["ID_Fournisseur"]?.Value;
            if (v == null || v == DBNull.Value) return 0;

            return int.TryParse(v.ToString(), out int id) ? id : 0;
        }

        // ✅ B) Remplissage robuste
        private void dgvFournisseurs_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            RemplirDepuisRow(dgvFournisseurs.Rows[e.RowIndex]);
        }

        private void dgvFournisseurs_SelectionChanged(object sender, EventArgs e)
        {
            RemplirDepuisSelection();
        }

        private void RemplirDepuisSelection()
        {
            if (_filling) return;
            if (dgvFournisseurs.CurrentRow == null) return;

            RemplirDepuisRow(dgvFournisseurs.CurrentRow);
        }

        private void RemplirDepuisRow(DataGridViewRow row)
        {
            _filling = true;
            try
            {
                txtNom.Text = row.Cells["Nom"]?.Value?.ToString() ?? "";
                txtContact.Text = row.Cells["Contact"]?.Value?.ToString() ?? "";
                txtTelephone.Text = row.Cells["Telephone"]?.Value?.ToString() ?? "";
                txtEmail.Text = row.Cells["Email"]?.Value?.ToString() ?? "";
                txtAdresse.Text = row.Cells["Adresse"]?.Value?.ToString() ?? "";

                // ✅ PRO : gère bit ET 0/1
                bool actif = row.Cells["Actif"]?.Value != DBNull.Value
                             && Convert.ToBoolean(row.Cells["Actif"].Value);

                chkActif.Checked = actif;
            }
            finally
            {
                _filling = false;
            }
        }

        private bool Valider()
        {
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                MessageBox.Show("Nom fournisseur requis");
                txtNom.Focus();
                return false;
            }
            return true;
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            if (!Valider()) return;

            try
            {
                using (SqlConnection con = new SqlConnection(_cs))
                {
                    con.Open();

                    string sql = @"
INSERT INTO dbo.Fournisseur(Nom, Contact, Telephone, Email, Adresse, Actif, DateCreation)
VALUES(@Nom, @Contact, @Tel, @Email, @Adresse, @Actif, GETDATE());";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@Nom", SqlDbType.NVarChar, 200).Value = txtNom.Text.Trim();
                        cmd.Parameters.Add("@Contact", SqlDbType.NVarChar, 200).Value = (txtContact.Text ?? "").Trim();
                        cmd.Parameters.Add("@Tel", SqlDbType.NVarChar, 50).Value = (txtTelephone.Text ?? "").Trim();
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = (txtEmail.Text ?? "").Trim();
                        cmd.Parameters.Add("@Adresse", SqlDbType.NVarChar, 300).Value = (txtAdresse.Text ?? "").Trim();
                        cmd.Parameters.Add("@Actif", SqlDbType.Bit).Value = chkActif.Checked;

                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog("Fournisseur", $"Ajout fournisseur: {txtNom.Text.Trim()}", "Succès");
                ChargerFournisseurs();
                btnNouveau_Click(null, null);
                MessageBox.Show("Fournisseur ajouté ✅");
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                MessageBox.Show("Ce fournisseur existe déjà (Nom en doublon).");
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Fournisseur", ex.Message, "Échec");
                MessageBox.Show("Erreur ajout : " + ex.Message);
            }
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            int id = GetSelectedFournisseurId();
            if (id <= 0)
            {
                MessageBox.Show("Sélectionnez un fournisseur à modifier.");
                return;
            }
            if (!Valider()) return;

            try
            {
                using (SqlConnection con = new SqlConnection(_cs))
                {
                    con.Open();

                    string sql = @"
UPDATE dbo.Fournisseur SET
    Nom=@Nom,
    Contact=@Contact,
    Telephone=@Tel,
    Email=@Email,
    Adresse=@Adresse,
    Actif=@Actif
WHERE ID_Fournisseur=@ID;";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@Nom", SqlDbType.NVarChar, 200).Value = txtNom.Text.Trim();
                        cmd.Parameters.Add("@Contact", SqlDbType.NVarChar, 200).Value = (txtContact.Text ?? "").Trim();
                        cmd.Parameters.Add("@Tel", SqlDbType.NVarChar, 50).Value = (txtTelephone.Text ?? "").Trim();
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = (txtEmail.Text ?? "").Trim();
                        cmd.Parameters.Add("@Adresse", SqlDbType.NVarChar, 300).Value = (txtAdresse.Text ?? "").Trim();
                        cmd.Parameters.Add("@Actif", SqlDbType.Bit).Value = chkActif.Checked;

                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog("Fournisseur", $"Modification fournisseur ID={id} | Nom={txtNom.Text.Trim()}", "Succès");
                ChargerFournisseurs();
                MessageBox.Show("Fournisseur modifié ✅");
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                MessageBox.Show("Impossible : Nom fournisseur déjà utilisé.");
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Fournisseur", ex.Message, "Échec");
                MessageBox.Show("Erreur modification : " + ex.Message);
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            int id = GetSelectedFournisseurId();
            if (id <= 0)
            {
                MessageBox.Show("Sélectionnez un fournisseur.");
                return;
            }

            if (MessageBox.Show("Désactiver ce fournisseur (recommandé) ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection con = new SqlConnection(_cs))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE dbo.Fournisseur SET Actif=0 WHERE ID_Fournisseur=@id;", con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog("Fournisseur", $"Désactivation fournisseur ID={id}", "Succès");
                ChargerFournisseurs();
                btnNouveau_Click(null, null);
                MessageBox.Show("Fournisseur désactivé ✅");
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Fournisseur", ex.Message, "Échec");
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }


        private void btnNouveau_Click(object sender, EventArgs e)
        {

            txtNom.Clear();
            txtContact.Clear();
            txtTelephone.Clear();
            txtEmail.Clear();
            txtAdresse.Clear();
            chkActif.Checked = true;
            txtNom.Focus();
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
