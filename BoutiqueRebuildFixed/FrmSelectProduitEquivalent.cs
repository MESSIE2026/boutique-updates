using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public partial class FrmSelectProduitEquivalent : Form
    {
        private readonly string _cs;
        private readonly int _idProduitSource;

        public int SelectedProduitId { get; private set; }
        public string SelectedType { get; private set; } = "EQUIVALENT";
        public int SelectedPriorite { get; private set; } = 1;

        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cmbType;
        private NumericUpDown numPrio;
        private Button btnOk, btnCancel;

        public FrmSelectProduitEquivalent(string connectionString, int idProduitSource)
        {
            _cs = connectionString;
            _idProduitSource = idProduitSource;

            BuildUi();
            LoadProduits();
        }

        private void SetPlaceholder(TextBox tb, string placeholder)
        {
            tb.ForeColor = Color.Gray;
            tb.Text = placeholder;

            tb.GotFocus += (s, e) =>
            {
                if (tb.ForeColor == Color.Gray)
                {
                    tb.Text = "";
                    tb.ForeColor = Color.Black;
                }
            };

            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.ForeColor = Color.Gray;
                    tb.Text = placeholder;
                }
            };
        }

        private void BuildUi()
        {
            this.Text = "Choisir un produit équivalent";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(720, 480);

            txtSearch = new TextBox { Dock = DockStyle.Top };
            SetPlaceholder(txtSearch, "Rechercher...");
            txtSearch.TextChanged += (s, e) =>
            {
                if (txtSearch.ForeColor == Color.Gray) return;
                LoadProduits(txtSearch.Text.Trim());
            };
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 48 };

            cmbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140, Left = 8, Top = 10 };
            cmbType.Items.AddRange(new[] { "EQUIVALENT", "ALTERNATIVE", "REMPLACANT" });
            cmbType.SelectedIndex = 0;

            numPrio = new NumericUpDown { Minimum = 1, Maximum = 99, Value = 1, Width = 60, Left = 160, Top = 10 };

            btnOk = new Button { Text = "OK", Width = 90, Left = 520, Top = 8 };
            btnCancel = new Button { Text = "Annuler", Width = 90, Left = 610, Top = 8 };

            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            panelBottom.Controls.Add(cmbType);
            panelBottom.Controls.Add(numPrio);
            panelBottom.Controls.Add(btnOk);
            panelBottom.Controls.Add(btnCancel);

            this.Controls.Add(dgv);
            this.Controls.Add(panelBottom);
            this.Controls.Add(txtSearch);
        }

        private void LoadProduits(string filter = "")
        {
            using (var con = new SqlConnection(_cs))
            using (var da = new SqlDataAdapter(@"
SELECT ID_Produit, NomProduit, RefProduit, CodeBarre, Prix, Devise, Categorie, Taille, Couleur
FROM dbo.Produit
WHERE ID_Produit <> @src
  AND (@f = '' OR NomProduit LIKE '%' + @f + '%' OR RefProduit LIKE '%' + @f + '%' OR CodeBarre LIKE '%' + @f + '%')
ORDER BY NomProduit;", con))
            {
                da.SelectCommand.Parameters.Add("@src", SqlDbType.Int).Value = _idProduitSource;
                da.SelectCommand.Parameters.Add("@f", SqlDbType.NVarChar, 200).Value = (filter ?? "").Trim();

                var dt = new DataTable();
                da.Fill(dt);
                dgv.DataSource = dt;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Choisissez un produit.");
                return;
            }

            var v = dgv.CurrentRow.Cells["ID_Produit"]?.Value;
            if (v == null || v == DBNull.Value)
            {
                MessageBox.Show("Sélection invalide.");
                return;
            }

            SelectedProduitId = Convert.ToInt32(v);
            SelectedType = cmbType.Text;
            SelectedPriorite = (int)numPrio.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
