using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace BoutiqueRebuildFixed
{
    internal static class ConfigSysteme
    {
        // ==========================================================
        // 0) SESSION / DEVISE / CONTEXTE
        // ==========================================================
        #region Session & Devise

        private static string devise = "FC";

        public static int SessionCaisseId { get; set; } = 0;
        public static int CaissierSessionId { get; set; } = 0;

        public static bool SessionOuverte => SessionCaisseId > 0;

        public static string GetDevise() => devise;

        
        public static void SetDevise(string nouvelleDevise)
        {
            if (!string.IsNullOrWhiteSpace(nouvelleDevise))
                devise = nouvelleDevise.Trim();
        }



        public static int IdEntreprise { get; set; }
        public static int IdMagasin { get; set; }
        public static int IdPoste { get; set; }

        // ====================== LOCKS & UI INVOKER (UNIQUE) ======================
        private static readonly object _sqlLock = new object();

        private static WeakReference<Control> _uiInvoker;

        public static void SetUiInvoker(Control control)
        {
            if (control == null) return;
            _uiInvoker = new WeakReference<Control>(control);
        }

        private static void RaiseOnUI(Action action)
        {
            if (action == null) return;

            try
            {
                Control c;
                if (_uiInvoker != null && _uiInvoker.TryGetTarget(out c) && c != null && !c.IsDisposed)
                {
                    if (c.InvokeRequired) c.BeginInvoke(action);
                    else action();
                    return;
                }
            }
            catch { /* ignore */ }

            // fallback
            action();
        }

        public static bool TryTestConnexion(out string err)
        {
            err = null;
            try
            {
                using (var con = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand("SELECT 1", con))
                {
                    con.Open();
                    cmd.ExecuteScalar();
                }
                return true;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }


        #endregion

        // ==========================================================
        // 0.1) DOSSIERS & FICHIERS CONFIG (APPDATA)
        // ==========================================================
        #region Paths & AppData

        private static readonly string ProgramDataDir =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BoutiqueRebuildFixed");

        private static void EnsureProgramDataDir()
        {
            try
            {
                if (!Directory.Exists(ProgramDataDir))
                    Directory.CreateDirectory(ProgramDataDir);
            }
            catch { }
        }

        private static string CheminConfig => Path.Combine(ProgramDataDir, "config.json");
        private static string CheminSqlConfig => Path.Combine(ProgramDataDir, IsDev ? "sqlconfig.dev.json" : "sqlconfig.json");
        private static string PrintersIniPath => Path.Combine(ProgramDataDir, "printers.ini");
        private static string CheminTraductionsDynamiques => Path.Combine(ProgramDataDir, "traductions_dynamiques.json");

        #endregion

        // ==========================================================
        // 0.2) IMPRIMANTES
        // ==========================================================
        #region Printers

        public static string ImprimanteTicketNom { get; set; } = "";
        public static string ImprimanteA4Nom { get; set; } = "";

        public static void LoadPrintersConfig()
        {
            try
            {
                EnsureProgramDataDir();
                if (!File.Exists(PrintersIniPath)) return;

                foreach (var line in File.ReadAllLines(PrintersIniPath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length != 2) continue;

                    string k = parts[0].Trim();
                    string v = parts[1].Trim();

                    if (k.Equals("ticket", StringComparison.OrdinalIgnoreCase))
                        ImprimanteTicketNom = v;
                    else if (k.Equals("a4", StringComparison.OrdinalIgnoreCase))
                        ImprimanteA4Nom = v;
                }
            }
            catch { /* silence */ }
        }

        public static void SavePrintersConfig()
        {
            try
            {
                EnsureProgramDataDir();
                var lines = new List<string>
                {
                    "ticket=" + (ImprimanteTicketNom ?? ""),
                    "a4=" + (ImprimanteA4Nom ?? "")
                };
                File.WriteAllLines(PrintersIniPath, lines);
            }
            catch { /* silence */ }
        }

        #endregion

        // ==========================================================
        // 1) ROLES SECURITE (SOURCE UNIQUE)
        // ==========================================================
        #region RolesSecurite

        public static class RolesSecurite
        {
            private static readonly string[] RolesAdmins =
            {
                "Superviseur",
                "Programmeur",
                "Gérant",
                "Directeur Général"
            };

            private static readonly string[] RolesManagers =
            {
                "Superviseur",
                "Gérant",
                "Directeur Général",
                "Programmeur",
            };

            private static readonly string[] RolesSeniors =
            {
                "Superviseur",
                "Gérant",
                "Programmeur",
                "Directeur Général"
            };

            public static bool EstAdmin(string role)
            {
                string rr = NormalizeRoleKey(role);
                return RolesAdmins.Any(r => NormalizeRoleKey(r) == rr);
            }

            public static bool EstManager(string role)
            {
                string rr = NormalizeRoleKey(role);
                return RolesManagers.Any(r => NormalizeRoleKey(r) == rr);
            }

            public static bool EstSenior(string role)
            {
                string rr = NormalizeRoleKey(role);
                return RolesSeniors.Any(r => NormalizeRoleKey(r) == rr);
            }

            public static string[] GetAdminRolesCopy() => RolesAdmins.ToArray();
        }

        internal static string NormalizeRoleKey(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Trim();

            string formD = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (char ch in formD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .Trim()
                     .ToLowerInvariant();
        }

        // ✅ Compat : appels existants
        public static bool EstRoleAdmin(string role) => RolesSecurite.EstAdmin(role);
        public static bool EstRoleManager(string role) => RolesSecurite.EstManager(role);
        public static bool EstRoleSenior(string role) => RolesSecurite.EstSenior(role);

        #endregion

        // ==========================================================
        // 2) SQL CONFIG + CONNECTION STRING
        // ==========================================================
        #region SQL Config

        public class SqlConfig
        {
            public string Serveur { get; set; } = @".\SQLEXPRESS";
            public int Port { get; set; } = 1433;
            public string BaseDeDonnees { get; set; } = "MaBaseSQL2019";

            // ✅ Choix du mode d'auth
            public bool UseWindowsAuth { get; set; } = false;

            // ✅ SQL Auth
            public string Utilisateur { get; set; } = "PDG";

            [JsonIgnore]
            public string MotDePasse { get; set; } = "";

            public string MotDePasseCrypt { get; set; } = "";

            public bool Encrypt { get; set; } = true;
            public bool TrustServerCertificate { get; set; } = true;
            public int ConnectTimeout { get; set; } = 5;
        }

        public static bool IsDev =>
#if DEBUG
    true;
#else
    false;
#endif
        public static void ResetSqlConfig()
        {
            try
            {
                EnsureProgramDataDir();

                if (File.Exists(CheminSqlConfig))
                    File.Delete(CheminSqlConfig);

                lock (_sqlLock)
                {
                    _sql = null; // force re-load -> va recréer cfg par défaut
                }
            }
            catch { /* ignore */ }
        }


        private static SqlConfig _sql;

        public static SqlConfig SQL
        {
            get
            {
                if (_sql != null) return _sql;
                lock (_sqlLock)
                {
                    if (_sql == null)
                        _sql = ChargerSqlConfig();
                    return _sql;
                }
            }
        }

        // ✅ Helpers publics pour modifier les infos proprement (et sauver)
        public static void UpdateSqlCredentials(string serveur, int port, string db,
            bool useWindowsAuth, string userSql, string passwordSql)
        {
            var cfg = SQL;

            cfg.Serveur = serveur ?? "";
            cfg.Port = port;
            cfg.BaseDeDonnees = db ?? "";
            cfg.UseWindowsAuth = useWindowsAuth;

            cfg.Utilisateur = userSql ?? "";
            cfg.MotDePasse = passwordSql ?? "";

            EnregistrerSqlConfig(cfg);
        }

        // ✅ Access rapides (compat)
        public static string Serveur
        {
            get { return SQL.Serveur; }
            set { var cfg = SQL; cfg.Serveur = value ?? ""; EnregistrerSqlConfig(cfg); }
        }

        public static int Port
        {
            get { return SQL.Port; }
            set { var cfg = SQL; cfg.Port = value; EnregistrerSqlConfig(cfg); }
        }

        public static string BaseDeDonnees
        {
            get { return SQL.BaseDeDonnees; }
            set { var cfg = SQL; cfg.BaseDeDonnees = value ?? ""; EnregistrerSqlConfig(cfg); }
        }

        public static string Utilisateur
        {
            get { return SQL.Utilisateur; }
            set { var cfg = SQL; cfg.Utilisateur = value ?? ""; EnregistrerSqlConfig(cfg); }
        }

        public static string MotDePasse
        {
            get { return SQL.MotDePasse; }
            set
            {
                var cfg = SQL;
                cfg.MotDePasse = value ?? "";
                EnregistrerSqlConfig(cfg);
            }
        }

        public static bool UseWindowsAuth
        {
            get { return SQL.UseWindowsAuth; }
            set
            {
                var cfg = SQL;
                cfg.UseWindowsAuth = value;
                EnregistrerSqlConfig(cfg);
            }
        }

        // ========================= DPAPI =========================

        // ========================= DPAPI =========================

        private static string DpapiEncryptToBase64(string plain)
        {
            if (string.IsNullOrEmpty(plain)) return "";
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plain);

                // ✅ même scope que le decrypt
                byte[] protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);

                return Convert.ToBase64String(protectedBytes);
            }
            catch { return ""; }
        }

        private static string DpapiDecryptFromBase64(string b64)
        {
            if (string.IsNullOrWhiteSpace(b64)) return "";
            try
            {
                byte[] protectedBytes = Convert.FromBase64String(b64);

                // ✅ même scope que le encrypt
                byte[] bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.LocalMachine);

                return Encoding.UTF8.GetString(bytes);
            }
            catch { return ""; }
        }

        private static void ChargerTraductionsLangues()
        {
            BoutiqueRebuildFixed.Localization.TraductionManager.LoadFromCulture(Langue);
        }


        // ========================= Load / Save =========================

        private static SqlConfig ChargerSqlConfig()
        {
            try
            {
                EnsureProgramDataDir();
                if (File.Exists(CheminSqlConfig))
                {
                    string json = File.ReadAllText(CheminSqlConfig, Encoding.UTF8);
                    var cfg = JsonConvert.DeserializeObject<SqlConfig>(json);
                    if (cfg != null)
                    {
                        // ✅ restaurer le mdp en mémoire
                        cfg.MotDePasse = DpapiDecryptFromBase64(cfg.MotDePasseCrypt);
                        return cfg;
                    }
                }
            }
            catch { /* ignore */ }

            return new SqlConfig();
        }

        public static void EnregistrerSqlConfig(SqlConfig cfg)
        {
            try
            {
                EnsureProgramDataDir();

                if (cfg == null) cfg = new SqlConfig();

                // ✅ chiffrer avant sérialisation
                cfg.MotDePasseCrypt = DpapiEncryptToBase64(cfg.MotDePasse ?? "");

                var json = JsonConvert.SerializeObject(cfg, Formatting.Indented);
                File.WriteAllText(CheminSqlConfig, json, Encoding.UTF8);

                lock (_sqlLock)
                {
                    _sql = cfg;
                }
            }
            catch { /* ignore */ }
        }

        // ========================= ConnectionString =========================

        public static string ConnectionString
        {
            get
            {
                var c = SQL;

                var csb = new SqlConnectionStringBuilder
                {
                    DataSource = BuildDataSource(c.Serveur, c.Port),
                    InitialCatalog = c.BaseDeDonnees,
                    Encrypt = c.Encrypt,
                    TrustServerCertificate = c.TrustServerCertificate,
                    ConnectTimeout = c.ConnectTimeout,
                    MultipleActiveResultSets = true
                };

                // Windows Auth
                if (c.UseWindowsAuth)
                {
                    csb.IntegratedSecurity = true;
                    return csb.ConnectionString;
                }

                // SQL Auth
                csb.UserID = (c.Utilisateur ?? "").Trim();

                // ⚠️ si pas de password, on ne crash pas ici
                // on laisse vide => l'échec sera au moment du con.Open()
                csb.Password = c.MotDePasse ?? "";

                // Si user vide, on tente Windows Auth par défaut (évite crash)
                if (string.IsNullOrWhiteSpace(csb.UserID))
                {
                    csb.IntegratedSecurity = true;
                }

                return csb.ConnectionString;
            }
        }


        public static bool HasValidSqlCredentials(out string message)
        {
            message = null;
            var c = SQL;

            if (c.UseWindowsAuth) return true;

            if (string.IsNullOrWhiteSpace(c.Utilisateur))
            {
                message = "Utilisateur SQL vide. Active Windows Auth ou renseigne un login SQL.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(c.MotDePasse))
            {
                message = "Mot de passe SQL vide pour l'utilisateur '" + c.Utilisateur + "'.";
                return false;
            }

            return true;
        }


        // ✅ Nettoie Serveur\Instance,Port (invalide)
        private static string BuildDataSource(string serveur, int port)
        {
            serveur = (serveur ?? "").Trim();
            if (string.IsNullOrWhiteSpace(serveur)) serveur = ".";

            if (serveur.Contains("\\") && serveur.Contains(","))
                serveur = serveur.Split(',')[0].Trim();

            if (serveur.Contains("\\")) return serveur;
            if (serveur.Contains(",")) return serveur;

            if (port <= 0) port = 1433;
            return serveur + "," + port;
        }

        // ✅ test rapide
        public static void TestConnexionSqlOrThrow()
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT 1", con))
            {
                con.Open();
                cmd.ExecuteScalar();
            }
        }

        #endregion

        // ==========================================================
        // 3) CONFIG JSON (Langue/Theme)
        // ==========================================================
        public static event Action OnLangueChange;
        public static event Action OnThemeChange;

        // ✅ on stocke le CODE culture, pas "Français"
        private static string _langue = "fr-FR";
        public static string Langue
        {
            get => _langue;
            set
            {
                // ✅ valeur par défaut sûre
                string v = string.IsNullOrWhiteSpace(value) ? "fr-FR" : value.Trim();

                // ✅ accepte "Français/Anglais" aussi (si tu as encore ça dans l'ancien json)
                v = CanonicalCulture(v);

                if (string.Equals(_langue, v, StringComparison.OrdinalIgnoreCase))
                    return;

                _langue = v;

                // ✅ recharge tes dicos internes si tu en as
                ChargerTraductionsLangues();
                ChargerTraductionsDynamiques();

                // ✅ déclenche sur UI thread
                RaiseOnUI(() => OnLangueChange?.Invoke());
            }
        }

        private static string _theme = "ClairDesign";
        public static string Theme
        {
            get => _theme;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                string canon = CanonicalTheme(value);
                if (string.Equals(_theme, canon, StringComparison.OrdinalIgnoreCase))
                    return;

                _theme = canon;
                RaiseOnUI(() => OnThemeChange?.Invoke());
            }
        }

        private class ConfigDonnees
        {
            public string Langue { get; set; }   // ✅ doit contenir fr-FR/en-US
            public string Theme { get; set; }
        }

        public static void ChargerConfig()
        {
            try
            {
                EnsureProgramDataDir();
                if (File.Exists(CheminConfig))
                {
                    string json = File.ReadAllText(CheminConfig);
                    var config = JsonConvert.DeserializeObject<ConfigDonnees>(json);

                    if (config != null)
                    {
                        if (!string.IsNullOrWhiteSpace(config.Langue))
                            Langue = config.Langue.Trim();

                        if (!string.IsNullOrWhiteSpace(config.Theme))
                            Theme = config.Theme.Trim();
                    }
                }
            }
            catch { /* ignore */ }

            // ✅ au cas où aucun fichier
            ChargerTraductionsLangues();
            ChargerTraductionsDynamiques();
        }


        // suppose que Langue/Theme sont des propriétés statiques existantes
        public static void EnregistrerConfig(string nouvelleLangue, string nouveauTheme)
        {
            try
            {
                EnsureProgramDataDir();

                string lang = CanonicalCulture(nouvelleLangue);
                string theme = CanonicalTheme(nouveauTheme);

                var config = new ConfigDonnees
                {
                    Langue = lang,
                    Theme = theme
                };

                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(CheminConfig, json);

                // ✅ ces setters déclenchent déjà les events + RaiseOnUI
                Langue = config.Langue;
                Theme = config.Theme;
            }
            catch { }
        }

        // ✅ accepte fr-FR/en-US/... et aussi "Français/Anglais" par compatibilité
        private static string CanonicalCulture(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "fr-FR";
            s = s.Trim();

            if (s.Equals("fr", StringComparison.OrdinalIgnoreCase)) return "fr-FR";
            if (s.Equals("en", StringComparison.OrdinalIgnoreCase)) return "en-US";

            if (s.IndexOf("fran", StringComparison.OrdinalIgnoreCase) >= 0) return "fr-FR";
            if (s.IndexOf("angl", StringComparison.OrdinalIgnoreCase) >= 0) return "en-US";

            // si on reçoit déjà un code style "sw-CD", on le garde
            return s;
        }

        private static string CanonicalTheme(string t)
        {
            if (string.IsNullOrWhiteSpace(t)) return "ClairDesign";

            t = t.Trim();
            var sb = new StringBuilder();
            foreach (char ch in t)
                if (!char.IsWhiteSpace(ch) && ch != '-' && ch != '_')
                    sb.Append(ch);

            string n = sb.ToString().ToLowerInvariant();

            if (n == "sombre" || n == "dark") return "Sombre";
            if (n == "clair" || n == "light") return "Clair";
            if (n == "clairdesign" || n == "design" || n == "default") return "ClairDesign";

            return "ClairDesign";
        }


        // ==========================================================
        // 4) TRADUCTIONS
        // ==========================================================
        #region Traductions

        private static readonly Dictionary<string, Dictionary<string, string>> TraductionsStatique =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["Français"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "btnVentes", "Ventes" },
                    { "btnClients", "Clients" },
                    { "btnProduits", "Produits" },
                    { "btnInventaire", "Inventaire" },
                    { "btnEmployes", "Employés" },
                    { "btnCaisse", "Caisse" },
                    { "btnDetails", "Détails" },
                    { "btnUtilisateurs", "Utilisateurs" },
                    { "btnDeconnexion", "Déconnexion" },
                    { "btnOuvrirStock", "Opérations Stock" },
                    { "btnOperationsStock", "Opérations Stock" },
                    { "btnDepenses", "Dépenses" },
                    { "btnPresenceAbsence", "Présence / Absence" },
                    { "btnSessionsCaisse", "Sessions Caisse" },
                    { "btnRetraitFidelite", "Retrait Fidélité" },
                    { "btnConfigSysteme", "Configuration Système" },
                    { "btnPermissions", "Permissions" }
                },
                ["Anglais"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "btnVentes", "Sales" },
                    { "btnClients", "Customers" },
                    { "btnProduits", "Products" },
                    { "btnInventaire", "Inventory" },
                    { "btnEmployes", "Employees" },
                    { "btnCaisse", "Cash Register" },
                    { "btnDetails", "Details" },
                    { "btnUtilisateurs", "Users" },
                    { "btnDeconnexion", "Logout" },
                    { "btnOuvrirStock", "Stock Operations" },
                    { "btnOperationsStock", "Stock Operations" },
                    { "btnDepenses", "Expenses" },
                    { "btnPresenceAbsence", "Attendance" },
                    { "btnSessionsCaisse", "Cash Sessions" },
                    { "btnRetraitFidelite", "Fidelity Cash-Out" },
                    { "btnConfigSysteme", "System Settings" },
                    { "btnPermissions", "Permissions" }
                }
            };

        private static Dictionary<string, string> TraductionsLangues =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, string> TraductionsDynamiques =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly string dossierLangues =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Langues");


        public static void ChargerTraductionsDynamiques(string cheminFichier = null)
        {
            try
            {
                EnsureProgramDataDir();
                if (string.IsNullOrWhiteSpace(cheminFichier))
                    cheminFichier = CheminTraductionsDynamiques;

                if (File.Exists(cheminFichier))
                {
                    string json = File.ReadAllText(cheminFichier);
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    TraductionsDynamiques = dict ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    TraductionsDynamiques = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
            }
            catch
            {
                TraductionsDynamiques = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static string LangueVersFichier(string l)
        {
            if (string.IsNullOrWhiteSpace(l)) return "fr.json";

            switch (l.Trim())
            {
                case "Anglais": return "en.json";
                case "Espagnol": return "es.json";
                case "Allemand": return "de.json";
                case "Chinois": return "zh.json";
                case "Arabe": return "ar.json";
                case "Russe": return "ru.json";
                case "Japonais": return "ja.json";
                case "Swahili": return "sw.json";
                case "Lingala": return "ln.json";
                case "Portugais": return "pt.json";
                default: return "fr.json";
            }
        }

        public static string Traduire(string cle)
        {
            if (string.IsNullOrWhiteSpace(cle)) return cle;
            cle = cle.Trim();

            if (TraductionsStatique.TryGetValue(Langue, out var dicSta)
                && dicSta.TryGetValue(cle, out var tSta))
                return tSta;

            if (TraductionsLangues.TryGetValue(cle, out var tLang))
                return tLang;

            if (TraductionsDynamiques.TryGetValue(cle, out var tDyn))
                return tDyn;

#if DEBUG
            System.Diagnostics.Debug.WriteLine($"[LANGUE] Clé manquante : {cle}");
#endif
            return cle;
        }

        #endregion

        // ==========================================================
        // 5) APPLIQUER TRADUCTIONS
        // ==========================================================
        #region Apply Translations

        public static void AppliquerTraductions(Control control)
        {
            if (control == null) return;

            if (!string.IsNullOrWhiteSpace(control.Name))
            {
                string t = Traduire(control.Name);
                if (!string.IsNullOrWhiteSpace(t) && t != control.Name)
                    control.Text = t;
            }

            if (control is MenuStrip ms)
                foreach (ToolStripItem item in ms.Items)
                    AppliquerTraductions(item);

            if (control is ContextMenuStrip cms)
                foreach (ToolStripItem item in cms.Items)
                    AppliquerTraductions(item);

            if (control is DataGridView dgv)
            {
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (!string.IsNullOrWhiteSpace(col.Name))
                    {
                        string h = Traduire(col.Name);
                        if (!string.IsNullOrWhiteSpace(h) && h != col.Name)
                            col.HeaderText = h;
                    }
                }
            }

            foreach (Control child in control.Controls)
                AppliquerTraductions(child);
        }

        public static void AppliquerTraductions(ToolStripItem item)
        {
            if (item == null) return;

            if (!string.IsNullOrWhiteSpace(item.Name))
            {
                string t = Traduire(item.Name);
                if (!string.IsNullOrWhiteSpace(t) && t != item.Name)
                    item.Text = t;
            }

            if (item is ToolStripMenuItem mi)
                foreach (ToolStripItem sub in mi.DropDownItems)
                    AppliquerTraductions(sub);
        }

        #endregion

        // ==========================================================
        // 6) THEME (SNAPSHOT DESIGNER + APPLY)
        // ==========================================================
        #region Theme

        private class ThemeSnapshot
        {
            public Color Back;
            public Color Fore;
            public bool HasBack;
            public bool HasFore;

            public bool IsDgv;
            public bool DgvEnableHeadersVisualStyles;
            public Color DgvBackgroundColor;
            public DataGridViewCellStyle DgvDefaultCellStyle;
            public DataGridViewCellStyle DgvColumnHeadersStyle;
            public DataGridViewCellStyle DgvRowHeadersStyle;
        }

        private static readonly ConditionalWeakTable<Control, ThemeSnapshot> _themeSnapshots
            = new ConditionalWeakTable<Control, ThemeSnapshot>();

        private static void CaptureThemeIfNeeded(Control c)
        {
            if (c == null) return;
            if (_themeSnapshots.TryGetValue(c, out _)) return;

            var snap = new ThemeSnapshot
            {
                Back = c.BackColor,
                Fore = c.ForeColor,
                HasBack = true,
                HasFore = true,
                IsDgv = c is DataGridView
            };

            if (c is DataGridView dgv)
            {
                snap.DgvEnableHeadersVisualStyles = dgv.EnableHeadersVisualStyles;
                snap.DgvBackgroundColor = dgv.BackgroundColor;

                snap.DgvDefaultCellStyle = dgv.DefaultCellStyle?.Clone();
                snap.DgvColumnHeadersStyle = dgv.ColumnHeadersDefaultCellStyle?.Clone();
                snap.DgvRowHeadersStyle = dgv.RowHeadersDefaultCellStyle?.Clone();
            }

            _themeSnapshots.Add(c, snap);
        }

        private static void CaptureThemeTree(Control c)
        {
            if (c == null) return;
            CaptureThemeIfNeeded(c);
            foreach (Control child in c.Controls)
                CaptureThemeTree(child);
        }

        private static void RestoreThemeTree(Control c)
        {
            if (c == null) return;

            if (_themeSnapshots.TryGetValue(c, out var snap))
            {
                if (snap.HasBack) c.BackColor = snap.Back;
                if (snap.HasFore) c.ForeColor = snap.Fore;

                if (c is DataGridView dgv && snap.IsDgv)
                {
                    dgv.EnableHeadersVisualStyles = snap.DgvEnableHeadersVisualStyles;
                    dgv.BackgroundColor = snap.DgvBackgroundColor;

                    if (snap.DgvDefaultCellStyle != null) dgv.DefaultCellStyle = snap.DgvDefaultCellStyle.Clone();
                    if (snap.DgvColumnHeadersStyle != null) dgv.ColumnHeadersDefaultCellStyle = snap.DgvColumnHeadersStyle.Clone();
                    if (snap.DgvRowHeadersStyle != null) dgv.RowHeadersDefaultCellStyle = snap.DgvRowHeadersStyle.Clone();
                }
            }

            foreach (Control child in c.Controls)
                RestoreThemeTree(child);
        }

        private static bool HasTag(Control c, string token)
        {
            if (c?.Tag == null) return false;
            var t = c.Tag.ToString() ?? "";
            return t.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private const string TAG_NoTheme = "NoTheme";
        private const string TAG_KeepBack = "KeepBack";
        private const string TAG_KeepFore = "KeepFore";
        private const string TAG_KeepDesigner = "KeepDesigner";

        public static void AppliquerTheme(Control control)
        {
            if (control == null) return;

            CaptureThemeTree(control);

            if (CanonicalTheme(Theme).Equals("ClairDesign", StringComparison.OrdinalIgnoreCase))
            {
                RestoreThemeTree(control);
                return;
            }

            if (Theme.Equals("Sombre", StringComparison.OrdinalIgnoreCase))
                AppliquerThemeRecursif(control, Color.FromArgb(30, 30, 30), Color.White);
            else
                AppliquerThemeRecursif(control, SystemColors.Control, SystemColors.ControlText);
        }

        private static void AppliquerThemeRecursif(Control control, Color backColor, Color foreColor)
        {
            if (control == null) return;

            CaptureThemeIfNeeded(control);

            bool noTheme = HasTag(control, TAG_NoTheme) || HasTag(control, TAG_KeepDesigner);
            bool keepBack = HasTag(control, TAG_KeepBack);
            bool keepFore = HasTag(control, TAG_KeepFore);

            if (Theme.Equals("Clair", StringComparison.OrdinalIgnoreCase))
            {
                if (control is TextBoxBase || control is ComboBox)
                {
                    keepBack = true;
                    keepFore = true;
                }
            }

            if (!noTheme)
            {
                if (!keepBack) control.BackColor = backColor;
                if (!keepFore) control.ForeColor = foreColor;

                if (control is DataGridView dgv)
                {
                    dgv.EnableHeadersVisualStyles = false;

                    if (!keepBack)
                    {
                        dgv.BackgroundColor = backColor;
                        dgv.DefaultCellStyle.BackColor = backColor;
                        dgv.ColumnHeadersDefaultCellStyle.BackColor = backColor;
                        dgv.RowHeadersDefaultCellStyle.BackColor = backColor;
                    }

                    if (!keepFore)
                    {
                        dgv.DefaultCellStyle.ForeColor = foreColor;
                        dgv.ColumnHeadersDefaultCellStyle.ForeColor = foreColor;
                        dgv.RowHeadersDefaultCellStyle.ForeColor = foreColor;
                    }

                    dgv.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                    dgv.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
                }
            }

            foreach (Control child in control.Controls)
                AppliquerThemeRecursif(child, backColor, foreColor);
        }

        #endregion

        // ==========================================================
        // 7) CONTEXT MENU (PRO)
        // ==========================================================
        #region ContextMenu

        public interface IContextMenuHandler { void MettreAJourTotaux(); }

        private static ContextMenuStrip menuContextuel;

        public static ContextMenuStrip MenuContextuel
        {
            get
            {
                if (menuContextuel == null)
                {
                    menuContextuel = new ContextMenuStrip();
                    menuContextuel.Opening += MenuContextuel_Opening;
                }
                return menuContextuel;
            }
        }

        private static void MenuContextuel_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var menu = (ContextMenuStrip)sender;
            var src = menu.SourceControl;

            menu.Items.Clear();

            if (src is DataGridView dgv) ConstruireMenuDataGrid(menu, dgv);
            else if (src is TextBoxBase tb) ConstruireMenuTexte(menu, tb);
            else ConstruireMenuGeneral(menu, src);

            BoutiqueRebuildFixed.Localization.TraductionManager.ApplyToContextMenu(menu);
        }

        private static void ConstruireMenuGeneral(ContextMenuStrip menu, Control src)
        {
            menu.Items.Add(new ToolStripMenuItem("Actualiser", null, (s, e) => src?.Refresh()) { Name = "cmsActualiser" });
        }

        private static void ConstruireMenuTexte(ContextMenuStrip menu, TextBoxBase tb)
        {
            menu.Items.Add(new ToolStripMenuItem("Annuler", null, (s, e) => { if (tb.CanUndo) tb.Undo(); }) { Name = "cmsUndo" });
            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add(new ToolStripMenuItem("Couper", null, (s, e) => tb.Cut()) { Name = "cmsCut" });
            menu.Items.Add(new ToolStripMenuItem("Copier", null, (s, e) => tb.Copy()) { Name = "cmsCopy" });
            menu.Items.Add(new ToolStripMenuItem("Coller", null, (s, e) => tb.Paste()) { Name = "cmsPaste" });
            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add(new ToolStripMenuItem("Tout sélectionner", null, (s, e) => tb.SelectAll()) { Name = "cmsSelectAll" });
            menu.Items.Add(new ToolStripMenuItem("Effacer", null, (s, e) => tb.Clear()) { Name = "cmsClear" });
        }

        private static void ConstruireMenuDataGrid(ContextMenuStrip menu, DataGridView dgv)
        {
            menu.Items.Add(new ToolStripMenuItem("Copier cellule", null, (s, e) => CopierCellule(dgv)) { Name = "cmsCopyCell" });
            menu.Items.Add(new ToolStripMenuItem("Copier ligne(s)", null, (s, e) => CopierLignes(dgv)) { Name = "cmsCopyRows" });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Supprimer ligne(s)", null, (s, e) => SupprimerLignes(dgv)) { Name = "cmsSupprimer" });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Exporter CSV", null, (s, e) => ExporterCsv(dgv)) { Name = "cmsExportCsv" });
            menu.Items.Add(new ToolStripMenuItem("Actualiser", null, (s, e) => dgv.Refresh()) { Name = "cmsActualiser" });
        }

        private static void CopierCellule(DataGridView dgv)
        {
            if (dgv.CurrentCell?.Value == null) return;
            Clipboard.SetText(dgv.CurrentCell.Value.ToString());
        }

        private static void CopierLignes(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;

            var sb = new StringBuilder();
            foreach (DataGridViewRow row in dgv.SelectedRows)
            {
                if (row.IsNewRow) continue;
                var cells = row.Cells.Cast<DataGridViewCell>()
                    .Select(c => c.Value?.ToString()?.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ") ?? "");
                sb.AppendLine(string.Join("\t", cells));
            }
            Clipboard.SetText(sb.ToString());
        }

        private static void SupprimerLignes(DataGridView dgv)
        {
            if (dgv.ReadOnly) return;
            if (dgv.SelectedRows.Count == 0) return;

            if (dgv.DataSource is BindingSource bs)
            {
                // ✅ supprimer par DataBoundItem, pas par Index (tri/filtre safe)
                var items = dgv.SelectedRows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Select(r => r.DataBoundItem)
                    .Where(item => item != null)
                    .Distinct()
                    .ToList();

                foreach (var item in items)
                    bs.Remove(item);
            }
            else
            {
                // ✅ supprimer en ordre décroissant d’index
                var rows = dgv.SelectedRows.Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .OrderByDescending(r => r.Index)
                    .ToList();

                foreach (var r in rows)
                    dgv.Rows.Remove(r);
            }

            var frm = dgv.FindForm();
            if (frm is IContextMenuHandler handler)
                handler.MettreAJourTotaux();
        }

        private static void ExporterCsv(DataGridView dgv)
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = "export.csv"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                var headers = dgv.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText);
                sb.AppendLine(string.Join(";", headers));

                foreach (DataGridViewRow r in dgv.Rows)
                {
                    if (r.IsNewRow) continue;
                    var vals = r.Cells.Cast<DataGridViewCell>()
                        .Select(c => (c.Value?.ToString() ?? "").Replace(";", ","));
                    sb.AppendLine(string.Join(";", vals));
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Export terminé ✔");
            }
        }

        public static void AppliquerMenuContextuel(Control control)
        {
            if (control == null) return;

            if (control is DataGridView || control is TextBoxBase || control is ComboBox)
                control.ContextMenuStrip = MenuContextuel;

            if (control.ContextMenuStrip != null)
                AppliquerTraductions(control.ContextMenuStrip);

            foreach (Control child in control.Controls)
                AppliquerMenuContextuel(child);
        }

        #endregion

        // ==========================================================
        // 8) AUDIT LOG (PRO)
        // ==========================================================
        #region AuditLog

        public static void AjouterAuditLog(string typeAction, string description, string resultat, string utilisateur = null)
        {
            string userApp = utilisateur;
            if (string.IsNullOrWhiteSpace(userApp))
            {
                try
                {
                    userApp = $"{SessionEmploye.Nom} {SessionEmploye.Prenom}".Trim();
                }
                catch
                {
                    userApp = Environment.UserName;
                }
            }

            string machine = Environment.MachineName;
            string ips = ObtenirToutesIPs();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO AuditLog (Utilisateur, AdresseIP, TypeAction, Description, Resultat)
VALUES (@Utilisateur, @AdresseIP, @TypeAction, @Description, @Resultat)", conn))
                {
                    cmd.Parameters.Add("@Utilisateur", SqlDbType.NVarChar, 80).Value = userApp ?? "";
                    cmd.Parameters.Add("@AdresseIP", SqlDbType.NVarChar, 250).Value = (machine ?? "") + " | " + (ips ?? "");
                    cmd.Parameters.Add("@TypeAction", SqlDbType.NVarChar, 80).Value = typeAction ?? "";
                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = description ?? "";
                    cmd.Parameters.Add("@Resultat", SqlDbType.NVarChar, 80).Value = resultat ?? "";

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Erreur AuditLog : " + ex.Message);
#endif
            }
        }

        private static string ObtenirToutesIPs()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var ips = host.AddressList
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.ToString())
                    .Distinct()
                    .ToList();

                return ips.Count == 0 ? "IP inconnue" : string.Join(";", ips);
            }
            catch
            {
                return "IP inconnue";
            }
        }

        #endregion

        // ==========================================================
        // 9) SECURITY: UNLOCK TEMP + VALIDATION MANAGER (UNIQUE)
        // ==========================================================
        #region Security / Validation

        private static readonly object _unlockLock = new object();
        private static readonly Dictionary<string, DateTime> _unlockUntil =
            new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public static bool EstDebloque(string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode)) return false;
            permissionCode = permissionCode.Trim();

            lock (_unlockLock)
            {
                if (_unlockUntil.TryGetValue(permissionCode, out var until))
                {
                    if (DateTime.Now <= until) return true;
                    _unlockUntil.Remove(permissionCode);
                }
                return false;
            }
        }

        public static void Debloquer(string permissionCode, int minutes = 10)
        {
            if (string.IsNullOrWhiteSpace(permissionCode)) return;
            permissionCode = permissionCode.Trim();

            lock (_unlockLock)
                _unlockUntil[permissionCode] = DateTime.Now.AddMinutes(minutes);
        }

        public static void ResetDeblocage(string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode)) return;
            permissionCode = permissionCode.Trim();

            lock (_unlockLock)
                _unlockUntil.Remove(permissionCode);
        }

        // ✅ Compat : empreinte (ne bloque jamais si pas de lecteur)
        public static Task<bool> DemanderEmpreinteSiBesoinAsync(string message = "Accès sécurisé : empreinte requise")
            => VerifierEmpreinteSiDisponibleAsync(message);

        // ✅ Biométrie placeholder
        public static Task<bool> VerifierEmpreinteSiDisponibleAsync(string message = "Confirmez par empreinte digitale")
        {
            try
            {
                bool biometrieDisponible = false;
                if (!biometrieDisponible)
                    return Task.FromResult(true);

                bool ok = false; // placeholder
                return Task.FromResult(ok);
            }
            catch
            {
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// ✅ Méthode UNIQUE de validation manager via FrmSignatureManager.
        /// Débloque la permission X minutes si validé.
        /// </summary>
        public static bool DemanderValidationManager(
            IWin32Window owner,
            string permissionCode,
            string typeAction,
            string reference,
            string details,
            int? idEmployeDemandeur,
            string roleDemandeur,
            bool startOnEmpreinte = false,
            int minutesDeblocage = 10)
        {
            if (EstDebloque(permissionCode)) return true;

            using (var f = new FrmSignatureManager(
                connectionString: ConnectionString,
                typeAction: typeAction,
                permissionCode: permissionCode,
                reference: reference,
                details: details,
                idEmployeDemandeur: idEmployeDemandeur,
                roleDemandeur: roleDemandeur))
            {
                f.StartOnEmpreinteTab = startOnEmpreinte;

                var dr = f.ShowDialog(owner);

                if (dr == DialogResult.OK && f.Approved)
                {
                    Debloquer(permissionCode, minutesDeblocage);

                    AjouterAuditLog(
                        "VALIDATION_MANAGER",
                        $"{typeAction} | Permission={permissionCode} | ValidéPar={f.ManagerNom} ({f.ManagerPoste}) | Ref={reference}",
                        "Succès"
                    );
                    return true;
                }

                AjouterAuditLog(
                    "VALIDATION_MANAGER",
                    $"{typeAction} | Permission={permissionCode} | REFUS | Ref={reference} | {details}",
                    "Refus"
                );
                return false;
            }
        }

        public static bool AutoriserOuBloquerModule(
    IWin32Window owner,
    string permissionCode,
    string titreAction,
    bool autoriseDb,
    bool accesControleOn,
    bool toujoursSignature,
    int? idEmployeDemandeur,
    string roleDemandeur,
    string detailsExtra = null,
    bool startOnEmpreinteTab = false)
        {
            // 1) Admin => direct
            if (RolesSecurite.EstAdmin(SessionEmploye.Poste))
                return true;

            // 2) Si autorisé DB
            if (autoriseDb)
            {
                // si module "sensibles" => tu peux demander signature si tu veux
                if (toujoursSignature)
                {
                    return DemanderValidationManager(
                        owner: owner,
                        permissionCode: permissionCode,
                        typeAction: titreAction,
                        reference: permissionCode,
                        details: detailsExtra ?? "",
                        idEmployeDemandeur: idEmployeDemandeur,
                        roleDemandeur: roleDemandeur,
                        startOnEmpreinte: startOnEmpreinteTab,
                        minutesDeblocage: 10
                    );
                }

                return true;
            }

            // 3) NON autorisé DB
            // OFF => blocage strict (aucune signature ne doit permettre l'ouverture)
            if (!accesControleOn)
                return false;

            // ON => signature manager possible
            return DemanderValidationManager(
                owner: owner,
                permissionCode: permissionCode,
                typeAction: titreAction,
                reference: permissionCode,
                details: detailsExtra ?? "",
                idEmployeDemandeur: idEmployeDemandeur,
                roleDemandeur: roleDemandeur,
                startOnEmpreinte: startOnEmpreinteTab,
                minutesDeblocage: 10
            );
        }

        public static bool EstModuleAutoriseDb(string roleDemandeur, string codeModule)
        {
            if (string.IsNullOrWhiteSpace(roleDemandeur) || string.IsNullOrWhiteSpace(codeModule))
                return false;

            try
            {
                using (var con = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 rm.Autorise
FROM Roles r
JOIN RoleModules rm ON rm.IdRole = r.IdRole
JOIN Modules m ON m.IdModule = rm.IdModule
WHERE r.NomRole = @role
  AND m.CodeModule = @code;", con))
                {
                    cmd.Parameters.Add("@role", SqlDbType.NVarChar, 100).Value = roleDemandeur.Trim();
                    cmd.Parameters.Add("@code", SqlDbType.NVarChar, 120).Value = codeModule.Trim();

                    con.Open();
                    object res = cmd.ExecuteScalar();
                    if (res == null || res == DBNull.Value) return false;

                    // Autorise peut être bit/int
                    return Convert.ToInt32(res) == 1;
                }
            }
            catch
            {
                return false;
            }
        }


        #endregion

        // ==========================================================
        // 10) ACCES CONTROLE (SystemSettings)
        // ==========================================================
        #region SystemSettings

        private const string SETTING_ACCES_CONTROLE = "ACCES_CONTROLE_ON";
        private static bool? _accesControleOnCache = null;

        public static bool AccesControleOn
        {
            get
            {
                if (_accesControleOnCache.HasValue) return _accesControleOnCache.Value;

                bool val = false;
                try
                {
                    using (var con = new SqlConnection(ConnectionString))
                    using (var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key]=@k)
BEGIN
    INSERT INTO SystemSettings([Key],[Value]) VALUES (@k, '0');
END
SELECT [Value] FROM SystemSettings WHERE [Key]=@k;", con))
                    {
                        cmd.Parameters.Add("@k", SqlDbType.NVarChar, 100).Value = SETTING_ACCES_CONTROLE;
                        con.Open();
                        var s = Convert.ToString(cmd.ExecuteScalar() ?? "0");
                        val = s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch { val = false; }

                _accesControleOnCache = val;
                return val;
            }
            set
            {
                _accesControleOnCache = value;

                try
                {
                    using (var con = new SqlConnection(ConnectionString))
                    using (var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE [Key]=@k)
    INSERT INTO SystemSettings([Key],[Value]) VALUES (@k, @v);
ELSE
    UPDATE SystemSettings SET [Value]=@v WHERE [Key]=@k;", con))
                    {
                        cmd.Parameters.Add("@k", SqlDbType.NVarChar, 100).Value = SETTING_ACCES_CONTROLE;
                        cmd.Parameters.Add("@v", SqlDbType.NVarChar, 10).Value = value ? "1" : "0";
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { /* ignore */ }
            }
        }

        #endregion

        // ==========================================================
        // 11) MODULES + PERMISSIONS (COMPAT PROJET)
        // ==========================================================
        #region Modules & Permissions

        public class ModuleDef
        {
            public string Code { get; set; }
            public string Nom { get; set; }
        }

        public static List<ModuleDef> GetModulesFormMain()
        {
            return new List<ModuleDef>
            {
                new ModuleDef{ Code="btnVentes", Nom="Ventes"},
                new ModuleDef{ Code="btnProduits", Nom="Produits"},
                new ModuleDef{ Code="btnClients", Nom="Clients"},
                new ModuleDef{ Code="btnInventaire", Nom="Inventaire"},

                new ModuleDef{ Code="btnInventaireScanner", Nom="Inventaire scanner (PDA)"},
                new ModuleDef{ Code="btnAlertesStockExp", Nom="Alertes stock & expiration"},

                new ModuleDef{ Code="btnCaisse", Nom="Caisse"},
                new ModuleDef{ Code="btnSessionsCaisse", Nom="Sessions caisse"},
                new ModuleDef{ Code="btnEntreesSortiesStock", Nom="Entrées / Sorties stock"},
                new ModuleDef{ Code="BtnClotureJournaliere", Nom="Clôture journalière"},

                new ModuleDef{ Code="btnPresenceAbsence", Nom="Présence / Absence"},
                new ModuleDef{ Code="btnDepenses", Nom="Dépenses"},
                new ModuleDef{ Code="btnOuvrirStock", Nom="Ouvrir stock"},

                new ModuleDef{ Code="btnEmployes", Nom="Employés"},
                new ModuleDef{ Code="btnUtilisateurs", Nom="Utilisateurs"},
                new ModuleDef{ Code="btnDetails", Nom="Détails"},

                new ModuleDef{ Code="btnStockAvance", Nom="Stock avancé"},
                new ModuleDef{ Code="btnStatistiquesAvancees", Nom="Statistiques avancées"},
                new ModuleDef{ Code="btnConfigSysteme", Nom="Configuration système"},
                new ModuleDef{ Code="btnPermissions", Nom="Permissions"},
                new ModuleDef{ Code="btnAuditLog", Nom="Audit log"},
                new ModuleDef{ Code="btnDeconnexion", Nom="Déconnexion"},

                new ModuleDef{ Code="btnRemisesPromotions", Nom="Remises & Promotions"},
                new ModuleDef{ Code="btnAnnulations", Nom="Annulations / Retours"},
                new ModuleDef{ Code="btnMarketing", Nom="Marketing"},
                new ModuleDef{ Code="btnComptables", Nom="Comptabilité"},
                new ModuleDef{ Code="btnSalairesAgents", Nom="Salaires des agents"},
                new ModuleDef{ Code="btnGestionFournisseursAchats", Nom="Gestion Fournisseurs & Achats"},

                new ModuleDef{ Code="btnBonCommande", Nom="Bon de commande"},
                new ModuleDef{ Code="btnReceptionFournisseur", Nom="Réception fournisseur"},
                new ModuleDef{ Code="btnFactureFournisseur", Nom="Facture fournisseur"},
                new ModuleDef{ Code="btnPaiementsFournisseur", Nom="Paiements fournisseur"},

                new ModuleDef{ Code="btnGestionImprimantes", Nom="Gestion des Imprimantes"},
                new ModuleDef{ Code="btnRetraitFidelite", Nom="Retrait Fidélité"},
                new ModuleDef{ Code="btnFournisseurs", Nom="Fournisseurs"},
                new ModuleDef{ Code="btnCatalogueFournisseurs", Nom="Catalogue Fournisseurs"},
                new ModuleDef{ Code="btnPartenaires", Nom="Partenaires"},
                new ModuleDef{ Code="btnPromoPartenaires", Nom="PromoPartenaires"},
                new ModuleDef{ Code="btnEntreesSortiesCaisse", Nom="Entrées / Sorties caisse"},
                new ModuleDef{ Code="btnOperationsStock", Nom="Operations Stock"},
            };
        }

        public static bool InitialiserModulesSiNecessaire(IEnumerable<ModuleDef> modules, out string err)
        {
            err = null;

            try
            {
                InitialiserModulesEtPermissions(modules);
                return true;
            }
            catch (SqlException ex)
            {
                // 18456 = Login failed
                err = "Connexion SQL impossible.\n" + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }



        public static void InitialiserModulesSiNecessaire(IEnumerable<ModuleDef> modules)
        {
            string err;
            InitialiserModulesSiNecessaire(modules, out err); // ignore err (ou log)
        }


        public static void InitialiserModulesEtPermissions(IEnumerable<ModuleDef> modules)
        {
            if (modules == null) return;

            var adminRoles = RolesSecurite.GetAdminRolesCopy();

            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    try
                    {
                        foreach (var m in modules)
                        {
                            if (m == null || string.IsNullOrWhiteSpace(m.Code)) continue;

                            using (SqlCommand cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM Modules WHERE CodeModule = @Code)
    INSERT INTO Modules(CodeModule, NomModule) VALUES(@Code, @Nom)
ELSE
    UPDATE Modules
    SET NomModule = CASE 
        WHEN (NomModule IS NULL OR LTRIM(RTRIM(NomModule)) = '') 
        THEN @Nom 
        ELSE NomModule 
    END
    WHERE CodeModule = @Code;", con, tx))
                            {
                                cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 120).Value = m.Code.Trim();
                                cmd.Parameters.Add("@Nom", SqlDbType.NVarChar, 250).Value = (m.Nom ?? m.Code).Trim();
                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var roleName in adminRoles)
                        {
                            int? idRole = null;

                            using (SqlCommand cmdRole = new SqlCommand(
                                "SELECT IdRole FROM Roles WHERE NomRole = @r", con, tx))
                            {
                                cmdRole.Parameters.Add("@r", SqlDbType.NVarChar, 100).Value = roleName;
                                object res = cmdRole.ExecuteScalar();
                                if (res != null && res != DBNull.Value)
                                    idRole = Convert.ToInt32(res);
                            }

                            if (!idRole.HasValue) continue;

                            using (SqlCommand cmdInsert = new SqlCommand(@"
INSERT INTO RoleModules(IdRole, IdModule, Autorise)
SELECT @IdRole, m.IdModule, 1
FROM Modules m
WHERE NOT EXISTS (
    SELECT 1 FROM RoleModules rm
    WHERE rm.IdRole = @IdRole AND rm.IdModule = m.IdModule
);", con, tx))
                            {
                                cmdInsert.Parameters.Add("@IdRole", SqlDbType.Int).Value = idRole.Value;
                                cmdInsert.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        #endregion

        // ==========================================================
        // 12) OUTILS DIVERS
        // ==========================================================
        #region Utils

        public static Control FindControlRecursive(Control parent, string name)
        {
            if (parent == null) return null;

            foreach (Control c in parent.Controls)
            {
                if (c.Name == name) return c;

                var found = FindControlRecursive(c, name);
                if (found != null) return found;
            }
            return null;
        }

        #endregion
    }
}