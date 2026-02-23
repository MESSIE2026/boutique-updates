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
    public partial class FrmDetailsBoutique : Form
    {
       
            private readonly string _cs;
            private readonly int _idEntreprise;
            private readonly int _idMagasin;
            private readonly DateTime _debut;
            private readonly DateTime _fin;

            private DataGridView dgv;
            private Label lblTitre;
        private readonly string _nomMagasin;
        private readonly string _adresse;

        public FrmDetailsBoutique(string connectionString, int idEntreprise, int idMagasin,
    DateTime debut, DateTime fin, string nomMagasin, string adresse)
        {
            _cs = connectionString;
            _idEntreprise = idEntreprise;
            _idMagasin = idMagasin;
            _debut = debut;
            _fin = fin;
            _nomMagasin = nomMagasin ?? "";
            _adresse = adresse ?? "";

            BuildUI();
            Load += (s, e) => Charger();
        }

        private void BuildUI()
            {
                Text = "Détail boutique";
                StartPosition = FormStartPosition.CenterParent;
                Width = 1100;
                Height = 650;

                lblTitre = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 36,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 0, 0, 0)
                };

                dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect
                };

                Controls.Add(dgv);
                Controls.Add(lblTitre);
            }

        private void Charger()
        {
            // ===== 1) Infos boutique (Nom + Adresse) =====
            string boutiqueNom = "";
            string boutiqueAdresse = "";

            if (_idMagasin > 0)
            {
                using (var conInfo = new SqlConnection(_cs))
                using (var cmdInfo = new SqlCommand(@"
SELECT TOP 1 Nom, Adresse
FROM dbo.Magasin
WHERE IdMagasin = @m;", conInfo))
                {
                    cmdInfo.Parameters.Add("@m", SqlDbType.Int).Value = _idMagasin;
                    conInfo.Open();

                    using (var rd = cmdInfo.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            boutiqueNom = rd["Nom"]?.ToString() ?? "";
                            boutiqueAdresse = rd["Adresse"]?.ToString() ?? "";
                        }
                    }
                }
            }

            // ===== 2) Titre =====
            if (_idMagasin == 0 && _idEntreprise == 0)
            {
                lblTitre.Text =
                    $"Boutique: NON AFFECTÉE | Période: {_debut:dd/MM/yyyy} → {_fin.AddDays(-1):dd/MM/yyyy}";
            }
            else
            {
                string labelBoutique;

                if (_idMagasin == 0 && _idEntreprise == 0)
                    labelBoutique = "NON AFFECTÉE";
                else if (!string.IsNullOrWhiteSpace(_nomMagasin) && _nomMagasin != "NON AFFECTE")
                    labelBoutique = _nomMagasin;
                else
                    labelBoutique = $"Magasin #{_idMagasin}";

                string labelAdresse = string.IsNullOrWhiteSpace(_adresse) ? "" : $" | Adresse: {_adresse}";

                lblTitre.Text = $"Boutique: {labelBoutique}{labelAdresse} | Entreprise={_idEntreprise} | Période: {_debut:dd/MM/yyyy} → {_fin.AddDays(-1):dd/MM/yyyy}";

                // ===== 3) Détails ventes =====
                using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
SELECT TOP 2000
    v.DateVente AS DateVente,
    v.CodeFacture,
    v.MontantTotal,
    v.Devise,
    v.ModePaiement,
    v.NomCaissier,
    v.Statut
FROM dbo.Vente v
WHERE
(
    -- NON AFFECTE
    (@e = 0 AND @m = 0
        AND (v.IdEntreprise IS NULL OR v.IdEntreprise = 0)
        AND (v.IdMagasin   IS NULL OR v.IdMagasin   = 0))
 OR
    -- BOUTIQUE NORMALE
    (@e > 0 AND @m > 0 AND v.IdEntreprise = @e AND v.IdMagasin = @m)
)
AND v.DateVente >= @d1
AND v.DateVente <  @d2
AND ISNULL(v.Statut,'OK') <> 'ANNULE'
ORDER BY v.DateVente DESC;", con))
            {
                cmd.Parameters.Add("@e", SqlDbType.Int).Value = _idEntreprise;
                cmd.Parameters.Add("@m", SqlDbType.Int).Value = _idMagasin;
                cmd.Parameters.Add("@d1", SqlDbType.DateTime2).Value = _debut;
                cmd.Parameters.Add("@d2", SqlDbType.DateTime2).Value = _fin;

                var dt = new DataTable();
                con.Open();
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                dgv.DataSource = dt;
            }
        }

    }
}
}
