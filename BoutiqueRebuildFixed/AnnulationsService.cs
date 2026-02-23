using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class AnnulationsService
    {
        private readonly string _cs;
        private readonly AnnulationsRepository _repo;

        public AnnulationsService(string cs)
        {
            _cs = cs;
            _repo = new AnnulationsRepository(cs);
        }

        public void Valider(AnnulationRetour a)
        {
            if (a == null) throw new Exception("Objet annulation invalide.");

            if (string.IsNullOrWhiteSpace(a.NomClient)) throw new Exception("Nom client obligatoire.");
            if (string.IsNullOrWhiteSpace(a.NumeroCommande)) throw new Exception("Numéro commande obligatoire.");
            if (string.IsNullOrWhiteSpace(a.NomProduit)) throw new Exception("Nom produit obligatoire.");

            // ✅ on valide la quantité retournée (plus Quantite)
            if (a.QuantiteRetournee <= 0) throw new Exception("Quantité retournée invalide.");

            if (a.PrixUnitaire <= 0) throw new Exception("Prix unitaire invalide.");
            if (string.IsNullOrWhiteSpace(a.Devise)) throw new Exception("Devise obligatoire.");
            if (string.IsNullOrWhiteSpace(a.TypeRetour)) throw new Exception("Type retour obligatoire.");
        }

        public int EnregistrerEtAppliquer(AnnulationRetour a, bool remettreStock, bool faireRemboursement)
        {
            Valider(a);

            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // ✅ Déterminer type retour
                        bool retourTotal = a.IdVente.HasValue && !a.IdDetailsVente.HasValue;
                        bool retourPartiel = a.IdVente.HasValue && a.IdDetailsVente.HasValue;

                        // ✅ quantité retournée
                        decimal qteRetour = a.QuantiteRetournee;
                        if (qteRetour <= 0) throw new Exception("Quantité retournée invalide.");

                        // ✅ montant à rembourser
                        decimal montantRetour = Math.Round(a.PrixUnitaire * qteRetour, 2);

                        // 1) INSERT AnnulationsRetours
                        int idAnnulation = _repo.Insert(a, conn, tx);

                        // 2) STOCK (ENTREE) si retour physique
                        if (remettreStock && a.IdDetailsVente.HasValue)
                        {
                            // récupérer ID_Produit depuis DetailsVente
                            int idProduit;
                            using (var cmdP = new SqlCommand(
                                "SELECT TOP 1 ID_Produit FROM dbo.DetailsVente WHERE ID_Details=@d", conn, tx))
                            {
                                cmdP.Parameters.AddWithValue("@d", a.IdDetailsVente.Value);
                                object o = cmdP.ExecuteScalar();
                                if (o == null || o == DBNull.Value) throw new Exception("Produit introuvable dans DetailsVente.");
                                idProduit = Convert.ToInt32(o);
                            }

                            using (var cmdS = new SqlCommand(@"
INSERT INTO dbo.OperationsStock
(
    ID_Produit, TypeOperation, Quantite, DateOperation,
    Utilisateur, Motif, Reference, Emplacement,
    Remarques, TypeMouvement,
    IdEntreprise, IdMagasin
)
VALUES
(
    @prod, 'ENTREE', @qte, GETDATE(),
    @u, @motif, @ref, 'RETOUR',
    @rem, 'RETOUR_CLIENT',
    @IdEntreprise, @IdMagasin
);", conn, tx))
                            {
                                cmdS.Parameters.AddWithValue("@prod", idProduit);
                                cmdS.Parameters.AddWithValue("@qte", qteRetour);
                                cmdS.Parameters.AddWithValue("@u", a.Utilisateur ?? "SYSTEM");
                                cmdS.Parameters.AddWithValue("@motif", a.MotifRetour ?? "Retour client");
                                cmdS.Parameters.AddWithValue("@ref", a.NumeroCommande ?? "");
                                cmdS.Parameters.AddWithValue("@rem", a.Commentaires ?? "");

                                cmdS.Parameters.AddWithValue("@IdEntreprise", AppContext.IdEntreprise);
                                cmdS.Parameters.AddWithValue("@IdMagasin", AppContext.IdMagasin);

                                cmdS.ExecuteNonQuery();
                            }
                        }

                        // 3) REMBOURSEMENT (PaiementsVente) si remboursement
                        if (faireRemboursement && a.IdVente.HasValue)
                        {
                            int idPaiement;

                            using (var cmdPay = new SqlCommand(@"
INSERT INTO dbo.PaiementsVente
(
    IdVente, ModePaiement, Devise, Montant, DatePaiement,
    ReferenceTransaction, Statut, MotifAnnulation,
    IdEntreprise, IdMagasin, IdPoste
)
VALUES
(
    @idV, 'REMBOURSEMENT', @dev, @montant, GETDATE(),
    @ref, 'OK', @motif,
    @IdEntreprise, @IdMagasin, @IdPoste
);

SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                            {
                                cmdPay.Parameters.AddWithValue("@idV", a.IdVente.Value);
                                cmdPay.Parameters.AddWithValue("@dev", a.Devise);

                                // ✅ négatif
                                cmdPay.Parameters.AddWithValue("@montant", -Math.Abs(montantRetour));

                                cmdPay.Parameters.AddWithValue("@ref", "RMB-" + idAnnulation);
                                cmdPay.Parameters.AddWithValue("@motif", a.MotifRetour ?? "Remboursement client");

                                cmdPay.Parameters.AddWithValue("@IdEntreprise", AppContext.IdEntreprise);
                                cmdPay.Parameters.AddWithValue("@IdMagasin", AppContext.IdMagasin);
                                cmdPay.Parameters.AddWithValue("@IdPoste", AppContext.IdPoste);

                                idPaiement = Convert.ToInt32(cmdPay.ExecuteScalar());
                            }

                            _repo.UpdateIdPaiementRemboursement(idAnnulation, idPaiement, conn, tx);
                        }

                        // 4) VENTE : annuler seulement si retour total
                        if (retourTotal)
                        {
                            using (var cmdV = new SqlCommand(@"
UPDATE dbo.Vente
SET Statut = 'ANNULE',
    AnnulePar = @u,
    DateAnnulation = GETDATE(),
    MotifAnnulation = @m
WHERE ID_Vente = @id;", conn, tx))
                            {
                                cmdV.Parameters.AddWithValue("@u", a.Utilisateur ?? "SYSTEM");
                                cmdV.Parameters.AddWithValue("@m", a.MotifRetour ?? "Retour total");
                                cmdV.Parameters.AddWithValue("@id", a.IdVente.Value);
                                cmdV.ExecuteNonQuery();
                            }
                        }
                        // ✅ retour partiel => on ne touche pas Vente

                        tx.Commit();
                        return idAnnulation;
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        public AnnulationsRepository Repo
        {
            get { return _repo; }
        }
    }
}