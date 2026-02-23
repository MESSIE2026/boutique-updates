using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    internal class VenteService
    {
        private readonly string _cs;
        private readonly ClientRepository _clientRepo;
        private readonly VenteRepository _venteRepo;
        private readonly StockRepository _stockRepo;

        public bool PanierContientProduitReglemente(DataGridView dgvPanier, out string details)
        {
            details = "";
            if (dgvPanier == null || dgvPanier.Rows.Count == 0) return false;

            // 1) IDs produits distincts
            var ids = dgvPanier.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r => Convert.ToInt32(r.Cells["ID_Produit"].Value ?? 0))
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (ids.Count == 0) return false;

            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;

                // IN (@p0,@p1,...) paramétré
                var paramNames = ids.Select((id, i) => "@p" + i).ToList();
                cmd.CommandText = $@"
SELECT p.ID_Produit, p.NomProduit
FROM dbo.Produit p
WHERE p.ID_Produit IN ({string.Join(",", paramNames)})
  AND (ISNULL(p.IsReglemente,0)=1 OR ISNULL(p.SignatureManagerRequired,0)=1);";

                for (int i = 0; i < ids.Count; i++)
                    cmd.Parameters.Add(paramNames[i], SqlDbType.Int).Value = ids[i];

                con.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    var list = new List<string>();
                    while (rd.Read())
                    {
                        int idP = Convert.ToInt32(rd["ID_Produit"]);
                        string nom = rd["NomProduit"]?.ToString() ?? "";
                        list.Add($"{idP} - {nom}");
                    }

                    if (list.Count > 0)
                    {
                        details = "Produits réglementés : " + string.Join(", ", list);
                        return true;
                    }
                }
            }

            return false;
        }
        public VenteService(string connectionString)
        {
            _cs = connectionString;
            _clientRepo = new ClientRepository(connectionString);
            _venteRepo = new VenteRepository();
            _stockRepo = new StockRepository(connectionString);
        }

        public int FinaliserVente(
            DataGridView dgvPanier,
            int idEmploye,
            string modePaiement,
            decimal montantTotal,
            string nomCaissier,
            string devise,
            // client
            string nomClient, string prenom, string adresse, string telephone, string email)
        {
            using (var con = new SqlConnection(_cs))
            {
                con.Open();
                using (var trans = con.BeginTransaction())
                {
                    try
                    {
                        // 1) Client (GetOrCreate)
                        int idClient = _clientRepo.GetOrCreateClientId(con, trans,
                            nomClient?.Trim(),
                            prenom?.Trim(),
                            adresse?.Trim(),
                            telephone?.Trim(),
                            email?.Trim());

                        // 2) Vente
                        int idVente = _venteRepo.InsertVente(con, trans,
                            idClient, idEmploye, modePaiement?.Trim(), montantTotal, nomCaissier?.Trim(), devise?.Trim());

                        // 3) Détails + stock atomique
                        foreach (DataGridViewRow r in dgvPanier.Rows)
                        {
                            if (r.IsNewRow) continue;

                            int idProduit = Convert.ToInt32(r.Cells["ID_Produit"].Value ?? 0);
                            string nomProduit = (r.Cells["NomProduit"].FormattedValue ?? "").ToString();
                            string refProduit = (r.Cells["RefProduit"].Value ?? "").ToString();

                            int quantite = 0;
                            int.TryParse((r.Cells["Quantite"].Value ?? "0").ToString(), out quantite);
                            if (quantite <= 0) throw new Exception("Quantité invalide dans le panier.");

                            decimal prixUnitaire = ParseDecimalFr(r.Cells["PrixUnitaire"].Value);
                            decimal remise = ParseDecimalFr(r.Cells["Remise"].Value);
                            decimal tva = ParseDecimalFr(r.Cells["TVA"].Value);
                            decimal montant = ParseDecimalFr(r.Cells["Montant"].Value);

                            // ✅ Stock atomique
                            if (!string.IsNullOrWhiteSpace(refProduit))
                                _stockRepo.SortieStockAtomique(
    con, trans,
    idProduit, refProduit, quantite,
    nomCaissier,          // utilisateur
    "VENTE",
    null,                 // emplacement (si tu veux, passe une valeur)
    null                  // remarques
);

                            _venteRepo.InsertDetail(con, trans,
                                idVente, idProduit, nomProduit, quantite, prixUnitaire,
                                refProduit, remise, tva, montant,
                                devise?.Trim(), nomCaissier?.Trim());
                        }

                        trans.Commit();
                        return idVente;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        private decimal ParseDecimalFr(object value)
        {
            if (value == null) return 0m;

            string s = value.ToString();
            if (string.IsNullOrWhiteSpace(s)) return 0m;

            decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out decimal v);
            return v;
        }
    }
}
