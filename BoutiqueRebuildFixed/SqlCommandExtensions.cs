using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class SqlCommandExtensions
    {
        public static SqlParameter AddParam(this SqlCommand cmd, string name, SqlDbType type, object value)
        {
            var p = cmd.Parameters.Add(name, type);
            p.Value = (value == null) ? (object)DBNull.Value : value;
            return p;
        }

        public static SqlParameter AddNVarChar(this SqlCommand cmd, string name, string value, int size)
        {
            var p = cmd.Parameters.Add(name, SqlDbType.NVarChar, size);
            p.Value = string.IsNullOrEmpty(value) ? (object)DBNull.Value : value;
            return p;
        }

        public static SqlParameter AddInt(this SqlCommand cmd, string name, int value)
        {
            var p = cmd.Parameters.Add(name, SqlDbType.Int);
            p.Value = value;
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

        public static SqlParameter AddDateTime(this SqlCommand cmd, string name, DateTime value)
        {
            var p = cmd.Parameters.Add(name, SqlDbType.DateTime);
            p.Value = value;
            return p;
        }
    }
}