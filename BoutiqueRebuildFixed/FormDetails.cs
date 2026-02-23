using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Org.BouncyCastle.Asn1.Cmp;

namespace BoutiqueRebuildFixed
{
    public partial class FormDetails : FormBase
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        private int _idVenteCourante;


        public FormDetails(int idVente)
        {
            InitializeComponent();

            if (idVente <= 0)
            {
                MessageBox.Show("FormDetails ouvert sans ID_Vente valide (idVente=" + idVente + ")");
                Close();
                return;
            }


            _idVenteCourante = idVente;

            this.Load += FormDetails_Load;

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;

            var btnRetourProduit = new Button
            {
                Text = "Retour Produit",
                Width = btnOk.Width,
                Height = btnOk.Height,
                Left = btnOk.Left,
                Top = btnOk.Top - btnOk.Height - 8,
                Anchor = btnOk.Anchor
            };
            btnRetourProduit.Click += btnRetourProduit_Click;
            this.Controls.Add(btnRetourProduit);
            btnRetourProduit.BringToFront();
        }

        private void FormDetails_Load(object sender, EventArgs e)
        {
            // Appliquer la configuration (langue, thème, menu contextuel)
            AppliquerConfigSysteme();


            MessageBox.Show("DEBUG: _idVenteCourante=" + _idVenteCourante);

            txtIDVente.Text = _idVenteCourante.ToString();
            ChargerDetailsVente();

            // Initialisation combo (comme tu avais)
            cmbCategorie.Items.AddRange(new string[]
            {
                "Vêtements", "Chaussures", "Accessoires", "Chaise", "Outils Travail", "Maisons", "Produits"
            });

            cmbTaille.Items.AddRange(new string[]
            {
                "S", "M", "L", "XL", "XXL"
            });

            dtpDateAjout.Value = DateTime.Now;
            numQuantite.Value = 1;

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
        private void AppliquerConfigSysteme()
        {
            // Thème simple
            if (ConfigSysteme.Theme == "Sombre")
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is DataGridView dgv)
                    {
                        dgv.BackgroundColor = Color.FromArgb(45, 45, 48);
                        dgv.ForeColor = Color.White;
                        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 28, 28);
                        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                        dgv.EnableHeadersVisualStyles = false;
                    }
                    else
                    {
                        ctrl.ForeColor = Color.White;
                        ctrl.BackColor = Color.FromArgb(30, 30, 30);
                    }
                }
            }
            else // Clair
            {
                this.BackColor = SystemColors.Control;
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is DataGridView dgv)
                    {
                        dgv.BackgroundColor = SystemColors.Window;
                        dgv.ForeColor = Color.Black;
                        dgv.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
                        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                        dgv.EnableHeadersVisualStyles = true;
                    }
                    else
                    {
                        ctrl.ForeColor = Color.Black;
                        ctrl.BackColor = SystemColors.Control;
                    }
                }
            }

            // Langue : exemple simple d'adaptation des textes
            if (ConfigSysteme.Langue == "en")
            {
                btnOk.Text = "OK";
                btnAnnuler.Text = "Cancel";
                btnSupprimer.Text = "Delete";
                btnChangerImage.Text = "Change Image";
                // etc...
            }
            else // Français
            {
                btnOk.Text = "OK";
                btnAnnuler.Text = "Annuler";
                btnSupprimer.Text = "Supprimer";
                btnChangerImage.Text = "Changer Image";
            }

            // Associer le menu contextuel ConfigSysteme.MenuContextuel au dgvDetailsVente
            dgvDetailsVente.ContextMenuStrip = ConfigSysteme.MenuContextuel;
        }

        private void ChargerDetailsVente()
        {
            try
            {
                int idVente = _idVenteCourante;

                if (idVente <= 0)
                {
                    MessageBox.Show("ID Vente manquant.");
                    return;
                }

                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT  ID_Details, ID_Vente, ID_Produit, Quantite, PrixUnitaire,
        RefProduit, NomProduit, Remise, TVA, Montant, Devise, NomCaissier,
        IdEntreprise, IdMagasin, IdPoste, QuantiteRetournee
FROM dbo.DetailsVente
WHERE ID_Vente = @idVente
ORDER BY ID_Details DESC;";

                    cmd.Parameters.Add("@idVente", SqlDbType.Int).Value = idVente;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvDetailsVente.DataSource = dt;
                        StyliserDgvDetails();
                        AppliquerLargeursColonnes();

                        // ===== UI PRO (colonnes + scrollbars) =====
                        dgvDetailsVente.ScrollBars = ScrollBars.Both;

                        dgvDetailsVente.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        dgvDetailsVente.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

                        dgvDetailsVente.RowTemplate.Height = 28; // hauteur ligne normale
                        dgvDetailsVente.ColumnHeadersHeight = 32;

                        dgvDetailsVente.AllowUserToResizeColumns = true;
                        dgvDetailsVente.AllowUserToResizeRows = false;

                        dgvDetailsVente.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        dgvDetailsVente.MultiSelect = false;

                        dgvDetailsVente.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
                        dgvDetailsVente.AutoGenerateColumns = true;

                        // ✅ Largeurs "normales" (lisibles)
                        foreach (DataGridViewColumn col in dgvDetailsVente.Columns)
                        {
                            col.MinimumWidth = 80;
                            col.Width = 130; // largeur par défaut
                        }

                        // ✅ Colonnes spécifiques plus larges
                        if (dgvDetailsVente.Columns.Contains("NomProduit"))
                            dgvDetailsVente.Columns["NomProduit"].Width = 260;

                        if (dgvDetailsVente.Columns.Contains("RefProduit"))
                            dgvDetailsVente.Columns["RefProduit"].Width = 140;

                        if (dgvDetailsVente.Columns.Contains("NomCaissier"))
                            dgvDetailsVente.Columns["NomCaissier"].Width = 160;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement détails vente : " + ex.Message);
            }
        }

        private void AppliquerLargeursColonnes()
        {
            foreach (DataGridViewColumn col in dgvDetailsVente.Columns)
            {
                col.MinimumWidth = 80;
                col.Width = 130;
            }

            if (dgvDetailsVente.Columns.Contains("NomProduit"))
                dgvDetailsVente.Columns["NomProduit"].Width = 260;

            if (dgvDetailsVente.Columns.Contains("RefProduit"))
                dgvDetailsVente.Columns["RefProduit"].Width = 140;

            if (dgvDetailsVente.Columns.Contains("NomCaissier"))
                dgvDetailsVente.Columns["NomCaissier"].Width = 160;
        }


        private void StyliserDgvDetails()
        {
            dgvDetailsVente.ScrollBars = ScrollBars.Both;

            dgvDetailsVente.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvDetailsVente.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            dgvDetailsVente.RowTemplate.Height = 28;
            dgvDetailsVente.ColumnHeadersHeight = 32;

            dgvDetailsVente.AllowUserToResizeColumns = true;
            dgvDetailsVente.AllowUserToResizeRows = false;

            dgvDetailsVente.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetailsVente.MultiSelect = false;

            dgvDetailsVente.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dgvDetailsVente.RowHeadersVisible = false;
            dgvDetailsVente.ReadOnly = true; // (si tu ne modifies pas dans la grille)
        }


        private void btnChangerImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                picProduit.Image = Image.FromFile(ofd.FileName);
                picProduit.ImageLocation = ofd.FileName;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                int idVente = _idVenteCourante;
                if (idVente <= 0) { MessageBox.Show("ID Vente manquant."); return; }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // ✅ Vérifier existence Vente (évite FK)
                    using (var check = new SqlCommand("SELECT COUNT(1) FROM dbo.Vente WHERE ID_Vente=@id", con))
                    {
                        check.Parameters.Add("@id", SqlDbType.Int).Value = idVente;
                        int ok = Convert.ToInt32(check.ExecuteScalar());
                        if (ok == 0)
                        {
                            MessageBox.Show("Cette vente n'existe pas en base. ID_Vente=" + idVente);
                            return;
                        }
                    }

                    string sql = @"
INSERT INTO dbo.DetailsVente
(ID_Vente, ID_Produit, Quantite, PrixUnitaire, RefProduit, NomProduit, Remise, TVA, Montant, Devise, NomCaissier)
VALUES
(@ID_Vente, @ID_Produit, @Quantite, @PrixUnitaire, @RefProduit, @NomProduit, @Remise, @TVA, @Montant, @Devise, @NomCaissier);";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@ID_Vente", SqlDbType.Int).Value = idVente;
                        cmd.Parameters.Add("@ID_Produit", SqlDbType.Int).Value = int.Parse(txtIDProduit.Text);
                        cmd.Parameters.Add("@Quantite", SqlDbType.Decimal).Value = numQuantite.Value;
                        cmd.Parameters.Add("@PrixUnitaire", SqlDbType.Decimal).Value = decimal.Parse(txtPrixUnitaire.Text);
                        cmd.Parameters.Add("@RefProduit", SqlDbType.NVarChar, 60).Value = txtRefProduit.Text ?? "";
                        cmd.Parameters.Add("@NomProduit", SqlDbType.NVarChar, 200).Value = txtNomProduit.Text ?? "";

                        decimal remise = decimal.TryParse(txtRemise.Text, out var r) ? r : 0m;
                        decimal tva = decimal.TryParse(txtTVA.Text, out var t) ? t : 0m;

                        cmd.Parameters.Add("@Remise", SqlDbType.Decimal).Value = remise;
                        cmd.Parameters.Add("@TVA", SqlDbType.Decimal).Value = tva;

                        decimal montant = (decimal.Parse(txtPrixUnitaire.Text) * (decimal)numQuantite.Value) - remise + tva;
                        cmd.Parameters.Add("@Montant", SqlDbType.Decimal).Value = montant;

                        cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = txtDevise.Text ?? "CDF";
                        cmd.Parameters.Add("@NomCaissier", SqlDbType.NVarChar, 80).Value = txtNomCaissier.Text ?? "";

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Détail de vente ajouté avec succès !");
                ChargerDetailsVente(); // ✅ refresh
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ajout : " + ex.Message);
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string sql = "DELETE FROM Produits WHERE Reference = @Reference";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Reference", txtRefProduit.Text);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                            MessageBox.Show("Produit supprimé !");
                        else
                            MessageBox.Show("Aucun produit trouvé avec cette référence.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fonction Détails à implémenter.");
        }

        private void btnRetourProduit_Click(object sender, EventArgs e)
        {
            if (dgvDetailsVente.CurrentRow == null)
            {
                MessageBox.Show("Sélectionne une ligne (produit) dans la liste.");
                return;
            }

            int idDetails = Convert.ToInt32(dgvDetailsVente.CurrentRow.Cells["ID_Details"].Value);
            int idVente = Convert.ToInt32(dgvDetailsVente.CurrentRow.Cells["ID_Vente"].Value);

            OuvrirAnnulationsDialog(idVente, idDetails);
        }

        private void OuvrirAnnulationsDialog(int idVente, int idDetails)
        {
            using (var dlg = new Form())
            {
                dlg.Text = "Annulation / Retour";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ShowInTaskbar = false;
                dlg.ClientSize = new Size(950, 620);

                using (var f = new FormAnnulations(idVente, idDetails))
                {
                    f.TopLevel = false;
                    f.FormBorderStyle = FormBorderStyle.None;
                    f.Dock = DockStyle.Fill;

                    dlg.Controls.Add(f);
                    f.Show();

                    dlg.ShowDialog(this);
                }
            }
        }

        private void picProduit_Click(object sender, EventArgs e)
        {

        }
    }
}

