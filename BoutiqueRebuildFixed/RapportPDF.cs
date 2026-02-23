using System;
using System.Data.SqlClient;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Windows.Forms;

public static class RapportPDF
{
    public static void ExportInventaireDuJour(string chaineConnexion)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(chaineConnexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("InventaireVentesDuJour", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataReader reader = cmd.ExecuteReader();

                using (SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier PDF|*.pdf",
                    Title = "Exporter Inventaire du jour",
                    FileName = "Inventaire_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
                })
                {
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        Document doc = new Document(PageSize.A4, 30f, 30f, 30f, 30f);
                        PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveDialog.FileName, FileMode.Create));
                        doc.Open();

                        var fontTitre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14f);
                        var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10f);
                        var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 9f);
                        var fontFooter = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8f);

                        // Logo
                        string logoPath = @"D:\ZAIRE\LOGO1.png";
                        if (File.Exists(logoPath))
                        {
                            Image logo = Image.GetInstance(logoPath);
                            logo.ScaleToFit(60f, 60f);
                            logo.Alignment = Element.ALIGN_CENTER;
                            doc.Add(logo);
                        }

                        // Titre
                        Paragraph titre = new Paragraph("INVENTAIRE DES VENTES DU JOUR", fontTitre);
                        titre.Alignment = Element.ALIGN_CENTER;
                        titre.SpacingAfter = 10f;
                        doc.Add(titre);

                        Paragraph date = new Paragraph("Date : " + DateTime.Now.ToString("dd/MM/yyyy"), fontCell);
                        date.Alignment = Element.ALIGN_RIGHT;
                        doc.Add(date);
                        doc.Add(new Paragraph("\n"));

                        // Tableau
                        PdfPTable table = new PdfPTable(4);
                        table.WidthPercentage = 100f;
                        table.SetWidths(new float[] { 4f, 2f, 2f, 2f });

                        string[] headers = { "Produit", "Qté vendue", "Montant total", "Stock actuel" };
                        foreach (var h in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(h, fontHeader));
                            cell.BackgroundColor = new BaseColor(240, 240, 240);
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.Padding = 5f;
                            table.AddCell(cell);
                        }

                        while (reader.Read())
                        {
                            table.AddCell(new PdfPCell(new Phrase(reader["NomProduit"].ToString(), fontCell)));
                            table.AddCell(new PdfPCell(new Phrase(reader["QuantitéTotaleVendue"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(reader["MontantTotal"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                            table.AddCell(new PdfPCell(new Phrase(reader["StockActuel"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        }

                        doc.Add(table);
                        doc.Add(new Paragraph("\n"));

                        // Signature boutique
                        Paragraph signature = new Paragraph("ZAÏRE MODE – Élégance. Dignité. Respect.\n\n", fontFooter);
                        signature.Alignment = Element.ALIGN_CENTER;
                        doc.Add(signature);

                        doc.Close();
                        MessageBox.Show("✅ Rapport d’inventaire généré avec succès !");
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur : " + ex.Message);
        }
    }
public static void ExportInventaireParCaissier(string chaineConnexion, string nomCaissier)
{
    try
    {
        using (SqlConnection conn = new SqlConnection(chaineConnexion))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("InventaireParCaissier", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@NomCaissier", nomCaissier);
            SqlDataReader reader = cmd.ExecuteReader();

            using (SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Fichier PDF|*.pdf",
                Title = "Exporter Inventaire du caissier",
                FileName = $"Inventaire_{nomCaissier}_{DateTime.Now:yyyyMMdd}.pdf"
            })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    Document doc = new Document(PageSize.A4, 30f, 30f, 30f, 30f);
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveDialog.FileName, FileMode.Create));
                    doc.Open();

                    var fontTitre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14f);
                    var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10f);
                    var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 9f);
                    var fontFooter = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8f);

                    Paragraph titre = new Paragraph($"INVENTAIRE DU JOUR – Caissier : {nomCaissier}", fontTitre);
                    titre.Alignment = Element.ALIGN_CENTER;
                    titre.SpacingAfter = 10f;
                    doc.Add(titre);

                    Paragraph date = new Paragraph("Date : " + DateTime.Now.ToString("dd/MM/yyyy"), fontCell);
                    date.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(date);
                    doc.Add(new Paragraph("\n"));

                    PdfPTable table = new PdfPTable(4);
                    table.WidthPercentage = 100f;
                    table.SetWidths(new float[] { 4f, 2f, 2f, 2f });

                    string[] headers = { "Produit", "Qté vendue", "Montant total", "Stock actuel" };
                    foreach (var h in headers)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(h, fontHeader));
                        cell.BackgroundColor = new BaseColor(240, 240, 240);
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.Padding = 5f;
                        table.AddCell(cell);
                    }

                    while (reader.Read())
                    {
                        table.AddCell(new PdfPCell(new Phrase(reader["NomProduit"].ToString(), fontCell)));
                        table.AddCell(new PdfPCell(new Phrase(reader["QuantitéTotaleVendue"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(reader["MontantTotal"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        table.AddCell(new PdfPCell(new Phrase(reader["StockActuel"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    }

                    doc.Add(table);
                    doc.Add(new Paragraph("\n"));

                    Paragraph signature = new Paragraph("ZAÏRE MODE – Suivi des performances avec élégance.\n\n", fontFooter);
                    signature.Alignment = Element.ALIGN_CENTER;
                    doc.Add(signature);

                    doc.Close();
                    MessageBox.Show("✅ Rapport du caissier généré avec succès !");
                    System.Diagnostics.Process.Start(saveDialog.FileName);
                }
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Erreur : " + ex.Message);
    }
}
    public static void ExportInventaireMensuel(string chaineConnexion, DateTime mois)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(chaineConnexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("InventaireMensuel", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Annee", mois.Year);
                cmd.Parameters.AddWithValue("@Mois", mois.Month);
                SqlDataReader reader = cmd.ExecuteReader();

                using (SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier PDF|*.pdf",
                    Title = "Exporter Inventaire mensuel",
                    FileName = $"Inventaire_{mois:yyyy_MM}.pdf"
                })
                {
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        Document doc = new Document(PageSize.A4, 30f, 30f, 30f, 30f);
                        PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveDialog.FileName, FileMode.Create));
                        doc.Open();

                        var fontTitre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14f);
                        var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10f);
                        var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 9f);
                        var fontFooter = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8f);

                        Paragraph titre = new Paragraph($"INVENTAIRE MENSUEL – {mois:MMMM yyyy}", fontTitre);
                        titre.Alignment = Element.ALIGN_CENTER;
                        titre.SpacingAfter = 10f;
                        doc.Add(titre);

                        PdfPTable table = new PdfPTable(4);
                        table.WidthPercentage = 100f;
                        table.SetWidths(new float[] { 4f, 2f, 2f, 2f });

                        string[] headers = { "Produit", "Qté vendue", "Montant total", "Stock actuel" };
                        foreach (var h in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(h, fontHeader));
                            cell.BackgroundColor = new BaseColor(240, 240, 240);
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.Padding = 5f;
                            table.AddCell(cell);
                        }

                        while (reader.Read())
                        {
                            table.AddCell(new PdfPCell(new Phrase(reader["NomProduit"].ToString(), fontCell)));
                            table.AddCell(new PdfPCell(new Phrase(reader["QuantitéTotaleVendue"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(reader["MontantTotal"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                            table.AddCell(new PdfPCell(new Phrase(reader["StockActuel"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        }

                        doc.Add(table);
                        doc.Add(new Paragraph("\n"));

                        Paragraph signature = new Paragraph("ZAÏRE MODE – Vue mensuelle pour mieux planifier.\n\n", fontFooter);
                        signature.Alignment = Element.ALIGN_CENTER;
                        doc.Add(signature);

                        doc.Close();
                        MessageBox.Show("✅ Rapport mensuel généré avec succès !");
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur : " + ex.Message);
        }
    }
    public static void ExportInventaireParProduit(string chaineConnexion, string nomProduit)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(chaineConnexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("InventaireParProduit", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@NomProduit", nomProduit);
                SqlDataReader reader = cmd.ExecuteReader();

                using (SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier PDF|*.pdf",
                    Title = "Exporter Inventaire par produit",
                    FileName = $"Inventaire_{nomProduit}_{DateTime.Now:yyyyMMdd}.pdf"
                })
                {
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        Document doc = new Document(PageSize.A4, 30f, 30f, 30f, 30f);
                        PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveDialog.FileName, FileMode.Create));
                        doc.Open();

                        var fontTitre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14f);
                        var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10f);
                        var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 9f);
                        var fontFooter = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8f);

                        Paragraph titre = new Paragraph($"INVENTAIRE DU JOUR – Produit : {nomProduit}", fontTitre);
                        titre.Alignment = Element.ALIGN_CENTER;
                        titre.SpacingAfter = 10f;
                        doc.Add(titre);

                        PdfPTable table = new PdfPTable(4);
                        table.WidthPercentage = 100f;
                        table.SetWidths(new float[] { 4f, 2f, 2f, 2f });

                        string[] headers = { "Produit", "Qté vendue", "Montant total", "Stock actuel" };
                        foreach (var h in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(h, fontHeader));
                            cell.BackgroundColor = new BaseColor(240, 240, 240);
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.Padding = 5f;
                            table.AddCell(cell);
                        }

                        while (reader.Read())
                        {
                            table.AddCell(new PdfPCell(new Phrase(reader["NomProduit"].ToString(), fontCell)));
                            table.AddCell(new PdfPCell(new Phrase(reader["QuantitéTotaleVendue"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            table.AddCell(new PdfPCell(new Phrase(reader["MontantTotal"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                            table.AddCell(new PdfPCell(new Phrase(reader["StockActuel"].ToString(), fontCell)) { HorizontalAlignment = Element.ALIGN_CENTER });
                        }

                        doc.Add(table);
                        doc.Add(new Paragraph("\n"));

                        Paragraph signature = new Paragraph("ZAÏRE MODE – Suivi par produit pour mieux gérer les stocks.\n\n", fontFooter);
                        signature.Alignment = Element.ALIGN_CENTER;
                        doc.Add(signature);

                        doc.Close();
                        MessageBox.Show("✅ Rapport par produit généré avec succès !");
                        System.Diagnostics.Process.Start(saveDialog.FileName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur : " + ex.Message);
        }
    }
}



