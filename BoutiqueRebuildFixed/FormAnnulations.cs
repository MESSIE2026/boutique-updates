using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iText.Commons.Actions;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace BoutiqueRebuildFixed
{
    public partial class FormAnnulations : FormBase
    {
        private readonly string _cs = ConfigSysteme.ConnectionString;
        private readonly AnnulationsService _svc;
        private readonly AnnulationsPdfService _pdf;

        private int _dernierIdAnnulation = -1;

        // (optionnel) infos venant de FormVentes
        private readonly int? _idVente;
        private readonly int? _idDetails;
        public FormAnnulations(int? idVente = null, int? idDetails = null)
        {
            InitializeComponent();

            FixLayoutAnnulations();

            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.Manual; // car on l'héberge dans un dialog
            this.FormBorderStyle = FormBorderStyle.None;

            this.Load += (s, e) =>
            {
                if (_idDetails.HasValue)
                    ChargerDepuisDetailsVente(_idDetails.Value);
            };

            _svc = new AnnulationsService(_cs);
            _pdf = new AnnulationsPdfService();

            _idVente = idVente;
            _idDetails = idDetails;

            ConfigurerDataGridView();
            InitialiserComboBox();
            InitialiserDevise();
            ChargerGrid();

            dgvAnnulationsRetours.CellClick -= dgvAnnulationsRetours_CellClick;
            dgvAnnulationsRetours.CellClick += dgvAnnulationsRetours_CellClick;

            txtPrixUnitaire.TextChanged += (s, e) => CalculerPrixTotal();
            nudQuantite.ValueChanged += (s, e) => CalculerPrixTotal();
        }
        private void ConfigurerDataGridView()
        {
            dgvAnnulationsRetours.ReadOnly = true;
            dgvAnnulationsRetours.AllowUserToAddRows = false;
            dgvAnnulationsRetours.AllowUserToDeleteRows = false;
            dgvAnnulationsRetours.MultiSelect = false;
            dgvAnnulationsRetours.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAnnulationsRetours.RowHeadersVisible = false;

            // ✅ Scrollbars
            dgvAnnulationsRetours.ScrollBars = ScrollBars.Both;

            // ✅ Colonnes "normales" (pas Fill)
            dgvAnnulationsRetours.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvAnnulationsRetours.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            // ✅ Hauteur lignes / entêtes
            dgvAnnulationsRetours.RowTemplate.Height = 28;
            dgvAnnulationsRetours.ColumnHeadersHeight = 32;

            // ✅ Redimensionnement utilisateur
            dgvAnnulationsRetours.AllowUserToResizeColumns = true;
            dgvAnnulationsRetours.AllowUserToResizeRows = false;

            // ✅ Texte sur une ligne (évite lignes géantes)
            dgvAnnulationsRetours.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            // Optionnel : meilleure lecture
            dgvAnnulationsRetours.AutoGenerateColumns = true;
        }

        private void FixLayoutAnnulations()
        {
            // 1) Créer un panel bas pour les boutons (si pas déjà)
            Panel pnlBottom = this.Controls.Find("pnlBottom", true).FirstOrDefault() as Panel;

            if (pnlBottom == null)
            {
                pnlBottom = new Panel
                {
                    Name = "pnlBottom",
                    Dock = DockStyle.Bottom,
                    Height = 70,
                    Padding = new Padding(10),
                    BackColor = this.BackColor
                };
                this.Controls.Add(pnlBottom);
                pnlBottom.BringToFront();
            }

            // 2) Mettre le DataGridView en FILL (il prendra la place restante)
            dgvAnnulationsRetours.Dock = DockStyle.Fill;
            dgvAnnulationsRetours.BringToFront();  // ok, mais le panel bas sera au-dessus

            // 3) Déplacer les boutons dans pnlBottom (adapte les noms si différents)
            MoveToBottomPanel(pnlBottom, btnValider);
            MoveToBottomPanel(pnlBottom, btnExporterPDF);
            MoveToBottomPanel(pnlBottom, btnAnnuler);

            // Optionnel: aligner à droite automatiquement
            int x = pnlBottom.Width - 10;
            foreach (var b in pnlBottom.Controls.OfType<Button>().Reverse())
            {
                b.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                b.Top = 15;
                x -= b.Width;
                b.Left = x;
                x -= 10;
            }

            pnlBottom.Resize += (s, e) =>
            {
                int xx = pnlBottom.Width - 10;
                foreach (var b in pnlBottom.Controls.OfType<Button>().Reverse())
                {
                    b.Top = 15;
                    xx -= b.Width;
                    b.Left = xx;
                    xx -= 10;
                }
            };
        }

        private void MoveToBottomPanel(Panel pnl, Control c)
        {
            if (c == null) return;
            if (c.Parent != pnl)
            {
                c.Parent?.Controls.Remove(c);
                pnl.Controls.Add(c);
            }
        }


        private void ChargerDepuisDetailsVente(int idDetails)
        {
            using (var con = new SqlConnection(_cs))
            {
                con.Open();

                string sql = @"
SELECT TOP 1
    d.ID_Details, d.ID_Vente, d.NomProduit,
    d.Quantite AS QteVendue, d.PrixUnitaire, d.Devise,
    v.CodeFacture, v.DateVente,
    c.Nom AS NomClient
FROM dbo.DetailsVente d
INNER JOIN dbo.Vente v ON v.ID_Vente = d.ID_Vente
LEFT JOIN dbo.Clients c ON c.ID_Clients = v.ID_Client
WHERE d.ID_Details = @id;";

                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@id", idDetails);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                        {
                            MessageBox.Show("Détail vente introuvable.");
                            return;
                        }

                        txtNomClient.Text = rd["NomClient"]?.ToString() ?? "";
                        txtNumCommande.Text = rd["CodeFacture"]?.ToString() ?? "";
                        dtpDateAchat.Value = Convert.ToDateTime(rd["DateVente"]);

                        txtNomProduit.Text = rd["NomProduit"]?.ToString() ?? "";
                        txtPrixUnitaire.Text = rd["PrixUnitaire"]?.ToString() ?? "";
                        cbDevise.Text = rd["Devise"]?.ToString() ?? "CDF";

                        decimal qteVendue = Convert.ToDecimal(rd["QteVendue"]);
                        nudQuantite.Maximum = Math.Max(1, qteVendue);
                        nudQuantite.Value = 1;

                        CalculerPrixTotal();
                    }
                }
            }
        }


        private void InitialiserDevise()
        {
            cbDevise.Items.Clear();
            cbDevise.Items.AddRange(new string[] { "USD", "CDF", "CFA", "EURO" });
            cbDevise.SelectedIndex = 0;
        }

        private void InitialiserComboBox()
        {
            cmbMotifRetour.Items.Clear();
            cmbMotifRetour.Items.AddRange(new string[]
            {
                "Produit défectueux", "Erreur de commande", "Non conforme", "Produit endommagé", "Autre"
            });
            cmbMotifRetour.SelectedIndex = 0;
        }

        private void ChargerGrid()
        {
            dgvAnnulationsRetours.DataSource = _svc.Repo.GetAll();

            // ✅ Largeur par défaut
            foreach (DataGridViewColumn col in dgvAnnulationsRetours.Columns)
            {
                col.MinimumWidth = 80;
                col.Width = 140;
            }

            // ✅ Colonnes importantes plus larges
            if (dgvAnnulationsRetours.Columns.Contains("NomClient"))
                dgvAnnulationsRetours.Columns["NomClient"].Width = 180;

            if (dgvAnnulationsRetours.Columns.Contains("NomProduit"))
                dgvAnnulationsRetours.Columns["NomProduit"].Width = 260;

            if (dgvAnnulationsRetours.Columns.Contains("Commentaires"))
                dgvAnnulationsRetours.Columns["Commentaires"].Width = 260;

            dgvAnnulationsRetours.ClearSelection();
        }

        private void CalculerPrixTotal()
        {
            if (decimal.TryParse(txtPrixUnitaire.Text.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out decimal pu))
            {
                txtPrixTotal.Text = (pu * nudQuantite.Value).ToString("F2");
            }
            else txtPrixTotal.Text = "0.00";
        }

        private bool VerifierChamps()
        {
            if (string.IsNullOrWhiteSpace(txtNomClient.Text) ||
                string.IsNullOrWhiteSpace(txtNumCommande.Text) ||
                string.IsNullOrWhiteSpace(txtNomProduit.Text) ||
                string.IsNullOrWhiteSpace(txtPrixUnitaire.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.");
                return false;
            }
            if (!decimal.TryParse(txtPrixUnitaire.Text.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show("Prix unitaire invalide.");
                return false;
            }
            return true;
        }
        private void FormAnnulations_Load(object sender, EventArgs e)
        {
           
        }

        
        private void btnValider_Click(object sender, EventArgs e)
        {
            if (!VerifierChamps()) return;

            try
            {
                var a = new AnnulationRetour
                {
                    IdVente = _idVente,
                    IdDetailsVente = _idDetails,

                    NomClient = txtNomClient.Text.Trim(),
                    NumeroCommande = txtNumCommande.Text.Trim(),
                    DateAchat = dtpDateAchat.Value.Date,
                    NomProduit = txtNomProduit.Text.Trim(),

                    PrixUnitaire = decimal.Parse(txtPrixUnitaire.Text.Replace(',', '.'), CultureInfo.InvariantCulture),
                    Devise = cbDevise.Text,

                    QuantiteAchetee = nudQuantite.Maximum,       // info
                    QuantiteRetournee = nudQuantite.Value,       // ✅ IMPORTANT

                    MotifRetour = cmbMotifRetour.Text,
                    Commentaires = txtCommentaires.Text.Trim(),
                    TypeRetour = rbRemboursement.Checked ? "Remboursement" : "Echange",
                    Utilisateur = SessionEmploye.Nom ?? "SYSTEM"
                };

                bool remettreStock = true; // retour produit -> stock
                bool faireRemboursement = rbRemboursement.Checked;

                _dernierIdAnnulation = _svc.EnregistrerEtAppliquer(a, remettreStock, faireRemboursement);

                MessageBox.Show("Annulation / retour enregistré. Export PDF disponible.");
                ChargerGrid();
                ReinitialiserFormulaire();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void ReinitialiserFormulaire()
        {
            txtNomClient.Clear();
            txtNumCommande.Clear();
            txtNomProduit.Clear();
            txtPrixUnitaire.Clear();
            txtCommentaires.Clear();
            nudQuantite.Value = 1;
            dtpDateAchat.Value = DateTime.Now;
            cmbMotifRetour.SelectedIndex = 0;
            rbRemboursement.Checked = true;
            txtPrixTotal.Text = "0.00";
        }
        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            ReinitialiserFormulaire();
        }

        private void btnExporterPDF_Click(object sender, EventArgs e)
        {
            if (_dernierIdAnnulation <= 0)
            {
                MessageBox.Show("Aucune annulation récente à exporter.");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "Annulation_Retour_Client.pdf";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                var row = _svc.Repo.GetById(_dernierIdAnnulation);
                if (row == null)
                {
                    MessageBox.Show("Données introuvables.");
                    return;
                }

                // Chemin logo PRO (pas D:\)
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "LOGO1.png");

                _pdf.Exporter(row, sfd.FileName, logoPath);
                MessageBox.Show("PDF exporté avec succès.");
            }
        }

        private void dgvAnnulationsRetours_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var r = dgvAnnulationsRetours.Rows[e.RowIndex];

            txtNomClient.Text = r.Cells["NomClient"]?.Value?.ToString() ?? "";
            txtNumCommande.Text = r.Cells["NumeroCommande"]?.Value?.ToString() ?? "";
            dtpDateAchat.Value = Convert.ToDateTime(r.Cells["DateAchat"]?.Value ?? DateTime.Now);

            txtNomProduit.Text = r.Cells["NomProduit"]?.Value?.ToString() ?? "";
            txtPrixUnitaire.Text = r.Cells["PrixUnitaire"]?.Value?.ToString() ?? "";

            cbDevise.Text = r.Cells["Devise"]?.Value?.ToString() ?? "CDF";
            cmbMotifRetour.Text = r.Cells["MotifRetour"]?.Value?.ToString() ?? "";

            string type = r.Cells["TypeRetour"]?.Value?.ToString() ?? "";
            rbRemboursement.Checked = type.Equals("Remboursement", StringComparison.OrdinalIgnoreCase);
            rbEchange.Checked = !rbRemboursement.Checked;

            // ✅ quantité retournée
            decimal qRet = 1;
            decimal.TryParse(r.Cells["QuantiteRetournee"]?.Value?.ToString(), out qRet);

            nudQuantite.Value = Math.Max(nudQuantite.Minimum, Math.Min(nudQuantite.Maximum, qRet));

            CalculerPrixTotal();
        }


        private void btnFermer_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Voulez-vous quitter ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
