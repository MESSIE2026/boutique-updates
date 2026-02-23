using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class PaiementsVenteService
    {
        public static DataTable GetPaiements(int idVente, string cs)
        {
            if (idVente <= 0) throw new ArgumentException("idVente invalide.");

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(@"
SELECT IdPaiement, ModePaiement, Devise, Montant, ReferenceTransaction,
       ISNULL(Statut,'VALIDE') AS Statut,
       ISNULL(AnnulePar,'') AS AnnulePar,
       DateAnnulation,
       ISNULL(MotifAnnulation,'') AS MotifAnnulation,
       DatePaiement
FROM dbo.PaiementsVente
WHERE IdVente=@id
ORDER BY DatePaiement;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static bool AnnulerPaiement(int idPaiement, string cs, string user, string motif)
        {
            if (idPaiement <= 0) throw new ArgumentException("idPaiement invalide.");
            user = string.IsNullOrWhiteSpace(user) ? "SYSTEM" : user.Trim();
            motif = (motif ?? "").Trim();

            if (motif.Length == 0) throw new ArgumentException("Motif obligatoire.");

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();

                using (var cmd = new SqlCommand(@"
UPDATE dbo.PaiementsVente
SET Statut='ANNULE',
    AnnulePar=@user,
    DateAnnulation=GETDATE(),
    MotifAnnulation=@motif
WHERE IdPaiement=@id AND ISNULL(Statut,'VALIDE')<>'ANNULE';", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPaiement;
                    cmd.Parameters.Add("@user", SqlDbType.NVarChar, 120).Value = user;
                    cmd.Parameters.Add("@motif", SqlDbType.NVarChar, 255).Value = motif;

                    int n = cmd.ExecuteNonQuery();
                    return n > 0;
                }
            }
        }

        // ✅ INSERT pendant la vente (dans la transaction existante)
        public static void InsererPaiements(int idVente, SqlConnection con, SqlTransaction trans,
            DataTable dtPaiements, string deviseVente)
        {
            if (idVente <= 0) throw new ArgumentException("idVente invalide.");
            if (con == null) throw new ArgumentNullException(nameof(con));
            if (trans == null) throw new ArgumentNullException(nameof(trans));
            if (dtPaiements == null || dtPaiements.Rows.Count == 0)
                throw new ArgumentException("Aucun paiement à insérer.");

            foreach (DataRow r in dtPaiements.Rows)
            {
                string mode = (r["Mode"] ?? "").ToString().Trim();
                string dev = (r["Devise"] ?? deviseVente).ToString().Trim().ToUpperInvariant();
                decimal montant = Convert.ToDecimal(r["Montant"]);
                string reference = (r["Reference"] ?? "").ToString().Trim();

                if (string.IsNullOrWhiteSpace(mode) || montant <= 0) continue;

                using (var cmdPay = new SqlCommand(@"
INSERT INTO dbo.PaiementsVente
(IdVente, ModePaiement, Devise, Montant, DatePaiement, ReferenceTransaction, Statut)
VALUES
(@IdVente, @Mode, @Devise, @Montant, GETDATE(), @Ref, 'VALIDE');", con, trans))
                {
                    cmdPay.Parameters.Add("@IdVente", SqlDbType.Int).Value = idVente;
                    cmdPay.Parameters.Add("@Mode", SqlDbType.NVarChar, 50).Value = mode;
                    cmdPay.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = dev;

                    var pm = cmdPay.Parameters.Add("@Montant", SqlDbType.Decimal);
                    pm.Precision = 18; pm.Scale = 2; pm.Value = montant;

                    cmdPay.Parameters.Add("@Ref", SqlDbType.NVarChar, 120).Value =
                        string.IsNullOrWhiteSpace(reference) ? (object)DBNull.Value : reference;

                    cmdPay.ExecuteNonQuery();
                }
            }
        }
    }
}
