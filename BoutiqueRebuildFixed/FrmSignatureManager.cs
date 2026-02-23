using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmSignatureManager : Form
    {
        private readonly string _cs;
        public bool StartOnForgotTab { get; set; } = false;
        public bool StartForgotEmployeeFlow { get; set; } = false; // ✅ flow "Mot de passe oublié" depuis FormLogin

        // Contexte demandé par la vente
        private readonly string _permissionCode;
        private readonly string _typeAction;
        private readonly string _reference;
        private readonly string _details;
        private readonly int? _idEmployeDemandeur;

        private readonly string _roleDemandeur;

        public bool StartOnEmpreinteTab { get; set; } = false;

        // Résultat
        public bool Approved { get; private set; } = false;
        public int ManagerId { get; private set; } = 0;
        public string ManagerNom { get; private set; } = "";
        public string ManagerPoste { get; private set; } = "";

        // ===================== UI (Tabs) =====================
        private TabControl tabMain;
        private TabPage tabSignature;
        private TabPage tabConfigPin;
        private TabPage tabForgot;         // ✅ NOUVEL ONGLET
        private TabPage tabEmpreinte;

        // ---- Signature tab UI
        private Label lblTitre;
        private Label lblContexte;
        private Label lblLogin;
        private Label lblPin;
        private TextBox txtLogin;
        private TextBox txtPin;
        private Button btnValider;
        private Button btnAnnuler;
        private Panel panelTop;
        private Button btnCfgForgot;

        // ---- Config PIN tab UI (Changer mot de passe/PIN manager)
        private Label lblCfgTitle;
        private Label lblCfgLogin;
        private TextBox txtCfgLogin;
        private Label lblCfgOld;
        private TextBox txtCfgOld;
        private Label lblCfgNewPin;
        private TextBox txtCfgNewPin;
        private Label lblCfgConfirmPin;
        private TextBox txtCfgConfirmPin;
        private Button btnCfgSave;
        private Label lblCfgHint;

        private Button btnEyeOld;
        private Button btnEyeNew;
        private Button btnEyeConfirm;

        private bool _showOld = false;
        private bool _showNew = false;
        private bool _showConfirm = false;

        // ---- Forgot tab UI (Mot de passe oublié)
        private Label lblFgTitle;
        private Label lblFgHint;
        private Label lblFgEmployeeLogin;
        private TextBox txtFgEmployeeLogin;
        private Label lblFgNewPwd;
        private TextBox txtFgNewPwd;
        private Label lblFgConfirmPwd;
        private TextBox txtFgConfirmPwd;
        private Button btnFgReset;
        private Button btnFgReset1234;
        // Flow reset employé depuis FormLogin
        private bool _forgotEmployeeFlow = false;   // vient de FormLogin
        private bool _fgTempApplied = false;        // 1234 appliqué -> étape suivante

        private Button btnEyeFgNew;
        private Button btnEyeFgConfirm;
        private bool _showFgNew = false;
        private bool _showFgConfirm = false;

        // ---- Empreinte tab UI
        private Label lblHelloInfo;
        private Label lblHelloLogin;
        private TextBox txtHelloLogin;
        private Label lblHelloHint;
        private Button btnOpenWindowsHello;
        private Button btnVerifyHello;
        private TabPage tabSetupPin;

        private Label lblSetupTitle;
        private Label lblSetupHint;
        private Label lblSetupLogin;
        private TextBox txtSetupLogin;
        private Label lblSetupNewPin;
        private TextBox txtSetupNewPin;
        private Label lblSetupConfirmPin;
        private TextBox txtSetupConfirmPin;
        private Button btnSetupSave;

        private Button btnEyeSetupNew;
        private Button btnEyeSetupConfirm;
        private bool _showSetupNew = false;
        private bool _showSetupConfirm = false;
        private string _pendingTargetLogin = null;   // login/matricule du manager à reset
        private bool _pendingForgotFlow = false;     // on est dans le flow oublié
        public bool CloseOnApproved { get; set; } = true; // ✅ fermeture auto si validation OK (pour ouvrir module)

        // ===================== ROLES =====================
        private static readonly string[] RolesPeuventConfigurerPin =
        {
            "Programmeur",
            "Directeur Général",
            "Gérant",
            "Superviseur"
        };

        private static readonly string[] RolesPeuventUtiliserEmpreinte =
        {
            "Programmeur",
            "Directeur Général",
            "Gérant",
            "Superviseur"
        };

        private static readonly string[] RolesPeuventResetPassword =
        {
            "Programmeur",
            "Directeur Général",
            "Gérant",
            "Superviseur"
        };

        private static readonly string[] PostesAdmin =
{
    "Superviseur",
    "Gérant",
    "Directeur Général",
    "Programmeur"
};

        private bool IsAdminPoste(string poste)
        {
            if (string.IsNullOrWhiteSpace(poste)) return false;

            string rr = NormalizeRoleKey(poste);
            return PostesAdmin.Any(p => NormalizeRoleKey(p) == rr);
        }
        private bool IsPosteManager(string poste) => IsAdminPoste(poste);

        public FrmSignatureManager(
    string connectionString,
    string typeAction,
    string permissionCode,
    string reference,
    string details,
    int? idEmployeDemandeur = null,
    string roleDemandeur = null)
        {
            _cs = connectionString;

            _typeAction = (typeAction ?? "").Trim();
            _permissionCode = (permissionCode ?? "").Trim();
            _reference = (reference ?? "").Trim();
            _details = (details ?? "").Trim();
            _idEmployeDemandeur = idEmployeDemandeur;

            _roleDemandeur = (roleDemandeur ?? "").Trim();

            // ✅ NE PLUS décider ici sur base de StartOnForgotTab
            // (car la propriété est définie APRES new FrmSignatureManager(...))

            InitializeComponent();
            AppliquerDroitsTabs();
        }

        // ===================== AUTH / ROLES =====================

        private bool IsAdminRole(string role)
        {
            var rr = NormalizeRoleKey(role);

            // si tu as RolesSecurite.EstAdmin, garde
            try
            {
                if (ConfigSysteme.RolesSecurite.EstAdmin(rr)) return true;
            }
            catch { }

            // fallback
            return rr.Contains("admin") || rr.Contains("programmeur") || rr.Contains("super admin");
        }

        private bool RoleAutorise(string role, string[] allowed)
        {
            string rr = NormalizeRoleKey(role);
            return allowed.Any(r => NormalizeRoleKey(r) == rr);
        }

        private string NormalizeRoleKey(string role)
        {
            try
            {
                return ConfigSysteme.NormalizeRoleKey(role);
            }
            catch
            {
                return (role ?? "").Trim().ToLowerInvariant();
            }
        }

        private void AppliquerDroitsTabs()
        {
            if (tabMain == null) return;

            bool isAdmin = IsAdminRole(_roleDemandeur);

            bool canPin = isAdmin || RoleAutorise(_roleDemandeur, RolesPeuventConfigurerPin);
            bool canHello = isAdmin || RoleAutorise(_roleDemandeur, RolesPeuventUtiliserEmpreinte);
            bool canSetup = canPin;
            // ✅ Tab Setup PIN (Configurer) - doit être invisible pour non-manager/non-admin
            if (tabSetupPin != null)
            {
                if (canSetup)
                {
                    if (!tabMain.TabPages.Contains(tabSetupPin))
                        tabMain.TabPages.Add(tabSetupPin);
                }
                else
                {
                    if (tabMain.TabPages.Contains(tabSetupPin))
                        tabMain.TabPages.Remove(tabSetupPin);
                }
            }

            // ✅ SAFE RESET :
            // si roleDemandeur est vide (cas FormLogin), on laisse VOIR l’onglet,
            // mais le bouton reste bloqué tant que Approved=false (via UpdateForgotUiState).
            bool hasRoleContext = !string.IsNullOrWhiteSpace(_roleDemandeur);
            bool canReset = !hasRoleContext || isAdmin || RoleAutorise(_roleDemandeur, RolesPeuventResetPassword);

            // ✅ Tab Config PIN
            if (tabConfigPin != null)
            {
                if (canPin)
                {
                    if (!tabMain.TabPages.Contains(tabConfigPin))
                        tabMain.TabPages.Add(tabConfigPin);
                }
                else
                {
                    if (tabMain.TabPages.Contains(tabConfigPin))
                        tabMain.TabPages.Remove(tabConfigPin);
                }
            }

            // ✅ Tab Forgot Password (NE DISPARAÎT PLUS au login)
            if (tabForgot != null && !tabMain.TabPages.Contains(tabForgot))
                tabMain.TabPages.Add(tabForgot);

            // ✅ Tab Empreinte
            if (tabEmpreinte != null)
            {
                if (canHello)
                {
                    if (!tabMain.TabPages.Contains(tabEmpreinte))
                        tabMain.TabPages.Add(tabEmpreinte);
                }
                else
                {
                    if (tabMain.TabPages.Contains(tabEmpreinte))
                        tabMain.TabPages.Remove(tabEmpreinte);
                }
            }

            if (StartOnEmpreinteTab && !canHello)
                StartOnEmpreinteTab = false;

            // ✅ UX : après gestion des tabs, on applique l’état du reset
            UpdateForgotUiState();
        }

        private void UpdateForgotUiState()
        {
            bool ok = Approved && ManagerId > 0;

            // ✅ l'employé reste toujours saisissable (comme tu l'as demandé)
            if (txtFgEmployeeLogin != null)
                txtFgEmployeeLogin.Enabled = true;

            // ✅ bouton 1234 : actif seulement si signature OK + employé renseigné
            bool hasEmp = !string.IsNullOrWhiteSpace(txtFgEmployeeLogin?.Text);
            if (btnFgReset1234 != null)
                btnFgReset1234.Enabled = ok && hasEmp;

            // ✅ champs nouveau mdp : actifs seulement après 1234
            if (txtFgNewPwd != null)
                txtFgNewPwd.Enabled = ok && _fgTempApplied;

            if (txtFgConfirmPwd != null)
                txtFgConfirmPwd.Enabled = ok && _fgTempApplied;

            // ✅ bouton valider : visible seulement après 1234 + activé seulement si mot de passe OK
            if (btnFgReset != null)
            {
                btnFgReset.Visible = ok && _fgTempApplied;

                bool pwdOk =
                    !string.IsNullOrWhiteSpace(txtFgNewPwd?.Text) &&
                    !string.IsNullOrWhiteSpace(txtFgConfirmPwd?.Text) &&
                    (txtFgNewPwd.Text.Trim() == txtFgConfirmPwd.Text.Trim()) &&
                    txtFgNewPwd.Text.Trim().Length >= 4;

                btnFgReset.Enabled = ok && _fgTempApplied && pwdOk;
            }

            // ✅ Texte d'aide
            if (lblFgHint != null)
            {
                if (!ok)
                {
                    lblFgHint.Text =
                        "⚠️ Reset contrôlé.\n" +
                        "1) Valide la signature (onglet Signature).\n" +
                        "2) Ensuite tu peux réinitialiser en 1234 puis valider le nouveau mot de passe.";
                }
                else if (!_fgTempApplied)
                {
                    lblFgHint.Text =
                        "✅ Signature validée.\n" +
                        "➡ Saisis l’employé puis clique 'Réinitialiser en 1234'.";
                }
                else
                {
                    lblFgHint.Text =
                        "✅ 1234 appliqué.\n" +
                        "➡ Saisis le nouveau mot de passe + confirmation, puis valide.";
                }
            }
        }

        private void btnFgReset1234_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ Exiger signature manager validée avant reset
                if (!Approved || ManagerId <= 0)
                {
                    MessageBox.Show("⚠️ D’abord valider la signature manager (onglet Signature).", "Sécurité",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tabMain.SelectedTab = tabSignature;
                    txtLogin?.Focus();
                    return;
                }

                string empLogin = (txtFgEmployeeLogin.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(empLogin))
                {
                    MessageBox.Show("Saisis le NomUtilisateur / Matricule de l’employé.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFgEmployeeLogin.Focus();
                    return;
                }

                int targetId = GetEmployeIdByLogin(empLogin);
                if (targetId <= 0)
                {
                    MessageBox.Show("Employé introuvable (NomUtilisateur / Matricule).", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFgEmployeeLogin.Focus();
                    return;
                }

                // ⛔ Interdire auto-reset (sécurité)
                if (targetId == ManagerId)
                {
                    MessageBox.Show(
                        "⛔ Sécurité : tu ne peux pas réinitialiser ton propre code.\n" +
                        "Demande à un autre Admin de valider la signature.",
                        "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ✅ Reset en 1234
                bool done = ResetEmployeToTemporary1234(empLogin);
                if (!done)
                {
                    MessageBox.Show("Reset impossible : employé introuvable.", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ Passe à l’étape 2 : autoriser la saisie du nouveau MDP
                _fgTempApplied = true;

                txtFgNewPwd.Clear();
                txtFgConfirmPwd.Clear();
                UpdateForgotUiState();

                txtFgNewPwd.Focus();

                LogResetPassword(empLogin);

                MessageBox.Show(
                    "✅ Mot de passe réinitialisé en 1234.\n\n" +
                    "➡ Si tu veux changer, saisis Nouveau + Confirmation puis clique 'Valider le nouveau mot de passe'.",
                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // UX : focus direct sur nouveau
                txtFgNewPwd.Clear();
                txtFgConfirmPwd.Clear();
                txtFgNewPwd.Focus();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Erreur SQL : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ResetEmployeToTemporary1234(string loginOrMatricule)
        {
            const string tempPin = "1234";

            int idEmp = GetEmployeIdByLogin(loginOrMatricule);
            if (idEmp <= 0) return false;

            byte[] saltBin = CreateSalt(16);
            byte[] hashBin = HashPBKDF2_64(tempPin, saltBin, iterations: 100000, bytes: 64);

            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
BEGIN TRY
    BEGIN TRAN;

    UPDATE dbo.Employes
    SET PinSaltBin = @salt,
        PinHashBin = @hash
    WHERE ID_Employe = @id;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRAN;
        SELECT 0;
        RETURN;
    END

    COMMIT TRAN;
    SELECT 1;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH
", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmp;
                cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = saltBin;
                cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 64).Value = hashBin;

                con.Open();
                int ok = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                return ok == 1;
            }
        }


        // ===================== UI HELPERS (EYE BUTTON) =====================

        private Button CreateEyeButton(TextBox tb, Func<bool> getState, Action<bool> setState)
        {
            var eye = new Button
            {
                Width = 32,
                Height = tb.Height,
                FlatStyle = FlatStyle.Flat,
                TabStop = false,
                Text = "👁",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                BackColor = SystemColors.Control,
                Cursor = Cursors.Hand
            };
            eye.FlatAppearance.BorderSize = 1;

            void PlaceEye()
            {
                if (tb.Parent == null) return;

                eye.Top = tb.Top;
                eye.Height = tb.Height;
                eye.Left = tb.Left + tb.Width - eye.Width; // dans le textbox

                // laisse de la place dans le textbox
                tb.Padding = new Padding(tb.Padding.Left, tb.Padding.Top, eye.Width + 6, tb.Padding.Bottom);
            }

            PlaceEye();
            tb.SizeChanged += (s, e) => PlaceEye();
            tb.LocationChanged += (s, e) => PlaceEye();

            eye.Click += (s, e) =>
            {
                bool show = !getState();
                setState(show);

                tb.PasswordChar = show ? '\0' : '●';
                eye.Text = show ? "🙈" : "👁";
                tb.Focus();
                tb.SelectionStart = tb.TextLength;
            };

            return eye;
        }



        // ===================== INITIALIZE COMPONENT =====================

        private void LogResetPassword(string cibleNomUtilisateur)
        {
            // ✅ Ne pas enregistrer le nouveau mot de passe en clair.
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.ManagerSignatureLog
(DateSignature, TypeAction, Reference, Details, ID_EmployeDemandeur, ID_Manager, ManagerNom, Machine)
VALUES
(GETDATE(), @t, @r, @d, @dem, @mid, @mnom, @mach);", con))
            {
                cmd.Parameters.Add("@t", SqlDbType.NVarChar, 60).Value = "RESET_PASSWORD";
                cmd.Parameters.Add("@r", SqlDbType.NVarChar, 120).Value = DBNull.Value;

                string d = $"[OK=1][RESET={cibleNomUtilisateur}]";
                cmd.Parameters.Add("@d", SqlDbType.NVarChar, 500).Value = Truncate(d, 500);

                cmd.Parameters.Add("@dem", SqlDbType.Int).Value =
                    _idEmployeDemandeur.HasValue ? (object)_idEmployeDemandeur.Value : DBNull.Value;

                cmd.Parameters.Add("@mid", SqlDbType.Int).Value =
                    (Approved && ManagerId > 0) ? (object)ManagerId : DBNull.Value;

                cmd.Parameters.Add("@mnom", SqlDbType.NVarChar, 120).Value =
                    string.IsNullOrWhiteSpace(ManagerNom) ? (object)DBNull.Value : Truncate(ManagerNom, 120);

                cmd.Parameters.Add("@mach", SqlDbType.NVarChar, 120).Value = Truncate(BuildMachineTag(), 120);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }



        private void InitializeComponent()
        {
            SuspendLayout();

            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(580, 460);
            KeyPreview = true;
            Text = "Signature Manager";

            // ================= TAB CONTROL =================
            tabMain = new TabControl { Dock = DockStyle.Fill };

            tabSignature = new TabPage("Signature");

            // ✅ Nouveau : config initiale (sans ancien code)
            tabSetupPin = new TabPage("Configurer mot de passe");

            tabConfigPin = new TabPage("Changer mot de passe");
            tabForgot = new TabPage("Mot de passe oublié");
            // tabEmpreinte sera créé dans BuildTabEmpreinte()

            tabMain.TabPages.Add(tabSignature);
            tabMain.TabPages.Add(tabSetupPin);    // ✅
            tabMain.TabPages.Add(tabConfigPin);
            tabMain.TabPages.Add(tabForgot);

            tabMain.SelectedIndexChanged += (s, e) =>
            {
                if (tabMain.SelectedTab == tabSignature) txtLogin?.Focus();
                else if (tabMain.SelectedTab == tabSetupPin) txtSetupLogin?.Focus();     // ✅
                else if (tabMain.SelectedTab == tabConfigPin) txtCfgLogin?.Focus();
                else if (tabMain.SelectedTab == tabForgot) txtFgEmployeeLogin?.Focus();
                else if (tabMain.SelectedTab == tabEmpreinte) txtHelloLogin?.Focus();
            };

            Controls.Add(tabMain);

            BuildTabSignature();

            BuildTabSetupPin();          // ✅ nouveau tab "Configurer"
            BuildTabConfigPin();         // "Changer"
            BuildTabForgotPassword();    // Reset contrôlé
            BuildTabEmpreinte();

            Load += FrmSignatureManager_Load;
            KeyDown += FrmSignatureManager_KeyDown;

            ResumeLayout(false);
        }

        // ===================== PBKDF2 (Employes.PinSaltBin / PinHashBin) =====================
        // Stockage recommandé : PinSaltBin VARBINARY(16), PinHashBin VARBINARY(64)
        private static byte[] CreateSalt(int size = 16)
        {
            byte[] salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);
            return salt;
        }

        private static byte[] HashPBKDF2_64(string secret, byte[] salt, int iterations = 100000, int bytes = 64)
        {
            // .NET Framework: ctor default = HMACSHA1 (ok). Si tu es en .NET >= 6, tu peux choisir SHA256.
            using (var pbkdf2 = new Rfc2898DeriveBytes(secret ?? "", salt, iterations))
                return pbkdf2.GetBytes(bytes);
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private static bool VerifyPBKDF2(string secret, byte[] salt, byte[] expectedHash, int iterations = 100000)
        {
            if (salt == null || expectedHash == null || expectedHash.Length == 0) return false;
            var test = HashPBKDF2_64(secret, salt, iterations, expectedHash.Length);
            return FixedTimeEquals(test, expectedHash);
        }


        private void BuildTabSetupPin()
        {
            lblSetupTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(18, 18),
                Text = "Configurer mot de passe / PIN (première fois)"
            };

            lblSetupHint = new Label
            {
                AutoSize = false,
                Location = new Point(20, 48),
                Size = new Size(535, 44),
                Text = "Pour un nouveau manager (ou si aucun code n’est configuré).\n" +
                       "Aucun ancien code requis."
            };

            lblSetupLogin = new Label
            {
                AutoSize = true,
                Location = new Point(22, 105),
                Text = "Nom utilisateur / Matricule"
            };

            txtSetupLogin = new TextBox
            {
                Location = new Point(25, 125),
                Width = 520
            };

            lblSetupNewPin = new Label
            {
                AutoSize = true,
                Location = new Point(22, 165),
                Text = "Nouveau PIN (min 4 chiffres)"
            };

            txtSetupNewPin = new TextBox
            {
                Location = new Point(25, 185),
                Width = 520,
                PasswordChar = '●'
            };

            lblSetupConfirmPin = new Label
            {
                AutoSize = true,
                Location = new Point(22, 225),
                Text = "Confirmer PIN"
            };

            txtSetupConfirmPin = new TextBox
            {
                Location = new Point(25, 245),
                Width = 520,
                PasswordChar = '●'
            };

            btnSetupSave = new Button
            {
                Text = "Configurer",
                Width = 160,
                Height = 36,
                Left = 385,
                Top = 305
            };
            btnSetupSave.Click += btnSetupSave_Click;

            tabSetupPin.Controls.Clear();
            tabSetupPin.Controls.Add(lblSetupTitle);
            tabSetupPin.Controls.Add(lblSetupHint);
            tabSetupPin.Controls.Add(lblSetupLogin);
            tabSetupPin.Controls.Add(txtSetupLogin);
            tabSetupPin.Controls.Add(lblSetupNewPin);
            tabSetupPin.Controls.Add(txtSetupNewPin);
            tabSetupPin.Controls.Add(lblSetupConfirmPin);
            tabSetupPin.Controls.Add(txtSetupConfirmPin);
            tabSetupPin.Controls.Add(btnSetupSave);

            // Yeux
            btnEyeSetupNew = CreateEyeButton(txtSetupNewPin, () => _showSetupNew, v => _showSetupNew = v);
            btnEyeSetupConfirm = CreateEyeButton(txtSetupConfirmPin, () => _showSetupConfirm, v => _showSetupConfirm = v);

            tabSetupPin.Controls.Add(btnEyeSetupNew);
            tabSetupPin.Controls.Add(btnEyeSetupConfirm);

            btnEyeSetupNew.BringToFront();
            btnEyeSetupConfirm.BringToFront();
        }


        private void BuildTabSignature()
        {
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 98,
                Padding = new Padding(12),
                BackColor = Color.WhiteSmoke
            };

            lblTitre = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(12, 10),
                Text = "Signature manager requise"
            };

            lblContexte = new Label
            {
                AutoSize = false,
                Location = new Point(12, 38),
                Size = new Size(535, 52),
                Text = "Action : ..."
            };

            panelTop.Controls.Add(lblTitre);
            panelTop.Controls.Add(lblContexte);

            lblLogin = new Label
            {
                AutoSize = true,
                Location = new Point(22, 118),
                Text = "Login manager (NomUtilisateur ou Matricule)"
            };

            txtLogin = new TextBox
            {
                Location = new Point(25, 138),
                Width = 520
            };

            lblPin = new Label
            {
                AutoSize = true,
                Location = new Point(22, 176),
                Text = "Code manager (PIN / Mot de passe)"
            };

            txtPin = new TextBox
            {
                Location = new Point(25, 196),
                Width = 520,
                PasswordChar = '●'
            };

            btnAnnuler = new Button
            {
                Text = "Annuler",
                Width = 120,
                Height = 36,
                Left = 295,
                Top = 260
            };
            btnAnnuler.Click += btnAnnuler_Click;

            btnValider = new Button
            {
                Text = "Valider",
                Width = 120,
                Height = 36,
                Left = 425,
                Top = 260
            };
            btnValider.Click += btnValider_Click;

            tabSignature.Controls.Clear();
            tabSignature.Controls.Add(panelTop);
            tabSignature.Controls.Add(lblLogin);
            tabSignature.Controls.Add(txtLogin);
            tabSignature.Controls.Add(lblPin);
            tabSignature.Controls.Add(txtPin);
            tabSignature.Controls.Add(btnAnnuler);
            tabSignature.Controls.Add(btnValider);
        }

        private void BuildTabConfigPin()
        {
            lblCfgTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(18, 18),
                Text = "Changer mot de passe / PIN manager"
            };

            lblCfgHint = new Label
            {
                AutoSize = false,
                Location = new Point(20, 48),
                Size = new Size(535, 40),
                Text = "Remplace l'ancien code. Ancien requis + Nouveau + Confirmation."
            };

            lblCfgLogin = new Label
            {
                AutoSize = true,
                Location = new Point(22, 98),
                Text = "Nom utilisateur / Matricule"
            };

            txtCfgLogin = new TextBox
            {
                Location = new Point(25, 118),
                Width = 520
            };

            lblCfgOld = new Label
            {
                AutoSize = true,
                Location = new Point(22, 158),
                Text = "Ancien mot de passe / ancien PIN"
            };

            txtCfgOld = new TextBox
            {
                Location = new Point(25, 178),
                Width = 520,
                PasswordChar = '●'
            };

            lblCfgNewPin = new Label
            {
                AutoSize = true,
                Location = new Point(22, 218),
                Text = "Nouveau mot de passe / nouveau PIN"
            };

            txtCfgNewPin = new TextBox
            {
                Location = new Point(25, 238),
                Width = 520,
                PasswordChar = '●'
            };

            lblCfgConfirmPin = new Label
            {
                AutoSize = true,
                Location = new Point(22, 278),
                Text = "Confirmer nouveau"
            };

            txtCfgConfirmPin = new TextBox
            {
                Location = new Point(25, 298),
                Width = 520,
                PasswordChar = '●'
            };

            btnCfgSave = new Button
            {
                Text = "Modifier",
                Width = 160,
                Height = 36,
                Left = 385,
                Top = 345
            };
            btnCfgSave.Click += btnCfgSave_Click;

            // ✅ Bouton forgot (doit être ajouté + pas superposé)
            btnCfgForgot = new Button
            {
                Text = "J'ai oublié mon ancien code",
                Width = 240,
                Height = 32,
                Left = 25,
                Top = 352 // ✅ différent de 345, et aligné bien
            };
            btnCfgForgot.Click += (s, e) =>
            {
                string target = (txtCfgLogin.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(target))
                {
                    MessageBox.Show("Saisis d’abord le Nom utilisateur / Matricule.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCfgLogin.Focus();
                    return;
                }

                MessageBox.Show(
                    "Mot de passe oublié (Manager) :\n" +
                    "➡ Un autre Admin doit valider la signature.\n" +
                    "➡ Après validation, le code sera remis à 1234 et tu reviendras sur 'Changer mot de passe'.",
                    "Mot de passe oublié", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ✅ prépare le flow
                _pendingTargetLogin = target;
                _pendingForgotFlow = true;

                // ✅ retourne vers Signature pour validation par un autre manager
                tabMain.SelectedTab = tabSignature;
                txtLogin?.Focus();
            };

            tabConfigPin.Controls.Clear();
            tabConfigPin.Controls.Add(lblCfgTitle);
            tabConfigPin.Controls.Add(lblCfgHint);
            tabConfigPin.Controls.Add(lblCfgLogin);
            tabConfigPin.Controls.Add(txtCfgLogin);
            tabConfigPin.Controls.Add(lblCfgOld);
            tabConfigPin.Controls.Add(txtCfgOld);
            tabConfigPin.Controls.Add(lblCfgNewPin);
            tabConfigPin.Controls.Add(txtCfgNewPin);
            tabConfigPin.Controls.Add(lblCfgConfirmPin);
            tabConfigPin.Controls.Add(txtCfgConfirmPin);

            tabConfigPin.Controls.Add(btnCfgSave);
            tabConfigPin.Controls.Add(btnCfgForgot);      // ✅ MANQUAIT
            btnCfgForgot.BringToFront();                 // ✅ sécurité

            // Yeux
            btnEyeOld = CreateEyeButton(txtCfgOld, () => _showOld, v => _showOld = v);
            btnEyeNew = CreateEyeButton(txtCfgNewPin, () => _showNew, v => _showNew = v);
            btnEyeConfirm = CreateEyeButton(txtCfgConfirmPin, () => _showConfirm, v => _showConfirm = v);

            tabConfigPin.Controls.Add(btnEyeOld);
            tabConfigPin.Controls.Add(btnEyeNew);
            tabConfigPin.Controls.Add(btnEyeConfirm);

            btnEyeOld.BringToFront();
            btnEyeNew.BringToFront();
            btnEyeConfirm.BringToFront();
        }

        private void BuildTabForgotPassword()
        {
            lblFgTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(18, 18),
                Text = "Mot de passe oublié (Employés)"
            };

            lblFgHint = new Label
            {
                AutoSize = false,
                Location = new Point(20, 48),
                Size = new Size(535, 60),
                Text =
                    "⚠️ Reset contrôlé.\n" +
                    "1) Un Admin valide la signature (onglet Signature).\n" +
                    "2) Clique 'Réinitialiser en 1234'.\n" +
                    "3) Saisis Nouveau + Confirmation puis valide."
            };

            lblFgEmployeeLogin = new Label
            {
                AutoSize = true,
                Location = new Point(22, 120),
                Text = "NomUtilisateur / Matricule de l'employé"
            };

            txtFgEmployeeLogin = new TextBox
            {
                Location = new Point(25, 140),
                Width = 520
            };

            lblFgNewPwd = new Label
            {
                AutoSize = true,
                Location = new Point(22, 180),
                Text = "Nouveau mot de passe"
            };

            txtFgNewPwd = new TextBox
            {
                Location = new Point(25, 200),
                Width = 520,
                PasswordChar = '●'
            };

            lblFgConfirmPwd = new Label
            {
                AutoSize = true,
                Location = new Point(22, 240),
                Text = "Confirmer nouveau mot de passe"
            };

            txtFgConfirmPwd = new TextBox
            {
                Location = new Point(25, 260),
                Width = 520,
                PasswordChar = '●'
            };

            // ✅ Bouton 1 : Reset en 1234
            btnFgReset1234 = new Button
            {
                Text = "Réinitialiser en 1234",
                Width = 200,
                Height = 36,
                Left = 25,
                Top = 315
            };
            btnFgReset1234.Click += btnFgReset1234_Click;

            // ✅ Bouton 2 : Valider nouveau mot de passe
            btnFgReset = new Button
            {
                Text = "Valider le nouveau mot de passe",
                Width = 260,
                Height = 36,
                Left = 285,
                Top = 315
            };
            btnFgReset.Click += btnFgReset_Click;

            // 🔁 Events de recalcul UI
            txtFgEmployeeLogin.TextChanged += (s, e) => UpdateForgotUiState();
            txtFgNewPwd.TextChanged += (s, e) => UpdateForgotUiState();
            txtFgConfirmPwd.TextChanged += (s, e) => UpdateForgotUiState();

            // ✅ état initial : pas encore 1234
            _fgTempApplied = false;

            tabForgot.Controls.Clear();
            tabForgot.Controls.Add(lblFgTitle);
            tabForgot.Controls.Add(lblFgHint);
            tabForgot.Controls.Add(lblFgEmployeeLogin);
            tabForgot.Controls.Add(txtFgEmployeeLogin);

            tabForgot.Controls.Add(lblFgNewPwd);
            tabForgot.Controls.Add(txtFgNewPwd);
            tabForgot.Controls.Add(lblFgConfirmPwd);
            tabForgot.Controls.Add(txtFgConfirmPwd);

            tabForgot.Controls.Add(btnFgReset1234);
            tabForgot.Controls.Add(btnFgReset);

            // 👁 Yeux
            btnEyeFgNew = CreateEyeButton(txtFgNewPwd, () => _showFgNew, v => _showFgNew = v);
            btnEyeFgConfirm = CreateEyeButton(txtFgConfirmPwd, () => _showFgConfirm, v => _showFgConfirm = v);

            tabForgot.Controls.Add(btnEyeFgNew);
            tabForgot.Controls.Add(btnEyeFgConfirm);

            btnEyeFgNew.BringToFront();
            btnEyeFgConfirm.BringToFront();

            // ✅ Applique l'état UI à la FIN (maintenant les boutons existent)
            UpdateForgotUiState();
        }

        private bool UpdateEmployePassword(int idEmploye, string newPwd)
        {
            // Ici "mot de passe" = PIN/secret stocké en PBKDF2 BIN
            string secret = (newPwd ?? "").Trim();
            if (secret.Length < 4) return false;

            byte[] saltBin = CreateSalt(16);
            byte[] hashBin = HashPBKDF2_64(secret, saltBin, iterations: 100000, bytes: 64);

            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET PinSaltBin = @salt,
    PinHashBin = @hash
WHERE ID_Employe = @id;

SELECT @@ROWCOUNT;
", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmploye;
                cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = saltBin;
                cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 64).Value = hashBin;

                con.Open();
                int rows = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                return rows > 0;
            }
        }



        private void BuildTabEmpreinte()
        {
            tabEmpreinte = new TabPage("Empreinte");

            lblHelloInfo = new Label
            {
                AutoSize = false,
                Location = new Point(18, 18),
                Size = new Size(535, 55),
                Text = "Empreinte via Windows Hello.\n" +
                       "1) Configure l'empreinte dans Windows.\n" +
                       "2) Puis ici : saisis le login du manager + pose le doigt."
            };

            lblHelloLogin = new Label
            {
                AutoSize = true,
                Location = new Point(22, 85),
                Text = "Login manager (NomUtilisateur ou Matricule)"
            };

            txtHelloLogin = new TextBox
            {
                Location = new Point(25, 105),
                Width = 520
            };

            lblHelloHint = new Label
            {
                AutoSize = false,
                Location = new Point(25, 135),
                Size = new Size(520, 30),
                ForeColor = Color.DimGray,
                Text = "Astuce : si Windows Hello n'est pas configuré, clique sur 'Configurer empreinte (Windows)'."
            };

            btnOpenWindowsHello = new Button
            {
                Text = "Configurer empreinte (Windows)",
                Width = 240,
                Height = 36,
                Left = 25,
                Top = 180
            };
            btnOpenWindowsHello.Click += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo("ms-settings:signinoptions") { UseShellExecute = true });
                    MessageBox.Show(
                        "Windows va s’ouvrir.\n" +
                        "Configure l’empreinte (Windows Hello), puis reviens ici et clique sur 'Vérifier empreinte'.",
                        "Empreinte", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch
                {
                    MessageBox.Show("Impossible d'ouvrir les paramètres Windows.", "Empreinte",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            btnVerifyHello = new Button
            {
                Text = "Vérifier empreinte",
                Width = 180,
                Height = 36,
                Left = 365,
                Top = 180
            };
            btnVerifyHello.Click += btnVerifyHello_Click;

            tabEmpreinte.Controls.Clear();
            tabEmpreinte.Controls.Add(lblHelloInfo);
            tabEmpreinte.Controls.Add(lblHelloLogin);
            tabEmpreinte.Controls.Add(txtHelloLogin);
            tabEmpreinte.Controls.Add(lblHelloHint);
            tabEmpreinte.Controls.Add(btnOpenWindowsHello);
            tabEmpreinte.Controls.Add(btnVerifyHello);

            if (!tabMain.TabPages.Contains(tabEmpreinte))
                tabMain.TabPages.Add(tabEmpreinte);
        }

        // ===================== EVENTS =====================

        private void FrmSignatureManager_Load(object sender, EventArgs e)
        {
            AppliquerDroitsTabs();

            // ✅ 1) Flow "Mot de passe oublié" depuis FormLogin
            // - On commence par Signature (validation manager)
            // - On ne doit PAS fermer automatiquement après validation
            _forgotEmployeeFlow = StartForgotEmployeeFlow;

            if (_forgotEmployeeFlow)
            {
                CloseOnApproved = false;          // ✅ important : ne pas quitter vers FormLogin
                _fgTempApplied = false;

                tabMain.SelectedTab = tabSignature;
                txtLogin?.Focus();

                UpdateForgotUiState();
                return;
            }

            // ✅ 2) Autre usage : ouvrir directement l’onglet "Mot de passe oublié" (si tu veux l’utiliser ailleurs)
            if (StartOnForgotTab && tabForgot != null && tabMain.TabPages.Contains(tabForgot))
            {
                tabMain.SelectedTab = tabForgot;
                txtFgEmployeeLogin?.Focus();
                UpdateForgotUiState();
                return;
            }

            UpdateForgotUiState();

            lblContexte.Text =
                $"Action : {_typeAction}\n" +
                $"Référence : {_reference}\n" +
                $"{_details}";

            if (StartOnEmpreinteTab && tabEmpreinte != null && tabMain.TabPages.Contains(tabEmpreinte))
            {
                tabMain.SelectedTab = tabEmpreinte;
                txtHelloLogin?.Focus();
                return;
            }

            tabMain.SelectedTab = tabSignature;
            txtLogin?.Focus();
        }

        private void FrmSignatureManager_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (tabMain.SelectedTab == tabSignature) btnAnnuler.PerformClick();
                else Close();

                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                if (tabMain.SelectedTab == tabSignature)
                    btnValider.PerformClick();
                else if (tabMain.SelectedTab == tabSetupPin)   // ✅
                    btnSetupSave.PerformClick();
                else if (tabMain.SelectedTab == tabConfigPin)
                    btnCfgSave.PerformClick();
                else if (tabMain.SelectedTab == tabForgot)
                    btnFgReset.PerformClick();
                else if (tabMain.SelectedTab == tabEmpreinte)
                    btnVerifyHello.PerformClick();

                e.Handled = true;
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            Approved = false;

            // ✅ reset complet (évite Approved=false mais ManagerId restant)
            ManagerId = 0;
            ManagerNom = "";
            ManagerPoste = "";

            try { InsertSignatureLog(approved: false, managerId: null, managerNom: null); } catch { }
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ===================== SIGNATURE : VALIDATION =====================

        private void btnValider_Click(object sender, EventArgs e)
        {
            try
            {
                string login = (txtLogin.Text ?? "").Trim();
                string code = (txtPin.Text ?? "").Trim(); // PIN OU MotDePasse

                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(code))
                {
                    MessageBox.Show("Veuillez saisir le login et le code (PIN / mot de passe).", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var mgr = GetEmployeManager(login, _permissionCode);
                if (mgr == null)
                {
                    MessageBox.Show("Manager introuvable ou login invalide.", "Accès refusé",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    try { InsertSignatureLog(false, null, login); } catch { }
                    return;
                }

                // ✅ Manager = IsManager OU PosteAdmin (Superviseur/Gérant/DG/Programmeur)
                bool isManager = mgr.IsManager || IsPosteManager(mgr.Poste);
                if (!isManager)
                {
                    MessageBox.Show("Cet employé n'a pas le niveau Manager requis.", "Accès refusé",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    try { InsertSignatureLog(false, mgr.ID_Employe, mgr.NomAff); } catch { }
                    return;
                }

                // ✅ Vérification : si PIN hash existe, on tente PIN puis fallback MDP
                bool ok = false;

                if (mgr.PinSaltBin != null && mgr.PinHashBin != null && mgr.PinHashBin.Length > 0)
                {
                    ok = VerifyPBKDF2(code, mgr.PinSaltBin, mgr.PinHashBin, iterations: 100000);
                }
                else
                {
                    // Aucun code configuré
                    ok = false;
                }

                if (!ok)
                {
                    MessageBox.Show("Code invalide (PIN).", "Accès refusé",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    try { InsertSignatureLog(false, mgr.ID_Employe, mgr.NomAff); } catch { }
                    return;
                }

                // ✅ Permission : HasPermission ou PosteAdmin
                if (!string.IsNullOrWhiteSpace(_permissionCode))
                {
                    bool hasPerm = mgr.HasPermission || IsPosteManager(mgr.Poste);
                    if (!hasPerm)
                    {
                        MessageBox.Show($"Permission refusée : {_permissionCode}", "Accès refusé",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        try { InsertSignatureLog(false, mgr.ID_Employe, mgr.NomAff); } catch { }
                        return;
                    }
                }

                // ✅ Succès
                Approved = true;
                ManagerId = mgr.ID_Employe;
                ManagerNom = mgr.NomAff;
                ManagerPoste = mgr.Poste;

                // UX
                UpdateForgotUiState();
                try { InsertSignatureLog(true, ManagerId, ManagerNom); } catch { }

                // Optionnel : vider le champ code après validation
                txtPin?.Clear();

                // ✅ Si on est dans le flow "j'ai oublié" (Manager)
                if (_pendingForgotFlow && !string.IsNullOrWhiteSpace(_pendingTargetLogin))
                {
                    int targetId = GetEmployeIdByLogin(_pendingTargetLogin);

                    if (targetId > 0 && targetId == ManagerId)
                    {
                        MessageBox.Show(
                            "⛔ Sécurité : tu ne peux pas valider et réinitialiser ton propre code.\n" +
                            "Demande à un autre Admin de valider.",
                            "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        _pendingForgotFlow = false;
                        _pendingTargetLogin = null;
                        return;
                    }

                    bool done = ResetEmployeToTemporary1234(_pendingTargetLogin);
                    if (!done)
                    {
                        MessageBox.Show("Reset impossible : employé introuvable.", "Erreur",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _pendingForgotFlow = false;
                        _pendingTargetLogin = null;
                        return;
                    }

                    LogResetPassword(_pendingTargetLogin);

                    // ✅ retour direct vers Changer avec ancien = 1234
                    tabMain.SelectedTab = tabConfigPin;
                    txtCfgLogin.Text = _pendingTargetLogin;
                    txtCfgOld.Text = "1234";
                    txtCfgNewPin.Clear();
                    txtCfgConfirmPin.Clear();
                    txtCfgNewPin.Focus();

                    _pendingForgotFlow = false;
                    _pendingTargetLogin = null;

                    MessageBox.Show("✅ Reset effectué (1234). Change maintenant ton code.", "Succès",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return;
                }

                // ✅ Si on est dans le flow Employé (depuis FormLogin) : après signature -> onglet Mot de passe oublié
                if (_forgotEmployeeFlow)
                {
                    tabMain.SelectedTab = tabForgot;

                    // reset étape
                    _fgTempApplied = false;

                    txtFgEmployeeLogin.Clear();
                    txtFgNewPwd.Clear();
                    txtFgConfirmPwd.Clear();

                    UpdateForgotUiState();
                    txtFgEmployeeLogin.Focus();

                    return; // ✅ ne pas fermer, ne pas toucher CloseOnApproved
                }

                // ✅ IMPORTANT :
                // Si ce FrmSignatureManager a été ouvert pour autoriser l'ouverture d'un module,
                // alors FormMain attend ShowDialog() == DialogResult.OK pour ouvrir le module.
                if (CloseOnApproved)
                {
                    DialogResult = DialogResult.OK;  // ✅ déclenche le retour OK au caller
                    Close();                         // ✅ ferme le dialog (sinon le module ne s’ouvrira jamais)
                    return;
                }

                // Sinon, on peut rester ouvert (ex: reset contrôlé)
                MessageBox.Show("✅ Signature validée. Tu peux continuer.", "Signature",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur signature manager : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== TAB : MOT DE PASSE OUBLIE =====================

        private void btnFgReset_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ Exiger signature manager validée avant reset
                if (!Approved || ManagerId <= 0)
                {
                    MessageBox.Show("⚠️ D’abord valider la signature manager (onglet Signature).", "Sécurité",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tabMain.SelectedTab = tabSignature;
                    txtLogin?.Focus();
                    return;
                }

                if (!_fgTempApplied)
                {
                    MessageBox.Show("D’abord clique sur 'Réinitialiser en 1234'.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string empLogin = (txtFgEmployeeLogin.Text ?? "").Trim();
                string newPwd = (txtFgNewPwd.Text ?? "").Trim();
                string confirm = (txtFgConfirmPwd.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(empLogin))
                {
                    MessageBox.Show("Saisis le NomUtilisateur / Matricule de l’employé.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFgEmployeeLogin.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(newPwd) || string.IsNullOrWhiteSpace(confirm))
                {
                    MessageBox.Show("Saisis le nouveau mot de passe + confirmation.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFgNewPwd.Focus();
                    return;
                }

                if (newPwd != confirm)
                {
                    MessageBox.Show("La confirmation ne correspond pas.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFgConfirmPwd.Focus();
                    return;
                }

                // (Optionnel) règle minimale
                if (newPwd.Length < 4)
                {
                    MessageBox.Show("Mot de passe trop court (min 4 caractères).", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFgNewPwd.Focus();
                    return;
                }

                int targetId = GetEmployeIdByLogin(empLogin);
                if (targetId <= 0)
                {
                    MessageBox.Show("Employé introuvable (NomUtilisateur / Matricule).", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFgEmployeeLogin.Focus();
                    return;
                }

                // ⛔ Interdire auto-reset
                if (targetId == ManagerId)
                {
                    MessageBox.Show(
                        "⛔ Sécurité : tu ne peux pas réinitialiser ton propre mot de passe.\n" +
                        "Demande à un autre Admin de valider la signature.",
                        "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ✅ Enregistrer le NOUVEAU mot de passe directement dans dbo.Employes
                bool saved = UpdateEmployePassword(targetId, newPwd);
                if (!saved)
                {
                    MessageBox.Show("Impossible d’enregistrer le nouveau mot de passe.", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LogResetPassword(empLogin);

                MessageBox.Show(
                    "✅ Nouveau mot de passe enregistré avec succès.\n\n" +
                    "➡ L’employé peut se connecter directement avec ce nouveau mot de passe.",
                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _fgTempApplied = false;
                UpdateForgotUiState();

                // UX
                txtFgEmployeeLogin.Clear();
                txtFgNewPwd.Clear();
                txtFgConfirmPwd.Clear();
                txtFgEmployeeLogin.Focus();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Erreur SQL : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetEmployeIdByLogin(string loginOrMatricule)
        {
            try
            {
                using (var con = new SqlConnection(_cs))
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 ID_Employe
FROM dbo.Employes
WHERE UPPER(LTRIM(RTRIM(ISNULL(NomUtilisateur,'')))) = UPPER(LTRIM(RTRIM(@x)))
   OR UPPER(LTRIM(RTRIM(ISNULL(Matricule,''))))      = UPPER(LTRIM(RTRIM(@x)));", con))
                {
                    cmd.Parameters.Add("@x", SqlDbType.NVarChar, 120).Value = (loginOrMatricule ?? "").Trim();
                    con.Open();

                    object o = cmd.ExecuteScalar();
                    if (o == null || o == DBNull.Value) return 0;
                    return Convert.ToInt32(o);
                }
            }
            catch
            {
                return 0;
            }
        }


        private void ResetEmployePassword_ByNomUtilisateur(string nomUtilisateur, string newPwd)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET MotDePasse = @pwd
WHERE UPPER(LTRIM(RTRIM(ISNULL(NomUtilisateur,'')))) = UPPER(LTRIM(RTRIM(@u)));

IF @@ROWCOUNT = 0
    RAISERROR(N'Employé introuvable (NomUtilisateur).', 16, 1);", con))
            {
                cmd.Parameters.Add("@u", SqlDbType.NVarChar, 120).Value = (nomUtilisateur ?? "").Trim();
                cmd.Parameters.Add("@pwd", SqlDbType.NVarChar, 200).Value = (newPwd ?? "").Trim();

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void ResetEmployePasswordAndPin_ByNomUtilisateur(string nomUtilisateur, string newPwd, string newPin)
        {
            // newPin doit être digits (ex: "1234")
            string pin = NormalizePin(newPin);
            if (pin.Length < 4) throw new Exception("PIN invalide (min 4 chiffres).");

            string salt = NewSalt();
            string hash = Sha256Hex(salt + pin);

            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET MotDePasse = @pwd,
    PinSalt    = @s,
    PinHash    = @h
WHERE UPPER(LTRIM(RTRIM(ISNULL(NomUtilisateur,'')))) = UPPER(LTRIM(RTRIM(@u)));

IF @@ROWCOUNT = 0
    RAISERROR(N'Employé introuvable (NomUtilisateur).', 16, 1);", con))
            {
                cmd.Parameters.Add("@u", SqlDbType.NVarChar, 120).Value = (nomUtilisateur ?? "").Trim();
                cmd.Parameters.Add("@pwd", SqlDbType.NVarChar, 200).Value = (newPwd ?? "").Trim();
                cmd.Parameters.Add("@s", SqlDbType.NVarChar, 80).Value = salt;
                cmd.Parameters.Add("@h", SqlDbType.NVarChar, 80).Value = hash;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ===================== CONFIG PIN TAB : SAVE =====================

        private void btnCfgSave_Click(object sender, EventArgs e)
        {
            try
            {
                string login = (txtCfgLogin.Text ?? "").Trim();
                string oldSecret = (txtCfgOld?.Text ?? "").Trim();
                string n1 = (txtCfgNewPin.Text ?? "").Trim();
                string n2 = (txtCfgConfirmPin.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(login))
                {
                    MessageBox.Show("Saisis le nom utilisateur / matricule.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCfgLogin.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(oldSecret))
                {
                    MessageBox.Show("Saisis l'ancien mot de passe / PIN.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCfgOld?.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(n1) || string.IsNullOrWhiteSpace(n2))
                {
                    MessageBox.Show("Saisis le nouveau code et la confirmation.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCfgNewPin.Focus();
                    return;
                }

                if (n1 != n2)
                {
                    MessageBox.Show("Les deux nouveaux codes ne correspondent pas.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCfgConfirmPin.Focus();
                    return;
                }

                var emp = GetEmployeManager(login, permCode: null);
                if (emp == null)
                {
                    MessageBox.Show("Employé introuvable (login/matricule).", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool isManager = emp.IsManager || IsPosteManager(emp.Poste);
                if (!isManager)
                {
                    MessageBox.Show("Cet employé n'a pas le niveau Manager/Directeur/Superviseur.", "Accès refusé",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool oldOk = false;

                // ✅ PIN configuré ? (BIN)
                if (emp.PinSaltBin != null && emp.PinHashBin != null && emp.PinHashBin.Length > 0)
                {
                    oldOk = VerifyPBKDF2(oldSecret, emp.PinSaltBin, emp.PinHashBin, iterations: 100000);
                }
                else
                {
                    oldOk = false; // aucun code configuré
                }

                if (!oldOk)
                {
                    MessageBox.Show("Ancien PIN incorrect.", "Refus",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ On garde PIN hash
                string newPin = NormalizePin(n1);
                if (newPin.Length < 4)
                {
                    MessageBox.Show("Nouveau PIN invalide. Minimum 4 chiffres.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                byte[] saltBin = CreateSalt(16);
                byte[] hashBin = HashPBKDF2_64(newPin, saltBin, iterations: 100000, bytes: 64);

                UpdateEmployePin(emp.ID_Employe, saltBin, hashBin);

                txtCfgOld?.Clear();
                txtCfgNewPin.Clear();
                txtCfgConfirmPin.Clear();

                MessageBox.Show("✅ PIN modifié avec succès.", "Succès",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Erreur SQL : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== DB HELPERS =====================

        private bool VerifyManagerPassword(string login, string password)
        {
            try
            {
                using (var con = new SqlConnection(_cs))
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 1
FROM dbo.Employes e
WHERE (UPPER(LTRIM(RTRIM(ISNULL(e.NomUtilisateur,'')))) = UPPER(LTRIM(RTRIM(@login)))
    OR UPPER(LTRIM(RTRIM(ISNULL(e.Matricule,''))))      = UPPER(LTRIM(RTRIM(@login))))
  AND ISNULL(e.MotDePasse,'') = @pwd
  AND (ISNULL(e.IsManager,0)=1 
       OR LTRIM(RTRIM(ISNULL(e.Poste,''))) IN (N'Superviseur',N'Gérant',N'Directeur Général',N'Programmeur'));
", con))
                {
                    cmd.Parameters.Add("@login", SqlDbType.NVarChar, 120).Value = (login ?? "").Trim();
                    cmd.Parameters.Add("@pwd", SqlDbType.NVarChar, 200).Value = (password ?? "").Trim();

                    con.Open();
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch { return false; }
        }

        private ManagerRow GetEmployeManager(string login, string permCode)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
SELECT TOP 1
    e.ID_Employe,
    (ISNULL(e.Nom,'') + ' ' + ISNULL(e.Prenom,'')) AS NomAff,
    LTRIM(RTRIM(ISNULL(e.Poste,''))) AS Poste,

    e.PinSaltBin,
    e.PinHashBin,

    ISNULL(e.IsManager,0) AS IsManager,

    CASE 
        WHEN @perm IS NULL OR LTRIM(RTRIM(@perm))='' THEN 1
        WHEN EXISTS (
            SELECT 1
            FROM dbo.Roles r
            JOIN dbo.RoleModules rm ON rm.IdRole = r.IdRole AND ISNULL(rm.Autorise,0)=1
            JOIN dbo.Modules m ON m.IdModule = rm.IdModule
            WHERE LTRIM(RTRIM(ISNULL(r.NomRole,''))) = LTRIM(RTRIM(ISNULL(e.Poste,'')))
              AND m.CodeModule = LTRIM(RTRIM(@perm))
        ) THEN 1 ELSE 0 
    END AS HasPermission
FROM dbo.Employes e
WHERE UPPER(LTRIM(RTRIM(ISNULL(e.NomUtilisateur,'')))) = UPPER(LTRIM(RTRIM(@login)))
   OR UPPER(LTRIM(RTRIM(ISNULL(e.Matricule,''))))      = UPPER(LTRIM(RTRIM(@login)));", con))
            {
                cmd.Parameters.Add("@login", SqlDbType.NVarChar, 120).Value = (login ?? "").Trim();
                cmd.Parameters.Add("@perm", SqlDbType.NVarChar, 80).Value = (permCode ?? "").Trim();

                con.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;

                    return new ManagerRow
                    {
                        ID_Employe = Convert.ToInt32(rd["ID_Employe"]),
                        NomAff = Convert.ToString(rd["NomAff"] ?? ""),
                        Poste = Convert.ToString(rd["Poste"] ?? ""),

                        PinSaltBin = SecurityBytes.DbToBytes(rd["PinSaltBin"]),
                        PinHashBin = SecurityBytes.DbToBytes(rd["PinHashBin"]),

                        IsManager = Convert.ToInt32(rd["IsManager"]) == 1,
                        HasPermission = Convert.ToInt32(rd["HasPermission"]) == 1
                    };
                }
            }
        }

        private void UpdateEmployePin(int idEmploye, byte[] saltBin, byte[] hashBin)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET PinSaltBin = @salt,
    PinHashBin = @hash
WHERE ID_Employe = @id;
", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmploye;
                cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = (object)saltBin ?? DBNull.Value;
                cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 64).Value = (object)hashBin ?? DBNull.Value;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ===================== EMPREINTE =====================

        private void btnVerifyHello_Click(object sender, EventArgs e)
        {
            try
            {
                string login = (txtHelloLogin.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(login))
                {
                    MessageBox.Show("Saisis le login du manager.", "Empreinte",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtHelloLogin.Focus();
                    return;
                }

                var mgr = GetEmployeManager(login, _permissionCode);
                if (mgr == null)
                {
                    MessageBox.Show("Manager introuvable.", "Empreinte",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!(mgr.IsManager || IsPosteManager(mgr.Poste)))
                {
                    MessageBox.Show("Cet employé n'a pas le niveau Manager requis.", "Empreinte",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!VerifyWithWindowsHello())
                    return;

                Approved = true;
                ManagerId = mgr.ID_Employe;
                ManagerNom = mgr.NomAff;
                ManagerPoste = mgr.Poste;

                UpdateForgotUiState();

                // ✅ Flow FormLogin : après empreinte OK -> onglet Mot de passe oublié (et on ne ferme pas)
                if (_forgotEmployeeFlow)
                {
                    tabMain.SelectedTab = tabForgot;

                    _fgTempApplied = false;
                    txtFgEmployeeLogin.Clear();
                    txtFgNewPwd.Clear();
                    txtFgConfirmPwd.Clear();

                    UpdateForgotUiState();
                    txtFgEmployeeLogin.Focus();
                    return;
                }

                // ✅ Sinon, comportement normal
                MessageBox.Show("✅ Empreinte validée. Signature OK.", "Empreinte",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (CloseOnApproved)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur empreinte : " + ex.Message, "Empreinte",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSetupSave_Click(object sender, EventArgs e)
        {
            try
            {
                string login = (txtSetupLogin.Text ?? "").Trim();
                string p1 = (txtSetupNewPin.Text ?? "").Trim();
                string p2 = (txtSetupConfirmPin.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(login))
                {
                    MessageBox.Show("Saisis le nom utilisateur / matricule.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSetupLogin.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(p1) || string.IsNullOrWhiteSpace(p2))
                {
                    MessageBox.Show("Saisis le nouveau PIN et la confirmation.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSetupNewPin.Focus();
                    return;
                }

                if (p1 != p2)
                {
                    MessageBox.Show("Les deux PIN ne correspondent pas.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtSetupConfirmPin.Focus();
                    return;
                }

                var emp = GetEmployeManager(login, permCode: null);
                if (emp == null)
                {
                    MessageBox.Show("Employé introuvable (login/matricule).", "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool isManager = emp.IsManager || IsPosteManager(emp.Poste);
                if (!isManager)
                {
                    MessageBox.Show("Cet employé n'a pas le niveau Manager/Directeur/Superviseur.", "Accès refusé",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ✅ Empêcher d'écraser si déjà configuré (première fois uniquement)
                if (emp.PinSaltBin != null && emp.PinSaltBin.Length > 0
                && emp.PinHashBin != null && emp.PinHashBin.Length > 0)
                {
                    MessageBox.Show("Ce manager a déjà un code configuré.\nUtilise l’onglet 'Changer mot de passe'.",
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string newPin = NormalizePin(p1);
                if (newPin.Length < 4)
                {
                    MessageBox.Show("PIN invalide. Minimum 4 chiffres.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                byte[] saltBin = CreateSalt(16);
                byte[] hashBin = HashPBKDF2_64(newPin, saltBin, iterations: 100000, bytes: 64);

                UpdateEmployePin(emp.ID_Employe, saltBin, hashBin);

                txtSetupNewPin.Clear();
                txtSetupConfirmPin.Clear();

                MessageBox.Show("✅ PIN configuré avec succès.", "Succès",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool VerifyWithWindowsHello()
        {
            try
            {
                string helperExe = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "HelloVerifyHelper.exe"
                );

                if (!System.IO.File.Exists(helperExe))
                {
                    MessageBox.Show("HelloVerifyHelper.exe introuvable.", "Empreinte",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var psi = new ProcessStartInfo(helperExe, "\"Signature manager requise\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var p = Process.Start(psi))
                {
                    p.WaitForExit();
                    return p.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        // ===================== LOG =====================

        private void InsertSignatureLog(bool approved, int? managerId, string managerNom)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.ManagerSignatureLog
(DateSignature, TypeAction, Reference, Details, ID_EmployeDemandeur, ID_Manager, ManagerNom, Machine)
VALUES
(GETDATE(), @t, @r, @d, @dem, @mid, @mnom, @mach);", con))
            {
                cmd.Parameters.Add("@t", SqlDbType.NVarChar, 60).Value = Truncate((_typeAction ?? "").Trim(), 60);

                cmd.Parameters.Add("@r", SqlDbType.NVarChar, 120).Value =
                    string.IsNullOrWhiteSpace(_reference) ? (object)DBNull.Value : Truncate(_reference.Trim(), 120);

                string details = BuildLogDetails(approved);
                cmd.Parameters.Add("@d", SqlDbType.NVarChar, 500).Value =
                    string.IsNullOrWhiteSpace(details) ? (object)DBNull.Value : Truncate(details, 500);

                cmd.Parameters.Add("@dem", SqlDbType.Int).Value =
                    _idEmployeDemandeur.HasValue ? (object)_idEmployeDemandeur.Value : DBNull.Value;

                cmd.Parameters.Add("@mid", SqlDbType.Int).Value =
                    managerId.HasValue ? (object)managerId.Value : DBNull.Value;

                cmd.Parameters.Add("@mnom", SqlDbType.NVarChar, 120).Value =
                    string.IsNullOrWhiteSpace(managerNom) ? (object)DBNull.Value : Truncate(managerNom.Trim(), 120);

                cmd.Parameters.Add("@mach", SqlDbType.NVarChar, 120).Value = Truncate(BuildMachineTag(), 120);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string BuildLogDetails(bool approved)
        {
            var ok = approved ? "1" : "0";
            var perm = string.IsNullOrWhiteSpace(_permissionCode) ? "" : _permissionCode.Trim();
            string baseDetails = (_details ?? "").Trim();

            string header = string.IsNullOrWhiteSpace(perm)
                ? $"[OK={ok}]"
                : $"[OK={ok}][PERM={perm}]";

            if (string.IsNullOrWhiteSpace(baseDetails))
                return header;

            return header + " " + baseDetails;
        }

        private string BuildMachineTag()
        {
            string pc = Environment.MachineName ?? "";
            string ip = GetLocalIp() ?? "";
            if (string.IsNullOrWhiteSpace(ip)) return pc.Trim();
            if (string.IsNullOrWhiteSpace(pc)) return ip.Trim();
            return (pc.Trim() + " | " + ip.Trim());
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = s.Trim();
            return (s.Length <= max) ? s : s.Substring(0, max);
        }

        // ===================== HELPERS =====================

        private static string NormalizePin(string input)
        {
            if (input == null) return "";
            return new string(input.Where(char.IsDigit).ToArray());
        }

        

        public static string NewSalt()
        {
            byte[] bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        public static string Sha256Hex(string input)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input ?? "");
                byte[] hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static bool SlowEqualsHex(string a, string b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }

        private static string GetLocalIp()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
                }
                return null;
            }
            catch { return null; }
        }

        private class ManagerRow
        {
            public int ID_Employe;
            public string NomAff;
            public string Poste;

            // ✅ Secrets depuis dbo.Employes (BIN)
            public byte[] PinSaltBin;
            public byte[] PinHashBin;

            public bool IsManager;
            public bool HasPermission;
        }
    }
}