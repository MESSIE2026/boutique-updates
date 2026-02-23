using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class ClientStatsService
    {
        private readonly string _cs;

        public ClientStatsService(string connectionString)
        {
            _cs = connectionString;
        }

        public void Recompute(int idClient, SqlConnection cn, SqlTransaction tx)
        {
            decimal totalAchats = 0m;
            int nbTickets = 0;
            DateTime? dernierAchat = null;

            // ✅ 1) Recalcul stats depuis Vente (exclut annulations + règlements crédits)
            using (SqlCommand cmd = new SqlCommand(@"
SELECT 
    ISNULL(SUM(ISNULL(MontantTotal,0)),0) AS TotalAchats,
    COUNT(*) AS NbTickets,
    MAX(DateVente) AS DernierAchat
FROM dbo.Vente
WHERE ID_Client=@id
  AND UPPER(ISNULL(Statut,'')) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
  AND UPPER(ISNULL(Statut,'')) NOT IN ('REGLEMENT_CREDIT','REGLEMENT CREDIT','PAIEMENT_CREDIT','PAIEMENT CREDIT');", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        totalAchats = r.IsDBNull(0) ? 0m : Convert.ToDecimal(r.GetValue(0));
                        nbTickets = r.IsDBNull(1) ? 0 : Convert.ToInt32(r.GetValue(1));
                        if (!r.IsDBNull(2)) dernierAchat = r.GetDateTime(2);
                    }
                }
            }

            // ✅ 2) Segment
            string segment = "NORMAL";
            if (totalAchats >= 5000000m || nbTickets >= 50) segment = "VIP";

            if (dernierAchat.HasValue)
            {
                double days = (DateTime.Now - dernierAchat.Value).TotalDays;
                if (days > 60) segment = "DORMANT";
            }

            // ✅ 3) Update ClientStats
            using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.ClientStats
SET TotalAchats=@t,
    NbTickets=@n,
    DernierAchat=@d,
    Segment=@s,
    DateMaj=GETDATE()
WHERE IdClient=@id;", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;

                var pT = cmd.Parameters.Add("@t", SqlDbType.Decimal);
                pT.Precision = 18;
                pT.Scale = 2;
                pT.Value = totalAchats;

                cmd.Parameters.Add("@n", SqlDbType.Int).Value = nbTickets;
                cmd.Parameters.Add("@d", SqlDbType.DateTime).Value = (object)dernierAchat ?? DBNull.Value;
                cmd.Parameters.Add("@s", SqlDbType.NVarChar, 20).Value = segment;

                cmd.ExecuteNonQuery();
            }
        }
    }
}