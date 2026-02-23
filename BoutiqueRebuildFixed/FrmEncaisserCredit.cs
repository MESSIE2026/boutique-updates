using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public partial class FrmEncaisserCredit : Form
    {
        private readonly int _idCredit;
        private readonly decimal _reste;
        private readonly CreditService _svc;

        private TextBox txtMontant;
        private ComboBox cboMode;
        private TextBox txtNote;
        private Button btnOk, btnCancel;
        private Label lblResteDb;      // reste réel DB
        private Label lblResteApres;   // reste après saisie
        private string _deviseCredit = "CDF";

        public FrmEncaisserCredit(int idCredit, string clientNom, decimal reste)
        {
            _idCredit = idCredit;
            _reste = reste;
            _svc = new CreditService(ConfigSysteme.ConnectionString);

            Text = $"Encaisser dette - {clientNom}";
            Width = 520;
            Height = 260;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10f);

            BuildUI();

            Shown += async (s, e) =>
            {
                await RefreshResteDepuisDbAsync();
                RecalcResteApres();
                txtMontant.Focus();
                txtMontant.SelectAll();
            };
        }

        private void BuildUI()
        {
            var fr = CultureInfo.GetCultureInfo("fr-FR");

            var lblR = new Label { Text = "Reste (DB) :", Left = 20, Top = 20, AutoSize = true };
            lblResteDb = new Label
            {
                Text = _reste.ToString("N2", fr),
                Left = 120,
                Top = 20,
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            var lblRA = new Label { Text = "Reste après :", Left = 260, Top = 20, AutoSize = true };
            lblResteApres = new Label
            {
                Text = _reste.ToString("N2", fr),
                Left = 360,
                Top = 20,
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            var lblM = new Label { Text = "Montant :", Left = 20, Top = 55, AutoSize = true };
            txtMontant = new TextBox { Left = 120, Top = 52, Width = 120, Text = "0,00" };

            var lblMode = new Label { Text = "Mode :", Left = 260, Top = 55, AutoSize = true };
            cboMode = new ComboBox { Left = 320, Top = 52, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboMode.Items.AddRange(new object[] { "ESPECES", "MOMO", "AIRTEL", "CARTE", "VIREMENT" });
            cboMode.SelectedIndex = 0;

            var lblN = new Label { Text = "Note :", Left = 20, Top = 90, AutoSize = true };
            txtNote = new TextBox { Left = 120, Top = 87, Width = 350, Height = 60, Multiline = true };

            btnOk = new Button { Text = "Encaisser", Left = 290, Top = 160, Width = 100, Height = 32 };
            btnCancel = new Button { Text = "Annuler", Left = 400, Top = 160, Width = 80, Height = 32 };

            btnOk.Click += async (s, e) => await EncaisserAsync();
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            // ✅ recalcul live du reste après
            txtMontant.TextChanged += (s, e) => RecalcResteApres();

            Controls.AddRange(new Control[] { lblR, lblResteDb, lblRA, lblResteApres, lblM, txtMontant, lblMode, cboMode, lblN, txtNote, btnOk, btnCancel });

        }

        private void RecalcResteApres()
        {
            var fr = CultureInfo.GetCultureInfo("fr-FR");

            if (!decimal.TryParse(txtMontant.Text, NumberStyles.Any, fr, out var m)) m = 0m;
            m = Math.Max(0m, Math.Round(m, 2));

            // ✅ reste réel = lblResteDb
            decimal resteDb = 0m;
            decimal.TryParse(lblResteDb.Text, NumberStyles.Any, fr, out resteDb);

            if (m > resteDb) m = resteDb; // clamp

            var resteApres = Math.Round(resteDb - m, 2);
            if (resteApres < 0m) resteApres = 0m;

            lblResteApres.Text = resteApres.ToString("N2", fr);
        }

        private async Task RefreshResteDepuisDbAsync()
        {
            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                await con.OpenAsync();

                using (var cmd = new SqlCommand(@"
SELECT TOP 1 
    ISNULL(cv.Reste,0) AS Reste,
    ISNULL(v.Devise,'CDF') AS Devise
FROM dbo.CreditVente cv
LEFT JOIN dbo.Vente v ON v.ID_Vente = cv.IdVente
WHERE cv.IdCredit=@id;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = _idCredit;

                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        if (await rd.ReadAsync())
                        {
                            var resteDb = rd["Reste"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["Reste"]);
                            var dev = rd["Devise"] == DBNull.Value ? "CDF" : rd["Devise"].ToString();

                            resteDb = Math.Round(resteDb, 2);

                            _deviseCredit = (dev ?? "CDF").Trim().ToUpperInvariant();
                            if (_deviseCredit == "FC") _deviseCredit = "CDF";
                            if (string.IsNullOrWhiteSpace(_deviseCredit)) _deviseCredit = "CDF";

                            var fr = CultureInfo.GetCultureInfo("fr-FR");
                            lblResteDb.Text = resteDb.ToString("N2", fr);

                            // (optionnel) afficher devise dans le titre
                            Text = $"{Text.Split('-')[0].Trim()} - {_deviseCredit}";
                        }
                    }
                }
            }
        }


        // =========================
        // 1) Trouver ID_Client à partir du crédit
        // =========================
        private int GetIdClientByCredit(int idCredit, SqlConnection con, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 IdClient
FROM dbo.CreditVente
WHERE IdCredit = @id;", con, trans))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCredit;
                object o = cmd.ExecuteScalar();
                if (o == null || o == DBNull.Value) return 0;
                return Convert.ToInt32(o);
            }
        }


        // =========================
        // 2) Créer une vente REGLEMENT_CREDIT => ID_Vente valide
        // =========================
        private int CreerVenteReglementCredit(SqlConnection con, SqlTransaction trans, int idClient, decimal montant, string modePaiement)
        {
            string devise = GetDeviseByCredit(_idCredit, con, trans);

            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Vente
(
    DateVente, ID_Client, IDEmploye, ModePaiement, MontantTotal,
    NomCaissier, Devise, IdSession, CodeFacture, Statut
)
VALUES
(
    GETDATE(), @ID_Client, @IDEmploye, @ModePaiement, @MontantTotal,
    @NomCaissier, @Devise, @IdSession, @CodeFacture, @Statut
);
SELECT CAST(SCOPE_IDENTITY() AS INT);", con, trans))
            {
                int idEmploye = SessionEmploye.ID_Employe;
                string caissier = ((SessionEmploye.Nom ?? "") + " " + (SessionEmploye.Prenom ?? "")).Trim();
                if (string.IsNullOrWhiteSpace(caissier)) caissier = "CAISSIER";

                int idSession = ConfigSysteme.SessionCaisseId;

                string codeFacture = $"REGCRED-{_idCredit}-{DateTime.Now:yyyyMMddHHmmss}";

                cmd.Parameters.Add("@ID_Client", SqlDbType.Int).Value = idClient;
                cmd.Parameters.Add("@IDEmploye", SqlDbType.Int).Value = idEmploye;
                cmd.Parameters.Add("@ModePaiement", SqlDbType.NVarChar, 30).Value = (modePaiement ?? "CASH").Trim().ToUpperInvariant();

                var pM = cmd.Parameters.Add("@MontantTotal", SqlDbType.Decimal);
                pM.Precision = 18; pM.Scale = 2; pM.Value = montant;

                cmd.Parameters.Add("@NomCaissier", SqlDbType.NVarChar, 120).Value = caissier;
                cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = devise;
                cmd.Parameters.Add("@IdSession", SqlDbType.Int).Value = idSession;
                cmd.Parameters.Add("@CodeFacture", SqlDbType.NVarChar, 50).Value = codeFacture;
                cmd.Parameters.Add("@Statut", SqlDbType.NVarChar, 50).Value = "REGLEMENT_CREDIT";

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private string GetDeviseByCredit(int idCredit, SqlConnection con, SqlTransaction trans)
        {
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 ISNULL(v.Devise,'CDF')
FROM dbo.CreditVente cv
LEFT JOIN dbo.Vente v ON v.ID_Vente = cv.IdVente
WHERE cv.IdCredit=@id;", con, trans))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCredit;
                var o = cmd.ExecuteScalar();
                var d = (o == null || o == DBNull.Value) ? "CDF" : o.ToString();
                d = (d ?? "").Trim().ToUpperInvariant();
                if (d == "FC") d = "CDF";
                return string.IsNullOrWhiteSpace(d) ? "CDF" : d;
            }
        }


        // =========================
        // 3) Encaisser (avec ID_Vente)
        // =========================
        private async Task EncaisserAsync()
        {
            // ✅ bloquer double-clic
            btnOk.Enabled = false;
            btnCancel.Enabled = false;

            try
            {
                // ✅ recharger le reste DB + devise sans bloquer l'UI
                await RefreshResteDepuisDbAsync();
                RecalcResteApres();

                var fr = CultureInfo.GetCultureInfo("fr-FR");

                if (!decimal.TryParse(txtMontant.Text, NumberStyles.Any, fr, out decimal m))
                {
                    MessageBox.Show("Montant invalide.");
                    return;
                }

                m = Math.Round(m, 2);
                if (m <= 0m)
                {
                    MessageBox.Show("Montant doit être > 0.");
                    return;
                }

                // ✅ reste DB depuis lbl
                decimal.TryParse(lblResteDb.Text, NumberStyles.Any, fr, out var resteDb);
                resteDb = Math.Round(resteDb, 2);

                if (m > resteDb)
                {
                    MessageBox.Show("Montant > reste (DB).");
                    return;
                }

                string mode = (cboMode.SelectedItem?.ToString() ?? "ESPECES").Trim().ToUpperInvariant();
                if (mode == "CASH") mode = "ESPECES"; // ✅ normalisation

                using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    await con.OpenAsync();
                    using (var trans = con.BeginTransaction())
                    {
                        try
                        {
                            // 1) ID_Client
                            int idClient = GetIdClientByCredit(_idCredit, con, trans);
                            if (idClient <= 0)
                                throw new Exception("ID_Client introuvable pour ce crédit.");

                            // 2) Vente règlement crédit
                            int idVente = CreerVenteReglementCredit(con, trans, idClient, m, mode);
                            if (idVente <= 0)
                                throw new Exception("Impossible de créer la vente règlement crédit.");

                            // 3) Paiement crédit + caisse
                            _svc.EncaisserPaiementCredit(
                                idCredit: _idCredit,
                                idVente: idVente,
                                montant: m,
                                modePaiement: mode,
                                note: (txtNote.Text ?? "").Trim(),
                                con: con,
                                trans: trans
                            );

                            trans.Commit();
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                        catch
                        {
                            try { trans.Rollback(); } catch { }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur encaissement : " + ex.Message);
            }
            finally
            {
                // ✅ réactiver si on n’a pas fermé
                if (!IsDisposed)
                {
                    btnOk.Enabled = true;
                    btnCancel.Enabled = true;
                }
            }
        }
    }
}