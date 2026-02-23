using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public sealed class OrdonnanceVenteDTO
    {
        public string Numero { get; set; }
        public string Prescripteur { get; set; }
        public DateTime DateOrdonnance { get; set; } = DateTime.Today;
        public string Patient { get; set; }
        public string Note { get; set; }
        public string ScanPath { get; set; }
        public string PdfPath { get; set; }
        public string CodeFacture { get; set; }
        public string CodeCarteClient { get; set; }
        public List<OrdonnanceLigneDTO> Lignes { get; set; } = new List<OrdonnanceLigneDTO>();
    }

    public sealed class OrdonnanceLigneDTO
    {
        public int IdProduit { get; set; }

        // ✅ On garde seulement le NOM (Article)
        public string NomProduit { get; set; }

        public int Qte { get; set; }

        public decimal PU { get; set; }
        public string Devise { get; set; }

        public decimal Total => Math.Round(PU * Qte, 2);
    }
}
