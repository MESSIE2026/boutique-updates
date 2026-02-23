using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class PaiementUtils
    {
        public static string NormalizeModePaiement(string mode)
        {
            mode = (mode ?? "").Trim().ToUpperInvariant();

            mode = mode.Replace("È", "E").Replace("É", "E").Replace("Ê", "E").Replace("Ë", "E");

            if (mode == "ESPECES" || mode == "ESPÈCES" || mode == "ESPECE" || mode == "LIQUIDE" || mode == "LIQUIDES")
                return "CASH";

            if (string.IsNullOrWhiteSpace(mode)) return "CASH";
            return mode;
        }
    }
}