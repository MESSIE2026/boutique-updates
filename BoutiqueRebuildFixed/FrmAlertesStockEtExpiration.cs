using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmAlertesStockEtExpiration : Form
    {
        private readonly string cs = ConfigSysteme.ConnectionString;

        private TabControl tabs;
        private DataGridView dgvStock;
        private DataGridView dgvExp;
        private Button btnRefresh;
        private Button btnPdf;
        private ComboBox cmbMagasinFiltre;

        private int? _idMagasinFiltre = null;

        public FrmAlertesStockEtExpiration()
        {
            InitializeComponent();

            Text = "Alertes Stock & Expiration";
            Width = 1100;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;

            // ===== UI =====
            tabs = new TabControl { Dock = DockStyle.Fill };

            var t1 = new TabPage("Stock sous seuil");
            var t2 = new TabPage("Expirations 90/60/30");

            dgvStock = BuildGrid();
            dgvExp = BuildGrid();

            t1.Controls.Add(dgvStock);
            t2.Controls.Add(dgvExp);

            tabs.TabPages.Add(t1);
            tabs.TabPages.Add(t2);

            btnRefresh = new Button { Dock = DockStyle.Top, Height = 32, Text = "Rafraîchir" };
            btnRefresh.Click += (s, e) => RefreshAll();

            btnPdf = new Button { Dock = DockStyle.Top, Height = 32, Text = "Exporter PDF (onglet)" };
            btnPdf.Click += (s, e) => ExporterPdfAlertes();

            cmbMagasinFiltre = new ComboBox
            {
                Dock = DockStyle.Top,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // ✅ Evite double abonnement (Designer + code) : on nettoie puis on abonne
            cmbMagasinFiltre.SelectedIndexChanged -= CmbMagasinFiltre_SelectedIndexChanged_Inline;
            cmbMagasinFiltre.SelectedIndexChanged += CmbMagasinFiltre_SelectedIndexChanged_Inline;

            Controls.Add(tabs);
            Controls.Add(btnPdf);
            Controls.Add(btnRefresh);
            Controls.Add(cmbMagasinFiltre);

            // ✅ Evite double Load (Designer + code)
            Load -= FrmAlertesStockEtExpiration_Load;
            Load += FrmAlertesStockEtExpiration_Load;
        }

        // Event inline sous forme de méthode privée UNIQUE (nom différent => pas conflit avec ton handler existant)
        private void CmbMagasinFiltre_SelectedIndexChanged_Inline(object sender, EventArgs e)
        {
            _idMagasinFiltre = (cmbMagasinFiltre.SelectedItem as ComboboxItem)?.Value;
            RefreshAll();
        }

        private DataGridView BuildGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White
            };
        }
        private void FrmAlertesStockEtExpiration_Load(object sender, EventArgs e)
        {
            ChargerMagasins();
            RefreshAll();
        }

        private void ChargerMagasins()
        {
            try
            {
                cmbMagasinFiltre.Items.Clear();
                cmbMagasinFiltre.Items.Add(new ComboboxItem("TOUS", 0));

                var dt = DbHelper.Table(cs, "SELECT IdMagasin, Nom FROM Magasin ORDER BY Nom");
                foreach (DataRow r in dt.Rows)
                    cmbMagasinFiltre.Items.Add(new ComboboxItem(r["Nom"].ToString(), Convert.ToInt32(r["IdMagasin"])));

                if (cmbMagasinFiltre.Items.Count > 0)
                    cmbMagasinFiltre.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement magasins : " + ex.Message);
            }
        }

        private void RefreshAll()
        {
            ChargerAlertesStock();
            ChargerAlertesExpiration();
        }

        private void ChargerAlertesStock()
        {
            try
            {
                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    using (var cmd = new SqlCommand("dbo.sp_GetStockAlerts", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Ajouter @IdMagasin seulement s'il existe dans le SP
                        SqlCommandBuilder.DeriveParameters(cmd);
                        if (cmd.Parameters.Contains("@IdMagasin"))
                            cmd.Parameters["@IdMagasin"].Value = _idMagasinFiltre ?? 0;

                        using (var da = new SqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);
                            dgvStock.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement alertes stock : " + ex.Message);
            }
        }

        private void ChargerAlertesExpiration()
        {
            try
            {
                using (var con = new SqlConnection(cs))
                {
                    con.Open();

                    using (var cmd = new SqlCommand("dbo.sp_GetExpirationAlerts", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlCommandBuilder.DeriveParameters(cmd);
                        if (cmd.Parameters.Contains("@IdMagasin"))
                            cmd.Parameters["@IdMagasin"].Value = _idMagasinFiltre ?? 0;

                        using (var da = new SqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);
                            dgvExp.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement alertes expiration : " + ex.Message);
            }
        }

        private void ExporterPdfAlertes()
        {
            DataGridView grid = (tabs.SelectedIndex == 0) ? dgvStock : dgvExp;

            if (grid.DataSource == null)
            {
                MessageBox.Show("Rien à exporter.");
                return;
            }

            string name = (tabs.SelectedIndex == 0) ? "Alertes_Stock" : "Alertes_Expiration";
            string path = PdfExportHelper.AskSavePdfPath($"{name}_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            if (path == null) return;

            var dt = PdfExportHelper.DataGridViewToDataTable(grid);

            PdfExportHelper.ExportDataTableToPdf(
                path,
                (tabs.SelectedIndex == 0)
                    ? "ALERTES STOCK (Sous seuil)"
                    : "ALERTES EXPIRATION (90/60/30)",
                dt
            );

            MessageBox.Show("PDF généré ✅");
        }
    }
}