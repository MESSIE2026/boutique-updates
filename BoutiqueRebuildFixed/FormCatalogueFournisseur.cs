using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FormCatalogueFournisseur : FormBase
    {
        private readonly string _cs = ConfigSysteme.ConnectionString;

        public class ComboboxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public ComboboxItem(string text, int value) { Text = text; Value = value; }
            public override string ToString() => Text;
        }
        public FormCatalogueFournisseur()
        {
            InitializeComponent();

            // Norme "FormBase"
            

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;

            this.Load += FormCatalogueFournisseur_Load;

            cmbFournisseur.SelectedIndexChanged += cmbFournisseur_SelectedIndexChanged;
            dgvCatalogue.CellClick += dgvCatalogue_CellClick;

            btnAjouter.Click += btnAjouter_Click;
            btnModifier.Click += btnModifier_Click;
            btnSupprimer.Click += btnSupprimer_Click;
            btnNouveau.Click += btnNouveau_Click;
            btnFermer.Click += (s, e) => this.Close();
        }

        private void RafraichirLangue() => ConfigSysteme.AppliquerTraductions(this);
        private void RafraichirTheme() => ConfigSysteme.AppliquerTheme(this);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }

        private void ConfigurerDgv()
        {
            dgvCatalogue.AutoGenerateColumns = true;
            dgvCatalogue.ReadOnly = true;
            dgvCatalogue.AllowUserToAddRows = false;
            dgvCatalogue.AllowUserToDeleteRows = false;
            dgvCatalogue.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCatalogue.MultiSelect = false;
            dgvCatalogue.ScrollBars = ScrollBars.Both;
            dgvCatalogue.RowHeadersVisible = false;
        }

        private void ChargerFournisseurs()
        {
            cmbFournisseur.BeginUpdate();
            try
            {
                cmbFournisseur.Items.Clear();

                using (SqlConnection con = new SqlConnection(_cs))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
SELECT ID_Fournisseur, Nom
FROM dbo.Fournisseur
WHERE Actif = 1
ORDER BY Nom;", con))
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            cmbFournisseur.Items.Add(
                                new ComboboxItem(rd["Nom"].ToString(), Convert.ToInt32(rd["ID_Fournisseur"]))
                            );
                        }
                    }
                }

                if (cmbFournisseur.Items.Count > 0)
                    cmbFournisseur.SelectedIndex = 0;
            }
            finally
            {
                cmbFournisseur.EndUpdate();
            }
        }


        private void ChargerProduits()
        {
            cmbProduit.Items.Clear();

            using (SqlConnection con = new SqlConnection(_cs))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"
SELECT ID_Produit, NomProduit
FROM dbo.Produit
ORDER BY NomProduit;", con))
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        cmbProduit.Items.Add(
                            new ComboboxItem(rd["NomProduit"].ToString(), Convert.ToInt32(rd["ID_Produit"]))
                        );
                    }
                }
            }

            if (cmbProduit.Items.Count > 0)
                cmbProduit.SelectedIndex = 0;

            cmbProduit.DropDownStyle = ComboBoxStyle.DropDown;
            cmbProduit.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbProduit.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        private bool TryGetSelectedFournisseurId(out int idFournisseur)
        {
            idFournisseur = 0;

            if (cmbFournisseur.SelectedItem is ComboboxItem it && it.Value > 0)
            {
                idFournisseur = it.Value;
                return true;
            }
            return false;
        }

        private bool TryGetSelectedProduitId(out int idProduit)
        {
            idProduit = 0;

            if (cmbProduit.SelectedItem is ComboboxItem it && it.Value > 0)
            {
                idProduit = it.Value;
                return true;
            }
            return false;
        }

        private void cmbFournisseur_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TryGetSelectedFournisseurId(out int idF))
                ChargerCatalogue(idF);

            btnNouveau_Click(null, null);
        }

        private void ChargerCatalogue(int idFournisseur)
        {
            using (SqlConnection con = new SqlConnection(_cs))
            {
                con.Open();
                string sql = @"
SELECT 
    fc.ID,
    fc.ID_Fournisseur,
    f.Nom AS Fournisseur,
    fc.ID_Produit,
    p.NomProduit,
    fc.PrixAchat,
    fc.Devise,
    fc.DelaiJours,
    fc.DateMaj
FROM dbo.FournisseurCatalogue fc
INNER JOIN dbo.Fournisseur f ON f.ID_Fournisseur = fc.ID_Fournisseur
INNER JOIN dbo.Produit p ON p.ID_Produit = fc.ID_Produit
WHERE fc.ID_Fournisseur = @idf
ORDER BY p.NomProduit;";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@idf", SqlDbType.Int).Value = idFournisseur;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvCatalogue.DataSource = null;
                    dgvCatalogue.DataSource = dt;

                    if (dgvCatalogue.Columns.Contains("ID")) dgvCatalogue.Columns["ID"].Visible = false;
                    if (dgvCatalogue.Columns.Contains("ID_Fournisseur")) dgvCatalogue.Columns["ID_Fournisseur"].Visible = false;
                    if (dgvCatalogue.Columns.Contains("ID_Produit")) dgvCatalogue.Columns["ID_Produit"].Visible = false;
                }
            }
        }

        private int GetSelectedCatalogueId()
        {
            if (dgvCatalogue.CurrentRow == null) return 0;

            object v = dgvCatalogue.CurrentRow.Cells["ID"]?.Value;
            if (v == null || v == DBNull.Value) return 0;

            int id;
            return int.TryParse(v.ToString(), out id) ? id : 0;
        }

        private void dgvCatalogue_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvCatalogue.Rows[e.RowIndex];

            // Remplir produit sélectionné
            int idProduit = 0;
            int.TryParse(row.Cells["ID_Produit"]?.Value?.ToString(), out idProduit);

            // Positionner cmbProduit sur cet item
            foreach (var item in cmbProduit.Items.Cast<object>())
            {
                if (item is ComboboxItem it && it.Value == idProduit)
                {
                    cmbProduit.SelectedItem = item;
                    break;
                }
            }

            txtPrixAchat.Text = row.Cells["PrixAchat"]?.Value?.ToString() ?? "";
            cmbDevise.Text = row.Cells["Devise"]?.Value?.ToString() ?? "CDF";

            int delai = 0;
            int.TryParse(row.Cells["DelaiJours"]?.Value?.ToString(), out delai);
            if (delai < (int)numDelaiJours.Minimum) delai = (int)numDelaiJours.Minimum;
            if (delai > (int)numDelaiJours.Maximum) delai = (int)numDelaiJours.Maximum;
            numDelaiJours.Value = delai;
        }
        private void btnAjouter_Click(object sender, EventArgs e)
        {
            if (!Valider()) return;

            TryGetSelectedFournisseurId(out int idF);
            TryGetSelectedProduitId(out int idP);

            if (!decimal.TryParse(txtPrixAchat.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var prix) || prix <= 0)
            {
                MessageBox.Show("Prix achat invalide.");
                return;
            }
            int delai = (int)numDelaiJours.Value;
            string devise = cmbDevise.Text;

            try
            {
                using (SqlConnection con = new SqlConnection(_cs))
                {
                    con.Open();

                    // Si existe déjà => on bloque (ou on peut faire UPDATE)
                    using (SqlCommand check = new SqlCommand(@"
SELECT COUNT(*) FROM dbo.FournisseurCatalogue
WHERE ID_Fournisseur=@f AND ID_Produit=@p;", con))
                    {
                        check.Parameters.Add("@f", SqlDbType.Int).Value = idF;
                        check.Parameters.Add("@p", SqlDbType.Int).Value = idP;

                        int exists = Convert.ToInt32(check.ExecuteScalar());
                        if (exists > 0)
                        {
                            MessageBox.Show("Ce produit existe déjà dans le catalogue de ce fournisseur. Utilise Modifier.");
                            return;
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.FournisseurCatalogue
(ID_Fournisseur, ID_Produit, PrixAchat, Devise, DelaiJours, DateMaj)
VALUES
(@F, @P, @Prix, @Devise, @Delai, GETDATE());", con))
                    {
                        cmd.Parameters.Add("@F", SqlDbType.Int).Value = idF;
                        cmd.Parameters.Add("@P", SqlDbType.Int).Value = idP;
                        var pPrix = cmd.Parameters.Add("@Prix", SqlDbType.Decimal);
                        pPrix.Precision = 18;
                        pPrix.Scale = 2;
                        pPrix.Value = prix;
                        cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = devise;
                        cmd.Parameters.Add("@Delai", SqlDbType.Int).Value = delai;

                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog("Catalogue Fournisseur",
                    $"Ajout: FournisseurID={idF} ProduitID={idP} Prix={prix} {devise} Delai={delai}",
                    "Succès");

                ChargerCatalogue(idF);
                btnNouveau_Click(null, null);
                MessageBox.Show("Catalogue ajouté ✅");
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Catalogue Fournisseur", ex.Message, "Échec");
                MessageBox.Show("Erreur ajout catalogue : " + ex.Message);
            }
        }
        
        

        private void btnModifier_Click(object sender, EventArgs e)
        {
            int idCat = GetSelectedCatalogueId();
            if (idCat <= 0)
            {
                MessageBox.Show("Sélectionnez une ligne à modifier dans le catalogue.");
                return;
            }
            if (!Valider()) return;

            TryGetSelectedFournisseurId(out int idF);
            TryGetSelectedProduitId(out int idP);

            decimal prix = decimal.Parse(txtPrixAchat.Text);
            int delai = (int)numDelaiJours.Value;
            string devise = cmbDevise.Text;

            try
            {
                using (SqlConnection con = new SqlConnection(_cs))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.FournisseurCatalogue SET
    ID_Produit=@P,
    PrixAchat=@Prix,
    Devise=@Devise,
    DelaiJours=@Delai,
    DateMaj=GETDATE()
WHERE ID=@ID;", con))
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = idCat;
                        cmd.Parameters.Add("@P", SqlDbType.Int).Value = idP;
                        var pPrix = cmd.Parameters.Add("@Prix", SqlDbType.Decimal);
                        pPrix.Precision = 18;
                        pPrix.Scale = 2;
                        pPrix.Value = prix;
                        cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = devise;
                        cmd.Parameters.Add("@Delai", SqlDbType.Int).Value = delai;

                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog("Catalogue Fournisseur",
                    $"Modification: CatalogueID={idCat} ProduitID={idP} Prix={prix} {devise} Delai={delai}",
                    "Succès");

                ChargerCatalogue(idF);
                MessageBox.Show("Catalogue modifié ✅");
            }
            catch (SqlException ex)
            {
                // Si tu as ajouté l'index unique, une tentative de doublon va tomber ici
                ConfigSysteme.AjouterAuditLog("Catalogue Fournisseur", ex.Message, "Échec");
                MessageBox.Show("Erreur SQL : " + ex.Message);
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Catalogue Fournisseur", ex.Message, "Échec");
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            int idCat = GetSelectedCatalogueId();
            if (idCat <= 0)
            {
                MessageBox.Show("Sélectionnez une ligne à supprimer dans le catalogue.");
                return;
            }

            if (MessageBox.Show("Confirmer la suppression ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            TryGetSelectedFournisseurId(out int idF);

            try
            {
                using (SqlConnection con = new SqlConnection(_cs))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.FournisseurCatalogue WHERE ID=@id;", con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCat;
                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog("Catalogue Fournisseur", $"Suppression CatalogueID={idCat}", "Succès");

                ChargerCatalogue(idF);
                btnNouveau_Click(null, null);
                MessageBox.Show("Ligne supprimée 🗑️");
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Catalogue Fournisseur", ex.Message, "Échec");
                MessageBox.Show("Erreur suppression : " + ex.Message);
            }
        }

private void btnNouveau_Click(object sender, EventArgs e)
        {
            txtPrixAchat.Clear();
            cmbDevise.SelectedIndex = 0;
            numDelaiJours.Value = 0;
        }

        private bool Valider()
        {
            if (!TryGetSelectedFournisseurId(out _))
            {
                MessageBox.Show("Sélectionnez un fournisseur.");
                return false;
            }

            if (!TryGetSelectedProduitId(out _))
            {
                MessageBox.Show("Sélectionnez un produit.");
                return false;
            }

            if (!decimal.TryParse(txtPrixAchat.Text, out decimal prix) || prix <= 0)
            {
                MessageBox.Show("Prix achat invalide.");
                return false;
            }

            if (cmbDevise.SelectedIndex < 0)
            {
                MessageBox.Show("Sélectionnez une devise.");
                return false;
            }

            return true;
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {

        }

        private void FormCatalogueFournisseur_Load(object sender, EventArgs e)
        {
            ConfigurerDgv();

            cmbDevise.Items.Clear();
            cmbDevise.Items.AddRange(new object[] { "CDF", "USD", "EUR", "ZAR" });
            cmbDevise.SelectedIndex = 0;

            numDelaiJours.Minimum = 0;
            numDelaiJours.Maximum = 365;
            numDelaiJours.Value = 0;

            cmbFournisseur.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFournisseur.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbFournisseur.AutoCompleteMode = AutoCompleteMode.SuggestAppend; // OK avec ListItems

            ChargerFournisseurs();
            ChargerProduits();

            RafraichirLangue();
            RafraichirTheme();

            // Charger catalogue du premier fournisseur
            if (TryGetSelectedFournisseurId(out int idF))
                ChargerCatalogue(idF);
        }
    }
}
