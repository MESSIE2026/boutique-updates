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
    public partial class FrmValidationDepenses : FormBase
    {
        private readonly string cs = ConfigSysteme.ConnectionString;

        private DataGridView dgv;
        private TextBox txtCommentaire;
        private Button btnValider, btnRejeter, btnActualiser, btnFermer;
        private Label lblInfo;
        public FrmValidationDepenses()
        {
            InitializeComponent();

            this.Load += FrmValidationDepenses_Load;
            

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }

        private void RafraichirLangue() => ConfigSysteme.AppliquerTraductions(this);
        private void RafraichirTheme() => ConfigSysteme.AppliquerTheme(this);

        private void FrmValidationDepenses_Load(object sender, EventArgs e)
        {
            ConstruireUI();
            HookEvents();

            RafraichirLangue();
            RafraichirTheme();

            ChargerEnAttente();
            bool ok = ManagerAutorise();
            btnValider.Enabled = ok;
            btnRejeter.Enabled = ok;
            if (!ok) lblInfo.Text = "Accès lecture seule : vous n'êtes pas autorisé à valider.";
        }

        private bool ManagerAutorise()
        {
            if (string.IsNullOrWhiteSpace(SessionEmploye.Poste)) return false;

            return SessionEmploye.Poste.Equals("Directeur", StringComparison.OrdinalIgnoreCase)
                || SessionEmploye.Poste.Equals("Superviseur", StringComparison.OrdinalIgnoreCase)
                || SessionEmploye.Poste.Equals("RH", StringComparison.OrdinalIgnoreCase);
        }

        private string GetValideurNomComplet()
        {
            string n = ((SessionEmploye.Nom ?? "") + " " + (SessionEmploye.Prenom ?? "")).Trim();
            if (!string.IsNullOrWhiteSpace(n)) return n;

            // fallback
            return "Manager";
        }

        private string GetStatutCurrentRow()
        {
            if (dgv?.CurrentRow == null) return "";
            if (!dgv.Columns.Contains("Statut")) return "";
            return (dgv.CurrentRow.Cells["Statut"].Value?.ToString() ?? "").Trim();
        }

        private void ChargerEnAttente()
{
    try
    {
        using (var con = new SqlConnection(cs))
        {
            con.Open();

            string sql = @"
SELECT
    IdDepense,
    DateDepense,
    Description,
    Montant,
    Devise,
    Categorie,
    Observations,
    Statut
FROM dbo.Depenses
WHERE ISNULL(Statut,'En attente') = 'En attente'
ORDER BY DateDepense DESC, IdDepense DESC;";

            var dt = new DataTable();
            using (var da = new SqlDataAdapter(sql, con))
                da.Fill(dt);

            dgv.DataSource = dt;

            // Format pro
            if (dgv.Columns.Contains("IdDepense"))
                dgv.Columns["IdDepense"].HeaderText = "ID";

            if (dgv.Columns.Contains("DateDepense"))
                dgv.Columns["DateDepense"].DefaultCellStyle.Format = "dd/MM/yyyy";

            if (dgv.Columns.Contains("Montant"))
            {
                dgv.Columns["Montant"].DefaultCellStyle.Format = "N2";
                dgv.Columns["Montant"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            lblInfo.Text = $"En attente : {dt.Rows.Count} dépense(s).";
            dgv.ClearSelection();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Erreur chargement dépenses en attente : " + ex.Message);
    }
}


        private void ConstruireUI()
        {
            this.Text = "Validation des dépenses (Manager)";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(1100, 650);

            this.Controls.Clear();

            // ===== TOP =====
            var top = new Panel { Dock = DockStyle.Top, Height = 110, Padding = new Padding(10) };

            lblInfo = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                Text = "Sélectionne une dépense puis Valider / Rejeter.",
                AutoEllipsis = true
            };

            var pnlComment = new Panel { Dock = DockStyle.Top, Height = 40 };

            var lblCom = new Label
            {
                Text = "Commentaire manager :",
                AutoSize = true,
                Left = 0,
                Top = 10
            };

            txtCommentaire = new TextBox
            {
                Left = 170,
                Top = 6,
                Width = 650,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            pnlComment.Controls.Add(lblCom);
            pnlComment.Controls.Add(txtCommentaire);

            var pnlBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                WrapContents = false
            };

            btnValider = new Button { Name = "btnValider", Text = "Valider", Width = 130, Height = 32, BackColor = Color.DarkGreen, ForeColor = Color.White };
            btnRejeter = new Button { Name = "btnRejeter", Text = "Rejeter", Width = 130, Height = 32, BackColor = Color.DarkRed, ForeColor = Color.White };
            btnActualiser = new Button { Name = "btnActualiser", Text = "Actualiser", Width = 130, Height = 32 };
            btnFermer = new Button { Name = "btnFermer", Text = "Fermer", Width = 130, Height = 32 };

            pnlBtns.Controls.AddRange(new Control[] { btnValider, btnRejeter, btnActualiser, btnFermer });

            top.Controls.Add(pnlBtns);
            top.Controls.Add(pnlComment);
            top.Controls.Add(lblInfo);

            // ===== GRID =====
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };

            this.Controls.Add(dgv);
            this.Controls.Add(top);
        }

        private void HookEvents()
        {
            btnActualiser.Click += (s, e) => ChargerEnAttente();
            btnFermer.Click += (s, e) => this.Close();

            btnValider.Click += (s, e) => ValiderManagerDepense();
            btnRejeter.Click += (s, e) => RejeterManagerDepense();
            

            dgv.SelectionChanged += (s, e) =>
            {
                // petit rappel visuel
                if (dgv.CurrentRow == null) return;
                lblInfo.Text = $"Dépense sélectionnée : ID={dgv.CurrentRow.Cells["IdDepense"].Value}";
            };
        }

        private void ValiderManagerDepense()
        {
            // ✅ droits au début (exactement comme ton modèle)
            if (!ManagerAutorise())
            {
                MessageBox.Show("Vous n'avez pas l'autorisation de valider.", "Accès refusé",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Sélectionne une dépense.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ✅ si déjà validé, on stop
            string statut = GetStatutCurrentRow();
            if (!string.IsNullOrWhiteSpace(statut) &&
                statut.Equals("Validé", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Cette dépense est déjà validée.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(dgv.CurrentRow.Cells["IdDepense"].Value);
            string commentaire = (txtCommentaire.Text ?? "").Trim(); // optionnel pour validation
            string validePar = GetValideurNomComplet();

            try
            {
                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    using (var cmd = new SqlCommand(@"
UPDATE dbo.Depenses
SET Statut = N'Validé',
    DateValidation = GETDATE(),
    ValidePar = @ValidePar,
    CommentaireValidation = @Com,
    RejetePar = NULL,
    DateRejet = NULL,
    MotifRejet = NULL
WHERE IdDepense = @Id
  AND ISNULL(Statut,'En attente') = N'En attente'
  AND ISNULL(Statut,'') <> N'Validé';", con))
                    {
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@ValidePar", SqlDbType.NVarChar, 100).Value = validePar;
                        cmd.Parameters.Add("@Com", SqlDbType.NVarChar, 255).Value = commentaire;

                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            MessageBox.Show("Cette dépense n'est plus en attente (déjà traitée).", "Info",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ChargerEnAttente();
                            return;
                        }
                    }
                }

                ConfigSysteme.AjouterAuditLog("Depenses",
                    $"VALIDATION dépense Id={id} par {validePar} | Commentaire='{commentaire}'",
                    "Succès");

                MessageBox.Show($"Validé ✅ par {validePar}", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtCommentaire.Clear();
                ChargerEnAttente();

                // Option : fermer automatiquement si tu veux
                // this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Depenses",
                    $"Erreur VALIDATION dépense Id={id} : {ex.Message}",
                    "Échec");

                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void RejeterManagerDepense()
        {
            // ✅ droits au début
            if (!ManagerAutorise())
            {
                MessageBox.Show("Vous n'avez pas l'autorisation de valider.", "Accès refusé",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Sélectionne une dépense.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ✅ si déjà validé, on stop (tu peux aussi bloquer si déjà rejeté)
            string statut = GetStatutCurrentRow();
            if (!string.IsNullOrWhiteSpace(statut) &&
                statut.Equals("Validé", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Cette dépense est déjà validée. Impossible de rejeter.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(dgv.CurrentRow.Cells["IdDepense"].Value);
            string motif = (txtCommentaire.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(motif))
            {
                MessageBox.Show("Motif de rejet obligatoire.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string rejetePar = GetValideurNomComplet();

            try
            {
                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    using (var cmd = new SqlCommand(@"
UPDATE dbo.Depenses
SET Statut = N'Rejeté',
    DateRejet = GETDATE(),
    RejetePar = @RejetePar,
    MotifRejet = @Motif,
    ValidePar = NULL,
    DateValidation = NULL,
    CommentaireValidation = NULL
WHERE IdDepense = @Id
  AND ISNULL(Statut,'En attente') = N'En attente'
  AND ISNULL(Statut,'') <> N'Validé';", con))
                    {
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@RejetePar", SqlDbType.NVarChar, 100).Value = rejetePar;
                        cmd.Parameters.Add("@Motif", SqlDbType.NVarChar, 255).Value = motif;

                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            MessageBox.Show("Cette dépense n'est plus en attente (déjà traitée).", "Info",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ChargerEnAttente();
                            return;
                        }
                    }
                }

                ConfigSysteme.AjouterAuditLog("Depenses",
                    $"REJET dépense Id={id} par {rejetePar} | Motif='{motif}'",
                    "Succès");

                MessageBox.Show($"Rejeté ❌ par {rejetePar}", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtCommentaire.Clear();
                ChargerEnAttente();
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Depenses",
                    $"Erreur REJET dépense Id={id} : {ex.Message}",
                    "Échec");

                MessageBox.Show("Erreur : " + ex.Message);
            }
        }


        private void ValiderOuRejeter(bool isValidation)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Sélectionne une dépense.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(dgv.CurrentRow.Cells["IdDepense"].Value);
            string commentaire = (txtCommentaire.Text ?? "").Trim();

            if (!isValidation && string.IsNullOrWhiteSpace(commentaire))
            {
                MessageBox.Show("Motif de rejet obligatoire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string user = ((SessionEmploye.Nom ?? "") + " " + (SessionEmploye.Prenom ?? "")).Trim();
            if (string.IsNullOrWhiteSpace(user)) user = "Manager";

            try
            {
                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    string sql;
                    if (isValidation)
                    {
                        sql = @"
UPDATE dbo.Depenses SET
    Statut = 'Validée',
    ValidePar = @User,
    DateValidation = GETDATE(),
    CommentaireValidation = @Com,
    RejetePar = NULL,
    DateRejet = NULL,
    MotifRejet = NULL
WHERE IdDepense = @Id AND Statut = 'En attente';";
                    }
                    else
                    {
                        sql = @"
UPDATE dbo.Depenses SET
    Statut = 'Rejetée',
    RejetePar = @User,
    DateRejet = GETDATE(),
    MotifRejet = @Com,
    ValidePar = NULL,
    DateValidation = NULL,
    CommentaireValidation = NULL
WHERE IdDepense = @Id AND Statut = 'En attente';";
                    }

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@User", SqlDbType.NVarChar, 80).Value = user;
                        cmd.Parameters.Add("@Com", SqlDbType.NVarChar, 255).Value = commentaire;

                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            MessageBox.Show("Cette dépense n'est plus en attente (déjà traitée).", "Info",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ChargerEnAttente();
                            return;
                        }
                    }
                }

                // audit log
                string action = isValidation ? "VALIDATION" : "REJET";
                ConfigSysteme.AjouterAuditLog("Depenses",
                    $"{action} dépense Id={id} par {user} | Commentaire='{commentaire}'",
                    "Succès");

                MessageBox.Show(isValidation ? "Dépense validée." : "Dépense rejetée.",
                    "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtCommentaire.Clear();
                ChargerEnAttente();
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Depenses",
                    $"Erreur validation/rejet dépense Id={id} : {ex.Message}",
                    "Échec");

                MessageBox.Show("Erreur : " + ex.Message);
            }
        }
    }
}
