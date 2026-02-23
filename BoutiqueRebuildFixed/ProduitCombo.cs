using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    internal class ProduitCombo
    {
        public int ID { get; set; }
        public string Nom { get; set; }
        public string Ref { get; set; }
        public decimal Prix { get; set; }

        public override string ToString()
        {
            return Nom; // affiché dans le ComboBox
        }
    }
}
