using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmCreditsClients : Form
    {

        private readonly CreditService _svc;
        private TextBox txtSearch;
        private Button btnRefresh;
        private Button btnEncaisser;
        private SplitContainer split;
        private DataGridView dgvCredits;
        private DataGridView dgvPays;


        public int SelectedCreditId { get; private set; }
        public decimal SelectedReste { get; private set; }
        public string SelectedClientNom { get; private set; } = "";
        public FrmCreditsClients()
        {
            InitializeComponent();

            _svc = new CreditService(ConfigSysteme.ConnectionString);

            Text = "Clients débiteurs (Crédits)";
            Width = 1100;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10f);

            BuildUI();
            LoadData();
        }

        private void FrmCreditsClients_Load(object sender, EventArgs e)
        {

        }

        private void BuildUI()
        {
            var lbl = new Label { Text = "Recherche client :", Left = 15, Top = 15, AutoSize = true };
            txtSearch = new TextBox { Left = 140, Top = 12, Width = 300 };

            txtSearch.TextChanged += (s, e) => FindAndSelectRow(txtSearch.Text);
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    FindAndSelectRow(txtSearch.Text);
                }
            };

            btnRefresh = new Button { Text = "Rafraîchir", Left = 450, Top = 10, Width = 110, Height = 30 };
            btnRefresh.Click += (s, e) => LoadData();

            btnEncaisser = new Button { Text = "Encaisser paiement", Left = 570, Top = 10, Width = 170, Height = 30 };
            btnEncaisser.Click += (s, e) => EncaisserSelection();

            split = new SplitContainer
            {
                Left = 15,
                Top = 50,
                Width = 1050,
                Height = 540,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 340,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // ====== DGV CREDITS (haut) ======
            dgvCredits = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvCredits.Columns.Add(new DataGridViewTextBoxColumn { Name = "IdCredit", HeaderText = "IdCredit", Visible = false });
            dgvCredits.Columns.Add("ClientNom", "Client");
            dgvCredits.Columns.Add("Telephone", "Téléphone");
            dgvCredits.Columns.Add("RefVente", "Ref Vente");
            dgvCredits.Columns.Add("Total", "Total");
            dgvCredits.Columns.Add("Reste", "Reste");
            dgvCredits.Columns.Add("Echeance", "Échéance");
            dgvCredits.Columns.Add("DateCredit", "Date");
            dgvCredits.Columns.Add("Statut", "Statut");

            dgvCredits.SelectionChanged += (s, e) => ChargerPaiementsDuCreditSelectionne();
            dgvCredits.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EncaisserSelection(); };

            // ====== DGV PAIEMENTS (bas) ======
            dgvPays = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvPays.Columns.Add(new DataGridViewTextBoxColumn { Name = "IdPaiement", HeaderText = "IdPaiement", Width = 90 });
            dgvPays.Columns.Add(new DataGridViewTextBoxColumn { Name = "DatePaiement", HeaderText = "Date", Width = 160 });
            dgvPays.Columns.Add(new DataGridViewTextBoxColumn { Name = "Montant", HeaderText = "Montant", Width = 110 });
            dgvPays.Columns.Add(new DataGridViewTextBoxColumn { Name = "ModePaiement", HeaderText = "Mode", Width = 110 });
            dgvPays.Columns.Add(new DataGridViewTextBoxColumn { Name = "Note", HeaderText = "Note" });

            split.Panel1.Controls.Add(dgvCredits);
            split.Panel2.Controls.Add(dgvPays);

            Controls.AddRange(new Control[] { lbl, txtSearch, btnRefresh, btnEncaisser, split });
        }

        private void LoadData()
        {
            dgvCredits.Rows.Clear();

            var list = _svc.GetCreditsOuverts(null);

            foreach (var c in list)
            {
                dgvCredits.Rows.Add(
                    c.IdCredit,
                    c.ClientNom,
                    c.Telephone,
                    c.RefVente,
                    c.Total.ToString("N2"),
                    c.Reste.ToString("N2"),
                    c.DateEcheance.HasValue ? c.DateEcheance.Value.ToString("dd/MM/yyyy") : "",
                    c.DateCredit.ToString("dd/MM/yyyy"),
                    c.Statut
                );
            }

            if (dgvCredits.Rows.Count > 0)
            {
                dgvCredits.ClearSelection();
                dgvCredits.Rows[0].Selected = true;
                dgvCredits.CurrentCell = dgvCredits.Rows[0].Cells["ClientNom"];
                ChargerPaiementsDuCreditSelectionne();
            }

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                FindAndSelectRow(txtSearch.Text);
        }

        private void ChargerPaiementsDuCreditSelectionne()
        {
            dgvPays.Rows.Clear();

            var row = dgvCredits.CurrentRow;
            if (row == null || row.IsNewRow) return;

            object vId = row.Cells["IdCredit"]?.Value;
            if (vId == null || vId == DBNull.Value) return;

            if (!int.TryParse(vId.ToString(), out int idCredit) || idCredit <= 0) return;

            var dt = _svc.GetPaiementsCredit(idCredit);

            foreach (DataRow r in dt.Rows)
            {
                dgvPays.Rows.Add(
                    Convert.ToInt32(r["IdPaiement"]),
                    Convert.ToDateTime(r["DatePaiement"]).ToString("dd/MM/yyyy HH:mm"),
                    Convert.ToDecimal(r["Montant"]).ToString("N2"),
                    Convert.ToString(r["ModePaiement"]),
                    Convert.ToString(r["Note"])
                );
            }
        }


        // ✅ Le cœur : amener la sélection au nom tapé
        private bool FindAndSelectRow(string query)
        {
            query = (query ?? "").Trim();

            if (query.Length == 0)
            {
                // ✅ si vide : revenir au début
                if (dgvCredits.Rows.Count > 0)
                {
                    dgvCredits.ClearSelection();
                    dgvCredits.Rows[0].Selected = true;
                    dgvCredits.CurrentCell = dgvCredits.Rows[0].Cells["ClientNom"];
                    dgvCredits.FirstDisplayedScrollingRowIndex = 0;
                }
                return false;
            }

            for (int i = 0; i < dgvCredits.Rows.Count; i++)
            {
                var r = dgvCredits.Rows[i];
                if (r.IsNewRow) continue;

                string client = Convert.ToString(r.Cells["ClientNom"].Value) ?? "";
                string tel = dgvCredits.Columns.Contains("Telephone")
                    ? (Convert.ToString(r.Cells["Telephone"].Value) ?? "")
                    : "";

                // ✅ cherche par nom OU téléphone
                if (client.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    tel.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    dgvCredits.ClearSelection();
                    r.Selected = true;

                    var firstVisible = r.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => c.Visible);
                    if (firstVisible != null)
                        dgvCredits.CurrentCell = firstVisible;

                    dgvCredits.FirstDisplayedScrollingRowIndex = Math.Max(0, i);
                    return true;
                }
            }

            return false;
        }


        private decimal ParseDecimalFR(object v)
        {
            if (v == null) return 0m;
            decimal.TryParse(
                v.ToString(),
                NumberStyles.Any,
                CultureInfo.GetCultureInfo("fr-FR"),
                out var d
            );
            return d;
        }

        private void EncaisserSelection()
        {
            if (dgvCredits == null)
            {
                MessageBox.Show("Grille crédits non initialisée (dgvCredits=null).");
                return;
            }

            var row = dgvCredits.CurrentRow;
            if (row == null || row.IsNewRow)
            {
                MessageBox.Show("Sélectionne une ligne valide.");
                return;
            }

            if (!dgvCredits.Columns.Contains("IdCredit") ||
                !dgvCredits.Columns.Contains("ClientNom") ||
                !dgvCredits.Columns.Contains("Reste"))
            {
                var cols = string.Join(", ", dgvCredits.Columns.Cast<DataGridViewColumn>().Select(c => c.Name));
                MessageBox.Show("Colonnes attendues introuvables.\n" +
                                "Attendu: IdCredit, ClientNom, Reste\n" +
                                "Colonnes actuelles: " + cols);
                return;
            }

            object vId = row.Cells["IdCredit"]?.Value;
            object vClient = row.Cells["ClientNom"]?.Value;
            object vReste = row.Cells["Reste"]?.Value;

            if (vId == null || vId == DBNull.Value)
            {
                MessageBox.Show("IdCredit est vide sur cette ligne.");
                return;
            }

            if (!int.TryParse(vId.ToString(), out int idCredit) || idCredit <= 0)
            {
                MessageBox.Show("Crédit invalide (IdCredit).");
                return;
            }

            string client = (vClient == null || vClient == DBNull.Value) ? "" : vClient.ToString();

            decimal reste = 0m;
            if (vReste != null && vReste != DBNull.Value)
                reste = ParseDecimalFR(vReste);

            using (var f = new FrmEncaisserCredit(idCredit, client, reste))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                    LoadData();
            }
        }

    }
}
