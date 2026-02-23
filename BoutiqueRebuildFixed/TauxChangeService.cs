using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class TauxChangeService
    {
        // ------------------------------
        // Normalisation devise
        // ------------------------------
        public static string Normalize(string d)
        {
            d = (d ?? "").Trim().ToUpperInvariant();
            if (d == "FC") d = "CDF";
            return d;
        }

        // ------------------------------
        // GetTaux (utile pour les anciens appels)
        // Retourne le taux ACTIF le plus récent
        // ------------------------------
        public static decimal GetTaux(string fromDevise, string toDevise, string connectionString)
        {
            fromDevise = Normalize(fromDevise);
            toDevise = Normalize(toDevise);

            if (string.IsNullOrWhiteSpace(fromDevise) || string.IsNullOrWhiteSpace(toDevise))
                return 0m;

            if (string.Equals(fromDevise, toDevise, StringComparison.OrdinalIgnoreCase))
                return 1m;

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 Taux
FROM dbo.TauxChange
WHERE DeviseFrom = @f AND DeviseTo = @t AND Actif = 1
ORDER BY DateEffet DESC, Id DESC;", con))
            {
                cmd.Parameters.Add("@f", SqlDbType.NVarChar, 10).Value = fromDevise;
                cmd.Parameters.Add("@t", SqlDbType.NVarChar, 10).Value = toDevise;

                con.Open();
                object o = cmd.ExecuteScalar();
                if (o == null || o == DBNull.Value) return 0m;

                return Convert.ToDecimal(o);
            }
        }

        // ------------------------------
        // Convertir (avec Transaction)
        // Appelle dbo.TauxChange_Convertir
        // ------------------------------
        public static decimal Convertir(
            SqlConnection con,
            SqlTransaction trans,
            decimal montant,
            string fromDevise,
            string toDevise,
            out decimal tauxUtilise)
        {
            fromDevise = Normalize(fromDevise);
            toDevise = Normalize(toDevise);

            if (string.IsNullOrWhiteSpace(fromDevise) || string.IsNullOrWhiteSpace(toDevise))
            {
                tauxUtilise = 0m;
                return 0m;
            }

            if (string.Equals(fromDevise, toDevise, StringComparison.OrdinalIgnoreCase))
            {
                tauxUtilise = 1m;
                return Math.Round(montant, 2);
            }

            // ✅ IMPORTANT : ne pas arrondir à 2 avant conversion
            using (var cmd = new SqlCommand("dbo.TauxChange_Convertir", con, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                var pMontant = cmd.Parameters.Add("@Montant", SqlDbType.Decimal);
                pMontant.Precision = 18;
                pMontant.Scale = 4;          // ✅ meilleure précision d'entrée
                pMontant.Value = montant;

                cmd.Parameters.Add("@From", SqlDbType.NVarChar, 10).Value = fromDevise;
                cmd.Parameters.Add("@To", SqlDbType.NVarChar, 10).Value = toDevise;

                var pRes = cmd.Parameters.Add("@Result", SqlDbType.Decimal);
                pRes.Direction = ParameterDirection.Output;
                pRes.Precision = 18;
                pRes.Scale = 2;              // résultat final (monétaire)

                var pTaux = cmd.Parameters.Add("@TauxUtilise", SqlDbType.Decimal);
                pTaux.Direction = ParameterDirection.Output;
                pTaux.Precision = 18;
                pTaux.Scale = 8;             // ✅ taux plus précis

                cmd.ExecuteNonQuery();

                tauxUtilise = (pTaux.Value == DBNull.Value) ? 0m : Convert.ToDecimal(pTaux.Value);
                decimal res = (pRes.Value == DBNull.Value) ? 0m : Convert.ToDecimal(pRes.Value);

                return res;
            }
        }

        // ------------------------------
        // Convertir (sans Transaction)
        // ------------------------------
        public static decimal Convertir(
            string connectionString,
            decimal montant,
            string fromDevise,
            string toDevise,
            out decimal tauxUtilise)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                return Convertir(con, null, montant, fromDevise, toDevise, out tauxUtilise);
            }
        }
    }
}