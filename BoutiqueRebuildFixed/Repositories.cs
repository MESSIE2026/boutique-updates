using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    internal class ClientRepository
    {
        private readonly string _cs;
        public ClientRepository(string connectionString) => _cs = connectionString;

        public int GetOrCreateClientId(SqlConnection con, SqlTransaction trans,
            string nom, string prenom, string adresse, string telephone, string email)
        {
            // 1) Chercher
            using (var cmdCheck = new SqlCommand(@"
SELECT TOP 1 ID_Clients
FROM Clients
WHERE Nom = @nom AND Telephone = @telephone", con, trans))
            {
                SqlHelper.AddNVarChar(cmdCheck, "@nom", nom, 120);
                SqlHelper.AddNVarChar(cmdCheck, "@telephone", telephone, 50);

                object result = cmdCheck.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    return Convert.ToInt32(result);
            }

            // 2) Créer
            using (var cmdInsert = new SqlCommand(@"
INSERT INTO Clients (Nom, Prenom, Adresse, Telephone, Email)
OUTPUT INSERTED.ID_Clients
VALUES (@nom, @prenom, @adresse, @telephone, @email)", con, trans))
            {
                SqlHelper.AddNVarChar(cmdInsert, "@nom", nom, 120);
                SqlHelper.AddNVarChar(cmdInsert, "@prenom", prenom, 120);
                SqlHelper.AddNVarChar(cmdInsert, "@adresse", adresse, 200);
                SqlHelper.AddNVarChar(cmdInsert, "@telephone", telephone, 50);

                if (string.IsNullOrWhiteSpace(email))
                    SqlHelper.AddParam(cmdInsert, "@email", SqlDbType.NVarChar, DBNull.Value);
                else
                    SqlHelper.AddNVarChar(cmdInsert, "@email", email, 120);

                return (int)cmdInsert.ExecuteScalar();
            }
        }
    }

    internal class ProduitRepository
    {
        private readonly string _cs;
        public ProduitRepository(string connectionString) => _cs = connectionString;

        /// <summary>
        /// Recherche scalable (TOP N) : évite de charger tous les produits.
        /// </summary>
        public List<FormVentes.ProduitCombo> SearchProduitsByPrefix(string prefix, int take = 25)
        {
            var list = new List<FormVentes.ProduitCombo>();
            if (prefix == null) prefix = "";
            prefix = prefix.Trim();

            using (var con = new SqlConnection(_cs))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP (@take) ID_Produit, NomProduit, RefProduit, Prix, Categorie, Taille, Couleur
FROM Produit
WHERE NomProduit LIKE @p + '%'
ORDER BY NomProduit", con))
                {
                    SqlHelper.AddParam(cmd, "@take", SqlDbType.Int, take);
                    SqlHelper.AddNVarChar(cmd, "@p", prefix, 200);

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new FormVentes.ProduitCombo
                            {
                                ID = Convert.ToInt32(dr["ID_Produit"]),
                                NomProduit = dr["NomProduit"].ToString(),
                                Ref = dr["RefProduit"].ToString(),
                                Prix = Convert.ToDecimal(dr["Prix"], CultureInfo.InvariantCulture),
                                Categorie = dr["Categorie"].ToString(),
                                Taille = dr["Taille"].ToString(),
                                Couleur = dr["Couleur"].ToString(),
                            });
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// ✅ GetProduitById en transaction (quand tu es déjà dans une vente)
        /// </summary>
        public FormVentes.ProduitCombo GetProduitById(SqlConnection con, SqlTransaction trans, int idProduit)
        {
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 ID_Produit, NomProduit, RefProduit, Prix, Categorie, Taille, Couleur
FROM Produit
WHERE ID_Produit = @id", con, trans))
            {
                SqlHelper.AddParam(cmd, "@id", SqlDbType.Int, idProduit);

                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;

                    return new FormVentes.ProduitCombo
                    {
                        ID = Convert.ToInt32(dr["ID_Produit"]),
                        NomProduit = dr["NomProduit"].ToString(),
                        Ref = dr["RefProduit"].ToString(),
                        Prix = Convert.ToDecimal(dr["Prix"], CultureInfo.InvariantCulture),
                        Categorie = dr["Categorie"].ToString(),
                        Taille = dr["Taille"].ToString(),
                        Couleur = dr["Couleur"].ToString(),
                    };
                }
            }
        }

        /// <summary>
        /// ✅ GetProduitById hors transaction (ouvre sa propre connexion)
        /// </summary>
        public FormVentes.ProduitCombo GetProduitById(int idProduit)
        {
            using (var con = new SqlConnection(_cs))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 ID_Produit, NomProduit, RefProduit, Prix, Categorie, Taille, Couleur
FROM Produit
WHERE ID_Produit = @id", con))
                {
                    SqlHelper.AddParam(cmd, "@id", SqlDbType.Int, idProduit);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read()) return null;

                        return new FormVentes.ProduitCombo
                        {
                            ID = Convert.ToInt32(dr["ID_Produit"]),
                            NomProduit = dr["NomProduit"].ToString(),
                            Ref = dr["RefProduit"].ToString(),
                            Prix = Convert.ToDecimal(dr["Prix"], CultureInfo.InvariantCulture),
                            Categorie = dr["Categorie"].ToString(),
                            Taille = dr["Taille"].ToString(),
                            Couleur = dr["Couleur"].ToString(),
                        };
                    }
                }
            }
        }
    }

    internal class StockRepository
    {
        private readonly string _cs;
        public StockRepository(string connectionString) => _cs = connectionString;

        public void SortieStockAtomique(
            SqlConnection con,
            SqlTransaction trans,
            int idProduit,
            string reference,
            int qte,
            string utilisateur,
            string motif = "VENTE",
            string emplacement = null,
            string remarques = null)
        {
            if (idProduit <= 0) throw new Exception("ID_Produit invalide pour stock.");
            if (string.IsNullOrWhiteSpace(reference)) throw new Exception("Référence invalide pour stock.");
            if (qte <= 0) throw new Exception("Quantité invalide pour stock.");

            utilisateur = (utilisateur ?? "").Trim();
            if (utilisateur.Length == 0) utilisateur = "SYSTEM";

            using (var cmd = new SqlCommand(@"
DECLARE @stockActuel INT;

SELECT @stockActuel =
    ISNULL(SUM(CASE WHEN TypeOperation = 'ENTREE' THEN Quantite ELSE 0 END), 0) -
    ISNULL(SUM(CASE WHEN TypeOperation = 'SORTIE' THEN Quantite ELSE 0 END), 0)
FROM OperationsStock WITH (UPDLOCK, HOLDLOCK)
WHERE Reference = @ref;

IF (@stockActuel IS NULL) SET @stockActuel = 0;

-- ✅ Ici tu avais dit: on ne bloque plus, on enregistre quand même la sortie
INSERT INTO OperationsStock
(ID_Produit, TypeOperation, Quantite, DateOperation, Utilisateur, Motif, Reference, Emplacement, Remarques)
VALUES
(@idProduit, 'SORTIE', @qte, GETDATE(), @utilisateur, @motif, @ref, @emplacement, @remarques);
", con, trans))
            {
                cmd.Parameters.Add("@idProduit", SqlDbType.Int).Value = idProduit;
                cmd.Parameters.Add("@qte", SqlDbType.Int).Value = qte;
                cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 50).Value = reference;
                cmd.Parameters.Add("@utilisateur", SqlDbType.NVarChar, 120).Value = utilisateur;

                cmd.Parameters.Add("@motif", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(motif) ? (object)DBNull.Value : motif;

                cmd.Parameters.Add("@emplacement", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(emplacement) ? (object)DBNull.Value : emplacement;

                cmd.Parameters.Add("@remarques", SqlDbType.NVarChar, 200).Value =
                    string.IsNullOrWhiteSpace(remarques) ? (object)DBNull.Value : remarques;

                cmd.ExecuteNonQuery();
            }
        }
    }

    internal class VenteRepository
    {
        public int InsertVente(SqlConnection con, SqlTransaction trans,
            int idClient, int idEmploye, string modePaiement, decimal montantTotal, string nomCaissier, string devise)
        {
            using (var cmd = new SqlCommand(@"
INSERT INTO Vente
(DateVente, ID_Client, IDEmploye, ModePaiement, MontantTotal, NomCaissier, Devise)
OUTPUT INSERTED.ID_Vente
VALUES
(GETDATE(), @idClient, @idEmploye, @modePaiement, @montantTotal, @nomCaissier, @devise);", con, trans))
            {
                SqlHelper.AddParam(cmd, "@idClient", SqlDbType.Int, idClient);
                SqlHelper.AddParam(cmd, "@idEmploye", SqlDbType.Int, idEmploye);
                SqlHelper.AddNVarChar(cmd, "@modePaiement", modePaiement, 50);
                SqlHelper.AddDecimal(cmd, "@montantTotal", montantTotal, 18, 2);
                SqlHelper.AddNVarChar(cmd, "@nomCaissier", nomCaissier, 120);
                SqlHelper.AddNVarChar(cmd, "@devise", devise, 10);

                return (int)cmd.ExecuteScalar();
            }
        }

        public void InsertDetail(SqlConnection con, SqlTransaction trans,
            int idVente, int idProduit, string nomProduit, int quantite, decimal prixUnitaire,
            string refProduit, decimal remise, decimal tva, decimal montant, string devise, string nomCaissier)
        {
            using (var cmd = new SqlCommand(@"
INSERT INTO DetailsVente
(ID_Vente, ID_Produit, NomProduit, Quantite, PrixUnitaire,
 RefProduit, Remise, TVA, Montant, Devise, NomCaissier)
VALUES
(@idVente, @idProduit, @nomProduit, @quantite, @prixUnitaire,
 @refProduit, @remise, @tva, @montant, @devise, @nomCaissier);", con, trans))
            {
                SqlHelper.AddParam(cmd, "@idVente", SqlDbType.Int, idVente);
                SqlHelper.AddParam(cmd, "@idProduit", SqlDbType.Int, idProduit);
                SqlHelper.AddNVarChar(cmd, "@nomProduit", nomProduit, 200);
                SqlHelper.AddParam(cmd, "@quantite", SqlDbType.Int, quantite);
                SqlHelper.AddDecimal(cmd, "@prixUnitaire", prixUnitaire, 18, 2);
                SqlHelper.AddNVarChar(cmd, "@refProduit", refProduit, 50);
                SqlHelper.AddDecimal(cmd, "@remise", remise, 18, 2);
                SqlHelper.AddDecimal(cmd, "@tva", tva, 18, 2);
                SqlHelper.AddDecimal(cmd, "@montant", montant, 18, 2);
                SqlHelper.AddNVarChar(cmd, "@devise", devise, 10);
                SqlHelper.AddNVarChar(cmd, "@nomCaissier", nomCaissier, 120);

                cmd.ExecuteNonQuery();
            }
        }
    }
}