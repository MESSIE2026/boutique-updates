using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmChoisirEquivalentVente : Form
    {
        private readonly DataGridView dgv = new DataGridView();
        private readonly Button btnOk = new Button();
        private readonly Button btnCancel = new Button();

        public int SelectedProduitId { get; private set; }

        // ✅ Constructeur vide (si tu veux garder Designer)
        public FrmChoisirEquivalentVente()
        {
        }

        // ✅ Constructeur utilisé par la vente
        public FrmChoisirEquivalentVente(DataTable dt) : this()
        {
            BuildUi(dt);
        }

        private void BuildUi(DataTable dt)
        {
            this.Text = "Choisir un équivalent disponible";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(820, 420);

            // Nettoyer si designer a déjà mis des contrôles
            this.Controls.Clear();

            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.DataSource = dt;

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 48 };

            btnOk.Text = "OK";
            btnOk.Width = 100;
            btnOk.Left = 580;
            btnOk.Top = 8;

            btnCancel.Text = "Annuler";
            btnCancel.Width = 100;
            btnCancel.Left = 690;
            btnCancel.Top = 8;

            btnOk.Click -= BtnOk_Click;
            btnOk.Click += BtnOk_Click;

            btnCancel.Click -= BtnCancel_Click;
            btnCancel.Click += BtnCancel_Click;

            bottom.Controls.Add(btnOk);
            bottom.Controls.Add(btnCancel);

            this.Controls.Add(dgv);
            this.Controls.Add(bottom);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Choisis un produit.");
                return;
            }

            object v = dgv.CurrentRow.Cells["ID_Produit"]?.Value;
            if (v == null || v == DBNull.Value)
            {
                MessageBox.Show("Sélection invalide.");
                return;
            }

            SelectedProduitId = Convert.ToInt32(v);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}