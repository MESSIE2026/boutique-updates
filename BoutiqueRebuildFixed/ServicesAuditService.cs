using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed.Services
{
    public static class AuditService
    {
        public static void Success(string action, string details)
        {
            ConfigSysteme.AjouterAuditLog(action, details, "Succès");
        }

        public static void Fail(string action, string details)
        {
            ConfigSysteme.AjouterAuditLog(action, details, "Refus");
        }
    }
}
