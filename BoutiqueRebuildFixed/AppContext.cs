using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class AppContext
    {
        public static int IdEntreprise { get; set; }
        public static int IdMagasin { get; set; }
        public static int IdPoste { get; set; }

        public static string NomEntreprise { get; set; } = "";
        public static string NomMagasin { get; set; } = "";
        public static string NomPOS { get; set; } = "";

        public static bool PosConfigured { get; set; } = false;
        public static bool ModeConfigPOS { get; set; } = false; // quand POS pas configuré

        public static void Clear()
        {
            IdEntreprise = 0;
            IdMagasin = 0;
            IdPoste = 0;
            NomEntreprise = NomMagasin = NomPOS = "";
        }
    }
}
