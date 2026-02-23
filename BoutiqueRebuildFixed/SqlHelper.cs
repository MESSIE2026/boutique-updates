using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    internal static class SqlHelper
    {
        public static SqlParameter AddParam(this SqlCommand cmd, string name, SqlDbType type, object value)
        {
            var p = cmd.Parameters.Add(name, type);
            p.Value = value ?? DBNull.Value;
            return p;
        }

        public static SqlParameter AddDecimal(this SqlCommand cmd, string name, decimal value, byte precision = 18, byte scale = 2)
        {
            var p = cmd.Parameters.Add(name, SqlDbType.Decimal);
            p.Precision = precision;
            p.Scale = scale;
            p.Value = value;
            return p;
        }

        public static SqlParameter AddNVarChar(this SqlCommand cmd, string name, string value, int size = 200)
        {
            var p = cmd.Parameters.Add(name, SqlDbType.NVarChar, size);
            p.Value = (object)value ?? DBNull.Value;
            return p;
        }
    }
}