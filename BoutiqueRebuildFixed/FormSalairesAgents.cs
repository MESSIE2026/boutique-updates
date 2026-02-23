using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Globalization;
using ITextRect = iTextSharp.text.Rectangle;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FormSalairesAgents : Form
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        private FormLikelembaWizard _likelembaWizard;
        // ✅ Id sélectionné (pour UPDATE)
        private int _idPaiementSelectionne = 0;

        // ✅ adapte ces 2 getters selon ton projet (SessionEmploye / Session / ConfigSysteme)
        private int GetIdEntreprise()
        {
            // EXEMPLE : return SessionEmploye.IdEntreprise;
            return ConfigSysteme.IdEntreprise; // <-- adapte si besoin
        }
        private int GetIdMagasin()
        {
            // EXEMPLE : return SessionEmploye.IdMagasin;
            return ConfigSysteme.IdMagasin; // <-- adapte si besoin
        }

        public FormSalairesAgents()
        {
            InitializeComponent();
            

            this.Load += FormSalairesAgents_Load;
            dgvSalairesAgents.SelectionChanged += DgvSalairesAgents_SelectionChanged;
            txtMontant.KeyPress += TxtMontant_KeyPress;

            ConfigurerInterface();

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;

        }

        private void FormSalairesAgents_Load(object sender, EventArgs e)
        {
            InitialiserListes();
            ChargerSalaires();
            ViderChamps();


            // Charger traductions dynamiques

            RafraichirLangue();
            RafraichirTheme();
        }

        private void RafraichirLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);
        }

        private void RafraichirTheme()
        {
            ConfigSysteme.AppliquerTheme(this);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 🔥 OBLIGATOIRE : éviter fuite mémoire
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;

            base.OnFormClosed(e);
        }
        private void ConfigurerInterface()
        {
            this.Text = "Gestion des salaires des agents";
            this.StartPosition = FormStartPosition.CenterScreen;

            dgvSalairesAgents.ReadOnly = true;
            dgvSalairesAgents.AllowUserToAddRows = false;
            dgvSalairesAgents.AllowUserToDeleteRows = false;
            dgvSalairesAgents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSalairesAgents.MultiSelect = false;
            dgvSalairesAgents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            btnEnregistrer.BackColor = Color.DarkGreen;
            btnEnregistrer.ForeColor = Color.White;

            btnFermer.BackColor = Color.DarkRed;
            btnFermer.ForeColor = Color.White;

            Button btnPrimes = new Button
            {
                Name = "btnPrimesCommissions",
                Text = "Primes/Commissions",
                BackColor = Color.MidnightBlue,
                ForeColor = Color.White,
                Width = btnEnregistrer.Width,
                Height = 32,

                // même alignement que Enregistrer
                Left = btnEnregistrer.Left,

                // au-dessus (8px d'espace)
                Top = btnEnregistrer.Top - 32 - 8
            };

            btnPrimes.Click += BtnPrimes_Click;
            this.Controls.Add(btnPrimes);
            btnPrimes.BringToFront();

            Button btnPerformance = new Button
            {
                Name = "btnPerformanceAgents",
                Text = "Performance Agents",
                BackColor = Color.Teal,
                ForeColor = Color.White,
                Width = btnEnregistrer.Width,
                Height = 32,

                // même alignement
                Left = btnEnregistrer.Left,

                // ✅ au-dessus de Primes (8px)
                Top = btnPrimes.Top - 32 - 8
            };

            btnPerformance.Click += (s, e) =>
            {
                using (var f = new FrmPerformanceAgents())
                {
                    f.ShowDialog(this);
                }
            };

            this.Controls.Add(btnPerformance);
            btnPerformance.BringToFront();
            txtMontant.ShortcutsEnabled = false; // bloque Ctrl+V etc.
        }
        private void InitialiserListes()
        {
            cboDevise.Items.Clear();
            cboDevise.Items.AddRange(new object[] { "USD", "CDF", "EUR" });
            cboDevise.SelectedIndex = 0;

            cboStatut.Items.Clear();
            cboStatut.Items.AddRange(new object[] { "Payé", "En attente", "Annulé" });
            cboStatut.SelectedIndex = 0;
        }
        private void ChargerSalaires()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string sql = @"
SELECT 
    Id,
    DatePaiement,
    ID_Employe,
    NomEmploye,
    Montant,
    Devise,
    Statut,
    Observations,
    IdEntreprise,
    IdMagasin
FROM SalairesPaiements
WHERE IdEntreprise = @IdEntreprise
  AND IdMagasin = @IdMagasin
ORDER BY DatePaiement DESC, Id DESC;";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                da.SelectCommand.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value = GetIdEntreprise();
                da.SelectCommand.Parameters.Add("@IdMagasin", SqlDbType.Int).Value = GetIdMagasin();

                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvSalairesAgents.DataSource = dt;
            }

            // ✅ Formatage PRO (sécurisé)
            if (dgvSalairesAgents.Columns.Contains("Id"))
                dgvSalairesAgents.Columns["Id"].HeaderText = "N°";

            if (dgvSalairesAgents.Columns.Contains("DatePaiement"))
            {
                dgvSalairesAgents.Columns["DatePaiement"].HeaderText = "Date paiement";
                dgvSalairesAgents.Columns["DatePaiement"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvSalairesAgents.Columns["DatePaiement"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (dgvSalairesAgents.Columns.Contains("ID_Employe"))
            {
                dgvSalairesAgents.Columns["ID_Employe"].HeaderText = "ID Employé";
                dgvSalairesAgents.Columns["ID_Employe"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (dgvSalairesAgents.Columns.Contains("NomEmploye"))
                dgvSalairesAgents.Columns["NomEmploye"].HeaderText = "Nom Employé";

            if (dgvSalairesAgents.Columns.Contains("Montant"))
            {
                dgvSalairesAgents.Columns["Montant"].HeaderText = "Montant";
                dgvSalairesAgents.Columns["Montant"].DefaultCellStyle.Format = "N2";
                dgvSalairesAgents.Columns["Montant"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (dgvSalairesAgents.Columns.Contains("Devise"))
            {
                dgvSalairesAgents.Columns["Devise"].HeaderText = "Devise";
                dgvSalairesAgents.Columns["Devise"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (dgvSalairesAgents.Columns.Contains("Statut"))
            {
                dgvSalairesAgents.Columns["Statut"].HeaderText = "Statut";
                dgvSalairesAgents.Columns["Statut"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            if (dgvSalairesAgents.Columns.Contains("Observations"))
                dgvSalairesAgents.Columns["Observations"].HeaderText = "Observations";

            // ✅ option : cacher colonnes internes
            if (dgvSalairesAgents.Columns.Contains("IdEntreprise"))
                dgvSalairesAgents.Columns["IdEntreprise"].Visible = false;
            if (dgvSalairesAgents.Columns.Contains("IdMagasin"))
                dgvSalairesAgents.Columns["IdMagasin"].Visible = false;

            dgvSalairesAgents.ClearSelection();
        }
        private void ViderChamps()
        {
            _idPaiementSelectionne = 0;

            txtIdEmploye.Clear();
            txtNomEmploye.Clear();
            txtMontant.Clear();
            txtObservations.Clear();
            cboDevise.SelectedIndex = 0;
            cboStatut.SelectedIndex = 0;
            dtpDatePaiement.Value = DateTime.Now;
            dgvSalairesAgents.ClearSelection();
        }
        private void DgvSalairesAgents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSalairesAgents.CurrentRow == null) return;

            var row = dgvSalairesAgents.CurrentRow;

            // ✅ garder l’ID pour UPDATE
            _idPaiementSelectionne = 0;
            if (row.Cells["Id"].Value != null && row.Cells["Id"].Value != DBNull.Value)
                _idPaiementSelectionne = Convert.ToInt32(row.Cells["Id"].Value);

            dtpDatePaiement.Value = Convert.ToDateTime(row.Cells["DatePaiement"].Value);
            txtIdEmploye.Text = row.Cells["ID_Employe"].Value.ToString();
            txtNomEmploye.Text = row.Cells["NomEmploye"].Value.ToString();
            txtMontant.Text = row.Cells["Montant"].Value.ToString();
            cboDevise.Text = row.Cells["Devise"].Value.ToString();
            cboStatut.Text = row.Cells["Statut"].Value.ToString();
            txtObservations.Text = row.Cells["Observations"].Value.ToString();
        }
        private void TxtMontant_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Autoriser chiffres, contrôle, '.' et ','
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',')
                e.Handled = true;

            // Empêcher 2 séparateurs décimaux
            if ((e.KeyChar == '.' || e.KeyChar == ',') && (txtMontant.Text.Contains(".") || txtMontant.Text.Contains(",")))
                e.Handled = true;
        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtIdEmploye.Text.Trim(), out int idEmploye) || idEmploye <= 0)
            {
                MessageBox.Show("ID Employé invalide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNomEmploye.Text))
            {
                MessageBox.Show("Nom Employé obligatoire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Accepte virgule ou point
            var raw = (txtMontant.Text ?? "").Trim().Replace(',', '.');
            if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal montant) || montant < 0)
            {
                MessageBox.Show("Montant invalide.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idEntreprise = GetIdEntreprise();
            int idMagasin = GetIdMagasin();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    bool modeUpdate = (_idPaiementSelectionne > 0);

                    string sqlInsert = @"
INSERT INTO SalairesPaiements
(DatePaiement, ID_Employe, NomEmploye, Montant, Devise, Statut, Observations, IdEntreprise, IdMagasin)
VALUES
(@DatePaiement, @ID_Employe, @NomEmploye, @Montant, @Devise, @Statut, @Observations, @IdEntreprise, @IdMagasin);";

                    string sqlUpdate = @"
UPDATE SalairesPaiements SET
    DatePaiement = @DatePaiement,
    ID_Employe = @ID_Employe,
    NomEmploye = @NomEmploye,
    Montant = @Montant,
    Devise = @Devise,
    Statut = @Statut,
    Observations = @Observations
WHERE Id = @Id
  AND IdEntreprise = @IdEntreprise
  AND IdMagasin = @IdMagasin;";

                    using (SqlCommand cmd = new SqlCommand(modeUpdate ? sqlUpdate : sqlInsert, con))
                    {
                        cmd.Parameters.Add("@DatePaiement", SqlDbType.Date).Value = dtpDatePaiement.Value.Date;
                        cmd.Parameters.Add("@ID_Employe", SqlDbType.Int).Value = idEmploye;
                        cmd.Parameters.Add("@NomEmploye", SqlDbType.NVarChar, 100).Value = txtNomEmploye.Text.Trim();

                        var pMontant = cmd.Parameters.Add("@Montant", SqlDbType.Decimal);
                        pMontant.Precision = 18;
                        pMontant.Scale = 2;
                        pMontant.Value = montant;

                        cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = cboDevise.Text;
                        cmd.Parameters.Add("@Statut", SqlDbType.NVarChar, 50).Value = cboStatut.Text;
                        cmd.Parameters.Add("@Observations", SqlDbType.NVarChar, 255).Value = (txtObservations.Text ?? "").Trim();

                        cmd.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value = idEntreprise;
                        cmd.Parameters.Add("@IdMagasin", SqlDbType.Int).Value = idMagasin;

                        if (modeUpdate)
                            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = _idPaiementSelectionne;

                        int rows = cmd.ExecuteNonQuery();

                        if (modeUpdate && rows == 0)
                        {
                            MessageBox.Show("Modification non effectuée (ligne introuvable ou accès magasin/entreprise invalide).",
                                "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                }

                MessageBox.Show(_idPaiementSelectionne > 0 ? "Paiement modifié avec succès." : "Paiement enregistré avec succès.",
                    "Gestion des salaires", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ChargerSalaires();
                ViderChamps();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Erreur SQL : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private DataTable ChargerSalairesPourExport(DateTime jour)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
SELECT 
    Id,
    DatePaiement,
    ID_Employe,
    NomEmploye,
    Montant,
    Devise,
    Statut,
    Observations
FROM SalairesPaiements
WHERE CONVERT(date, DatePaiement) = @Jour
  AND IdEntreprise = @IdEntreprise
  AND IdMagasin = @IdMagasin
ORDER BY Id ASC;";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@Jour", SqlDbType.Date).Value = jour.Date;

                    // ✅ adapte ces 2 lignes à ta session réelle si besoin
                    cmd.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value = GetIdEntreprise();
                    cmd.Parameters.Add("@IdMagasin", SqlDbType.Int).Value = GetIdMagasin();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }



        private void btnNouveau_Click(object sender, EventArgs e)
        {
            ViderChamps();
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnPrimes_Click(object sender, EventArgs e)
        {
            using (var f = new FrmPrimesCommissions())
            {
                var r = f.ShowDialog(this);
                if (r != DialogResult.OK) return;

                // Ici : intégrer prime+commission dans le montant du salaire
                // (on ajoute au montant actuel)
                decimal actuel = 0m;
                var raw = (txtMontant.Text ?? "").Trim().Replace(',', '.');
                decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out actuel);

                decimal nouveau = actuel + f.Result.Total;

                txtMontant.Text = nouveau.ToString("0.00", CultureInfo.InvariantCulture);

                // Option : aligner devise automatiquement
                cboDevise.Text = f.Result.Devise;

                // Option : tracer en observation
                txtObservations.Text = (txtObservations.Text ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(txtObservations.Text)) txtObservations.Text += " | ";
                txtObservations.Text += $"Prime+Com({f.Result.PeriodeDebut:dd/MM}–{f.Result.PeriodeFin:dd/MM})={f.Result.Total:0.00} {f.Result.Devise}";
            }
        }

        private void btnExporterPDF_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Fichier PDF (*.pdf)|*.pdf",
                FileName = $"Salaires_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            // ✅ Export du JOUR uniquement (aujourd’hui)
            DateTime jourExport = DateTime.Today;

            // ✅ Titre reste "mois précédent" (comme tu as validé)
            DateTime moisSalaireDate = DateTime.Today.AddMonths(-1);
            string moisSalaire = moisSalaireDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("fr-FR")).ToUpper();

            Document doc = new Document(PageSize.A4.Rotate(), 25f, 25f, 25f, 25f);

            try
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(sfd.FileName, FileMode.Create));
                doc.Open();

                // =========================
                // FONTS (PLUS LISIBLES)
                // =========================
                var fontTitre = FontFactory.GetFont(FontFactory.HELVETICA, 16f, iTextSharp.text.Font.BOLD);
                var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA, 12f, iTextSharp.text.Font.BOLD);
                var fontSmall = FontFactory.GetFont(FontFactory.HELVETICA, 10.5f, iTextSharp.text.Font.NORMAL);
                var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 11f, iTextSharp.text.Font.NORMAL);
                var fontFooter = FontFactory.GetFont(FontFactory.HELVETICA, 11f, iTextSharp.text.Font.NORMAL);

                // =========================
                // ENTÊTE PRO : gauche + logo droite (OK)
                // =========================
                PdfPTable top = new PdfPTable(2) { WidthPercentage = 100f };
                top.SetWidths(new float[] { 75f, 25f });

                PdfPCell left = new PdfPCell { Border = PdfPCell.NO_BORDER };
                left.AddElement(new Paragraph("ZAIRE MODE SARL",
                    FontFactory.GetFont(FontFactory.HELVETICA, 15f, iTextSharp.text.Font.BOLD)));
                left.AddElement(new Paragraph("23, Bld Lumumba / Immeuble Masina Plaza", fontSmall));
                left.AddElement(new Paragraph("+243861507560 / E-MAIL: Zaireshop@hotmail.com", fontSmall));
                left.AddElement(new Paragraph("PAGE: ZAIRE.CD", fontSmall));
                left.AddElement(new Paragraph("RCCM: 25-B-01497", fontSmall));
                left.AddElement(new Paragraph("IDNAT: 01-F4300-N73258E", fontSmall));
                top.AddCell(left);

                PdfPCell right = new PdfPCell
                {
                    Border = PdfPCell.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                string logoPath = @"D:\ZAIRE\LOGO1.png";
                if (File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(95f, 95f);
                    logo.Alignment = Element.ALIGN_RIGHT;
                    right.AddElement(logo);
                }
                top.AddCell(right);

                doc.Add(top);

                // Ligne de séparation
                PdfPTable sep = new PdfPTable(1) { WidthPercentage = 100f, SpacingBefore = 8f, SpacingAfter = 10f };
                PdfPCell sepCell = new PdfPCell(new Phrase(""))
                {
                    Border = iTextSharp.text.Rectangle.BOTTOM_BORDER,
                    BorderWidthBottom = 1.2f,
                    PaddingBottom = 4f
                };
                sep.AddCell(sepCell);
                doc.Add(sep);

                // =========================
                // TITRE
                // =========================
                Paragraph titre = new Paragraph($"SALAIRE DU MOIS DE {moisSalaire}", fontTitre)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 6f
                };
                doc.Add(titre);

                Paragraph sous = new Paragraph($"Export du : {jourExport:dd/MM/yyyy}", fontSmall)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 12f
                };
                doc.Add(sous);

                // =========================
                // TABLEAU : export depuis SQL (du JOUR + entreprise + magasin)
                // =========================
                DataTable dtExport = ChargerSalairesPourExport(jourExport);

                if (dtExport.Rows.Count == 0)
                {
                    MessageBox.Show(
                        $"Aucun paiement trouvé pour la date {jourExport:dd/MM/yyyy}.",
                        "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // ✅ 8 colonnes fixes (comme ton SELECT export)
                int colCount = 8;

                PdfPTable table = new PdfPTable(colCount)
                {
                    WidthPercentage = 100f,
                    HeaderRows = 1,
                    SpacingBefore = 5f
                };

                // Largeurs PRO : Id, Date, ID, Nom, Montant, Devise, Statut, Observations
                try
                {
                    table.SetWidths(new float[] { 6f, 10f, 10f, 24f, 12f, 8f, 10f, 20f });
                }
                catch { /* ignore */ }

                // ✅ En-têtes fixes (ne dépend plus du DataGridView)
                string[] headers = { "N°", "Date paiement", "ID Employé", "Nom Employé", "Montant", "Devise", "Statut", "Observations" };
                foreach (string htxt in headers)
                {
                    PdfPCell h = new PdfPCell(new Phrase(htxt, fontHeader))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        BackgroundColor = new BaseColor(235, 235, 235),
                        PaddingTop = 7f,
                        PaddingBottom = 7f,
                        PaddingLeft = 5f,
                        PaddingRight = 5f
                    };
                    table.AddCell(h);
                }

                int lignesAjoutees = 0;
                decimal totalMontant = 0m;

                // ✅ Remplissage depuis SQL
                foreach (DataRow dr in dtExport.Rows)
                {
                    bool alt = (lignesAjoutees % 2 == 1);
                    BaseColor bg = alt ? new BaseColor(250, 250, 250) : BaseColor.WHITE;

                    // ordre des colonnes identique à ton SELECT export
                    string[] cols = { "Id", "DatePaiement", "ID_Employe", "NomEmploye", "Montant", "Devise", "Statut", "Observations" };

                    foreach (string colName in cols)
                    {
                        string txt = dr[colName]?.ToString() ?? "";

                        // ✅ format date propre
                        if (colName == "DatePaiement")
                        {
                            if (dr["DatePaiement"] is DateTime dte)
                                txt = dte.ToString("dd/MM/yyyy");
                            else if (DateTime.TryParse(txt, out DateTime parsed))
                                txt = parsed.ToString("dd/MM/yyyy");
                        }

                        // ✅ format montant propre
                        if (colName == "Montant")
                        {
                            var rawM = txt.Trim().Replace(" ", "").Replace(",", ".");
                            if (decimal.TryParse(rawM, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal mFmt))
                                txt = mFmt.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"));
                        }

                        PdfPCell c = new PdfPCell(new Phrase(txt, fontCell))
                        {
                            BackgroundColor = bg,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            PaddingTop = 6f,
                            PaddingBottom = 6f,
                            PaddingLeft = 5f,
                            PaddingRight = 5f
                        };

                        // ✅ Centrage demandé
                        if (colName == "Id" || colName == "Devise" || colName == "Montant" || colName == "Statut"
                            || colName == "DatePaiement" || colName == "ID_Employe")
                            c.HorizontalAlignment = Element.ALIGN_CENTER;
                        else
                            c.HorizontalAlignment = Element.ALIGN_LEFT;

                        // ✅ Total Montant (sur valeur brute)
                        if (colName == "Montant")
                        {
                            var rawSum = (dr["Montant"]?.ToString() ?? "").Trim().Replace(" ", "").Replace(",", ".");
                            if (decimal.TryParse(rawSum, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal mSum))
                                totalMontant += mSum;
                        }

                        table.AddCell(c);
                    }

                    lignesAjoutees++;
                }

                doc.Add(table);

                // =========================
                // TOTAL (PRO)
                // =========================
                doc.Add(new Paragraph("\n"));

                PdfPTable tblTotal = new PdfPTable(2)
                {
                    WidthPercentage = 35f,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 5f
                };
                tblTotal.SetWidths(new float[] { 50f, 50f });

                PdfPCell t1 = new PdfPCell(new Phrase("TOTAL", fontHeader))
                {
                    BackgroundColor = new BaseColor(235, 235, 235),
                    Padding = 8f,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                PdfPCell t2 = new PdfPCell(new Phrase(totalMontant.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")), fontHeader))
                {
                    BackgroundColor = new BaseColor(235, 235, 235),
                    Padding = 8f,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                tblTotal.AddCell(t1);
                tblTotal.AddCell(t2);
                doc.Add(tblTotal);

                // =========================
                // SIGNATURE (PROPRE, PAS AU COIN)
                // =========================
                doc.Add(new Paragraph("\n\n"));

                PdfPTable sign = new PdfPTable(2) { WidthPercentage = 100f };
                sign.SetWidths(new float[] { 50f, 50f });

                PdfPCell sLeft = new PdfPCell(new Phrase(" ", fontFooter))
                {
                    Border = PdfPCell.NO_BORDER
                };

                PdfPCell sRight = new PdfPCell { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };

                sRight.AddElement(new Paragraph($"Fait à Kinshasa, le {DateTime.Now:dd/MM/yyyy}", fontFooter)
                { Alignment = Element.ALIGN_CENTER });

                sRight.AddElement(new Paragraph("Le Comptable", fontFooter)
                { Alignment = Element.ALIGN_CENTER, SpacingBefore = 10f });

                // Ligne signature
                sRight.AddElement(new Paragraph("______________________________", fontFooter)
                { Alignment = Element.ALIGN_CENTER, SpacingBefore = 20f });

                sRight.AddElement(new Paragraph("MESSIE MATALA", fontFooter)
                { Alignment = Element.ALIGN_CENTER, SpacingBefore = 6f });

                sign.AddCell(sLeft);
                sign.AddCell(sRight);
                doc.Add(sign);

                MessageBox.Show(
                    $"Export PDF réussi.\nMois salaire : {moisSalaire}\nDate export : {jourExport:dd/MM/yyyy}\nLignes exportées : {lignesAjoutees}",
                    "PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur PDF : " + ex.Message);
            }
            finally
            {
                if (doc.IsOpen()) doc.Close();
            }
        }

        private void btnLikelemba_Click(object sender, EventArgs e)
        {
            try
            {
                if (_likelembaWizard == null || _likelembaWizard.IsDisposed)
                {
                    _likelembaWizard = new FormLikelembaWizard();
                    _likelembaWizard.Owner = this;

                    // Si un jour on ferme vraiment (Close), on remet null
                    _likelembaWizard.FormClosed += (s, ev) => _likelembaWizard = null;
                }

                // Option A: tu laisses le formulaire salaires ouvert
                _likelembaWizard.Show();
                _likelembaWizard.BringToFront();
                _likelembaWizard.Activate();

                // Option B: si tu veux cacher Salaires quand Likelemba s’ouvre :
                // this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ouverture Likelemba : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
