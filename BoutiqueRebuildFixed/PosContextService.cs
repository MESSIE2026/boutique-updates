using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class PosContextService
    {
        public static bool ChargerContextePOS(out string message)
        {
            message = "";
            string machine = Environment.MachineName;

            try
            {
                using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
                using (var cmd = new SqlCommand(@"
SELECT TOP 1
    p.IdPoste, p.NomPOS,
    m.IdMagasin, m.Nom AS NomMagasin,
    e.IdEntreprise, e.Nom AS NomEntreprise
FROM dbo.PostePOS p
JOIN dbo.Magasin m ON m.IdMagasin = p.IdMagasin
JOIN dbo.Entreprise e ON e.IdEntreprise = p.IdEntreprise
WHERE p.Actif=1 AND p.MachineName=@m;", con))
                {
                    cmd.Parameters.Add("@m", SqlDbType.NVarChar, 120).Value = machine;

                    con.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                        {
                            message = "POS introuvable pour MachineName=" + machine;
                            return false;
                        }

                        AppContext.IdPoste = Convert.ToInt32(rd["IdPoste"]);
                        AppContext.NomPOS = rd["NomPOS"].ToString();

                        AppContext.IdMagasin = Convert.ToInt32(rd["IdMagasin"]);
                        AppContext.NomMagasin = rd["NomMagasin"].ToString();

                        AppContext.IdEntreprise = Convert.ToInt32(rd["IdEntreprise"]);
                        AppContext.NomEntreprise = rd["NomEntreprise"].ToString();

                        AppContext.PosConfigured = true;
                        AppContext.ModeConfigPOS = false;

                        message =
                            $"OK | Machine={machine} | Poste={AppContext.IdPoste}({AppContext.NomPOS}) | " +
                            $"Mag={AppContext.IdMagasin}({AppContext.NomMagasin}) | Ent={AppContext.IdEntreprise}({AppContext.NomEntreprise})";

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                message = "Erreur ChargerContextePOS: " + ex.Message;
                return false;
            }
        }
    }
}