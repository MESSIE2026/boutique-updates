using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed.Models
{
    public class AuthResult
    {
        public bool Allowed { get; set; }
        public bool UsedSignature { get; set; }
        public string ManagerName { get; set; }
        public string ManagerRole { get; set; }
        public string DenyReason { get; set; }

        public static AuthResult Ok() => new AuthResult { Allowed = true };
        public static AuthResult Deny(string reason) => new AuthResult { Allowed = false, DenyReason = reason };
    }
}