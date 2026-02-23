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
    public partial class FrmConfigPostePOS : Form
    {
        private readonly string _cs = ConfigSysteme.ConnectionString;

        private Label lblMachine;
        private TextBox txtMachine;

        private Label lblEntreprise;
        private ComboBox cmbEntreprise;

        private Label lblMagasin;
        private ComboBox cmbMagasin;

        private Label lblVille;
        private TextBox txtVille;

        private Label lblAdresse;
        private TextBox txtAdresse;

        private Label lblNomPos;
        private TextBox txtNomPOS;

        private Button btnEnregistrer;
        private Button btnFermer;
        private Button btnRefresh;

        private Button btnAddEntreprise;
        private Button btnAddMagasin;

        private DataTable _dtEntreprise;
        private DataTable _dtMagasin;

        public FrmConfigPostePOS()
        {
            BuildUI();

            Load += FrmConfigPostePOS_Load;
            cmbEntreprise.SelectedIndexChanged += CmbEntreprise_SelectedIndexChanged;

            // ✅ Quand on change le magasin -> maj Ville + Adresse
            cmbMagasin.SelectedIndexChanged += (s, e) => UpdateInfosFromSelectedMagasin();

            btnEnregistrer.Click += BtnEnregistrer_Click;
            btnFermer.Click += (s, e) => Close();
            btnRefresh.Click += (s, e) => ReloadAll();

            btnAddEntreprise.Click += BtnAddEntreprise_Click;
            btnAddMagasin.Click += BtnAddMagasin_Click;
        }

        private void BuildUI()
        {
            Text = "Configuration Poste POS";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 560;
            Height = 380;

            int leftLabel = 20;
            int leftControl = 190;
            int top = 20;
            int h = 28;
            int gap = 12;
            int wControl = 320;

            lblMachine = new Label { Left = leftLabel, Top = top, Width = 160, Height = h, Text = "MachineName :" };
            txtMachine = new TextBox
            {
                Left = leftControl,
                Top = top,
                Width = wControl,
                Height = h,
                ReadOnly = true
            };
            top += h + gap;

            lblEntreprise = new Label { Left = leftLabel, Top = top, Width = 160, Height = h, Text = "Entreprise :" };
            cmbEntreprise = new ComboBox
            {
                Left = leftControl,
                Top = top,
                Width = wControl,
                Height = h,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnAddEntreprise = new Button
            {
                Left = leftControl + wControl - 34,
                Top = top,
                Width = 34,
                Height = h,
                Text = "+"
            };
            cmbEntreprise.Width = wControl - 40;

            top += h + gap;

            lblMagasin = new Label { Left = leftLabel, Top = top, Width = 160, Height = h, Text = "Magasin :" };
            cmbMagasin = new ComboBox
            {
                Left = leftControl,
                Top = top,
                Width = wControl,
                Height = h,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            btnAddMagasin = new Button
            {
                Left = leftControl + wControl - 34,
                Top = top,
                Width = 34,
                Height = h,
                Text = "+"
            };
            cmbMagasin.Width = wControl - 40;

            top += h + gap;

            lblVille = new Label { Left = leftLabel, Top = top, Width = 160, Height = h, Text = "Ville :" };
            txtVille = new TextBox
            {
                Left = leftControl,
                Top = top,
                Width = wControl,
                Height = h,
                ReadOnly = true
            };

            top += h + gap;

            lblAdresse = new Label { Left = leftLabel, Top = top, Width = 160, Height = h, Text = "Adresse :" };
            txtAdresse = new TextBox
            {
                Left = leftControl,
                Top = top,
                Width = wControl,
                Height = h,
                ReadOnly = true
            };

            top += h + gap;

            lblNomPos = new Label { Left = leftLabel, Top = top, Width = 160, Height = h, Text = "Nom POS :" };
            txtNomPOS = new TextBox
            {
                Left = leftControl,
                Top = top,
                Width = wControl,
                Height = h
            };

            top += h + 18;

            btnEnregistrer = new Button { Left = leftControl, Top = top, Width = 140, Height = 34, Text = "Enregistrer" };
            btnRefresh = new Button { Left = leftControl + 150, Top = top, Width = 120, Height = 34, Text = "Rafraîchir" };
            btnFermer = new Button { Left = leftControl + 280, Top = top, Width = 90, Height = 34, Text = "Fermer" };

            Controls.AddRange(new Control[]
            {
                lblMachine, txtMachine,

                lblEntreprise, cmbEntreprise, btnAddEntreprise,
                lblMagasin, cmbMagasin, btnAddMagasin,

                lblVille, txtVille,
                lblAdresse, txtAdresse,

                lblNomPos, txtNomPOS,

                btnEnregistrer, btnRefresh, btnFermer
            });
        }

        private void FrmConfigPostePOS_Load(object sender, EventArgs e)
            {
                txtMachine.Text = Environment.MachineName;
                ReloadAll();
            }

        private void ReloadAll()
        {
            try
            {
                ChargerEntreprises();
                ChargerMagasins();
                ChargerConfigurationPosteSiExiste();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement configuration POS : " + ex.Message,
                    "POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // 1) CHARGER ENTREPRISES
        // =========================
        private void ChargerEntreprises()
        {
            using (var con = new SqlConnection(_cs))
            {
                con.Open();
                using (var da = new SqlDataAdapter(@"
SELECT IdEntreprise, Nom
FROM dbo.Entreprise
ORDER BY Nom;", con))
                {
                    _dtEntreprise = new DataTable();
                    da.Fill(_dtEntreprise);
                }
            }

            cmbEntreprise.DataSource = null;
            cmbEntreprise.DisplayMember = "Nom";
            cmbEntreprise.ValueMember = "IdEntreprise";
            cmbEntreprise.DataSource = _dtEntreprise;
        }

        private static string AskText(string label, string caption)
        {
            using (Form f = new Form())
            {
                f.Text = caption;
                f.Width = 420;
                f.Height = 165;
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false;
                f.MinimizeBox = false;

                Label lbl = new Label { Left = 12, Top = 12, Width = 380, Text = label };
                TextBox tb = new TextBox { Left = 12, Top = 38, Width = 380 };

                Button ok = new Button { Text = "OK", Left = 232, Width = 80, Top = 78, DialogResult = DialogResult.OK };
                Button cancel = new Button { Text = "Annuler", Left = 312, Width = 80, Top = 78, DialogResult = DialogResult.Cancel };

                f.Controls.Add(lbl);
                f.Controls.Add(tb);
                f.Controls.Add(ok);
                f.Controls.Add(cancel);

                f.AcceptButton = ok;
                f.CancelButton = cancel;

                return f.ShowDialog() == DialogResult.OK ? tb.Text : null;
            }
        }

        private void BtnAddEntreprise_Click(object sender, EventArgs e)
        {
            string nom = AskText("Nom de l'entreprise :", "Nouvelle entreprise");
            if (string.IsNullOrWhiteSpace(nom)) return;

            try
            {
                int newId;
                using (var con = new SqlConnection(_cs))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Entreprise(Nom)
OUTPUT INSERTED.IdEntreprise
VALUES(@nom);", con))
                    {
                        cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 200).Value = nom.Trim();
                        newId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                ChargerEntreprises();
                cmbEntreprise.SelectedValue = newId;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ajout entreprise : " + ex.Message);
            }
        }

        private void BtnAddMagasin_Click(object sender, EventArgs e)
        {
            if (cmbEntreprise.SelectedValue == null)
            {
                MessageBox.Show("Choisis d'abord une entreprise.");
                return;
            }

            int idEntreprise = Convert.ToInt32(cmbEntreprise.SelectedValue);

            string nomMag = AskText("Nom du magasin :", "Nouveau magasin");
            if (string.IsNullOrWhiteSpace(nomMag)) return;

            string ville = AskText("Ville du magasin :", "Ville");
            if (ville == null) ville = "";

            string adresse = AskText("Adresse du magasin :", "Adresse");
            if (adresse == null) adresse = "";

            try
            {
                EnsureVilleColumnExists();
                EnsureAdresseColumnExists();

                int newId;
                using (var con = new SqlConnection(_cs))
                {
                    con.Open();
                    using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Magasin(IdEntreprise, Nom, Ville, Adresse)
OUTPUT INSERTED.IdMagasin
VALUES(@e, @nom, @ville, @adr);", con))
                    {
                        cmd.Parameters.Add("@e", SqlDbType.Int).Value = idEntreprise;
                        cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 200).Value = nomMag.Trim();
                        cmd.Parameters.Add("@ville", SqlDbType.NVarChar, 100).Value = ville.Trim();
                        cmd.Parameters.Add("@adr", SqlDbType.NVarChar, 250).Value = adresse.Trim();
                        newId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                ChargerMagasins();
                FilterMagasinsByEntreprise();
                cmbMagasin.SelectedValue = newId;

                UpdateInfosFromSelectedMagasin();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ajout magasin : " + ex.Message);
            }
        }

        private void EnsureVilleColumnExists()
        {
            using (var con = new SqlConnection(_cs))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
IF COL_LENGTH('dbo.Magasin', 'Ville') IS NULL
    ALTER TABLE dbo.Magasin ADD Ville NVARCHAR(100) NULL;", con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void EnsureAdresseColumnExists()
        {
            using (var con = new SqlConnection(_cs))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
IF COL_LENGTH('dbo.Magasin', 'Adresse') IS NULL
    ALTER TABLE dbo.Magasin ADD Adresse NVARCHAR(250) NULL;", con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // =========================
        // 2) CHARGER MAGASINS
        // =========================
        private void ChargerMagasins()
        {
            using (var con = new SqlConnection(_cs))
            {
                con.Open();
                using (var da = new SqlDataAdapter(@"
SELECT IdMagasin, IdEntreprise, Nom,
       ISNULL(Ville,'') AS Ville,
       ISNULL(Adresse,'') AS Adresse,
       (Nom 
            + CASE WHEN ISNULL(Ville,'') <> '' THEN ' - ' + Ville ELSE '' END
       ) AS Libelle
FROM dbo.Magasin
ORDER BY Nom;", con))
                {
                    _dtMagasin = new DataTable();
                    da.Fill(_dtMagasin);
                }
            }

            FilterMagasinsByEntreprise();
        }

        private void CmbEntreprise_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterMagasinsByEntreprise();
        }

        private void FilterMagasinsByEntreprise()
        {
            if (_dtMagasin == null) return;

            int idEntreprise = 0;
            if (cmbEntreprise.SelectedValue != null)
                int.TryParse(cmbEntreprise.SelectedValue.ToString(), out idEntreprise);

            DataView dv = new DataView(_dtMagasin);

            if (idEntreprise > 0)
                dv.RowFilter = "IdEntreprise = " + idEntreprise;
            else
                dv.RowFilter = "1=0";

            cmbMagasin.DataSource = null;
            cmbMagasin.DisplayMember = "Libelle";
            cmbMagasin.ValueMember = "IdMagasin";
            cmbMagasin.DataSource = dv;

            if (cmbMagasin.Items.Count > 0)
                cmbMagasin.SelectedIndex = 0;

            UpdateInfosFromSelectedMagasin();
        }

        private void UpdateInfosFromSelectedMagasin()
        {
            if (_dtMagasin == null || cmbMagasin.SelectedValue == null)
            {
                txtVille.Text = "";
                txtAdresse.Text = "";
                return;
            }

            int idMag = 0;
            int.TryParse(cmbMagasin.SelectedValue.ToString(), out idMag);
            if (idMag <= 0)
            {
                txtVille.Text = "";
                txtAdresse.Text = "";
                return;
            }

            var rows = _dtMagasin.Select("IdMagasin = " + idMag);
            if (rows.Length == 0)
            {
                txtVille.Text = "";
                txtAdresse.Text = "";
                return;
            }

            txtVille.Text = rows[0]["Ville"]?.ToString() ?? "";
            txtAdresse.Text = rows[0]["Adresse"]?.ToString() ?? "";
        }

        // ==========================================
        // 3) CHARGER CONFIG POSTE SI EXISTE
        // ==========================================
        private void ChargerConfigurationPosteSiExiste()
        {
            string machine = Environment.MachineName;

            using (var con = new SqlConnection(_cs))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 IdPoste, IdEntreprise, IdMagasin, NomPOS
FROM dbo.PostePOS
WHERE MachineName = @m;", con))
                {
                    cmd.Parameters.Add("@m", SqlDbType.NVarChar, 120).Value = machine;

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                            return;

                        int idEntreprise = Convert.ToInt32(rd["IdEntreprise"]);
                        int idMagasin = Convert.ToInt32(rd["IdMagasin"]);
                        string nomPos = rd["NomPOS"]?.ToString() ?? "";

                        cmbEntreprise.SelectedValue = idEntreprise;

                        FilterMagasinsByEntreprise();
                        cmbMagasin.SelectedValue = idMagasin;

                        UpdateInfosFromSelectedMagasin();

                        txtNomPOS.Text = nomPos;
                    }
                }
            }
        }

        // =========================
        // 4) ENREGISTRER (UPSERT)
        // =========================
        private void BtnEnregistrer_Click(object sender, EventArgs e)
        {
            string machine = Environment.MachineName;

            if (cmbEntreprise.SelectedValue == null)
            {
                MessageBox.Show("Choisis une entreprise.");
                return;
            }
            if (cmbMagasin.SelectedValue == null)
            {
                MessageBox.Show("Choisis un magasin.");
                return;
            }

            int idEntreprise = Convert.ToInt32(cmbEntreprise.SelectedValue);
            int idMagasin = Convert.ToInt32(cmbMagasin.SelectedValue);
            string nomPos = (txtNomPOS.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(nomPos))
            {
                MessageBox.Show("NomPOS obligatoire.");
                return;
            }

            try
            {
                using (var con = new SqlConnection(_cs))
                {
                    con.Open();

                    int idPosteExistant = 0;
                    using (var cmdCheck = new SqlCommand(
                        "SELECT TOP 1 IdPoste FROM dbo.PostePOS WHERE MachineName=@m", con))
                    {
                        cmdCheck.Parameters.Add("@m", SqlDbType.NVarChar, 120).Value = machine;

                        object existing = cmdCheck.ExecuteScalar();
                        if (existing != null && existing != DBNull.Value)
                            idPosteExistant = Convert.ToInt32(existing);
                    }

                    if (idPosteExistant <= 0)
                    {
                        using (var cmdIns = new SqlCommand(@"
INSERT INTO dbo.PostePOS(IdEntreprise, IdMagasin, NomPOS, MachineName)
VALUES(@e, @mag, @nom, @m);", con))
                        {
                            cmdIns.Parameters.Add("@e", SqlDbType.Int).Value = idEntreprise;
                            cmdIns.Parameters.Add("@mag", SqlDbType.Int).Value = idMagasin;
                            cmdIns.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = nomPos;
                            cmdIns.Parameters.Add("@m", SqlDbType.NVarChar, 120).Value = machine;
                            cmdIns.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmdUp = new SqlCommand(@"
UPDATE dbo.PostePOS
SET IdEntreprise=@e, IdMagasin=@mag, NomPOS=@nom
WHERE MachineName=@m;", con))
                        {
                            cmdUp.Parameters.Add("@e", SqlDbType.Int).Value = idEntreprise;
                            cmdUp.Parameters.Add("@mag", SqlDbType.Int).Value = idMagasin;
                            cmdUp.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = nomPos;
                            cmdUp.Parameters.Add("@m", SqlDbType.NVarChar, 120).Value = machine;
                            cmdUp.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("✅ Poste POS enregistré pour : " + machine,
                    "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ChargerConfigurationPosteSiExiste();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur enregistrement POS : " + ex.Message,
                    "POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}