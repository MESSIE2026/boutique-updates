using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class MarketingRowDto
    {
        public int Id { get; set; }
        public string NomCampagne { get; set; }
        public string TypeCampagne { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public decimal Budget { get; set; }
        public string Statut { get; set; }

        // Stats (LEFT JOIN)
        public int Vues { get; set; }
        public int Messages { get; set; }
        public int Spectateurs { get; set; }
        public decimal BudgetQuotidien { get; set; }
        public int NombreVentes { get; set; }
        public decimal MontantVendus { get; set; }
        public string Devise { get; set; }
    }
}