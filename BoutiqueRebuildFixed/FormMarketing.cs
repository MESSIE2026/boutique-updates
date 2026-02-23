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

    public partial class FormMarketing : Form
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        private int selectedCampaignId = -1;
        private decimal montantVendu = 0m;
        private readonly MarketingRepository _repo;
        private readonly MarketingService _svc;

        private int _page = 1;
        private const int PageSize = 50;

        private Panel pnlFacebook;                 // le rectangle
        private TextBox txtFacebookCampaignId;     // champ ID campagne
        private Button btnFacebookSettings;        // ouvre FrmFacebookRapide
        private Button btnSaveFacebookCampaignId;  // enregistre ID campagne
        private Button btnSyncFacebookMini;        // sync
        public FormMarketing()
        {
            InitializeComponent();

            BuildFacebookPanelLeft();

            this.Load += FormMarketing_Load;

            _repo = new MarketingRepository(connectionString);
            _svc = new MarketingService(connectionString);

            ConfigurerDataGridView();
            InitialiserComboBoxType();
            InitialiserComboBoxStatut();
            InitialiserComboDevise();

            // anti-double safe
            cmbStatut.SelectedIndexChanged -= CmbStatut_SelectedIndexChanged;
            cmbStatut.SelectedIndexChanged += CmbStatut_SelectedIndexChanged;

            MettreAJourStatutCampagnes();
            ChargerCampagnes();

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        private void FormMarketing_Load(object sender, EventArgs e)
        {
            ConfigSysteme.AppliquerTraductions(this);
            ConfigSysteme.AppliquerTheme(this);

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

        private void BuildFacebookPanelLeft()
        {
            // Panel vertical à gauche
            pnlFacebook = new Panel
            {
                Dock = DockStyle.Left,
                Width = 270,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(0, 80, 80)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                AutoSize = false,
                Padding = new Padding(0, 6, 0, 6) // petit air en haut/bas
            };

            // Lignes (on augmente un peu pour respirer)
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); // Titre
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); // btn paramètres
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); // btn sync
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26)); // label campaign id
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // textbox campaign id
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); // btn enregistrer
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); // espace
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26)); // aide

            // petit helper : espace autour de chaque contrôle
            void Styliser(Control c)
            {
                c.Margin = new Padding(0, 6, 0, 0); // espace vertical
                                                    // si tu veux aussi un petit air à gauche/droite :
                                                    // c.Margin = new Padding(2, 6, 2, 0);
            }

            var lblTitle = new Label
            {
                Text = "Facebook / Meta",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Styliser(lblTitle);
            lblTitle.Margin = new Padding(0, 0, 0, 6); // titre: pas d’espace au-dessus, un peu en dessous

            btnFacebookSettings = new Button
            {
                Text = "Paramètres Facebook",
                Dock = DockStyle.Fill,
                Height = 30
            };
            Styliser(btnFacebookSettings);
            btnFacebookSettings.Click += (s, e) =>
            {
                using (var f = new FrmFacebookRapide())
                    f.ShowDialog(this);
            };

            btnSyncFacebookMini = new Button
            {
                Text = "Sync Facebook",
                Dock = DockStyle.Fill,
                Height = 30
            };
            Styliser(btnSyncFacebookMini);
            btnSyncFacebookMini.Click += btnSyncFacebook_Click;

            var lblCampId = new Label
            {
                Text = "Facebook Campaign ID :",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Styliser(lblCampId);

            txtFacebookCampaignId = new TextBox
            {
                Dock = DockStyle.Fill
            };
            Styliser(txtFacebookCampaignId);

            btnSaveFacebookCampaignId = new Button
            {
                Text = "Enregistrer ID",
                Dock = DockStyle.Fill,
                Height = 30
            };
            Styliser(btnSaveFacebookCampaignId);
            btnSaveFacebookCampaignId.Click += BtnSaveFacebookCampaignId_Click;

            var lblHelp = new Label
            {
                Text = "1) Paramètres  2) Colle ID  3) Enregistrer  4) Sync",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Styliser(lblHelp);
            lblHelp.Margin = new Padding(0, 2, 0, 0); // aide un peu plus bas

            table.Controls.Add(lblTitle, 0, 0);
            table.Controls.Add(btnFacebookSettings, 0, 1);
            table.Controls.Add(btnSyncFacebookMini, 0, 2);
            table.Controls.Add(lblCampId, 0, 3);
            table.Controls.Add(txtFacebookCampaignId, 0, 4);
            table.Controls.Add(btnSaveFacebookCampaignId, 0, 5);
            table.Controls.Add(lblHelp, 0, 7);

            pnlFacebook.Controls.Add(table);

            Controls.Add(pnlFacebook);
            pnlFacebook.BringToFront();
        }



        private decimal LireDecimal(string text)
{
    if (decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal val))
        return val;
    return 0m;
}
        private void InsererStatistiquesCampagne(int campagneId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
            INSERT INTO StatistiquesPublicites
            (CampagneId, DateVente, NomCampagne, Vues, Messages, Commentaires, Statut)
            SELECT 
                Id,
                GETDATE(),
                NomCampagne,
                Vues,
                ConversationsMessages,
                Commentaires,
                Statut
            FROM CampagnesMarketing
            WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", campagneId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur insertion statistiques : " + ex.Message);
            }
        }
        private int LireInt(string text)
        {
            if (int.TryParse(text, out int v)) return v;
            return 0;
        }

        private void InsererOuMettreAJourStatistiques(int campagneId, SqlConnection conn)
        {
            string sql = @"
IF EXISTS (SELECT 1 FROM StatistiquesPublicites WHERE CampagneId = @CampagneId)
BEGIN
    UPDATE StatistiquesPublicites SET
        NomCampagne     = @NomCampagne,
        Vues            = @Vues,
        Messages        = @Messages,
        Commentaires    = @Commentaires,
        Statut          = @Statut,
        Spectateurs     = @Spectateurs,
        BudgetQuotidien = @BudgetQuotidien,
        NombreVentes    = @NombreVentes,
        MontantVendus   = @MontantVendus,
        Devise          = @Devise
    WHERE CampagneId = @CampagneId
END
ELSE
BEGIN
    INSERT INTO StatistiquesPublicites
        (CampagneId, DateVente, NomCampagne, Vues, Messages, Commentaires, Statut,
         Spectateurs, BudgetQuotidien, NombreVentes, MontantVendus, Devise)
    VALUES
        (@CampagneId, GETDATE(), @NomCampagne, @Vues, @Messages, @Commentaires, @Statut,
         @Spectateurs, @BudgetQuotidien, @NombreVentes, @MontantVendus, @Devise)
END";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@CampagneId", SqlDbType.Int).Value = campagneId;
                cmd.Parameters.Add("@NomCampagne", SqlDbType.NVarChar, 150).Value = txtNomCampagne.Text.Trim();

                cmd.Parameters.Add("@Vues", SqlDbType.Int).Value = LireInt(txtVues.Text);
                cmd.Parameters.Add("@Messages", SqlDbType.Int).Value = LireInt(txtConversationsMessages.Text);
                cmd.Parameters.Add("@Commentaires", SqlDbType.NVarChar).Value = txtCommentaires.Text.Trim();
                cmd.Parameters.Add("@Statut", SqlDbType.NVarChar, 30).Value = cmbStatut.Text;

                cmd.Parameters.Add("@Spectateurs", SqlDbType.Int).Value = LireInt(txtSpectateurs.Text);
                cmd.Parameters.Add("@BudgetQuotidien", SqlDbType.Decimal).Value = LireDecimal(txtBudgetQuotidien.Text);

                cmd.Parameters.Add("@NombreVentes", SqlDbType.Int).Value = LireInt(txtNombreVentes.Text);
                cmd.Parameters.Add("@MontantVendus", SqlDbType.Decimal).Value = LireDecimal(txtMontantVendus.Text);
                cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = cbDevise.SelectedItem?.ToString() ?? "FC";

                cmd.ExecuteNonQuery();
            }
        }
        private void SupprimerStatistiquesSiExistent(int campagneId, SqlConnection conn)
        {
            string sql = "DELETE FROM StatistiquesPublicites WHERE CampagneId = @CampagneId";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CampagneId", campagneId);
                cmd.ExecuteNonQuery();
            }
        }
        private void ConfigurerDataGridView()
        {
            dgvMarketing.ReadOnly = true;
            dgvMarketing.AllowUserToAddRows = false;
            dgvMarketing.AllowUserToDeleteRows = false;
            dgvMarketing.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMarketing.MultiSelect = false;
            dgvMarketing.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMarketing.RowHeadersVisible = false;

            dgvMarketing.CellClick -= DgvMarketing_CellClick;
            dgvMarketing.CellClick += DgvMarketing_CellClick;
        }

        private void InitialiserComboBoxType()
        {
            cmbTypeCampagne.Items.Clear();
            cmbTypeCampagne.Items.AddRange(new string[]
            {
                "Emailing",
                "Publicité",
                "Événement",
                "Réseaux sociaux",
                "Autre"
            });
            cmbTypeCampagne.SelectedIndex = 0;
        }
        private void InitialiserComboDevise()
        {
            cbDevise.Items.Clear();
            cbDevise.Items.AddRange(new string[] { "FC", "$", "£", "€" });
            cbDevise.SelectedIndex = 0;

            cbDevise.SelectedIndexChanged -= cbDevise_SelectedIndexChanged;
            cbDevise.SelectedIndexChanged += cbDevise_SelectedIndexChanged;
        }

        private void cbDevise_SelectedIndexChanged(object sender, EventArgs e)
        {
            MettreAJourAffichageMontantVendu();
        }

        private void MettreAJourAffichageMontantVendu()
        {
            string deviseSelectionnee = cbDevise.SelectedItem?.ToString() ?? "FC";
            lblMontantVendus.Text = montantVendu.ToString("N2") + " " + deviseSelectionnee;
        }
        private void InitialiserComboBoxStatut()
        {
            cmbStatut.Items.Clear();
            cmbStatut.Items.AddRange(new string[]
            {
                "Planifiée",
                "En cours",
                "Terminée",
                "Annulée"
            });
            cmbStatut.SelectedIndex = 0;
        }


        private void ChargerCampagnes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
SELECT Id, NomCampagne, TypeCampagne, DateDebut, DateFin, Budget, Statut, Commentaires,
       ConversationsMessages, Vues, Spectateurs, BudgetQuotidien, FacebookCampaignId
FROM CampagnesMarketing
ORDER BY DateDebut DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvMarketing.DataSource = dt;
                    dgvMarketing.ClearSelection();


                    if (dgvMarketing.Columns.Contains("Id"))
                        dgvMarketing.Columns["Id"].Visible = false;
                    if (dgvMarketing.Columns.Contains("Commentaires"))
                        dgvMarketing.Columns["Commentaires"].Visible = false;
                    if (dgvMarketing.Columns.Contains("FacebookCampaignId"))
                        dgvMarketing.Columns["FacebookCampaignId"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement campagnes : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private bool VerifierChamps()
        {
            if (string.IsNullOrWhiteSpace(txtNomCampagne.Text))
            {
                MessageBox.Show("Le nom de la campagne est obligatoire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbTypeCampagne.SelectedIndex < 0)
            {
                MessageBox.Show("Veuillez sélectionner un type de campagne.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbStatut.SelectedIndex < 0)
            {
                MessageBox.Show("Veuillez sélectionner un statut.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (dateDebut.Value.Date > dateFin.Value.Date)
            {
                MessageBox.Show("La date de début doit être antérieure à la date de fin.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtBudget.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal budget) || budget < 0)
            {
                MessageBox.Show("Budget invalide ou négatif.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validation des champs résultats uniquement si statut = "Terminée"
            if (cmbStatut.Text == "Terminée")
            {
                if (!int.TryParse(txtConversationsMessages.Text, out int conv) || conv < 0)
                {
                    MessageBox.Show("ConversationsMessages invalide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (!int.TryParse(txtVues.Text, out int vues) || vues < 0)
                {
                    MessageBox.Show("Vues invalide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (!int.TryParse(txtSpectateurs.Text, out int spectateurs) || spectateurs < 0)
                {
                    MessageBox.Show("Spectateurs invalide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (!decimal.TryParse(txtBudgetQuotidien.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal budgetQ) || budgetQ < 0)
                {
                    MessageBox.Show("Budget Quotidien invalide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private void ReinitialiserFormulaire()
        {
            txtNomCampagne.Clear();
            cmbTypeCampagne.SelectedIndex = 0;
            dateDebut.Value = DateTime.Now.Date;
            dateFin.Value = DateTime.Now.Date.AddDays(7);
            txtBudget.Clear();
            cmbStatut.SelectedIndex = 0;
            txtCommentaires.Clear();

            // Réinitialiser les champs résultats
            txtConversationsMessages.Text = "0";
            txtVues.Text = "0";
            txtSpectateurs.Text = "0";
            txtBudgetQuotidien.Text = "0.00";

            // Verrouiller les champs résultats au départ
            txtConversationsMessages.ReadOnly = true;
            txtVues.ReadOnly = true;
            txtSpectateurs.ReadOnly = true;
            txtBudgetQuotidien.ReadOnly = true;

            selectedCampaignId = -1;
            dgvMarketing.ClearSelection();
        }
        private void CmbStatut_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool terminee = cmbStatut.Text == "Terminée";

            txtConversationsMessages.ReadOnly = !terminee;
            txtVues.ReadOnly = !terminee;
            txtSpectateurs.ReadOnly = !terminee;
            txtBudgetQuotidien.ReadOnly = !terminee;

            if (!terminee)
            {
                txtConversationsMessages.Text = "0";
                txtVues.Text = "0";
                txtSpectateurs.Text = "0";
                txtBudgetQuotidien.Text = "0.00";
            }
        }

        private void DgvMarketing_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvMarketing.Rows[e.RowIndex];
            selectedCampaignId = Convert.ToInt32(row.Cells["Id"].Value);

            txtNomCampagne.Text = row.Cells["NomCampagne"].Value.ToString();
            cmbTypeCampagne.Text = row.Cells["TypeCampagne"].Value.ToString();
            dateDebut.Value = Convert.ToDateTime(row.Cells["DateDebut"].Value);
            dateFin.Value = Convert.ToDateTime(row.Cells["DateFin"].Value);
            txtBudget.Text = Convert.ToDecimal(row.Cells["Budget"].Value).ToString("F2");
            cmbStatut.Text = row.Cells["Statut"].Value.ToString();
            txtCommentaires.Text = row.Cells["Commentaires"].Value.ToString();

            bool terminee = cmbStatut.Text == "Terminée";

            if (terminee)
            {
                txtConversationsMessages.Text = row.Cells["ConversationsMessages"].Value.ToString();
                txtVues.Text = row.Cells["Vues"].Value.ToString();
                txtSpectateurs.Text = row.Cells["Spectateurs"].Value.ToString();
                txtBudgetQuotidien.Text = Convert.ToDecimal(row.Cells["BudgetQuotidien"].Value).ToString("F2");
            }
            else
            {
                txtConversationsMessages.Text = "0";
                txtVues.Text = "0";
                txtSpectateurs.Text = "0";
                txtBudgetQuotidien.Text = "0.00";
            }

            if (txtFacebookCampaignId != null)
            {
                if (dgvMarketing.Columns.Contains("FacebookCampaignId"))
                    txtFacebookCampaignId.Text = row.Cells["FacebookCampaignId"].Value?.ToString() ?? "";
                else
                    txtFacebookCampaignId.Text = "";
            }

            txtConversationsMessages.ReadOnly = !terminee;
            txtVues.ReadOnly = !terminee;
            txtSpectateurs.ReadOnly = !terminee;
            txtBudgetQuotidien.ReadOnly = !terminee;

            ChargerDernieresStatsDansControles(selectedCampaignId);

        }

        private void MettreAJourStatutCampagnes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        UPDATE CampagnesMarketing
                        SET Statut = 'Terminée'
                        WHERE Statut = 'En cours' AND DateFin < CAST(GETDATE() AS DATE)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int lignesAffectees = cmd.ExecuteNonQuery();
                        if (lignesAffectees > 0)
                        {
                            ChargerCampagnes();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur mise à jour statut campagnes : " + ex.Message);
            }
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            if (selectedCampaignId < 0)
            {
                MessageBox.Show("Veuillez sélectionner une campagne à modifier.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!VerifierChamps()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
            UPDATE CampagnesMarketing SET
                NomCampagne = @NomCampagne,
                TypeCampagne = @TypeCampagne,
                DateDebut = @DateDebut,
                DateFin = @DateFin,
                Budget = @Budget,
                Statut = @Statut,
                Commentaires = @Commentaires,
                ConversationsMessages = @ConversationsMessages,
                Vues = @Vues,
                Spectateurs = @Spectateurs,
                BudgetQuotidien = @BudgetQuotidien
            WHERE Id = @Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NomCampagne", txtNomCampagne.Text.Trim());
                        cmd.Parameters.AddWithValue("@TypeCampagne", cmbTypeCampagne.Text);
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value.Date);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin.Value.Date);
                        cmd.Parameters.Add("@Budget", SqlDbType.Decimal).Value = LireDecimal(txtBudget.Text);
                        cmd.Parameters.AddWithValue("@Statut", cmbStatut.Text);
                        cmd.Parameters.AddWithValue("@Commentaires", txtCommentaires.Text.Trim());

                        if (cmbStatut.Text == "Terminée")
                        {
                            cmd.Parameters.AddWithValue("@ConversationsMessages", int.Parse(txtConversationsMessages.Text));
                            cmd.Parameters.AddWithValue("@Vues", int.Parse(txtVues.Text));
                            cmd.Parameters.AddWithValue("@Spectateurs", int.Parse(txtSpectateurs.Text));
                            cmd.Parameters.Add("@BudgetQuotidien", SqlDbType.Decimal)
                                .Value = LireDecimal(txtBudgetQuotidien.Text);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@ConversationsMessages", 0);
                            cmd.Parameters.AddWithValue("@Vues", 0);
                            cmd.Parameters.AddWithValue("@Spectateurs", 0);
                            cmd.Parameters.Add("@BudgetQuotidien", SqlDbType.Decimal).Value = 0m;
                        }

                        cmd.Parameters.AddWithValue("@Id", selectedCampaignId);
                        cmd.ExecuteNonQuery();
                    }

                    // 👉 Synchronisation des statistiques
                    if (cmbStatut.Text == "Terminée")
                    {
                        InsererOuMettreAJourStatistiques(selectedCampaignId, conn);
                    }
                    else
                    {
                        SupprimerStatistiquesSiExistent(selectedCampaignId, conn);
                    }
                }

                MessageBox.Show("Campagne modifiée avec succès.",
                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ChargerCampagnes();
                ReinitialiserFormulaire();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur modification campagne : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (selectedCampaignId < 0)
            {
                MessageBox.Show("Veuillez sélectionner une campagne à supprimer.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Voulez-vous vraiment supprimer cette campagne ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM CampagnesMarketing WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", selectedCampaignId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Campagne supprimée avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ChargerCampagnes();
                ReinitialiserFormulaire();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur suppression campagne : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSaveFacebookCampaignId_Click(object sender, EventArgs e)
        {
            if (selectedCampaignId <= 0)
            {
                MessageBox.Show("Sélectionne d'abord une campagne dans la liste.");
                return;
            }

            string fbId = (txtFacebookCampaignId.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(fbId))
            {
                MessageBox.Show("Colle le Campaign ID Facebook (ex: 2385...).");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE CampagnesMarketing SET FacebookCampaignId=@fb WHERE Id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@fb", fbId);
                        cmd.Parameters.AddWithValue("@id", selectedCampaignId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("ID Facebook enregistré.");
                ChargerCampagnes(); // refresh grille
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur: " + ex.Message);
            }
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            if (!VerifierChamps())
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1️⃣ Insertion dans CampagnesMarketing
                    string insertCampagneQuery = @"
                INSERT INTO CampagnesMarketing
                    (NomCampagne, TypeCampagne, DateDebut, DateFin, Budget, Statut, Commentaires,
                     ConversationsMessages, Vues, Spectateurs, BudgetQuotidien)
                VALUES
                    (@NomCampagne, @TypeCampagne, @DateDebut, @DateFin, @Budget, @Statut, @Commentaires,
                     @ConversationsMessages, @Vues, @Spectateurs, @BudgetQuotidien);

                SELECT SCOPE_IDENTITY();";

                    int newCampaignId;

                    using (SqlCommand cmd = new SqlCommand(insertCampagneQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@NomCampagne", txtNomCampagne.Text.Trim());
                        cmd.Parameters.AddWithValue("@TypeCampagne", cmbTypeCampagne.Text);
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut.Value.Date);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin.Value.Date);
                        cmd.Parameters.Add("@Budget", SqlDbType.Decimal).Value = LireDecimal(txtBudget.Text);
                        cmd.Parameters.AddWithValue("@Statut", cmbStatut.Text);
                        cmd.Parameters.AddWithValue("@Commentaires", txtCommentaires.Text.Trim());

                        if (cmbStatut.Text == "Terminée")
                        {
                            cmd.Parameters.AddWithValue("@ConversationsMessages", int.Parse(txtConversationsMessages.Text));
                            cmd.Parameters.AddWithValue("@Vues", int.Parse(txtVues.Text));
                            cmd.Parameters.AddWithValue("@Spectateurs", int.Parse(txtSpectateurs.Text));
                            cmd.Parameters.Add("@BudgetQuotidien", SqlDbType.Decimal).Value = LireDecimal(txtBudgetQuotidien.Text);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@ConversationsMessages", 0);
                            cmd.Parameters.AddWithValue("@Vues", 0);
                            cmd.Parameters.AddWithValue("@Spectateurs", 0);
                            cmd.Parameters.Add("@BudgetQuotidien", SqlDbType.Decimal).Value = 0m;
                        }

                        // Exécution et récupération ID (attention conversion)
                        object result = cmd.ExecuteScalar();
                        newCampaignId = Convert.ToInt32(Convert.ToDecimal(result));
                    }

                    string insertStatsQuery = @"
INSERT INTO StatistiquesPublicites
    (CampagneId, DateVente, NomCampagne, Vues, Messages, Commentaires, Statut, Spectateurs, BudgetQuotidien, NombreVentes, MontantVendus, Devise)
VALUES
    (@CampagneId, GETDATE(), @NomCampagne, @Vues, @Messages, @Commentaires, @Statut, @Spectateurs, @BudgetQuotidien, @NombreVentes, @MontantVendus, @Devise)";

                    using (SqlCommand cmdStats = new SqlCommand(insertStatsQuery, conn))
                    {
                        cmdStats.Parameters.AddWithValue("@CampagneId", newCampaignId);
                        cmdStats.Parameters.AddWithValue("@NomCampagne", txtNomCampagne.Text.Trim());
                        cmdStats.Parameters.AddWithValue("@Vues", int.Parse(txtVues.Text));
                        cmdStats.Parameters.AddWithValue("@Messages", int.Parse(txtConversationsMessages.Text));
                        cmdStats.Parameters.AddWithValue("@Commentaires", txtCommentaires.Text.Trim());
                        cmdStats.Parameters.AddWithValue("@Statut", cmbStatut.Text);
                        cmdStats.Parameters.AddWithValue("@Spectateurs", int.Parse(txtSpectateurs.Text));
                        cmdStats.Parameters.Add("@BudgetQuotidien", SqlDbType.Decimal).Value = LireDecimal(txtBudgetQuotidien.Text);
                        cmdStats.Parameters.AddWithValue("@NombreVentes", int.Parse(txtNombreVentes.Text));
                        cmdStats.Parameters.Add("@MontantVendus", SqlDbType.Decimal).Value = LireDecimal(txtMontantVendus.Text);

                        // Ajout du paramètre Devise, par exemple depuis un ComboBox cbDevise
                        cmdStats.Parameters.AddWithValue("@Devise", cbDevise.SelectedItem?.ToString() ?? "FC");

                        cmdStats.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Campagne ajoutée avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ChargerCampagnes();
                ReinitialiserFormulaire();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ajout campagne : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChargerDernieresStatsDansControles(int campagneId)
        {
            if (campagneId <= 0) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // On prend la dernière ligne de stats (la plus récente)
                    string sql = @"
SELECT TOP 1
    Vues,
    Messages,
    Commentaires,
    Statut,
    Spectateurs,
    BudgetQuotidien,
    NombreVentes,
    MontantVendus,
    Devise,
    DateVente
FROM StatistiquesPublicites
WHERE CampagneId = @id
ORDER BY DateVente DESC;";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = campagneId;

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            if (!rd.Read())
                            {
                                // Pas de stats => remettre à zéro proprement
                                RemettreStatsAZeroDansControles();
                                return;
                            }

                            // ⚠️ sécuriser DBNull
                            txtVues.Text = (rd["Vues"] == DBNull.Value ? 0 : Convert.ToInt32(rd["Vues"])).ToString();
                            txtConversationsMessages.Text = (rd["Messages"] == DBNull.Value ? 0 : Convert.ToInt32(rd["Messages"])).ToString();

                            // Commentaires : chez toi c’est TextBox, OK
                            txtCommentaires.Text = (rd["Commentaires"] == DBNull.Value ? "" : rd["Commentaires"].ToString());

                            // Statut : si tu veux forcer le statut depuis stats
                            string statut = (rd["Statut"] == DBNull.Value ? "" : rd["Statut"].ToString());
                            if (!string.IsNullOrWhiteSpace(statut))
                                cmbStatut.Text = statut;

                            txtSpectateurs.Text = (rd["Spectateurs"] == DBNull.Value ? 0 : Convert.ToInt32(rd["Spectateurs"])).ToString();

                            decimal budgetQ = (rd["BudgetQuotidien"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["BudgetQuotidien"]));
                            txtBudgetQuotidien.Text = budgetQ.ToString("F2", CultureInfo.InvariantCulture);

                            // Ces 2 champs existent chez toi (vu dans InsertStats)
                            int nbVentes = (rd["NombreVentes"] == DBNull.Value ? 0 : Convert.ToInt32(rd["NombreVentes"]));
                            txtNombreVentes.Text = nbVentes.ToString();

                            decimal montant = (rd["MontantVendus"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["MontantVendus"]));
                            txtMontantVendus.Text = montant.ToString("F2", CultureInfo.InvariantCulture);

                            // Devise + affichage label montant
                            string devise = (rd["Devise"] == DBNull.Value ? "FC" : rd["Devise"].ToString());
                            if (cbDevise.Items.Contains(devise)) cbDevise.SelectedItem = devise;

                            montantVendu = montant;
                            MettreAJourAffichageMontantVendu();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement stats: " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemettreStatsAZeroDansControles()
        {
            txtVues.Text = "0";
            txtConversationsMessages.Text = "0";
            txtSpectateurs.Text = "0";
            txtBudgetQuotidien.Text = "0.00";

            txtNombreVentes.Text = "0";
            txtMontantVendus.Text = "0.00";

            montantVendu = 0m;
            MettreAJourAffichageMontantVendu();
        }


        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            ReinitialiserFormulaire();
        }

        private void btnObjectifs_Click(object sender, EventArgs e)
        {
            using (var f = new FrmObjectifsCampagnes())
                f.ShowDialog(this);
        }

        private async void btnSyncFacebook_Click(object sender, EventArgs e)
        {
            try
            {
                // Optionnel mais pro : exiger une campagne sélectionnée
                if (selectedCampaignId <= 0)
                {
                    MessageBox.Show("Sélectionne d'abord une campagne à synchroniser.");
                    return;
                }

                btnSyncFacebookMini.Enabled = false;
                Cursor = Cursors.WaitCursor;

                var job = new FacebookInsightsJob(ConfigSysteme.ConnectionString);
                await job.RunAsync();

                // ✅ refresh grid (si jamais la campagne a été mise à jour)
                ChargerCampagnes();

                // ✅ recharge les dernières stats (DB) dans tes contrôles
                ChargerDernieresStatsDansControles(selectedCampaignId);

                MessageBox.Show("Sync Facebook OK.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur Sync Facebook: " + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnSyncFacebookMini.Enabled = true;
            }
        }
    }
 }
