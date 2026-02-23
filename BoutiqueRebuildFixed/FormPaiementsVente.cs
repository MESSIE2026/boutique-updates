using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using DFont = System.Drawing.Font;
namespace BoutiqueRebuildFixed
{
    public partial class FormPaiementsVente : Form
    {
        // Résultat en mode split
        public List<PaiementLine> Result { get; private set; } = new List<PaiementLine>();

        private decimal _total = 0m;
        private decimal _totalVente = 0m;
        private string _devise = "CDF";
        private int _idVente = 0;
        private bool _allowPartial = false;   // ✅ true si vente crédit
        private readonly string _cs = ConfigSysteme.ConnectionString;


        private bool _modeHistorique = false;     // true = lecture DB, false = saisie split
        private string _userAnnulation = "SYSTEM"; // pas besoin de ConfigSysteme.NomUtilisateur
        private List<OrdonnanceLigneDTO> _lignesOrdonnance = new List<OrdonnanceLigneDTO>();
        private OrdonnanceVenteDTO _ordonnanceCourante = null;
        private Button btnOrdonnance;
        private Button btnOuvrirPdfOrdonnance;
        private OrdonnanceVenteDTO _prefillOrdonnance = null;

        // ✅ Exposer l'ordonnance au formulaire parent (FormVente)
        public OrdonnanceVenteDTO OrdonnanceResult => _ordonnanceCourante;
        public void ChargerContexteOrdonnance(List<OrdonnanceLigneDTO> lignes, string numeroOrd, string prescripteur, string patient)
        {
            _lignesOrdonnance = lignes ?? new List<OrdonnanceLigneDTO>();

            _ordonnanceCourante = new OrdonnanceVenteDTO
            {
                Numero = numeroOrd ?? "",
                Prescripteur = prescripteur ?? "",
                Patient = patient ?? "",
                DateOrdonnance = DateTime.Today
            };
        }

        public class PaiementLine
        {
            public string ModePaiement { get; set; }   // ✅ aligné DB (ModePaiement)
            public string Devise { get; set; }         // devise vente (après conversion)
            public decimal Montant { get; set; }       // montant en devise vente
            public string Reference { get; set; }

            // vérité terrain (caisse)
            public string DeviseOriginale { get; set; }
            public decimal MontantOriginal { get; set; }
            public decimal TauxApplique { get; set; }
        }



        // ==========================================================
        // 1) MODE SAISIE (Split)
        // ==========================================================
        public FormPaiementsVente(decimal total, string devise, string userAnnulation)
    : this(total, devise, userAnnulation, false)
{

}

// ==========================================================
// 1bis) MODE SAISIE (Split) - CREDIT (paiement partiel autorisé)
// ==========================================================
public FormPaiementsVente(decimal total, string devise, string userAnnulation, bool allowPartial)
{
    InitializeComponent();

    _modeHistorique = false;
    _allowPartial = allowPartial;
    _total = total;
    _devise = string.IsNullOrWhiteSpace(devise) ? "CDF" : devise.Trim().ToUpperInvariant();
    _userAnnulation = string.IsNullOrWhiteSpace(userAnnulation) ? "SYSTEM" : userAnnulation.Trim();

    Text = allowPartial ? "Paiements (Crédit - acompte)" : "Paiements (Split)";
    StartPosition = FormStartPosition.CenterParent;

    ConfigurerScrollPanel();
    ConfigurerGridSaisie(); // ✅ colonnes d'abord !
           
            ConfigurerBoutonOrdonnance();   // ✅ ajoute le bouton

            dgvPaiements.CellEndEdit += dgvPaiements_CellEndEdit;
    dgvPaiements.RowsRemoved += (s, e) => MettreAJourTotauxUI();
    dgvPaiements.UserAddedRow += (s, e) => MettreAJourTotauxUI();

    string montantDefaut = _allowPartial
        ? "0,00"
        : _total.ToString("0.00", CultureInfo.GetCultureInfo("fr-FR"));

    dgvPaiements.Rows.Add(0, "CASH", _devise, montantDefaut, "");

    MettreAJourTotauxUI();

    // en split : pas d'annulation DB
    if (btnAnnulerPaiement != null) btnAnnulerPaiement.Visible = false;
    if (txtMotifAnnulation != null) txtMotifAnnulation.Visible = false;
}

// ==========================================================
// 2) MODE HISTORIQUE (DB)
// ==========================================================
public FormPaiementsVente(int idVente, string userAnnulation)
{
    InitializeComponent();

            KeyPreview = true;
            AcceptButton = btnOk;

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnOk.PerformClick();
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    e.SuppressKeyPress = true;
                    btnAnnuler.PerformClick(); // ou Close()
                }
            };

            _modeHistorique = true;
    _allowPartial = false;
    _idVente = idVente;
    _userAnnulation = string.IsNullOrWhiteSpace(userAnnulation) ? "SYSTEM" : userAnnulation.Trim();

    Text = "Paiements (Historique)";
    StartPosition = FormStartPosition.CenterParent;

    ConfigurerScrollPanel();
    ConfigurerGridHistorique();

    this.Load += (s, e) => ChargerPaiementsVenteDepuisDb();

    if (btnAnnulerPaiement != null) btnAnnulerPaiement.Visible = true;
    if (txtMotifAnnulation != null) txtMotifAnnulation.Visible = true;

    if (btnAnnulerPaiement != null)
        btnAnnulerPaiement.Click += btnAnnulerPaiement_Click;
}

        private void ConfigurerBoutonOrdonnance()
        {
            if (panelMain == null) return;

            // Bouton générer / saisir ordonnance (TOUJOURS dispo)
            btnOrdonnance = new Button
            {
                Text = "Ordonnance (PDF)",
                Width = 160,
                Height = 32,
                Left = 12,
                Top = 12,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Enabled = true
            };
            btnOrdonnance.Click += BtnOrdonnance_Click;

            // Bouton ouvrir PDF (désactivé tant qu’on n’a pas de PDF)
            btnOuvrirPdfOrdonnance = new Button
            {
                Text = "Ouvrir PDF",
                Width = 110,
                Height = 32,
                Left = btnOrdonnance.Left + btnOrdonnance.Width + 8, // ✅ position stable
                Top = 12,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Enabled = false
            };

            btnOuvrirPdfOrdonnance.Click += (s, e) =>
            {
                try
                {
                    if (_ordonnanceCourante != null &&
                        !string.IsNullOrWhiteSpace(_ordonnanceCourante.PdfPath) &&
                        File.Exists(_ordonnanceCourante.PdfPath))
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = _ordonnanceCourante.PdfPath,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                    else
                    {
                        MessageBox.Show("Aucun PDF d'ordonnance disponible.");
                    }
                }
                catch { }
            };

            panelMain.Controls.Add(btnOrdonnance);
            panelMain.Controls.Add(btnOuvrirPdfOrdonnance);
        }


        private decimal GetTotalVenteDepuisDb(int idVente)
        {
            // ⚠️ adapte le nom de colonne/clef selon ta table Vente
            const string sql = "SELECT ISNULL(MontantTotal,0) FROM Vente WHERE ID_Vente=@id";

            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;
                con.Open();

                object o = cmd.ExecuteScalar();
                if (o == null || o == DBNull.Value) return 0m;

                return Convert.ToDecimal(o);
            }
        }

        // ==========================================================
        // PANEL SCROLL : page entière
        // ==========================================================
        private void ConfigurerScrollPanel()
        {
            if (panelMain == null) return;

            panelMain.AutoScroll = true;
            panelMain.HorizontalScroll.Enabled = true;
            panelMain.HorizontalScroll.Visible = true;
            panelMain.VerticalScroll.Enabled = true;
            panelMain.VerticalScroll.Visible = true;

            // Minimum de surface scrollable (adapte si tu veux)
            panelMain.AutoScrollMinSize = new Size(900, 650);

            // Évite double-scroll sur le form
            this.AutoScroll = false;
        }

        // ==========================================================
        // GRID : SAISIE (Split)
        // ==========================================================
        private void ConfigurerGridSaisie()
        {
            dgvPaiements.AutoGenerateColumns = false;
            dgvPaiements.Columns.Clear();

            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "IdPaiement", HeaderText = "Id", Visible = false });
            var colMode = new DataGridViewComboBoxColumn
            {
                Name = "Mode",
                HeaderText = "Mode",
                DataSource = new[] { "CASH", "MOMO", "CARTE", "VIREMENT", "FIDELITE", "CREDIT" }
            };

            var colDev = new DataGridViewComboBoxColumn
            {
                Name = "Devise",
                HeaderText = "Devise",
                DataSource = new[] { "CDF", "USD", "EUR" }
            };

            dgvPaiements.Columns.Add(colMode);
            dgvPaiements.Columns.Add(colDev);
            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "Montant", HeaderText = "Montant" });
            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reference", HeaderText = "Référence" });

            dgvPaiements.AllowUserToAddRows = true;
            dgvPaiements.AllowUserToDeleteRows = true;
            dgvPaiements.ReadOnly = false;

            // Colonnes remplissent le grid
            dgvPaiements.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPaiements.ScrollBars = ScrollBars.Vertical;

            // Le grid est dans panelMain => si tu veux, tu peux le rendre large
            dgvPaiements.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            if (panelMain != null) dgvPaiements.Width = panelMain.ClientSize.Width - 20;

            dgvPaiements.RowHeadersVisible = false;
            dgvPaiements.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPaiements.MultiSelect = false;

            btnOk.Visible = true;
            btnOk.Text = "OK";
            btnAnnuler.Visible = true;
            btnAnnuler.Text = "Annuler";
        }

        // ==========================================================
        // GRID : HISTORIQUE (DB)
        // ==========================================================
        private void ConfigurerGridHistorique()
        {
            dgvPaiements.AutoGenerateColumns = false;
            dgvPaiements.Columns.Clear();

            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "IdPaiement", HeaderText = "IdPaiement", Visible = false });
            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mode", HeaderText = "Mode" });
            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "Devise", HeaderText = "Devise" });
            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "Montant", HeaderText = "Montant" });
            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reference", HeaderText = "Référence" });

            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "Statut", HeaderText = "Statut" });
            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "AnnulePar", HeaderText = "Annulé par" });
            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "DateAnnulation", HeaderText = "Date annulation" });
            dgvPaiements.Columns.Add(new DataGridViewTextBoxColumn { Name = "MotifAnnulation", HeaderText = "Motif" });

            dgvPaiements.AllowUserToAddRows = false;
            dgvPaiements.AllowUserToDeleteRows = false;
            dgvPaiements.ReadOnly = true;

            // Colonnes remplissent le grid
            dgvPaiements.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPaiements.ScrollBars = ScrollBars.Vertical;

            dgvPaiements.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            if (panelMain != null) dgvPaiements.Width = panelMain.ClientSize.Width - 20;

            dgvPaiements.RowHeadersVisible = false;
            dgvPaiements.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPaiements.MultiSelect = false;

            btnOk.Visible = true;
            btnOk.Text = "Fermer";
            btnAnnuler.Visible = false;
        }

        // ==========================================================
        // DB : charger paiements (historique)
        // ==========================================================
        private void ChargerPaiementsVenteDepuisDb()
        {
            if (_idVente <= 0)
            {
                MessageBox.Show("IdVente invalide.");
                return;
            }

            dgvPaiements.Rows.Clear();

            DataTable dt = PaiementsVenteService.GetPaiements(_idVente, _cs);

            decimal totalPaye = 0m;

            foreach (DataRow r in dt.Rows)
            {
                int idPaiement = Convert.ToInt32(r["IdPaiement"]);
                string mode = (r["ModePaiement"] ?? "").ToString().Trim();
                string dev = (r["Devise"] ?? "").ToString().Trim();

                decimal montant = 0m;
                if (r["Montant"] != DBNull.Value)
                    montant = Convert.ToDecimal(r["Montant"]);

                string reference = (r["ReferenceTransaction"] ?? "").ToString();
                string statut = (r["Statut"] ?? "VALIDE").ToString().Trim().ToUpperInvariant();
                string annulePar = (r["AnnulePar"] ?? "").ToString();
                string dateAnnul = r["DateAnnulation"] == DBNull.Value ? "" :
                    Convert.ToDateTime(r["DateAnnulation"]).ToString("dd/MM/yyyy HH:mm");
                string motif = (r["MotifAnnulation"] ?? "").ToString();

                int rowIndex = dgvPaiements.Rows.Add(
                    idPaiement, mode, dev, montant.ToString("N2"),
                    reference, statut, annulePar, dateAnnul, motif
                );

                bool estAnnule = new[] { "ANNULE", "ANNULEE", "ANNULÉ", "ANNULÉE" }
    .Contains((statut ?? "").Trim().ToUpperInvariant());

                if (estAnnule)
                {
                    dgvPaiements.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Gray;
                    dgvPaiements.Rows[rowIndex].DefaultCellStyle.Font = new DFont(dgvPaiements.Font, FontStyle.Strikeout);
                }
                else
                {
                    totalPaye += montant;
                }
            }

            if (lblTotalPaye != null) lblTotalPaye.Text = totalPaye.ToString("N2");

            // ✅ Total vente depuis DB + reste réel
            decimal totalVente = GetTotalVenteDepuisDb(_idVente);
            decimal reste = totalVente - totalPaye;
            if (reste < 0) reste = 0m;

            if (lblResteAPayer != null) lblResteAPayer.Text = reste.ToString("N2");
        }
        // ==========================================================
        // Parse robuste FR (corrige le bug “paiement valide ignoré”)
        // ==========================================================
        private decimal LireDecimalFR(string s)
        {
            s = (s ?? "").Trim();

            // supprime espaces + NBSP (souvent dans 1 200,00)
            s = s.Replace("\u00A0", "").Replace(" ", "");

            decimal v;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out v))
                return v;

            // secours si l'utilisateur tape 12.5 au lieu de 12,5
            s = s.Replace(".", ",");
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out v))
                return v;

            return 0m;
        }

        // ==========================================================
        // UI Totaux (mode split)
        // ==========================================================
        private void MettreAJourTotauxUI()
        {
            if (_modeHistorique) return;

            decimal sum = 0m;

            foreach (DataGridViewRow r in dgvPaiements.Rows)
            {
                if (r.IsNewRow) continue;

                string devOrig = Convert.ToString(r.Cells["Devise"]?.Value ?? _devise).Trim().ToUpperInvariant();
                string sMontant = Convert.ToString(r.Cells["Montant"]?.Value ?? "0");
                decimal mOrig = LireDecimalFR(sMontant);

                if (mOrig <= 0m) continue;

                decimal mVente = mOrig;

                if (devOrig == "FC") devOrig = "CDF";
                if (_devise == "FC") _devise = "CDF";

                if (!string.Equals(devOrig, _devise, StringComparison.OrdinalIgnoreCase))
                {
                    decimal taux = TauxChangeService.GetTaux(devOrig, _devise, _cs);
                    if (taux > 0m) mVente = Math.Round(mOrig * taux, 2);
                }

                sum += mVente;
            }

            if (lblTotalPaye != null) lblTotalPaye.Text = sum.ToString("N2");

            decimal reste = _total - sum;
            if (reste < 0) reste = 0m;
            if (lblResteAPayer != null) lblResteAPayer.Text = reste.ToString("N2");
        }


        private void dgvPaiements_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            MettreAJourTotauxUI();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (_modeHistorique)
            {
                Close();
                return;
            }

            Result.Clear();
            decimal sum = 0m;

            foreach (DataGridViewRow r in dgvPaiements.Rows)
            {
                if (r.IsNewRow) continue;

                string modePaiement = Convert.ToString(r.Cells["Mode"]?.Value ?? "").Trim();
                string devOrig = Convert.ToString(r.Cells["Devise"]?.Value ?? _devise).Trim().ToUpperInvariant();

                string sMontant = Convert.ToString(r.Cells["Montant"]?.Value ?? "0");
                decimal mOrig = LireDecimalFR(sMontant);

                if (string.IsNullOrWhiteSpace(modePaiement) || mOrig <= 0m) continue;

                // ✅ conversion vers devise de la vente (_devise)
                decimal taux;
                decimal mVente = mOrig;

                if (!string.Equals(devOrig, _devise, StringComparison.OrdinalIgnoreCase))
                {
                    // Ici on convertit via SP (recommandé)
                    using (var con = new SqlConnection(_cs))
                    {
                        con.Open();
                        // pas de tx ici car tu es juste en saisie; la vente finale aura sa tx
                        mVente = TauxChangeService.Convertir(con, null, mOrig, devOrig, _devise, out taux);
                    }

                    if (taux <= 0m)
                    {
                        MessageBox.Show($"Taux introuvable pour {devOrig} -> {_devise}. Enregistre d'abord le taux.");
                        return;
                    }
                }
                else
                {
                    taux = 1m;
                }

                sum += mVente;

                Result.Add(new PaiementLine
                {
                    ModePaiement = modePaiement,
                    Devise = _devise,              // ✅ devise vente
                    Montant = mVente,              // ✅ montant en devise vente
                    Reference = Convert.ToString(r.Cells["Reference"]?.Value ?? "").Trim(),

                    DeviseOriginale = devOrig,      // ✅ devise saisie
                    MontantOriginal = mOrig,        // ✅ montant saisi
                    TauxApplique = taux
                });
            }

            if (Result.Count == 0)
            {
                MessageBox.Show("Ajoute au moins un paiement valide.");
                return;
            }

            if (!_allowPartial)
            {
                if (Math.Round(sum, 2) != Math.Round(_total, 2))
                {
                    MessageBox.Show("Total paiements (" + sum.ToString("N2") + ") ≠ Total vente (" + _total.ToString("N2") + ")");
                    return;
                }
            }
            else
            {
                // ✅ crédit : acompte autorisé (<= total), mais pas 0
                if (sum <= 0m || Math.Round(sum, 2) > Math.Round(_total, 2))
                {
                    MessageBox.Show("En mode crédit, l'acompte doit être > 0 et ≤ " + _total.ToString("N2"));
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        

        private void btnAnnulerPaiement_Click(object sender, EventArgs e)
        {
            if (!_modeHistorique) return;

            if (dgvPaiements.CurrentRow == null)
            {
                MessageBox.Show("Sélectionne un paiement.");
                return;
            }

            int idPaiement = 0;
            int.TryParse(Convert.ToString(dgvPaiements.CurrentRow.Cells["IdPaiement"]?.Value), out idPaiement);

            if (idPaiement <= 0)
            {
                MessageBox.Show("IdPaiement invalide.");
                return;
            }

            string statut = Convert.ToString(dgvPaiements.CurrentRow.Cells["Statut"]?.Value ?? "")
                .Trim().ToUpperInvariant();

            if (statut == "ANNULE")
            {
                MessageBox.Show("Ce paiement est déjà annulé.");
                return;
            }

            string motif = txtMotifAnnulation == null ? "" : txtMotifAnnulation.Text.Trim();
            if (string.IsNullOrWhiteSpace(motif))
            {
                MessageBox.Show("Motif obligatoire (champ Motif).");
                return;
            }

            if (MessageBox.Show("Annuler ce paiement ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            bool ok = PaiementsVenteService.AnnulerPaiement(idPaiement, _cs, _userAnnulation, motif);

            if (!ok)
            {
                MessageBox.Show("Paiement déjà annulé ou introuvable.");
                return;
            }

            ChargerPaiementsVenteDepuisDb();
            if (txtMotifAnnulation != null) txtMotifAnnulation.Clear();
            MessageBox.Show("✅ Paiement annulé.");
        }

        private void BtnOrdonnance_Click(object sender, EventArgs e)
        {
            try
            {
                var lignes = _lignesOrdonnance ?? new List<OrdonnanceLigneDTO>();

                // ✅ si l'utilisateur a déjà créé une ordonnance, on pré-remplit avec celle-là
                var prefill = _ordonnanceCourante ?? _prefillOrdonnance;
                using (var f = new FrmOrdonnanceVente(lignes, prefill))
                {
                    var dr = f.ShowDialog(this);
                    if (dr != DialogResult.OK) return;

                    _ordonnanceCourante = f.Result;

                    if (_ordonnanceCourante != null &&
                        !string.IsNullOrWhiteSpace(_ordonnanceCourante.PdfPath) &&
                        File.Exists(_ordonnanceCourante.PdfPath))
                    {
                        btnOuvrirPdfOrdonnance.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur Ordonnance : " + ex.Message);
            }
        }



        private void panelMain_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnExporterPDF_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) Choisir fichier PDF
                string filePath = DemanderCheminPdf_PaiementsAnnules();
                if (string.IsNullOrWhiteSpace(filePath)) return;

                // 2) Charger données du jour (annulés)
                DataTable dt = ChargerPaiementsAnnulesDuJour(_cs);

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Aucun paiement annulé aujourd'hui.");
                    return;
                }

                // 3) Générer PDF
                GenererPdfPaiementsAnnulesDuJour(filePath, dt);

                // 4) Ouvrir
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch { }

                MessageBox.Show("✅ PDF exporté : paiements annulés du jour.");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Accès refusé. Choisis un autre dossier (Bureau, Documents, D:\\, USB...).");
            }
            catch (IOException ioEx)
            {
                MessageBox.Show("Erreur fichier (PDF ouvert ou bloqué) : " + ioEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur export PDF : " + ex.Message);
            }
        }

        // ==========================================================
        // SaveFileDialog : choisir emplacement
        // ==========================================================
        private string DemanderCheminPdf_PaiementsAnnules()
        {
            string nom = "Paiements_Annules_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf";

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Exporter les paiements annulés du jour";
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = nom;
                sfd.AddExtension = true;
                sfd.DefaultExt = "pdf";
                sfd.OverwritePrompt = true;
                sfd.RestoreDirectory = true;
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                return sfd.ShowDialog() == DialogResult.OK ? sfd.FileName : null;
            }
        }

        // ==========================================================
        // DB : Paiements annulés du jour
        // - On filtre sur DateAnnulation (recommandé)
        // - Si DateAnnulation est NULL, on peut filtrer sur DatePaiement
        // ==========================================================
        private DataTable ChargerPaiementsAnnulesDuJour(string cs)
        {
            DateTime d1 = DateTime.Today;
            DateTime d2 = d1.AddDays(1);

            string sql = @"
SELECT
    p.IdPaiement,
    p.IdVente,
    ISNULL(p.ModePaiement,'') AS ModePaiement,
    ISNULL(p.Devise,'') AS Devise,
    ISNULL(p.Montant,0) AS Montant,
    p.DatePaiement,
    ISNULL(p.ReferenceTransaction,'') AS ReferenceTransaction,
    ISNULL(p.Statut,'') AS Statut,
    ISNULL(p.AnnulePar,'') AS AnnulePar,
    p.DateAnnulation,
    ISNULL(p.MotifAnnulation,'') AS MotifAnnulation
FROM dbo.PaiementsVente p
WHERE
    p.IdEntreprise = @ent
    AND p.IdMagasin = @mag
    AND (@poste IS NULL OR p.IdPoste = @poste)
    AND UPPER(ISNULL(p.Statut,'')) IN ('ANNULE','ANNULEE','ANNULÉ','ANNULÉE')
    AND (
        (p.DateAnnulation >= @d1 AND p.DateAnnulation < @d2)
        OR
        (p.DateAnnulation IS NULL AND p.DatePaiement >= @d1 AND p.DatePaiement < @d2)
    )
ORDER BY ISNULL(p.DateAnnulation, p.DatePaiement) ASC, p.IdPaiement ASC;";

            var dt = new DataTable();

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = d1;
                cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = d2;
                cmd.Parameters.Add("@ent", SqlDbType.Int).Value = ConfigSysteme.IdEntreprise;
                cmd.Parameters.Add("@mag", SqlDbType.Int).Value = ConfigSysteme.IdMagasin;
                cmd.Parameters.Add("@poste", SqlDbType.Int).Value = ConfigSysteme.IdPoste;

                con.Open();
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            return dt;
        }

        // ==========================================================
        // PDF : export pro A4
        // ==========================================================
        private void GenererPdfPaiementsAnnulesDuJour(string filePath, DataTable dt)
        {
            var fontTitre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14f);
            var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10f);
            var fontSmall = FontFactory.GetFont(FontFactory.HELVETICA, 9f);
            var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 9f);

            BaseColor grayHeader = new BaseColor(240, 240, 240);
            BaseColor grayBorder = new BaseColor(200, 200, 200);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var doc = new Document(PageSize.A4, 36f, 36f, 28f, 30f))
            {
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                // ===== EN-TÊTE =====
                PdfPTable top = new PdfPTable(1) { WidthPercentage = 100 };

                // ✅ ICI LA CORRECTION : PdfPCell.NO_BORDER (PAS DRectangle.NO_BORDER)
                PdfPCell head = new PdfPCell { Border = PdfPCell.NO_BORDER };

                head.AddElement(new Paragraph("ZAIRE MODE SARL", fontTitre));
                head.AddElement(new Paragraph("RAPPORT - PAIEMENTS ANNULES (JOURNEE)", fontHeader));
                head.AddElement(new Paragraph("Date : " + DateTime.Now.ToString("dd/MM/yyyy"), fontSmall));
                head.AddElement(new Paragraph("Généré le : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), fontSmall));

                top.AddCell(head);
                doc.Add(top);

                doc.Add(new Paragraph(" "));

                // ✅ ICI LA CORRECTION : iTextSharp.text.pdf.draw.LineSeparator
                doc.Add(new Chunk(new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0)));

                doc.Add(new Paragraph(" "));

                // ===== TABLE =====
                PdfPTable table = new PdfPTable(9) { WidthPercentage = 100f };
                table.SetWidths(new float[] { 10f, 10f, 14f, 8f, 10f, 14f, 12f, 10f, 22f });

                PdfPCell Cell(string text, iTextSharp.text.Font f, int align, BaseColor bg, bool headerCell = false)
                {
                    return new PdfPCell(new Phrase(text ?? "", f))
                    {
                        HorizontalAlignment = align,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        BackgroundColor = bg,
                        PaddingTop = 5f,
                        PaddingBottom = 5f,
                        PaddingLeft = 4f,
                        PaddingRight = 4f,
                        BorderColor = grayBorder,
                        BorderWidth = headerCell ? 1.1f : 0.6f,
                        NoWrap = false
                    };
                }

                // Header
                table.AddCell(Cell("IdPay", fontHeader, Element.ALIGN_CENTER, grayHeader, true));
                table.AddCell(Cell("IdVente", fontHeader, Element.ALIGN_CENTER, grayHeader, true));
                table.AddCell(Cell("Mode", fontHeader, Element.ALIGN_LEFT, grayHeader, true));
                table.AddCell(Cell("Dev", fontHeader, Element.ALIGN_CENTER, grayHeader, true));
                table.AddCell(Cell("Montant", fontHeader, Element.ALIGN_RIGHT, grayHeader, true));
                table.AddCell(Cell("Référence", fontHeader, Element.ALIGN_LEFT, grayHeader, true));
                table.AddCell(Cell("Annulé par", fontHeader, Element.ALIGN_LEFT, grayHeader, true));
                table.AddCell(Cell("Date", fontHeader, Element.ALIGN_LEFT, grayHeader, true));
                table.AddCell(Cell("Motif", fontHeader, Element.ALIGN_LEFT, grayHeader, true));

                // Totaux par devise
                Dictionary<string, decimal> totDevise = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                foreach (DataRow r in dt.Rows)
                {
                    int idPay = Convert.ToInt32(r["IdPaiement"]);
                    int idVente = Convert.ToInt32(r["IdVente"]);
                    string mode = Convert.ToString(r["ModePaiement"]);
                    string dev = Convert.ToString(r["Devise"]);
                    decimal montant = Convert.ToDecimal(r["Montant"]);
                    string reference = Convert.ToString(r["ReferenceTransaction"]);
                    string annulePar = Convert.ToString(r["AnnulePar"]);

                    DateTime? dateAnnul = r["DateAnnulation"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DateAnnulation"]);
                    DateTime? datePay = r["DatePaiement"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DatePaiement"]);
                    string motif = Convert.ToString(r["MotifAnnulation"]);

                    DateTime? dAff = dateAnnul ?? datePay;
                    string dTxt = dAff.HasValue ? dAff.Value.ToString("dd/MM/yyyy HH:mm") : "";

                    table.AddCell(Cell(idPay.ToString(), fontCell, Element.ALIGN_CENTER, null));
                    table.AddCell(Cell(idVente.ToString(), fontCell, Element.ALIGN_CENTER, null));
                    table.AddCell(Cell(mode, fontCell, Element.ALIGN_LEFT, null));
                    table.AddCell(Cell(dev, fontCell, Element.ALIGN_CENTER, null));
                    table.AddCell(Cell(montant.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")), fontCell, Element.ALIGN_RIGHT, null));
                    table.AddCell(Cell(reference, fontCell, Element.ALIGN_LEFT, null));
                    table.AddCell(Cell(annulePar, fontCell, Element.ALIGN_LEFT, null));
                    table.AddCell(Cell(dTxt, fontCell, Element.ALIGN_LEFT, null));
                    table.AddCell(Cell(motif, fontCell, Element.ALIGN_LEFT, null));

                    if (string.IsNullOrWhiteSpace(dev)) dev = "N/A";
                    if (!totDevise.ContainsKey(dev)) totDevise[dev] = 0m;
                    totDevise[dev] += montant;
                }

                doc.Add(table);

                doc.Add(new Paragraph(" "));

                // ===== TOTAUX =====
                PdfPTable totals = new PdfPTable(2)
                {
                    WidthPercentage = 45f,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 8f
                };
                totals.SetWidths(new float[] { 55f, 45f });

                foreach (var kv in totDevise.OrderBy(x => x.Key))
                {
                    totals.AddCell(new PdfPCell(new Phrase("TOTAL ANNULE (" + kv.Key + ")", fontHeader))
                    {
                        BorderColor = grayBorder,
                        Padding = 6f,
                        BackgroundColor = new BaseColor(250, 250, 250)
                    });

                    totals.AddCell(new PdfPCell(new Phrase(kv.Value.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")) + " " + kv.Key, fontHeader))
                    {
                        BorderColor = grayBorder,
                        Padding = 6f,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    });
                }

                doc.Add(totals);

                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph("Note : Rapport des paiements annulés sur la journée.", fontSmall));
                doc.Add(new Paragraph("Généré par : " + (string.IsNullOrWhiteSpace(_userAnnulation) ? "SYSTEM" : _userAnnulation), fontSmall));

                doc.Close();
            }
        }
    }
}