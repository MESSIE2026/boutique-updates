using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace BoutiqueRebuildFixed
{
    public class AnnulationsPdfService
    {
        public void Exporter(AnnulationRetour a, string filePath, string logoPath)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (string.IsNullOrWhiteSpace(filePath)) throw new Exception("Chemin PDF invalide.");

            // ✅ Calculer le total à partir du modèle (pas la DB)
            decimal qteRetour = a.QuantiteRetournee; // IMPORTANT : quantité retournée
            decimal prixTotal = Math.Round(a.PrixUnitaire * qteRetour, 2);

            var doc = new Document(PageSize.A4, 20, 20, 20, 20);
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            doc.Open();
            doc.Open();

            var fontTitre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            var fontLabel = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);

            doc.Add(new Paragraph("ZAIRE MODE SARL\n23, Bld Lumumba / Immeuble Masina Plaza\n\n", fontTitre)
            { Alignment = Element.ALIGN_CENTER });

            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
            {
                var logo = Image.GetInstance(logoPath);
                logo.Alignment = Element.ALIGN_CENTER;
                logo.ScaleToFit(70f, 70f);
                logo.SpacingAfter = 8f;
                doc.Add(logo);
            }

            doc.Add(new Paragraph("+243861507560 | Zaireshop@hotmail.com\nPAGE: ZAIRE.CD | RCCM: 25-B-01497 | IDNAT: 01-F4300-N73258E\n\n", fontNormal)
            { Alignment = Element.ALIGN_CENTER });

            doc.Add(new Paragraph("FICHE D’ANNULATION / RETOUR\n\n", fontTitre)
            { Alignment = Element.ALIGN_CENTER });

            PdfPTable table = new PdfPTable(2) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 30f, 70f });

            // ✅ C# 7.3 : pas de fonction locale "void AddRow(...)"
            table.AddCell(new PdfPCell(new Phrase("Nom du client", fontLabel)) { Padding = 6, BorderWidth = 0.5f });
            table.AddCell(new PdfPCell(new Phrase(a.NomClient ?? "", fontNormal)) { Padding = 6, BorderWidth = 0.5f });

            table.AddCell(new PdfPCell(new Phrase("N° Commande", fontLabel)) { Padding = 6, BorderWidth = 0.5f });
            table.AddCell(new PdfPCell(new Phrase(a.NumeroCommande ?? "", fontNormal)) { Padding = 6, BorderWidth = 0.5f });

            table.AddCell(new PdfPCell(new Phrase("Produit", fontLabel)) { Padding = 6, BorderWidth = 0.5f });
            table.AddCell(new PdfPCell(new Phrase(a.NomProduit ?? "", fontNormal)) { Padding = 6, BorderWidth = 0.5f });

            // ✅ Quantité retournée (pas Quantite)
            table.AddCell(new PdfPCell(new Phrase("Quantité retournée", fontLabel)) { Padding = 6, BorderWidth = 0.5f });
            table.AddCell(new PdfPCell(new Phrase(qteRetour.ToString("0.##"), fontNormal)) { Padding = 6, BorderWidth = 0.5f });

            // Optionnel : afficher quantité achetée si tu la stockes
            if (a.QuantiteAchetee > 0)
            {
                table.AddCell(new PdfPCell(new Phrase("Quantité achetée", fontLabel)) { Padding = 6, BorderWidth = 0.5f });
                table.AddCell(new PdfPCell(new Phrase(a.QuantiteAchetee.ToString("0.##"), fontNormal)) { Padding = 6, BorderWidth = 0.5f });
            }

            table.AddCell(new PdfPCell(new Phrase("Prix unitaire", fontLabel)) { Padding = 6, BorderWidth = 0.5f });
            table.AddCell(new PdfPCell(new Phrase($"{a.PrixUnitaire:F2} {a.Devise}", fontNormal)) { Padding = 6, BorderWidth = 0.5f });

            // ✅ Prix total calculé
            table.AddCell(new PdfPCell(new Phrase("Prix total", fontLabel)) { Padding = 6, BorderWidth = 0.5f });
            table.AddCell(new PdfPCell(new Phrase($"{prixTotal:F2} {a.Devise}", fontNormal)) { Padding = 6, BorderWidth = 0.5f });

            table.AddCell(new PdfPCell(new Phrase("Motif", fontLabel)) { Padding = 6, BorderWidth = 0.5f });
            table.AddCell(new PdfPCell(new Phrase(a.MotifRetour ?? "", fontNormal)) { Padding = 6, BorderWidth = 0.5f });

            table.AddCell(new PdfPCell(new Phrase("Type", fontLabel)) { Padding = 6, BorderWidth = 0.5f });
            table.AddCell(new PdfPCell(new Phrase(a.TypeRetour ?? "", fontNormal)) { Padding = 6, BorderWidth = 0.5f });

            doc.Add(table);

            doc.Add(new Paragraph($"\nFait à Kinshasa, le {DateTime.Now:dd/MM/yyyy}\n\n", fontNormal)
            { Alignment = Element.ALIGN_RIGHT });

            doc.Add(new Paragraph("Administration\nMESSIE MATALA", fontNormal)
            { Alignment = Element.ALIGN_RIGHT });

            doc.Close();
        }
    }
}