using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Data.SqlClient;
using System.Windows.Forms;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore; // pour XUnit

namespace BoutiqueRebuildFixed
{
    public static class PdfExportHelper
    {
        public static string AskSavePdfPath(string defaultFileName)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Fichiers PDF (*.pdf)|*.pdf";
                sfd.FileName = defaultFileName;
                return sfd.ShowDialog() == DialogResult.OK ? sfd.FileName : null;
            }
        }

        public static DataTable DataGridViewToDataTable(DataGridView dgv)
        {
            var dt = new DataTable();

            var cols = dgv.Columns.Cast<DataGridViewColumn>()
                .Where(c => c.Visible && !(c is DataGridViewButtonColumn))
                .OrderBy(c => c.DisplayIndex)
                .ToList();

            foreach (var c in cols)
                dt.Columns.Add(c.HeaderText);

            foreach (DataGridViewRow r in dgv.Rows)
            {
                if (r.IsNewRow) continue;
                var row = dt.NewRow();
                for (int i = 0; i < cols.Count; i++)
                    row[i] = (r.Cells[cols[i].Name].Value ?? "").ToString();
                dt.Rows.Add(row);
            }
            return dt;
        }

        public static void ExportDataTableToPdf(string path, string title, DataTable dt, string subtitle = null)
        {
            if (dt == null || dt.Columns.Count == 0)
                throw new Exception("DataTable vide.");

            var doc = new PdfDocument();
            doc.Info.Title = title ?? "Export";

            PdfPage page = doc.AddPage();

            // A4 paysage (297 x 210 mm)
            page.Width = XUnit.FromMillimeter(297);
            page.Height = XUnit.FromMillimeter(210);

            XGraphics gfx = XGraphics.FromPdfPage(page);

            int marginL = 30, marginR = 30, marginT = 30, marginB = 30;
            double y = marginT;

            var fontTitle = new XFont("Arial", 14, XFontStyle.Bold);
            var fontSub = new XFont("Arial", 10, XFontStyle.Regular);
            var fontHeader = new XFont("Arial", 9, XFontStyle.Bold);
            var fontCell = new XFont("Arial", 9, XFontStyle.Regular);

            double pageW = page.Width.Point - marginL - marginR;
            double pageH = page.Height.Point - marginT - marginB;

            void DrawCentered(string t, XFont f)
            {
                var w = gfx.MeasureString(t, f).Width;
                gfx.DrawString(t, f, XBrushes.Black, new XPoint((page.Width.Point - w) / 2, y));
                y += (f.Size + 8);
            }

            void DrawHeader()
            {
                DrawCentered(title ?? "EXPORT", fontTitle);

                if (!string.IsNullOrWhiteSpace(subtitle))
                {
                    gfx.DrawString(subtitle, fontSub, XBrushes.Black, new XPoint(marginL, y));
                    y += 18;
                }

                gfx.DrawLine(XPens.Black, marginL, y, marginL + pageW, y);
                y += 12;
            }

            void DrawTableHeader(double[] wCols)
            {
                double x = marginL;
                double rowH = 18;

                gfx.DrawRectangle(XBrushes.LightGray, marginL, y, pageW, rowH);

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    gfx.DrawString(dt.Columns[i].ColumnName, fontHeader, XBrushes.Black,
                        new XRect(x + 3, y + 3, wCols[i] - 6, rowH),
                        XStringFormats.TopLeft);

                    gfx.DrawLine(XPens.Gray, x, y, x, y + rowH);
                    x += wCols[i];
                }

                gfx.DrawLine(XPens.Gray, marginL + pageW, y, marginL + pageW, y + rowH);
                y += rowH;
                gfx.DrawLine(XPens.Gray, marginL, y, marginL + pageW, y);
            }

            void NewPage(double[] wCols)
            {
                page = doc.AddPage();
                page.Width = XUnit.FromMillimeter(297);
                page.Height = XUnit.FromMillimeter(210);

                gfx = XGraphics.FromPdfPage(page);
                y = marginT;

                DrawHeader();
                DrawTableHeader(wCols);
            }

            void Ensure(double need, double[] wCols)
            {
                if (y + need > marginT + pageH)
                    NewPage(wCols);
            }

            DrawHeader();

            // ✅ Largeurs colonnes
            int colCount = dt.Columns.Count;
            double[] colWidths = new double[colCount];

            for (int i = 0; i < colCount; i++)
            {
                double w = gfx.MeasureString(dt.Columns[i].ColumnName, fontHeader).Width + 10;
                colWidths[i] = Math.Max(60, w);
            }

            double tot = colWidths.Sum();
            if (tot > pageW)
            {
                double scale = pageW / tot;
                for (int i = 0; i < colCount; i++) colWidths[i] *= scale;
            }

            DrawTableHeader(colWidths);

            foreach (DataRow r in dt.Rows)
            {
                Ensure(18, colWidths);

                double x = marginL;
                double rowH = 16;

                for (int i = 0; i < colCount; i++)
                {
                    string text = (r[i] ?? "").ToString();
                    gfx.DrawString(text, fontCell, XBrushes.Black,
                        new XRect(x + 3, y + 2, colWidths[i] - 6, rowH),
                        XStringFormats.TopLeft);

                    gfx.DrawLine(XPens.LightGray, x, y, x, y + rowH);
                    x += colWidths[i];
                }

                gfx.DrawLine(XPens.LightGray, marginL + pageW, y, marginL + pageW, y + rowH);
                y += rowH;
                gfx.DrawLine(XPens.LightGray, marginL, y, marginL + pageW, y);
            }

            doc.Save(path);

            try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch { }
        }
    }
}