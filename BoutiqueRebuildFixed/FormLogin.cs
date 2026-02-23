using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using BoutiqueRebuildFixed;

namespace BoutiqueRebuildFixed
{
    public partial class FormLogin : FormBase
    {
        // UI Controls
        private Panel root;
        private Panel card;
        private Panel leftBrand;
        private Panel rightForm;

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblBrand;
        private Label lblBrandSub;

        private Panel pnlUser;
        private Panel pnlPass;

        private PictureBox icoUser;
        private PictureBox icoLock;

        private TextBox txtUser;
        private TextBox txtPass;

        private Button btnTogglePass;
        private Button btnConnexionPro;

        private LinkLabel lnkForgot;
        private Label lblError;

        // Drag (si tu mets FormBorderStyle=None)
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private const int PBKDF2_ITER = 100000;

        public FormLogin()
        {
            InitializeComponent();

            // 👇 Optionnel : look pro sans bord
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(18, 18, 22);
            this.MinimumSize = new Size(900, 520);

            // Events
            this.Shown += (s, e) =>
            {
                LogOuvrir();
                ConfigSysteme.ChargerConfig();

                BuildUi();
                ApplyProTheme();

                // Abonne-toi APRES BuildUi (important)
                ConfigSysteme.OnLangueChange -= RafraichirLangue;
                ConfigSysteme.OnThemeChange -= RafraichirTheme;
                ConfigSysteme.OnLangueChange += RafraichirLangue;
                ConfigSysteme.OnThemeChange += RafraichirTheme;

                RafraichirLangue();
                RafraichirTheme();
            };

            this.FormClosed += (s, e) => LogSortir();
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            lblBienvenue.Text = "Bienvenue dans la Boutique ZAIRE.";

            RafraichirLangue();
            RafraichirTheme();
        }

        private void RafraichirLangue()
        {
            // Si l'UI n'est pas encore construite, on sort proprement
            if (lblTitle == null || lblSubtitle == null || btnConnexionPro == null || lnkForgot == null)
                return;

            // Traductions globales (ok)
            ConfigSysteme.AppliquerTraductions(this);

            // Textes par défaut
            lblTitle.Text = "Connexion";
            lblSubtitle.Text = "Accédez au système de caisse ZAIRE";
            btnConnexionPro.Text = "Se connecter";
            lnkForgot.Text = "Mot de passe oublié ?";
        }

        private void RafraichirTheme()
        {
            // Si ton theme change les BackColor etc, tu peux le laisser.
            ConfigSysteme.AppliquerTheme(this);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        private static extern bool DeleteObject(IntPtr hObject);

        private static void ApplyRoundedRegion(Control c, int radius)
        {
            if (c.Width <= 0 || c.Height <= 0) return;

            // IMPORTANT: libérer l'ancienne region
            var old = c.Region;
            IntPtr hrgn = CreateRoundRectRgn(0, 0, c.Width + 1, c.Height + 1, radius, radius);

            try
            {
                c.Region = Region.FromHrgn(hrgn);
            }
            finally
            {
                // libère le handle natif
                DeleteObject(hrgn);
                old?.Dispose();
            }
        }

        private void BuildUi()
        {
            this.Controls.Clear();

            // Root
            root = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28),
                BackColor = Color.Transparent
            };
            this.Controls.Add(root);

            // Card
            card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 24, 30),
                Padding = new Padding(0),
            };
            root.Controls.Add(card);

            // Arrondis
            card.Resize += (s, e) => ApplyRoundedRegion(card, 22);
            ApplyRoundedRegion(card, 22); // 1er apply

            // Ombre simple via Paint root (optionnel)
            root.Paint += (s, e) => DrawShadow(e.Graphics, card.Bounds);

            // Layout: 2 colonnes
            leftBrand = new Panel
            {
                Dock = DockStyle.Left,
                Width = 360,
                BackColor = Color.FromArgb(18, 90, 70), // Vert pro
                Padding = new Padding(26)
            };
            rightForm = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 24, 30),
                Padding = new Padding(34)
            };
            card.Controls.Add(rightForm);
            card.Controls.Add(leftBrand);

            // Drag sur zones (Form sans bord)
            leftBrand.MouseDown += DragForm;
            rightForm.MouseDown += DragForm;
            card.MouseDown += DragForm;

            BuildBrandPanel();
            BuildFormPanel();
        }

        private void BuildBrandPanel()
        {
            lblBrand = new Label
            {
                Text = "ZAIRE",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 20)
            };
            lblBrandSub = new Label
            {
                Text = "POS • Caisse • Stock • Ventes",
                ForeColor = Color.FromArgb(230, 255, 245),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(2, 70)
            };

            var badge = new Label
            {
                Text = "PRO",
                ForeColor = Color.FromArgb(18, 90, 70),
                BackColor = Color.FromArgb(220, 255, 245),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(54, 28),
                Location = new Point(0, 105)
            };
            badge.Paint += (s, e) =>
            {
                badge.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, badge.Width, badge.Height, 12, 12));
            };

            var line = new Panel
            {
                Height = 1,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(255, 255, 255),
                Visible = false
            };

            var tips = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 110,
                Text = "• Connexion sécurisée\n• Interface rapide pour ventes\n• Gestion des utilisateurs & rôles",
                ForeColor = Color.FromArgb(235, 255, 250),
                Font = new Font("Segoe UI", 10),
                Padding = new Padding(0, 0, 0, 14)
            };

            leftBrand.Controls.Add(tips);
            leftBrand.Controls.Add(line);
            leftBrand.Controls.Add(badge);
            leftBrand.Controls.Add(lblBrandSub);
            leftBrand.Controls.Add(lblBrand);
        }

        private void BuildFormPanel()
        {
            // Boutons fenêtre (close/minimize) pour form borderless
            var btnClose = new Button
            {
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Width = 40,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(rightForm.Width - 45, 5),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            rightForm.Controls.Add(btnClose);
            rightForm.Resize += (s, e) => btnClose.Location = new Point(rightForm.Width - 45, 5);

            lblTitle = new Label
            {
                Text = "Connexion",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 45)
            };
            lblSubtitle = new Label
            {
                Text = "Accédez au système de caisse ZAIRE",
                ForeColor = Color.FromArgb(190, 190, 200),
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(2, 90)
            };

            rightForm.Controls.Add(lblTitle);
            rightForm.Controls.Add(lblSubtitle);

            // Champs
            pnlUser = BuildInputRow("Nom d'utilisateur", out txtUser, out icoUser, isPassword: false);
            pnlPass = BuildInputRow("Mot de passe", out txtPass, out icoLock, isPassword: true);
            // ✅ Toggle password (œil) - bien placé et robuste
            btnTogglePass = new Button
            {
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(220, 220, 230),
                BackColor = Color.Transparent,
                Width = 42,
                Height = 42,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btnTogglePass.FlatAppearance.BorderSize = 0;

            // Important : l'œil ne doit pas "voler" le focus du textbox
            btnTogglePass.MouseDown += (s, e) =>
            {
                // appui -> montrer
                txtPass.UseSystemPasswordChar = false;
                btnTogglePass.Text = "🙈";
            };
            btnTogglePass.MouseUp += (s, e) =>
            {
                // relâche -> cacher
                txtPass.UseSystemPasswordChar = true;
                btnTogglePass.Text = "👁";
            };

            // Place dans pnlPass
            pnlPass.Controls.Add(btnTogglePass);

            // Position à droite, centré verticalement
            void PlaceEye()
            {
                btnTogglePass.Left = pnlPass.Width - btnTogglePass.Width - 10;
                btnTogglePass.Top = (pnlPass.Height - btnTogglePass.Height) / 2;
            }
            pnlPass.Resize += (s, e) => PlaceEye();
            PlaceEye();

            // Ajuste la largeur du textbox pour ne pas passer sous l'œil
            void FixPasswordTextWidth()
            {
                txtPass.Width = pnlPass.Width - 70 - btnTogglePass.Width; // 70 = marge + icône
            }
            pnlPass.Resize += (s, e) => FixPasswordTextWidth();
            FixPasswordTextWidth();


            pnlUser.Location = new Point(0, 140);
            pnlPass.Location = new Point(0, 215);

            rightForm.Controls.Add(pnlUser);
            rightForm.Controls.Add(pnlPass);

            // Toggle password
            btnTogglePass = new Button
            {
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(220, 220, 230),
                BackColor = Color.Transparent,
                Width = 46,
                Height = 42,
                Cursor = Cursors.Hand
            };
            btnTogglePass.FlatAppearance.BorderSize = 0;
            btnTogglePass.Click += (s, e) =>
            {
                txtPass.UseSystemPasswordChar = !txtPass.UseSystemPasswordChar;
                btnTogglePass.Text = txtPass.UseSystemPasswordChar ? "👁" : "🙈";
            };
            // Place toggle à droite du champ password
            btnTogglePass.Parent = pnlPass;
            btnTogglePass.Location = new Point(pnlPass.Width - 52, 10);
            pnlPass.Resize += (s, e) => btnTogglePass.Location = new Point(pnlPass.Width - 52, 10);

            // Error label (au lieu de MessageBox)
            lblError = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(255, 120, 120),
                Font = new Font("Segoe UI", 9),
                AutoSize = false,
                Height = 30,
                Dock = DockStyle.None,
                Location = new Point(2, 285),
                Width = 520
            };
            rightForm.Controls.Add(lblError);

            // Forgot
            lnkForgot = new LinkLabel
            {
                Text = "Mot de passe oublié ?",
                LinkColor = Color.FromArgb(120, 190, 255),
                ActiveLinkColor = Color.White,
                VisitedLinkColor = Color.FromArgb(120, 190, 255),
                AutoSize = true,
                Location = new Point(2, 320)
            };
            lnkForgot.Click += (s, e) =>
            {
                try
                {
                    using (var frm = new FrmSignatureManager(
                        ConfigSysteme.ConnectionString,
                        typeAction: "FORGOT_PASSWORD",
                        permissionCode: "",                 // pas de module requis ici
                        reference: "FormLogin",
                        details: "Réinitialisation mot de passe depuis l'écran de connexion",
                        idEmployeDemandeur: null,           // pas connecté
                        roleDemandeur: null                 // IMPORTANT : null au login
                    ))
                    {
                        // ✅ IMPORTANT :
                        // - On démarre sur Signature (validation manager)
                        // - Après validation, on va sur "Mot de passe oublié" sans fermer
                        frm.StartForgotEmployeeFlow = true;   // ✅ NOUVEAU
                        frm.StartOnForgotTab = false;         // ✅ éviter d’ouvrir directement tabForgot
                        frm.CloseOnApproved = false;          // ✅ sécurité supplémentaire

                        frm.ShowDialog(this);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur ouverture réinitialisation : " + ex.Message,
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            rightForm.Controls.Add(lnkForgot); // ✅ OBLIGATOIRE sinon il n'apparaît pas

            // Button connect
            btnConnexionPro = new Button
            {
                Text = "Se connecter",
                Width = 260,
                Height = 48,
                Location = new Point(0, 370),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(18, 140, 110)
            };
            btnConnexionPro.FlatAppearance.BorderSize = 0;
            btnConnexionPro.Paint += (s, e) =>
            {
                btnConnexionPro.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnConnexionPro.Width, btnConnexionPro.Height, 14, 14));
            };

            btnConnexionPro.MouseEnter += (s, e) => btnConnexionPro.BackColor = Color.FromArgb(22, 165, 130);
            btnConnexionPro.MouseLeave += (s, e) => btnConnexionPro.BackColor = Color.FromArgb(18, 140, 110);

            btnConnexionPro.Click += btnConnexion_Click;
            rightForm.Controls.Add(btnConnexionPro);

            // Enter = connexion
            txtPass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnConnexionPro.PerformClick(); };

            // 🔁 Map tes anciens TextBox si tu veux garder noms existants
            // (Optionnel) si ton code existant utilise txtNomUtilisateur / txtMotDePasse
            // Alors tu peux soit renommer ici, soit faire :
            // txtNomUtilisateur = txtUser; txtMotDePasse = txtPass;
        }

        private Panel BuildInputRow(string placeholder, out TextBox tb, out PictureBox icon, bool isPassword)
        {
            var panel = new Panel
            {
                Width = 520,
                Height = 60,
                BackColor = Color.FromArgb(30, 30, 38),
                Padding = new Padding(14, 10, 14, 10)
            };

            panel.Resize += (s, e) => ApplyRoundedRegion(panel, 14);
            ApplyRoundedRegion(panel, 14);

            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(55, 55, 70), 1))
                using (var path = RoundedRectPath(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 14))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            };

            icon = new PictureBox
            {
                Width = 22,
                Height = 22,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = new Point(10, 18)
            };

            icon.Image = isPassword ? DrawLockIcon() : DrawUserIcon();

            // ✅ IMPORTANT: on utilise une variable locale au lieu de "tb" (out)
            TextBox localTb = new TextBox
            {
                BorderStyle = BorderStyle.None,
                ForeColor = Color.FromArgb(240, 240, 245),
                BackColor = Color.FromArgb(30, 30, 38),
                Font = new Font("Segoe UI", 12),
                Location = new Point(42, 18),
                Width = panel.Width - 70,
                UseSystemPasswordChar = isPassword
            };

            // Placeholder
            localTb.GotFocus += (s, e) =>
            {
                if (localTb.Text == placeholder)
                {
                    localTb.Text = "";
                    localTb.ForeColor = Color.FromArgb(240, 240, 245);
                }
                if (lblError != null) lblError.Text = "";
            };

            localTb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(localTb.Text))
                {
                    localTb.Text = placeholder;
                    localTb.ForeColor = Color.FromArgb(150, 150, 165);
                }
            };

            // Init placeholder
            localTb.Text = placeholder;
            localTb.ForeColor = Color.FromArgb(150, 150, 165);

            panel.Controls.Add(icon);
            panel.Controls.Add(localTb);

            panel.Resize += (s, e) =>
            {
                localTb.Width = panel.Width - 70;
            };

            // ✅ assigner le out à la FIN
            tb = localTb;

            return panel;
        }

        private void ApplyProTheme()
        {
            // Ajuste tailles responsives
            rightForm.Resize += (s, e) =>
            {
                int w = Math.Min(560, rightForm.Width - 10);
                pnlUser.Width = w;
                pnlPass.Width = w;
                btnConnexionPro.Width = Math.Min(320, w);
                lblError.Width = w;

                btnConnexionPro.Location = new Point(0, 370);
            };
        }


        private void btnConnexion_Click(object sender, EventArgs e)
        {
            string nom = (txtUser.Text ?? "").Trim();
            string motDePasse = (txtPass.Text ?? "").Trim();

            if (nom == "Nom d'utilisateur") nom = "";
            if (motDePasse == "Mot de passe") motDePasse = "";

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(motDePasse))
            {
                lblError.ForeColor = Color.FromArgb(255, 120, 120);
                lblError.Text = "Veuillez saisir le nom d'utilisateur et le mot de passe.";
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();


                    bool connected = false;
                    bool usedFallbackClear = false;

                    // ✅ 1) Essai PIN BIN (16/64) - pas de return en cas d'échec
                    string queryPin = @"
SELECT TOP 1
    ID_Employe, Nom, Prenom, Poste,
    PinSaltBin, PinHashBin
FROM dbo.Employes
WHERE NomUtilisateur = @NomUtilisateur
  AND IsActif = 1;";

                    byte[] saltBin = null;
                    byte[] hashBin = null;

                    using (SqlCommand cmd = new SqlCommand(queryPin, con))
                    {
                        cmd.Parameters.Add("@NomUtilisateur", SqlDbType.NVarChar, 80).Value = nom;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                saltBin = SecurityBytes.DbToBytes(dr["PinSaltBin"]);
                                hashBin = SecurityBytes.DbToBytes(dr["PinHashBin"]);

                                // ✅ ne tente PBKDF2 que si format exact 16/64
                                if (saltBin != null && hashBin != null && saltBin.Length == 16 && hashBin.Length == 64)
                                {
                                    if (VerifyPBKDF2_64(motDePasse, saltBin, hashBin))
                                    {
                                        SessionEmploye.ID_Employe = Convert.ToInt32(dr["ID_Employe"]);
                                        SessionEmploye.Nom = dr["Nom"].ToString();
                                        SessionEmploye.Prenom = dr["Prenom"].ToString();
                                        SessionEmploye.Poste = dr["Poste"].ToString();
                                        connected = true;
                                    }
                                    // ❌ si échec -> on NE return PAS. On laisse fallback faire son travail.
                                }
                            }
                        }
                    }

                    // ============================================================
                    // ✅ 2) FALLBACK TEMPORAIRE (MOT DE PASSE EN CLAIR) => À SUPPRIMER
                    // ============================================================
                    if (!connected)
                    {
                        string queryClear = @"
SELECT TOP 1 ID_Employe, Nom, Prenom, Poste
FROM dbo.Employes
WHERE NomUtilisateur = @NomUtilisateur
  AND MotDePasse = @MotDePasse
  AND IsActif = 1;";

                        using (SqlCommand cmd = new SqlCommand(queryClear, con))
                        {
                            cmd.Parameters.Add("@NomUtilisateur", SqlDbType.NVarChar, 80).Value = nom;
                            cmd.Parameters.Add("@MotDePasse", SqlDbType.NVarChar, 200).Value = motDePasse;

                            using (SqlDataReader dr = cmd.ExecuteReader())
                            {
                                if (!dr.Read())
                                {
                                    ConfigSysteme.AjouterAuditLog("Connexion",
                                        $"Échec connexion '{nom}' (mauvais identifiants)", "Échec");

                                    lblError.ForeColor = Color.FromArgb(255, 120, 120);
                                    lblError.Text = "Nom d'utilisateur ou mot de passe incorrect.";
                                    return;
                                }

                                SessionEmploye.ID_Employe = Convert.ToInt32(dr["ID_Employe"]);
                                SessionEmploye.Nom = dr["Nom"].ToString();
                                SessionEmploye.Prenom = dr["Prenom"].ToString();
                                SessionEmploye.Poste = dr["Poste"].ToString();
                                connected = true;

                                usedFallbackClear = true; // ✅ on a utilisé MotDePasse
                            }
                        }

                        // ✅ Migration automatique après fermeture du reader
                        if (connected && usedFallbackClear)
                        {
                            try
                            {
                                MigrerMotDePasseClairVersPinBin(con, SessionEmploye.ID_Employe, motDePasse);

                                ConfigSysteme.AjouterAuditLog("Connexion",
                                    $"Migration MotDePasse->PinHash OK | ID={SessionEmploye.ID_Employe}", "Succès");
                            }
                            catch (Exception exMig)
                            {
                                // On ne bloque pas la connexion si migration échoue
                                ConfigSysteme.AjouterAuditLog("Connexion",
                                    $"Migration MotDePasse->PinHash ÉCHEC | ID={SessionEmploye.ID_Employe} | {exMig.Message}", "Échec");
                            }
                        }
                    }

                    // ============================================================
                    // ✅ 3) Charger contexte POS (signature avec out string)
                    // ============================================================
                    if (PosContextService.ChargerContextePOS(out string msgPos))
                    {
                        AppContext.PosConfigured = true;
                        AppContext.ModeConfigPOS = false;
                    }
                    else
                    {
                        AppContext.PosConfigured = false;
                        AppContext.ModeConfigPOS = true;

                        MessageBox.Show(
                            "⚠️ Ce PC n'est pas configuré comme POS.\n" +
                            "Ouvre : BOSS > Configuration Poste POS\n\n" +
                            "Détail : " + msgPos,
                            "Configuration POS",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }

                    // ============================================================
                    // ✅ 4) Audit + UI + ouverture FormMain
                    // ============================================================
                    ConfigSysteme.AjouterAuditLog("Connexion",
                        $"Connexion réussie '{nom}' (ID={SessionEmploye.ID_Employe}) | Poste={SessionEmploye.Poste} | POS={AppContext.NomPOS}",
                        "Succès");

                    lblError.ForeColor = Color.FromArgb(120, 255, 170);
                    lblError.Text = $"Bienvenue {SessionEmploye.Prenom} {SessionEmploye.Nom}…";

                    try
                    {
                        int nb = MigrerTousLesComptesClairsVersPinBin(con);
                        if (nb > 0)
                            ConfigSysteme.AjouterAuditLog("SECURITY", $"Migration BIN auto: {nb} comptes migrés", "Succès");
                    }
                    catch (Exception exMigAll)
                    {
                        ConfigSysteme.AjouterAuditLog("SECURITY", $"Migration BIN auto ÉCHEC: {exMigAll.Message}", "Échec");
                    }

                    using (FormMain main = new FormMain())
                    {
                        main.RoleUtilisateur = SessionEmploye.Poste;

                        this.Hide();
                        main.ShowDialog(this);
                        this.Close();
                    }
                }
            }
            catch (SqlException ex)
            {
                ConfigSysteme.AjouterAuditLog("Connexion", $"SQL ERROR '{nom}' : {ex}", "Erreur");

                MessageBox.Show(
                    ex.Message,
                    "Erreur SQL (détail)",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Connexion", $"APP ERROR '{nom}' : {ex}", "Erreur");

                MessageBox.Show(
                    ex.ToString(),
                    "Erreur application (détail)",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ====== Helpers graphiques ======
        [DllImport("gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private static GraphicsPath RoundedRectPath(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ============================================================
        // ✅ SECURITY HELPERS (PBKDF2 64 bytes + Migration)
        // ============================================================

        private static byte[] CreateSalt(int size = 16)
        {
            byte[] salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);
            return salt;
        }

        private void MigrerMotDePasseClairVersPinBin(SqlConnection con, int idEmploye, string motDePasse)
        {
            if (idEmploye <= 0) return;
            if (string.IsNullOrWhiteSpace(motDePasse)) return;

            byte[] salt = CreateSalt(16);
            byte[] hash = HashPBKDF2_64(motDePasse, salt, PBKDF2_ITER, 64);

            using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET PinSaltBin = @s,
    PinHashBin = @h,
    MotDePasse = NULL
WHERE ID_Employe = @id;", con))
            {
                cmd.Parameters.Add("@s", SqlDbType.VarBinary, 16).Value = salt;
                cmd.Parameters.Add("@h", SqlDbType.VarBinary, 64).Value = hash;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmploye;
                cmd.ExecuteNonQuery();
            }
        }

        private int MigrerTousLesComptesClairsVersPinBin(SqlConnection con)
        {
            var list = new System.Collections.Generic.List<(int id, string pwd)>();

            using (var cmd = new SqlCommand(@"
SELECT ID_Employe, MotDePasse
FROM dbo.Employes
WHERE IsActif = 1
  AND MotDePasse IS NOT NULL
  AND LTRIM(RTRIM(MotDePasse)) <> ''
  AND (PinSaltBin IS NULL OR PinHashBin IS NULL);", con))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                    list.Add((Convert.ToInt32(rd["ID_Employe"]), rd["MotDePasse"].ToString()));
            }

            int ok = 0;
            foreach (var it in list)
            {
                MigrerMotDePasseClairVersPinBin(con, it.id, it.pwd);
                ok++;
            }
            return ok;
        }


        private static byte[] DbToBytes(object dbVal)
        {
            if (dbVal == null || dbVal == DBNull.Value) return null;

            // Cas normal: VARBINARY => byte[]
            if (dbVal is byte[] b) return b;

            // Cas: certains anciens enregistrements ou colonnes => string (base64 / hex / 0x)
            if (dbVal is string s)
            {
                s = (s ?? "").Trim();
                if (s.Length == 0) return null;

                // 0xABCD... (hex SQL)
                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    s = s.Substring(2);

                // Hex pur (même longueur, uniquement 0-9A-F)
                bool looksHex = (s.Length % 2 == 0);
                if (looksHex)
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        char c = s[i];
                        bool ok = (c >= '0' && c <= '9') ||
                                  (c >= 'a' && c <= 'f') ||
                                  (c >= 'A' && c <= 'F');
                        if (!ok) { looksHex = false; break; }
                    }
                }

                if (looksHex)
                {
                    var bytes = new byte[s.Length / 2];
                    for (int i = 0; i < bytes.Length; i++)
                        bytes[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
                    return bytes;
                }

                // Base64
                try { return Convert.FromBase64String(s); }
                catch { return null; }
            }

            // Autre type inattendu
            return null;
        }


        private static byte[] HashPBKDF2_64(string password, byte[] salt, int iterations = PBKDF2_ITER, int bytes = 64)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password ?? "", salt, iterations))
                return pbkdf2.GetBytes(bytes);
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private static bool VerifyPBKDF2_64(string password, byte[] salt, byte[] expectedHash)
        {
            if (salt == null || expectedHash == null) return false;
            byte[] test = HashPBKDF2_64(password, salt, PBKDF2_ITER, expectedHash.Length);
            return FixedTimeEquals(test, expectedHash);
        }

        private void MigrerMotDePasseClairVersPin(SqlConnection con, int idEmploye, string motDePasse)
        {
            // ✅ sécurité : si vide on ne migre pas
            if (idEmploye <= 0) return;
            if (string.IsNullOrWhiteSpace(motDePasse)) return;

            // ✅ Salt 16 bytes (ok même si colonne VARBINARY(64))
            byte[] salt = CreateSalt(16);

            // ✅ Hash 64 bytes (PinHash VARBINARY(64))
            byte[] hash = HashPBKDF2_64(motDePasse, salt, 100000, 64);

            using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET PinSalt = @s,
    PinHash = @h,
    MotDePasse = NULL
WHERE ID_Employe = @id;", con))
            {
                cmd.Parameters.Add("@s", SqlDbType.VarBinary, 64).Value = salt; // 16 bytes stockés dans varbinary(64)
                cmd.Parameters.Add("@h", SqlDbType.VarBinary, 64).Value = hash; // 64 bytes
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmploye;
                cmd.ExecuteNonQuery();
            }
        }


        private static bool VerifyPasswordPBKDF2(string password, byte[] salt, byte[] expectedHash)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                byte[] testHash = pbkdf2.GetBytes(expectedHash.Length);

                int diff = 0;
                for (int i = 0; i < testHash.Length; i++)
                    diff |= testHash[i] ^ expectedHash[i];

                return diff == 0;
            }
        }


        private void DrawShadow(Graphics g, Rectangle rect)
        {
            var r = rect;
            r.Inflate(10, 10);

            using (var brush = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
            {
                g.FillRectangle(brush, r);
            }
        }
        private void DragForm(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private Image DrawUserIcon()
        {
            var bmp = new Bitmap(24, 24);

            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var pen = new Pen(Color.FromArgb(220, 255, 245), 2))
                {
                    g.DrawEllipse(pen, 7, 4, 10, 10);
                    g.DrawArc(pen, 5, 11, 14, 12, 20, 140);
                }
            }

            return bmp;
        }

        private Image DrawLockIcon()
        {
            var bmp = new Bitmap(24, 24);

            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var pen = new Pen(Color.FromArgb(220, 255, 245), 2))
                {
                    g.DrawArc(pen, 6, 6, 12, 10, 200, 140);
                    g.DrawRectangle(pen, 6, 11, 12, 10);
                    g.DrawEllipse(pen, 11, 15, 2, 2);
                }
            }

            return bmp;
        }

        // Stubs (si tu les as déjà ailleurs, supprime)
        private void LogOuvrir() { /* ... */ }
        private void LogSortir() { /* ... */ }
    }
}