using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class MarketingRepository
    {
        private readonly string _cs;
        public MarketingRepository(string cs) { _cs = cs; }

        public DataTable GetCampagnesPaged(
            int page, int pageSize,
            string statut, string type, string search,
            DateTime? from, DateTime? to,
            int? idEntreprise = null)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();

                string sql = @"
WITH base AS (
    SELECT
        c.Id,
        c.NomCampagne,
        c.TypeCampagne,
        c.DateDebut,
        c.DateFin,
        c.Budget,
        c.Statut,
        c.Commentaires,

        ISNULL(s.Vues, 0) AS Vues,
        ISNULL(s.Messages, 0) AS Messages,
        ISNULL(s.Spectateurs, 0) AS Spectateurs,
        ISNULL(s.BudgetQuotidien, 0) AS BudgetQuotidien,
        ISNULL(s.NombreVentes, 0) AS NombreVentes,
        ISNULL(s.MontantVendus, 0) AS MontantVendus,
        ISNULL(s.Devise, 'FC') AS Devise,

        ROW_NUMBER() OVER (ORDER BY c.DateDebut DESC, c.Id DESC) AS rn
    FROM CampagnesMarketing c
    LEFT JOIN StatistiquesPublicites s ON s.CampagneId = c.Id
    WHERE 1=1
      AND (@Statut IS NULL OR c.Statut = @Statut)
      AND (@Type IS NULL OR c.TypeCampagne = @Type)
      AND (@Search IS NULL OR c.NomCampagne LIKE '%' + @Search + '%')
      AND (@From IS NULL OR c.DateDebut >= @From)
      AND (@To IS NULL OR c.DateFin <= @To)
      AND (@IdEntreprise IS NULL OR c.IdEntreprise = @IdEntreprise)
)
SELECT *
FROM base
WHERE rn BETWEEN @StartRow AND @EndRow
ORDER BY rn;";

                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@Statut", SqlDbType.NVarChar, 30).Value =
                        (object)statut ?? DBNull.Value;

                    cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 50).Value =
                        (object)type ?? DBNull.Value;

                    cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 150).Value =
                        string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search.Trim();

                    cmd.Parameters.Add("@From", SqlDbType.Date).Value =
                        (object)from ?? DBNull.Value;

                    cmd.Parameters.Add("@To", SqlDbType.Date).Value =
                        (object)to ?? DBNull.Value;

                    cmd.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value =
                        (object)idEntreprise ?? DBNull.Value;

                    int start = (page - 1) * pageSize + 1;
                    int end = page * pageSize;

                    cmd.Parameters.Add("@StartRow", SqlDbType.Int).Value = start;
                    cmd.Parameters.Add("@EndRow", SqlDbType.Int).Value = end;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }
    }
}