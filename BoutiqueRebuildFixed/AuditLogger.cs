using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class AuditLogger
    {
        private static string Cs = ConfigSysteme.ConnectionString;

        public static void Log(string typeAction, string description)
        {
            Log(typeAction, description, "Succès");
        }

        public static void Log(string typeAction, string description, string resultat)
        {
            Insert(DateTime.Now, GetUser(), GetIp(), typeAction, description, resultat);
        }

        private static void Insert(DateTime d, string user, string ip,
                                   string action, string desc, string res)
        {
            try
            {
                using (var cn = new SqlConnection(Cs))
                using (var cmd = new SqlCommand(@"
INSERT INTO AuditLog
(DateHeure, Utilisateur, AdresseIP, TypeAction, Description, Resultat)
VALUES (@d,@u,@ip,@a,@desc,@r)", cn))
                {
                    cmd.Parameters.AddWithValue("@d", d);
                    cmd.Parameters.AddWithValue("@u", user);
                    cmd.Parameters.AddWithValue("@ip", ip);
                    cmd.Parameters.AddWithValue("@a", action);
                    cmd.Parameters.AddWithValue("@desc", desc);
                    cmd.Parameters.AddWithValue("@r", res);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        private static string GetUser()
        {
            try { return Environment.UserName; }
            catch { return "SYSTEM"; }
        }

        private static string GetIp()
        {
            try
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch { }
            return "0.0.0.0";
        }
    }
}