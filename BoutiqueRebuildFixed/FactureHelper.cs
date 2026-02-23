using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class FactureHelper
    {
        // Exemple de résultat: ZR-01-20260128-00015
        public static string BuildCodeFactureSequence(SqlConnection con, SqlTransaction trans, int magasinId, DateTime dateVente)
        {
            if (magasinId <= 0) throw new Exception("MagasinId invalide.");

            // ✅ DateKey = 20260128
            int dateKey = int.Parse(dateVente.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture));

            int nextNo;

            // ✅ UPDLOCK + HOLDLOCK => pas de doublons même si 2 ventes en même temps
            using (var cmd = new SqlCommand(@"
SET NOCOUNT ON;

DECLARE @newNo INT;

UPDATE dbo.FactureSequence WITH (UPDLOCK, HOLDLOCK)
SET DernierNumero = DernierNumero + 1
OUTPUT inserted.DernierNumero
WHERE MagasinId = @mag AND DateKey = @dateKey;

IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO dbo.FactureSequence(DateKey, MagasinId, DernierNumero)
    VALUES(@dateKey, @mag, 1);

    SET @newNo = 1;
    SELECT @newNo;
END
", con, trans))
            {
                cmd.Parameters.Add("@mag", SqlDbType.Int).Value = magasinId;
                cmd.Parameters.Add("@dateKey", SqlDbType.Int).Value = dateKey;

                nextNo = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // ✅ Format facture (tu peux changer)
            string code = $"ZR-{magasinId:00}-{dateKey}-{nextNo:00000}";
            return code;
        }
    }
}