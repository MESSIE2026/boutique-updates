using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class MarketingService
    {
        private readonly string _cs;
        public MarketingService(string cs) { _cs = cs; }

        public void UpsertCampagneEtStats(
            int? campagneId,
            Action<SqlCommand> fillCampagneParams,
            Action<int, SqlConnection, SqlTransaction> upsertStatsOrDeleteIfNeeded)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();

                using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        int id;

                        if (campagneId.HasValue && campagneId.Value > 0)
                        {
                            string sqlUpdate = @"
UPDATE CampagnesMarketing SET
    NomCampagne=@NomCampagne,
    TypeCampagne=@TypeCampagne,
    DateDebut=@DateDebut,
    DateFin=@DateFin,
    Budget=@Budget,
    Statut=@Statut,
    Commentaires=@Commentaires,
    ConversationsMessages=@ConversationsMessages,
    Vues=@Vues,
    Spectateurs=@Spectateurs,
    BudgetQuotidien=@BudgetQuotidien
WHERE Id=@Id;";

                            using (SqlCommand cmd = new SqlCommand(sqlUpdate, cn, tx))
                            {
                                fillCampagneParams(cmd);
                                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = campagneId.Value;
                                cmd.ExecuteNonQuery();
                            }

                            id = campagneId.Value;
                        }
                        else
                        {
                            string sqlInsert = @"
INSERT INTO CampagnesMarketing
(NomCampagne, TypeCampagne, DateDebut, DateFin, Budget, Statut, Commentaires,
 ConversationsMessages, Vues, Spectateurs, BudgetQuotidien)
VALUES
(@NomCampagne, @TypeCampagne, @DateDebut, @DateFin, @Budget, @Statut, @Commentaires,
 @ConversationsMessages, @Vues, @Spectateurs, @BudgetQuotidien);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                            using (SqlCommand cmd = new SqlCommand(sqlInsert, cn, tx))
                            {
                                fillCampagneParams(cmd);
                                id = (int)cmd.ExecuteScalar();
                            }
                        }

                        upsertStatsOrDeleteIfNeeded(id, cn, tx);

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}