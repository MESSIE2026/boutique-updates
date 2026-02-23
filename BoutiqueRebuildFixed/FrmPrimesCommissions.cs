using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{

    public static class PromptBox
    {
        public static string ShowDialog(string text, string caption, bool password = false)
        {
            Form prompt = new Form();
            prompt.Width = 420;
            prompt.Height = 170;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.Text = caption;
            prompt.StartPosition = FormStartPosition.CenterParent;
            prompt.MaximizeBox = false;
            prompt.MinimizeBox = false;

            Label textLabel = new Label() { Left = 12, Top = 15, Width = 380, Text = text };
            TextBox inputBox = new TextBox() { Left = 12, Top = 45, Width = 380 };
            if (password) inputBox.UseSystemPasswordChar = true;

            Button ok = new Button() { Text = "OK", Left = 232, Width = 75, Top = 80, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Annuler", Left = 317, Width = 75, Top = 80, DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(ok);
            prompt.Controls.Add(cancel);

            prompt.AcceptButton = ok;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? inputBox.Text : "";
        }
    }
public class PrimeCommissionResult
    {
        public int IdEmploye { get; set; }
        public string NomEmploye { get; set; } = "";
        public DateTime PeriodeDebut { get; set; }
        public DateTime PeriodeFin { get; set; }
        public decimal TotalVentes { get; set; }
        public decimal Pourcentage { get; set; }
        public decimal CommissionAuto { get; set; }
        public decimal PrimeManuelle { get; set; }
        public decimal Total => CommissionAuto + PrimeManuelle;
        public string Devise { get; set; } = "CDF";
        public string Statut { get; set; } = "En attente";
    }
    public partial class FrmPrimesCommissions : Form
    {
        private readonly string _cs = ConfigSysteme.ConnectionString;
        private int _lastInsertId = 0;

        public PrimeCommissionResult Result { get; private set; } = new PrimeCommissionResult();

        ComboBox cboEmploye = new ComboBox();
        TextBox txtTotalVentes = new TextBox();
        TextBox txtPourcentage = new TextBox();
        TextBox txtCommissionAuto = new TextBox();
        TextBox txtPrimeManuelle = new TextBox();
        TextBox txtTotal = new TextBox();
        ComboBox cboDevise = new ComboBox();
        ComboBox cboStatut = new ComboBox();
        DateTimePicker dtpDebut = new DateTimePicker();
        DateTimePicker dtpFin = new DateTimePicker();
        TextBox txtObs = new TextBox();

        Button btnCalculer = new Button();
        Button btnEnregistrer = new Button();
        Button btnValiderManager = new Button();
        Button btnFermer = new Button();
        Button btnExporterPDF = new Button();
        public FrmPrimesCommissions()
        {
            InitializeComponent();

            Text = "Primes / Commissions";
            StartPosition = FormStartPosition.CenterParent;
            Width = 720;
            Height = 520;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            BuildUI();
            Load += (s, e) =>
            {
                InitLists();
                ChargerEmployes();
                dtpDebut.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dtpFin.Value = dtpDebut.Value.AddMonths(1).AddDays(-1);
                Recalc();
            };

            btnCalculer.Click += (s, e) => { ChargerTotalVentes(); Recalc(); };
            btnEnregistrer.Click += (s, e) => Enregistrer("En attente");
            btnValiderManager.Click += (s, e) => ValiderManager();
            btnExporterPDF.Click += (s, e) => ExporterPDF();
            btnFermer.Click += (s, e) => Close();

            txtPourcentage.TextChanged += (s, e) => Recalc();
            txtPrimeManuelle.TextChanged += (s, e) => Recalc();

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        private void FrmPrimesCommissions_Load(object sender, EventArgs e)
        {

        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }

        private void RafraichirLangue() => ConfigSysteme.AppliquerTraductions(this);
        private void RafraichirTheme() => ConfigSysteme.AppliquerTheme(this);

        private void InitLists()
        {
            cboDevise.Items.Clear();
            cboDevise.Items.AddRange(new object[] { "USD", "CDF", "EUR" });
            cboDevise.SelectedIndex = 1; // CDF

            cboStatut.Items.Clear();
            cboStatut.Items.AddRange(new object[] { "En attente", "Validé", "Annulé" });
            cboStatut.SelectedIndex = 0;

            cboEmploye.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatut.DropDownStyle = ComboBoxStyle.DropDownList;

            dtpDebut.Format = DateTimePickerFormat.Short;
            dtpFin.Format = DateTimePickerFormat.Short;

            txtTotalVentes.ReadOnly = true;
            txtCommissionAuto.ReadOnly = true;
            txtTotal.ReadOnly = true;

            txtTotalVentes.TextAlign = HorizontalAlignment.Right;
            txtPourcentage.TextAlign = HorizontalAlignment.Right;
            txtCommissionAuto.TextAlign = HorizontalAlignment.Right;
            txtPrimeManuelle.TextAlign = HorizontalAlignment.Right;
            txtTotal.TextAlign = HorizontalAlignment.Right;

            txtObs.Multiline = true;
            txtObs.Height = 55;
        }

        private void BuildUI()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14) };
            Controls.Add(panel);

            int y = 10;

            panel.Controls.Add(MakeLabel("Employé", 10, y));
            cboEmploye.SetBounds(160, y - 3, 500, 28);
            panel.Controls.Add(cboEmploye);
            y += 40;

            panel.Controls.Add(MakeLabel("Période début", 10, y));
            dtpDebut.SetBounds(160, y - 3, 160, 28);
            panel.Controls.Add(dtpDebut);

            panel.Controls.Add(MakeLabel("Période fin", 350, y));
            dtpFin.SetBounds(460, y - 3, 200, 28);
            panel.Controls.Add(dtpFin);
            y += 40;

            panel.Controls.Add(MakeLabel("Devise", 10, y));
            cboDevise.SetBounds(160, y - 3, 160, 28);
            panel.Controls.Add(cboDevise);

            panel.Controls.Add(MakeLabel("Statut", 350, y));
            cboStatut.SetBounds(460, y - 3, 200, 28);
            panel.Controls.Add(cboStatut);
            y += 40;

            panel.Controls.Add(MakeLabel("Total ventes (auto)", 10, y));
            txtTotalVentes.SetBounds(160, y - 3, 160, 28);
            panel.Controls.Add(txtTotalVentes);

            btnCalculer.Text = "Calculer ventes";
            btnCalculer.SetBounds(350, y - 4, 150, 30);
            panel.Controls.Add(btnCalculer);
            y += 40;

            panel.Controls.Add(MakeLabel("% Commission", 10, y));
            txtPourcentage.SetBounds(160, y - 3, 160, 28);
            txtPourcentage.Text = "0";
            panel.Controls.Add(txtPourcentage);

            panel.Controls.Add(MakeLabel("Commission (auto)", 350, y));
            txtCommissionAuto.SetBounds(460, y - 3, 200, 28);
            panel.Controls.Add(txtCommissionAuto);
            y += 40;

            panel.Controls.Add(MakeLabel("Prime manuelle", 10, y));
            txtPrimeManuelle.SetBounds(160, y - 3, 160, 28);
            txtPrimeManuelle.Text = "0";
            panel.Controls.Add(txtPrimeManuelle);

            panel.Controls.Add(MakeLabel("Total (Prime+Com)", 350, y));
            txtTotal.SetBounds(460, y - 3, 200, 28);
            panel.Controls.Add(txtTotal);
            y += 45;

            panel.Controls.Add(MakeLabel("Observations", 10, y));
            txtObs.SetBounds(160, y - 3, 500, 60);
            panel.Controls.Add(txtObs);
            y += 80;

            btnEnregistrer.Text = "Enregistrer";
            btnEnregistrer.BackColor = Color.DarkGreen;
            btnEnregistrer.ForeColor = Color.White;
            btnEnregistrer.SetBounds(160, y, 140, 38);
            panel.Controls.Add(btnEnregistrer);

            btnValiderManager.Text = "Valider Manager";
            btnValiderManager.BackColor = Color.DarkBlue;
            btnValiderManager.ForeColor = Color.White;
            btnValiderManager.SetBounds(315, y, 170, 38);
            panel.Controls.Add(btnValiderManager);

            btnExporterPDF.Text = "Exporter PDF";
            btnExporterPDF.BackColor = Color.DimGray;
            btnExporterPDF.ForeColor = Color.White;
            btnExporterPDF.SetBounds(160, y + 45, 140, 38); // tu peux ajuster position
            panel.Controls.Add(btnExporterPDF);

            btnFermer.Text = "Fermer";
            btnFermer.BackColor = Color.DarkRed;
            btnFermer.ForeColor = Color.White;
            btnFermer.SetBounds(500, y, 160, 38);
            panel.Controls.Add(btnFermer);
        }

        private Label MakeLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Left = x,
                Top = y + 3
            };
        }

        private void ChargerEmployes()
        {
            using (SqlConnection con = new SqlConnection(_cs))
            {
                con.Open();

                var dt = new DataTable();

                // Affiche "Nom Prenom" dans la ComboBox, garde ID_Employe comme valeur
                string sql = @"
            SELECT 
                ID_Employe,
                LTRIM(RTRIM(Nom)) + ' ' + LTRIM(RTRIM(Prenom)) AS NomComplet
            FROM Employes
            ORDER BY Nom, Prenom";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                cboEmploye.DataSource = dt;
                cboEmploye.DisplayMember = "NomComplet";
                cboEmploye.ValueMember = "ID_Employe";
                cboEmploye.SelectedIndex = dt.Rows.Count > 0 ? 0 : -1;
            }
        }


        private void ChargerTotalVentes()
        {
            if (cboEmploye.SelectedValue == null) return;

            int idEmp = Convert.ToInt32(cboEmploye.SelectedValue);

            DateTime d1 = dtpDebut.Value.Date;
            DateTime d2excl = dtpFin.Value.Date.AddDays(1); // fin exclusive
            string devise = cboDevise.Text;

            string sql = @"
        SELECT ISNULL(SUM(MontantTotal), 0)
        FROM Vente
        WHERE IDEmploye = @id
          AND DateVente >= @d1 AND DateVente < @d2excl
          AND Devise = @dev
          AND ISNULL(Statut,'') NOT IN ('Annulé','Annule','ANNULÉ','ANNULE')";

            using (SqlConnection con = new SqlConnection(_cs))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmp;
                    cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = d1;
                    cmd.Parameters.Add("@d2excl", SqlDbType.DateTime).Value = d2excl;
                    cmd.Parameters.Add("@dev", SqlDbType.NVarChar, 10).Value = devise;

                    decimal total = Convert.ToDecimal(cmd.ExecuteScalar() ?? 0m);
                    txtTotalVentes.Text = total.ToString("0.00", CultureInfo.InvariantCulture);
                }
            }
        }

        private void Recalc()
        {
            decimal totalVentes = ParseDecimal(txtTotalVentes.Text);
            decimal pct = ParseDecimal(txtPourcentage.Text);
            decimal prime = ParseDecimal(txtPrimeManuelle.Text);

            decimal commission = Math.Round(totalVentes * pct / 100m, 2);
            decimal total = commission + prime;

            txtCommissionAuto.Text = commission.ToString("0.00", CultureInfo.InvariantCulture);
            txtTotal.Text = total.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private void ExporterPDF()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Fichier PDF (*.pdf)|*.pdf",
                FileName = $"PrimesCommissions_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            Document doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);

            try
            {
                PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.Create));
                doc.Open();

                // Fonts
                var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA, 13f, iTextSharp.text.Font.BOLD);
                var fontSmall = FontFactory.GetFont(FontFactory.HELVETICA, 9f, iTextSharp.text.Font.NORMAL);
                var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA, 10f, iTextSharp.text.Font.BOLD);
                var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 9f, iTextSharp.text.Font.NORMAL);

                // Helper pour centrer comme ton "DrawCentered"
                Action<string, iTextSharp.text.Font, float> DrawCentered = (text, font, spacingAfter) =>
                {
                    var p = new Paragraph(text, font) { Alignment = Element.ALIGN_CENTER, SpacingAfter = spacingAfter };
                    doc.Add(p);
                };

                // ================= ENTÊTE PRO =================
                DrawCentered("ZAIRE MODE SARL", fontTitle, 2);
                DrawCentered("23, Bld Lumumba, Q1 Masina Sans Fil", fontSmall, 2);
                DrawCentered("RCCM: 25-B-01497 | ID.NAT: 01-F4300-N73258E", fontSmall, 12);

                // Titre
                DrawCentered("FICHE PRIMES / COMMISSIONS", fontHeader, 10);

                // Infos principales
                PdfPTable info = new PdfPTable(2) { WidthPercentage = 100f };
                info.SetWidths(new float[] { 30f, 70f });

                AddInfoRow(info, "Employé", cboEmploye.Text, fontHeader, fontCell);
                AddInfoRow(info, "Période", $"{dtpDebut.Value:dd/MM/yyyy}  au  {dtpFin.Value:dd/MM/yyyy}", fontHeader, fontCell);
                AddInfoRow(info, "Devise", cboDevise.Text, fontHeader, fontCell);
                AddInfoRow(info, "Statut", cboStatut.Text, fontHeader, fontCell);

                doc.Add(info);
                doc.Add(new Paragraph("\n"));

                // Tableau chiffres
                PdfPTable t = new PdfPTable(2) { WidthPercentage = 70f, HorizontalAlignment = Element.ALIGN_LEFT };
                t.SetWidths(new float[] { 50f, 50f });

                AddMoneyRow(t, "Total ventes (auto)", txtTotalVentes.Text, fontHeader, fontCell);
                AddMoneyRow(t, "% Commission", txtPourcentage.Text, fontHeader, fontCell);
                AddMoneyRow(t, "Commission (auto)", txtCommissionAuto.Text, fontHeader, fontCell);
                AddMoneyRow(t, "Prime manuelle", txtPrimeManuelle.Text, fontHeader, fontCell);
                AddMoneyRow(t, "TOTAL (Prime+Com)", txtTotal.Text, fontHeader, fontCell);

                doc.Add(t);

                // Observations
                if (!string.IsNullOrWhiteSpace(txtObs.Text))
                {
                    doc.Add(new Paragraph("\n"));
                    doc.Add(new Paragraph("Observations :", fontHeader));
                    doc.Add(new Paragraph(txtObs.Text.Trim(), fontCell));
                }

                // Footer
                doc.Add(new Paragraph("\n\n"));
                var footer = new Paragraph($"Fait à Kinshasa, le {DateTime.Now:dd/MM/yyyy}\n\nLe Manager / Responsable\n\n____________________", fontSmall)
                {
                    Alignment = Element.ALIGN_RIGHT
                };
                doc.Add(footer);

                MessageBox.Show("Export PDF réussi.", "PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur PDF : " + ex.Message, "PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                doc.Close();
            }
        }

        private void AddInfoRow(PdfPTable table, string label, string value,
            iTextSharp.text.Font fontLabel, iTextSharp.text.Font fontValue)
        {
            var c1 = new PdfPCell(new Phrase(label, fontLabel)) { Padding = 5f, BackgroundColor = BaseColor.LIGHT_GRAY };
            var c2 = new PdfPCell(new Phrase(value ?? "", fontValue)) { Padding = 5f };
            table.AddCell(c1);
            table.AddCell(c2);
        }

        private void AddMoneyRow(PdfPTable table, string label, string value,
            iTextSharp.text.Font fontLabel, iTextSharp.text.Font fontValue)
        {
            var c1 = new PdfPCell(new Phrase(label, fontLabel)) { Padding = 5f };
            var c2 = new PdfPCell(new Phrase(value ?? "", fontValue))
            {
                Padding = 5f,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            table.AddCell(c1);
            table.AddCell(c2);
        }

        private decimal ParseDecimal(string s)
        {
            var raw = (s ?? "").Trim().Replace(',', '.');

            decimal v;
            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out v))
                return v;

            return 0m;
        }

        private void Enregistrer(string statut)
        {
            if (cboEmploye.SelectedValue == null)
            {
                MessageBox.Show("Choisis un employé.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idEmp = Convert.ToInt32(cboEmploye.SelectedValue);
            string nomEmp = cboEmploye.Text;

            decimal totalVentes = ParseDecimal(txtTotalVentes.Text);
            decimal pct = ParseDecimal(txtPourcentage.Text);
            decimal commission = ParseDecimal(txtCommissionAuto.Text);
            decimal prime = ParseDecimal(txtPrimeManuelle.Text);

            string devise = cboDevise.Text;

            using (SqlConnection con = new SqlConnection(_cs))
            {
                con.Open();

                string sql = @"
                    INSERT INTO PrimesCommissions
                    (PeriodeDebut, PeriodeFin, ID_Employe, NomEmploye, TotalVentes, Pourcentage, CommissionAuto, PrimeManuelle, Devise, Statut, Observations)
                    VALUES
                    (@d1,@d2,@id,@nom,@tv,@pct,@com,@prime,@dev,@statut,@obs);
                    SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@d1", SqlDbType.Date).Value = dtpDebut.Value.Date;
                    cmd.Parameters.Add("@d2", SqlDbType.Date).Value = dtpFin.Value.Date;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEmp;
                    cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 100).Value = nomEmp;

                    AddDecimal(cmd, "@tv", totalVentes);
                    AddDecimal(cmd, "@pct", pct, 9, 2);
                    AddDecimal(cmd, "@com", commission);
                    AddDecimal(cmd, "@prime", prime);

                    cmd.Parameters.Add("@dev", SqlDbType.NVarChar, 10).Value = devise;
                    cmd.Parameters.Add("@statut", SqlDbType.NVarChar, 30).Value = statut;
                    cmd.Parameters.Add("@obs", SqlDbType.NVarChar, 255).Value = (txtObs.Text ?? "").Trim();

                    _lastInsertId = Convert.ToInt32(cmd.ExecuteScalar());
                    cboStatut.Text = statut;

                    Result = new PrimeCommissionResult
                    {
                        IdEmploye = idEmp,
                        NomEmploye = nomEmp,
                        PeriodeDebut = dtpDebut.Value.Date,
                        PeriodeFin = dtpFin.Value.Date,
                        TotalVentes = totalVentes,
                        Pourcentage = pct,
                        CommissionAuto = commission,
                        PrimeManuelle = prime,
                        Devise = devise,
                        Statut = statut
                    };

                    MessageBox.Show("Enregistré.", "Primes/Commissions", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Pour intégration salaire : on renvoie OK même en "En attente" si tu veux déjà importer
                    this.DialogResult = DialogResult.OK;
                }
            }
        }

        

        private string GetValideurNomComplet()
        {
            // Adapte selon ton objet SessionEmploye
            // Exemple :
            // return (SessionEmploye.Nom + " " + SessionEmploye.Prenom).Trim();

            // Si tu n'as que NomUtilisateur :
            return (SessionEmploye.Nom ?? "Inconnu").Trim();
        }

        private void ValiderManager()
        {
            // ✅ droits au début (exactement comme ton modèle)
            if (string.IsNullOrWhiteSpace(SessionEmploye.Poste) ||
                !(SessionEmploye.Poste.Equals("Directeur", StringComparison.OrdinalIgnoreCase) ||
                  SessionEmploye.Poste.Equals("Superviseur", StringComparison.OrdinalIgnoreCase) ||
                  SessionEmploye.Poste.Equals("RH", StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Vous n'avez pas l'autorisation de valider.", "Accès refusé",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ si déjà validé, on stop
            if (!string.IsNullOrWhiteSpace(cboStatut.Text) &&
                cboStatut.Text.Equals("Validé", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Cette fiche est déjà validée.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // ✅ il faut un enregistrement en base à valider
            // Si pas encore enregistré, on enregistre d'abord en "En attente"
            if (_lastInsertId <= 0)
            {
                Enregistrer("En attente");
                if (_lastInsertId <= 0) return; // sécurité
            }

            string validePar = GetValideurNomComplet();

            using (SqlConnection con = new SqlConnection(_cs))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.PrimesCommissions
SET Statut = N'Validé',
    DateValidation = GETDATE(),
    ValidePar = @ValidePar
WHERE Id = @Id
  AND ISNULL(Statut,'') <> N'Validé';", con))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = _lastInsertId;
                    cmd.Parameters.Add("@ValidePar", SqlDbType.NVarChar, 100).Value = validePar;
                    cmd.ExecuteNonQuery();
                }
            }

            cboStatut.Text = "Validé";
            Result.Statut = "Validé";

            MessageBox.Show($"Validé ✅ par {validePar}", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Option : renvoyer OK pour que FormSalairesAgents récupère direct
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// ✅ Remplace ceci par ton système réel (login, rôle, PIN, etc.)
        /// </summary>
        private bool VerifierManager()
        {
            string code = PromptBox.ShowDialog("Entrez le code manager :", "Validation manager", true);
            return !string.IsNullOrWhiteSpace(code) && code == "0000";
        }

        private void AddDecimal(SqlCommand cmd, string name, decimal value, byte precision = 18, byte scale = 2)
        {
            var p = cmd.Parameters.Add(name, SqlDbType.Decimal);
            p.Precision = precision;
            p.Scale = scale;
            p.Value = value;
        }
    }
}
