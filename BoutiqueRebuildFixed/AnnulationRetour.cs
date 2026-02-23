using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class AnnulationRetour
    {
        public int Id { get; set; }
        public int? IdVente { get; set; }
        public int? IdDetailsVente { get; set; }
        public int? IdPaiementRemboursement { get; set; }

        public string NomClient { get; set; }
        public string NumeroCommande { get; set; }
        public DateTime DateAchat { get; set; }
        public string NomProduit { get; set; }

        // qté vendue (info) / qté retournée (celle qui compte)
        public decimal QuantiteAchetee { get; set; }
        public decimal QuantiteRetournee { get; set; }

        public decimal PrixUnitaire { get; set; }
        public string Devise { get; set; }

        public string MotifRetour { get; set; }
        public string Commentaires { get; set; }
        public string TypeRetour { get; set; } // Remboursement / Echange
        public string Utilisateur { get; set; }

        // ✅ pour PDF/affichage (sans écrire en DB)
        public decimal PrixTotalCalcule
        {
            get { return Math.Round(PrixUnitaire * QuantiteRetournee, 2); }
        }
    }
}