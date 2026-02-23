using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DocumentFormat.OpenXml.VariantTypes;
using ZXing;
using ZXing.Common;
using BoutiqueRebuildFixed.Models;
using BoutiqueRebuildFixed.Security;

namespace BoutiqueRebuildFixed
{

    public partial class FormEmployes : Form
    {

        private ContextMenuStrip _menuEmp;
        private Color _empBase = Color.FromArgb(10, 45, 30);      // Vert foncé
        private Color _empAccent = Color.FromArgb(20, 90, 55);    // Vert accent
        private Color _empInk = Color.White;
        private PrintDocument _printDoc = null;
        private int _printIndex = 0;
        private List<EmployeCardInfo> _cardsToPrint = new List<EmployeCardInfo>();
        private TextBox _txtCodeCarteEmploye;
        private Button _btnGenererCodeCarte;
        private enum FiltreEmployes { Actifs, Inactifs, Tous }
        private FiltreEmployes _filtreEmp = FiltreEmployes.Actifs;
        private Button _btnReactiver;
        private Button _btnDesactiver;
        private Panel _panelActionsEmp;

        public FormEmployes()
        {
            InitializeComponent();

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;

            this.Load += FormEmployes_Load;

            RafraichirTheme();
            FixColorsAfterTheme();

            // ✅ D'abord créer les boutons qui dépendent de btnChangerPhoto
            AddPrintButtonsUnderChangerPhoto();

            // ✅ Boutons Désactiver / Réactiver alignés avec txt + bouton Générer
            _btnDesactiver = new Button
            {
                Name = "btnDesactiverEmploye",
                Text = "🚫 Désactiver",
                Height = _btnGenererCodeCarte.Height,
                Width = 150
            };
            _btnDesactiver.Click += btnSupprimer_Click;

            _btnReactiver = new Button
            {
                Name = "btnReactiverEmploye",
                Text = "✅ Réactiver",
                Height = _txtCodeCarteEmploye.Height,
                Width = 150
            };
            _btnReactiver.Click += btnReactiver_Click;

            // ✅ Même colonne à droite des 2 contrôles (txt + bouton)
            int leftBtn = _btnGenererCodeCarte.Right + 10;

            // Alignements
            _btnDesactiver.Left = leftBtn;
            _btnDesactiver.Top = _btnGenererCodeCarte.Top;        // même ligne que Générer

            _btnReactiver.Left = leftBtn;
            _btnReactiver.Top = _txtCodeCarteEmploye.Top;        // même ligne que TextBox

            // ✅ Ne pas dépasser la photo
            int maxRight = picPhoto.Left - 10;
            int maxWidth = maxRight - leftBtn;
            if (maxWidth < 120) maxWidth = 120;

            _btnDesactiver.Width = Math.Min(_btnDesactiver.Width, maxWidth);
            _btnReactiver.Width = Math.Min(_btnReactiver.Width, maxWidth);

            // ✅ Important: même parent que les contrôles
            var parent = _btnGenererCodeCarte.Parent;
            parent.Controls.Add(_btnDesactiver);
            parent.Controls.Add(_btnReactiver);

            _btnDesactiver.BringToFront();
            _btnReactiver.BringToFront();

            // ✅ Visibilité gérée par SelectionChanged (par défaut)
            _btnDesactiver.Visible = true;   // employé actif au démarrage
            _btnReactiver.Visible = false;




            // ✅ Ensuite seulement, menu contextuel (utilise btnSupprimer_Click / btnReactiver_Click)
            AjouterMenuContextuel();

            

        }

        private class EmployeCardInfo
        {
            public int IdEmploye { get; set; }
            public string NomComplet { get; set; }
            public string Matricule { get; set; }
            public string Poste { get; set; }
            public string Departement { get; set; }
            public string Sexe { get; set; }
            public DateTime? DateNaissance { get; set; }
            public string Telephone { get; set; }
            public string Email { get; set; }
            public string Adresse { get; set; }
            public DateTime? DateEmbauche { get; set; }

            public string CodeCarte { get; set; }   // EMP-000123
            public string PhotoPath { get; set; }
        }

        // ==============================
        //  CARTES EMPLOYES - HELPERS UI
        // ==============================

        // Logo (même que Clients)
        private readonly string _logoPath = @"D:\ZAIRE\LOGO1.png";

        

        // ------------- Helpers dessin -------------
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

        private void DrawDivider(Graphics g, int x, int y1, int y2)
        {
            using (var p = new Pen(Color.FromArgb(70, 255, 255, 255), 1f))
                g.DrawLine(p, x, y1, x, y2);
        }

        private void DrawLogoSafe(Graphics g, Rectangle r)
        {
            if (!File.Exists(_logoPath)) return;

            try
            {
                using (var img = Image.FromFile(_logoPath))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(img, r);
                }
            }
            catch { }
        }

        private void DrawLogoPattern(Graphics g, Rectangle area, int logoW = 44, int logoH = 30, int gapX = 22, int gapY = 18, int alpha = 24)
        {
            if (!File.Exists(_logoPath)) return;

            try
            {
                using (var img = Image.FromFile(_logoPath))
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
                            g.DrawImage(img, r, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                }
            }
            catch { }
        }

        private void GetScopeEmp(out int? idEntreprise, out int? idMagasin)
        {
            idEntreprise = null;
            idMagasin = null;

            // Si tu as ConfigSysteme.IdEntreprise / IdMagasin
            try
            {
                // adapte si tes propriétés ont un autre nom
                idEntreprise = ConfigSysteme.IdEntreprise;
                idMagasin = ConfigSysteme.IdMagasin;
            }
            catch
            {
                // sinon laisse null (ça compile quand même)
            }
        }

        private void AddPrintButtonsUnderChangerPhoto()
        {
            // Ajuste ces valeurs selon ton layout
            int x = btnChangerPhoto.Left;
            int y = btnChangerPhoto.Bottom + 8;
            int w = btnChangerPhoto.Width;
            int h = btnChangerPhoto.Height;

            var btnPVC = new Button
            {
                Name = "btnImprimerPVC",
                Text = "🖨 Imprimer PVC",
                Left = x,
                Top = y,
                Width = w,
                Height = h
            };
            btnPVC.Click += btnImprimerEmployePVC_Click;

            var btnPlast = new Button
            {
                Name = "btnImprimerPlast",
                Text = "🖨 Imprimer Plast",
                Left = x,
                Top = btnPVC.Bottom + 6,
                Width = w,
                Height = h
            };
            btnPlast.Click += btnImprimerEmployePlast_Click;

            this.Controls.Add(btnPVC);
            this.Controls.Add(btnPlast);

            // TextBox Code Carte
            _txtCodeCarteEmploye = new TextBox
            {
                Name = "txtCodeCarteEmploye",
                Left = btnChangerPhoto.Left,
                Top = btnPlast.Bottom + 8,
                Width = btnChangerPhoto.Width,
                ReadOnly = true
            };
            this.Controls.Add(_txtCodeCarteEmploye);

            // Bouton générer code
            _btnGenererCodeCarte = new Button
            {
                Name = "btnGenererCodeCarte",
                Text = "⚙ Générer Code Carte",
                Left = btnChangerPhoto.Left,
                Top = _txtCodeCarteEmploye.Bottom + 6,
                Width = btnChangerPhoto.Width,
                Height = btnChangerPhoto.Height
            };
            _btnGenererCodeCarte.Click += btnGenererCodeCarte_Click;
            this.Controls.Add(_btnGenererCodeCarte);
        }

        // Dans FormEmployes_Load(), appelle :
        

        private Rectangle DrawWhiteSafeBox(Graphics g, Rectangle inner, string title, int boxWidth)
        {
            int topOffset = 64;
            int bottomMargin = 92;

            int boxH = inner.Height - (topOffset + bottomMargin);
            if (boxH < 120) boxH = 120;

            Rectangle box = new Rectangle(
                inner.Right - boxWidth,
                inner.Top + topOffset,
                boxWidth,
                boxH
            );

            using (var br = new SolidBrush(Color.White))
                g.FillRectangle(br, box);

            using (var p = new Pen(Color.FromArgb(210, 210, 210), 1f))
                g.DrawRectangle(p, box);

            using (var f = new Font("Segoe UI", 8, FontStyle.Bold))
            using (var brT = new SolidBrush(Color.FromArgb(60, 60, 60)))
                g.DrawString(title ?? "", f, brT, box.Left + 10, box.Top + 8);

            return box;
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

        // ------------- QR / Barcode -------------
        private Bitmap GenerateQr(string text, int size = 240)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = size,
                    Height = size,
                    Margin = 1
                }
            };
            return writer.Write(text);
        }

        private void ConfigurerTxtPin()
        {
            if (txtPin == null) return;

            txtPin.MaxLength = 6;                 // recommandé
            txtPin.UseSystemPasswordChar = true;  // masque
            txtPin.ShortcutsEnabled = false;      // pas de copier/coller
            txtPin.TextAlign = HorizontalAlignment.Center;

            txtPin.KeyPress -= TxtPin_KeyPress;
            txtPin.KeyPress += TxtPin_KeyPress;
        }

        private void TxtPin_KeyPress(object sender, KeyPressEventArgs e)
        {
            // autoriser backspace
            if (e.KeyChar == (char)Keys.Back) return;

            // autoriser seulement chiffres
            if (!char.IsDigit(e.KeyChar))
                e.Handled = true;
        }


        private Bitmap GenerateCode128(string text, int width = 900, int height = 200)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 1,
                    PureBarcode = true
                }
            };
            return writer.Write(text);
        }

        private void FormEmployes_Load(object sender, EventArgs e)
        {

            // ✅ Remplir combobox
            cmbPoste.Items.Clear();
            cmbPoste.Items.AddRange(new string[] {
                "Directeur","Vendeur","Caissier","Superviseur","Livreur",
                "Photographe","Sécurité","Informaticien","Gerant","DirecteurMarketing"
            });

            cmbDepartement.Items.Clear();
            cmbDepartement.Items.AddRange(new string[] {
                "Ventes","Administration","Finance","RH","Marketing","Informatique","Depot"
            });

            cmbSexe.Items.Clear();
            cmbSexe.Items.AddRange(new string[] { "Homme", "Femme" });

            dtpDateNaissance.Value = DateTime.Now.AddYears(-25);
            dtpDateEmbauche.Value = DateTime.Now;

            // ✅ Charger + config dgv après
            ChargerEmployes();
            ConfigurerDgv();
            ConfigurerTxtPin();

            RafraichirLangue();
            RafraichirTheme();

            // ✅ option: selection change pour remplir champs
            dgvEmployes.SelectionChanged += dgvEmployes_SelectionChanged;
        }
        private void RafraichirLangue() => ConfigSysteme.AppliquerTraductions(this);
        private void RafraichirTheme() => ConfigSysteme.AppliquerTheme(this);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // ✅ éviter fuites mémoire
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }

        private EmployeCardInfo LoadEmployeCardInfo(int idEmploye)
        {
            string S(IDataRecord r, int i)
            {
                if (r.IsDBNull(i)) return "";
                return Convert.ToString(r.GetValue(i))?.Trim() ?? "";
            }

            DateTime? D(IDataRecord r, int i)
            {
                if (r.IsDBNull(i)) return null;
                // gère Date, DateTime, string convertible, etc.
                return Convert.ToDateTime(r.GetValue(i));
            }

            using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP 1
    ID_Employe,
    LTRIM(RTRIM(ISNULL(Nom,''))) 
      + CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(Prenom,''))), '') IS NULL THEN '' ELSE ' ' + LTRIM(RTRIM(ISNULL(Prenom,''))) END AS NomComplet,
    Matricule,
    Poste,
    Departement,
    Sexe,
    DateNaissance,
    Telephone,
    Email,
    Adresse,
    DateEmbauche,
    CodeCarteEmploye,
    PhotoPath
FROM Employes
WHERE ID_Employe=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmploye;

                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        return new EmployeCardInfo
                        {
                            IdEmploye = r.IsDBNull(0) ? 0 : Convert.ToInt32(r.GetValue(0)),
                            NomComplet = S(r, 1),
                            Matricule = S(r, 2),
                            Poste = S(r, 3),
                            Departement = S(r, 4),
                            Sexe = S(r, 5),
                            DateNaissance = D(r, 6),
                            Telephone = S(r, 7),
                            Email = S(r, 8),
                            Adresse = S(r, 9),
                            DateEmbauche = D(r, 10),
                            CodeCarte = S(r, 11),
                            PhotoPath = S(r, 12)
                        };
                    }
                }
            }
        }

        private bool TryGetValidPin(out string pin)
        {
            pin = (txtPin?.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(pin))
                return true; // ✅ PIN optionnel à la création

            if (pin.Length != 6)
            {
                MessageBox.Show("Le PIN doit contenir exactement 6 chiffres.");
                return false;
            }

            if (!pin.All(char.IsDigit))
            {
                MessageBox.Show("Le PIN doit contenir seulement des chiffres.");
                return false;
            }

            return true;
        }

        private List<int> GetSelectedEmployeIds(int max = 8)
        {
            var ids = new List<int>();

            if (dgvEmployes.SelectedRows != null && dgvEmployes.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow r in dgvEmployes.SelectedRows)
                {
                    object v = r.Cells["ID_Employe"]?.Value;
                    if (v != null && int.TryParse(v.ToString(), out int id) && id > 0)
                        ids.Add(id);
                    if (ids.Count >= max) break;
                }
            }
            else if (dgvEmployes.CurrentRow != null)
            {
                object v = dgvEmployes.CurrentRow.Cells["ID_Employe"]?.Value;
                if (v != null && int.TryParse(v.ToString(), out int id) && id > 0)
                    ids.Add(id);
            }

            return ids.Distinct().Take(max).ToList();
        }

        private string Safe(string s) => string.IsNullOrWhiteSpace(s) ? "-" : s.Trim();

        private void DrawFramedPhoto(Graphics g, Rectangle r, string path, float biasY = -0.18f)
        {
            // Fond blanc + cadre
            using (var br = new SolidBrush(Color.White))
                g.FillRectangle(br, r);

            using (var pen = new Pen(Color.FromArgb(210, 210, 210), 2f))
            {
                pen.Alignment = PenAlignment.Inset;
                g.DrawRectangle(pen, r);
            }

            Rectangle inner = Rectangle.Inflate(r, -3, -3);

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                using (var br = new SolidBrush(Color.FromArgb(240, 240, 240)))
                    g.FillRectangle(br, inner);

                using (var f = new Font("Segoe UI", 9, FontStyle.Italic))
                using (var brT = new SolidBrush(Color.FromArgb(120, 120, 120)))
                {
                    var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("PHOTO", f, brT, inner, fmt);
                }
                return;
            }

            try
            {
                using (var img = Image.FromFile(path))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    // Cover (remplit sans déformer)
                    float ratio = Math.Max((float)inner.Width / img.Width, (float)inner.Height / img.Height);
                    int w = (int)(img.Width * ratio);
                    int h = (int)(img.Height * ratio);

                    int x = inner.Left + (inner.Width - w) / 2;

                    // ✅ Décale vers le HAUT (biasY négatif = on monte)
                    int extraH = h - inner.Height;
                    int y = inner.Top - (int)(extraH * Math.Abs(biasY)); // ex: -0.18 => montre + de cheveux

                    // Clamp (évite de sortir trop)
                    int minY = inner.Top - extraH; // tout en haut
                    int maxY = inner.Top;          // tout en bas
                    if (y < minY) y = minY;
                    if (y > maxY) y = maxY;

                    g.SetClip(inner);
                    g.DrawImage(img, new Rectangle(x, y, w, h));
                    g.ResetClip();
                }
            }
            catch
            {
                // ignore
            }
        }



        private void btnImprimerEmployePVC_Click(object sender, EventArgs e)
        {
            int id = GetSelectedEmployeId();
            if (id <= 0) { MessageBox.Show("Sélectionne un employé."); return; }

            var info = LoadEmployeCardInfo(id);
            if (info == null) { MessageBox.Show("Employé introuvable."); return; }
            if (string.IsNullOrWhiteSpace(info.CodeCarte))
            {
                MessageBox.Show("Ce employé n'a pas de CodeCarteEmploye.");
                return;
            }

            _cardsToPrint.Clear();
            _cardsToPrint.Add(info);
            _printIndex = 0;

            _printDoc = new PrintDocument();
            _printDoc.DocumentName = "Carte Employe - PVC";
            _printDoc.DefaultPageSettings.Landscape = true;
            _printDoc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

            _printDoc.PrintPage -= PrintDoc_EmployePVC_PrintPage;
            _printDoc.PrintPage += PrintDoc_EmployePVC_PrintPage;

            using (var dlg = new PrintDialog())
            {
                dlg.Document = _printDoc;
                if (dlg.ShowDialog() == DialogResult.OK)
                    _printDoc.Print();
            }
        }

        private void PrintDoc_EmployePVC_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

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
                DrawEmployeCardRecto(e.Graphics, card, info);
                _printIndex = 1;
                e.HasMorePages = true;
            }
            else
            {
                DrawEmployeCardVerso(e.Graphics, card);
                _printIndex = 0;
                e.HasMorePages = false;
            }
        }

        private void btnImprimerEmployePlast_Click(object sender, EventArgs e)
        {
            var ids = GetSelectedEmployeIds(8);
            if (ids.Count == 0)
            {
                MessageBox.Show("Sélectionne 1 à 8 employés (Ctrl+Click / Shift+Click).");
                return;
            }

            _cardsToPrint.Clear();
            foreach (var id in ids)
            {
                var info = LoadEmployeCardInfo(id);
                if (info == null) { MessageBox.Show("Employé introuvable ID=" + id); return; }
                if (string.IsNullOrWhiteSpace(info.CodeCarte))
                {
                    MessageBox.Show($"L'employé '{info.NomComplet}' n'a pas de CodeCarteEmploye.");
                    return;
                }
                _cardsToPrint.Add(info);
            }
            while (_cardsToPrint.Count < 8) _cardsToPrint.Add(null);

            _printIndex = 0;
            _printDoc = new PrintDocument();
            _printDoc.DocumentName = "Cartes Employes - Plastification A4";
            _printDoc.DefaultPageSettings.Landscape = false;
            _printDoc.DefaultPageSettings.Margins = new Margins(20, 20, 20, 20);

            _printDoc.PrintPage -= PrintDoc_EmployePlast_PrintPage;
            _printDoc.PrintPage += PrintDoc_EmployePlast_PrintPage;

            using (var dlg = new PrintDialog())
            {
                dlg.Document = _printDoc;
                if (dlg.ShowDialog() == DialogResult.OK)
                    _printDoc.Print();
            }
        }

        private void PrintDoc_EmployePlast_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle page = e.MarginBounds;

            int cols = 2, rows = 4;
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
                        if (info != null) DrawEmployeCardRecto(e.Graphics, card, info);
                        else DrawCardVide(e.Graphics, card); // méthode de Clients
                    }
                    else
                    {
                        DrawEmployeCardVerso(e.Graphics, card);
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

        private void DrawEmployeCardRecto(Graphics g, Rectangle card, EmployeCardInfo info)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawShadow(g, card, 6, 18);
            FillLinearGradient(g, card, _empBase, _empAccent, 35f);

            using (var pen = new Pen(Color.FromArgb(160, 255, 255, 255), 2f))
            {
                pen.Alignment = PenAlignment.Inset;
                g.DrawRectangle(pen, card);
            }

            float scale = card.Width / 860f;
            if (scale < 0.65f) scale = 0.65f;

            int pad = (int)(18 * scale);
            Rectangle inner = Rectangle.Inflate(card, -pad, -pad);

            // ====== Layout: colonne droite (photo + scan) ======
            int rightW = (int)(inner.Width * 0.30f);
            rightW = Math.Max((int)(210 * scale), Math.Min((int)(300 * scale), rightW));

            Rectangle rightCol = new Rectangle(inner.Right - rightW, inner.Top, rightW, inner.Height);

            int headerH = (int)(64 * scale);
            Rectangle headerRect = new Rectangle(inner.Left, inner.Top, inner.Width, headerH);

            // Logo à gauche dans l'entête
            Rectangle logoRect = new Rectangle(inner.Left, inner.Top + (int)(6 * scale), (int)(96 * scale), (int)(56 * scale));
            DrawLogoSafe(g, logoRect);

            // Photo à droite en face de l'entête
            int photoH = (int)(155 * scale);
            int photoW = rightW - (int)(10 * scale);
            Rectangle photoRect = new Rectangle(rightCol.Left + (int)(5 * scale), inner.Top + (int)(6 * scale), photoW, photoH);
            DrawFramedPhoto(g, photoRect, info?.PhotoPath, biasY: -0.28f);

            // Titre centré dans l'entête (entre logo et colonne droite)
            int titleLeft = logoRect.Right + (int)(10 * scale);
            int titleRight = rightCol.Left - (int)(10 * scale);
            Rectangle titleRect = Rectangle.FromLTRB(titleLeft, headerRect.Top, titleRight, headerRect.Bottom);

            using (var fBrand = new Font("Segoe UI", 14f * scale, FontStyle.Bold))
            using (var fSub = new Font("Segoe UI", 10.5f * scale, FontStyle.Bold))
            using (var brText = new SolidBrush(_empInk))
            using (var brSoft = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
            {
                var fmtC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("ZAIRE MODE SARL", fBrand, brText, titleRect, fmtC);

                Rectangle sub = new Rectangle(titleRect.Left, titleRect.Top + (int)(34 * scale), titleRect.Width, (int)(26 * scale));
                g.DrawString("CARTE DE SERVICE", fSub, brSoft, sub, fmtC);
            }

            // Ligne fine sous entête
            using (var p = new Pen(Color.FromArgb(80, 255, 255, 255), 1f))
                g.DrawLine(p, inner.Left, headerRect.Bottom + (int)(4 * scale), inner.Right, headerRect.Bottom + (int)(4 * scale));

            // ====== Zone texte (gauche) ======
            Rectangle textArea = Rectangle.FromLTRB(
                inner.Left,
                headerRect.Bottom + (int)(12 * scale),
                rightCol.Left - (int)(10 * scale),
                inner.Bottom - (int)(10 * scale)
            );

            // ====== Zone scan (droite bas) réduite ======
            int scanH = (int)(inner.Height * 0.42f);
            scanH = Math.Max((int)(150 * scale), Math.Min((int)(220 * scale), scanH));

            Rectangle scanBox = new Rectangle(
                rightCol.Left + (int)(5 * scale),
                inner.Bottom - scanH,
                rightCol.Width - (int)(10 * scale),
                scanH - (int)(6 * scale)
            );

            // Box blanche "SCAN"
            using (var br = new SolidBrush(Color.White))
                g.FillRectangle(br, scanBox);

            using (var pen2 = new Pen(Color.FromArgb(210, 210, 210), 1.5f))
            {
                pen2.Alignment = PenAlignment.Inset;
                g.DrawRectangle(pen2, scanBox);
            }

            using (var f = new Font("Segoe UI", 8f * scale, FontStyle.Bold))
            using (var brT = new SolidBrush(Color.FromArgb(70, 70, 70)))
                g.DrawString("SCAN", f, brT, scanBox.Left + (int)(10 * scale), scanBox.Top + (int)(8 * scale));

            // ====== Données ======
            string code = Safe(info?.CodeCarte);
            string nom = Safe(info?.NomComplet);
            string matricule = Safe(info?.Matricule);
            string poste = Safe(info?.Poste);
            string dept = Safe(info?.Departement);
            string sexe = Safe(info?.Sexe);
            string tel = Safe(info?.Telephone);
            string email = Safe(info?.Email);
            string adr = Safe(info?.Adresse);

            string dn = info?.DateNaissance.HasValue == true ? info.DateNaissance.Value.ToString("dd/MM/yyyy") : "-";
            string de = info?.DateEmbauche.HasValue == true ? info.DateEmbauche.Value.ToString("dd/MM/yyyy") : "-";

            // ====== Texte multi-lignes ======
            using (var fName = new Font("Segoe UI", 12.5f * scale, FontStyle.Bold))
            using (var fLbl = new Font("Segoe UI", 9.6f * scale, FontStyle.Regular))
            using (var fCode = new Font("Consolas", 10.5f * scale, FontStyle.Bold))
            using (var brMain = new SolidBrush(_empInk))
            using (var brSoft = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
            {
                int y = textArea.Top;

                // NOM
                string nomShort = nom;
                if (nomShort.Length > 40) nomShort = nomShort.Substring(0, 40) + "...";
                g.DrawString(nomShort, fName, brMain, new RectangleF(textArea.Left, y, textArea.Width, 999));
                y += (int)(26 * scale);

                // Infos
                g.DrawString($"Matricule : {matricule}", fLbl, brSoft, textArea.Left, y); y += (int)(18 * scale);
                g.DrawString($"Poste     : {poste}", fLbl, brSoft, textArea.Left, y); y += (int)(18 * scale);
                g.DrawString($"Dépt      : {dept}", fLbl, brSoft, textArea.Left, y); y += (int)(18 * scale);
                g.DrawString($"Sexe      : {sexe}", fLbl, brSoft, textArea.Left, y); y += (int)(18 * scale);
                g.DrawString($"Né(e) le  : {dn}", fLbl, brSoft, textArea.Left, y); y += (int)(18 * scale);

                g.DrawString($"Téléphone : {tel}", fLbl, brSoft, textArea.Left, y); y += (int)(18 * scale);
                g.DrawString($"Email     : {email}", fLbl, brSoft, textArea.Left, y); y += (int)(18 * scale);

                // Adresse (peut être longue)
                RectangleF adrRect = new RectangleF(textArea.Left, y, textArea.Width, (int)(42 * scale));
                g.DrawString($"Adresse   : {adr}", fLbl, brSoft, adrRect);
                y += (int)(44 * scale);

                g.DrawString($"Embauché(e): {de}", fLbl, brSoft, textArea.Left, y); y += (int)(20 * scale);

                // Code carte (important)
                g.DrawString($"Code : {code}", fCode, brMain, textArea.Left, y); y += (int)(18 * scale);

                // Bas de carte
                int yInfo = inner.Bottom - (int)(22 * scale);
                g.DrawString("• Accès interne • Présenter à l'entrée •", fLbl, brSoft, inner.Left, yInfo);
            }

            // ====== QR + Barcode (réduits) ======
            if (!string.IsNullOrWhiteSpace(code) && code != "-")
            {
                int qrSize = Math.Max((int)(78 * scale), Math.Min((int)(92 * scale), scanBox.Width - (int)(20 * scale)));
                Rectangle qrRect = new Rectangle(
                    scanBox.Left + (scanBox.Width - qrSize) / 2,
                    scanBox.Top + (int)(28 * scale),
                    qrSize,
                    qrSize
                );

                using (var qr = GenerateQr(code, 220))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(qr, qrRect);
                }

                int barH = (int)(30 * scale);
                Rectangle barRect = new Rectangle(
                    scanBox.Left + (int)(10 * scale),
                    qrRect.Bottom + (int)(10 * scale),
                    scanBox.Width - (int)(20 * scale),
                    barH
                );

                using (var barcode = GenerateCode128(code, 900, 180))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(barcode, barRect);
                }

                using (var fMini = new Font("Segoe UI", 8f * scale, FontStyle.Bold))
                using (var br = new SolidBrush(Color.FromArgb(40, 40, 40)))
                {
                    var fmt = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(code, fMini, br,
                        new RectangleF(scanBox.Left, barRect.Bottom + (int)(2 * scale), scanBox.Width, (int)(16 * scale)), fmt);
                }
            }
            else
            {
                // Si pas de code
                using (var f = new Font("Segoe UI", 9f * scale, FontStyle.Italic))
                using (var br = new SolidBrush(Color.FromArgb(120, 120, 120)))
                {
                    var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("Aucun code", f, br, scanBox, fmt);
                }
            }
        }

        private void DrawEmployeCardVerso(Graphics g, Rectangle card)
        {
            // Simple : verso élégant + motif logo + cadre (comme clients), mais vert léger
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            FillLinearGradient(g, card, Color.White, Color.FromArgb(235, 245, 240), 90f);

            DrawLogoPattern(g, card, logoW: 52, logoH: 36, gapX: 26, gapY: 22, alpha: 35);

            using (var pen = new Pen(Color.FromArgb(180, 10, 45, 30), 3f))
            {
                pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                g.DrawRectangle(pen, card);
            }

            Rectangle band = new Rectangle(card.Left + 18, card.Top + (card.Height / 2) - 34, card.Width - 36, 68);
            using (var brBand = new SolidBrush(Color.FromArgb(140, 10, 45, 30)))
                g.FillRectangle(brBand, band);

            using (var f = new Font("Segoe UI Black", 16, FontStyle.Bold))
            using (var brText = new SolidBrush(Color.White))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("ZAIRE MODE SARL", f, brText, band, fmt);
            }

            Rectangle logoTop = new Rectangle(card.Left + 18, card.Top + 14, 90, 60);
            DrawLogoSafe(g, logoTop);
        }

        private void DrawRoundPhoto(Graphics g, Rectangle r, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                // placeholder
                using (var br = new SolidBrush(Color.FromArgb(90, 255, 255, 255)))
                    g.FillEllipse(br, r);
                return;
            }

            using (var img = Image.FromFile(path))
            using (var gp = new System.Drawing.Drawing2D.GraphicsPath())
            {
                gp.AddEllipse(r);
                g.SetClip(gp);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, r);
                g.ResetClip();

                using (var pen = new Pen(Color.FromArgb(200, 255, 255, 255), 2f))
                    g.DrawEllipse(pen, r);
            }
        }


        private void ChargerEmployes()
        {
            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();

                    string where = "";
                    if (_filtreEmp == FiltreEmployes.Actifs) where = "WHERE IsActif = 1";
                    else if (_filtreEmp == FiltreEmployes.Inactifs) where = "WHERE IsActif = 0";

                    string query = $@"
SELECT TOP (1000)
    ID_Employe,
    Nom,
    Prenom,
    Matricule,
    NomUtilisateur,             -- ✅ AJOUT
    Poste,
    Departement,
    Sexe,
    DateNaissance,
    Telephone,
    Email,
    Adresse,
    DateEmbauche,
    PhotoPath,
    CodeCarteEmploye,
    IsActif,
    CASE WHEN IsActif=1 THEN 'ACTIF' ELSE 'INACTIF' END AS Statut
FROM dbo.Employes
{where}
ORDER BY Nom, Prenom;";

                    var da = new SqlDataAdapter(query, cn);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvEmployes.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement employés : " + ex.Message);
            }
        }


        private void SetPictureNoLock(PictureBox pb, string path)
        {
            try
            {
                if (pb.Image != null)
                {
                    var old = pb.Image;
                    pb.Image = null;
                    old.Dispose();
                }

                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    pb.ImageLocation = null;
                    pb.Image = null;
                    return;
                }

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var img = Image.FromStream(fs))
                {
                    pb.Image = new Bitmap(img);
                }
                pb.ImageLocation = path;
            }
            catch
            {
                pb.ImageLocation = null;
                pb.Image = null;
            }
        }


        private void ConfigurerDgv()
        {
            // ✅ Lecture
            dgvEmployes.ReadOnly = true;
            dgvEmployes.AllowUserToAddRows = false;
            dgvEmployes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEmployes.MultiSelect = true;
            dgvEmployes.RowHeadersVisible = false;

            // ✅ Barres de défilement
            dgvEmployes.ScrollBars = ScrollBars.Both;

            // ✅ IMPORTANT : pour avoir la barre horizontale, éviter Fill
            dgvEmployes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvEmployes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            // ✅ Lisibilité
            dgvEmployes.DefaultCellStyle.Font = new Font("Segoe UI", 10.5f, FontStyle.Regular);
            dgvEmployes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            dgvEmployes.RowTemplate.Height = 32;
            dgvEmployes.ColumnHeadersHeight = 38;
            dgvEmployes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvEmployes.EnableHeadersVisualStyles = false;
            dgvEmployes.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvEmployes.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // ✅ Alternance
            dgvEmployes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvEmployes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);

            // ✅ Ajuster largeur des colonnes (tu peux adapter)
            void W(string col, int width)
            {
                if (dgvEmployes.Columns.Contains(col))
                    dgvEmployes.Columns[col].Width = width;
            }

            W("Nom", 140);
            W("Prenom", 140);
            W("Matricule", 120);
            W("NomUtilisateur", 170);  // ✅ AJOUT
            W("Poste", 120);
            W("Departement", 130);
            W("Sexe", 70);
            W("Telephone", 120);
            W("Email", 180);
            W("Adresse", 220);
            W("PhotoPath", 160);
            W("CodeCarteEmploye", 140);

            if (dgvEmployes.Columns.Contains("NomUtilisateur"))
                dgvEmployes.Columns["NomUtilisateur"].HeaderText = "Utilisateur";

            if (dgvEmployes.Columns.Contains("ID_Employe"))
                dgvEmployes.Columns["ID_Employe"].Visible = false;

            if (dgvEmployes.Columns.Contains("DateNaissance"))
            {
                dgvEmployes.Columns["DateNaissance"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvEmployes.Columns["DateNaissance"].Width = 110;
            }

            if (dgvEmployes.Columns.Contains("DateEmbauche"))
            {
                dgvEmployes.Columns["DateEmbauche"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvEmployes.Columns["DateEmbauche"].Width = 110;
            }

            // ✅ Permet à la barre horizontale d'apparaître facilement
            dgvEmployes.HorizontalScrollingOffset = 0;
            dgvEmployes.AutoSize = false;
            if (dgvEmployes.Columns.Contains("IsActif"))
                dgvEmployes.Columns["IsActif"].Visible = false;

            if (dgvEmployes.Columns.Contains("Statut"))
                dgvEmployes.Columns["Statut"].Width = 90;
        }

        private void dgvEmployes_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEmployes.DataSource == null) return;
            if (dgvEmployes.CurrentRow == null && (dgvEmployes.SelectedRows == null || dgvEmployes.SelectedRows.Count == 0)) return;

            var row = (dgvEmployes.SelectedRows.Count > 0) ? dgvEmployes.SelectedRows[0] : dgvEmployes.CurrentRow;
            if (row == null) return;

            // ✅ Si une colonne manque, on sort sans casser
            bool Has(string col) => dgvEmployes.Columns.Contains(col);

            if (Has("Nom")) txtNom.Text = row.Cells["Nom"]?.Value?.ToString() ?? "";
            if (Has("Prenom")) txtPrenom.Text = row.Cells["Prenom"]?.Value?.ToString() ?? "";
            if (Has("Matricule")) txtMatricule.Text = row.Cells["Matricule"]?.Value?.ToString() ?? "";
            if (Has("Poste")) cmbPoste.Text = row.Cells["Poste"]?.Value?.ToString() ?? "";
            if (Has("Departement")) cmbDepartement.Text = row.Cells["Departement"]?.Value?.ToString() ?? "";
            if (Has("Sexe")) cmbSexe.Text = row.Cells["Sexe"]?.Value?.ToString() ?? "";

            if (_txtCodeCarteEmploye != null && Has("CodeCarteEmploye"))
                _txtCodeCarteEmploye.Text = row.Cells["CodeCarteEmploye"]?.Value?.ToString() ?? "";

            if (txtNomUtilisateur != null && Has("NomUtilisateur"))
                txtNomUtilisateur.Text = row.Cells["NomUtilisateur"]?.Value?.ToString() ?? "";

            if (Has("DateNaissance") && DateTime.TryParse(row.Cells["DateNaissance"]?.Value?.ToString(), out DateTime dn))
                dtpDateNaissance.Value = dn;

            if (Has("DateEmbauche") && DateTime.TryParse(row.Cells["DateEmbauche"]?.Value?.ToString(), out DateTime de))
                dtpDateEmbauche.Value = de;

            if (Has("Telephone")) txtTelephone.Text = row.Cells["Telephone"]?.Value?.ToString() ?? "";
            if (Has("Email")) txtEmail.Text = row.Cells["Email"]?.Value?.ToString() ?? "";
            if (Has("Adresse")) rtbAdresse.Text = row.Cells["Adresse"]?.Value?.ToString() ?? "";

            if (Has("PhotoPath"))
            {
                string photo = row.Cells["PhotoPath"]?.Value?.ToString() ?? "";
                SetPictureNoLock(picPhoto, photo);
            }

            bool actif = true;
            if (Has("IsActif"))
            {
                var v = row.Cells["IsActif"]?.Value;
                if (v != null && v != DBNull.Value) actif = Convert.ToBoolean(v);
            }

            if (_btnDesactiver != null) _btnDesactiver.Visible = actif;
            if (_btnReactiver != null) _btnReactiver.Visible = !actif;
        }


        private void FixColorsAfterTheme()
        {
            // Exemple : un thème sombre
            Color bg = Color.FromArgb(20, 20, 20);
            Color fg = Color.White;

            foreach (Control c in this.Controls)
                ApplyColorsRecursive(c, bg, fg);

            // DGV : force
            dgvEmployes.BackgroundColor = Color.White;
            dgvEmployes.DefaultCellStyle.ForeColor = Color.Black;
            dgvEmployes.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        private void ApplyColorsRecursive(Control c, Color bg, Color fg)
        {
            // Ne force pas tout sur DGV ou autres contrôles spéciaux
            if (c is TextBox tb)
            {
                tb.BackColor = bg;
                tb.ForeColor = fg;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is ComboBox cb)
            {
                cb.BackColor = bg;
                cb.ForeColor = fg;
                cb.FlatStyle = FlatStyle.Flat;
            }
            else if (c is RichTextBox rtb)
            {
                rtb.BackColor = bg;
                rtb.ForeColor = fg;
            }
            else if (c is Label lbl)
            {
                lbl.ForeColor = fg;
            }

            foreach (Control child in c.Controls)
                ApplyColorsRecursive(child, bg, fg);
        }

        private void btnChangerPhoto_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    SetPictureNoLock(picPhoto, ofd.FileName);
                }
            }
        }

        private string BuildCodeCarteEmploye(int idEmploye) => "EMP-" + idEmploye.ToString("D6");



        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                // 0) Valider PIN (optionnel)
                if (!TryGetValidPin(out string pin)) return;

                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();

                    using (var tx = cn.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        int id = 0;

                        try
                        {
                            // ✅ 1) Déterminer NomUtilisateur (depuis txtNomUtilisateur si présent, sinon générer)
                            string nomUtilisateurInput = (txtNomUtilisateur?.Text ?? "").Trim();

                            string nomU;
                            if (!string.IsNullOrWhiteSpace(nomUtilisateurInput))
                            {
                                // nettoie un peu (optionnel)
                                nomU = nomUtilisateurInput.Trim();
                                // ✅ éviter doublons
                                nomU = EnsureUniqueNomUtilisateur(nomU);
                            }
                            else
                            {
                                // fallback auto
                                nomU = BuildNomUtilisateur(
                                    (txtMatricule.Text ?? "").Trim(),
                                    (txtNom.Text ?? "").Trim(),
                                    (txtPrenom.Text ?? "").Trim()
                                );
                            }

                            // 2) INSERT employé
                            string sql = @"
INSERT INTO dbo.Employes
(
    Nom, Prenom, Matricule, NomUtilisateur,
    Poste, Departement, Sexe, DateNaissance,
    Telephone, Email, Adresse, DateEmbauche, PhotoPath
)
VALUES
(
    @Nom, @Prenom, @Matricule, @NomUtilisateur,
    @Poste, @Departement, @Sexe, @DateNaissance,
    @Telephone, @Email, @Adresse, @DateEmbauche, @PhotoPath
);
SELECT CAST(SCOPE_IDENTITY() as int);";

                            using (var cmd = new SqlCommand(sql, cn, tx))
                            {
                                cmd.Parameters.Add("@Nom", SqlDbType.NVarChar, 80).Value = (txtNom.Text ?? "").Trim();
                                cmd.Parameters.Add("@Prenom", SqlDbType.NVarChar, 80).Value = (txtPrenom.Text ?? "").Trim();
                                cmd.Parameters.Add("@Matricule", SqlDbType.NVarChar, 30).Value = (txtMatricule.Text ?? "").Trim();

                                cmd.Parameters.Add("@NomUtilisateur", SqlDbType.NVarChar, 120).Value = nomU;

                                cmd.Parameters.Add("@Poste", SqlDbType.NVarChar, 60).Value = (cmbPoste.Text ?? "").Trim();
                                cmd.Parameters.Add("@Departement", SqlDbType.NVarChar, 60).Value = (cmbDepartement.Text ?? "").Trim();
                                cmd.Parameters.Add("@Sexe", SqlDbType.NVarChar, 10).Value = (cmbSexe.Text ?? "").Trim();

                                cmd.Parameters.Add("@DateNaissance", SqlDbType.Date).Value = dtpDateNaissance.Value.Date;
                                cmd.Parameters.Add("@Telephone", SqlDbType.NVarChar, 30).Value = (txtTelephone.Text ?? "").Trim();

                                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 120).Value = DBNull.Value;
                                else
                                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 120).Value = txtEmail.Text.Trim();

                                cmd.Parameters.Add("@Adresse", SqlDbType.NVarChar, 250).Value = (rtbAdresse.Text ?? "").Trim();
                                cmd.Parameters.Add("@DateEmbauche", SqlDbType.Date).Value = dtpDateEmbauche.Value.Date;

                                object photoPath = (object)(picPhoto.ImageLocation ?? "");
                                cmd.Parameters.Add("@PhotoPath", SqlDbType.NVarChar, 260).Value =
                                    string.IsNullOrWhiteSpace(photoPath.ToString()) ? (object)DBNull.Value : photoPath;

                                id = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                                if (id <= 0) throw new Exception("ID employé non généré.");
                            }

                            // 3) Code carte
                            string codeCarte = BuildCodeCarteEmploye(id);

                            using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET CodeCarteEmploye=@c
WHERE ID_Employe=@id;", cn, tx))
                            {
                                cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = codeCarte;
                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                                cmd.ExecuteNonQuery();
                            }

                            // 4) PIN (si fourni)
                            if (!string.IsNullOrWhiteSpace(pin))
                            {
                                byte[] salt = CreateSalt(16);
                                byte[] hash = HashPBKDF2_64(pin, salt, 100000, 64);

                                using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET PinSaltBin = @salt,
    PinHashBin = @hash
WHERE ID_Employe = @id;", cn, tx))
                                {
                                    cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = salt;
                                    cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 64).Value = hash;
                                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                                    int rows = cmd.ExecuteNonQuery();
                                    if (rows == 0) throw new Exception("PIN non enregistré : employé introuvable après création.");
                                }
                            }

                            tx.Commit();

                            // ✅ mettre à jour le champ pour l’UI (si vide au départ)
                            if (txtNomUtilisateur != null && string.IsNullOrWhiteSpace((txtNomUtilisateur.Text ?? "").Trim()))
                                txtNomUtilisateur.Text = nomU;

                            // 5) Audit + UI
                            ConfigSysteme.AjouterAuditLog(
                                "Ajout Employé",
                                $"Employé ajouté ID={id} | {txtNom.Text} {txtPrenom.Text} | User={nomU} | Code={codeCarte} | PIN={(string.IsNullOrWhiteSpace(pin) ? "NON" : "OUI")}",
                                "Succès"
                            );

                            MessageBox.Show($"✅ Employé enregistré !\nID={id}\nNomUtilisateur={nomU}\nCode Carte={codeCarte}");

                            txtPin?.Clear();
                        }
                        catch
                        {
                            try { tx.Rollback(); } catch { }
                            throw;
                        }
                    }
                }

                ChargerEmployes();
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog(
                    "Ajout Employé",
                    $"Échec ajout : {txtNom.Text} {txtPrenom.Text} | {ex.Message}",
                    "Échec"
                );
                MessageBox.Show("Erreur SQL : " + ex.Message);
            }
        }

        private string BuildNomUtilisateur(string matricule, string nom, string prenom)
        {
            // ✅ 0) Priorité manuelle : txtNomUtilisateur (si existe + rempli)
            string manuel = "";
            try
            {
                // si tu as bien un TextBox txtNomUtilisateur dans le designer
                manuel = (txtNomUtilisateur?.Text ?? "").Trim();
            }
            catch
            {
                manuel = "";
            }

            if (!string.IsNullOrWhiteSpace(manuel))
            {
                // normalise (enlève espaces/symboles) pour éviter caractères bizarres
                string baseManual = NormalizeUserPart(manuel);
                if (string.IsNullOrWhiteSpace(baseManual)) baseManual = "user";
                return EnsureUniqueNomUtilisateur(baseManual);
            }

            // ✅ 1) Priorité : Matricule
            if (!string.IsNullOrWhiteSpace(matricule))
                return EnsureUniqueNomUtilisateur(NormalizeUserPart(matricule.Trim()));

            // ✅ 2) Sinon : nom.prenom
            string baseU = (NormalizeUserPart(nom) + "." + NormalizeUserPart(prenom)).Trim('.');
            if (string.IsNullOrWhiteSpace(baseU)) baseU = "user";

            // ✅ 3) éviter doublons en DB
            return EnsureUniqueNomUtilisateur(baseU);
        }


        private string NormalizeUserPart(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Trim().ToLowerInvariant();

            // enlève espaces
            s = new string(s.Where(ch => char.IsLetterOrDigit(ch)).ToArray());
            return s;
        }

        private string EnsureUniqueNomUtilisateur(string baseU)
        {
            string u = baseU;
            int n = 1;

            using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                cn.Open();
                while (NomUtilisateurExists(cn, u))
                {
                    n++;
                    u = baseU + n.ToString(); // ex: messie.k  -> messie.k2
                }
            }
            return u;
        }

        private bool NomUtilisateurExists(SqlConnection cn, string nomU)
        {
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 1
FROM dbo.Employes
WHERE UPPER(LTRIM(RTRIM(ISNULL(NomUtilisateur,'')))) = UPPER(LTRIM(RTRIM(@u)));", cn))
            {
                cmd.Parameters.Add("@u", SqlDbType.NVarChar, 120).Value = (nomU ?? "").Trim();
                return cmd.ExecuteScalar() != null;
            }
        }


        private int GetSelectedEmployeId()
        {
            var row = (dgvEmployes.SelectedRows.Count > 0) ? dgvEmployes.SelectedRows[0] : dgvEmployes.CurrentRow;
            if (row == null) return 0;

            if (!dgvEmployes.Columns.Contains("ID_Employe")) return 0;

            object v = row.Cells["ID_Employe"].Value;
            return (v != null && v != DBNull.Value && int.TryParse(v.ToString(), out int id)) ? id : 0;
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            int id = GetSelectedEmployeId();
            if (id <= 0) { MessageBox.Show("Sélectionne un employé."); return; }

            var r = MessageBox.Show(
                "Désactiver cet employé ?\n\n✅ Ventes intactes\n❌ Connexion bloquée",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (r != DialogResult.Yes) return;

            GetScopeEmp(out int? idEntreprise, out int? idMagasin);

            var res = SecurityService.TryAuthorize(this, new AuthRequest
            {
                ActionCode = "EMP_DISABLE",
                Title = "Désactivation employé",
                Reference = "EMP:" + id,
                Details = $"Désactivation EMP={id} | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste})",
                AlwaysSignature = true,
                RiskLevel = 2,
                TargetId = id,
                IdEntreprise = idEntreprise,
                IdMagasin = idMagasin,
                Scope = AuthScope.Magasin
            });

            if (!res.Allowed)
            {
                MessageBox.Show(res.DenyReason ?? "Accès interdit.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (DesactiverEmployeAvecTransaction(id))
            {
                MessageBox.Show("✅ Employé désactivé !");
                ChargerEmployes();
            }
        }

        private bool DesactiverEmployeAvecTransaction(int idEmploye)
        {
            if (idEmploye <= 0)
            {
                MessageBox.Show("ID Employé invalide.");
                return false;
            }

            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();

                    using (var tx = cn.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            // ✅ Désactivation (ne touche que si l'employé est ACTIF)
                            using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET IsActif = 0,
    DateDesactivation = GETDATE()
WHERE ID_Employe = @id
  AND IsActif = 1;", cn, tx))
                            {
                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmploye;

                                int rows = cmd.ExecuteNonQuery();
                                if (rows == 0)
                                    throw new Exception("Employé introuvable ou déjà INACTIF.");
                            }

                            tx.Commit();

                            // ✅ Audit
                            ConfigSysteme.AjouterAuditLog(
                                "DESACTIVER_EMPLOYE",
                                $"Employé désactivé ID={idEmploye}",
                                "Succès"
                            );

                            return true;
                        }
                        catch (Exception exTx)
                        {
                            try { tx.Rollback(); } catch { }

                            // ✅ Audit erreur
                            ConfigSysteme.AjouterAuditLog(
                                "DESACTIVER_EMPLOYE",
                                $"Échec désactivation ID={idEmploye} | {exTx.Message}",
                                "Échec"
                            );

                            MessageBox.Show("Erreur désactivation : " + exTx.Message);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur SQL : " + ex.Message);
                return false;
            }
        }

        private void AjouterMenuContextuel()
        {
            _menuEmp = new ContextMenuStrip();

            var itemDetails = new ToolStripMenuItem("👁 Voir détails");
            itemDetails.Click += (s, e) =>
            {
                if (dgvEmployes.CurrentRow == null) return;
                var nom = dgvEmployes.CurrentRow.Cells["Nom"]?.Value?.ToString();
                var prenom = dgvEmployes.CurrentRow.Cells["Prenom"]?.Value?.ToString();
                var poste = dgvEmployes.CurrentRow.Cells["Poste"]?.Value?.ToString();
                MessageBox.Show($"Employé : {nom} {prenom}\nPoste : {poste}", "Détails");
            };

            var itemDesactiver = new ToolStripMenuItem("🚫 Désactiver (INACTIF)");
            itemDesactiver.Click += (s, e) => btnSupprimer_Click(s, e);

            var itemReactiver = new ToolStripMenuItem("✅ Réactiver (ACTIF)");
            itemReactiver.Click += (s, e) => btnReactiver_Click(s, e);

            _menuEmp.Items.Add(new ToolStripSeparator());
            _menuEmp.Items.Add(itemDesactiver);
            _menuEmp.Items.Add(itemReactiver);

            _menuEmp.Items.Add(new ToolStripSeparator());
            var itemActifs = new ToolStripMenuItem("Afficher : Actifs");
            itemActifs.Click += (s, e) => { _filtreEmp = FiltreEmployes.Actifs; ChargerEmployes(); };

            var itemInactifs = new ToolStripMenuItem("Afficher : Inactifs");
            itemInactifs.Click += (s, e) => { _filtreEmp = FiltreEmployes.Inactifs; ChargerEmployes(); };

            var itemTous = new ToolStripMenuItem("Afficher : Tous");
            itemTous.Click += (s, e) => { _filtreEmp = FiltreEmployes.Tous; ChargerEmployes(); };

            _menuEmp.Items.Add(itemActifs);
            _menuEmp.Items.Add(itemInactifs);
            _menuEmp.Items.Add(itemTous);

            var itemImprimerPVC = new ToolStripMenuItem("🖨 Imprimer carte (PVC)");
            itemImprimerPVC.Click += (s, e) => btnImprimerEmployePVC_Click(s, e);

            var itemImprimerPlast = new ToolStripMenuItem("🖨 Imprimer carte (Plastification A4)");
            itemImprimerPlast.Click += (s, e) => btnImprimerEmployePlast_Click(s, e);

            _menuEmp.Items.Add(itemDetails);
            _menuEmp.Items.Add(new ToolStripSeparator());
            _menuEmp.Items.Add(itemImprimerPVC);
            _menuEmp.Items.Add(itemImprimerPlast);

            dgvEmployes.ContextMenuStrip = _menuEmp;
        }

        private void btnReactiver_Click(object sender, EventArgs e)
        {
            int id = GetSelectedEmployeId();
            if (id <= 0) { MessageBox.Show("Sélectionne un employé."); return; }

            GetScopeEmp(out int? idEntreprise, out int? idMagasin);

            var res = SecurityService.TryAuthorize(this, new AuthRequest
            {
                ActionCode = "EMP_REACTIVATE",
                Title = "Réactivation employé",
                Reference = "EMP:" + id,
                Details = $"Réactivation EMP={id} | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste})",
                AlwaysSignature = true,
                RiskLevel = 2,
                TargetId = id,
                IdEntreprise = idEntreprise,
                IdMagasin = idMagasin,
                Scope = AuthScope.Magasin
            });

            if (!res.Allowed)
            {
                MessageBox.Show(res.DenyReason ?? "Accès interdit.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ReactiverEmploye(id))
            {
                MessageBox.Show("✅ Employé réactivé !");
                ChargerEmployes();
            }
        }

        private bool ReactiverEmploye(int idEmploye)
        {
            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET IsActif = 1,
    DateDesactivation = NULL
WHERE ID_Employe = @id AND IsActif = 0; ", cn))                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmploye;
                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0) throw new Exception("Employé introuvable ou déjà actif.");
                    }
                }

                ConfigSysteme.AjouterAuditLog("REACTIVER_EMPLOYE", $"Employé réactivé ID={idEmploye}", "Succès");
                return true;
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("REACTIVER_EMPLOYE", $"Échec réactivation ID={idEmploye} | {ex.Message}", "Échec");
                MessageBox.Show("Erreur : " + ex.Message);
                return false;
            }
        }


        private void btnDetails_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fonction Détails à implémenter.");
        }

        private void btnGenererCodeCarte_Click(object sender, EventArgs e)
        {
            int id = GetSelectedEmployeId();
            if (id <= 0) { MessageBox.Show("Sélectionne un employé."); return; }

            GetScopeEmp(out int? idEntreprise, out int? idMagasin);

            var res = SecurityService.TryAuthorize(this, new AuthRequest
            {
                ActionCode = "EMP_GENERATE_CARD_CODE",
                Title = "Génération code carte employé",
                Reference = "EMP:" + id,
                Details = $"Générer code carte EMP={id} | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste})",
                AlwaysSignature = false,   // tu peux mettre true si tu veux
                RiskLevel = 1,
                TargetId = id,
                IdEntreprise = idEntreprise,
                IdMagasin = idMagasin,
                Scope = AuthScope.Magasin
            });

            if (!res.Allowed)
            {
                MessageBox.Show(res.DenyReason ?? "Accès interdit.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string code = BuildCodeCarteEmploye(id);

            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE Employes SET CodeCarteEmploye=@c WHERE ID_Employe=@id", cn))
                    {
                        cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = code;
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        cmd.ExecuteNonQuery();
                    }
                }

                _txtCodeCarteEmploye.Text = code;
                ChargerEmployes();

                MessageBox.Show("✅ Code carte généré : " + code);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur génération code : " + ex.Message);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    DataTable dt = cn.GetSchema("Tables");

                    string listeTables = "Tables trouvées :\n";
                    foreach (DataRow row in dt.Rows)
                        listeTables += "- " + row["TABLE_NAME"] + "\n";

                    MessageBox.Show(listeTables, "Liste des tables SQL");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur SQL : " + ex.Message);
            }
        }

        // ================================
        //  PIN SECURISE (PBKDF2 -> BIN)
        //  Stocke dans: PinSaltBin (16) / PinHashBin (64)
        // ================================
        private static byte[] CreateSalt(int size = 16)
        {
            byte[] salt = new byte[size];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                rng.GetBytes(salt);
            return salt;
        }

        private static byte[] HashPBKDF2_64(string pin, byte[] salt, int iterations = 100000, int bytes = 64)
        {
            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(pin, salt, iterations))
            {
                return pbkdf2.GetBytes(bytes);
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private static bool VerifyPBKDF2(string pin, byte[] salt, byte[] expectedHash)
        {
            var test = HashPBKDF2_64(pin, salt, 100000, expectedHash.Length);
            return FixedTimeEquals(test, expectedHash);
        }


        private void btnSavePin_Click(object sender, EventArgs e)
        {
            int id = GetSelectedEmployeId();
            if (id <= 0) { MessageBox.Show("Sélectionne un employé."); return; }

            // txtPin = ton textbox dans le designer
            string pin = (txtPin.Text ?? "").Trim();

            if (pin.Length != 6)
            {
                MessageBox.Show("Le PIN doit contenir exactement 6 chiffres.");
                return;
            }

            // Optionnel: n'accepte que chiffres
            if (!pin.All(char.IsDigit))
            {
                MessageBox.Show("Le PIN doit contenir seulement des chiffres.");
                return;
            }

            // ✅ Sécurité: demande signature manager (optionnel mais recommandé)
            GetScopeEmp(out int? idEntreprise, out int? idMagasin);
            var res = SecurityService.TryAuthorize(this, new AuthRequest
            {
                ActionCode = "EMP_SET_PIN",
                Title = "Définir / changer PIN employé",
                Reference = "EMP:" + id,
                Details = $"Définition PIN EMP={id} | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste})",
                AlwaysSignature = true,
                RiskLevel = 2,
                TargetId = id,
                IdEntreprise = idEntreprise,
                IdMagasin = idMagasin,
                Scope = AuthScope.Magasin
            });

            if (!res.Allowed)
            {
                MessageBox.Show(res.DenyReason ?? "Accès interdit.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                byte[] salt = CreateSalt(16);
                byte[] hash = HashPBKDF2_64(pin, salt, 100000, 64);

                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(@"
UPDATE dbo.Employes
SET PinSaltBin = @salt,
    PinHashBin = @hash
WHERE ID_Employe = @id;", cn))
                    {
                        cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = salt;  // 16 bytes
                        cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 64).Value = hash;  // 64 bytes
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog("EMP_SET_PIN", $"PIN défini/modifié ID={id}", "Succès");
                txtPin.Clear();

                MessageBox.Show("✅ PIN enregistré (sécurisé).");
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("EMP_SET_PIN", $"Échec PIN ID={id} | {ex.Message}", "Échec");
                MessageBox.Show("Erreur enregistrement PIN : " + ex.Message);
            }
        }
    }
 }

