using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Presentation;
using DataTable = System.Data.DataTable;
using static BoutiqueRebuildFixed.FormMain;
using static iTextSharp.awt.geom.Point2D;
using BoutiqueRebuildFixed.Models;
using BoutiqueRebuildFixed.Security;
using System.Runtime.CompilerServices;

namespace BoutiqueRebuildFixed
{
    public partial class Utilisateurs : Form
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        private int _selectedUserId = -1;
        private string _selectedAvatarPath = null;
        private enum FiltreUsers { Actifs, Inactifs, Tous }
        private FiltreUsers _filtreUsers = FiltreUsers.Actifs;
        private void RafraichirLangue() => AppliquerLangue();
        private readonly string[] _rolesSource = new[] { "Admin", "Employé", "Partenaire" };

        // ===============================
        // ✅ PBKDF2 PRO (salt + hash + iterations)
        // ===============================
        // ⚠️ Ne change pas ces valeurs sans migration (ou stocke iterations en DB)
        private const int PBKDF2_SALT_SIZE = 16;
        private const int PBKDF2_HASH_SIZE = 32;
        private const int PBKDF2_ITERATIONS_DEFAULT = 100000;

        // Si ton .NET supporte HashAlgorithmName.SHA256, on l'utilise.
        // Sinon, fallback HMACSHA1 (comportement classique .NET Framework).
        private static byte[] CreateSalt(int size = PBKDF2_SALT_SIZE)
        {
            byte[] salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);
            return salt;
        }

        private static byte[] HashPasswordPBKDF2(string password, byte[] salt, int iterations, int bytes = PBKDF2_HASH_SIZE)
        {
            if (password == null) password = "";
            if (salt == null) throw new ArgumentNullException(nameof(salt));
            if (iterations <= 0) throw new ArgumentOutOfRangeException(nameof(iterations));

            try
            {
                // ✅ .NET (plus récent) : SHA256
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                    return pbkdf2.GetBytes(bytes);
            }
            catch
            {
                // ✅ fallback .NET Framework ancien : HMACSHA1 (par défaut)
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
                    return pbkdf2.GetBytes(bytes);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }

        private static bool VerifyPasswordPBKDF2(string password, byte[] salt, byte[] expectedHash, int iterations)
        {
            if (expectedHash == null || expectedHash.Length == 0) return false;
            byte[] testHash = HashPasswordPBKDF2(password, salt, iterations, expectedHash.Length);
            return FixedTimeEquals(testHash, expectedHash);
        }


        public Utilisateurs()
        {
            InitializeComponent();

            this.Load += FormUtilisateurs_Load;

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;

            // ✅ important : capter la sélection de ligne
            dgvUtilisateurs.SelectionChanged += dgvUtilisateurs_SelectionChanged;
        }
        public void AppliquerLangue()
        {
            // Appliquer traduction sur tous les contrôles enfants
            ConfigSysteme.AppliquerTraductions(this);

            // Traduire manuellement les items du ComboBox (non pris en charge automatiquement)
            RebuildRolesCombo();

            // Traduire les colonnes DataGridView explicitement (parfois nécessaire)
            foreach (DataGridViewColumn col in dgvUtilisateurs.Columns)
            {
                string texteCol = ConfigSysteme.Traduire(col.Name);
                if (string.IsNullOrEmpty(texteCol) || texteCol == col.Name)
                    texteCol = ConfigSysteme.Traduire(col.HeaderText);

                if (!string.IsNullOrEmpty(texteCol) && texteCol != col.HeaderText)
                    col.HeaderText = texteCol;
            }
        }


        private void FormUtilisateurs_Load(object sender, EventArgs e)
        {
            RebuildRolesCombo();

            dptDateCreation.Value = DateTime.Now;
            chkActif.Checked = true;

            // ✅ Entreprises + Magasins
            ChargerEntreprises();
            cmbEntreprise.SelectedIndexChanged -= cmbEntreprise_SelectedIndexChanged;
            cmbEntreprise.SelectedIndexChanged += cmbEntreprise_SelectedIndexChanged;

            if (cmbEntreprise.SelectedItem is ComboboxItem ent)
                ChargerMagasins(ent.Value);

            ChargerUtilisateurs();
            ConfigurerDgvUtilisateurs();
            

            ConfigSysteme.ChargerConfig();
            RafraichirLangue();
            RafraichirTheme();
        }

        private void RebuildRolesCombo()
        {
            string current = cmbRole.SelectedItem?.ToString();

            cmbRole.BeginUpdate();
            try
            {
                cmbRole.Items.Clear();
                foreach (var r in _rolesSource)
                    cmbRole.Items.Add(ConfigSysteme.Traduire(r) ?? r);

                // garder sélection si possible
                if (!string.IsNullOrWhiteSpace(current))
                {
                    for (int i = 0; i < cmbRole.Items.Count; i++)
                    {
                        if (string.Equals(cmbRole.Items[i]?.ToString(), current, StringComparison.OrdinalIgnoreCase))
                        {
                            cmbRole.SelectedIndex = i;
                            return;
                        }
                    }
                }

                if (cmbRole.Items.Count > 0 && cmbRole.SelectedIndex < 0)
                    cmbRole.SelectedIndex = 0;
            }
            finally
            {
                cmbRole.EndUpdate();
            }
        }


        private void cmbEntreprise_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbEntreprise.SelectedItem is ComboboxItem ent)
                ChargerMagasins(ent.Value);
        }

        private void ConfigurerDgvUtilisateurs()
        {
            dgvUtilisateurs.AutoGenerateColumns = true;

            dgvUtilisateurs.ReadOnly = true;
            dgvUtilisateurs.AllowUserToAddRows = false;
            dgvUtilisateurs.AllowUserToDeleteRows = false;
            dgvUtilisateurs.MultiSelect = false;
            dgvUtilisateurs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // ✅ barres de défilement
            dgvUtilisateurs.ScrollBars = ScrollBars.Both;

            // ✅ colonnes libres, on défile horizontalement
            dgvUtilisateurs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // ✅ important : pas de retour à la ligne sinon ça compresse visuellement
            dgvUtilisateurs.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgvUtilisateurs.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvUtilisateurs.RowTemplate.Height = 26;

            // ✅ esthétique
            dgvUtilisateurs.RowHeadersVisible = false;
            dgvUtilisateurs.EnableHeadersVisualStyles = false;
            dgvUtilisateurs.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvUtilisateurs.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // ✅ espace interne (padding) pour éviter "coincé"
            dgvUtilisateurs.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            dgvUtilisateurs.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            dgvUtilisateurs.AllowUserToResizeColumns = true;
        }

        private void GetScopeFromSelection(out int idEntreprise, out int idMagasin)
        {
            idEntreprise = 0;
            idMagasin = 0;

            // 1) depuis la grille (priorité)
            try
            {
                if (dgvUtilisateurs.CurrentRow != null)
                {
                    int.TryParse(dgvUtilisateurs.CurrentRow.Cells["IdEntreprise"]?.Value?.ToString(), out idEntreprise);
                    int.TryParse(dgvUtilisateurs.CurrentRow.Cells["IdMagasin"]?.Value?.ToString(), out idMagasin);
                }
            }
            catch { }

            // 2) fallback depuis combo
            if (idEntreprise <= 0 && cmbEntreprise.SelectedItem is ComboboxItem ent)
                idEntreprise = ent.Value;

            if (idMagasin <= 0 && cmbMagasin.SelectedItem is ComboboxItem mag)
                idMagasin = mag.Value;
        }

        private void AjusterColonnesDgvUtilisateurs()
        {
            if (dgvUtilisateurs.Columns.Count == 0) return;

            // ✅ MinWidth global : évite colonnes coincées
            foreach (DataGridViewColumn c in dgvUtilisateurs.Columns)
            {
                c.MinimumWidth = 80;
                c.Resizable = DataGridViewTriState.True;
            }

            // Cacher IDs
            if (dgvUtilisateurs.Columns.Contains("ID"))
                dgvUtilisateurs.Columns["ID"].Visible = false;

            if (dgvUtilisateurs.Columns.Contains("IdEntreprise"))
                dgvUtilisateurs.Columns["IdEntreprise"].Visible = false;

            if (dgvUtilisateurs.Columns.Contains("IdMagasin"))
                dgvUtilisateurs.Columns["IdMagasin"].Visible = false;

            // ✅ Largeurs raisonnables (tu peux augmenter si tu veux)
            SetColWidth("NomEntreprise", 160);
            SetColWidth("NomMagasin", 160);
            SetColWidth("NomUtilisateur", 180);
            SetColWidth("Nom", 140);
            SetColWidth("Prenom", 140);
            SetColWidth("Role", 120);
            SetColWidth("Actif", 80);
            SetColWidth("Email", 220);
            SetColWidth("Telephone", 140);
            SetColWidth("DateCreation", 160);
            SetColWidth("AvatarPath", 260);

            // ✅ Alignements
            AlignCenter("Actif");
            AlignCenter("Role");
            AlignCenter("DateCreation");

            // ✅ Format date
            if (dgvUtilisateurs.Columns.Contains("DateCreation"))
                dgvUtilisateurs.Columns["DateCreation"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

            // ✅ force scrollbar horizontal (si beaucoup de colonnes)
            dgvUtilisateurs.HorizontalScrollingOffset = 0;
        }

        private void SetColWidth(string name, int width)
        {
            if (!dgvUtilisateurs.Columns.Contains(name)) return;
            dgvUtilisateurs.Columns[name].Width = width;
        }

        private void AlignCenter(string name)
        {
            if (!dgvUtilisateurs.Columns.Contains(name)) return;
            dgvUtilisateurs.Columns[name].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
        // ✅ 1) ChargerUtilisateurs() SANS mot de passe + colonnes propres

        // ✅ 4) Validation simple avant insertion
        private bool ValiderFormulaire(out string erreur)
        {
            erreur = null;

            string nomUtilisateur = (txtNomUtilisateur.Text ?? "").Trim();
            string mdp = txtMotDePasse.Text ?? "";
            string role = (cmbRole.Text ?? "").Trim();
            string email = (txtEmail.Text ?? "").Trim();
            string tel = (txtTelephone.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(nomUtilisateur))
            {
                erreur = "Nom d'utilisateur obligatoire.";
                return false;
            }

            if (nomUtilisateur.Length < 3)
            {
                erreur = "Nom d'utilisateur trop court (min 3).";
                return false;
            }

            if (string.IsNullOrWhiteSpace(mdp))
            {
                erreur = "Mot de passe obligatoire.";
                return false;
            }

            if (mdp.Length < 6)
            {
                erreur = "Mot de passe trop court (min 6).";
                return false;
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                erreur = "Rôle obligatoire.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                // validation simple
                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    erreur = "Email invalide.";
                    return false;
                }
            }

            // Tel : optionnel, mais tu peux forcer un format si tu veux
            if (tel.Length > 0 && tel.Length < 6)
            {
                erreur = "Téléphone invalide.";
                return false;
            }

            // confirmation mdp
            if ((txtMotDePasse.Text ?? "") != (txtConfirmationMotDePasse.Text ?? ""))
            {
                erreur = ConfigSysteme.Traduire("MotDePasseNonCorrespond") ?? "Les mots de passe ne correspondent pas.";
                return false;
            }

            return true;
        }

        private void ChargerEntreprises()
        {
            cmbEntreprise.Items.Clear();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"
SELECT IdEntreprise, Nom
FROM dbo.Entreprise
WHERE Actif = 1
ORDER BY Nom;", con))
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        cmbEntreprise.Items.Add(
                            new ComboboxItem(rd["Nom"].ToString(), Convert.ToInt32(rd["IdEntreprise"]))
                        );
                    }
                }
            }

            if (cmbEntreprise.Items.Count > 0)
                cmbEntreprise.SelectedIndex = 0;
        }

        private void ChargerMagasins(int idEntreprise)
        {
            cmbMagasin.Items.Clear();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"
SELECT IdMagasin, Nom
FROM dbo.Magasin
WHERE Actif = 1 AND IdEntreprise = @IdEntreprise
ORDER BY Nom;", con))
                {
                    cmd.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value = idEntreprise;

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            cmbMagasin.Items.Add(
                                new ComboboxItem(rd["Nom"].ToString(), Convert.ToInt32(rd["IdMagasin"]))
                            );
                        }
                    }
                }
            }

            if (cmbMagasin.Items.Count > 0)
                cmbMagasin.SelectedIndex = 0;
        }


        private void ChargerUtilisateurs()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string where = "";
                    if (_filtreUsers == FiltreUsers.Actifs) where = "WHERE u.Actif = 1";
                    else if (_filtreUsers == FiltreUsers.Inactifs) where = "WHERE u.Actif = 0";

                    string sql = $@"
SELECT TOP (1000)
    u.ID,
    u.IdEntreprise,
    e.Nom AS NomEntreprise,
    u.IdMagasin,
    m.Nom AS NomMagasin,
    u.NomUtilisateur,
    u.Nom,
    u.Prenom,
    u.Role,
    u.Actif,
    CASE WHEN u.Actif=1 THEN 'ACTIF' ELSE 'INACTIF' END AS Statut,
    u.Email,
    u.Telephone,
    u.DateCreation,
    u.AvatarPath
FROM dbo.Utilisateurs u
LEFT JOIN dbo.Entreprise e ON e.IdEntreprise = u.IdEntreprise
LEFT JOIN dbo.Magasin m ON m.IdMagasin = u.IdMagasin
{where}
ORDER BY u.ID DESC;";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(sql, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvUtilisateurs.DataSource = dt;
                    }
                }

                AjusterColonnesDgvUtilisateurs();
                AjouterMenuContextuelUsers();// si tu veux garder tes réglages
                  // ta méthode de largeur + scroll

                // ✅ cacher colonnes techniques
                if (dgvUtilisateurs.Columns.Contains("ID")) dgvUtilisateurs.Columns["ID"].Visible = false;
                if (dgvUtilisateurs.Columns.Contains("IdEntreprise")) dgvUtilisateurs.Columns["IdEntreprise"].Visible = false;
                if (dgvUtilisateurs.Columns.Contains("IdMagasin")) dgvUtilisateurs.Columns["IdMagasin"].Visible = false;
                if (dgvUtilisateurs.Columns.Contains("Actif")) dgvUtilisateurs.Columns["Actif"].Visible = false;

                // ✅ optionnel : largeur statut
                if (dgvUtilisateurs.Columns.Contains("Statut"))
                    dgvUtilisateurs.Columns["Statut"].Width = 95;

                // ✅ sélectionner première ligne pour que chkActif reflète l'état
                if (dgvUtilisateurs.Rows.Count > 0)
                {
                    dgvUtilisateurs.ClearSelection();
                    dgvUtilisateurs.Rows[0].Selected = true;
                    dgvUtilisateurs.CurrentCell = dgvUtilisateurs.Rows[0].Cells["NomUtilisateur"];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show((ConfigSysteme.Traduire("Erreur") ?? "Erreur") + " : " + ex.Message,
                    ConfigSysteme.Traduire("Erreur") ?? "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool DesactiverUtilisateur(int idUser)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Utilisateurs
SET Actif = 0
WHERE ID = @id AND Actif = 1;", con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUser;
                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0) throw new Exception("Utilisateur introuvable ou déjà inactif.");
                    }
                }

                ConfigSysteme.AjouterAuditLog("DESACTIVER_USER", "Utilisateur désactivé ID=" + idUser, "Succès");
                return true;
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("DESACTIVER_USER", "Échec désactivation ID=" + idUser + " | " + ex.Message, "Échec");
                MessageBox.Show("Erreur : " + ex.Message);
                return false;
            }
        }

        private bool ReactiverUtilisateur(int idUser)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Utilisateurs
SET Actif = 1
WHERE ID = @id AND Actif = 0;", con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUser;
                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0) throw new Exception("Utilisateur introuvable ou déjà actif.");
                    }
                }

                ConfigSysteme.AjouterAuditLog("REACTIVER_USER", "Utilisateur réactivé ID=" + idUser, "Succès");
                return true;
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("REACTIVER_USER", "Échec réactivation ID=" + idUser + " | " + ex.Message, "Échec");
                MessageBox.Show("Erreur : " + ex.Message);
                return false;
            }
        }


        // ✅ 2) PBKDF2 : fonctions utilitaires (hash + salt)
        // Stockage recommandé: MotDePasseHash VARBINARY(32), MotDePasseSalt VARBINARY(16)

       
        private void btnChangerAvatar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
                ofd.Title = "Choisir un avatar";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _selectedAvatarPath = ofd.FileName;
                    picAvatar.ImageLocation = _selectedAvatarPath;
                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!ValiderFormulaire(out string err))
            {
                MessageBox.Show(err,
                    ConfigSysteme.Traduire("Erreur") ?? "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ C# 7.3 : pas de "is not"
            var ent = cmbEntreprise.SelectedItem as ComboboxItem;
            if (ent == null)
            {
                MessageBox.Show("Choisis une entreprise.",
                    ConfigSysteme.Traduire("Information") ?? "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var mag = cmbMagasin.SelectedItem as ComboboxItem;
            if (mag == null)
            {
                MessageBox.Show("Choisis un magasin.",
                    ConfigSysteme.Traduire("Information") ?? "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string nomUtilisateur = (txtNomUtilisateur.Text ?? "").Trim();
                string nom = (txtNom.Text ?? "").Trim();
                string prenom = (txtPrenom.Text ?? "").Trim();
                string role = (cmbRole.Text ?? "").Trim();
                bool actif = chkActif.Checked;
                string email = (txtEmail.Text ?? "").Trim();
                string tel = (txtTelephone.Text ?? "").Trim();
                DateTime dateCreation = dptDateCreation.Value;

                int idEntreprise = ent.Value;
                int idMagasin = mag.Value;

                // ✅ hash PBKDF2 (C# 7.3 compatible)
                int iterations = PBKDF2_ITERATIONS_DEFAULT;
                byte[] salt = CreateSalt(PBKDF2_SALT_SIZE);
                byte[] hash = HashPasswordPBKDF2(txtMotDePasse.Text, salt, iterations, PBKDF2_HASH_SIZE);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string sql = @"
INSERT INTO dbo.Utilisateurs
(IdEntreprise, IdMagasin, NomUtilisateur, MotDePasseHash, MotDePasseSalt, MotDePasseIterations, Nom, Prenom, Role, Actif, Email, Telephone, DateCreation, AvatarPath)
VALUES
(@IdEntreprise, @IdMagasin, @NomUtilisateur, @MotDePasseHash, @MotDePasseSalt, @MotDePasseIterations, @Nom, @Prenom, @Role, @Actif, @Email, @Telephone, @DateCreation, @AvatarPath);";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value = idEntreprise;
                        cmd.Parameters.Add("@IdMagasin", SqlDbType.Int).Value = idMagasin;

                        cmd.Parameters.Add("@NomUtilisateur", SqlDbType.NVarChar, 255).Value = nomUtilisateur;
                        cmd.Parameters.Add("@MotDePasseHash", SqlDbType.VarBinary, 32).Value = hash;
                        cmd.Parameters.Add("@MotDePasseSalt", SqlDbType.VarBinary, 16).Value = salt;

                        cmd.Parameters.Add("@Nom", SqlDbType.NVarChar, 100).Value =
                            string.IsNullOrWhiteSpace(nom) ? (object)DBNull.Value : nom;

                        cmd.Parameters.Add("@Prenom", SqlDbType.NVarChar, 100).Value =
                            string.IsNullOrWhiteSpace(prenom) ? (object)DBNull.Value : prenom;

                        cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 255).Value = role;

                        cmd.Parameters.Add("@Actif", SqlDbType.Bit).Value = actif;

                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value =
                            string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email;

                        cmd.Parameters.Add("@Telephone", SqlDbType.NVarChar, 50).Value =
                            string.IsNullOrWhiteSpace(tel) ? (object)DBNull.Value : tel;

                        cmd.Parameters.Add("@DateCreation", SqlDbType.DateTime).Value = dateCreation;

                        cmd.Parameters.Add("@AvatarPath", SqlDbType.NVarChar, 500).Value =
                            string.IsNullOrWhiteSpace(_selectedAvatarPath) ? (object)DBNull.Value : _selectedAvatarPath;
                        cmd.Parameters.Add("@MotDePasseIterations", SqlDbType.Int).Value = iterations;

                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog("Utilisateurs",
                    "Nouvel utilisateur : " + nomUtilisateur + " (" + prenom + " " + nom + "), Rôle: " + role +
                    ", Ent:" + idEntreprise + " Mag:" + idMagasin,
                    "Succès");

                MessageBox.Show(
                    ConfigSysteme.Traduire("UtilisateurEnregistre") ?? "Utilisateur enregistré avec succès.",
                    ConfigSysteme.Traduire("Information") ?? "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                ChargerUtilisateurs();
            }
            catch (SqlException ex)
            {
                // ✅ index unique : 2601/2627
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    MessageBox.Show("Ce nom d'utilisateur existe déjà pour ce magasin.",
                        ConfigSysteme.Traduire("Erreur") ?? "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show((ConfigSysteme.Traduire("Erreur") ?? "Erreur") + " : " + ex.Message,
                    ConfigSysteme.Traduire("Erreur") ?? "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show((ConfigSysteme.Traduire("Erreur") ?? "Erreur") + " : " + ex.Message,
                    ConfigSysteme.Traduire("Erreur") ?? "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ 7) Sélection utilisateur depuis la grille (pour supprimer par ID + remplir champs)
        private void dgvUtilisateurs_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUtilisateurs.CurrentRow == null) return;

            object idObj = dgvUtilisateurs.CurrentRow.Cells["ID"].Value;
            if (idObj == null || idObj == DBNull.Value) return;

            _selectedUserId = Convert.ToInt32(idObj);

            txtNomUtilisateur.Text = dgvUtilisateurs.CurrentRow.Cells["NomUtilisateur"].Value?.ToString();
            txtNom.Text = dgvUtilisateurs.CurrentRow.Cells["Nom"].Value?.ToString();
            txtPrenom.Text = dgvUtilisateurs.CurrentRow.Cells["Prenom"].Value?.ToString();
            cmbRole.Text = dgvUtilisateurs.CurrentRow.Cells["Role"].Value?.ToString();

            bool actif = true; // par défaut
            object v = dgvUtilisateurs.CurrentRow.Cells["Actif"]?.Value;

            if (v != null && v != DBNull.Value)
                actif = Convert.ToBoolean(v);

            chkActif.Checked = actif;

            // ✅ si tu as 2 boutons
            if (btnDesactiverUser != null) btnDesactiverUser.Visible = actif;
            if (btnReactiverUser != null) btnReactiverUser.Visible = !actif;

            txtEmail.Text = dgvUtilisateurs.CurrentRow.Cells["Email"].Value?.ToString();
            txtTelephone.Text = dgvUtilisateurs.CurrentRow.Cells["Telephone"].Value?.ToString();

            if (DateTime.TryParse(dgvUtilisateurs.CurrentRow.Cells["DateCreation"].Value?.ToString(), out DateTime dc))
                dptDateCreation.Value = dc;

            _selectedAvatarPath = dgvUtilisateurs.CurrentRow.Cells["AvatarPath"].Value?.ToString();
            picAvatar.ImageLocation = string.IsNullOrWhiteSpace(_selectedAvatarPath) ? null : _selectedAvatarPath;

            // ✅ sélectionner entreprise + magasin
            int idEntreprise = 0, idMagasin = 0;

            int.TryParse(dgvUtilisateurs.CurrentRow.Cells["IdEntreprise"].Value?.ToString(), out idEntreprise);
            int.TryParse(dgvUtilisateurs.CurrentRow.Cells["IdMagasin"].Value?.ToString(), out idMagasin);

            // Entreprise
            for (int i = 0; i < cmbEntreprise.Items.Count; i++)
            {
                if (cmbEntreprise.Items[i] is ComboboxItem it && it.Value == idEntreprise)
                {
                    cmbEntreprise.SelectedIndex = i;
                    break;
                }
            }

            // Magasins (recharger ceux de l’entreprise puis select)
            ChargerMagasins(idEntreprise);

            for (int i = 0; i < cmbMagasin.Items.Count; i++)
            {
                if (cmbMagasin.Items[i] is ComboboxItem it && it.Value == idMagasin)
                {
                    cmbMagasin.SelectedIndex = i;
                    break;
                }
            }
        }


        private void AjouterMenuContextuelUsers()
        {
            var menu = new ContextMenuStrip();

            var itemDesactiver = new ToolStripMenuItem("🚫 Désactiver (INACTIF)");
            itemDesactiver.Click += (s, e) => btnDesactiverUser_Click(s, e);

            var itemReactiver = new ToolStripMenuItem("✅ Réactiver (ACTIF)");
            itemReactiver.Click += (s, e) => btnReactiverUser_Click(s, e);

            menu.Items.Add(itemDesactiver);
            menu.Items.Add(itemReactiver);
            menu.Items.Add(new ToolStripSeparator());

            var itemActifs = new ToolStripMenuItem("Afficher : Actifs");
            itemActifs.Click += (s, e) => { _filtreUsers = FiltreUsers.Actifs; ChargerUtilisateurs(); };

            var itemInactifs = new ToolStripMenuItem("Afficher : Inactifs");
            itemInactifs.Click += (s, e) => { _filtreUsers = FiltreUsers.Inactifs; ChargerUtilisateurs(); };

            var itemTous = new ToolStripMenuItem("Afficher : Tous");
            itemTous.Click += (s, e) => { _filtreUsers = FiltreUsers.Tous; ChargerUtilisateurs(); };

            menu.Items.Add(itemActifs);
            menu.Items.Add(itemInactifs);
            menu.Items.Add(itemTous);

            dgvUtilisateurs.ContextMenuStrip = menu;
        }


        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (_selectedUserId <= 0)
            {
                MessageBox.Show("Sélectionne d'abord un utilisateur dans la liste.",
                    ConfigSysteme.Traduire("Information") ?? "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool actifActuel = chkActif.Checked; // vient de SelectionChanged
            bool nouvelEtat = !actifActuel;

            string action = nouvelEtat ? "Activer" : "Désactiver";

            var confirm = MessageBox.Show(
                action + " l'utilisateur '" + txtNomUtilisateur.Text + "' ?",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string sql = "UPDATE dbo.Utilisateurs SET Actif = @Actif WHERE ID = @ID";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@Actif", SqlDbType.Bit).Value = nouvelEtat;
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = _selectedUserId;

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            ConfigSysteme.AjouterAuditLog("Utilisateurs",
                                action + " utilisateur : " + _selectedUserId + " (" + txtNomUtilisateur.Text + ")",
                                "Succès");

                            MessageBox.Show(
                                action + " effectué.",
                                ConfigSysteme.Traduire("Information") ?? "Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ChargerUtilisateurs();
                        }
                        else
                        {
                            MessageBox.Show(
                                ConfigSysteme.Traduire("UtilisateurNonTrouve") ?? "Utilisateur non trouvé.",
                                ConfigSysteme.Traduire("Information") ?? "Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Utilisateurs",
                    "Erreur toggle Actif user " + _selectedUserId + ": " + ex.Message, "Échec");

                MessageBox.Show(
                    (ConfigSysteme.Traduire("Erreur") ?? "Erreur") + " : " + ex.Message,
                    ConfigSysteme.Traduire("Erreur") ?? "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnDetails_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ConfigSysteme.Traduire("FonctionDetailsNonImplementee") ?? "Fonction Détails à implémenter.");
        }

        private void btnDesactiverUser_Click(object sender, EventArgs e)
        {
            if (_selectedUserId <= 0) { MessageBox.Show("Sélectionne un utilisateur."); return; }

            var r = MessageBox.Show(
                "Désactiver cet utilisateur ?\n\n✅ Données intactes\n❌ Connexion bloquée",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (r != DialogResult.Yes) return;

            GetScopeFromSelection(out int idEntreprise, out int idMagasin);

            var res = SecurityService.TryAuthorize(this, new AuthRequest
            {
                ActionCode = "USER_DISABLE",
                Title = "Désactivation utilisateur",
                Reference = "USER:" + _selectedUserId,
                Details = $"Demande désactivation user={_selectedUserId} ({txtNomUtilisateur.Text}) | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste}) | Ent={idEntreprise} Mag={idMagasin}",
                AlwaysSignature = true,
                RiskLevel = 2,
                TargetId = _selectedUserId,
                IdEntreprise = (idEntreprise > 0 ? idEntreprise : (int?)null),
                IdMagasin = (idMagasin > 0 ? idMagasin : (int?)null),
                Scope = AuthScope.Magasin
            });

            if (!res.Allowed)
            {
                MessageBox.Show(res.DenyReason ?? "Accès interdit.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (DesactiverUtilisateur(_selectedUserId))
                ChargerUtilisateurs();
        }


        private void btnReactiverUser_Click(object sender, EventArgs e)
        {
            if (_selectedUserId <= 0) { MessageBox.Show("Sélectionne un utilisateur."); return; }

            GetScopeFromSelection(out int idEntreprise, out int idMagasin);

            var res = SecurityService.TryAuthorize(this, new AuthRequest
            {
                ActionCode = "USER_REACTIVATE",
                Title = "Réactivation utilisateur",
                Reference = "USER:" + _selectedUserId,
                Details = $"Demande réactivation user={_selectedUserId} ({txtNomUtilisateur.Text}) | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste}) | Ent={idEntreprise} Mag={idMagasin}",
                AlwaysSignature = true,
                RiskLevel = 2,
                TargetId = _selectedUserId,
                IdEntreprise = (idEntreprise > 0 ? idEntreprise : (int?)null),
                IdMagasin = (idMagasin > 0 ? idMagasin : (int?)null),
                Scope = AuthScope.Magasin
            });

            if (!res.Allowed)
            {
                MessageBox.Show(res.DenyReason ?? "Accès interdit.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ReactiverUtilisateur(_selectedUserId))
                ChargerUtilisateurs();
        }
    }
}