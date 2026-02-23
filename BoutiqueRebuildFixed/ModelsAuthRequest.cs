using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed.Models
{
    public enum AuthScope
    {
        Entreprise,
        Region,
        Magasin,
        PostePOS
    }

    public class AuthRequest
    {
        public string ActionCode { get; set; }     // ex: "USER_DISABLE", "OPEN_MODULE_btnComptables"
        public string Title { get; set; }          // ex: "Désactivation utilisateur"
        public string Reference { get; set; }      // ex: "USER:12"
        public string Details { get; set; }        // détails audit

        public bool AlwaysSignature { get; set; }  // forcer signature

        // ✅ grande entreprise (optionnel)
        public int? IdEntreprise { get; set; }
        public int? IdMagasin { get; set; }
        public int? IdRegion { get; set; }
        public int? IdPOS { get; set; }
        public AuthScope Scope { get; set; } = AuthScope.Magasin;

        // ✅ actions sensibles (optionnel)
        public decimal? Amount { get; set; }       // remises/retours/paiements
        public int RiskLevel { get; set; } = 0;    // 0 normal, 1 moyen, 2 critique

        public int? TargetId { get; set; }         // id user/employe/vente

        public int? IdPoste { get; set; }
    }
}
