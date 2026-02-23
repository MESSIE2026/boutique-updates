using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{

    public class CompteClientService
    {
        private readonly string _cs;

        public CompteClientService(string connectionString)
        {
            _cs = connectionString;
        }

        // ✅ Crée les lignes CompteClient / LoyaltyCompte / ClientStats si absentes
        public void EnsureClientModules(int idClient, SqlConnection cn, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(@"
IF NOT EXISTS(SELECT 1 FROM CompteClient WHERE IdClient=@id)
    INSERT INTO CompteClient(IdClient,Plafond,Solde,Statut,DateMaj)
    VALUES(@id,0,0,'ACTIF',GETDATE());

IF NOT EXISTS(SELECT 1 FROM LoyaltyCompte WHERE IdClient=@id)
    INSERT INTO LoyaltyCompte(IdClient,Points,CashbackSolde,Statut,DateMaj)
    VALUES(@id,0,0,'BRONZE',GETDATE());

IF NOT EXISTS(SELECT 1 FROM ClientStats WHERE IdClient=@id)
    INSERT INTO ClientStats(IdClient,TotalAchats,NbTickets,Segment,DateMaj)
    VALUES(@id,0,0,'NORMAL',GETDATE());
", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                cmd.ExecuteNonQuery();
            }
        }

        public void EnsureClientModulesStandalone(int idClient)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction())
                {
                    try
                    {
                        EnsureClientModules(idClient, cn, tx);
                        tx.Commit();
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        public void UpdatePlafond(int idClient, decimal plafond, SqlConnection cn, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(@"
UPDATE CompteClient
SET Plafond=@p, DateMaj=GETDATE()
WHERE IdClient=@id;", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                cmd.Parameters.Add("@p", SqlDbType.Decimal).Value = plafond;
                cmd.ExecuteNonQuery();
            }
        }

        public void AddToSolde(int idClient, decimal delta, SqlConnection cn, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(@"
UPDATE CompteClient
SET Solde = ISNULL(Solde,0) + @d, DateMaj=GETDATE()
WHERE IdClient=@id;", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                cmd.Parameters.Add("@d", SqlDbType.Decimal).Value = delta;
                cmd.ExecuteNonQuery();
            }
        }

        public (decimal plafond, decimal solde, string statut) GetCompte(int idClient)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
SELECT Plafond, Solde, Statut
FROM CompteClient WHERE IdClient=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return (0m, 0m, "INCONNU");

                        decimal plafond = r.IsDBNull(0) ? 0m : Convert.ToDecimal(r.GetValue(0));
                        decimal solde = r.IsDBNull(1) ? 0m : Convert.ToDecimal(r.GetValue(1));
                        string statut = r.IsDBNull(2) ? "ACTIF" : r.GetString(2);

                        return (plafond, solde, statut);
                    }
                }
            }
        }
    }
}