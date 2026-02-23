using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using ZXing;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FormPartenaires : Form
    {
        private DataGridView _dgv;
        private Button _btnRefresh;
        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnCard;
        private Button _btnPromo;
        private Button _btnPrintPlast;
        private Button _btnPrintPVC;

        private readonly string _logoPath = @"D:\ZAIRE\LOGO1.png";

        // Impression
        private PrintDocument _printDoc = null;
        private int _printIndex = 0; // 0 recto, 1 verso

        private List<PartenaireCardInfo> _cardsToPrint = new List<PartenaireCardInfo>();
        private ContextMenuStrip _menu;

        // Couleurs Premium (copie de FormClients)
        private Color _premiumBase = Color.FromArgb(16, 24, 40);
        private Color _premiumAccent = Color.FromArgb(198, 169, 107);
        private Color _premiumInk = Color.FromArgb(245, 245, 245);

        // ====== Refresh + reselect ======
        private int _lastSelectedId = 0;
        private Image _logoCache = null;

        private void SaveSelection()
        {
            _lastSelectedId = GetSelectedId();
        }

        private void SelectRowById(int idPartenaire)
        {
            if (idPartenaire <= 0) return;
            if (_dgv == null || _dgv.Rows == null) return;
            if (!_dgv.Columns.Contains("IdPartenaire")) return;

            foreach (DataGridViewRow row in _dgv.Rows)
            {
                if (row.IsNewRow) continue;

                object v = row.Cells["IdPartenaire"]?.Value;
                if (v != null && v != DBNull.Value && int.TryParse(v.ToString(), out int id) && id == idPartenaire)
                {
                    _dgv.ClearSelection();
                    row.Selected = true;

                    // mettre CurrentCell sur une cellule visible
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.Visible)
                        {
                            _dgv.CurrentCell = cell;
                            break;
                        }
                    }

                    // scroll
                    if (row.Index >= 0 && row.Index < _dgv.Rows.Count)
                        _dgv.FirstDisplayedScrollingRowIndex = Math.Max(0, row.Index);

                    return;
                }
            }
        }

        private void RefreshAndReselect(int idToReselect = 0)
        {
            int id = idToReselect > 0 ? idToReselect : GetSelectedId();

            LoadPartenaires();     // ton chargement
            SelectRowById(id);     // reselect
        }


        private class PartenaireCardInfo
        {
            public int IdPartenaire { get; set; }
            public string Nom { get; set; }
            public string CodeCarte { get; set; }
        }
        public FormPartenaires()
        {
            InitializeComponent();

            TryLoadLogoCache();
            this.FormClosed += (s, e) => DisposeLogoCache();

            Text = "Partenaires";
            Width = 980;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            _dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false // ✅ cohérent avec GetSelectedId() + actions mono
            };

            _btnPromo = new Button { Text = "💰 Solde & Retraits", Dock = DockStyle.Bottom, Height = 42 };
            _btnCard = new Button { Text = "💳 Générer / Voir Code Carte", Dock = DockStyle.Bottom, Height = 42 };
            _btnDelete = new Button { Text = "🗑 Supprimer", Dock = DockStyle.Bottom, Height = 42 };
            _btnEdit = new Button { Text = "✏ Modifier", Dock = DockStyle.Bottom, Height = 42 };
            _btnAdd = new Button { Text = "➕ Ajouter", Dock = DockStyle.Bottom, Height = 42 };
            _btnRefresh = new Button { Text = "🔄 Actualiser", Dock = DockStyle.Bottom, Height = 42 };

            _btnPrintPlast = new Button { Text = "🖨 Imprimer carte (Plastification)", Dock = DockStyle.Bottom, Height = 42 };
            _btnPrintPVC = new Button { Text = "🖨 Imprimer carte (PVC)", Dock = DockStyle.Bottom, Height = 42 };
            // ✅ emojis + texte en dessous (abréviations si besoin)
            StyleBottomEmojiButton(_btnRefresh, "🔄", "Actu");
            StyleBottomEmojiButton(_btnAdd, "➕", "Ajout");
            StyleBottomEmojiButton(_btnEdit, "✏", "Modif");
            StyleBottomEmojiButton(_btnDelete, "🗑", "Suppr");
            StyleBottomEmojiButton(_btnCard, "💳", "Carte");
            StyleBottomEmojiButton(_btnPromo, "💰", "Solde");
            StyleBottomEmojiButton(_btnPrintPlast, "🖨", "Plastif");
            StyleBottomEmojiButton(_btnPrintPVC, "🖨", "PVC");

            _btnRefresh.Click += (s, e) => RefreshAndReselect();

            _btnPromo.Dock = DockStyle.None;
            _btnCard.Dock = DockStyle.None;
            _btnDelete.Dock = DockStyle.None;
            _btnEdit.Dock = DockStyle.None;
            _btnAdd.Dock = DockStyle.None;
            _btnRefresh.Dock = DockStyle.None;
            _btnPrintPlast.Dock = DockStyle.None;
            _btnPrintPVC.Dock = DockStyle.None;


            var bottomBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 74,
                Padding = new Padding(8, 8, 8, 8),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true
            };

            // ✅ ordre maîtrisé (tu choisis)
            bottomBar.Controls.Add(_btnRefresh);
            bottomBar.Controls.Add(_btnAdd);
            bottomBar.Controls.Add(_btnEdit);
            bottomBar.Controls.Add(_btnDelete);
            bottomBar.Controls.Add(_btnCard);
            bottomBar.Controls.Add(_btnPromo);
            bottomBar.Controls.Add(_btnPrintPlast);
            bottomBar.Controls.Add(_btnPrintPVC);

            // ✅ ordre des Controls : d'abord la grille, ensuite la barre (ou inverse selon ton besoin)
            Controls.Add(_dgv);
            Controls.Add(bottomBar);

            _btnPrintPlast.Click += (s, e) => PrintPartenairePlastification();
            _btnPrintPVC.Click += (s, e) => PrintPartenairePVC();

            InitMenuContextuel();
            _dgv.MouseDown += _dgv_MouseDown_SelectRowRightClick;
            

            _btnAdd.Click += (s, e) => AddPartenaire();
            _btnEdit.Click += (s, e) => EditSelected();
            _btnDelete.Click += (s, e) => DeleteSelected();
            _btnCard.Click += (s, e) => GenerateOrShowCard();
            _btnPromo.Click += (s, e) => OpenPromoManager();

            LoadPartenaires();
        }

        private void StyleBottomEmojiButton(Button b, string emoji, string label)
        {
            // ✅ Emoji en haut, texte en bas
            b.Text = $"{emoji}\n{label}";
            b.TextAlign = ContentAlignment.MiddleCenter;

            // ✅ Important pour que \n soit bien pris en compte
            b.AutoSize = false;

            // ✅ taille uniforme (ajuste si tu veux)
            b.Width = 120;
            b.Height = 52;

            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 30, 30);

            b.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            b.Padding = new Padding(6, 4, 6, 4);
        }


        private void FormPartenaires_Load(object sender, EventArgs e)
        {

        }

        private void TryLoadLogoCache()
        {
            DisposeLogoCache();

            if (!System.IO.File.Exists(_logoPath)) return;
            try
            {
                // ✅ copie en mémoire pour éviter le lock du fichier
                using (var fs = new System.IO.FileStream(_logoPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                using (var tmp = Image.FromStream(fs))
                {
                    _logoCache = new Bitmap(tmp);
                }
            }
            catch
            {
                _logoCache = null;
            }
        }

        private void DisposeLogoCache()
        {
            try { _logoCache?.Dispose(); } catch { }
            _logoCache = null;
        }

        private void InitMenuContextuel()
        {
            _menu = new ContextMenuStrip();

            var mnuGen = new ToolStripMenuItem("💳 Générer / Voir Code Carte");
            var mnuPVC = new ToolStripMenuItem("🖨 Imprimer carte (PVC)");
            var mnuA4 = new ToolStripMenuItem("🖨 Imprimer carte (Plastification A4)");
            var mnuSolde = new ToolStripMenuItem("💰 Solde / Retrait");
            var mnuRefresh = new ToolStripMenuItem("🔄 Actualiser");

            mnuGen.Click += (s, e) =>
            {
                int id = GetSelectedId();
                if (id <= 0) { MessageBox.Show("Sélectionne un partenaire."); return; }
                GenerateOrShowCard();
                RefreshAndReselect(id);
            };

            mnuPVC.Click += (s, e) =>
            {
                int id = GetSelectedId();
                if (id <= 0) { MessageBox.Show("Sélectionne un partenaire."); return; }
                PrintPartenairePVC(); // ✅ à toi (ou code plus bas si tu veux)
            };

            mnuA4.Click += (s, e) =>
            {
                PrintPartenairePlastification(); // ✅ à toi (A4 batch 1..8)
            };

            mnuSolde.Click += (s, e) =>
            {
                int id = GetSelectedId();
                if (id <= 0) { MessageBox.Show("Sélectionne un partenaire."); return; }
                OpenPromoManager();
                RefreshAndReselect(id);
            };

            mnuRefresh.Click += (s, e) => RefreshAndReselect();

            _menu.Items.AddRange(new ToolStripItem[]
            {
        mnuGen,
        new ToolStripSeparator(),
        mnuPVC,
        mnuA4,
        new ToolStripSeparator(),
        mnuSolde,
        new ToolStripSeparator(),
        mnuRefresh
            });

            // Option: désactiver si pas de ligne
            _menu.Opening += (s, e) =>
            {
                bool has = (_dgv.CurrentRow != null);
                mnuGen.Enabled = has;
                mnuPVC.Enabled = has;
                mnuA4.Enabled = true;   // A4 peut être multi-select
                mnuSolde.Enabled = has;
                mnuRefresh.Enabled = true;
            };

            _dgv.ContextMenuStrip = _menu;
        }

        private void _dgv_MouseDown_SelectRowRightClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var hit = _dgv.HitTest(e.X, e.Y);
            if (hit.RowIndex < 0) return;

            _dgv.ClearSelection();

            DataGridViewRow row = _dgv.Rows[hit.RowIndex];
            row.Selected = true;

            // mettre CurrentCell sur une cellule visible
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.Visible)
                {
                    _dgv.CurrentCell = cell;
                    break;
                }
            }
        }



        // ✅ Format conseillé : PART-000025
        private string BuildCodeCartePartenaire(int idPartenaire)
        {
            return "PART-" + idPartenaire.ToString("D6");
        }

        private void LoadPartenaires()
        {
            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var da = new SqlDataAdapter(@"
SELECT 
    IdPartenaire,
    Nom,
    Telephone,
    Email,
    Adresse,
    CodeCarte,
    SoldePromo,
    Actif
FROM Partenaire
ORDER BY Nom;", cn))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        _dgv.DataSource = dt;
                    }
                }

                if (_dgv.Columns.Contains("IdPartenaire")) _dgv.Columns["IdPartenaire"].HeaderText = "ID";
                if (_dgv.Columns.Contains("Nom")) _dgv.Columns["Nom"].HeaderText = "Nom";
                if (_dgv.Columns.Contains("Telephone")) _dgv.Columns["Telephone"].HeaderText = "Téléphone";
                if (_dgv.Columns.Contains("Email")) _dgv.Columns["Email"].HeaderText = "Email";
                if (_dgv.Columns.Contains("Adresse")) _dgv.Columns["Adresse"].HeaderText = "Adresse";
                if (_dgv.Columns.Contains("CodeCarte")) _dgv.Columns["CodeCarte"].HeaderText = "Carte";
                if (_dgv.Columns.Contains("SoldePromo")) _dgv.Columns["SoldePromo"].HeaderText = "Solde Promo";
                if (_dgv.Columns.Contains("Actif")) _dgv.Columns["Actif"].HeaderText = "Actif";

                // ✅ format SoldePromo
                if (_dgv.Columns.Contains("SoldePromo"))
                {
                    var col = _dgv.Columns["SoldePromo"];
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    col.DefaultCellStyle.Format = "N2"; // ou "C2" si tu veux devise
                }

                _dgv.CellFormatting -= _dgv_CellFormatting_Solde;
                _dgv.CellFormatting += _dgv_CellFormatting_Solde;
                if (_dgv.Rows.Count > 0)
                {
                    _dgv.ClearSelection();
                    _dgv.Rows[0].Selected = true;

                    // met une CurrentCell visible
                    if (_dgv.Columns.Count > 0)
                        _dgv.CurrentCell = _dgv.Rows[0].Cells[_dgv.Columns[0].Index];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement partenaires :\n" + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _dgv_CellFormatting_Solde(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgv.Columns[e.ColumnIndex].Name != "SoldePromo") return;
            if (e.Value == null || e.Value == DBNull.Value) return;

            if (decimal.TryParse(e.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v) ||
                decimal.TryParse(e.Value.ToString(), out v))
            {
                // ✅ rouge si négatif (simple, lisible)
                if (v < 0)
                    e.CellStyle.ForeColor = Color.IndianRed;
                else
                    e.CellStyle.ForeColor = Color.DarkGreen;
            }
        }

        private int GetSelectedId()
        {
            if (_dgv.CurrentRow == null) return 0;
            object v = _dgv.CurrentRow.Cells["IdPartenaire"]?.Value;
            if (v == null || v == DBNull.Value) return 0;
            int.TryParse(v.ToString(), out int id);
            return id;
        }

        // ===================== LOAD INFO PARTENAIRE (NOM + CODE) =====================
        private PartenaireCardInfo LoadPartenaireCardInfo(int idPartenaire)
        {
            using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP 1
    IdPartenaire,
    LTRIM(RTRIM(ISNULL(Nom,''))) AS Nom,
    ISNULL(CodeCarte,'') AS CodeCarte
FROM Partenaire
WHERE IdPartenaire=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPartenaire;
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        return new PartenaireCardInfo
                        {
                            IdPartenaire = r.IsDBNull(0) ? 0 : r.GetInt32(0),
                            Nom = r.IsDBNull(1) ? "" : r.GetString(1),
                            CodeCarte = r.IsDBNull(2) ? "" : r.GetString(2)
                        };
                    }
                }
            }
        }

        private bool TryGetSelectedPartenaireFromGrid(out int idPartenaire, out string codeCarte)
        {
            idPartenaire = 0;
            codeCarte = "";

            if (_dgv.CurrentRow == null)
            {
                MessageBox.Show("Sélectionne un partenaire.");
                return false;
            }

            object vId = _dgv.CurrentRow.Cells["IdPartenaire"]?.Value;
            if (vId == null || vId == DBNull.Value || !int.TryParse(vId.ToString(), out idPartenaire) || idPartenaire <= 0)
            {
                MessageBox.Show("ID partenaire invalide.");
                return false;
            }

            object vCode = _dgv.CurrentRow.Cells["CodeCarte"]?.Value;
            codeCarte = (vCode == null || vCode == DBNull.Value) ? "" : vCode.ToString().Trim();

            if (string.IsNullOrWhiteSpace(codeCarte))
            {
                MessageBox.Show("Ce partenaire n'a pas de CodeCarte. Clique sur 'Générer / Voir Code Carte' d'abord.");
                return false;
            }

            return true;
        }

        private List<int> GetSelectedPartenaireIds(int max = 8)
        {
            var ids = new List<int>();

            if (_dgv.SelectedRows != null && _dgv.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow r in _dgv.SelectedRows)
                {
                    object v = r.Cells["IdPartenaire"]?.Value;
                    if (v != null && v != DBNull.Value && int.TryParse(v.ToString(), out int id) && id > 0)
                        ids.Add(id);

                    if (ids.Count >= max) break;
                }
            }
            else if (_dgv.CurrentRow != null)
            {
                object v = _dgv.CurrentRow.Cells["IdPartenaire"]?.Value;
                if (v != null && v != DBNull.Value && int.TryParse(v.ToString(), out int id) && id > 0)
                    ids.Add(id);
            }

            return ids.Distinct().Take(max).ToList();
        }

        // ===================== ZXING QR / BARCODE =====================
        private Bitmap GenerateQr(string text, int size = 240)
        {
            var writer = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = size,
                    Height = size,
                    Margin = 1
                }
            };
            return writer.Write(text);
        }

        private Bitmap GenerateCode128(string text, int width = 900, int height = 200)
        {
            var writer = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 1,
                    PureBarcode = true
                }
            };
            return writer.Write(text);
        }

        // ===================== DESIGN HELPERS (copie) =====================
        private void FillLinearGradient(Graphics g, Rectangle r, Color c1, Color c2, float angle = 35f)
        {
            using (var br = new LinearGradientBrush(r, c1, c2, angle))
                g.FillRectangle(br, r);
        }

        private void DrawShadow(Graphics g, Rectangle r, int spread = 6, int alpha = 18)
        {
            Rectangle s = Rectangle.Inflate(r, spread, spread);
            using (var br = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                g.FillRectangle(br, s);
        }

        private void DrawPremiumFrame(Graphics g, Rectangle card)
        {
            using (var penOuter = new Pen(_premiumAccent, 3f))
            using (var penInner = new Pen(Color.FromArgb(180, _premiumAccent), 1.2f))
            {
                penOuter.Alignment = PenAlignment.Inset;
                penInner.Alignment = PenAlignment.Inset;

                g.DrawRectangle(penOuter, card);
                var inner = Rectangle.Inflate(card, -10, -10);
                g.DrawRectangle(penInner, inner);
            }
        }

        private void DrawPremiumRibbon(Graphics g, Rectangle card, string text)
        {
            var ribbon = new Rectangle(card.Right - 210, card.Top + 38, 190, 30);
            using (var br = new SolidBrush(Color.FromArgb(220, _premiumAccent)))
                g.FillRectangle(br, ribbon);

            using (var f = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var brT = new SolidBrush(Color.FromArgb(25, 25, 25)))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(text ?? "", f, brT, ribbon, fmt);
            }
        }

        private void DrawLogoSafe(Graphics g, Rectangle r)
        {
            if (_logoCache == null) return;
            try
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(_logoCache, r);
            }
            catch { }
        }

        // Remplace DrawLogoPattern par :
        private void DrawLogoPattern(Graphics g, Rectangle area, int logoW = 44, int logoH = 30, int gapX = 22, int gapY = 18, int alpha = 24)
        {
            if (_logoCache == null) return;

            try
            {
                using (var ia = new ImageAttributes())
                {
                    var cm = new ColorMatrix
                    {
                        Matrix00 = 1f,
                        Matrix11 = 1f,
                        Matrix22 = 1f,
                        Matrix33 = alpha / 255f
                    };
                    ia.SetColorMatrix(cm);

                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    for (int y = area.Top; y < area.Bottom; y += (logoH + gapY))
                    {
                        for (int x = area.Left; x < area.Right; x += (logoW + gapX))
                        {
                            var r = new Rectangle(x, y, logoW, logoH);
                            g.DrawImage(_logoCache, r, 0, 0, _logoCache.Width, _logoCache.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                }
            }
            catch { }
        }

        private Rectangle DrawWhiteSafeBox(Graphics g, Rectangle inner, string title, int boxWidth)
        {
            int topOffset = 64;
            int bottomMargin = 92;

            int boxH = inner.Height - (topOffset + bottomMargin);
            if (boxH < 120) boxH = 120;

            Rectangle box = new Rectangle(inner.Right - boxWidth, inner.Top + topOffset, boxWidth, boxH);

            using (var br = new SolidBrush(Color.White))
                g.FillRectangle(br, box);

            using (var p = new Pen(Color.FromArgb(210, 210, 210), 1f))
                g.DrawRectangle(p, box);

            using (var f = new Font("Segoe UI", 8, FontStyle.Bold))
            using (var brT = new SolidBrush(Color.FromArgb(60, 60, 60)))
                g.DrawString(title ?? "", f, brT, box.Left + 10, box.Top + 8);

            return box;
        }

        private void DrawDivider(Graphics g, int x, int y1, int y2)
        {
            using (var p = new Pen(Color.FromArgb(70, 255, 255, 255), 1f))
                g.DrawLine(p, x, y1, x, y2);
        }

        // ===================== DESSIN RECTO / VERSO (PARTENAIRE) =====================
        private void DrawPartenaireCardRecto(Graphics g, Rectangle card, PartenaireCardInfo info)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawShadow(g, card, spread: 6, alpha: 18);
            FillLinearGradient(g, card, _premiumBase, Color.FromArgb(30, 58, 90), 35f);
            DrawPremiumFrame(g, card);

            float scale = card.Width / 860f;
            if (scale < 0.65f) scale = 0.65f;
            if (scale > 1.05f) scale = 1.05f;

            int pad = (int)(18 * scale);
            if (pad < 10) pad = 10;
            Rectangle inner = Rectangle.Inflate(card, -pad, -pad);

            // ✅ Texte ruban = PARTENAIRE
            DrawPremiumRibbon(g, card, "PARTENAIRE");

            int safeW = (int)(inner.Width * 0.33f);
            if (safeW < (int)(210 * scale)) safeW = (int)(210 * scale);
            if (safeW > (int)(290 * scale)) safeW = (int)(290 * scale);

            int dividerX = inner.Right - safeW - (int)(16 * scale);
            DrawDivider(g, dividerX, inner.Top + (int)(10 * scale), inner.Bottom - (int)(10 * scale));

            Rectangle safeBox = DrawWhiteSafeBox(g, inner, "SCAN", 230);

            Rectangle logoRect = new Rectangle(inner.Left + (int)(4 * scale), inner.Top + (int)(6 * scale),
                                               (int)(92 * scale), (int)(62 * scale));
            DrawLogoSafe(g, logoRect);

            int textLeft = inner.Left + (int)(110 * scale);
            int textRight = dividerX - (int)(12 * scale);
            int textWidth = Math.Max(10, textRight - textLeft);

            using (var fBrand = new Font("Segoe UI", 13f * scale, FontStyle.Bold))
            using (var fTitle = new Font("Segoe UI", 10.5f * scale, FontStyle.Bold))
            using (var fSmall = new Font("Segoe UI", 9.2f * scale, FontStyle.Regular))
            using (var fCode = new Font("Consolas", 10.5f * scale, FontStyle.Bold))
            using (var brText = new SolidBrush(_premiumInk))
            using (var brSoft = new SolidBrush(Color.FromArgb(215, _premiumInk)))
            {
                int y = inner.Top + (int)(6 * scale);

                g.DrawString("ZAIRE MODE SARL", fBrand, brText, textLeft, y);
                y += (int)(22 * scale);

                g.DrawString("CARTE PARTENAIRE", fTitle, brSoft, textLeft, y);
                y += (int)(18 * scale);

                string nom = (info?.Nom ?? "PARTENAIRE").Trim();
                if (nom.Length > 28) nom = nom.Substring(0, 28) + "...";
                g.DrawString(nom, fSmall, brText, new RectangleF(textLeft, y, textWidth, 999));
                y += (int)(18 * scale);

                string code = (info?.CodeCarte ?? "").Trim();
                g.DrawString("Code : " + code, fCode, brText, new RectangleF(textLeft, y, textWidth, 999));

                int yInfo = inner.Bottom - (int)(52 * scale);
                if (yInfo < y + (int)(26 * scale)) yInfo = y + (int)(26 * scale);

                g.DrawString("• Carte partenaire", fSmall, brSoft, inner.Left + (int)(4 * scale), yInfo);
                yInfo += (int)(16 * scale);
                g.DrawString("• Présentez la carte à chaque passage", fSmall, brSoft, inner.Left + (int)(4 * scale), yInfo);
            }

            string codeCarte = (info?.CodeCarte ?? "").Trim();

            int qrSize = (int)(118 * scale);
            if (qrSize < 75) qrSize = 75;
            if (qrSize > 125) qrSize = 125;

            Rectangle qrRect = new Rectangle(
                safeBox.Left + (int)(12 * scale),
                safeBox.Top + (int)(23 * scale),
                qrSize,
                qrSize
            );

            using (var qr = GenerateQr(codeCarte, 240))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(qr, qrRect);
            }

            int barH = (int)(40 * scale);
            if (barH < 25) barH = 25;
            if (barH > 52) barH = 52;

            Rectangle barImgRect = new Rectangle(
                safeBox.Left + (int)(12 * scale),
                qrRect.Bottom + (int)(12 * scale),
                safeBox.Width - (int)(20 * scale),
                barH
            );

            using (var barcode = GenerateCode128(codeCarte, 900, 200))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(barcode, barImgRect);
            }

            using (var fMini = new Font("Segoe UI", 8f * scale, FontStyle.Bold))
            using (var br = new SolidBrush(Color.FromArgb(40, 40, 40)))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(codeCarte, fMini, br,
                    new RectangleF(safeBox.Left, barImgRect.Bottom + (int)(2 * scale), safeBox.Width, (int)(16 * scale)), fmt);
            }
        }

        private void DrawPartenaireCardVerso(Graphics g, Rectangle card)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            FillLinearGradient(g, card, Color.White, Color.FromArgb(242, 244, 248), 90f);
            DrawLogoPattern(g, card, logoW: 52, logoH: 36, gapX: 26, gapY: 22, alpha: 38);
            DrawPremiumFrame(g, card);

            Rectangle band = new Rectangle(card.Left + 18, card.Top + (card.Height / 2) - 34, card.Width - 36, 68);
            using (var brBand = new SolidBrush(Color.FromArgb(110, _premiumAccent)))
                g.FillRectangle(brBand, band);

            string title = "ZAIRE MODE SARL";
            using (var f = new Font("Segoe UI Black", 16, FontStyle.Bold))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var brShadow = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
                    g.DrawString(title, f, brShadow, new RectangleF(band.Left + 2, band.Top + 2, band.Width, band.Height), fmt);

                using (var brText = new SolidBrush(Color.FromArgb(25, 25, 25)))
                    g.DrawString(title, f, brText, band, fmt);
            }

            using (var f2 = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var br2 = new SolidBrush(Color.FromArgb(120, 20, 20, 20)))
            {
                var fmt2 = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("CARTE PARTENAIRE", f2, br2,
                    new RectangleF(card.Left, band.Bottom + 8, card.Width, 18), fmt2);
            }

            Rectangle logoTop = new Rectangle(card.Left + 18, card.Top + 14, 90, 60);
            DrawLogoSafe(g, logoTop);
        }

        private void DrawCardVide(Graphics g, Rectangle card)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var br = new SolidBrush(Color.White))
                g.FillRectangle(br, card);

            using (var p = new Pen(Color.FromArgb(220, 220, 220), 2f))
                g.DrawRectangle(p, card);

            using (var f = new Font("Segoe UI", 10, FontStyle.Italic))
            using (var brT = new SolidBrush(Color.FromArgb(150, 150, 150)))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("CASE VIDE", f, brT, card, fmt);
            }
        }

        // ===================== IMPRESSION PLASTIFICATION (A4 / 8 cartes) =====================
        private void PrintPartenairePlastification()
        {
            var ids = GetSelectedPartenaireIds(8);
            if (ids.Count == 0)
            {
                MessageBox.Show("Sélectionnez 1 à 8 partenaires dans la liste (Ctrl+Click / Shift+Click).");
                return;
            }

            _cardsToPrint.Clear();
            foreach (int id in ids)
            {
                var info = LoadPartenaireCardInfo(id);
                if (info == null)
                {
                    MessageBox.Show("Partenaire introuvable ID=" + id);
                    return;
                }

                if (string.IsNullOrWhiteSpace(info.CodeCarte))
                {
                    MessageBox.Show($"Le partenaire '{info.Nom}' n'a pas de CodeCarte. Générez d'abord sa carte.");
                    return;
                }

                _cardsToPrint.Add(info);
            }

            while (_cardsToPrint.Count < 8)
                _cardsToPrint.Add(null);

            _printIndex = 0;

            _printDoc = new PrintDocument();
            _printDoc.EndPrint -= PrintDoc_EndPrint_Dispose;
            _printDoc.EndPrint += PrintDoc_EndPrint_Dispose;
            _printDoc.DocumentName = "Cartes Partenaire - Plastification (A4) - Batch";
            _printDoc.DefaultPageSettings.Landscape = false;
            _printDoc.DefaultPageSettings.Margins = new Margins(20, 20, 20, 20);

            _printDoc.PrintPage -= PrintDoc_Plastification_PrintPage;
            _printDoc.PrintPage += PrintDoc_Plastification_PrintPage;

            using (var dlg = new PrintDialog())
            {
                dlg.Document = _printDoc;
                if (dlg.ShowDialog() == DialogResult.OK)
                    _printDoc.Print();
            }
        }

        private void PrintDoc_EndPrint_Dispose(object sender, PrintEventArgs e)
        {
            try
            {
                if (_printDoc != null)
                {
                    _printDoc.PrintPage -= PrintDoc_Plastification_PrintPage;
                    _printDoc.PrintPage -= PrintDoc_PVC_PrintPage;
                    _printDoc.EndPrint -= PrintDoc_EndPrint_Dispose;
                    _printDoc.Dispose();
                    _printDoc = null;
                }
            }
            catch { }
        }

        private void PrintDoc_Plastification_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle page = e.MarginBounds;

            int cols = 2;
            int rows = 4;

            int cellW = page.Width / cols;
            int cellH = page.Height / rows;

            int maxW = (int)(cellW * 0.95);
            int maxH = (int)(cellH * 0.92);

            int cardW = maxW;
            int cardH = (int)(cardW / 1.593);
            if (cardH > maxH)
            {
                cardH = maxH;
                cardW = (int)(cardH * 1.593);
            }

            int idx = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (idx >= 8) break;

                    int x = page.Left + c * cellW + (cellW - cardW) / 2;
                    int y = page.Top + r * cellH + (cellH - cardH) / 2;

                    Rectangle card = new Rectangle(x, y, cardW, cardH);

                    if (_printIndex == 0)
                    {
                        var info = _cardsToPrint[idx];
                        if (info != null)
                            DrawPartenaireCardRecto(e.Graphics, card, info);
                        else
                            DrawCardVide(e.Graphics, card);
                    }
                    else
                    {
                        DrawPartenaireCardVerso(e.Graphics, card);
                    }

                    idx++;
                }
            }

            if (_printIndex == 0)
            {
                _printIndex = 1;
                e.HasMorePages = true;
            }
            else
            {
                _printIndex = 0;
                e.HasMorePages = false;
            }
        }

        // ===================== IMPRESSION PVC (1 carte) =====================
        private void PrintPartenairePVC()
        {
            if (!TryGetSelectedPartenaireFromGrid(out int id, out string codeCarte)) return;

            var info = LoadPartenaireCardInfo(id);
            if (info == null || string.IsNullOrWhiteSpace(info.CodeCarte))
            {
                MessageBox.Show("Impossible de charger le partenaire / CodeCarte.");
                return;
            }

            _cardsToPrint.Clear();
            _cardsToPrint.Add(info);

            _printIndex = 0;

            _printDoc = new PrintDocument();
            _printDoc.EndPrint -= PrintDoc_EndPrint_Dispose;
            _printDoc.EndPrint += PrintDoc_EndPrint_Dispose;
            _printDoc.DocumentName = "Carte Partenaire - PVC";
            _printDoc.DefaultPageSettings.Landscape = true;
            _printDoc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

            _printDoc.PrintPage -= PrintDoc_PVC_PrintPage;
            _printDoc.PrintPage += PrintDoc_PVC_PrintPage;

            using (var dlg = new PrintDialog())
            {
                dlg.Document = _printDoc;
                if (dlg.ShowDialog() == DialogResult.OK)
                    _printDoc.Print();
            }
        }

        private void PrintDoc_PVC_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var info = _cardsToPrint[0];
            Rectangle page = e.MarginBounds;

            int cardW = page.Width;
            int cardH = (int)(cardW / 1.593);

            if (cardH > page.Height)
            {
                cardH = page.Height;
                cardW = (int)(cardH * 1.593);
            }

            int x = page.Left + (page.Width - cardW) / 2;
            int y = page.Top + (page.Height - cardH) / 2;

            Rectangle card = new Rectangle(x, y, cardW, cardH);

            if (_printIndex == 0)
            {
                DrawPartenaireCardRecto(e.Graphics, card, info);
                _printIndex++;
                e.HasMorePages = true;
            }
            else
            {
                DrawPartenaireCardVerso(e.Graphics, card);
                e.HasMorePages = false;
                _printIndex = 0;
            }
        }


        private void AddPartenaire()
        {
            string nom = Prompt("Nom partenaire :", "Ajouter Partenaire");
            if (string.IsNullOrWhiteSpace(nom)) return;

            string tel = Prompt("Téléphone (optionnel) :", "Ajouter Partenaire");
            string email = Prompt("Email (optionnel) :", "Ajouter Partenaire");
            string adr = Prompt("Adresse (optionnel) :", "Ajouter Partenaire");

            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(@"
INSERT INTO Partenaire(Nom,Telephone,Email,Adresse,CodeCarte,SoldePromo,Actif)
VALUES(@n,@t,@e,@a,NULL,0,1);", cn))
                    {
                        cmd.Parameters.Add("@n", SqlDbType.NVarChar, 120).Value = nom.Trim();
                        cmd.Parameters.Add("@t", SqlDbType.NVarChar, 30).Value =
                            string.IsNullOrWhiteSpace(tel) ? (object)DBNull.Value : tel.Trim();
                        cmd.Parameters.Add("@e", SqlDbType.NVarChar, 120).Value =
                            string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email.Trim();
                        cmd.Parameters.Add("@a", SqlDbType.NVarChar, 250).Value =
                            string.IsNullOrWhiteSpace(adr) ? (object)DBNull.Value : adr.Trim();

                        cmd.ExecuteNonQuery();
                    }
                }

                LoadPartenaires();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ajout partenaire :\n" + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void EditSelected()
        {
            int id = GetSelectedId();
            if (id <= 0) { MessageBox.Show("Sélectionne un partenaire."); return; }

            string nomOld = (_dgv.CurrentRow.Cells["Nom"]?.Value ?? "").ToString();
            string telOld = (_dgv.CurrentRow.Cells["Telephone"]?.Value ?? "").ToString();
            string emailOld = (_dgv.CurrentRow.Cells["Email"]?.Value ?? "").ToString();
            string adrOld = (_dgv.CurrentRow.Cells["Adresse"]?.Value ?? "").ToString();

            string nom = Prompt("Nom partenaire :", "Modifier Partenaire", nomOld);
            if (string.IsNullOrWhiteSpace(nom)) return;

            string tel = Prompt("Téléphone (optionnel) :", "Modifier Partenaire", telOld);
            string email = Prompt("Email (optionnel) :", "Modifier Partenaire", emailOld);
            string adr = Prompt("Adresse (optionnel) :", "Modifier Partenaire", adrOld);

            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(@"
UPDATE Partenaire
SET Nom=@n, Telephone=@t, Email=@e, Adresse=@a
WHERE IdPartenaire=@id;", cn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@n", SqlDbType.NVarChar, 120).Value = nom.Trim();
                        cmd.Parameters.Add("@t", SqlDbType.NVarChar, 30).Value =
                            string.IsNullOrWhiteSpace(tel) ? (object)DBNull.Value : tel.Trim();
                        cmd.Parameters.Add("@e", SqlDbType.NVarChar, 120).Value =
                            string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email.Trim();
                        cmd.Parameters.Add("@a", SqlDbType.NVarChar, 250).Value =
                            string.IsNullOrWhiteSpace(adr) ? (object)DBNull.Value : adr.Trim();

                        cmd.ExecuteNonQuery();
                    }
                }

                LoadPartenaires();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur modification :\n" + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void DeleteSelected()
        {
            int id = GetSelectedId();
            if (id <= 0) { MessageBox.Show("Sélectionne un partenaire."); return; }

            if (MessageBox.Show("Désactiver ce partenaire (soft-delete) ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(@"
UPDATE Partenaire
SET Actif = 0
WHERE IdPartenaire=@id;", cn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadPartenaires();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur désactivation :\n" + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateOrShowCard()
        {
            int id = GetSelectedId();
            if (id <= 0) { MessageBox.Show("Sélectionne un partenaire."); return; }

            using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                cn.Open();

                string existing = "";
                using (var cmd = new SqlCommand("SELECT ISNULL(CodeCarte,'') FROM Partenaire WHERE IdPartenaire=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    existing = (cmd.ExecuteScalar() ?? "").ToString().Trim();
                }

                if (!string.IsNullOrWhiteSpace(existing))
                {
                    MessageBox.Show("Carte partenaire : " + existing);
                    return;
                }

                string code = BuildCodeCartePartenaire(id);

                using (var cmd = new SqlCommand(@"
UPDATE Partenaire
SET CodeCarte=@c
WHERE IdPartenaire=@id
  AND (CodeCarte IS NULL OR LTRIM(RTRIM(CodeCarte))='');", cn))
                {
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 20).Value = code;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ Carte générée : " + code);
            }

            LoadPartenaires();
        }

        private void OpenPromoManager()
        {
            try
            {
                int id = GetSelectedId();
                if (id <= 0)
                {
                    MessageBox.Show("Sélectionne un partenaire dans la liste (clique sur une ligne).");
                    return;
                }

                using (var f = new FormPromoPartenaireManager(id))
                {
                    f.StartPosition = FormStartPosition.CenterParent;
                    f.ShowDialog(this);
                }

                RefreshAndReselect(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "ERREUR ouverture Solde/Retraits :\n\n" + ex.Message + "\n\n" + ex.StackTrace,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error
                );
            }
        }


        // --- Prompt helper (avec valeur par défaut)
        private static string Prompt(string text, string caption, string defaultValue = "")
        {
            using (Form f = new Form())
            {
                f.Text = caption;
                f.Width = 420;
                f.Height = 170;
                f.StartPosition = FormStartPosition.CenterParent;

                Label lbl = new Label { Left = 10, Top = 10, Width = 380, Text = text };
                TextBox tb = new TextBox { Left = 10, Top = 35, Width = 380, Text = defaultValue ?? "" };
                Button ok = new Button { Text = "OK", Left = 230, Width = 80, Top = 75, DialogResult = DialogResult.OK };
                Button cancel = new Button { Text = "Annuler", Left = 310, Width = 80, Top = 75, DialogResult = DialogResult.Cancel };

                f.Controls.Add(lbl);
                f.Controls.Add(tb);
                f.Controls.Add(ok);
                f.Controls.Add(cancel);
                f.AcceptButton = ok;
                f.CancelButton = cancel;

                return f.ShowDialog() == DialogResult.OK ? tb.Text : null;
            }
        }
    }
}
