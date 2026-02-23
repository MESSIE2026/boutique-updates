using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class AnnulationsRepository
    {
        private readonly string _cs;

        public AnnulationsRepository(string cs)
        {
            _cs = cs;
        }

        public DataTable GetAll()
        {
            using (var conn = new SqlConnection(_cs))
            {
                const string q = @"
SELECT
    Id, NomClient, NumeroCommande, DateAchat, NomProduit,
    Quantite,            -- si tu l’utilises comme QuantiteAchetee
    QuantiteRetournee,
    PrixUnitaire,
    Devise,
    MotifRetour,
    Commentaires,
    TypeRetour,
    IdVente,
    IdDetailsVente,
    IdPaiementRemboursement
FROM dbo.AnnulationsRetours
ORDER BY Id DESC;";

                using (var da = new SqlDataAdapter(q, conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }


        public int Insert(AnnulationRetour a, SqlConnection conn, SqlTransaction tx)
        {
            const string q = @"
INSERT INTO dbo.AnnulationsRetours
(
  NomClient, NumeroCommande, DateAchat, NomProduit,
  Quantite, PrixUnitaire,
  MotifRetour, Commentaires, TypeRetour, Devise,
  IdVente, IdDetailsVente, QuantiteRetournee
)
VALUES
(
  @NomClient, @NumeroCommande, @DateAchat, @NomProduit,
  @QuantiteAchetee, @PrixUnitaire,
  @MotifRetour, @Commentaires, @TypeRetour, @Devise,
  @IdVente, @IdDetailsVente, @QuantiteRetournee
);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var cmd = new SqlCommand(q, conn, tx))
            {
                cmd.Parameters.AddWithValue("@NomClient", a.NomClient ?? "");
                cmd.Parameters.AddWithValue("@NumeroCommande", a.NumeroCommande ?? "");
                cmd.Parameters.AddWithValue("@DateAchat", a.DateAchat.Date);
                cmd.Parameters.AddWithValue("@NomProduit", a.NomProduit ?? "");

                cmd.Parameters.AddWithValue("@QuantiteAchetee", a.QuantiteAchetee);
                cmd.Parameters.AddWithValue("@PrixUnitaire", a.PrixUnitaire);

                cmd.Parameters.AddWithValue("@MotifRetour", a.MotifRetour ?? "");
                cmd.Parameters.AddWithValue("@Commentaires", a.Commentaires ?? "");
                cmd.Parameters.AddWithValue("@TypeRetour", a.TypeRetour ?? "");
                cmd.Parameters.AddWithValue("@Devise", a.Devise ?? "");

                cmd.Parameters.AddWithValue("@IdVente", (object)a.IdVente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdDetailsVente", (object)a.IdDetailsVente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@QuantiteRetournee", a.QuantiteRetournee);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        public AnnulationRetour GetById(int id)
        {
            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT TOP 1 * FROM dbo.AnnulationsRetours WHERE Id=@Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;

                        return new AnnulationRetour
                        {
                            Id = Convert.ToInt32(rd["Id"]),
                            IdVente = rd["IdVente"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["IdVente"]),
                            IdDetailsVente = rd["IdDetailsVente"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["IdDetailsVente"]),
                            IdPaiementRemboursement = rd["IdPaiementRemboursement"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["IdPaiementRemboursement"]),

                            NomClient = rd["NomClient"]?.ToString(),
                            NumeroCommande = rd["NumeroCommande"]?.ToString(),
                            DateAchat = Convert.ToDateTime(rd["DateAchat"]),
                            NomProduit = rd["NomProduit"]?.ToString(),

                            QuantiteAchetee = Convert.ToDecimal(rd["Quantite"]),
                            QuantiteRetournee = Convert.ToDecimal(rd["QuantiteRetournee"]),

                            PrixUnitaire = Convert.ToDecimal(rd["PrixUnitaire"]),
                            Devise = rd["Devise"]?.ToString(),

                            MotifRetour = rd["MotifRetour"]?.ToString(),
                            Commentaires = rd["Commentaires"]?.ToString(),
                            TypeRetour = rd["TypeRetour"]?.ToString()
                        };
                    }
                }
            }
        }

        public void UpdateIdPaiementRemboursement(int idAnnulation, int idPaiement, SqlConnection conn, SqlTransaction tx)
        {
            using (var cmd = new SqlCommand(
                "UPDATE AnnulationsRetours SET IdPaiementRemboursement=@p WHERE Id=@id",
                conn, tx))
            {
                cmd.Parameters.AddWithValue("@p", idPaiement);
                cmd.Parameters.AddWithValue("@id", idAnnulation);
                cmd.ExecuteNonQuery();
            }
        }
    }
}