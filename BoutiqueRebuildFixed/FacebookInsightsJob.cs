using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class FacebookInsightsJob
    {
        private readonly string _cs;

        public FacebookInsightsJob(string cs)
        {
            _cs = cs;
        }

        public async Task RunAsync()
        {
            // 1) Lire config
            string token = ConfigurationManager.AppSettings["FacebookAccessToken"];
            string graphVersion = ConfigurationManager.AppSettings["MetaGraphVersion"] ?? "v19.0";

            if (string.IsNullOrWhiteSpace(token) || token == "PASTE_YOUR_TOKEN_HERE")
                throw new Exception("Configure d’abord FacebookAccessToken dans App.config.");

            var api = new FacebookApiClient(token, graphVersion);

            // 2) Lire campagnes qui ont FacebookCampaignId
            List<CampagneFb> campagnes = GetCampagnesAvecFacebookId();

            // 3) Pour chaque campagne → insights → upsert stats
            foreach (var c in campagnes)
            {
                // Exemple: stats “mois en cours” (tu peux changer)
                DateTime since = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime until = DateTime.Now.Date;

                FacebookInsights ins = await api.GetCampaignInsightsAsync(c.FacebookCampaignId, since, until);

                UpsertStats(c.IdCampagneMarketing, c.NomCampagne, ins, "USD"); // devise = exemple
            }
        }

        private List<CampagneFb> GetCampagnesAvecFacebookId()
        {
            var list = new List<CampagneFb>();

            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand(@"
SELECT Id, NomCampagne, FacebookCampaignId
FROM CampagnesMarketing
WHERE FacebookCampaignId IS NOT NULL AND LTRIM(RTRIM(FacebookCampaignId)) <> ''", cn))
                {
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            list.Add(new CampagneFb
                            {
                                IdCampagneMarketing = rd.GetInt32(0),
                                NomCampagne = rd.IsDBNull(1) ? "" : rd.GetString(1),
                                FacebookCampaignId = rd.IsDBNull(2) ? "" : rd.GetString(2)
                            });
                        }
                    }
                }
            }

            return list;
        }

        private void UpsertStats(int campagneId, string nomCampagne, FacebookInsights ins, string devise)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();

                string sql = @"
IF EXISTS (SELECT 1 FROM StatistiquesPublicites WHERE CampagneId = @CampagneId)
BEGIN
    UPDATE StatistiquesPublicites SET
        DateVente      = GETDATE(),
        NomCampagne    = @NomCampagne,
        Vues           = @Vues,
        Messages       = @Messages,
        Commentaires   = @Commentaires,
        Statut         = @Statut,
        Spectateurs    = @Spectateurs,
        BudgetQuotidien= @BudgetQuotidien,
        NombreVentes   = @NombreVentes,
        MontantVendus  = @MontantVendus,
        Devise         = @Devise
    WHERE CampagneId = @CampagneId
END
ELSE
BEGIN
    INSERT INTO StatistiquesPublicites
        (CampagneId, DateVente, NomCampagne, Vues, Messages, Commentaires, Statut,
         Spectateurs, BudgetQuotidien, NombreVentes, MontantVendus, Devise)
    VALUES
        (@CampagneId, GETDATE(), @NomCampagne, @Vues, @Messages, @Commentaires, @Statut,
         @Spectateurs, @BudgetQuotidien, @NombreVentes, @MontantVendus, @Devise)
END";

                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    // Ici on map Facebook => champs DB (adaptation simple)
                    cmd.Parameters.Add("@CampagneId", SqlDbType.Int).Value = campagneId;
                    cmd.Parameters.Add("@NomCampagne", SqlDbType.NVarChar, 150).Value = nomCampagne ?? "";

                    // Exemple :
                    // - Vues = impressions
                    // - Spectateurs = reach
                    // - BudgetQuotidien = spend / nb jours (simple)
                    cmd.Parameters.Add("@Vues", SqlDbType.Int).Value = ins.Impressions;
                    cmd.Parameters.Add("@Messages", SqlDbType.Int).Value = 0;         // Facebook ne donne pas "messages" direct sans autre endpoint
                    cmd.Parameters.Add("@Commentaires", SqlDbType.NVarChar).Value = ""; // idem
                    cmd.Parameters.Add("@Statut", SqlDbType.NVarChar, 30).Value = "Auto";

                    cmd.Parameters.Add("@Spectateurs", SqlDbType.Int).Value = ins.Reach;

                    decimal budgetQuot = 0m;
                    int day = DateTime.Now.Day;
                    if (day > 0) budgetQuot = ins.Spend / day;

                    cmd.Parameters.Add("@BudgetQuotidien", SqlDbType.Decimal).Value = budgetQuot;

                    cmd.Parameters.Add("@NombreVentes", SqlDbType.Int).Value = 0;       // à remplir si tu calcules ventes depuis ton POS
                    cmd.Parameters.Add("@MontantVendus", SqlDbType.Decimal).Value = ins.Spend; // ici on met spend comme exemple
                    cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = devise ?? "USD";

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private class CampagneFb
        {
            public int IdCampagneMarketing { get; set; }
            public string NomCampagne { get; set; }
            public string FacebookCampaignId { get; set; }
        }
    }
}