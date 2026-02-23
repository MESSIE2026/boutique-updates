using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class SecurityBytes
    {
            public static byte[] DbToBytes(object dbVal)
            {
                if (dbVal == null || dbVal == DBNull.Value) return null;

                // SQL types
                if (dbVal is SqlBinary sb) return sb.IsNull ? null : sb.Value;
                if (dbVal is SqlBytes sby) return sby.IsNull ? null : sby.Value;

                // Normal: VARBINARY
                if (dbVal is byte[] b) return b;

                // anciens enregistrements => string (hex 0x..., hex pur, ou base64)
                if (dbVal is string s)
                {
                    s = (s ?? "").Trim();
                    if (s.Length == 0) return null;

                    if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        s = s.Substring(2);

                    bool looksHex = (s.Length % 2 == 0);
                    if (looksHex)
                    {
                        for (int i = 0; i < s.Length; i++)
                        {
                            char c = s[i];
                            bool ok = (c >= '0' && c <= '9') ||
                                      (c >= 'a' && c <= 'f') ||
                                      (c >= 'A' && c <= 'F');
                            if (!ok) { looksHex = false; break; }
                        }
                    }

                    if (looksHex)
                    {
                        var bytes = new byte[s.Length / 2];
                        for (int i = 0; i < bytes.Length; i++)
                            bytes[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
                        return bytes;
                    }

                    try { return Convert.FromBase64String(s); }
                    catch { return null; }
                }

                return null;
            }
        }
    }