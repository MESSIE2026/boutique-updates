using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using System.Data.SqlClient;
using System.Windows.Forms;
using BoutiqueRebuildFixed.Localization;

namespace BoutiqueRebuildFixed
{
    public partial class FormConfigurationSysteme : FormBase
    {
        private bool _initDone = false;
        private CheckBox chkWindowsAuth;
        private Button btnResetSql;

        private List<CultureInfo> _cultures;
        private bool _lockLangEvent = false;

        public class LangItem
        {
            public string Label { get; set; }      // affiché: Français / Anglais
            public string Culture { get; set; }    // stocké: fr-FR / en-US
            public override string ToString() => Label;
        }
        public FormConfigurationSysteme()
        {
            InitializeComponent();

            

            this.Load += FormConfigurationSysteme_Load;

            cbLangue.SelectedIndexChanged += CbLangue_SelectedIndexChanged;
            cbTheme.SelectedIndexChanged += CbTheme_SelectedIndexChanged;
        }
        private void FormConfigurationSysteme_Load(object sender, EventArgs e)
        {
            _initDone = false;
            ChargerCombosLangueTheme();// important
            ChargerLanguesSimples();   // ✅ remet Français/Anglais + thèmes
            InitialiserUI();             // tes champs SQL + checkbox etc.
            _initDone = true;            // seulement à la fin

            // ✅ On force une mise à jour propre (FormBase fera aussi, mais ici c'est ok)
        }

        private void ChargerLanguesSimples()
        {
            _lockLangEvent = true;
            try
            {
                cbLangue.BeginUpdate();

                cbLangue.DataSource = null;
                cbLangue.Items.Clear();

                var items = new List<LangItem>
        {
            new LangItem { Label = "Français", Culture = "fr-FR" },
            new LangItem { Label = "Anglais",  Culture = "en-US" } // ou en-GB
        };

                cbLangue.DisplayMember = "Label";
                cbLangue.ValueMember = "Culture";
                cbLangue.DataSource = items;

                // Sélectionner la langue enregistrée
                cbLangue.SelectedValue = string.IsNullOrWhiteSpace(ConfigSysteme.Langue) ? "fr-FR" : ConfigSysteme.Langue;
            }
            finally
            {
                cbLangue.EndUpdate();
                _lockLangEvent = false;
            }
        }

        private void ChargerToutesLanguesDansCombo()
        {
            _cultures = CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .OrderBy(c => c.NativeName)
                .ToList();

            // Bind
            cbLangue.BeginUpdate();
            try
            {
                cbLangue.DataSource = null;
                cbLangue.DisplayMember = "NativeName"; // affichage
                cbLangue.ValueMember = "Name";         // valeur = "fr-FR"
                cbLangue.DataSource = _cultures;

                // ✅ Recherche / AutoComplete
                cbLangue.DropDownStyle = ComboBoxStyle.DropDown; // permet de taper
                cbLangue.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cbLangue.AutoCompleteSource = AutoCompleteSource.CustomSource;

                var ac = new AutoCompleteStringCollection();
                foreach (var c in _cultures)
                    ac.Add($"{c.NativeName} [{c.Name}]"); // ex: Français (France) [fr-FR]
                cbLangue.AutoCompleteCustomSource = ac;
            }
            finally
            {
                cbLangue.EndUpdate();
            }

            // sélectionner langue actuelle (code)
            _lockLangEvent = true;
            try
            {
                cbLangue.SelectedValue = ConfigSysteme.Langue; // ex: fr-FR
            }
            finally
            {
                _lockLangEvent = false;
            }

            // ✅ Filtrage quand l'utilisateur tape (optionnel mais très utile)
            cbLangue.TextUpdate -= cbLangue_TextUpdate;
            cbLangue.TextUpdate += cbLangue_TextUpdate;
        }

        private void cbLangue_TextUpdate(object sender, EventArgs e)
        {
            if (_cultures == null || _cultures.Count == 0) return;

            string q = (cbLangue.Text ?? "").Trim();
            if (q.Length < 2) return; // évite de filtrer trop tôt

            // Cherche par nom natif, nom anglais ou code
            var filtered = _cultures
                .Where(c =>
                    c.NativeName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    c.EnglishName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    c.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(50) // limite pour rester rapide
                .ToList();

            if (filtered.Count == 0) return;

            int caret = cbLangue.SelectionStart;

            cbLangue.BeginUpdate();
            try
            {
                cbLangue.DataSource = null;
                cbLangue.DisplayMember = "NativeName";
                cbLangue.ValueMember = "Name";
                cbLangue.DataSource = filtered;

                cbLangue.DroppedDown = true;
                cbLangue.Text = q;
                cbLangue.SelectionStart = caret;
                cbLangue.SelectionLength = 0;
            }
            finally
            {
                cbLangue.EndUpdate();
            }
        }


        private void ChargerCombosLangueTheme()
        {
            // 1) Remplir les items (si vides ou si DataSource les a écrasés)
            cbLangue.BeginUpdate();
            cbTheme.BeginUpdate();

            try
            {
                cbLangue.DataSource = null;
                cbTheme.DataSource = null;

                cbLangue.Items.Clear();
                cbTheme.Items.Clear();

                // ✅ Langues
                cbLangue.Items.Add("Français");
                cbLangue.Items.Add("Anglais");

                // ✅ Thèmes
                cbTheme.Items.Add("Sombre");
                cbTheme.Items.Add("Clair");
                cbTheme.Items.Add("Clair Design");
            }
            finally
            {
                cbLangue.EndUpdate();
                cbTheme.EndUpdate();
            }

            // 2) Sélectionner la valeur courante
            // (au cas où ConfigSysteme.Theme/Langue contient déjà une valeur)
            SelectComboItem(cbLangue, ConfigSysteme.Langue, fallbackIndex: 0);
            SelectComboItem(cbTheme, ConfigSysteme.Theme, fallbackIndex: 0);
        }

        private void SelectComboItem(ComboBox cb, string value, int fallbackIndex)
        {
            if (cb.Items.Count == 0) return;

            int idx = -1;
            if (!string.IsNullOrWhiteSpace(value))
            {
                for (int i = 0; i < cb.Items.Count; i++)
                {
                    if (string.Equals(cb.Items[i].ToString(), value, StringComparison.OrdinalIgnoreCase))
                    {
                        idx = i;
                        break;
                    }
                }
            }

            cb.SelectedIndex = (idx >= 0) ? idx : Math.Min(fallbackIndex, cb.Items.Count - 1);
        }

        private void InitialiserUI()
        {
            // ✅ créer le checkbox 1 fois
            if (chkWindowsAuth == null)
            {
                chkWindowsAuth = new CheckBox
                {
                    Name = "chkWindowsAuth",
                    Text = "Windows Auth (Integrated Security)",
                    AutoSize = true
                };

                // ✅ même parent que le bouton Tester (IMPORTANT)
                var parent = btnTesterConnexion.Parent;   // Panel / GroupBox / TabPage
                chkWindowsAuth.Parent = parent;

                // ✅ position juste en dessous du bouton Tester
                chkWindowsAuth.Left = btnTesterConnexion.Left;
                chkWindowsAuth.Top = btnTesterConnexion.Bottom + 6;

                chkWindowsAuth.BringToFront();

                // ✅ optionnel: quand on coche, désactiver user/mdp
                chkWindowsAuth.CheckedChanged += (s, e) =>
                {
                    bool win = chkWindowsAuth.Checked;
                    txtUtilisateur.Enabled = !win;
                    txtMotDePasse.Enabled = !win;

                    if (win)
                    {
                        txtUtilisateur.Text = "";
                        txtMotDePasse.Text = "";
                    }
                };
            }

            if (btnResetSql == null)
            {
                btnResetSql = new Button
                {
                    Name = "btnResetSql",
                    Text = "Réinitialiser Connexion",
                    Width = btnTesterConnexion.Width,
                    Height = btnTesterConnexion.Height
                };

                // même parent que le bouton Tester
                var parent = btnTesterConnexion.Parent;
                btnResetSql.Parent = parent;

                // juste en dessous du checkbox (ou du bouton Tester si tu préfères)
                btnResetSql.Left = btnTesterConnexion.Left;
                btnResetSql.Top = chkWindowsAuth.Bottom + 8;

                btnResetSql.Click += btnResetSql_Click;
                btnResetSql.BringToFront();
            }

            

            // ✅ charger valeurs actuelles
            txtServeur.Text = ConfigSysteme.Serveur;
            txtBase.Text = ConfigSysteme.BaseDeDonnees;
            txtUtilisateur.Text = ConfigSysteme.Utilisateur;
            txtMotDePasse.Text = string.Empty;
            txtPort.Text = ConfigSysteme.Port.ToString();

            // ✅ état du checkbox + appliquer enable/disable
            chkWindowsAuth.Checked = ConfigSysteme.SQL.UseWindowsAuth;
            txtUtilisateur.Enabled = !chkWindowsAuth.Checked;
            txtMotDePasse.Enabled = !chkWindowsAuth.Checked;
        }

        private void btnResetSql_Click(object sender, EventArgs e)
        {
            var ok = MessageBox.Show(
                "Cette action va supprimer la configuration SQL enregistrée sur ce PC.\n\n" +
                "Après ça, tu devras reconfigurer Serveur / Base / Login.\n\n" +
                "Continuer ?",
                "Réinitialiser",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (ok != DialogResult.Yes) return;

            ConfigSysteme.ResetSqlConfig();

            // vider l'UI
            txtServeur.Text = @".\SQLEXPRESS";
            txtBase.Text = "MaBaseSQL2019";
            txtPort.Text = "1433";
            txtUtilisateur.Text = "";
            txtMotDePasse.Text = "";
            chkWindowsAuth.Checked = false;

            MessageBox.Show("Configuration SQL réinitialisée. Reconfigure puis clique Tester Connexion.",
                "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        private void CbLangue_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_initDone) return;
            if (_lockLangEvent) return;
            if (cbLangue.SelectedValue == null) return;

            // 1) Code culture sélectionné ("fr-FR" / "en-US")
            string cultureCode = cbLangue.SelectedValue.ToString();

            // 2) Sauver la config (ça met ConfigSysteme.Langue)
            ConfigSysteme.EnregistrerConfig(cultureCode, ConfigSysteme.Theme);

            // 3) Choisir le fichier JSON selon la langue
            string file = cultureCode.StartsWith("en", StringComparison.OrdinalIgnoreCase)
                ? "en.json"
                : "fr.json";

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lang", file);

            // 4) Charger le JSON + appliquer partout
            Traductions.Charger(path);

            foreach (Form f in Application.OpenForms)
                Traductions.Appliquer(f);
        }

        private void CbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_initDone) return;
            if (cbTheme.SelectedItem == null) return;

            string nouveauTheme = cbTheme.SelectedItem.ToString();

            // ✅ UNE action (déclenche OnThemeChange via ConfigSysteme.Theme setter)
            ConfigSysteme.EnregistrerConfig(ConfigSysteme.Langue, nouveauTheme);

            // ✅ refresh immédiat pour ce form
        }

        // Reste du code inchangé...
        private bool Valider()
        {
            if (string.IsNullOrWhiteSpace(txtServeur.Text))
            {
                MessageBox.Show("Le serveur est requis.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtBase.Text))
            {
                MessageBox.Show("La base de données est requise.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtPort.Text, out int port) || port <= 0)
            {
                MessageBox.Show("Le port est invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // ✅ Si WindowsAuth => pas besoin user
            if (!chkWindowsAuth.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtUtilisateur.Text))
                {
                    MessageBox.Show("L'utilisateur SQL est requis (ou coche Windows Auth).", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }


        private void btnTesterConnexion_Click(object sender, EventArgs e)
        {
            if (!Valider()) return;

            // On construit une config "temporaire" depuis l'UI
            var cfg = new ConfigSysteme.SqlConfig
            {
                Serveur = txtServeur.Text.Trim(),
                BaseDeDonnees = txtBase.Text.Trim(),
                Port = int.Parse(txtPort.Text),

                UseWindowsAuth = chkWindowsAuth.Checked,

                Utilisateur = txtUtilisateur.Text.Trim(),
                MotDePasse = txtMotDePasse.Text, // peut être vide -> contrôlé plus bas

                // reprendre options actuelles
                Encrypt = ConfigSysteme.SQL.Encrypt,
                TrustServerCertificate = ConfigSysteme.SQL.TrustServerCertificate,
                ConnectTimeout = ConfigSysteme.SQL.ConnectTimeout
            };

            try
            {
                var csb = new SqlConnectionStringBuilder
                {
                    DataSource = BuildDataSource(cfg.Serveur, cfg.Port),
                    InitialCatalog = cfg.BaseDeDonnees,
                    Encrypt = cfg.Encrypt,
                    TrustServerCertificate = cfg.TrustServerCertificate,
                    ConnectTimeout = cfg.ConnectTimeout,
                    MultipleActiveResultSets = true
                };

                if (cfg.UseWindowsAuth)
                {
                    csb.IntegratedSecurity = true;
                }
                else
                {
                    csb.UserID = cfg.Utilisateur ?? "";
                    csb.Password = cfg.MotDePasse ?? "";

                    if (string.IsNullOrWhiteSpace(csb.UserID))
                        throw new Exception("Utilisateur SQL vide.");

                    if (string.IsNullOrWhiteSpace(csb.Password))
                        throw new Exception("Mot de passe SQL vide. Saisis le mot de passe.");
                }

                using (var con = new SqlConnection(csb.ConnectionString))
                {
                    con.Open();
                }

                ConfigSysteme.AjouterAuditLog(
                    "Test Connexion",
                    $"Connexion réussie | Serveur={cfg.Serveur} | DB={cfg.BaseDeDonnees} | WindowsAuth={cfg.UseWindowsAuth}",
                    "Succès"
                );

                MessageBox.Show("Connexion réussie ✔", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;

                if (cfg.UseWindowsAuth && msg.IndexOf("untrusted domain", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    msg = "Windows Auth impossible dans ton réseau (domaine non approuvé).\n" +
                          "Décoche Windows Auth et utilise un login SQL (PDG + mot de passe).";
                }

                MessageBox.Show("Échec de connexion.\n\n" + msg, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            if (!Valider()) return;

            // 1) Langue + Thème
            string langue = (cbLangue.SelectedValue?.ToString() ?? ConfigSysteme.Langue);
            string theme = (cbTheme.SelectedItem?.ToString() ?? ConfigSysteme.Theme);
            ConfigSysteme.EnregistrerConfig(langue, theme);

            // 2) SQL (mise à jour propre)
            var cfg = ConfigSysteme.SQL;

            string oldUser = cfg.Utilisateur ?? "";
            bool oldWinAuth = cfg.UseWindowsAuth;

            cfg.Serveur = txtServeur.Text.Trim();
            cfg.BaseDeDonnees = txtBase.Text.Trim();
            cfg.Port = int.Parse(txtPort.Text);

            cfg.UseWindowsAuth = chkWindowsAuth.Checked;
            cfg.Utilisateur = txtUtilisateur.Text.Trim();

            bool userChanged = !string.Equals(oldUser, cfg.Utilisateur, StringComparison.OrdinalIgnoreCase);
            bool winAuthChanged = (oldWinAuth != cfg.UseWindowsAuth);

            if (cfg.UseWindowsAuth)
            {
                // Windows Auth -> on ignore user/mdp
                cfg.Utilisateur = "";
                cfg.MotDePasse = "";
            }
            else
            {
                // SQL Auth -> user obligatoire
                if (string.IsNullOrWhiteSpace(cfg.Utilisateur))
                {
                    MessageBox.Show("Utilisateur SQL requis (ou coche Windows Auth).",
                        "SQL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Si on a changé le user OU on vient de passer de WindowsAuth -> SQLAuth,
                // il faut forcer la saisie du mot de passe
                if ((userChanged || winAuthChanged) && string.IsNullOrWhiteSpace(txtMotDePasse.Text))
                {
                    MessageBox.Show("Saisis le mot de passe SQL correspondant à l'utilisateur.",
                        "SQL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Ne pas écraser l'ancien mot de passe si champ vide
                if (!string.IsNullOrWhiteSpace(txtMotDePasse.Text))
                    cfg.MotDePasse = txtMotDePasse.Text;
            }

            // 3) Sauver dans sqlconfig.json (DPAPI via EnregistrerSqlConfig)
            ConfigSysteme.EnregistrerSqlConfig(cfg);

            // 4) Audit
            ConfigSysteme.AjouterAuditLog(
                "Config système",
                $"Enregistrement config | Langue={langue} | Thème={theme} | Serveur={cfg.Serveur} | DB={cfg.BaseDeDonnees} | Port={cfg.Port} | WindowsAuth={cfg.UseWindowsAuth} | User={cfg.Utilisateur}",
                "Succès"
            );

            MessageBox.Show("Configuration enregistrée ✔", "Succès",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            Close();
        }

        private static string BuildDataSource(string serveur, int port)
        {
            serveur = (serveur ?? "").Trim();

            if (serveur.Contains("\\")) return serveur;  // instance => pas de port
            if (serveur.Contains(",")) return serveur;   // déjà IP,PORT

            if (port <= 0) port = 1433;
            return $"{serveur},{port}";
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
