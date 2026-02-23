using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class LoyaltyService
    {
        private readonly string _cs;

        public LoyaltyService(string connectionString)
        {
            _cs = connectionString;
        }

        // ✅ Gain points + cashback sur vente
        // Règles exemple :
        // - 1 point / 1000 CDF
        // - cashback BRONZE 0.5%, SILVER 1%, GOLD 1.5%
        public void ApplyGain(int idClient, int idVente, decimal montantNet,
                              SqlConnection cn, SqlTransaction tx)
        {
            string statut = "BRONZE";
            int points = 0;
            decimal cashbackSolde = 0m;

            using (SqlCommand cmd = new SqlCommand(@"
SELECT Statut, Points, CashbackSolde
FROM LoyaltyCompte
WHERE IdClient=@id;", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read()) throw new Exception("LoyaltyCompte introuvable. (Assure EnsureClientModules)");

                    if (!r.IsDBNull(0)) statut = r.GetString(0);
                    if (!r.IsDBNull(1)) points = r.GetInt32(1);
                    if (!r.IsDBNull(2)) cashbackSolde = Convert.ToDecimal(r.GetValue(2));
                }
            }

            int pointsGagnes = (int)Math.Floor(montantNet / 1000m);

            decimal taux = 0.005m;
            string st = (statut ?? "BRONZE").ToUpperInvariant();
            if (st == "SILVER") taux = 0.010m;
            else if (st == "GOLD") taux = 0.015m;

            decimal cashbackGagne = Math.Round(montantNet * taux, 2);

            // Maj compte
            using (SqlCommand cmd = new SqlCommand(@"
UPDATE LoyaltyCompte
SET Points = ISNULL(Points,0) + @p,
    CashbackSolde = ISNULL(CashbackSolde,0) + @c,
    DateMaj=GETDATE()
WHERE IdClient=@id;", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                cmd.Parameters.Add("@p", SqlDbType.Int).Value = pointsGagnes;
                cmd.Parameters.Add("@c", SqlDbType.Decimal).Value = cashbackGagne;
                cmd.ExecuteNonQuery();
            }

            // Mouvement
            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO LoyaltyMouvement(IdClient,DateMvt,Type,PointsDelta,CashbackDelta,RefVente,Note)
VALUES(@id,GETDATE(),'GAIN',@p,@c,@v,@n);", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                cmd.Parameters.Add("@p", SqlDbType.Int).Value = pointsGagnes;
                cmd.Parameters.Add("@c", SqlDbType.Decimal).Value = cashbackGagne;
                cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVente;
                cmd.Parameters.Add("@n", SqlDbType.NVarChar, 200).Value = "Gain fidélité sur vente";
                cmd.ExecuteNonQuery();
            }

            // Recalcul statut (ex simple)
            UpdateStatutIfNeeded(idClient, cn, tx);
        }

        private void UpdateStatutIfNeeded(int idClient, SqlConnection cn, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(@"
UPDATE LoyaltyCompte
SET Statut = CASE 
                WHEN Points >= 2000 THEN 'GOLD'
                WHEN Points >= 500 THEN 'SILVER'
                ELSE 'BRONZE'
            END,
    DateMaj=GETDATE()
WHERE IdClient=@id;", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                cmd.ExecuteNonQuery();
            }
        }

        // ✅ Utiliser cashback (déduit du solde cashback)
        public decimal UseCashback(int idClient, int idVente, decimal montantAUtiliser,
                                   SqlConnection cn, SqlTransaction tx)
        {
            if (montantAUtiliser <= 0m) return 0m;

            decimal solde = 0m;
            using (SqlCommand cmd = new SqlCommand(@"
SELECT ISNULL(CashbackSolde,0)
FROM LoyaltyCompte WHERE IdClient=@id;", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                solde = Convert.ToDecimal(cmd.ExecuteScalar());
            }

            decimal used = (solde < montantAUtiliser) ? solde : montantAUtiliser;
            if (used <= 0m) return 0m;

            using (SqlCommand cmd = new SqlCommand(@"
UPDATE LoyaltyCompte
SET CashbackSolde = ISNULL(CashbackSolde,0) - @u,
    DateMaj=GETDATE()
WHERE IdClient=@id;", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                cmd.Parameters.Add("@u", SqlDbType.Decimal).Value = used;
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO LoyaltyMouvement(IdClient,DateMvt,Type,PointsDelta,CashbackDelta,RefVente,Note)
VALUES(@id,GETDATE(),'USE_CASHBACK',0,@d,@v,@n);", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                cmd.Parameters.Add("@d", SqlDbType.Decimal).Value = -used;
                cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVente;
                cmd.Parameters.Add("@n", SqlDbType.NVarChar, 200).Value = "Utilisation cashback";
                cmd.ExecuteNonQuery();
            }

            return used;
        }
    }
}