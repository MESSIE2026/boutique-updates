using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmInventaireScanner : Form
    {
        private readonly string cs = ConfigSysteme.ConnectionString;

        ComboBox cmbDepot;
        Button btnNouvelleSession, btnAppliquerLigne, btnValider, btnRafraichir;
        Label lblSession;
        TextBox txtScan;
        NumericUpDown nudQte;
        DataGridView dgv;
        private Button btnPdf;
        public event Action InventaireValide;

        int _sessionId = 0;

        public FrmInventaireScanner()
        {
            InitializeComponent();

            Text = "Inventaire Scanner (PDA)";
            Width = 1000;
            Height = 650;

            BuildUI();

            Load += (s, e) =>
            {
                ChargerDepots();
                RafraichirGrille();
            };
        }

        void BuildUI()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 125 };
            Controls.Add(top);

            Label L(string t, int l, int tp) => new Label { Left = l, Top = tp, AutoSize = true, Text = t };

            top.Controls.Add(L("Dépôt", 15, 8));
            cmbDepot = new ComboBox
            {
                Left = 15,
                Top = 25,
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnNouvelleSession = new Button
            {
                Left = 265,
                Top = 23,
                Width = 160,
                Height = 28,
                Text = "Nouvelle session"
            };

            top.Controls.Add(L("Session", 435, 8));
            lblSession = new Label
            {
                Left = 435,
                Top = 28,
                Width = 260,
                Text = "Session: Aucune"
            };

            top.Controls.Add(L("Code barre (scan)", 15, 60));
            txtScan = new TextBox { Left = 15, Top = 78, Width = 260 };

            top.Controls.Add(L("Quantité comptée (Base)", 285, 60));
            nudQte = new NumericUpDown
            {
                Left = 285,
                Top = 78,
                Width = 100,
                Minimum = 1,
                Maximum = 999999,
                Value = 1
            };

            btnAppliquerLigne = new Button
            {
                Left = 395,
                Top = 76,
                Width = 150,
                Height = 28,
                Text = "Appliquer ligne"
            };

            btnRafraichir = new Button
            {
                Left = 555,
                Top = 76,
                Width = 120,
                Height = 28,
                Text = "Rafraîchir"
            };

            btnValider = new Button
            {
                Left = 685,
                Top = 76,
                Width = 180,
                Height = 28,
                Text = "Valider inventaire"
            };

            btnPdf = new Button
            {
                Left = 875,
                Top = 76,
                Width = 110,
                Height = 28,
                Text = "PDF"
            };

            top.Controls.AddRange(new Control[]
            {
                cmbDepot, btnNouvelleSession, lblSession,
                txtScan, nudQte, btnAppliquerLigne, btnRafraichir, btnValider, btnPdf
            });

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(dgv);

            btnNouvelleSession.Click += (s, e) => CreerSession();
            btnAppliquerLigne.Click += (s, e) => AppliquerLigneDepuisScan();
            btnRafraichir.Click += (s, e) => RafraichirGrille();
            btnValider.Click += (s, e) => ValiderSession();
            btnPdf.Click += (s, e) => ExporterPdfInventaire();

            txtScan.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    AppliquerLigneDepuisScan();
                }
            };
        }

        private void ExporterPdfInventaire()
        {
            if (_sessionId <= 0 || dgv.DataSource == null)
            {
                MessageBox.Show("Aucune session inventaire à exporter.");
                return;
            }

            string path = PdfExportHelper.AskSavePdfPath($"Inventaire_Session_{_sessionId}.pdf");
            if (path == null) return;

            var dt = PdfExportHelper.DataGridViewToDataTable(dgv);
            PdfExportHelper.ExportDataTableToPdf(path,
                "INVENTAIRE (Scanner)",
                dt,
                $"Session: {_sessionId} | Dépôt: {cmbDepot.Text}");

            MessageBox.Show("PDF généré ✅");
        }

        void ChargerDepots()
        {
            cmbDepot.Items.Clear();

            // ✅ TEXT par défaut (pas de CommandType ici)
            var dt = DbHelper.Table(cs,
                "SELECT ID_Depot, NomDepot FROM Depot ORDER BY NomDepot");

            foreach (DataRow r in dt.Rows)
                cmbDepot.Items.Add(new ComboboxItem(r["NomDepot"].ToString(), Convert.ToInt32(r["ID_Depot"])));

            if (cmbDepot.Items.Count > 0) cmbDepot.SelectedIndex = 0;
        }

        int? DepotId() => cmbDepot.SelectedItem is ComboboxItem it ? it.Value : (int?)null;

        void CreerSession()
        {
            var dep = DepotId();
            if (dep == null)
            {
                MessageBox.Show("Sélectionne un dépôt.");
                return;
            }

            // ✅ 3ème argument = timeoutSeconds => mettre 0
            object v = DbHelper.ScalarSp(cs, "dbo.sp_StartInventaireSession", 0,
                new SqlParameter("@ID_Depot", SqlDbType.Int) { Value = dep.Value },
                new SqlParameter("@CreePar", SqlDbType.NVarChar, 200)
                {
                    Value = (SessionEmploye.Nom + " " + SessionEmploye.Prenom).Trim()
                }
            );

            if (v == null || v == DBNull.Value)
            {
                MessageBox.Show("La procédure n'a pas retourné l'ID de session.");
                return;
            }

            _sessionId = Convert.ToInt32(v);
            lblSession.Text = "Session: " + _sessionId;

            MessageBox.Show("Session inventaire créée ✅");
            RafraichirGrille();
            txtScan.Focus();
        }

        void AppliquerLigneDepuisScan()
        {
            if (_sessionId <= 0)
            {
                MessageBox.Show("Crée d’abord une session inventaire.");
                return;
            }

            string code = (txtScan.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Scanne un code barre.");
                return;
            }

            int idProduit = TrouverProduitParCodeBarreStrict(code);
            if (idProduit <= 0)
                return;

            int qBase = (int)nudQte.Value;

            // ✅ Stored procedure => ExecSp
            DbHelper.ExecSp(cs, "dbo.sp_UpsertInventaireLigne", 0,
                new SqlParameter("@ID_Session", SqlDbType.Int) { Value = _sessionId },
                new SqlParameter("@ID_Produit", SqlDbType.Int) { Value = idProduit },
                new SqlParameter("@ID_Variante", SqlDbType.Int) { Value = DBNull.Value },
                // si ta SP attend int => remplace par SqlDbType.Int
                new SqlParameter("@QteCompteeBase", SqlDbType.Int) { Value = qBase }
            );

            try { System.Media.SystemSounds.Beep.Play(); } catch { }

            txtScan.Clear();
            nudQte.Value = 1;
            RafraichirGrille();
            txtScan.Focus();
        }

        int TrouverProduitParCodeBarreStrict(string code)
        {
            code = (code ?? "").Trim();
            if (code.Length == 0) return 0;

            object cntObj = DbHelper.Scalar(cs,
                "SELECT COUNT(*) FROM Produit WHERE CodeBarre=@c",
                new SqlParameter("@c", SqlDbType.NVarChar, 80) { Value = code });

            int cnt = (cntObj == null || cntObj == DBNull.Value) ? 0 : Convert.ToInt32(cntObj);
            if (cnt == 0) return 0;

            if (cnt > 1)
            {
                MessageBox.Show("⚠️ Code barre dupliqué en base. Corrige Produit.CodeBarre (doit être unique).");
                return 0;
            }

            object v = DbHelper.Scalar(cs,
                "SELECT ID_Produit FROM Produit WHERE CodeBarre=@c",
                new SqlParameter("@c", SqlDbType.NVarChar, 80) { Value = code });

            if (v == null || v == DBNull.Value) return 0;
            return Convert.ToInt32(v);
        }

        void RafraichirGrille()
        {
            if (_sessionId <= 0)
            {
                dgv.DataSource = null;
                return;
            }

            // ✅ TEXT par défaut
            var dt = DbHelper.Table(cs, @"
SELECT
    il.ID_Ligne,
    p.NomProduit,
    il.QteCompteeBase,
    il.DernierScan
FROM InventaireLigne il
JOIN Produit p ON p.ID_Produit=il.ID_Produit
WHERE il.ID_Session=@s
ORDER BY il.DernierScan DESC",
                new SqlParameter("@s", SqlDbType.Int) { Value = _sessionId });

            dgv.DataSource = dt;
            if (dgv.Columns.Contains("ID_Ligne"))
                dgv.Columns["ID_Ligne"].Visible = false;
        }

        void ValiderSession()
        {
            if (_sessionId <= 0)
            {
                MessageBox.Show("Aucune session inventaire.");
                return;
            }

            try
            {
                DbHelper.ExecSp(cs, "dbo.sp_ValiderInventaireSession", 0,
                    new SqlParameter("@ID_Session", SqlDbType.Int) { Value = _sessionId },
                    new SqlParameter("@Utilisateur", SqlDbType.NVarChar, 200)
                    {
                        Value = (SessionEmploye.Nom + " " + SessionEmploye.Prenom).Trim()
                    }
                );

                MessageBox.Show("Inventaire validé ✅ (ajustements appliqués)");
                InventaireValide?.Invoke();
                _sessionId = 0;
                lblSession.Text = "Session: Aucune";
                dgv.DataSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur validation : " + ex.Message);
            }
        }

        public class ComboboxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public ComboboxItem(string text, int value) { Text = text; Value = value; }
            public override string ToString() => Text;
        }
    }
}