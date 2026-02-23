using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FormAuditLog : FormBase
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;

        private int pageCourante = 1;
        private const int pageSize = 50;
        private int totalRecords = 0;

        protected override string NomModule
        {
            get { return "AUDIT_VIEWER"; }
        }

        public FormAuditLog()
        {
            InitializeComponent();
            this.Load += new EventHandler(FormAuditLog_Load);
        }

        private void FormAuditLog_Load(object sender, EventArgs e)
        {
            ChargerActions();

            cmbAction.SelectedIndexChanged += (s, ev) =>
            {
                pageCourante = 1;
                ChargerAuditLog();
            };

            ChargerAuditLog();
        }

        private void ChargerActions()
        {
            cmbAction.Items.Clear();

            cmbAction.Items.Add("ALL");
            cmbAction.Items.Add("VIEW");
            cmbAction.Items.Add("ADD");
            cmbAction.Items.Add("UPDATE");
            cmbAction.Items.Add("DELETE");
            cmbAction.Items.Add("EXPORT");
            cmbAction.Items.Add("ERROR");

            cmbAction.SelectedIndex = 0;
        }

        private string GetActionSelectionnee()
        {
            return cmbAction.SelectedItem?.ToString() ?? "ALL";
        }

        private void ChargerAuditLog()
        {
            string action = GetActionSelectionnee();

            string sql = @"
SELECT DateHeure, Utilisateur, AdresseIP, TypeAction, Description, Resultat
FROM AuditLog
WHERE (@Action = 'ALL' OR TypeAction = @Action)
ORDER BY DateHeure DESC";

            using (var cn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Action", action);

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvAuditLog.DataSource = dt;
                }
            }

            ColorerResultat();
        }

        private void ColorerResultat()
        {
            foreach (DataGridViewRow row in dgvAuditLog.Rows)
            {
                if (row.Cells["Resultat"].Value == null) continue;

                row.Cells["Resultat"].Style.ForeColor =
                    row.Cells["Resultat"].Value.ToString() == "Succès"
                        ? Color.Green
                        : Color.Red;
            }
        }
        private void btnFiltrer_Click(object sender, EventArgs e)
        {
            cmbAction.SelectedIndex = 0;
            pageCourante = 1;
            ChargerAuditLog();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            pageCourante = 1;
            ChargerAuditLog();
        }

        private void btnPrecedent_Click(object sender, EventArgs e)
        {
            if (pageCourante > 1)
            {
                pageCourante--;
                ChargerAuditLog();
            }
        }

        private void btnSuivant_Click(object sender, EventArgs e)
        {
            if (pageCourante * pageSize < totalRecords)
            {
                pageCourante++;
                ChargerAuditLog();
            }
        }

private void MettreAJourBoutonsPagination()
        {
            btnPrecedent.Enabled = pageCourante > 1;
            btnSuivant.Enabled = pageCourante * pageSize < totalRecords;
        }
    }
}
