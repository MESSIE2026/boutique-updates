using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmPermissions : FormBase
    {
        SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString);

        public FrmPermissions()
        {
            InitializeComponent();
            

            // Écoute les changements globaux
            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        


        private void FrmPermissions_Load(object sender, EventArgs e)
        {

            // Appliquer AU CHARGEMENT
            RafraichirLangue();
            RafraichirTheme();
            if (!RolePeutVoirPermissions(SessionEmploye.Poste))
            {
                MessageBox.Show("Accès refusé.");
                this.Close();
                return;
            }

            InitialiserGrid();
            ChargerRoles();


            if (cboRoles.Items.Count > 0)
            {
                cboRoles.SelectedIndex = 0;
                ChargerModules();
            }
        }

        private void RafraichirLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);

            // ✅ Recharger juste l'affichage des modules selon la langue
            ChargerModules();
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

        private bool RolePeutVoirPermissions(string nomRole)
        {
            // Liste des rôles autorisés à voir ce formulaire
            string[] rolesAutorises = { "Superviseur", "Programmeur", "Gérant", "Directeur Général" };
            return rolesAutorises.Any(r => string.Equals(r, nomRole, StringComparison.OrdinalIgnoreCase));
        }

        void InitialiserGrid()
        {
            dgvPermissions.Columns.Clear();

            dgvPermissions.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "CodeModule",
                HeaderText = "Code",
                Visible = false
            });

            dgvPermissions.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "NomModule",
                HeaderText = "Module",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvPermissions.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                Name = "Autorise",
                HeaderText = "Autorisé",
                Width = 90
            });

            dgvPermissions.AllowUserToAddRows = false;
            dgvPermissions.RowHeadersVisible = false;
        }

        private void OuvrirFormSecurise(string permissionCode, Func<Form> factory)
        {
            // Admin => direct
            if (ConfigSysteme.RolesSecurite.EstAdmin(SessionEmploye.Poste))
            {
                factory().ShowDialog(this);
                return;
            }

            // déjà débloqué => direct
            if (ConfigSysteme.EstDebloque(permissionCode))
            {
                factory().ShowDialog(this);
                return;
            }

            bool ok = ConfigSysteme.DemanderValidationManager(
                owner: this,
                permissionCode: permissionCode,
                typeAction: "OUVERTURE_FORMULAIRE",
                reference: "FrmPermissions",
                details: "Déblocage rapide depuis Permissions",
                idEmployeDemandeur: SessionEmploye.ID_Employe,
                roleDemandeur: SessionEmploye.Poste
            );

            if (!ok)
            {
                MessageBox.Show("Accès refusé.", "Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ConfigSysteme.Debloquer(permissionCode, 10);
            factory().ShowDialog(this);
        }



        void ChargerRoles()
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT IdRole, NomRole FROM Roles ORDER BY NomRole", con))
                {
                    if (con.State != ConnectionState.Open)
                        con.Open();

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement rôles : " + ex.Message);
                return;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

            cboRoles.DataSource = dt;
            cboRoles.DisplayMember = "NomRole";
            cboRoles.ValueMember = "IdRole";

            SelectRoleDansCombo(SessionEmploye.Poste);
        }

        private void SelectRoleDansCombo(string role)
        {
            for (int i = 0; i < cboRoles.Items.Count; i++)
            {
                DataRowView drv = (DataRowView)cboRoles.Items[i];
                if (drv["NomRole"].ToString().Equals(role, StringComparison.OrdinalIgnoreCase))
                {
                    cboRoles.SelectedIndex = i;
                    return;
                }
            }
            if (cboRoles.Items.Count > 0)
                cboRoles.SelectedIndex = 0;
        }

        private void cboRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChargerModules();
        }

        void ChargerModules()
        {
            dgvPermissions.Rows.Clear();
            if (cboRoles.SelectedItem == null) return;

            int idRole = Convert.ToInt32(((DataRowView)cboRoles.SelectedItem)["IdRole"]);
            string nomRole = ((DataRowView)cboRoles.SelectedItem)["NomRole"].ToString();

            bool roleEstAdmin = ConfigSysteme.RolesSecurite.EstAdmin(nomRole);

            using (SqlConnection cn = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                cn.Open();

                DataTable modules = new DataTable();
                new SqlDataAdapter(
                    "SELECT IdModule, CodeModule, NomModule FROM Modules ORDER BY NomModule",
                    cn).Fill(modules);

                DataTable perms = new DataTable();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT IdModule, Autorise FROM RoleModules WHERE IdRole=@r", cn))
                {
                    cmd.Parameters.AddWithValue("@r", idRole);
                    new SqlDataAdapter(cmd).Fill(perms);
                }

                foreach (DataRow m in modules.Rows)
                {
                    int idModule = (int)m["IdModule"];
                    string code = m["CodeModule"].ToString();
                    string label = ConfigSysteme.Traduire(code);

                    bool autorise = roleEstAdmin
                        ? true
                        : perms.Select("IdModule=" + idModule)
                              .Any(r => Convert.ToBoolean(r["Autorise"]));

                    int idx = dgvPermissions.Rows.Add(code, label, autorise);

                    if (roleEstAdmin)
                    {
                        dgvPermissions.Rows[idx].Cells["Autorise"].ReadOnly = true;
                        dgvPermissions.Rows[idx].DefaultCellStyle.BackColor = Color.LightGray;
                    }
                }

                btnSave.Enabled = !roleEstAdmin;
            }
        }

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            int idRole = Convert.ToInt32(cboRoles.SelectedValue);

            using (SqlConnection cn = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                cn.Open();

                foreach (DataGridViewRow row in dgvPermissions.Rows)
                {
                    string code = row.Cells["CodeModule"].Value.ToString();
                    bool autorise = Convert.ToBoolean(row.Cells["Autorise"].Value);

                    int idModule;
                    using (SqlCommand c = new SqlCommand(
                        "SELECT IdModule FROM Modules WHERE CodeModule=@c", cn))
                    {
                        c.Parameters.AddWithValue("@c", code);
                        idModule = (int)c.ExecuteScalar();
                    }

                    using (SqlCommand cmd = new SqlCommand(@"
IF EXISTS (SELECT 1 FROM RoleModules WHERE IdRole=@r AND IdModule=@m)
    UPDATE RoleModules SET Autorise=@a WHERE IdRole=@r AND IdModule=@m
ELSE
    INSERT INTO RoleModules VALUES(@r,@m,@a)
", cn))
                    {
                        cmd.Parameters.AddWithValue("@r", idRole);
                        cmd.Parameters.AddWithValue("@m", idModule);
                        cmd.Parameters.AddWithValue("@a", autorise);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            MessageBox.Show("Permissions enregistrées.");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void cboRoles_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
    }
}
