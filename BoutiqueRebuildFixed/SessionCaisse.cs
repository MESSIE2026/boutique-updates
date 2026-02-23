using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace BoutiqueRebuildFixed
{
    public partial class SessionCaisse : FormBase
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;

        public SessionCaisse()
        {
            InitializeComponent();
            

            // ❌ LoadCaissiers();  --> supprimé
            LoadSessions();

            // ✅ caissier connecté
            CmbCaissier.DataSource = null;
            CmbCaissier.Items.Clear();
            CmbCaissier.Text = SessionEmploye.Nom; // "juste le nom"
            CmbCaissier.Enabled = false;

            // ✅ juste la date du jour (si tu gardes txtDateOuverture, sinon ignore)
            txtDateOuverture.Text = DateTime.Today.ToString("dd/MM/yyyy");

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }
        private void RafraichirLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);
        }

        private void RafraichirTheme()
        {
            ConfigSysteme.AppliquerTheme(this);
        }
        private void MettreAJourTotauxSession(SqlConnection con, SqlTransaction trans, int idSession)
        {
            // ✅ vérifier si colonnes EUR existent
            bool hasEur = false;
            using (var chk = new SqlCommand(@"
SELECT CASE WHEN COL_LENGTH('dbo.SessionsCaisse','TotalEspecesEUR') IS NULL THEN 0 ELSE 1 END;", con, trans))
            {
                hasEur = Convert.ToInt32(chk.ExecuteScalar()) == 1;
            }

            string sql = hasEur ? @"
;WITH P AS
(
    SELECT
        UPPER(LTRIM(RTRIM(ISNULL(p.Devise,'')))) AS Devise,
        UPPER(LTRIM(RTRIM(ISNULL(p.ModePaiement,'')))) AS ModePaiement,
        ISNULL(p.Montant,0) AS Montant
    FROM dbo.PaiementsVente p
    INNER JOIN dbo.Vente v ON v.ID_Vente = p.IdVente
    WHERE v.IdSession = @idSession
),
Agg AS
(
    SELECT
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('ESPECES','CASH','LIQUIDE') THEN Montant ELSE 0 END) AS TotalEspecesCDF,
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('CARTE','CARD') THEN Montant ELSE 0 END) AS TotalCarteCDF,
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('MOBILE MONEY','MOBILEMONEY','MOMO') THEN Montant ELSE 0 END) AS TotalMobileMoneyCDF,

        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('ESPECES','CASH','LIQUIDE') THEN Montant ELSE 0 END) AS TotalEspecesUSD,
        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('CARTE','CARD') THEN Montant ELSE 0 END) AS TotalCarteUSD,
        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('MOBILE MONEY','MOBILEMONEY','MOMO') THEN Montant ELSE 0 END) AS TotalMobileMoneyUSD,

        SUM(CASE WHEN Devise='EUR' AND ModePaiement IN('ESPECES','CASH','LIQUIDE') THEN Montant ELSE 0 END) AS TotalEspecesEUR,
        SUM(CASE WHEN Devise='EUR' AND ModePaiement IN('CARTE','CARD') THEN Montant ELSE 0 END) AS TotalCarteEUR,
        SUM(CASE WHEN Devise='EUR' AND ModePaiement IN('MOBILE MONEY','MOBILEMONEY','MOMO') THEN Montant ELSE 0 END) AS TotalMobileMoneyEUR
    FROM P
)
UPDATE s
SET
    TotalEspecesCDF     = ISNULL(a.TotalEspecesCDF,0),
    TotalCarteCDF       = ISNULL(a.TotalCarteCDF,0),
    TotalMobileMoneyCDF = ISNULL(a.TotalMobileMoneyCDF,0),

    TotalEspecesUSD     = ISNULL(a.TotalEspecesUSD,0),
    TotalCarteUSD       = ISNULL(a.TotalCarteUSD,0),
    TotalMobileMoneyUSD = ISNULL(a.TotalMobileMoneyUSD,0),

    TotalEspecesEUR     = ISNULL(a.TotalEspecesEUR,0),
    TotalCarteEUR       = ISNULL(a.TotalCarteEUR,0),
    TotalMobileMoneyEUR = ISNULL(a.TotalMobileMoneyEUR,0)
FROM dbo.SessionsCaisse s
CROSS JOIN Agg a
WHERE s.IdSession = @idSession;
" : @"
;WITH P AS
(
    SELECT
        UPPER(LTRIM(RTRIM(ISNULL(p.Devise,'')))) AS Devise,
        UPPER(LTRIM(RTRIM(ISNULL(p.ModePaiement,'')))) AS ModePaiement,
        ISNULL(p.Montant,0) AS Montant
    FROM dbo.PaiementsVente p
    INNER JOIN dbo.Vente v ON v.ID_Vente = p.IdVente
    WHERE v.IdSession = @idSession
),
Agg AS
(
    SELECT
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('ESPECES','CASH','LIQUIDE') THEN Montant ELSE 0 END) AS TotalEspecesCDF,
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('CARTE','CARD') THEN Montant ELSE 0 END) AS TotalCarteCDF,
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('MOBILE MONEY','MOBILEMONEY','MOMO') THEN Montant ELSE 0 END) AS TotalMobileMoneyCDF,

        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('ESPECES','CASH','LIQUIDE') THEN Montant ELSE 0 END) AS TotalEspecesUSD,
        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('CARTE','CARD') THEN Montant ELSE 0 END) AS TotalCarteUSD,
        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('MOBILE MONEY','MOBILEMONEY','MOMO') THEN Montant ELSE 0 END) AS TotalMobileMoneyUSD
    FROM P
)
UPDATE s
SET
    TotalEspecesCDF     = ISNULL(a.TotalEspecesCDF,0),
    TotalCarteCDF       = ISNULL(a.TotalCarteCDF,0),
    TotalMobileMoneyCDF = ISNULL(a.TotalMobileMoneyCDF,0),

    TotalEspecesUSD     = ISNULL(a.TotalEspecesUSD,0),
    TotalCarteUSD       = ISNULL(a.TotalCarteUSD,0),
    TotalMobileMoneyUSD = ISNULL(a.TotalMobileMoneyUSD,0)
FROM dbo.SessionsCaisse s
CROSS JOIN Agg a
WHERE s.IdSession = @idSession;
";

            using (var cmd = new SqlCommand(sql, con, trans))
            {
                cmd.Parameters.Add("@idSession", SqlDbType.Int).Value = idSession;
                cmd.ExecuteNonQuery();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 🔥 OBLIGATOIRE : éviter fuite mémoire
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;

            base.OnFormClosed(e);
        }
        private void LoadCaissiers()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT ID_Employe, Nom + ' ' + Prenom AS NomComplet FROM Employes ORDER BY Nom, Prenom";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    CmbCaissier.DataSource = dt;
                    CmbCaissier.DisplayMember = "NomComplet";
                    CmbCaissier.ValueMember = "ID_Employe";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement caissiers : " + ex.Message);
            }
        }

        private void LoadSessions()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"
SELECT IdSession, IdCaissier, DateOuverture, DateFermeture,
       TotalEspecesCDF, TotalCarteCDF, TotalMobileMoneyCDF,
       TotalEspecesUSD, TotalCarteUSD, TotalMobileMoneyUSD,
       TotalRemboursements, CashReel, Etat
FROM SessionsCaisse
WHERE IdPoste = @idPoste
ORDER BY DateOuverture DESC;";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                    adapter.SelectCommand.Parameters.Add("@idPoste", SqlDbType.Int).Value = AppContext.IdPoste;
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    DataGridViewSessions.DataSource = dt;
                    FormatDataGridView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement sessions : " + ex.Message);
            }
        }

        private void FormatDataGridView()
        {
            if (DataGridViewSessions.Columns.Contains("IdSession"))
                DataGridViewSessions.Columns["IdSession"].Visible = false;

            if (DataGridViewSessions.Columns.Contains("IdCaissier"))
                DataGridViewSessions.Columns["IdCaissier"].Visible = false;

            DataGridViewSessions.Columns["DateOuverture"].HeaderText = "Date Ouverture";
            DataGridViewSessions.Columns["DateFermeture"].HeaderText = "Date Fermeture";

            DataGridViewSessions.Columns["TotalEspecesCDF"].HeaderText = "Espèces CDF";
            DataGridViewSessions.Columns["TotalCarteCDF"].HeaderText = "Carte CDF";
            DataGridViewSessions.Columns["TotalMobileMoneyCDF"].HeaderText = "Mobile Money CDF";

            DataGridViewSessions.Columns["TotalEspecesUSD"].HeaderText = "Espèces USD";
            DataGridViewSessions.Columns["TotalCarteUSD"].HeaderText = "Carte USD";
            DataGridViewSessions.Columns["TotalMobileMoneyUSD"].HeaderText = "Mobile Money USD";

            DataGridViewSessions.Columns["TotalRemboursements"].HeaderText = "Remboursements";
            DataGridViewSessions.Columns["CashReel"].HeaderText = "Cash Réel";
            DataGridViewSessions.Columns["Etat"].HeaderText = "État";
        }

        private void SessionCaisse_Load(object sender, EventArgs e)
        {

        }

        private bool SessionOuvertePourCaissier(int idCaissier, out int idSession)
        {
            idSession = 0;

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 IdSession
FROM dbo.SessionsCaisse
WHERE IdCaissier = @id
  AND IdPoste = @idPoste
  AND Etat = 'OUVERTE'
ORDER BY DateOuverture DESC;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCaissier;
                    cmd.Parameters.Add("@idPoste", SqlDbType.Int).Value = AppContext.IdPoste;

                    object r = cmd.ExecuteScalar();
                    if (r == null || r == DBNull.Value) return false;

                    idSession = Convert.ToInt32(r);
                    return idSession > 0;
                }
            }
        }


        private void BtnOuvrirSession_Click(object sender, EventArgs e)
        {
            if (SessionEmploye.ID_Employe <= 0)
            {
                MessageBox.Show("Aucun employé connecté.");
                return;
            }

            int idCaissier = SessionEmploye.ID_Employe;

            if (SessionOuvertePourCaissier(idCaissier, out int sid))
            {
                ConfigSysteme.SessionCaisseId = sid;
                ConfigSysteme.CaissierSessionId = idCaissier;
                MessageBox.Show("Session déjà ouverte (IdSession=" + sid + ")");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string checkQuery = @"
SELECT COUNT(*) 
FROM SessionsCaisse 
WHERE IdCaissier = @idCaissier 
  AND IdPoste = @idPoste
  AND Etat = 'OUVERTE'";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@idCaissier", idCaissier);
                        checkCmd.Parameters.AddWithValue("@idPoste", AppContext.IdPoste);
                        int openSessions = (int)checkCmd.ExecuteScalar();
                        if (openSessions > 0)
                        {
                            MessageBox.Show("Une session est déjà ouverte pour ce caissier.");
                            return;
                        }
                    }

                    string insertQuery = @"
INSERT INTO SessionsCaisse (IdCaissier, DateOuverture, Etat, IdEntreprise, IdMagasin, IdPoste)
OUTPUT INSERTED.IdSession
VALUES (@idCaissier, @dateOuverture, 'OUVERTE', @idEntreprise, @idMagasin, @idPoste)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@idCaissier", idCaissier);
                        cmd.Parameters.AddWithValue("@dateOuverture", DateTime.Now);
                        cmd.Parameters.Add("@idEntreprise", SqlDbType.Int).Value = AppContext.IdEntreprise;
                        cmd.Parameters.Add("@idMagasin", SqlDbType.Int).Value = AppContext.IdMagasin;
                        cmd.Parameters.Add("@idPoste", SqlDbType.Int).Value = AppContext.IdPoste;

                        int idSession = Convert.ToInt32(cmd.ExecuteScalar());

                        ConfigSysteme.SessionCaisseId = idSession;
                        ConfigSysteme.CaissierSessionId = idCaissier;

                        MessageBox.Show($"Session ouverte avec succès. (Session #{idSession})");

                        LoadSessions();

                        // ✅ juste la date du jour
                        txtDateOuverture.Text = DateTime.Today.ToString("dd/MM/yyyy");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ouverture de la session : " + ex.Message);
            }
        }

        private void BtnCloreSession_Click(object sender, EventArgs e)
        {
            if (DataGridViewSessions.CurrentRow == null)
            {
                MessageBox.Show("Veuillez sélectionner une session à fermer.");
                return;
            }

            int idSession = Convert.ToInt32(DataGridViewSessions.CurrentRow.Cells["IdSession"].Value);

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (var trans = con.BeginTransaction())
                    {
                        try
                        {
                            // ✅ recalcul totaux avant fermeture (sécurité)
                            MettreAJourTotauxSession(con, trans, idSession);

                            using (var cmd = new SqlCommand(@"
UPDATE SessionsCaisse
SET DateFermeture = @dateFermeture,
    Etat = 'FERMEE'
WHERE IdSession = @idSession AND Etat = 'OUVERTE';
", con, trans))
                            {
                                cmd.Parameters.AddWithValue("@dateFermeture", DateTime.Now);
                                cmd.Parameters.AddWithValue("@idSession", idSession);

                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected <= 0)
                                    throw new Exception("La session est déjà fermée ou n'existe pas.");
                            }

                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }

                    MessageBox.Show("Session fermée avec succès.");
                    ConfigSysteme.AjouterAuditLog("SessionsCaisse", $"Session ID={idSession} fermée à {DateTime.Now}", "Succès");

                    if (ConfigSysteme.SessionCaisseId == idSession)
                    {
                        ConfigSysteme.SessionCaisseId = 0;
                        ConfigSysteme.CaissierSessionId = 0;
                    }

                    LoadSessions();
                    txtDateOuverture.Clear();
                }
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("SessionsCaisse", $"Erreur fermeture session ID={idSession} : {ex.Message}", "Échec");
                MessageBox.Show("Erreur lors de la fermeture de la session : " + ex.Message);
            }
        }
        private void DataGridViewSessions_SelectionChanged(object sender, EventArgs e)
        {
            if (DataGridViewSessions.CurrentRow == null) return;

            var row = DataGridViewSessions.CurrentRow;

            txtDateOuverture.Text = row.Cells["DateOuverture"].Value == null
                ? ""
                : Convert.ToDateTime(row.Cells["DateOuverture"].Value).ToString("g");

            decimal especesCDF = Convert.ToDecimal(row.Cells["TotalEspecesCDF"].Value ?? 0m);
            decimal carteCDF = Convert.ToDecimal(row.Cells["TotalCarteCDF"].Value ?? 0m);
            decimal momoCDF = Convert.ToDecimal(row.Cells["TotalMobileMoneyCDF"].Value ?? 0m);

            decimal especesUSD = Convert.ToDecimal(row.Cells["TotalEspecesUSD"].Value ?? 0m);
            decimal carteUSD = Convert.ToDecimal(row.Cells["TotalCarteUSD"].Value ?? 0m);
            decimal momoUSD = Convert.ToDecimal(row.Cells["TotalMobileMoneyUSD"].Value ?? 0m);

            lblVenteEspece.Text = $"{especesCDF:N2} CDF | {especesUSD:N2} USD";
            lblVenteCarte.Text = $"{carteCDF:N2} CDF | {carteUSD:N2} USD";

            // ⚠️ Ajoute ce label dans le designer
            lblMobileMoney.Text = $"{momoCDF:N2} CDF | {momoUSD:N2} USD";

            lblRemboursements.Text = $"{Convert.ToDecimal(row.Cells["TotalRemboursements"].Value ?? 0m):N2}";
            lblCashReel.Text = $"{Convert.ToDecimal(row.Cells["CashReel"].Value ?? 0m):N2}";
        }

        private void BtnImprimerZ_Click(object sender, EventArgs e)
        {
            {
                MessageBox.Show("Fonction d'impression Z non implémentée.");
            }
        }

        private void BtnExporterPDF_Click(object sender, EventArgs e)
        {
            if (DataGridViewSessions.CurrentRow == null)
            {
                MessageBox.Show("Veuillez sélectionner une session à exporter.");
                return;
            }

            int idSession = Convert.ToInt32(DataGridViewSessions.CurrentRow.Cells["IdSession"].Value);

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
SELECT 
    IdSession,
    IdCaissier,
    DateOuverture,
    DateFermeture,
    ISNULL(TotalEspecesCDF, 0)       AS TotalEspecesCDF,
    ISNULL(TotalCarteCDF, 0)         AS TotalCarteCDF,
    ISNULL(TotalMobileMoneyCDF, 0)   AS TotalMobileMoneyCDF,
    ISNULL(TotalEspecesUSD, 0)       AS TotalEspecesUSD,
    ISNULL(TotalCarteUSD, 0)         AS TotalCarteUSD,
    ISNULL(TotalMobileMoneyUSD, 0)   AS TotalMobileMoneyUSD,
    ISNULL(TotalRemboursements, 0)   AS TotalRemboursements,
    ISNULL(CashReel, 0)              AS CashReel,
    ISNULL(Etat, '')                 AS Etat
FROM dbo.SessionsCaisse
WHERE IdSession = @idSession;";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@idSession", idSession);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Session non trouvée.");
                                return;
                            }

                            DateTime dateOuverture = reader.GetDateTime(reader.GetOrdinal("DateOuverture"));
                            DateTime? dateFermeture =
                                reader.IsDBNull(reader.GetOrdinal("DateFermeture"))
                                    ? (DateTime?)null
                                    : reader.GetDateTime(reader.GetOrdinal("DateFermeture"));

                            decimal totalEspecesCDF = reader.GetDecimal(reader.GetOrdinal("TotalEspecesCDF"));
                            decimal totalCarteCDF = reader.GetDecimal(reader.GetOrdinal("TotalCarteCDF"));
                            decimal totalMomoCDF = reader.GetDecimal(reader.GetOrdinal("TotalMobileMoneyCDF"));

                            decimal totalEspecesUSD = reader.GetDecimal(reader.GetOrdinal("TotalEspecesUSD"));
                            decimal totalCarteUSD = reader.GetDecimal(reader.GetOrdinal("TotalCarteUSD"));
                            decimal totalMomoUSD = reader.GetDecimal(reader.GetOrdinal("TotalMobileMoneyUSD"));

                            decimal totalRemboursements = reader.GetDecimal(reader.GetOrdinal("TotalRemboursements"));
                            decimal cashReel = reader.GetDecimal(reader.GetOrdinal("CashReel"));
                            string etat = reader.GetString(reader.GetOrdinal("Etat"));

                            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                            {
                                saveFileDialog.Title = "Enregistrer le rapport PDF";
                                saveFileDialog.Filter = "Fichiers PDF (*.pdf)|*.pdf";
                                saveFileDialog.FileName = $"RapportZ_Session_{idSession}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                                {
                                    MessageBox.Show("Export annulé.");
                                    return;
                                }

                                PdfDocument document = new PdfDocument();
                                document.Info.Title = $"Rapport Z - Session {idSession}";

                                PdfPage page = document.AddPage();
                                XGraphics gfx = XGraphics.FromPdfPage(page);

                                XFont fontTitle = new XFont("Arial", 14);
                                XFont font = new XFont("Arial", 12);

                                int yPoint = 40;
                                int lineHeight = 25;
                                int marginLeft = 40;

                                gfx.DrawString("Rapport Z", fontTitle, XBrushes.Black,
                                    new XRect(0, yPoint, page.Width.Point, lineHeight),
                                    XStringFormats.TopCenter);

                                yPoint += 60;

                                void DrawLine(string label, string value)
                                {
                                    gfx.DrawString(label, font, XBrushes.Black,
                                        new XRect(marginLeft, yPoint, 200, lineHeight),
                                        XStringFormats.TopLeft);

                                    gfx.DrawString(value ?? "", font, XBrushes.Black,
                                        new XRect(marginLeft + 210, yPoint, page.Width.Point - (marginLeft + 210), lineHeight),
                                        XStringFormats.TopLeft);

                                    yPoint += lineHeight;
                                }

                                DrawLine("Session :", idSession.ToString());
                                DrawLine("Date Ouverture :", dateOuverture.ToString("g"));
                                DrawLine("Date Fermeture :", dateFermeture?.ToString("g") ?? "N/A");
                                DrawLine("État :", etat);

                                yPoint += 10;

                                DrawLine("Espèces CDF :", totalEspecesCDF.ToString("N2") + " CDF");
                                DrawLine("Carte CDF :", totalCarteCDF.ToString("N2") + " CDF");
                                DrawLine("Mobile Money CDF :", totalMomoCDF.ToString("N2") + " CDF");

                                yPoint += 10;

                                DrawLine("Espèces USD :", totalEspecesUSD.ToString("N2") + " USD");
                                DrawLine("Carte USD :", totalCarteUSD.ToString("N2") + " USD");
                                DrawLine("Mobile Money USD :", totalMomoUSD.ToString("N2") + " USD");

                                yPoint += 10;

                                DrawLine("Remboursements :", totalRemboursements.ToString("N2"));
                                DrawLine("Cash Réel :", cashReel.ToString("N2"));

                                document.Save(saveFileDialog.FileName);
                                Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });

                                MessageBox.Show("Rapport PDF créé avec succès !");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la génération du PDF : " + ex.Message);
            }
        }
    }
}

