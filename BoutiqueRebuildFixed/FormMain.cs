using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BoutiqueRebuildFixed;
using static BoutiqueRebuildFixed.ConfigSysteme;
using static BoutiqueRebuildFixed.FormMain;
using static BoutiqueRebuildFixed.FormMain.Pro3DMenuRenderer;
using BoutiqueRebuildFixed.Models;
using BoutiqueRebuildFixed.Security;
using BoutiqueRebuildFixed.Services;

namespace BoutiqueRebuildFixed
{

    public partial class FormMain : FormBase
    {
        public string RoleUtilisateur { get; set; }
        private MenuStrip menuPrincipal;
        private Panel panelInfoUtilisateur;
        private Panel topBar;
        private Label lblAppTitle;
        private StatusStrip status;
        private ToolStripStatusLabel stUser;
        private ToolStripStatusLabel stTheme;
        private ToolStripStatusLabel stTime;
        private Timer timerClock;
        private Panel pnlUserRight;
        private bool _topBarPaintHooked = false;
        private const int USER_RIGHT_MARGIN = 70; // ✅ recule Nom/Rôle vers la gauche
        private int _wheelAccum = 0;
        private int _lastScrollY = -1;
        private ClockControl clockControl;
        private ToolStripControlHost hostClock;
        private ToolStripMenuItem miAccesControle;
        private EventHandler _handlerAccesControle;
        private Panel _lockOverlay;
        private Label _lockLabel;
        private Timer _lockTimer;
        private bool _openingModule = false;
        private TableLayoutPanel _rootLayout;
        private Panel pnlPosMid;
        // ✅ Labels Contexte POS
        private Label lblEntreprise;
        private Label lblMagasin;
        private Label lblCaisse;
        private bool _defaultOpenedOnce = false;
        private bool _menuConstruit = false;
        private DateTime _lastOpenModule = DateTime.MinValue;


        private readonly HashSet<string> _modulesToujoursSignature =
    new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "btnConfigSysteme",
    "btnPermissions",
    "btnPresenceAbsence",
    "btnAuditLog",
    "btnRemisesPromotions",
    "btnAnnulations",
    "btnMarketing",
    "btnComptables",
    "btnSalairesAgents",
    "btnGestionFournisseursAchats",
    "btnGestionImprimantes",
    "btnStatistiquesAvancees",
    "btnRetraitFidelite",
    "btnInventaireScanner",
    "btnAlertesStockExp",
    "btnBonCommande",
    "btnReceptionFournisseur",
    "btnFactureFournisseur",
    "btnPaiementsFournisseur",
    "btnFournisseurs",
    "btnCatalogueFournisseurs",
    "btnPartenaires",
    "btnPromoPartenaires",
    // ajoute ceux que tu veux protéger “toujours”
};

        private void AppliquerThemeMenuPrincipal()
        {
            if (menuPrincipal == null) return;

            bool sombre = string.Equals(ConfigSysteme.Theme, "Sombre", StringComparison.OrdinalIgnoreCase);

            if (sombre)
            {
                menuPrincipal.Renderer = new DarkMenuRenderer();
                menuPrincipal.BackColor = Color.FromArgb(22, 22, 28);
                menuPrincipal.ForeColor = Color.FromArgb(235, 235, 245);
            }
            else
            {
                var r = new Pro3DMenuRenderer();
                menuPrincipal.Renderer = r;
                menuPrincipal.BackColor = r.BaseBack;
                menuPrincipal.ForeColor = r.BaseFore;
            }
        }

        private void OuvrirParDefautSelonRole()
        {
            if (_defaultOpenedOnce) return;
            _defaultOpenedOnce = true;

            // si POS pas configuré => ne pas lancer
            if (AppContext.ModeConfigPOS) return;

            // Non-admin => ouvrir ventes automatiquement
            if (!IsAdminRole())
            {
                // si déjà un form affiché, ne pas écraser
                if (panelContenu != null && panelContenu.Controls.OfType<Form>().Any())
                    return;

                OuvrirFormDansPanel(new FormVentes());
            }
        }
        public class Pro3DMenuRenderer : ToolStripProfessionalRenderer
        {
            public Color BaseBack { get; set; } = Color.FromArgb(245, 247, 252);
            public Color BaseFore { get; set; } = Color.FromArgb(25, 25, 35);
            public Color HoverBack { get; set; } = Color.FromArgb(220, 235, 255);
            public Color PressBack { get; set; } = Color.FromArgb(190, 220, 255);

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (var b = new SolidBrush(BaseBack))
                    e.Graphics.FillRectangle(b, e.AffectedBounds);

                // petite ligne "3D" en bas
                using (var pen = new Pen(Color.FromArgb(180, 180, 190)))
                    e.Graphics.DrawLine(pen, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width, e.ToolStrip.Height - 1);
            }

            public class ClockControl : Control
            {
                public string Format { get; set; } = "dd/MM/yyyy  HH:mm:ss";

                public ClockControl()
                {
                    SetStyle(ControlStyles.AllPaintingInWmPaint |
                             ControlStyles.OptimizedDoubleBuffer |
                             ControlStyles.UserPaint |
                             ControlStyles.ResizeRedraw, true);

                    DoubleBuffered = true;
                    TabStop = false;
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    base.OnPaint(e);

                    // ✅ Fond (évite transparent)
                    using (var b = new SolidBrush(this.BackColor))
                        e.Graphics.FillRectangle(b, this.ClientRectangle);

                    var txt = DateTime.Now.ToString(Format);

                    TextRenderer.DrawText(
                        e.Graphics,
                        txt,
                        this.Font,
                        this.ClientRectangle,
                        this.ForeColor,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding
                    );
                }
            }
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                Rectangle r = new Rectangle(Point.Empty, e.Item.Size);

                Color back = BaseBack;
                if (e.Item.Pressed) back = PressBack;
                else if (e.Item.Selected) back = HoverBack;

                using (var b = new SolidBrush(back))
                    e.Graphics.FillRectangle(b, r);

                // petit contour 3D
                if (e.Item.Selected || e.Item.Pressed)
                {
                    using (var pen = new Pen(Color.FromArgb(160, 160, 180)))
                        e.Graphics.DrawRectangle(pen, 0, 0, r.Width - 1, r.Height - 1);
                }
            }



            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = BaseFore;
                base.OnRenderItemText(e);
            }
        }

        private void ConstruireMenu()
        {
            menuPrincipal = new MenuStrip();

            miAccesControle = new ToolStripMenuItem("Accès contrôle")
            {
                Checked = ConfigSysteme.AccesControleOn,
                CheckOnClick = false
            };
            miAccesControle.Click += miAccesControle_Click;

            menuPrincipal.Items.Add(miAccesControle);

            this.MainMenuStrip = menuPrincipal;
            this.Controls.Add(menuPrincipal);
        }

        public FormMain()
        {
            InitializeComponent();

            // ✅ GARANTIT 1 SEUL MenuStrip (évite le "je clique mais rien ne se passe")
            foreach (var ms in this.Controls.OfType<MenuStrip>().ToList())
                this.Controls.Remove(ms);

            // si le designer a créé un menuStrip1, il est supprimé ici.
            // et on force la reconstruction correcte
            menuPrincipal = null;
            miAccesControle = null;
            _handlerAccesControle = null;

            if (panelContenu != null && !(panelContenu is SmoothScrollPanel))
            {
                var old = panelContenu;

                var p = new SmoothScrollPanel
                {
                    Name = old.Name,
                    Dock = old.Dock,
                    Location = old.Location,
                    Size = old.Size,
                    Padding = old.Padding,
                    Margin = old.Margin,
                    BackColor = old.BackColor,
                    AutoScroll = false // ✅ IMPORTANT : scroll = Form enfant
                };

                while (old.Controls.Count > 0)
                    p.Controls.Add(old.Controls[0]);

                var parent = old.Parent;
                int index = parent.Controls.GetChildIndex(old);

                parent.Controls.Remove(old);
                parent.Controls.Add(p);
                parent.Controls.SetChildIndex(p, index);

                panelContenu = p;     // ✅ OBLIGATOIRE
                old.Dispose();
            }

            this.Load += FormMain_Load;
            

        }

        private bool IsAdminRole()
        {
            return ConfigSysteme.RolesSecurite.EstAdmin(SessionEmploye.Poste);
        }

        private void InitLockedOverlay()
        {
            if (panelContenu == null) return;
            if (_lockOverlay != null) return;

            _lockOverlay = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = Color.FromArgb(140, 0, 0, 0) // noir semi-transparent
            };

            _lockLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White
            };

            _lockOverlay.Controls.Add(_lockLabel);
            panelContenu.Controls.Add(_lockOverlay);
            _lockOverlay.BringToFront();

            _lockTimer = new Timer { Interval = 1800 };
            _lockTimer.Tick += (s, e) =>
            {
                _lockTimer.Stop();
                _lockOverlay.Visible = false;
            };
        }

        private void ShowLockedOverlay(string message)
        {
            InitLockedOverlay();

            if (_lockOverlay == null) return;

            _lockLabel.Text = "🔒 " + message;
            _lockOverlay.Visible = true;
            _lockOverlay.BringToFront();

            _lockTimer.Stop();
            _lockTimer.Start();
        }


        private void HookResizePanelContenu()
        {
            if (panelContenu == null) return;

            // évite double abonnement si le constructeur est rappelé / handle recréé
            panelContenu.Resize -= PanelContenu_Resize;
            panelContenu.Resize += PanelContenu_Resize;
        }

        private void PanelContenu_Resize(object sender, EventArgs e)
        {
            if (panelContenu == null) return;
            var ff = panelContenu.Controls.OfType<Form>().FirstOrDefault();
            if (ff == null) return;

            if (ff.Dock == DockStyle.Fill) return; // ✅ évite le tremblement

            int targetW = panelContenu.ClientSize.Width - 2;
            if (ff.Width != targetW)
                ff.Width = targetW;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            string err;
            if (!ConfigSysteme.TryTestConnexion(out err))
            {
                MessageBox.Show("SQL non connecté.\n\n" + err +
                    "\n\nOuvre : Configuration Système > SQL puis clique 'Tester Connexion' et 'Enregistrer'.",
                    "SQL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ConfigSysteme.InitialiserModulesSiNecessaire(ConfigSysteme.GetModulesFormMain());

            AppliquerDesignProTopBarUser();

            if (AppContext.ModeConfigPOS)
                ShowLockedOverlay("PC non configuré. Ouvre : Fichier > Configuration Poste POS");

            RefreshPosLabels();

            ConfigSysteme.ChargerConfig();
            ConfigSysteme.LoadPrintersConfig();

            // ✅ 1 seul menu
            CreerMenuPrincipal();

            // ✅ toggle accès contrôle INSÉRÉ dans ce même menu
            AjouterToggleAccesControleDansMenu();

            AppliquerThemeMenuPrincipal();
            RafraichirEtatMenusModules();

            AppliquerCouleursBandesPro();
            InitLockedOverlay();

            lblRoleUtilisateur.Text = $"Rôle : {SessionEmploye.Poste}";
            lblCaissier.Text = $"{SessionEmploye.Prenom} {SessionEmploye.Nom}";

            DesactiverTousLesBoutons();

            if (ConfigSysteme.RolesSecurite.EstAdmin(SessionEmploye.Poste))
                ActiverTousLesBoutons();
            else
                AppliquerPermissionsDepuisDB(SessionEmploye.Poste);

            btnPermissions.Visible = ConfigSysteme.RolesSecurite.EstAdmin(SessionEmploye.Poste);

            if (miAccesControle != null)
                miAccesControle.Checked = ConfigSysteme.AccesControleOn;

            AppliquerLangueGlobalePro();

            EnsureRootLayout();
            HookResizePanelContenu();

            RafraichirEtatMenusModules();
            RafraichirTheme();
        }


        private bool HasPermissionInDb(string codeModule)
        {
            if (string.IsNullOrWhiteSpace(codeModule)) return false;

            // Cherche le bouton correspondant (même s'il est déplacé/hors écran)
            var ctrl = FindControlRecursive(this, codeModule);

            if (ctrl is Button b)
                return b.Enabled; // ✅ si DB l'a autorisé, tu as mis btn.Enabled=true

            return false;
        }

        private void AjouterToggleAccesControleDansMenu()
        {
            // menuPrincipal doit exister
            if (menuPrincipal == null || menuPrincipal.Items == null)
                return;

            // SessionEmploye est un TYPE (statique) chez toi
            string role = SessionEmploye.Poste ?? "";

            // Visible uniquement pour managers
            if (!ConfigSysteme.EstRoleManager(role))
                return;

            // Si miAccesControle n'existe pas dans le menu, on l’insère
            if (miAccesControle == null)
                miAccesControle = new ToolStripMenuItem(); // (au cas où tu ne l'as pas instancié ailleurs)

            miAccesControle.Name = "miAccesControle";
            miAccesControle.Text = "Accès contrôlé (Managers)";
            miAccesControle.CheckOnClick = true;

            // éviter double abonnement si la méthode est appelée plusieurs fois
            if (_handlerAccesControle != null)
                miAccesControle.CheckedChanged -= _handlerAccesControle;

            miAccesControle.Checked = ConfigSysteme.AccesControleOn;

            MettreStyleToggleAccesControle();

            _handlerAccesControle = (s, e) =>
            {
                string r = SessionEmploye.Poste ?? "";

                // Seuls managers peuvent changer
                if (!ConfigSysteme.EstRoleManager(r))
                {
                    miAccesControle.CheckedChanged -= _handlerAccesControle;
                    miAccesControle.Checked = ConfigSysteme.AccesControleOn;
                    miAccesControle.CheckedChanged += _handlerAccesControle;
                    return;
                }

                ConfigSysteme.AccesControleOn = miAccesControle.Checked;

                MettreStyleToggleAccesControle();
                RafraichirEtatMenusModules();
            };

            miAccesControle.CheckedChanged += _handlerAccesControle;

            // Insérer seulement si pas déjà dans le menu
            bool dejaDansMenu = menuPrincipal.Items
                .OfType<ToolStripItem>()
                .Any(i => i.Name == "miAccesControle");

            if (!dejaDansMenu)
            {
                int insertIndex = Math.Min(1, menuPrincipal.Items.Count);
                menuPrincipal.Items.Insert(insertIndex, miAccesControle);
            }
        }

        private void NeutraliserPanelBoutons()
        {
            // 1) si tu as déjà assigné panelBoutons ailleurs => OK
            if (panelBoutons == null)
            {
                // 2) tentative: chercher un Panel dont le Name contient "bouton"
                panelBoutons = this.Controls
                    .OfType<Control>()
                    .SelectMany(c => GetAllControls(c))
                    .OfType<Panel>()
                    .FirstOrDefault(p => p.Name != null &&
                                         p.Name.IndexOf("bouton", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (panelBoutons == null) return;

            // ✅ le rendre "inexistant"
            panelBoutons.Visible = false;
            panelBoutons.Enabled = false;
            panelBoutons.Dock = DockStyle.None;
            panelBoutons.Width = 0;
            panelBoutons.Height = 0;
            panelBoutons.Location = new Point(-5000, -5000);
        }

        private bool DemanderSignatureBoss(string permissionCode, string titreAction)
        {
            using (var sig = new FrmSignatureManager(
                connectionString: ConfigSysteme.ConnectionString,
                typeAction: titreAction,
                permissionCode: permissionCode,
                reference: permissionCode,
                details: $"Accès demandé par {SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste})",
                idEmployeDemandeur: SessionEmploye.ID_Employe,
                roleDemandeur: SessionEmploye.Poste
            ))
            {
                var dr = sig.ShowDialog(this);
                return (dr == DialogResult.OK && sig.Approved);
            }
        }

        private IEnumerable<Control> GetAllControls(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (var child in GetAllControls(c))
                    yield return child;
            }
        }


        private void RafraichirEtatMenusModules()
        {
            if (menuPrincipal == null) return;

            foreach (ToolStripItem it in menuPrincipal.Items)
            {
                if (it is ToolStripMenuItem mi)
                    RafraichirEtatMenuRecursive(mi);
            }
        }

        private void RafraichirEtatMenuRecursive(ToolStripMenuItem mi)
        {
            if (mi == null) return;
            if (mi == miAccesControle) return;

            if (mi.Tag is Tuple<string, Func<Form>> t)
                AppliquerEtatMenuModule(mi, t.Item1, t.Item2);

            foreach (ToolStripItem sub in mi.DropDownItems)
            {
                if (sub is ToolStripMenuItem subMi)
                    RafraichirEtatMenuRecursive(subMi);
            }
        }

        private void MettreStyleToggleAccesControle()
        {
            if (miAccesControle == null) return;
            miAccesControle.Text = ConfigSysteme.AccesControleOn
                ? "🟢 Accès contrôlé (ON)"
                : "🔴 Accès contrôlé (OFF)";
        }

        private void BrancherClickMenuModule(ToolStripMenuItem mi)
        {
            if (mi == null) return;
            mi.Click -= MenuModule_Click;   // enlève toujours avant (anti doublon)
            mi.Click += MenuModule_Click;
        }

        private void StartScrollDebug()
        {
            var t = new Timer { Interval = 200 };
            t.Tick += (s, e) =>
            {
                int y = -panelContenu.AutoScrollPosition.Y;
                if (y != _lastScrollY)
                {
                    _lastScrollY = y;
                    System.Diagnostics.Debug.WriteLine($"[SCROLL] Y={y}  time={DateTime.Now:HH:mm:ss.fff}");
                }
            };
            t.Start();
        }

        private static string Prompt(string text, string caption)
        {
            using (Form f = new Form())
            {
                f.Text = caption;
                f.Width = 420;
                f.Height = 160;
                f.StartPosition = FormStartPosition.CenterParent;

                Label lbl = new Label { Left = 10, Top = 10, Width = 380, Text = text };
                TextBox tb = new TextBox { Left = 10, Top = 35, Width = 380 };
                Button ok = new Button { Text = "OK", Left = 230, Width = 80, Top = 70, DialogResult = DialogResult.OK };
                Button cancel = new Button { Text = "Annuler", Left = 310, Width = 80, Top = 70, DialogResult = DialogResult.Cancel };

                f.Controls.Add(lbl);
                f.Controls.Add(tb);
                f.Controls.Add(ok);
                f.Controls.Add(cancel);
                f.AcceptButton = ok;
                f.CancelButton = cancel;

                return f.ShowDialog() == DialogResult.OK ? tb.Text : null;
            }
        }

        private void AppliquerDesignProTopBarUser()
        {
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // ===== TOPBAR =====
            if (topBar == null)
            {
                topBar = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 32,
                    Padding = new Padding(10, 4, 10, 4)
                };

                // ✅ Titre à gauche
                lblAppTitle = new Label
                {
                    AutoSize = true,
                    Text = "ZAIRE POS",
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    Dock = DockStyle.Left,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(0, 2, 14, 0) // espace après le titre
                };

                // ✅ Panel du milieu (zone restante après ZAIRE POS)
                pnlPosMid = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0, 2, 10, 0),
                    BackColor = Color.Transparent
                };

                // ✅ Labels POS (petits)
                if (lblEntreprise == null) lblEntreprise = new Label { Name = "lblEntreprise" };
                if (lblMagasin == null) lblMagasin = new Label { Name = "lblMagasin" };
                if (lblCaisse == null) lblCaisse = new Label { Name = "lblCaisse" };

                // Entreprise en gras + plus grand
                lblEntreprise.AutoSize = true;
                lblEntreprise.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                lblEntreprise.ForeColor = Color.FromArgb(20, 40, 70);
                lblEntreprise.Margin = new Padding(0, 2, 0, 0);

                // Magasin normal
                lblMagasin.AutoSize = true;
                lblMagasin.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
                lblMagasin.ForeColor = Color.FromArgb(40, 40, 60);
                lblMagasin.Margin = new Padding(0, 2, 0, 0);

                // Caisse normal
                lblCaisse.AutoSize = true;
                lblCaisse.Font = new Font("Segoe UI", 9.0F, FontStyle.Bold);
                lblCaisse.ForeColor = Color.FromArgb(40, 40, 60);
                lblCaisse.Margin = new Padding(0, 2, 0, 0);

                // ✅ Les mettre en ligne dans le panel milieu
                var flowPos = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoScroll = false,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty,
                    BackColor = Color.Transparent
                };

                flowPos.Controls.Add(lblEntreprise);
                flowPos.Controls.Add(new Label { AutoSize = true, Text = "   |   ", Font = lblEntreprise.Font, ForeColor = lblEntreprise.ForeColor });
                flowPos.Controls.Add(lblMagasin);
                flowPos.Controls.Add(new Label { AutoSize = true, Text = "   |   ", Font = lblEntreprise.Font, ForeColor = lblEntreprise.ForeColor });
                flowPos.Controls.Add(lblCaisse);

                pnlPosMid.Controls.Add(flowPos);

                // ✅ Panel droit (Caissier / Rôle) -> réduit le blanc
                pnlUserRight = new Panel
                {
                    Dock = DockStyle.Right,
                    Width = 520,                 // ✅ un peu plus large
                    Padding = new Padding(0, 0, 40, 0) // ✅ plus de marge droite => tout part à gauche
                };

                // ✅ RÔLE
                lblRoleUtilisateur.AutoSize = true;
                lblRoleUtilisateur.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                lblRoleUtilisateur.TextAlign = ContentAlignment.MiddleRight;

                // ✅ NOM
                lblCaissier.AutoSize = false;
                lblCaissier.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                lblCaissier.TextAlign = ContentAlignment.MiddleRight;
                lblCaissier.AutoEllipsis = true;
                lblCaissier.Height = 20;

                pnlUserRight.Controls.Add(lblCaissier);
                pnlUserRight.Controls.Add(lblRoleUtilisateur);

                pnlUserRight.Resize += (s, e) =>
                {
                    int leftMargin = 10;
                    int gap = 14;
                    int USER_RIGHT_MARGIN_LOCAL = 80;

                    lblRoleUtilisateur.AutoSize = true;
                    lblCaissier.AutoSize = false;
                    lblCaissier.AutoEllipsis = true;
                    lblCaissier.Height = lblRoleUtilisateur.PreferredHeight;

                    int yRole = (pnlUserRight.Height - lblRoleUtilisateur.PreferredHeight) / 2;
                    int yName = (pnlUserRight.Height - lblCaissier.Height) / 2;

                    int roleW = lblRoleUtilisateur.PreferredWidth;
                    int roleX = pnlUserRight.Width - roleW - USER_RIGHT_MARGIN_LOCAL;
                    roleX = Math.Max(leftMargin + 120, roleX);

                    lblRoleUtilisateur.Location = new Point(roleX, yRole);

                    int nameX = leftMargin;
                    int nameW = lblRoleUtilisateur.Left - gap - leftMargin;
                    if (nameW < 80) nameW = 80;

                    lblCaissier.Location = new Point(nameX, yName);
                    lblCaissier.Width = nameW;

                    lblRoleUtilisateur.Visible = true;
                };

                // ✅ ordre dock : Right, Fill, Left
                topBar.Controls.Add(pnlUserRight);
                topBar.Controls.Add(pnlPosMid);
                topBar.Controls.Add(lblAppTitle);

                this.Controls.Add(topBar);
            }

            // ⚠️ IMPORTANT: ne pas accrocher Paint à chaque appel
            if (!_topBarPaintHooked)
            {
                _topBarPaintHooked = true;

                topBar.Paint += (s, e) =>
                {
                    var g = e.Graphics;
                    var rect = topBar.ClientRectangle;

                    using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(
                        rect,
                        Color.FromArgb(235, 250, 246),
                        Color.FromArgb(210, 238, 232),
                        90f))
                    {
                        g.FillRectangle(br, rect);
                    }

                    using (var pen = new Pen(Color.FromArgb(170, 170, 180)))
                        g.DrawLine(pen, 0, rect.Height - 1, rect.Width, rect.Height - 1);
                };
            }

            // ===== MENUSTRIP =====
            if (menuPrincipal != null)
            {
                menuPrincipal.Dock = DockStyle.Top;
                menuPrincipal.AutoSize = false;
                menuPrincipal.Height = 28;
                menuPrincipal.Padding = new Padding(6, 2, 6, 2);
                menuPrincipal.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }

            // ===== STATUS =====
            if (status == null)
{
    status = new StatusStrip
    {
        Dock = DockStyle.Bottom,
        SizingGrip = false,
        Height = 24
    };

    stUser = new ToolStripStatusLabel();
    stTheme = new ToolStripStatusLabel();

                // ✅ Clock en Label (pas ToolStripStatusLabel)
                clockControl = new ClockControl
                {
                    Width = 190,
                    Height = 18,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(90, 90, 110),
                    BackColor = status.BackColor // ✅ PAS Transparent
                };

                hostClock = new ToolStripControlHost(clockControl)
                {
                    AutoSize = false,
                    Width = 190,
                    Height = 18,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty
                };

                status.SuspendLayout();

    status.Items.Add(stUser);
    status.Items.Add(new ToolStripStatusLabel("  |  "));
    status.Items.Add(stTheme);

    // ✅ “tampon” stable pour pousser l’heure à droite
    status.Items.Add(new ToolStripStatusLabel { Spring = true });

    status.Items.Add(hostClock);

    status.ResumeLayout(true);

    this.Controls.Add(status);
}

            StartClock();

                // ===== CONTENU =====
                if (panelContenu != null)
                {
                    panelContenu.Dock = DockStyle.Fill;
                    panelContenu.Padding = new Padding(10, 10, 0, 10);

                    panelContenu.AutoScroll = false; // ✅ IMPORTANT : scroll = Form enfant
                }

            // ✅ Corrige définitivement l’ordre des Dock (contenu jamais derrière menu)
        }


        private int GetIdVenteSelectionneeOuCourante()
        {
            var ventes = panelContenu?.Controls.OfType<FormVentes>().FirstOrDefault();
            return ventes?.CurrentIdVente ?? 0;
        }

        private void StartClock()
        {
            if (clockControl == null) return;
            if (timerClock != null) return;

            timerClock = new Timer { Interval = 1000 };
            timerClock.Tick += (s, e) =>
            {
                if (IsDisposed || clockControl == null) return;
                clockControl.Invalidate(); // ✅ pas de Text => pas de Layout => pas de jitter
            };
            timerClock.Start();
        }

        private void AppliquerLangueGlobalePro()
        {
            // FormMain
            ConfigSysteme.AppliquerTraductions(this);

            // MenuStrip
            if (menuPrincipal != null)
                ConfigSysteme.AppliquerTraductions(menuPrincipal);

            // IMPORTANT: traduire aussi les ToolStripMenuItem ajoutés avec texte "Ventes", etc.
            // => donne-leur un Name stable = CodeModule, sinon ça ne peut pas être traduit proprement
            // (on corrige ça dans AjouterMenuDepuisBouton ci-dessous)

            // Form enfant dans panel
            foreach (Control c in panelContenu.Controls)
            {
                if (c is Form f)
                {
                    ConfigSysteme.AppliquerTraductions(f);
                    ConfigSysteme.AppliquerMenuContextuel(f);
                }
            }
        }

        private void Form_ControlAdded(object sender, ControlEventArgs e)
        {
            if (e?.Control == null) return;

            // 🔒 IMPORTANT : NE JAMAIS appliquer le thème contrôle par contrôle
            // sinon ClairDesign sera cassé
            if (!string.Equals(ConfigSysteme.Theme, "ClairDesign", StringComparison.OrdinalIgnoreCase))
            {
                ConfigSysteme.AppliquerTheme(e.Control);
            }

            ConfigSysteme.AppliquerTraductions(e.Control);
            ConfigSysteme.AppliquerMenuContextuel(e.Control);
        }

        private Panel panelBoutons; // <-- si tu as ce panel dans le designer, lie-le à ce champ

        private void FixDockOrder()
        {
            if (IsDisposed) return;
            if (panelContenu == null || menuPrincipal == null || status == null || topBar == null) return;

            NeutraliserPanelBoutons();

            SuspendLayout();

            topBar.Dock = DockStyle.Top;
            menuPrincipal.Dock = DockStyle.Top;
            status.Dock = DockStyle.Bottom;
            panelContenu.Dock = DockStyle.Fill;

            EnsureOnForm(panelContenu);
            EnsureOnForm(status);
            EnsureOnForm(menuPrincipal);
            EnsureOnForm(topBar);

            // ✅ Retirer puis ré-ajouter dans l'ordre DOCK correct (garanti)
            Controls.Remove(panelContenu);
            Controls.Remove(status);
            Controls.Remove(menuPrincipal);
            Controls.Remove(topBar);

            // 1) FILL (au fond)
            Controls.Add(panelContenu);
            // 2) BOTTOM
            Controls.Add(status);
            // 3) TOP (Menu)
            Controls.Add(menuPrincipal);
            // 4) TOP (TopBar au dessus)
            Controls.Add(topBar);

            topBar.BringToFront();
            menuPrincipal.BringToFront();

            ResumeLayout(true);
        }

        private void BuildRootLayout()
        {
            if (_rootLayout != null) return;
            if (topBar == null || menuPrincipal == null || panelContenu == null || status == null) return;

            NeutraliserDockFillParasites(); // ✅ important

            _rootLayout = new TableLayoutPanel
            {
                Name = "rootLayout",
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));  // topBar
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // menu
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // contenu
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));  // status

            // détache
            topBar.Parent?.Controls.Remove(topBar);
            menuPrincipal.Parent?.Controls.Remove(menuPrincipal);
            panelContenu.Parent?.Controls.Remove(panelContenu);
            status.Parent?.Controls.Remove(status);

            // dock fill dans les cellules
            topBar.Dock = DockStyle.Fill; topBar.Margin = Padding.Empty;
            menuPrincipal.Dock = DockStyle.Fill; menuPrincipal.Margin = Padding.Empty;
            panelContenu.Dock = DockStyle.Fill; panelContenu.Margin = Padding.Empty;
            status.Dock = DockStyle.Fill; status.Margin = Padding.Empty;

            panelContenu.Padding = new Padding(10, 10, 10, 10);

            _rootLayout.Controls.Add(topBar, 0, 0);
            _rootLayout.Controls.Add(menuPrincipal, 0, 1);
            _rootLayout.Controls.Add(panelContenu, 0, 2);
            _rootLayout.Controls.Add(status, 0, 3);

            // Ajout root (et seulement root) au Form
            this.Controls.Add(_rootLayout);
            _rootLayout.BringToFront();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            EnsureRootLayout();
        }


        private void EnsureOnForm(Control c)
        {
            if (c == null) return;

            // Si déjà enfant direct du Form => OK
            if (c.Parent == this) return;

            // Si a un autre parent => on le détache
            c.Parent?.Controls.Remove(c);

            // Ajout au Form
            if (!this.Controls.Contains(c))
                this.Controls.Add(c);
        }

        private void EnsureRootLayout()
        {
            if (IsDisposed) return;
            if (_rootLayout != null) return;
            if (topBar == null || menuPrincipal == null || panelContenu == null || status == null) return;

            // Désactive les parasites Dock=Fill
            NeutraliserPanelBoutons();
            NeutraliserDockFillParasites();

            // Crée la grille racine
            _rootLayout = new TableLayoutPanel
            {
                Name = "rootLayout",
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));  // TopBar
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));  // Menu
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // Contenu
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));  // Status

            // Détacher des anciens parents (important)
            if (topBar.Parent != null) topBar.Parent.Controls.Remove(topBar);
            if (menuPrincipal.Parent != null) menuPrincipal.Parent.Controls.Remove(menuPrincipal);
            if (panelContenu.Parent != null) panelContenu.Parent.Controls.Remove(panelContenu);
            if (status.Parent != null) status.Parent.Controls.Remove(status);

            // Dans TableLayoutPanel on met Dock=Fill partout
            topBar.Dock = DockStyle.Fill;
            menuPrincipal.Dock = DockStyle.Fill;
            panelContenu.Dock = DockStyle.Fill;
            status.Dock = DockStyle.Fill;

            topBar.Margin = Padding.Empty;
            menuPrincipal.Margin = Padding.Empty;
            panelContenu.Margin = Padding.Empty;
            status.Margin = Padding.Empty;

            // ✅ Contenu : padding propre
            panelContenu.Padding = new Padding(10, 10, 10, 10);
            panelContenu.AutoScroll = false; // scroll sur Form enfant, pas sur panel

            // Ajouter dans la grille
            _rootLayout.Controls.Add(topBar, 0, 0);
            _rootLayout.Controls.Add(menuPrincipal, 0, 1);
            _rootLayout.Controls.Add(panelContenu, 0, 2);
            _rootLayout.Controls.Add(status, 0, 3);

            // Retirer tous les contrôles dockés qui peuvent gêner (optionnel mais safe)
            // (On laisse si tu as d'autres contrôles essentiels)
            // this.Controls.Clear();  <-- évite si tu as d'autres panels importants

            // Ajout root au Form
            this.Controls.Add(_rootLayout);
            _rootLayout.BringToFront();

            // Sécurité : garder l’ordre interne
            topBar.BringToFront();
            menuPrincipal.BringToFront();
        }


        private void DebugParents()
        {
            System.Diagnostics.Debug.WriteLine($"panelContenu parent={panelContenu?.Parent?.Name}");
            System.Diagnostics.Debug.WriteLine($"topBar parent={topBar?.Parent?.Name}");
            System.Diagnostics.Debug.WriteLine($"menuPrincipal parent={menuPrincipal?.Parent?.Name}");
            System.Diagnostics.Debug.WriteLine($"status parent={status?.Parent?.Name}");
        }

        private void AppliquerDesignProCompact()
        {
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // ===== TOPBAR (compact) =====
            if (topBar == null)
            {
                topBar = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 38,                      // ✅ petit
                    Padding = new Padding(12, 6, 12, 6)
                };

                lblAppTitle = new Label
                {
                    AutoSize = true,
                    Text = "ZAIRE POS",
                    Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),   // ✅ plus petit
                    Location = new Point(12, 8)
                };

                topBar.Controls.Add(lblAppTitle);
                this.Controls.Add(topBar);
            }

            // ===== MENUSTRIP =====
            if (menuPrincipal != null)
            {
                menuPrincipal.Dock = DockStyle.Top;
                menuPrincipal.AutoSize = false;
                menuPrincipal.Height = 32;          // ✅ compact
                menuPrincipal.Padding = new Padding(8, 3, 8, 3);
                menuPrincipal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

                // Remet dans l’ordre
                this.Controls.Remove(menuPrincipal);
                this.Controls.Add(menuPrincipal);
            }

            // ===== USER BAR =====
            if (panelInfoUtilisateur != null)
            {
                panelInfoUtilisateur.Dock = DockStyle.Top;
                panelInfoUtilisateur.Height = 40;   // ✅ compact
                panelInfoUtilisateur.Padding = new Padding(10, 0, 10, 0);

                lblRoleUtilisateur.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
                lblCaissier.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);

                lblRoleUtilisateur.AutoSize = false;
                lblCaissier.AutoSize = false;

                lblRoleUtilisateur.Width = 320;
                lblCaissier.Width = 520;

                lblRoleUtilisateur.Location = new Point(12, 0);
                lblCaissier.Location = new Point(340, 0);

                lblRoleUtilisateur.Height = panelInfoUtilisateur.Height;
                lblCaissier.Height = panelInfoUtilisateur.Height;

                lblRoleUtilisateur.TextAlign = ContentAlignment.MiddleLeft;
                lblCaissier.TextAlign = ContentAlignment.MiddleLeft;

                this.Controls.Remove(panelInfoUtilisateur);
                this.Controls.Add(panelInfoUtilisateur);
            }

            

            if (timerClock == null)
            {
                timerClock = new Timer { Interval = 1000 };
                timerClock.Tick += (s, e) => stTime.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
                timerClock.Start();
            }

            stUser.Text = "Utilisateur : " + SessionEmploye.Prenom + " " + SessionEmploye.Nom;
            stTheme.Text = "Thème : " + (ConfigSysteme.Theme ?? "Clair");

            // ✅ IMPORTANT : Panel contenu doit rester FILL APRÈS tout le reste
            if (panelContenu != null)
            {
                panelContenu.Dock = DockStyle.Fill;
                panelContenu.Padding = new Padding(14);
            }

            // ✅ Ordre final (Top -> Menu -> UserBar -> Contenu -> Status)
            if (menuPrincipal != null) 
            if (panelInfoUtilisateur != null) panelInfoUtilisateur.BringToFront();
            if (status != null) status.SendToBack(); // bottom
        }


        private void AppliquerCouleursBandesPro()
        {
            bool sombre = string.Equals(ConfigSysteme.Theme, "Sombre", StringComparison.OrdinalIgnoreCase);
            bool clairDesign = string.Equals(ConfigSysteme.Theme, "ClairDesign", StringComparison.OrdinalIgnoreCase);

            if (clairDesign) return;

            if (sombre)
            {
                // Bande 1: TopBar (teal foncé)
                Color top = Color.FromArgb(10, 70, 60);
                // Bande 2: Menu (plus sombre)
                Color menu = Color.FromArgb(18, 18, 22);
                // Bande 3: Contenu (très sombre)
                Color content = Color.FromArgb(20, 20, 26);
                // Bande bas: Status
                Color statusBg = Color.FromArgb(24, 24, 30);

                this.BackColor = content;

                if (topBar != null) topBar.BackColor = top;
                if (lblAppTitle != null) lblAppTitle.ForeColor = Color.White;

                lblCaissier.ForeColor = Color.FromArgb(230, 255, 245);
                lblRoleUtilisateur.ForeColor = Color.FromArgb(180, 255, 235);

                if (menuPrincipal != null)
                {
                    menuPrincipal.BackColor = menu;
                    menuPrincipal.ForeColor = Color.White;
                }

                if (panelContenu != null) panelContenu.BackColor = content;

                if (status != null)
                {
                    status.BackColor = statusBg;
                    status.ForeColor = Color.FromArgb(200, 200, 210);
                }
            }
            else
            {
                // Bande 1: TopBar (bleu/teal clair)
                Color top = Color.FromArgb(230, 248, 244);
                // Bande 2: Menu (blanc cassé)
                Color menu = Color.FromArgb(245, 247, 252);
                // Bande 3: Contenu (blanc)
                Color content = Color.White;
                // Bande bas: Status (gris très léger)
                Color statusBg = Color.FromArgb(245, 247, 252);

                this.BackColor = menu;

                if (topBar != null) topBar.BackColor = top;
                if (lblAppTitle != null) lblAppTitle.ForeColor = Color.FromArgb(18, 60, 55);

                lblCaissier.ForeColor = Color.FromArgb(20, 80, 140);
                lblRoleUtilisateur.ForeColor = Color.FromArgb(18, 140, 110);

                if (menuPrincipal != null)
                {
                    menuPrincipal.BackColor = menu;
                    menuPrincipal.ForeColor = Color.FromArgb(25, 25, 35);
                }

                if (panelContenu != null) panelContenu.BackColor = content;

                if (status != null)
                {
                    status.BackColor = statusBg;
                    status.ForeColor = Color.FromArgb(90, 90, 110);
                }
            }

            // Menu items : éviter incohérences
            AppliquerThemeMenuPrincipal();
        }

        private async Task<bool> AutoriserAdminParEmpreinte(string message)
        {
            if (!ConfigSysteme.RolesSecurite.EstAdmin(SessionEmploye.Poste))
            {
                MessageBox.Show("Accès interdit.");
                return false;
            }

            bool ok = await ConfigSysteme.DemanderEmpreinteSiBesoinAsync(message);
            if (!ok)
            {
                MessageBox.Show("Accès refusé (empreinte non validée).");
                return false;
            }

            return true;
        }


        private ToolStripMenuItem AjouterMenuFichierSecurise(
    string codeModule,
    Button bouton,
    string titreAction,
    Func<Form> factory
)
        {
            var item = new ToolStripMenuItem
            {
                Name = codeModule,
                Text = ConfigSysteme.Traduire(codeModule),
                Enabled = bouton?.Enabled ?? false
            };

            // ✅ Tag + style uniquement
            AppliquerEtatMenuModule(item, codeModule, factory);

            // ✅ CLICK attach UNE SEULE FOIS (anti doublon)
            BrancherClickMenuModule(item);

            // ✅ synchronise le menu si le bouton change
            if (bouton != null)
            {
                bouton.EnabledChanged += (s, e) =>
                {
                    item.Enabled = bouton.Enabled;
                    AppliquerEtatMenuModule(item, codeModule, factory);
                };
            }

            return item;
        }

        private void FixerCouleursSousMenus(ToolStripMenuItem mi, Color back, Color fore)
        {
            foreach (ToolStripItem sub in mi.DropDownItems)
            {
                sub.BackColor = back;
                sub.ForeColor = fore;

                if (sub is ToolStripMenuItem subMi)
                    FixerCouleursSousMenus(subMi, back, fore);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // ✅ Hook events (attention : OnHandleCreated peut être appelé plusieurs fois si handle recréé)
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
            if (timerClock != null)
            {
                timerClock.Stop();
                timerClock.Dispose();
                timerClock = null;
            }
        }

        private void RafraichirLangue()
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RafraichirLangue));
                return;
            }

            // ✅ Form + tous les contrôles (dont boutons cachés)
            ConfigSysteme.AppliquerTraductions(this);

            // ✅ Menu principal
            if (menuPrincipal != null)
                ConfigSysteme.AppliquerTraductions(menuPrincipal);

            // ✅ Form enfant
            foreach (Control c in panelContenu.Controls)
                if (c is Form f)
                    ConfigSysteme.AppliquerTraductions(f);
        }

        private void RafraichirTheme()
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RafraichirTheme));
                return;
            }

            // ✅ Appliquer thème global (ClairDesign => RESTORE via snapshot)
            ConfigSysteme.AppliquerTheme(this);

            // ✅ Form enfant dans panel
            if (panelContenu != null)
            {
                foreach (Control c in panelContenu.Controls)
                    if (c is Form f)
                        ConfigSysteme.AppliquerTheme(f);
            }

            // ✅ Labels Role/Caissier
            if (!string.Equals(ConfigSysteme.Theme, "ClairDesign", StringComparison.OrdinalIgnoreCase))
            {
                lblRoleUtilisateur.ForeColor = Color.DarkBlue;
                lblCaissier.ForeColor = Color.DarkBlue;
            }
            else
            {
                lblRoleUtilisateur.Refresh();
                lblCaissier.Refresh();
            }

            // ✅ Menu principal
            AppliquerThemeMenuPrincipal();

            // ✅ Couleurs bandes (peut modifier status.BackColor)
            AppliquerCouleursBandesPro();

            // ✅ Panel user : sombre = dark, sinon = normal windows
            if (panelInfoUtilisateur != null)
            {
                panelInfoUtilisateur.BackColor =
                    string.Equals(ConfigSysteme.Theme, "Sombre", StringComparison.OrdinalIgnoreCase)
                    ? Color.FromArgb(45, 45, 48)
                    : SystemColors.Control;
            }

            // ✅ IMPORTANT : synchroniser l’horloge avec le StatusStrip (SANS Transparent)
            if (clockControl != null && status != null)
            {
                clockControl.BackColor = status.BackColor;
                clockControl.ForeColor = status.ForeColor;
                clockControl.Invalidate();   // repaint seulement (pas de layout)
            }
        }

        private void AppliquerStylesPro()
        {
            bool sombre = string.Equals(ConfigSysteme.Theme, "Sombre", StringComparison.OrdinalIgnoreCase);
            bool clairDesign = string.Equals(ConfigSysteme.Theme, "ClairDesign", StringComparison.OrdinalIgnoreCase);

            // Si ClairDesign = tu laisses ton moteur restaurer le Designer (on ne force pas trop)
            if (clairDesign) return;

            // Couleurs pro
            Color bg = sombre ? Color.FromArgb(18, 18, 22) : Color.FromArgb(245, 246, 250);
            Color card = sombre ? Color.FromArgb(24, 24, 30) : Color.White;
            Color text = sombre ? Color.White : Color.FromArgb(25, 25, 35);
            Color subText = sombre ? Color.FromArgb(190, 190, 200) : Color.FromArgb(90, 90, 110);
            Color accent = Color.FromArgb(18, 140, 110);

            this.BackColor = bg;

            if (topBar != null) topBar.BackColor = card;
            if (lblAppTitle != null) lblAppTitle.ForeColor = text;

            if (menuPrincipal != null)
            {
                menuPrincipal.BackColor = card;
                menuPrincipal.ForeColor = text;
            }

            if (panelInfoUtilisateur != null)
            {
                panelInfoUtilisateur.BackColor = card;
                lblRoleUtilisateur.ForeColor = accent;
                lblCaissier.ForeColor = accent;
            }

            if (panelContenu != null)
            {
                panelContenu.BackColor = sombre ? Color.FromArgb(20, 20, 26) : Color.White;
            }

            if (status != null)
            {
                status.BackColor = card;
                status.ForeColor = subText;
            }

            // ✅ menu items : éviter texte blanc sur fond blanc
            AppliquerThemeMenuPrincipal();
        }


        private void InitialiserInfoUtilisateurPanel()
        {
            if (panelInfoUtilisateur == null)
            {
                panelInfoUtilisateur = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 40,
                    BackColor = SystemColors.Control   // ✅ neutre (Clair / ClairDesign)
                };

                this.Controls.Add(panelInfoUtilisateur);
                panelInfoUtilisateur.BringToFront();
            }

            // =========================
            // Label Role
            // =========================
            lblRoleUtilisateur.Dock = DockStyle.None;
            lblRoleUtilisateur.AutoSize = false;
            lblRoleUtilisateur.Width = 250;
            lblRoleUtilisateur.Height = panelInfoUtilisateur.Height;
            lblRoleUtilisateur.Location = new Point(0, 0);

            lblRoleUtilisateur.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            lblRoleUtilisateur.TextAlign = ContentAlignment.MiddleLeft;
            lblRoleUtilisateur.Padding = new Padding(15, 0, 0, 0);

            // ⚠️ NE PAS forcer la couleur en ClairDesign
            if (!string.Equals(ConfigSysteme.Theme, "ClairDesign", StringComparison.OrdinalIgnoreCase))
            {
                lblRoleUtilisateur.ForeColor = Color.DarkBlue;
            }

            // =========================
            // Label Caissier
            // =========================
            lblCaissier.Dock = DockStyle.None;
            lblCaissier.AutoSize = false;
            lblCaissier.Width = 350;
            lblCaissier.Height = panelInfoUtilisateur.Height;
            lblCaissier.Location = new Point(lblRoleUtilisateur.Right, 0);

            lblCaissier.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            lblCaissier.TextAlign = ContentAlignment.MiddleLeft;
            lblCaissier.Padding = new Padding(15, 0, 0, 0);

            // ⚠️ NE PAS forcer la couleur en ClairDesign
            if (!string.Equals(ConfigSysteme.Theme, "ClairDesign", StringComparison.OrdinalIgnoreCase))
            {
                lblCaissier.ForeColor = Color.DarkBlue;
            }

            // Ajoute les labels si pas déjà présents
            if (!panelInfoUtilisateur.Controls.Contains(lblRoleUtilisateur))
                panelInfoUtilisateur.Controls.Add(lblRoleUtilisateur);

            if (!panelInfoUtilisateur.Controls.Contains(lblCaissier))
                panelInfoUtilisateur.Controls.Add(lblCaissier);

            // ✅ Appliquer le thème au panel immédiatement (surtout au lancement)
            RafraichirTheme();
        }

        private void OuvrirDialogue(Form f, bool modal = true)
        {
            if (f == null) return;

            // Optionnel : si tu veux toujours au centre
            f.StartPosition = FormStartPosition.CenterParent;

            // Optionnel : éviter multi-instances du même formulaire (si tu veux)
            // f.ShowInTaskbar = false;

            if (modal)
            {
                // Fenêtre modale
                f.ShowDialog(this);
            }
            else
            {
                // Fenêtre indépendante
                f.Show(this);
                f.BringToFront();
                f.Activate();
            }
        }


        private void CreerMenuPrincipal()
        {
            if (_menuConstruit) return;      // ✅ anti double build
            if (menuPrincipal != null) return;

            menuPrincipal = new MenuStrip
            {
                Dock = DockStyle.Top,
                RenderMode = ToolStripRenderMode.Professional
            };

            // ✅ NON-ADMIN : 3 menus max (À propos, Ventes, Fichier)
            if (!IsAdminRole())
            {
                BuildMenuNonAdmin();
                _menuConstruit = true;

                this.MainMenuStrip = menuPrincipal;
                if (menuPrincipal.Parent == null)
                    this.Controls.Add(menuPrincipal);

                AppliquerThemeMenuPrincipal();
                RafraichirEtatMenusModules();
                ConfigSysteme.AppliquerTraductions(menuPrincipal);

                return;
            }

            // ✅ ADMIN : menu complet (BOSS + À propos + Fichier + tous modules)
            BuildMenuAdminComplet();
            _menuConstruit = true;

            this.MainMenuStrip = menuPrincipal;
            if (menuPrincipal.Parent == null)
                this.Controls.Add(menuPrincipal);

            CacherBoutonsTransformesEnMenus();
            AppliquerThemeMenuPrincipal();
            RafraichirEtatMenusModules();
            ConfigSysteme.AppliquerTraductions(menuPrincipal);
        }


        // ✅ Helper : création item top-level simple
        private ToolStripMenuItem NewTopItem(string name, string text, Action onClick)
        {
            var mi = new ToolStripMenuItem
            {
                Name = name,
                Text = text
            };
            if (onClick != null)
                mi.Click += (s, e) => onClick();

            return mi;
        }


        // ✅ Helper : item module sécurisé (utilise TON système Tag + OuvrirModule)
        private ToolStripMenuItem NewSecureModuleItem(string codeModule, string text, Func<Form> factory)
        {
            var item = new ToolStripMenuItem
            {
                Name = codeModule,
                Text = text
            };

            // ✅ Tag standard (comme chez toi)
            item.Tag = new Tuple<string, Func<Form>>(codeModule, factory);

            // ✅ état / couleurs (ON/OFF + permissions)
            AppliquerEtatMenuModule(item, codeModule, factory);

            // ✅ click central
            BrancherClickMenuModule(item);

            return item;
        }

        private void BuildMenuNonAdmin()
        {
            menuPrincipal.Items.Clear();

            // 1) À propos (top-left)
            var miAProposTop = NewTopItem("miAPropos", "ℹ À propos", () =>
            {
                using (var f = new FrmAPropos())
                {
                    f.StartPosition = FormStartPosition.CenterParent;
                    f.ShowDialog(this);
                }
            });
            menuPrincipal.Items.Add(miAProposTop);

            // 2) Ventes (top-level)
            // IMPORTANT: Name = btnVentes (pour garder compatibilité permissions)
            var miVentesTop = new ToolStripMenuItem
            {
                Name = "btnVentes",
                Text = "🛒 Ventes"
            };
            miVentesTop.Tag = new Tuple<string, Func<Form>>("btnVentes", () => new FormVentes());
            AppliquerEtatMenuModule(miVentesTop, "btnVentes", () => new FormVentes());
            BrancherClickMenuModule(miVentesTop);
            menuPrincipal.Items.Add(miVentesTop);

            // 3) Fichier (modules secondaires)
            var menuFichier = new ToolStripMenuItem
            {
                Name = "MenuFichier",
                Text = "📁 Fichier"
            };

            // 🔸 Modules "secondaires" dans Fichier (petit écran)
            // ⚠️ Name = codeModule exact (NE PAS casser)
            menuFichier.DropDownItems.Add(NewSecureModuleItem("btnProduits", "📦 Produits", () => new FormProduits()));
            menuFichier.DropDownItems.Add(NewSecureModuleItem("btnClients", "👥 Clients", () => new FormClients()));
            menuFichier.DropDownItems.Add(NewSecureModuleItem("btnInventaire", "🏬 Inventaire", () => new FormInventaire()));
            menuFichier.DropDownItems.Add(NewSecureModuleItem("btnCaisse", "💳 Caisse", () => new FormCaisse()));
            menuFichier.DropDownItems.Add(NewSecureModuleItem("btnSessionsCaisse", "🧾 Sessions caisse", () => new SessionCaisse()));
            menuFichier.DropDownItems.Add(NewSecureModuleItem("btnEntreesSortiesCaisse", "↕ Entrées / Sorties caisse", () => new FrmEntreesSortiesCaisse()));
            menuFichier.DropDownItems.Add(NewSecureModuleItem("BtnClotureJournaliere", "🔒 Clôture journalière", () => new FrmClotureJournaliere()));
            // Présence / Absence
            menuFichier.DropDownItems.Add(
                NewSecureModuleItem("btnPresenceAbsence", "🕒 Présence / Absence", () => new FrmPresenceAbsence())
            );

            // Retrait fidélité
            menuFichier.DropDownItems.Add(
                NewSecureModuleItem("btnRetraitFidelite", "🎁 Retrait fidélité", () => new FrmFideliteRetrait())
            );

            menuFichier.DropDownItems.Add(new ToolStripSeparator());

            // Détails (si vente sélectionnée) : garde ton système
            var miDetails = new ToolStripMenuItem
            {
                Name = "btnDetails",
                Text = "🔎 Détails vente"
            };
            miDetails.Click += (s, e) =>
            {
                int idVente = GetIdVenteSelectionneeOuCourante();
                if (idVente <= 0)
                {
                    MessageBox.Show("Sélectionne d'abord une vente.");
                    return;
                }
                OuvrirModule("btnDetails", "Ouverture module Détails", () => new FormDetails(idVente));
            };
            // état + look
            miDetails.Tag = new Tuple<string, Func<Form>>("btnDetails", () => new FormDetails(GetIdVenteSelectionneeOuCourante()));
            AppliquerEtatMenuModule(miDetails, "btnDetails", () => new FormDetails(GetIdVenteSelectionneeOuCourante()));
            menuFichier.DropDownItems.Add(miDetails);

            menuFichier.DropDownItems.Add(new ToolStripSeparator());

            // Déconnexion / Quitter
            var miDeconnexion = new ToolStripMenuItem
            {
                Name = "btnDeconnexion",
                Text = "🚪 " + ConfigSysteme.Traduire("Deconnexion")
            };
            miDeconnexion.Click += (s, e) => btnDeconnexion.PerformClick();

            var miQuitter = new ToolStripMenuItem
            {
                Name = "menuQuitter",
                Text = "❌ " + ConfigSysteme.Traduire("menuQuitter")
            };
            miQuitter.Click += (s, e) => Application.Exit();

            menuFichier.DropDownItems.Add(miDeconnexion);
            menuFichier.DropDownItems.Add(miQuitter);

            menuPrincipal.Items.Add(menuFichier);
        }

        private void BuildMenuAdminComplet()
        {
            menuPrincipal.Items.Clear();

            // =========================================================
            // STYLE GLOBAL MENU (lisible + propre)
            // =========================================================
            try
            {
                menuPrincipal.RenderMode = ToolStripRenderMode.System;
                menuPrincipal.Font = new Font("Segoe UI", 11F, FontStyle.Regular); // ✅ global un peu plus gros

                // ✅ items top-level un peu plus gros
                foreach (ToolStripItem it in menuPrincipal.Items)
                    it.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            }
            catch { }

            // =========================================================
            // Helpers locaux (robustes) : ne dépendent pas de Enabled/Visible des boutons
            // =========================================================
            void ApplyTopStyle(ToolStripMenuItem top, Color back, Color fore)
            {
                if (top == null) return;

                top.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                top.BackColor = back;
                top.ForeColor = fore;

                // style des sous-items
                foreach (ToolStripItem si in top.DropDownItems)
                {
                    if (si is ToolStripSeparator) continue;
                    si.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }

                // ✅ garder la couleur même au survol
                top.DropDownOpening += (s, e) =>
                {
                    try
                    {
                        top.ForeColor = fore;
                        top.BackColor = back;
                    }
                    catch { }
                };
            }

            void ApplyDropStyle(ToolStripMenuItem top, Color back, Color fore)
            {
                if (top == null) return;
                top.DropDown.BackColor = back;

                foreach (ToolStripItem si in top.DropDownItems)
                {
                    if (si is ToolStripSeparator) continue;
                    si.BackColor = back;
                    si.ForeColor = fore;
                    si.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
            }

            void FireClickEvenIfDisabled(Button btn)
            {
                if (btn == null) return;

                try
                {
                    var mi = typeof(Control).GetMethod("OnClick",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    mi.Invoke(btn, new object[] { EventArgs.Empty });
                }
                catch
                {
                    try { btn.PerformClick(); } catch { }
                }
            }

            ToolStripMenuItem NewItemFromButton(string name, string text, Button btn)
            {
                var mi = new ToolStripMenuItem
                {
                    Name = name,
                    Text = text,
                    Enabled = true
                };

                mi.Click += (s, e) =>
                {
                    try
                    {
                        if (btn == null)
                        {
                            MessageBox.Show("Module introuvable (bouton non initialisé).");
                            return;
                        }

                        FireClickEvenIfDisabled(btn);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur ouverture module : " + ex.Message);
                    }
                };

                return mi;
            }

            ToolStripMenuItem NewPanelFormItem(string name, string text, Func<Form> factory)
            {
                var mi = new ToolStripMenuItem
                {
                    Name = name,
                    Text = text,
                    Enabled = true
                };

                mi.Click += (s, e) =>
                {
                    try
                    {
                        if (AppContext.ModeConfigPOS)
                        {
                            ShowLockedOverlay("Configuration POS obligatoire avant d’utiliser le système.");
                            return;
                        }

                        Form f = (factory != null) ? factory.Invoke() : null;
                        if (f == null)
                        {
                            MessageBox.Show("Formulaire introuvable.");
                            return;
                        }

                        OuvrirFormDansPanel(f);
                        RafraichirEtatMenusModules();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur ouverture : " + ex.Message);
                    }
                };

                return mi;
            }

            ToolStripMenuItem NewBossItem(string name, string text, string code, string action, Action run)
            {
                var mi = new ToolStripMenuItem { Name = name, Text = text, Enabled = true };
                mi.Click += (s, e) =>
                {
                    if (!DemanderSignatureBoss(code, action))
                    {
                        MessageBox.Show("Signature refusée.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    run?.Invoke();
                };
                return mi;
            }

            // =========================================================
            // 1) ✅ A PROPOS (TOP)
            // =========================================================
            var miAProposTop = NewTopItem("miAPropos", "ℹ À propos", () =>
            {
                using (var f = new FrmAPropos())
                {
                    f.StartPosition = FormStartPosition.CenterParent;
                    f.ShowDialog(this);
                }
            });
            miAProposTop.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            menuPrincipal.Items.Add(miAProposTop);

            // =========================================================
            // 2) ✅ MENU FICHIER (session) - 2ème
            // =========================================================
            var menuFichier = new ToolStripMenuItem
            {
                Name = "MenuFichier",
                Text = "📁 " + ConfigSysteme.Traduire("Menu Fichier")
            };

            var miDeconnexion = new ToolStripMenuItem
            {
                Name = "btnDeconnexion",
                Text = "🚪 " + ConfigSysteme.Traduire("Deconnexion")
            };
            miDeconnexion.Click += (s, e) => btnDeconnexion.PerformClick();
            menuFichier.DropDownItems.Add(miDeconnexion);

            var miQuitter = new ToolStripMenuItem
            {
                Name = "menuQuitter",
                Text = "❌ " + ConfigSysteme.Traduire("menuQuitter")
            };
            miQuitter.Click += (s, e) => Application.Exit();
            menuFichier.DropDownItems.Add(miQuitter);

            // styles Fichier
            ApplyTopStyle(menuFichier, Color.FromArgb(35, 35, 35), Color.White);
            ApplyDropStyle(menuFichier, Color.FromArgb(45, 45, 45), Color.White);

            menuPrincipal.Items.Add(menuFichier);

            // =========================================================
            // 3) ✅ MENU BOSS (Accès ON -> visible + clickable)
            // =========================================================
            var menuBoss = new ToolStripMenuItem
            {
                Name = "MenuBoss",
                Text = "📊 BOSS",
                Visible = true
            };

            // Couleur Boss
            ApplyTopStyle(menuBoss, Color.FromArgb(85, 40, 120), Color.White);
            ApplyDropStyle(menuBoss, Color.FromArgb(95, 50, 130), Color.White);

            // 0) Configuration POS
            var itemConfigPOS_Boss = new ToolStripMenuItem("🖥 Configuration Poste POS")
            {
                Name = "miConfigPOS_Boss",
                Visible = true,
                Enabled = true
            };
            itemConfigPOS_Boss.Click += (s, e) =>
            {
                if (!DemanderSignatureBoss("CFG_POS", "Ouverture Configuration Poste POS"))
                {
                    MessageBox.Show("Signature refusée.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var f = new FrmConfigPostePOS())
                {
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        if (PosContextService.ChargerContextePOS(out string msg))
                        {
                            RefreshPosLabels();

                            AppContext.ModeConfigPOS = false;
                            AppContext.PosConfigured = true;

                            if (_lockOverlay != null) _lockOverlay.Visible = false;

                            RafraichirEtatMenusModules();
                            MessageBox.Show("POS configuré. Système débloqué.\n\n" + msg, "POS",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            AppContext.ModeConfigPOS = true;
                            AppContext.PosConfigured = false;

                            if (_lockOverlay != null) _lockOverlay.Visible = true;

                            RafraichirEtatMenusModules();
                            MessageBox.Show("POS non valide.\n\n" + msg, "POS",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            };
            menuBoss.DropDownItems.Add(itemConfigPOS_Boss);
            menuBoss.DropDownItems.Add(new ToolStripSeparator());

            menuBoss.DropDownItems.Add(NewBossItem(
                "miDashboardBoss",
                "📈 Tableau de bord (150 boutiques)",
                "BOSS_DASHBOARD",
                "Ouverture Dashboard Boss",
                delegate { OuvrirDialogue(new FrmDashboardBoss(), false); }
            ));

            menuBoss.DropDownItems.Add(NewBossItem(
                "miDetailsBoutique",
                "🏬 Détails boutique (Ventes)",
                "BOSS_DETAILS_VENTES",
                "Ouverture Détails Boutique (Ventes)",
                delegate { MessageBox.Show("Ouvre ce détail depuis le Dashboard (bouton Détail)."); }
            ));

            menuBoss.DropDownItems.Add(NewBossItem(
                "miDetailsPaiements",
                "💳 Détails boutique (Paiements)",
                "BOSS_DETAILS_PAIEMENTS",
                "Ouverture Détails Boutique (Paiements)",
                delegate { MessageBox.Show("Ouvre ce détail depuis le Dashboard (bouton Paiements)."); }
            ));

            menuBoss.DropDownItems.Add(NewBossItem(
                "miLikelembaWizard",
                "🤝 Likelemba - Zaïre Sociale",
                "BOSS_LIKEMBA",
                "Ouverture Likelemba - Zaïre Sociale",
                delegate { OuvrirDialogue(new FormLikelembaWizard(), false); }
            ));

            menuBoss.DropDownItems.Add(NewBossItem(
                "miBackupRestore",
                "💾 Backup / Restore",
                "BOSS_BACKUP_RESTORE",
                "Ouverture Backup/Restore",
                delegate { OuvrirDialogue(new FormBackupRestore(), true); }
            ));

            menuBoss.DropDownItems.Add(new ToolStripSeparator());

            menuBoss.DropDownItems.Add(NewBossItem(
                "miConfigSysteme",
                "⚙ Configuration système",
                "BOSS_CONFIG_SYSTEME",
                "Ouverture configuration système",
                delegate { OuvrirDialogue(new FormConfigurationSysteme(), true); }
            ));

            menuBoss.DropDownItems.Add(NewBossItem(
                "miAuditLog",
                "🧾 Audit log",
                "BOSS_AUDIT_LOG",
                "Ouverture module Audit log",
                delegate { OuvrirDialogue(new FormAuditLog(), true); }
            ));

            menuBoss.DropDownItems.Add(NewBossItem(
                "miPermissions",
                "🛡 Gestion permissions",
                "BOSS_PERMISSIONS",
                "Ouverture gestion permissions",
                delegate { OuvrirDialogue(new FrmPermissions(), true); }
            ));

            menuPrincipal.Items.Add(menuBoss);

            // =========================================================
            // 4) ✅ MENU SUPERVISEUR (après BOSS)
            // =========================================================
            var menuSuperviseur = new ToolStripMenuItem
            {
                Name = "MenuSuperviseur",
                Text = "🧑‍💼 Superviseur"
            };

            ApplyTopStyle(menuSuperviseur, Color.FromArgb(20, 90, 55), Color.White);
            ApplyDropStyle(menuSuperviseur, Color.FromArgb(25, 105, 65), Color.White);

            menuSuperviseur.DropDownItems.Add(NewItemFromButton("btnProduits", "📦 Produits", btnProduits));
            menuSuperviseur.DropDownItems.Add(NewItemFromButton("btnClients", "👥 Clients", btnClients));
            menuSuperviseur.DropDownItems.Add(NewItemFromButton("btnInventaire", "📦 Inventaire", btnInventaire));
            menuSuperviseur.DropDownItems.Add(NewItemFromButton("btnCaisse", "🏦 Caisse", btnCaisse));
            menuSuperviseur.DropDownItems.Add(NewItemFromButton("btnSessionsCaisse", "🧾 Sessions caisse", btnSessionsCaisse));
            menuSuperviseur.DropDownItems.Add(NewItemFromButton("btnDetails", "🧾 Détails", btnDetails));

            menuSuperviseur.DropDownItems.Add(new ToolStripSeparator());

            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnEmployes", "👔 Employés", delegate { return new FormEmployes(); }));
            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnUtilisateurs", "👤 Utilisateurs", delegate { return new Utilisateurs(); }));

            menuSuperviseur.DropDownItems.Add(new ToolStripSeparator());

            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnOperationsStock", "📦 Opérations Stock", delegate { return new FrmOperationsStock(); }));
            menuSuperviseur.DropDownItems.Add(NewPanelFormItem(
                (btnStockAvance != null ? btnStockAvance.Name : "btnStockAvance"),
                "📊 Stock avancé",
                delegate { return new FrmStockAvance(); }
            ));

            menuSuperviseur.DropDownItems.Add(new ToolStripSeparator());

            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnFournisseurs", "🏷 Fournisseurs", delegate { return new FormFournisseurs(); }));
            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnCatalogueFournisseurs", "📚 Catalogue fournisseurs", delegate { return new FormCatalogueFournisseur(); }));

            menuSuperviseur.DropDownItems.Add(new ToolStripSeparator());

            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnPresenceAbsence", "🕒 Présence / Absence", delegate { return new FrmPresenceAbsence(); }));
            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnRemisesPromotions", "🏷 Remises & Promotions", delegate { return new FrmRemisesPromotions(); }));
            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnAnnulations", "↩ Annulations / Retours", delegate { return new FormAnnulations(); }));

            menuSuperviseur.DropDownItems.Add(new ToolStripSeparator());

            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnGestionFournisseursAchats", "🧩 Fournisseurs & Achats", delegate { return new FormGestionFournisseursAchats(); }));
            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnGestionImprimantes", "🖨 Gestion Imprimantes", delegate { return new FormGestionImprimantes(); }));

            menuSuperviseur.DropDownItems.Add(new ToolStripSeparator());

            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnRetraitFidelite", "🎁 Retrait fidélité", delegate { return new FrmFideliteRetrait(); }));
            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnInventaireScanner", "📱 Inventaire scanner (PDA)", delegate { return new FrmInventaireScanner(); }));
            menuSuperviseur.DropDownItems.Add(NewPanelFormItem("btnAlertesStockExp", "⚠ Alertes stock & expiration", delegate { return new FrmAlertesStockEtExpiration(); }));

            menuPrincipal.Items.Add(menuSuperviseur);

            // =========================================================
            // 5) ✅ MENU COMPTABLES
            // =========================================================
            var menuComptables = new ToolStripMenuItem
            {
                Name = "MenuComptables",
                Text = "💼 Comptables"
            };

            ApplyTopStyle(menuComptables, Color.FromArgb(0, 80, 140), Color.White);
            ApplyDropStyle(menuComptables, Color.FromArgb(10, 90, 150), Color.White);

            menuComptables.DropDownItems.Add(NewPanelFormItem("miFormComptables", "💼 Comptabilité", delegate { return new FormComptables(); }));
            menuComptables.DropDownItems.Add(NewItemFromButton("btnEntreesSortiesCaisse", "💵 Entrées / Sorties caisse", btnEntreesSortiesCaisse));
            menuComptables.DropDownItems.Add(NewItemFromButton(BtnClotureJournaliere != null ? BtnClotureJournaliere.Name : "BtnClotureJournaliere", "🧾 Clôture journalière", BtnClotureJournaliere));
            menuComptables.DropDownItems.Add(NewItemFromButton("btnDepenses", "🧾 Dépenses", btnDepenses));

            menuComptables.DropDownItems.Add(new ToolStripSeparator());

            menuComptables.DropDownItems.Add(NewPanelFormItem("btnBonCommande", "🧾 Bon de commande", delegate { return new FrmBonCommande(); }));
            menuComptables.DropDownItems.Add(NewPanelFormItem("btnReceptionFournisseur", "📦 Réception fournisseur", delegate { return new FrmReception(); }));
            menuComptables.DropDownItems.Add(NewPanelFormItem("btnFactureFournisseur", "🧾 Facture fournisseur", delegate { return new FrmFactureFournisseur(); }));
            menuComptables.DropDownItems.Add(NewPanelFormItem("btnPaiementsFournisseur", "💳 Paiements fournisseur", delegate { return new FrmPaiementsFournisseur(); }));

            menuComptables.DropDownItems.Add(new ToolStripSeparator());

            menuComptables.DropDownItems.Add(NewPanelFormItem("btnSalairesAgents", "💰 Salaires des agents", delegate { return new FormSalairesAgents(); }));

            menuPrincipal.Items.Add(menuComptables);

            // =========================================================
            // 6) ✅ MENU MARKETING
            // =========================================================
            var menuMarketing = new ToolStripMenuItem
            {
                Name = "MenuMarketing",
                Text = "📣 Marketing"
            };

            ApplyTopStyle(menuMarketing, Color.FromArgb(140, 80, 0), Color.White);
            ApplyDropStyle(menuMarketing, Color.FromArgb(155, 90, 5), Color.White);

            menuMarketing.DropDownItems.Add(NewPanelFormItem("btnMarketing", "📣 Marketing", delegate { return new FormMarketing(); }));
            menuMarketing.DropDownItems.Add(NewPanelFormItem("btnStatistiquesAvancees", "📊 Statistiques avancées", delegate { return new FormStatistiquesAvancees(); }));

            menuMarketing.DropDownItems.Add(new ToolStripSeparator());

            menuMarketing.DropDownItems.Add(NewPanelFormItem("btnPartenaires", "🤝 Partenaires", delegate { return new FormPartenaires(); }));
            menuMarketing.DropDownItems.Add(NewPanelFormItem("btnPromoPartenaires", "🎯 Promo Partenaires", delegate { return new FormPromoPartenaireManager(); }));

            menuPrincipal.Items.Add(menuMarketing);

            // =========================================================
            // 7) ✅ VENTE (TOP-LEVEL) - plus gros + plus lisible
            // =========================================================
            // ⚠️ AjouterMenuDepuisBouton crée le top item. Après l'ajout, on récupère l'item et on le stylise.

            int beforeCount = menuPrincipal.Items.Count;
            AjouterMenuDepuisBouton("btnVentes", btnVentes);

            // retrouver le menu ajouté
            ToolStripMenuItem miVentes = null;
            if (menuPrincipal.Items.Count > beforeCount)
            {
                miVentes = menuPrincipal.Items[menuPrincipal.Items.Count - 1] as ToolStripMenuItem;
            }
            else
            {
                // fallback : chercher par Name
                foreach (ToolStripItem it in menuPrincipal.Items)
                {
                    if (it is ToolStripMenuItem t && t.Name == "btnVentes") { miVentes = t; break; }
                }
            }

            if (miVentes != null)
            {
                // ✅ Même police que les autres menus (alignement parfait)
                miVentes.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

                // ✅ Couleurs
                miVentes.ForeColor = Color.White;
                miVentes.BackColor = Color.FromArgb(180, 20, 35); // rouge Zaïre

                // ✅ Donne un aspect plus "grand" sans casser la hauteur globale
                miVentes.AutoSize = true;

                // Padding augmente l'espace interne -> plus lisible, et reste centré
                miVentes.Padding = new Padding(14, 6, 14, 6);

                // Margin externe (optionnel) pour respirer
                miVentes.Margin = new Padding(4, 0, 4, 0);

                // ✅ Sous-items
                foreach (ToolStripItem si in miVentes.DropDownItems)
                {
                    if (si is ToolStripSeparator) continue;
                    si.Font = new Font("Segoe UI", 10F, FontStyle.Regular); // comme tes autres menus
                }

                miVentes.DropDown.BackColor = Color.FromArgb(195, 30, 45);
                foreach (ToolStripItem si in miVentes.DropDownItems)
                {
                    if (si is ToolStripSeparator) continue;
                    si.ForeColor = Color.White;
                    si.BackColor = Color.FromArgb(195, 30, 45);
                }
            }


            // =========================================================
            // ✅ forcer le style top-level après ajout (ordre demandé)
            // =========================================================
            // Ordre demandé :
            // A PROPOS, MENU FICHIER, BOSS, SUPERVISEUR, COMPTABLE, MARKETING, VENTE
            // -> On a déjà ajouté dans cet ordre exact.

            try
            {
                // rendre les top-level un peu gros (sauf si Ventes déjà plus gros)
                foreach (ToolStripItem it in menuPrincipal.Items)
                {
                    var t = it as ToolStripMenuItem;
                    if (t == null) continue;

                    if (t == miVentes) continue;

                    t.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                }
            }
            catch { }
        }




        private void AjouterMenuDepuisBouton(string codeModule, Button bouton)
        {
            if (bouton == null) return;
            if (string.IsNullOrWhiteSpace(codeModule)) return;
            if (menuPrincipal == null) return;

            var menu = new ToolStripMenuItem
            {
                Name = codeModule,
                Text = ConfigSysteme.Traduire(codeModule)
            };

            // ✅ Factory dans Tag (comme les autres menus)
            menu.Tag = new Tuple<string, Func<Form>>(codeModule, () => CreerFormParCode(codeModule));

            // ✅ état visuel / enabled
            AppliquerEtatMenuModule(menu, codeModule, () => CreerFormParCode(codeModule));

            // ✅ UN SEUL handler
            BrancherClickMenuModule(menu);

            menuPrincipal.Items.Add(menu);

            // suivre l'état du bouton
            bouton.EnabledChanged += (s, e) =>
                AppliquerEtatMenuModule(menu, codeModule, () => CreerFormParCode(codeModule));
        }

        // ✅ Factory centralisée (à compléter)
        private Form CreerFormParCode(string codeModule)
        {
            switch (codeModule)
            {
                case "btnVentes": return new FormVentes();
                case "btnProduits": return new FormProduits();
                case "btnClients": return new FormClients();
                case "btnInventaire": return new FormInventaire();
                case "btnCaisse": return new FormCaisse();
                case "btnSessionsCaisse": return new SessionCaisse();
                case "btnEntreesSortiesCaisse": return new FrmEntreesSortiesCaisse();
                case "BtnClotureJournaliere": return new FrmClotureJournaliere();
                case "btnPresenceAbsence": return new FrmPresenceAbsence();
                case "btnDepenses": return new FrmDepense();
                case "btnStockAvance": return new FrmStockAvance();
                case "btnEmployes": return new FormEmployes();
                case "btnDetails":
                    return new FormDetails(GetIdVenteSelectionneeOuCourante());
                case "btnUtilisateurs": return new Utilisateurs();
                case "btnConfigSysteme": return new FormConfigurationSysteme();
                case "btnPermissions": return new FrmPermissions();
                case "btnAuditLog": return new FormAuditLog();
                case "btnRemisesPromotions": return new FrmRemisesPromotions();
                case "btnAnnulations": return new FormAnnulations();
                case "btnMarketing": return new FormMarketing();
                case "btnComptables": return new FormComptables();
                case "btnSalairesAgents": return new FormSalairesAgents();
                case "btnInventaireScanner": return new FrmInventaireScanner();
                case "btnAlertesStockExp": return new FrmAlertesStockEtExpiration();
                case "btnOperationsStock": return new FrmOperationsStock();

                case "btnBonCommande": return new FrmBonCommande();
                case "btnReceptionFournisseur": return new FrmReception();
                case "btnFactureFournisseur": return new FrmFactureFournisseur();
                case "btnPaiementsFournisseur": return new FrmPaiementsFournisseur();

                case "btnFournisseurs": return new FormFournisseurs();
                case "btnCatalogueFournisseurs": return new FormCatalogueFournisseur();
                case "btnPartenaires": return new FormPartenaires();
                case "btnPromoPartenaires": return new FormPromoPartenaireManager();

                default:
                    return new Form { Text = "Module non défini : " + codeModule };
            }
        }



        private void AppliquerEtatMenuModule(ToolStripMenuItem menu, string codeModule, Func<Form> factory)
        {
            if (menu == null) return;
            if (string.IsNullOrWhiteSpace(codeModule)) return;

            bool isAdmin = ConfigSysteme.RolesSecurite.EstAdmin(SessionEmploye.Poste);
            bool autoriseDb = HasPermissionInDb(codeModule);
            bool modeOn = ConfigSysteme.AccesControleOn;

            // ✅ Tag toujours à jour
            menu.Tag = new Tuple<string, Func<Form>>(codeModule, factory);

            // Admin ou autorisé DB
            if (isAdmin || autoriseDb)
            {
                menu.Enabled = true;
                menu.ForeColor = menuPrincipal != null ? menuPrincipal.ForeColor : SystemColors.ControlText;
                menu.Font = new Font(menu.Font, FontStyle.Regular);
                return;
            }

            // Non autorisé + OFF => bloqué
            if (!modeOn)
            {
                menu.Enabled = false;
                menu.ForeColor = Color.Gray;
                menu.Font = new Font(menu.Font, FontStyle.Italic);
                return;
            }

            // Non autorisé + ON => autorisé via signature
            menu.Enabled = true;
            menu.ForeColor = Color.Gray;
            menu.Font = new Font(menu.Font, FontStyle.Italic);
        }

        private DateTime _lastOpen = DateTime.MinValue;

        private void RefreshPosLabels()
        {
            if (lblEntreprise == null || lblMagasin == null || lblCaisse == null)
                return;

            if (AppContext.ModeConfigPOS || !AppContext.PosConfigured)
            {
                lblEntreprise.Text = "Entreprise: (non configuré)";
                lblMagasin.Text = "Magasin: (non configuré)";
                lblCaisse.Text = "Caisse: " + Environment.MachineName;
                return;
            }

            lblEntreprise.Text = "🏢 Entreprise: " + (AppContext.NomEntreprise ?? "");
            lblMagasin.Text = "🏬 Magasin: " + (AppContext.NomMagasin ?? "");
            lblCaisse.Text = "💳 Caisse: " + (AppContext.NomPOS ?? "");
        }


        private void MenuModule_Click(object sender, EventArgs e)
        {
            if ((DateTime.Now - _lastOpen).TotalMilliseconds < 300) return;
            _lastOpen = DateTime.Now;

            if (AppContext.ModeConfigPOS)
            {
                ShowLockedOverlay("Configuration POS obligatoire avant d’utiliser le système.");
                return;
            }

            var item = sender as ToolStripMenuItem;
            if (item == null) return;

            var t = item.Tag as Tuple<string, Func<Form>>;
            if (t == null) return;

            OuvrirModule(t.Item1, "Ouverture " + t.Item1, t.Item2);

            AppliquerEtatMenuModule(item, t.Item1, t.Item2);
        }


        private void CacherBoutonsTransformesEnMenus()
{
    Button[] boutons =
    {
        btnVentes, btnProduits, btnClients, btnInventaire, btnCaisse,
        btnSessionsCaisse, BtnClotureJournaliere, btnEntreesSortiesCaisse,
        btnPresenceAbsence, btnDepenses, btnEmployes, btnOperationsStock,
        btnUtilisateurs, btnDetails, btnStockAvance, btnStatistiquesAvancees,
        btnGestionFournisseursAchats, btnGestionImprimantes, btnRetraitFidelite,
        btnInventaireScanner, btnAlertesStockExp,
        btnBonCommande, btnReceptionFournisseur, btnFactureFournisseur,
        btnPaiementsFournisseur, btnPartenaires, btnPromoPartenaires,
    };

    foreach (var btn in boutons.Where(b => b != null))
    {
        btn.Visible = false;     // ✅ propre
        // btn.Enabled garde les permissions (important)
    }
}


        // ================== RÔLES ==================
        bool EstRoleAdmin(string role)
        {
            string[] admins =
            {
                "Superviseur",
                "Programmeur",
                "Gérant",
                "Directeur Général",
                
            };

            return admins.Any(r =>
                r.Equals(role, StringComparison.OrdinalIgnoreCase));
        }
        void ListerTousLesBoutons(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button btn)
                {
                    System.Diagnostics.Debug.WriteLine($"Bouton présent dans FormMain : {btn.Name}");
                }
                if (c.HasChildren)
                    ListerTousLesBoutons(c);
            }
        }

        // ================== BOUTONS ==================
        void DesactiverTousLesBoutons()
        {
            DesactiverBoutonsRecursif(this);
        }

        void DesactiverBoutonsRecursif(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button btn && btn.Name.StartsWith("btn"))
                {
                    btn.Enabled = false;
                    // btn.Visible = true;   // ❌ SUPPRIME
                }

                if (c.HasChildren)
                    DesactiverBoutonsRecursif(c);
            }
        }

        void ActiverTousLesBoutons()
        {
            ActiverBoutonsRecursif(this);
        }

        void ActiverBoutonsRecursif(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button btn && btn.Name.StartsWith("btn"))
                {
                    btn.Enabled = true;
                    // btn.Visible = true;   // ❌ SUPPRIME
                }

                if (c.HasChildren)
                    ActiverBoutonsRecursif(c);
            }
        }

        // ================== PERMISSIONS DB ==================
        void AppliquerPermissionsDepuisDB(string poste)
        {
            DesactiverTousLesBoutons();

            // Liste des rôles admins (qui doivent toujours avoir accès à btnEntreesSortiesCaisse)
            string[] rolesAdmin = new string[]
            {
        "Superviseur",
        "Programmeur",
        "Gérant",
        "Directeur Général"
            };

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(@"
            SELECT m.CodeModule
            FROM Modules m
            INNER JOIN RoleModules rm ON rm.IdModule = m.IdModule
            INNER JOIN Roles r ON r.IdRole = rm.IdRole
            WHERE r.NomRole = @poste
            AND rm.Autorise = 1", con);

                cmd.Parameters.AddWithValue("@poste", poste);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                var modulesAutorises = new List<string>();

                while (dr.Read())
                {
                    string nomBtn = dr["CodeModule"].ToString();
                    modulesAutorises.Add(nomBtn);
                }

                // Activation des boutons
                foreach (string nomBtn in modulesAutorises)
                {
                    Control ctrl = FindControlRecursive(this, nomBtn);
                    if (ctrl is Button btn)
                    {
                        btn.Enabled = true;
                        // btn.Visible = true;   // ❌ SUPPRIME
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Bouton NON trouvé pour CodeModule : {nomBtn}");
                    }
                }
            }
        }

        private Control FindControlRecursive(Control parent, string name)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Name == name)
                    return c;
                Control child = FindControlRecursive(c, name);
                if (child != null)
                    return child;
            }
            return null;
        }


        // ================== PANEL ==================
        public void AfficherFormDansPanel(Form form)
        {
            OuvrirFormDansPanel(form);
        }

        // DANS FormMain
        // ✅ DANS FormMain
        private bool PeutOuvrirModule(string codeModule, string titreAction)
        {
            // 1) Vérifier si le module est autorisé par permissions (via l'état du bouton)
            bool autoriseParPermissions = false;
            var ctrl = this.Controls.Find(codeModule, true).FirstOrDefault();
            if (ctrl is Button b)
                autoriseParPermissions = b.Enabled;

            // 2) ADMIN : si tu veux que même admin tape mot de passe, commente ce bloc
            //    Sinon, admin passe sans mot de passe.
            if (ConfigSysteme.RolesSecurite.EstAdmin(SessionEmploye.Poste))
                return true;

            // 3) 🔒 ICI : on demande TOUJOURS un mot de passe / PIN manager à chaque ouverture
            using (var sig = new FrmSignatureManager(
    connectionString: ConfigSysteme.ConnectionString,
    typeAction: titreAction,
    permissionCode: codeModule,
    reference: codeModule,
    details: $"Accès demandé par {SessionEmploye.Prenom} {SessionEmploye.Nom} | AutoriséDB={autoriseParPermissions}",
    idEmployeDemandeur: SessionEmploye.ID_Employe,
    roleDemandeur: SessionEmploye.Poste // ✅ IMPORTANT
))
            {
                var dr = sig.ShowDialog(this);

                if (dr == DialogResult.OK && sig.Approved)
                {
                    // ✅ Audit (optionnel)
                    ConfigSysteme.AjouterAuditLog(
                        "Accès Module (Signature)",
                        $"{titreAction} | Module={codeModule} | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} | AutoriséDB={autoriseParPermissions} | ValidéPar={sig.ManagerNom} ({sig.ManagerPoste})",
                        "Succès"
                    );

                    return true;
                }

                ConfigSysteme.AjouterAuditLog(
                    "Accès Module (Signature)",
                    $"{titreAction} | Module={codeModule} | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} | REFUS",
                    "Refus"
                );

                return false;
            }
        }




        private Control GetFocusedControl(Control c)
        {
            while (c is ContainerControl cc && cc.ActiveControl != null)
                c = cc.ActiveControl;
            return c;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var focused = GetFocusedControl(this);

            // ✅ si l'utilisateur est dans un contrôle qui consomme les flèches => on ne scrolle pas le panel
            if (focused is DataGridView || focused is TextBoxBase || focused is ComboBox || focused is ListBox)
                return base.ProcessCmdKey(ref msg, keyData);

            // ✅ si un Form enfant est affiché, c'est lui qui doit scroller
            var ff = panelContenu?.Controls.OfType<Form>().FirstOrDefault();

            if (ff != null && (ff.Focused || ff.ContainsFocus))
            {
                int pas = 25;

                // AutoScrollPosition est en négatif : on travaille avec -Y
                int y = -ff.AutoScrollPosition.Y;

                if (keyData == Keys.Down)
                {
                    ff.AutoScrollPosition = new Point(0, y + pas);
                    return true;
                }
                if (keyData == Keys.Up)
                {
                    ff.AutoScrollPosition = new Point(0, Math.Max(0, y - pas));
                    return true;
                }
                if (keyData == Keys.PageDown)
                {
                    ff.AutoScrollPosition = new Point(0, y + ff.ClientSize.Height);
                    return true;
                }
                if (keyData == Keys.PageUp)
                {
                    ff.AutoScrollPosition = new Point(0, Math.Max(0, y - ff.ClientSize.Height));
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OuvrirModule(string codeModule, string titreAction, Func<Form> factory, bool toujoursSignature = false)
        {
            // ✅ Anti double clic (évite ouverture multiple)
            if ((DateTime.Now - _lastOpenModule).TotalMilliseconds < 350) return;
            _lastOpenModule = DateTime.Now;

            if (string.IsNullOrWhiteSpace(codeModule))
            {
                MessageBox.Show("Code module invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (factory == null)
            {
                MessageBox.Show("Factory invalide (module non disponible).", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ✅ Sécurité : si POS non chargé mais on a déjà PostePOS en DB, on tente de charger ici
            // (ça évite "ModeConfigPOS=true" juste parce que l'appel n'a pas été fait au login)
            if (AppContext.IdPoste <= 0 || AppContext.IdEntreprise <= 0 || AppContext.IdMagasin <= 0)
            {
                try
                {
                    string msg;
                    if (PosContextService.ChargerContextePOS(out msg))
                    {
                        // OK
                        ConfigSysteme.AjouterAuditLog("POS_CONTEXT", msg, "OK");
                    }
                    else
                    {
                        ConfigSysteme.AjouterAuditLog("POS_CONTEXT", msg, "KO");
                        AppContext.ModeConfigPOS = true;
                    }
                }
                catch (Exception ex)
                {
                    ConfigSysteme.AjouterAuditLog("POS_CONTEXT", "Erreur: " + ex.Message, "KO");
                    AppContext.ModeConfigPOS = true;
                }
            }

            // 🔒 Bloque tout si POS pas configuré
            if (AppContext.ModeConfigPOS)
            {
                ShowLockedOverlay("PC non configuré. Ouvre : Fichier > Configuration Poste POS");
                return;
            }

            // ✅ Admin direct
            if (ConfigSysteme.RolesSecurite.EstAdmin(SessionEmploye.Poste))
            {
                try
                {
                    OuvrirFormDansPanel(factory());

                    ConfigSysteme.AjouterAuditLog(
                        "OUVRIR_MODULE",
                        $"{titreAction} | Module={codeModule} | Admin={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste}) | Ent={AppContext.IdEntreprise} Mag={AppContext.IdMagasin} Poste={AppContext.IdPoste}",
                        "Succès"
                    );
                }
                catch (Exception ex)
                {
                    ConfigSysteme.AjouterAuditLog(
                        "OUVRIR_MODULE",
                        $"{titreAction} | Module={codeModule} | ERREUR={ex.Message}",
                        "Échec"
                    );
                    MessageBox.Show("Erreur ouverture module : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // ✅ Règle signature obligatoire si sensible
            bool forceSignature = toujoursSignature || (_modulesToujoursSignature != null && _modulesToujoursSignature.Contains(codeModule));

            // ✅ Autorisation
            AuthResult res;
            try
            {
                res = SecurityService.TryAuthorize(this, new AuthRequest
                {
                    ActionCode = "OPEN_MODULE_" + codeModule,
                    Title = titreAction,
                    Reference = "MODULE:" + codeModule,
                    Details =
                        $"{titreAction} | Module={codeModule} | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste}) " +
                        $"| Ent={AppContext.IdEntreprise} Mag={AppContext.IdMagasin} Poste={AppContext.IdPoste}",
                    AlwaysSignature = forceSignature,

                    // ✅ Contexte POS (grande entreprise)
                    IdEntreprise = AppContext.IdEntreprise,
                    IdMagasin = AppContext.IdMagasin,
                    IdPoste = AppContext.IdPoste, // ⚠️ ajoute ce champ dans AuthRequest
                    Scope = AuthScope.Magasin
                });
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog(
                    "OUVRIR_MODULE",
                    $"{titreAction} | Module={codeModule} | ERREUR TryAuthorize={ex.Message}",
                    "Échec"
                );

                MessageBox.Show("Erreur sécurité : " + ex.Message, "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (res == null || !res.Allowed)
            {
                ConfigSysteme.AjouterAuditLog(
                    "OUVRIR_MODULE_REFUS",
                    $"{titreAction} | Module={codeModule} | Raison={(res?.DenyReason ?? "Refus")} | User={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste}) | Ent={AppContext.IdEntreprise} Mag={AppContext.IdMagasin} Poste={AppContext.IdPoste}",
                    "Refus"
                );

                MessageBox.Show(res?.DenyReason ?? "Accès interdit.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Ouverture
            try
            {
                OuvrirFormDansPanel(factory());

                ConfigSysteme.AjouterAuditLog(
                    "OUVRIR_MODULE",
                    $"{titreAction} | Module={codeModule} | Signature={res.UsedSignature} | ValidéPar={res.ManagerName} ({res.ManagerRole}) | Ent={AppContext.IdEntreprise} Mag={AppContext.IdMagasin} Poste={AppContext.IdPoste}",
                    "Succès"
                );
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog(
                    "OUVRIR_MODULE",
                    $"{titreAction} | Module={codeModule} | ERREUR OuvrirForm={ex.Message}",
                    "Échec"
                );

                MessageBox.Show("Erreur ouverture module : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void miAccesControle_Click(object sender, EventArgs e)
        {
            ConfigSysteme.AccesControleOn = !ConfigSysteme.AccesControleOn;

            // ✅ jamais null (si on a bien cliqué un ToolStripMenuItem)
            if (sender is ToolStripMenuItem item)
                item.Checked = ConfigSysteme.AccesControleOn;

            RafraichirEtatMenusModules(); // ✅ obligatoire
        }

        private bool DemanderSignaturePourModule(string codeModule, string titreAction)
        {
            using (var sig = new FrmSignatureManager(
                connectionString: ConfigSysteme.ConnectionString,
                typeAction: "OUVERTURE_MODULE",
                permissionCode: codeModule,
                reference: codeModule,
                details: $"Demande accès {codeModule} par {SessionEmploye.Prenom} {SessionEmploye.Nom}",
                idEmployeDemandeur: SessionEmploye.ID_Employe,
                roleDemandeur: SessionEmploye.Poste   // ✅ IMPORTANT
            ))
            {
                var dr = sig.ShowDialog(this);

                if (dr == DialogResult.OK && sig.Approved)
                {
                    ConfigSysteme.AjouterAuditLog(
                        "Accès Module (Signature)",
                        $"{titreAction} | Module={codeModule} | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} | ValidéPar={sig.ManagerNom} ({sig.ManagerPoste})",
                        "Succès"
                    );
                    return true;
                }

                ConfigSysteme.AjouterAuditLog(
                    "Accès Module (Signature)",
                    $"{titreAction} | Module={codeModule} | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} | REFUS",
                    "Refus"
                );

                return false;
            }
        }



        private void OuvrirFormDansPanel(Form f)
        {
            if (f == null || panelContenu == null) return;

            panelContenu.SuspendLayout();
            try
            {
                panelContenu.Controls.Clear();

                // ✅ le panelContenu reste FILL, géré par FormMain
                panelContenu.AutoScroll = false;
                panelContenu.Padding = Padding.Empty;

                f.TopLevel = false;
                f.FormBorderStyle = FormBorderStyle.None;
                f.Dock = DockStyle.Fill;
                f.Location = Point.Empty;        // ✅ important
                f.Margin = Padding.Empty;        // ✅ important

                // ✅ si tu veux scroll, mets AutoScroll sur le FORM enfant, OK
                f.AutoScroll = true;
                f.AutoScrollMinSize = Size.Empty;

                panelContenu.Controls.Add(f);
                f.Show();
                f.BringToFront();                // ✅ sécurité
            }
            finally
            {
                panelContenu.ResumeLayout(true);
            }
        }

        private void NeutraliserDockFillParasites()
        {
            bool ContientUIPrincipale(Control c)
            {
                if (c == null) return false;

                return (topBar != null && c.Contains(topBar))
                    || (menuPrincipal != null && c.Contains(menuPrincipal))
                    || (panelContenu != null && c.Contains(panelContenu))
                    || (status != null && c.Contains(status));
            }

            foreach (Control c in this.Controls)
            {
                if (c == null) continue;

                // ✅ Ne jamais toucher rootLayout ni les contrôles principaux
                if (c == _rootLayout) continue;
                if (c == topBar || c == menuPrincipal || c == panelContenu || c == status) continue;

                // ✅ Ne jamais toucher un PARENT qui contient nos contrôles principaux
                if (ContientUIPrincipale(c)) continue;

                // ✅ Neutraliser seulement les vrais parasites
                if (c.Dock == DockStyle.Fill)
                {
                    c.Visible = false;
                    c.Enabled = false;
                    c.Dock = DockStyle.None;
                    c.Location = new Point(-5000, -5000);
                    c.SendToBack();
                }
            }
        }


        private void btnVentes_Click(object sender, EventArgs e)
              => OuvrirModule("btnVentes", "Ouverture module Ventes", () => new FormVentes());

        private void btnProduits_Click(object sender, EventArgs e)
        {
            if (!PeutOuvrirModule("btnProduits", "Ouverture module Produits"))
                return;

            ConfigSysteme.AjouterAuditLog("Accès Module", "Ouverture module Produits", "Succès");
            AfficherFormDansPanel(new FormProduits());
        }

        private void btnClients_Click(object sender, EventArgs e)
        => OuvrirModule("btnClients", "Ouverture module Clients", () => new FormClients());

        private void btnInventaire_Click(object sender, EventArgs e)
       => OuvrirModule("btnInventaire", "Ouverture module Inventaire", () => new FormInventaire());

        private void btnEmployes_Click(object sender, EventArgs e)
            => OuvrirModule("btnEmployes", "Ouverture module Employés", () => new FormEmployes());

        private void btnCaisse_Click(object sender, EventArgs e)
        => OuvrirModule("btnCaisse", "Ouverture module Caisse", () => new FormCaisse());

        private void btnDetails_Click(object sender, EventArgs e)
        {
            int idVente = GetIdVenteSelectionneeOuCourante();
            if (idVente <= 0)
            {
                MessageBox.Show("Sélectionne d'abord une vente.");
                return;
            }

            OuvrirModule("btnDetails", "Ouverture module Détails", () => new FormDetails(idVente));
        }

        private void btnUtilisateurs_Click(object sender, EventArgs e)
=> OuvrirModule("btnUtilisateurs", "Ouverture module Utilisateurs", () => new Utilisateurs());

        private void btnDeconnexion_Click(object sender, EventArgs e)
        {
            // Déconnexion : pas besoin de signature
            ConfigSysteme.AjouterAuditLog("Déconnexion",
                $"Utilisateur : {SessionEmploye.Prenom} {SessionEmploye.Nom}",
                "Succès");

            new FormLogin().Show();
            this.Close();
        }

        private void lblCassier_Click(object sender, EventArgs e)
        {

        }
        

        private void btnSessionsCaisse_Click(object sender, EventArgs e)
            => OuvrirModule("btnSessionsCaisse", "Ouverture module Sessions caisse", () => new SessionCaisse());

        

        private void BtnClotureJournaliere_Click(object sender, EventArgs e)
        => OuvrirModule("BtnClotureJournaliere", "Ouverture module Clôture journalière", () => new FrmClotureJournaliere());



        private void btnPresenceAbsence_Click(object sender, EventArgs e)
             => OuvrirModule("btnPresenceAbsence", "Ouverture module Présence / Absence", () => new FrmPresenceAbsence());

        private void btnDepenses_Click(object sender, EventArgs e)
         => OuvrirModule("btnDepenses", "Ouverture module Dépenses", () => new FrmDepense());

        private void btnConfigSysteme_Click(object sender, EventArgs e)
        {
            // ✅ Protégé aussi (si tu veux que seul manager peut ouvrir la config)
            if (!PeutOuvrirModule("btnConfigSysteme", "Ouverture configuration système"))
                return;

            ConfigSysteme.AjouterAuditLog("Configuration", "Ouverture configuration système", "Succès");

            using (FormConfigurationSysteme frm = new FormConfigurationSysteme())
                frm.ShowDialog();

            ConfigSysteme.ChargerConfig();
            RafraichirTheme();
            RafraichirLangue();
        }

        private async void btnPermissions_Click(object sender, EventArgs e)
        {
            // ✅ Tu veux absolument empreinte/manager : laisse comme ça.
            // Ici, ce n'est pas le SignatureManager mais ta biométrie.
            if (!await AutoriserAdminParEmpreinte("Permissions : confirmez par empreinte"))
                return;

            ConfigSysteme.AjouterAuditLog("Permissions", "Accès gestion des permissions", "Succès");
            new FrmPermissions().ShowDialog(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btnStockAvance_Click(object sender, EventArgs e)
        => OuvrirModule("btnStockAvance", "Ouverture module Stock avancé", () => new FrmStockAvance());

        private void btnExtraireTousTextes_Click(object sender, EventArgs e)
        {
        }

        private void btnStatistiquesAvancees_Click(object sender, EventArgs e)
            => OuvrirModule("btnStatistiquesAvancees", "Ouverture module Statistiques avancées", () => new FormStatistiquesAvancees());

        private void btnAuditLog_Click(object sender, EventArgs e)
        => OuvrirModule("btnAuditLog", "Ouverture module Audit log", () => new FormAuditLog());

        private void btnGestionFournisseursAchats_Click(object sender, EventArgs e)
        => OuvrirModule("btnGestionFournisseursAchats", "Ouverture module Gestion Fournisseurs & Achats", () => new FormGestionFournisseursAchats());

        private void btnGestionImprimantes_Click(object sender, EventArgs e)
            => OuvrirModule("btnGestionImprimantes", "Ouverture module Gestion des Imprimantes", () => new FormGestionImprimantes());

        private void menuPrincipal_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void btnRemisesPromotions_Click(object sender, EventArgs e)
         => OuvrirModule("btnRemisesPromotions", "Ouverture module Remises & Promotions", () => new FrmRemisesPromotions());

        private void btnAnnulations_Click(object sender, EventArgs e)
          => OuvrirModule("btnAnnulations", "Ouverture module Annulations / Retours", () => new FormAnnulations());

        private void btnMarketing_Click(object sender, EventArgs e)
        => OuvrirModule("btnMarketing", "Ouverture module Marketing", () => new FormMarketing());

        private void btnComptables_Click(object sender, EventArgs e)
         => OuvrirModule("btnComptables", "Ouverture module Comptabilité", () => new FormComptables());

        private void btnSalairesAgents_Click(object sender, EventArgs e)
            => OuvrirModule("btnSalairesAgents", "Ouverture module Salaires des agents", () => new FormSalairesAgents());

        private void btnRetraitFidelite_Click(object sender, EventArgs e)
         => OuvrirModule("btnRetraitFidelite", "Ouverture module Retrait fidélité", () => new FrmFideliteRetrait());

        private void btnInventaireScanner_Click(object sender, EventArgs e)
         => OuvrirModule("btnInventaireScanner", "Ouverture module Inventaire scanner (PDA)", () => new FrmInventaireScanner());


        private void btnAlertesStockExp_Click(object sender, EventArgs e)
        => OuvrirModule("btnAlertesStockExp", "Ouverture module Alertes stock & expiration", () => new FrmAlertesStockEtExpiration());

        private void btnBonCommande_Click(object sender, EventArgs e)
            => OuvrirModule("btnBonCommande", "Ouverture module Bon de commande", () => new FrmBonCommande());

        private void btnReceptionFournisseur_Click(object sender, EventArgs e)
         => OuvrirModule("btnReceptionFournisseur", "Ouverture module Réception fournisseur", () => new FrmReception());


        private void btnFactureFournisseur_Click(object sender, EventArgs e)
           => OuvrirModule("btnFactureFournisseur", "Ouverture module Facture fournisseur", () => new FrmFactureFournisseur());

        private void btnPaiementsFournisseur_Click(object sender, EventArgs e)
         => OuvrirModule("btnPaiementsFournisseur", "Ouverture module Paiements fournisseur", () => new FrmPaiementsFournisseur());


        private void btnFournisseurs_Click(object sender, EventArgs e)
            => OuvrirModule("btnFournisseurs", "Ouverture module Fournisseurs", () => new FormFournisseurs());

        private void btnCatalogueFournisseurs_Click(object sender, EventArgs e)
        => OuvrirModule("btnCatalogueFournisseurs", "Ouverture module Catalogue Fournisseurs", () => new FormCatalogueFournisseur());

        private void btnPartenaires_Click(object sender, EventArgs e)
            => OuvrirModule("btnPartenaires", "Ouverture module Partenaires", () => new FormPartenaires());

        private void btnPromoPartenaires_Click(object sender, EventArgs e)
        => OuvrirModule("btnPromoPartenaires", "Ouverture module Promo Partenaires (Solde/Retraits)", () => new FormPromoPartenaireManager());

        private void btnEntreesSortiesCaisse_Click(object sender, EventArgs e)
        => OuvrirModule("btnEntreesSortiesCaisse", "Ouverture module Entrées / Sorties caisse", () => new FrmEntreesSortiesCaisse());

        private void btnOperationsStock_Click(object sender, EventArgs e)
        => OuvrirModule("btnOperationsStock", "Ouverture module Operations Stock", () => new FrmOperationsStock());

    }
}



