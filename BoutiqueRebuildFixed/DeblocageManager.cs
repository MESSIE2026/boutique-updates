using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class DeblocageManager
    {
        private static readonly Dictionary<string, DateTime> _unlockUntil =
            new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public static bool EstDebloque(string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode)) return false;

            var key = permissionCode.Trim();
            if (_unlockUntil.TryGetValue(key, out var until))
            {
                if (DateTime.Now <= until) return true;
                _unlockUntil.Remove(key);
            }
            return false;
        }

        public static void Debloquer(string permissionCode, int minutes = 10)
        {
            if (string.IsNullOrWhiteSpace(permissionCode)) return;
            _unlockUntil[permissionCode.Trim()] = DateTime.Now.AddMinutes(minutes);
        }

        public static void ResetDeblocage(string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode)) return;
            _unlockUntil.Remove(permissionCode.Trim());
        }
    }
}