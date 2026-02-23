using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FormInventaire : FormBase
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;

        public FormInventaire()
        {
            InitializeComponent();
           
            Load += FormInventaire_Load;

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }
        private void FormInventaire_Load(object sender, EventArgs e)
        {
            InitialiserUI();
            ConfigurerDgvInventaire();
            ChargerInventaireGlobal();
            ChargerStatistiquesVentes();

            dtpDateAjout.Value = DateTime.Now;
            numQuantite.Value = 1;

            RafraichirLangue();
            RafraichirTheme();
        }
        private void InitialiserUI()
        {
            cmbCategorie.Items.AddRange(new string[]
            {
        "Vêtements","Accessoires","Chaussures","Beauté","Maison",
        "Électronique","Papeterie","Alimentation","Artisanat","Autres"
            });

            dtpDateAjout.Value = DateTime.Now;
            numQuantite.Value = 1;

            // Applique le thème en fonction de ConfigSysteme.Theme
            ConfigSysteme.AppliquerTheme(this);
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

        // =========================
        // CONFIG DGV
        // =========================
        private void ConfigurerDgvInventaire()
        {
            dgvInventaire.ReadOnly = true;
            dgvInventaire.AllowUserToAddRows = false;
            dgvInventaire.AllowUserToDeleteRows = false;
            dgvInventaire.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInventaire.MultiSelect = false;

            // ✅ Barres de défilement horizontale + verticale
            dgvInventaire.ScrollBars = ScrollBars.Both;

            // ✅ On veut des largeurs "réfléchies", donc PAS Fill global
            dgvInventaire.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvInventaire.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            // ✅ Lisibilité
            dgvInventaire.RowHeadersVisible = false;
            dgvInventaire.AllowUserToResizeRows = false;
            dgvInventaire.AllowUserToResizeColumns = true; // l’utilisateur peut ajuster si besoin

            dgvInventaire.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvInventaire.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // ✅ Couleurs stock
            dgvInventaire.CellFormatting -= dgvInventaire_CellFormatting;
            dgvInventaire.CellFormatting += dgvInventaire_CellFormatting;

            // ✅ Appliquer après bind
            dgvInventaire.DataBindingComplete -= dgvInventaire_DataBindingComplete;
            dgvInventaire.DataBindingComplete += dgvInventaire_DataBindingComplete;
        }

        private void dgvInventaire_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvInventaire.Columns.Count == 0) return;

            dgvInventaire.SuspendLayout();

            // ----- LARGEURS + FORMAT -----
            // Référence
            if (dgvInventaire.Columns.Contains("RefProduit"))
            {
                var c = dgvInventaire.Columns["RefProduit"];
                c.HeaderText = "Réf";
                c.Width = 110;
                c.Frozen = true; // ✅ reste visible au scroll horizontal
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }

            // Nom produit (colonne la plus importante)
            if (dgvInventaire.Columns.Contains("Nom_Produit"))
            {
                var c = dgvInventaire.Columns["Nom_Produit"];
                c.HeaderText = "Produit";
                c.Width = 260;
                c.Frozen = true; // ✅ reste visible aussi
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }

            // Stock initial / vendu / restant
            if (dgvInventaire.Columns.Contains("StockInitial"))
            {
                var c = dgvInventaire.Columns["StockInitial"];
                c.HeaderText = "Stock init.";
                c.Width = 90;
            }

            if (dgvInventaire.Columns.Contains("QuantiteVendue"))
            {
                var c = dgvInventaire.Columns["QuantiteVendue"];
                c.HeaderText = "Vendu";
                c.Width = 80;
            }

            if (dgvInventaire.Columns.Contains("StockRestant"))
            {
                var c = dgvInventaire.Columns["StockRestant"];
                c.HeaderText = "Restant";
                c.Width = 85;
            }

            // Prix + devise
            if (dgvInventaire.Columns.Contains("Prix"))
            {
                var c = dgvInventaire.Columns["Prix"];
                c.HeaderText = "Prix";
                c.Width = 90;
                c.DefaultCellStyle.Format = "N2";
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (dgvInventaire.Columns.Contains("Devise"))
            {
                var c = dgvInventaire.Columns["Devise"];
                c.HeaderText = "Devise";
                c.Width = 70;
            }

            // Catégorie
            if (dgvInventaire.Columns.Contains("Categorie"))
            {
                var c = dgvInventaire.Columns["Categorie"];
                c.HeaderText = "Catégorie";
                c.Width = 140;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }

            // Fournisseur
            if (dgvInventaire.Columns.Contains("Fournisseur"))
            {
                var c = dgvInventaire.Columns["Fournisseur"];
                c.HeaderText = "Fournisseur";
                c.Width = 170;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }

            // ✅ Optionnel: rendre certaines colonnes ajustables par contenu
            // Exemple : Devise ou Catégorie
            // dgvInventaire.Columns["Devise"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            dgvInventaire.ResumeLayout();

            // ✅ Force les barres de scroll à apparaître correctement
            dgvInventaire.PerformLayout();
        }


        // =========================
        // INVENTAIRE GLOBAL
        // =========================
        private void ChargerInventaireGlobal()
        {
            string sql = @"
SELECT 
    i.RefProduit,
    i.Nom_Produit,
    i.Quantite AS StockInitial,

    -- ✅ Quantité vendue NETTE (vendu - retours), uniquement ventes non annulées
    ISNULL(SUM(
        CASE 
            WHEN ISNULL(v.Statut,'') <> 'ANNULE' 
            THEN (ISNULL(dv.Quantite,0) - ISNULL(dv.QuantiteRetournee,0))
            ELSE 0
        END
    ), 0) AS QuantiteVendue,

    -- ✅ Stock restant = stock initial - vendu net
    (i.Quantite - ISNULL(SUM(
        CASE 
            WHEN ISNULL(v.Statut,'') <> 'ANNULE' 
            THEN (ISNULL(dv.Quantite,0) - ISNULL(dv.QuantiteRetournee,0))
            ELSE 0
        END
    ), 0)) AS StockRestant,

    p.Prix,
    p.Devise,
    p.Categorie,
    i.Fournisseur
FROM Inventaire i
LEFT JOIN Produit p ON i.RefProduit = p.RefProduit

-- ✅ On joint dv puis Vente pour filtrer Statut
LEFT JOIN DetailsVente dv ON i.RefProduit = dv.RefProduit
LEFT JOIN Vente v ON dv.ID_Vente = v.ID_Vente

GROUP BY 
    i.RefProduit,
    i.Nom_Produit,
    i.Quantite,
    p.Prix,
    p.Devise,
    p.Categorie,
    i.Fournisseur
ORDER BY i.Nom_Produit;";

            ChargerDansDgv(sql);
        }

        // =========================
        // STATISTIQUES VENTES
        // =========================
        private void ChargerStatistiquesVentes()
        {
            lblJour.Text = CalculerVente("DAY");
            lblSemaine.Text = CalculerVente("WEEK");
            lblMois.Text = CalculerVente("MONTH");
            lblAnnee.Text = CalculerVente("YEAR");
        }

        private string CalculerVente(string periode)
        {
            DateTime now = DateTime.Now;
            DateTime start;
            DateTime end;

            switch (periode)
            {
                case "DAY":
                    start = now.Date;
                    end = start.AddDays(1);
                    break;

                case "WEEK":
                    // ✅ semaine = lundi -> lundi (adaptable)
                    int diff = (7 + (int)now.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                    start = now.Date.AddDays(-diff);
                    end = start.AddDays(7);
                    break;

                case "MONTH":
                    start = new DateTime(now.Year, now.Month, 1);
                    end = start.AddMonths(1);
                    break;

                case "YEAR":
                    start = new DateTime(now.Year, 1, 1);
                    end = start.AddYears(1);
                    break;

                default:
                    start = now.Date;
                    end = start.AddDays(1);
                    break;
            }

            string sql = @"
SELECT ISNULL(SUM(dv.Montant), 0)
FROM DetailsVente dv
INNER JOIN Vente v ON dv.ID_Vente = v.ID_Vente
WHERE v.DateVente >= @start AND v.DateVente < @end
  AND ISNULL(v.Statut,'') <> 'ANNULE';";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@start", SqlDbType.DateTime).Value = start;
                cmd.Parameters.Add("@end", SqlDbType.DateTime).Value = end;

                con.Open();
                object result = cmd.ExecuteScalar();
                decimal total = (result == DBNull.Value) ? 0m : Convert.ToDecimal(result);
                return total.ToString("N2");
            }
        }


        // =========================
        // CHARGEMENT DGV
        // =========================
        private void ChargerDansDgv(string sql)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvInventaire.DataSource = dt;
                dgvInventaire.Refresh();
            }
        }

        // =========================
        // COLORATION STOCK
        // =========================
        private void dgvInventaire_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvInventaire.Columns[e.ColumnIndex].Name == "StockRestant" && e.Value != null)
            {
                int stock = Convert.ToInt32(e.Value);
                e.CellStyle.BackColor = stock <= 5 ? Color.LightCoral : Color.LightGreen;
            }
        }

        private void btChangerImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                picProduit.Image = Image.FromFile(ofd.FileName);
                picProduit.ImageLocation = ofd.FileName;
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            string sql = @"
INSERT INTO Inventaire
(Nom_Produit, Description, [Prix Unitaire], Date_Ajout, Fournisseur, Quantite, RefProduit)
VALUES
(@Nom, @Desc, @Prix, @Date, @Four, @Qte, @Ref)";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Nom", txtNomProduit.Text);
                        cmd.Parameters.AddWithValue("@Desc", rtbDescription.Text);

                        if (decimal.TryParse(txtPrixUnitaire.Text, out decimal prix))
                            cmd.Parameters.AddWithValue("@Prix", prix);
                        else
                            cmd.Parameters.AddWithValue("@Prix", DBNull.Value);

                        cmd.Parameters.AddWithValue("@Date", dtpDateAjout.Value);
                        cmd.Parameters.AddWithValue("@Four", txtFournisseur.Text);
                        cmd.Parameters.AddWithValue("@Qte", (int)numQuantite.Value);
                        cmd.Parameters.AddWithValue("@Ref", txtReference.Text);

                        cmd.ExecuteNonQuery();
                    }

                    // === Audit Log ===
                    string messageAudit = $"Produit ajouté : Nom={txtNomProduit.Text}, Réf={txtReference.Text}, Quantité={(int)numQuantite.Value}";
                    AjouterAuditLog("Inventaire", messageAudit, "Succès", con);

                    MessageBox.Show("Produit ajouté !");
                    ChargerInventaireGlobal();
                }
            }
            catch (Exception ex)
            {
                // En cas d’erreur, log aussi l’échec
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        string messageAudit = $"Erreur ajout produit : Nom={txtNomProduit.Text}, Réf={txtReference.Text}, Erreur={ex.Message}";
                        AjouterAuditLog("Inventaire", messageAudit, "Échec", con);
                    }
                }
                catch
                {
                    // Ignorer les erreurs d’audit pour ne pas casser le flow
                }

                MessageBox.Show("Erreur ajout produit : " + ex.Message);
            }
        }

        // Méthode pour insérer dans la table AuditLog
        private void AjouterAuditLog(string table, string message, string statut, SqlConnection con)
        {
            string sqlAudit = @"
INSERT INTO AuditLog (TableName, Message, Statut, DateHeure, Utilisateur)
VALUES (@TableName, @Message, @Statut, @DateHeure, @Utilisateur)";

            using (SqlCommand cmdAudit = new SqlCommand(sqlAudit, con))
            {
                cmdAudit.Parameters.AddWithValue("@TableName", table);
                cmdAudit.Parameters.AddWithValue("@Message", message);
                cmdAudit.Parameters.AddWithValue("@Statut", statut);
                cmdAudit.Parameters.AddWithValue("@DateHeure", DateTime.Now);
                cmdAudit.Parameters.AddWithValue("@Utilisateur", SessionEmploye.Prenom); // ou ID ou login selon dispo
                cmdAudit.ExecuteNonQuery();
            }
        }


        private void btAnnuler_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btSupprimer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReference.Text))
            {
                MessageBox.Show("Référence produit manquante.");
                return;
            }

            var confirm = MessageBox.Show(
                $"Confirmer la suppression du produit ?\n\nRéf: {txtReference.Text}",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // ✅ 1) Essayer de supprimer par ID_Produit si dispo (meilleur)
                    int? idProduit = null;

                    if (dgvInventaire.CurrentRow != null)
                    {
                        // Si tu as une colonne ID_Produit dans ta source (Produit ou Inventaire)
                        if (dgvInventaire.Columns.Contains("ID_Produit") &&
                            int.TryParse(dgvInventaire.CurrentRow.Cells["ID_Produit"].Value?.ToString(), out int id))
                        {
                            idProduit = id;
                        }
                    }

                    int rows = 0;

                    if (idProduit.HasValue)
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM Inventaire WHERE ID_Produit = @id", con))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit.Value;
                            rows = cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // ✅ 2) Fallback par RefProduit MAIS sécurisé : ref doit être unique
                        using (SqlCommand chk = new SqlCommand(
                            "SELECT COUNT(1) FROM Inventaire WHERE RefProduit = @ref", con))
                        {
                            chk.Parameters.Add("@ref", SqlDbType.NVarChar, 50).Value = txtReference.Text.Trim();
                            int count = Convert.ToInt32(chk.ExecuteScalar());

                            if (count > 1)
                            {
                                MessageBox.Show(
                                    "Suppression bloquée : cette référence existe en plusieurs lignes.\n" +
                                    "Utilise un ID (ID_Produit) pour supprimer de manière sûre.",
                                    "Sécurité",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                return;
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM Inventaire WHERE RefProduit = @ref", con))
                        {
                            cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 50).Value = txtReference.Text.Trim();
                            rows = cmd.ExecuteNonQuery();
                        }
                    }

                    // ✅ Audit (succès/échec)
                    string statut = rows > 0 ? "Succès" : "Échec";
                    string msg = rows > 0
                        ? $"Produit supprimé : Ref={txtReference.Text.Trim()} (ID={(idProduit.HasValue ? idProduit.Value.ToString() : "N/A")})"
                        : $"Suppression: produit introuvable : Ref={txtReference.Text.Trim()}";

                    AjouterAuditLog("Inventaire", msg, statut, con);

                    MessageBox.Show(rows > 0 ? "Produit supprimé !" : "Produit introuvable");
                }

                ChargerInventaireGlobal();
            }
            catch (Exception ex)
            {
                // Audit erreur (best effort)
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        AjouterAuditLog("Inventaire",
                            $"Erreur suppression produit : Ref={txtReference.Text.Trim()}, Erreur={ex.Message}",
                            "Échec", con);
                    }
                }
                catch { }

                MessageBox.Show("Erreur suppression produit : " + ex.Message);
            }
        }
        private void btDetails_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Détails produit à implémenter (historique des ventes).");
        }
        public void MiseAJourStock(string refProduit, int qteVendue)
        {
            string sql = @"
    UPDATE Inventaire
    SET Quantite = Quantite - @qte
    WHERE RefProduit = @ref AND Quantite >= @qte";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@qte", qteVendue);
                        cmd.Parameters.AddWithValue("@ref", refProduit);

                        con.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows == 0)
                        {
                            MessageBox.Show(
                                "Stock insuffisant ou produit introuvable.",
                                "Stock",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erreur lors de la mise à jour du stock : " + ex.Message,
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // =========================
        // THEME SYSTEME
        // =========================
        public static void AppliquerTheme(Form form, bool sombre)
        {
            Color bg = sombre ? Color.FromArgb(30, 30, 30) : Color.White;
            Color fg = sombre ? Color.White : Color.Black;

            form.BackColor = bg;
            foreach (Control c in form.Controls)
            {
                c.BackColor = bg;
                c.ForeColor = fg;
            }
        }

        private void btOuvrirScanner_Click(object sender, EventArgs e)
        {
            var f = new FrmInventaireScanner();
            f.InventaireValide += () =>
            {
                ChargerInventaireGlobal();
                ChargerStatistiquesVentes();
            };
            f.Show(this); // non modal possible
        }
    }
}

