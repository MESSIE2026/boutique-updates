using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

// ZXing + Drawing (pour générer une image PNG du code-barres)
using System.Drawing;               // Bitmap
using System.Drawing.Imaging;       // ImageFormat
using ZXing;
using ZXing.Common;

// iTextSharp
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

// ✅ Alias anti-conflit
using PdfFont = iTextSharp.text.Font;
using PdfRectangle = iTextSharp.text.Rectangle;

using System.Drawing.Printing;


namespace BoutiqueRebuildFixed
{

    // =========================
    // DTO (si tu les as déjà, supprime ces classes)
    // =========================

    public partial class FrmOrdonnanceVente : Form
    {
        public OrdonnanceVenteDTO Result { get; private set; }

        private readonly List<OrdonnanceLigneDTO> _lignes;
        private string _scanPath = "";

        // UI
        private TextBox txtNumero;
        private TextBox txtPrescripteur;
        private DateTimePicker dtDate;
        private TextBox txtPatient;
        private TextBox txtNote;
        private TextBox txtScan;
        private DataGridView dgv;

        private Button btnScan;
        private Button btnPreviewPdf;
        private Button btnOk;
        private Button btnAnnuler;

        // ✅ nouveau
        private Button btnTicket;


        public FrmOrdonnanceVente(List<OrdonnanceLigneDTO> lignes) : this(lignes, null) { }

        public FrmOrdonnanceVente(List<OrdonnanceLigneDTO> lignes, OrdonnanceVenteDTO prefill)
        {
            _lignes = lignes ?? new List<OrdonnanceLigneDTO>();

            InitializeComponent();
            BuildUi();
            LoadLines();

            if (prefill != null)
            {
                txtNumero.Text = prefill.Numero ?? "";
                txtPrescripteur.Text = prefill.Prescripteur ?? "";
                txtPatient.Text = prefill.Patient ?? "";
                dtDate.Value = prefill.DateOrdonnance == default(DateTime) ? DateTime.Today : prefill.DateOrdonnance;
                txtNote.Text = prefill.Note ?? "";

                _scanPath = prefill.ScanPath ?? "";
                txtScan.Text = _scanPath;
            }

            Text = "Ordonnance - Vente";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 860;
            Height = 560;
        }

        private void BuildUi()
        {
            var panelTop = new Panel { Dock = DockStyle.Top, Height = 190, Padding = new Padding(12) };
            var panelMid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(12) };

            // Labels + inputs
            var lblNumero = new Label { Text = "N° Ordonnance *", AutoSize = true, Left = 10, Top = 12 };
            txtNumero = new TextBox { Left = 140, Top = 8, Width = 250 };

            var lblPresc = new Label { Text = "Prescripteur", AutoSize = true, Left = 420, Top = 12 };
            txtPrescripteur = new TextBox { Left = 510, Top = 8, Width = 300 };

            var lblDate = new Label { Text = "Date", AutoSize = true, Left = 10, Top = 48 };
            dtDate = new DateTimePicker { Left = 140, Top = 44, Width = 250, Format = DateTimePickerFormat.Short, Value = DateTime.Today };

            var lblPatient = new Label { Text = "Patient", AutoSize = true, Left = 420, Top = 48 };
            txtPatient = new TextBox { Left = 510, Top = 44, Width = 300 };

            var lblNote = new Label { Text = "Note", AutoSize = true, Left = 10, Top = 84 };
            txtNote = new TextBox { Left = 140, Top = 80, Width = 670, Height = 60, Multiline = true, ScrollBars = ScrollBars.Vertical };

            var lblScan = new Label { Text = "Scan (option)", AutoSize = true, Left = 10, Top = 152 };
            txtScan = new TextBox { Left = 140, Top = 148, Width = 560, ReadOnly = true };
            btnScan = new Button { Left = 710, Top = 146, Width = 100, Height = 28, Text = "Joindre..." };
            btnScan.Click += BtnScan_Click;

            panelTop.Controls.AddRange(new Control[]
            {
                lblNumero, txtNumero,
                lblPresc, txtPrescripteur,
                lblDate, dtDate,
                lblPatient, txtPatient,
                lblNote, txtNote,
                lblScan, txtScan, btnScan
            });

            // Grid
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            panelMid.Controls.Add(dgv);

            // Bottom buttons
            btnPreviewPdf = new Button { Text = "Prévisualiser PDF", Width = 140, Height = 32, Left = 12, Top = 12 };
            btnPreviewPdf.Click += BtnPreviewPdf_Click;

            // ✅ bouton Ticket
            btnTicket = new Button { Text = "Imprimer Ticket", Width = 140, Height = 32, Left = 160, Top = 12 };
            btnTicket.Click += BtnPrintTicket_Click;
            panelBottom.Controls.Add(btnTicket);

            btnOk = new Button { Text = "OK", Width = 120, Height = 32, Anchor = AnchorStyles.Right | AnchorStyles.Top };
            btnAnnuler = new Button { Text = "Annuler", Width = 120, Height = 32, Anchor = AnchorStyles.Right | AnchorStyles.Top };

            btnOk.Top = 12;
            btnAnnuler.Top = 12;

            btnOk.Click += BtnOk_Click;
            btnAnnuler.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            panelBottom.Resize += (s, e) =>
            {
                btnAnnuler.Left = panelBottom.Width - btnAnnuler.Width - 12;
                btnOk.Left = btnAnnuler.Left - btnOk.Width - 10;
            };

            panelBottom.Controls.Add(btnPreviewPdf);
            panelBottom.Controls.Add(btnTicket);
            panelBottom.Controls.Add(btnOk);
            panelBottom.Controls.Add(btnAnnuler);

            Controls.Add(panelMid);
            Controls.Add(panelTop);
            Controls.Add(panelBottom);
        }

        private void LoadLines()
        {
            var dt = new DataTable();
            dt.Columns.Add("ID_Produit", typeof(int));
            dt.Columns.Add("Article", typeof(string));
            dt.Columns.Add("Qté", typeof(int));
            dt.Columns.Add("PU", typeof(string));
            dt.Columns.Add("Total", typeof(string));

            foreach (var l in _lignes)
            {
                int qte = l.Qte <= 0 ? 1 : l.Qte;
                string dev = string.IsNullOrWhiteSpace(l.Devise) ? "" : l.Devise.Trim();

                dt.Rows.Add(
                    l.IdProduit,
                    l.NomProduit ?? "",
                    qte,
                    l.PU.ToString("N2") + (dev.Length > 0 ? " " + dev : ""),
                    (l.PU * qte).ToString("N2") + (dev.Length > 0 ? " " + dev : "")
                );
            }

            dgv.DataSource = dt;
            if (dgv.Columns["ID_Produit"] != null)
                dgv.Columns["ID_Produit"].Visible = false;
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Joindre un scan (image ou PDF)";
                ofd.Filter = "Images/PDF|*.jpg;*.jpeg;*.png;*.bmp;*.pdf|Tous les fichiers|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    _scanPath = ofd.FileName;
                    txtScan.Text = _scanPath;
                }
            }
        }

        // ===================== PDF : choix emplacement =====================
        private string AskPdfSavePath(string numero)
        {
            string safeNum = string.Join("_", (numero ?? "ORD").Where(ch => !Path.GetInvalidFileNameChars().Contains(ch))).Trim();
            if (string.IsNullOrWhiteSpace(safeNum)) safeNum = "ORD";

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Enregistrer l'ordonnance PDF";
                sfd.Filter = "Fichier PDF (*.pdf)|*.pdf";
                sfd.FileName = $"Ordonnance_{safeNum}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                return (sfd.ShowDialog(this) == DialogResult.OK) ? sfd.FileName : null;
            }
        }

        private void BtnPreviewPdf_Click(object sender, EventArgs e)
        {
            try
            {
                var dto = BuildDtoOrThrow();

                string chosen = AskPdfSavePath(dto.Numero);
                if (string.IsNullOrWhiteSpace(chosen)) return;

                var pdfPath = GeneratePdf(dto, chosen);
                dto.PdfPath = pdfPath;

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pdfPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ordonnance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            try
            {
                var dto = BuildDtoOrThrow();

                string chosen = AskPdfSavePath(dto.Numero);
                if (string.IsNullOrWhiteSpace(chosen)) return;

                var pdfPath = GeneratePdf(dto, chosen);
                dto.PdfPath = pdfPath;

                Result = dto;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ordonnance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private OrdonnanceVenteDTO BuildDtoOrThrow()
        {
            var num = (txtNumero.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(num))
                throw new Exception("Le numéro d’ordonnance est obligatoire.");

            return new OrdonnanceVenteDTO
            {
                Numero = num,
                Prescripteur = (txtPrescripteur.Text ?? "").Trim(),
                DateOrdonnance = dtDate.Value.Date,
                Patient = (txtPatient.Text ?? "").Trim(),
                Note = (txtNote.Text ?? "").Trim(),
                ScanPath = string.IsNullOrWhiteSpace(_scanPath) ? "" : _scanPath,
                Lignes = _lignes.Select(x => new OrdonnanceLigneDTO
                {
                    IdProduit = x.IdProduit,
                    NomProduit = x.NomProduit,
                    Qte = x.Qte <= 0 ? 1 : x.Qte,
                    PU = x.PU,
                    Devise = x.Devise
                }).ToList()
            };
        }

        // ===================== PDF Generator (chemin choisi) =====================
        private string GeneratePdf(OrdonnanceVenteDTO o, string fullPath)
        {
            string folder = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(folder))
                Directory.CreateDirectory(folder);

            using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 36, 36, 36, 36))
            {
                var writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                var fontTitre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14f);
                var fontSmall = FontFactory.GetFont(FontFactory.HELVETICA, 9f);

                var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fontLabel = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fontText = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                // ENTÊTE
                PdfPTable top = new PdfPTable(2) { WidthPercentage = 100 };
                top.SetWidths(new float[] { 70f, 30f });

                PdfPCell left = new PdfPCell { Border = PdfPCell.NO_BORDER };
                left.AddElement(new Paragraph("ZAIRE MODE SARL", fontTitre));
                left.AddElement(new Paragraph("23, Bld Lumumba / Immeuble Masina Plaza", fontSmall));
                left.AddElement(new Paragraph("+243861507560 / E-MAIL: Zaireshop@hotmail.com", fontSmall));
                left.AddElement(new Paragraph("PAGE: ZAIRE.CD", fontSmall));
                left.AddElement(new Paragraph("RCCM: 25-B-01497", fontSmall));
                left.AddElement(new Paragraph("IDNAT: 01-F4300-N73258E", fontSmall));
                top.AddCell(left);

                PdfPCell right = new PdfPCell { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

                string logoPath = @"D:\ZAIRE\LOGO1.png";
                if (File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(90f, 90f);
                    logo.Alignment = Element.ALIGN_RIGHT;
                    right.AddElement(logo);
                }
                top.AddCell(right);

                doc.Add(top);
                doc.Add(new Paragraph(" "));

                doc.Add(new Paragraph(new Chunk(new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0))));
                doc.Add(new Paragraph(" "));

                doc.Add(new Paragraph("ORDONNANCE (VENTE)", fontTitle) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph(" "));

                var info = new PdfPTable(2) { WidthPercentage = 100 };
                info.SetWidths(new float[] { 25f, 75f });

                AddRow(info, "N° Ordonnance :", o.Numero, fontLabel, fontText);
                AddRow(info, "Date :", o.DateOrdonnance.ToString("dd/MM/yyyy"), fontLabel, fontText);
                AddRow(info, "Prescripteur :", string.IsNullOrWhiteSpace(o.Prescripteur) ? "-" : o.Prescripteur, fontLabel, fontText);
                AddRow(info, "Patient :", string.IsNullOrWhiteSpace(o.Patient) ? "-" : o.Patient, fontLabel, fontText);
                AddRow(info, "Note :", string.IsNullOrWhiteSpace(o.Note) ? "-" : o.Note, fontLabel, fontText);

                doc.Add(info);
                doc.Add(new Paragraph(" "));

                var tbl = new PdfPTable(4) { WidthPercentage = 100 };
                tbl.SetWidths(new float[] { 55f, 10f, 17f, 18f });

                AddHeader(tbl, "Article", fontLabel);
                AddHeader(tbl, "Qté", fontLabel);
                AddHeader(tbl, "PU", fontLabel);
                AddHeader(tbl, "Total", fontLabel);

                foreach (var l in o.Lignes)
                {
                    int qte = l.Qte <= 0 ? 1 : l.Qte;
                    string dev = string.IsNullOrWhiteSpace(l.Devise) ? "" : l.Devise.Trim();

                    AddCell(tbl, l.NomProduit ?? "", fontText, Element.ALIGN_LEFT);
                    AddCell(tbl, qte.ToString(), fontText, Element.ALIGN_CENTER);

                    AddCell(tbl, l.PU.ToString("N2") + (dev.Length > 0 ? " " + dev : ""), fontText, Element.ALIGN_RIGHT);
                    AddCell(tbl, (l.PU * qte).ToString("N2") + (dev.Length > 0 ? " " + dev : ""), fontText, Element.ALIGN_RIGHT);
                }

                doc.Add(tbl);

                // TOTAL TTC
                decimal totalTtc = 0m;
                foreach (var l in o.Lignes)
                {
                    int qte = l.Qte <= 0 ? 1 : l.Qte;
                    totalTtc += Math.Round(l.PU * qte, 2);
                }
                totalTtc = Math.Round(totalTtc, 2);

                string deviseTtc = "";
                var devises = o.Lignes.Select(x => (x.Devise ?? "").Trim().ToUpperInvariant())
                                      .Where(x => x.Length > 0)
                                      .Distinct()
                                      .ToList();
                if (devises.Count == 1) deviseTtc = devises[0];
                else if (devises.Count > 1) deviseTtc = "MULTI";

                var ttcTbl = new PdfPTable(2) { WidthPercentage = 45, HorizontalAlignment = Element.ALIGN_RIGHT };
                ttcTbl.SetWidths(new float[] { 55f, 45f });

                var cLbl = new PdfPCell(new Phrase("TOTAL TTC :", fontLabel))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 6f
                };
                var cVal = new PdfPCell(new Phrase(totalTtc.ToString("N2") + (deviseTtc.Length > 0 ? " " + deviseTtc : ""), fontLabel))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 6f
                };

                ttcTbl.AddCell(cLbl);
                ttcTbl.AddCell(cVal);

                doc.Add(new Paragraph(" "));
                doc.Add(ttcTbl);

                // scan
                if (!string.IsNullOrWhiteSpace(o.ScanPath) && File.Exists(o.ScanPath))
                {
                    doc.Add(new Paragraph(" "));
                    doc.Add(new Paragraph("Scan joint :", fontLabel));

                    var ext = Path.GetExtension(o.ScanPath).ToLowerInvariant();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp")
                    {
                        try
                        {
                            var img = iTextSharp.text.Image.GetInstance(o.ScanPath);
                            img.ScaleToFit(520f, 520f);
                            img.Alignment = Element.ALIGN_CENTER;
                            doc.Add(img);
                        }
                        catch
                        {
                            doc.Add(new Paragraph("(Impossible d'intégrer l'image.)", fontText));
                        }
                    }
                    else
                    {
                        doc.Add(new Paragraph("Fichier scan (PDF) : " + o.ScanPath, fontText));
                    }
                }

                doc.Add(new Paragraph(" "));
                doc.Add(new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0));
                doc.Add(new Paragraph(" "));

                doc.Add(new Paragraph("Merci pour votre fidélité, à la prochaine !", fontText) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("La Qualité fait la différence.", fontText) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("Les marchandises vendues ne peuvent être ni reprises, ni échangées.", fontSmall) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph(" "));

                // barcode PDF (optionnel)
                try
                {
                    string codeFacture = (!string.IsNullOrWhiteSpace(o.CodeFacture) ? o.CodeFacture : o.Numero) ?? "";
                    codeFacture = codeFacture.Trim();

                    var img1 = BuildBarcodeImage(codeFacture, 300, 40);
                    if (img1 != null)
                    {
                        doc.Add(img1);
                        doc.Add(new Paragraph("Code Facture : " + codeFacture, fontSmall) { Alignment = Element.ALIGN_CENTER });
                        doc.Add(new Paragraph(" "));
                    }
                }
                catch { }

                doc.Close();
                writer.Close();
            }

            return fullPath;
        }

        private iTextSharp.text.Image BuildBarcodeImage(string text, int width = 420, int height = 70)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            var bw = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 1,
                    PureBarcode = false
                }
            };

            using (var bmp = bw.Write(text))
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                var img = iTextSharp.text.Image.GetInstance(ms.ToArray());
                img.Alignment = Element.ALIGN_CENTER;
                img.ScaleToFit(width, height);
                return img;
            }
        }

        private void AddRow(PdfPTable t, string label, string value, PdfFont fl, PdfFont fv)
        {
            var c1 = new PdfPCell(new Phrase(label, fl))
            {
                Border = PdfRectangle.NO_BORDER,
                PaddingBottom = 4f
            };

            var c2 = new PdfPCell(new Phrase(value ?? "", fv))
            {
                Border = PdfRectangle.NO_BORDER,
                PaddingBottom = 4f
            };

            t.AddCell(c1);
            t.AddCell(c2);
        }

        private void AddHeader(PdfPTable t, string text, PdfFont f)
        {
            var c = new PdfPCell(new Phrase(text, f))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 6f
            };
            t.AddCell(c);
        }

        private void AddCell(PdfPTable t, string text, PdfFont f, int align = Element.ALIGN_LEFT)
        {
            var c = new PdfPCell(new Phrase(text ?? "", f))
            {
                HorizontalAlignment = align,
                Padding = 6f
            };
            t.AddCell(c);
        }

        private string GetPrinterName(bool isTicket)
        {
            ConfigSysteme.LoadPrintersConfig(); // au cas où

            string p = isTicket ? ConfigSysteme.ImprimanteTicketNom : ConfigSysteme.ImprimanteA4Nom;
            if (string.IsNullOrWhiteSpace(p)) return null;

            // vérifier si installée
            foreach (string pr in PrinterSettings.InstalledPrinters)
                if (string.Equals(pr, p, StringComparison.OrdinalIgnoreCase))
                    return pr;

            return null;
        }


        // ===================== TICKET =====================
        private void BtnPrintTicket_Click(object sender, EventArgs e)
        {
            try
            {
                var dto = BuildDtoOrThrow();

                // ✅ utiliser imprimante Ticket sauvegardée
                PrintTicket(dto, useTicketPrinter: true);

                // si tu veux forcer une imprimante précise:
                // PrintTicket(dto, true, "Nom exact imprimante");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ticket", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void PrintTicket(OrdonnanceVenteDTO o, bool useTicketPrinter, string printerNameOverride = null)
        {
            if (o == null) throw new ArgumentNullException(nameof(o));

            var pd = new PrintDocument();

            // 1) priorité: override manuel
            if (!string.IsNullOrWhiteSpace(printerNameOverride))
            {
                pd.PrinterSettings.PrinterName = printerNameOverride;
            }
            else
            {
                // 2) sinon: config (ticket ou A4)
                string printer = GetPrinterName(isTicket: useTicketPrinter);
                if (!string.IsNullOrWhiteSpace(printer))
                    pd.PrinterSettings.PrinterName = printer;
                else
                    MessageBox.Show("Imprimante non trouvée. Configure Ticket/A4 dans Gestion Imprimantes.");
            }

            // marges
            pd.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

            // (optionnel) taille ticket 80mm: à activer si ton driver le permet
            // pd.DefaultPageSettings.PaperSize = new PaperSize("Ticket80", 315, 1000);

            pd.PrintPage += (s, e) =>
            {
                float y = e.MarginBounds.Top;
                float left = e.MarginBounds.Left;
                float right = e.MarginBounds.Right;
                float width = e.MarginBounds.Width;

                using (var fTitle = new System.Drawing.Font("Segoe UI", 10f, FontStyle.Bold))
                using (var fText = new System.Drawing.Font("Segoe UI", 8.5f, FontStyle.Regular))
                using (var fBold = new System.Drawing.Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    Action<string, System.Drawing.Font, float> DrawCenter = (text, f, addSpace) =>
                    {
                        var sz = e.Graphics.MeasureString(text, f);
                        float x = left + (width - sz.Width) / 2f;
                        e.Graphics.DrawString(text, f, Brushes.Black, x, y);
                        y += sz.Height + addSpace;
                    };

                    Action<string, System.Drawing.Font, float> DrawLeft = (text, f, addSpace) =>
                    {
                        e.Graphics.DrawString(text, f, Brushes.Black, left, y);
                        y += e.Graphics.MeasureString(text, f).Height + addSpace;
                    };

                    Action<float> DrawLine = (addSpace) =>
                    {
                        y += 2;
                        e.Graphics.DrawLine(Pens.Black, left, y, right, y);
                        y += addSpace;
                    };

                    // ===== EN-TÊTE
                    DrawCenter("ZAIRE MODE SARL", fTitle, 1);
                    DrawCenter("23, Bld Lumumba / Immeuble Masina Plaza", fText, 1);
                    DrawCenter("+243861507560  |  Zaireshop@hotmail.com", fText, 1);
                    DrawCenter("RCCM: 25-B-01497  |  IDNAT: 01-F4300-N73258E", fText, 1);

                    DrawLine(4);

                    DrawCenter("ORDONNANCE (VENTE)", fBold, 2);

                    DrawLeft("N° : " + (o.Numero ?? ""), fBold, 1);
                    DrawLeft("Date : " + o.DateOrdonnance.ToString("dd/MM/yyyy"), fText, 1);

                    if (!string.IsNullOrWhiteSpace(o.Prescripteur)) DrawLeft("Prescripteur : " + o.Prescripteur, fText, 1);
                    if (!string.IsNullOrWhiteSpace(o.Patient)) DrawLeft("Patient : " + o.Patient, fText, 1);
                    if (!string.IsNullOrWhiteSpace(o.Note)) DrawLeft("Note : " + o.Note, fText, 1);

                    DrawLine(4);

                    // ===== COLONNES (Article | Qté | Total)
                    float colQte = 35f;
                    float colTot = 70f;
                    float colArt = width - colQte - colTot;

                    e.Graphics.DrawString("Article", fBold, Brushes.Black, left, y);
                    e.Graphics.DrawString("Qté", fBold, Brushes.Black, left + colArt, y);
                    e.Graphics.DrawString("Total", fBold, Brushes.Black, left + colArt + colQte, y);
                    y += e.Graphics.MeasureString("X", fBold).Height + 2;

                    DrawLine(3);

                    decimal totalTtc = 0m;
                    string devise = "";

                    var lignes = o.Lignes ?? new List<OrdonnanceLigneDTO>();

                    foreach (var l in lignes)
                    {
                        int qte = l.Qte <= 0 ? 1 : l.Qte;
                        decimal tot = Math.Round(l.PU * qte, 2);
                        totalTtc += tot;

                        if (string.IsNullOrWhiteSpace(devise))
                            devise = (l.Devise ?? "").Trim().ToUpperInvariant();

                        string art = (l.NomProduit ?? "").Trim();
                        if (art.Length > 26) art = art.Substring(0, 26);

                        e.Graphics.DrawString(art, fText, Brushes.Black, left, y);
                        e.Graphics.DrawString(qte.ToString(), fText, Brushes.Black, left + colArt, y);
                        e.Graphics.DrawString(tot.ToString("N2"), fText, Brushes.Black, left + colArt + colQte, y);
                        y += e.Graphics.MeasureString("X", fText).Height + 2;
                    }

                    DrawLine(4);

                    totalTtc = Math.Round(totalTtc, 2);
                    string totalStr = totalTtc.ToString("N2") + (string.IsNullOrWhiteSpace(devise) ? "" : " " + devise);

                    var szTot = e.Graphics.MeasureString("TOTAL TTC : " + totalStr, fBold);
                    e.Graphics.DrawString("TOTAL TTC : " + totalStr, fBold, Brushes.Black, right - szTot.Width, y);
                    y += szTot.Height + 4;

                    DrawLine(4);

                    DrawCenter("Merci pour votre fidélité !", fText, 1);
                    DrawCenter("La Qualité fait la différence.", fText, 1);
                    DrawCenter("Aucun échange, aucun remboursement.", fText, 1);

                    e.HasMorePages = false;
                }
            };

            pd.Print();
        }
    }
}
