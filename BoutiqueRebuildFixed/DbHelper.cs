using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public static class DbHelper
    {
        // =========================
        // ✅ TEXT (CommandType.Text implicite)
        // =========================
        public static DataTable Table(string cs, string sql, params SqlParameter[] prms)
            => Table(cs, sql, CommandType.Text, 0, prms);

        public static object Scalar(string cs, string sql, params SqlParameter[] prms)
            => Scalar(cs, sql, CommandType.Text, 0, prms);

        public static int Exec(string cs, string sql, params SqlParameter[] prms)
            => Exec(cs, sql, CommandType.Text, 0, prms);

        // TEXT avec timeout
        public static DataTable Table(string cs, string sql, int timeoutSeconds, params SqlParameter[] prms)
            => Table(cs, sql, CommandType.Text, timeoutSeconds, prms);

        public static object Scalar(string cs, string sql, int timeoutSeconds, params SqlParameter[] prms)
            => Scalar(cs, sql, CommandType.Text, timeoutSeconds, prms);

        public static int Exec(string cs, string sql, int timeoutSeconds, params SqlParameter[] prms)
            => Exec(cs, sql, CommandType.Text, timeoutSeconds, prms);

        // =========================
        // ✅ STORED PROCEDURE (CommandType.StoredProcedure implicite)
        // =========================
        public static DataTable TableSp(string cs, string spName, params SqlParameter[] prms)
            => Table(cs, spName, CommandType.StoredProcedure, 0, prms);

        public static object ScalarSp(string cs, string spName, params SqlParameter[] prms)
            => Scalar(cs, spName, CommandType.StoredProcedure, 0, prms);

        public static int ExecSp(string cs, string spName, params SqlParameter[] prms)
            => Exec(cs, spName, CommandType.StoredProcedure, 0, prms);

        // SP avec timeout
        public static DataTable TableSp(string cs, string spName, int timeoutSeconds, params SqlParameter[] prms)
            => Table(cs, spName, CommandType.StoredProcedure, timeoutSeconds, prms);

        public static object ScalarSp(string cs, string spName, int timeoutSeconds, params SqlParameter[] prms)
            => Scalar(cs, spName, CommandType.StoredProcedure, timeoutSeconds, prms);

        public static int ExecSp(string cs, string spName, int timeoutSeconds, params SqlParameter[] prms)
            => Exec(cs, spName, CommandType.StoredProcedure, timeoutSeconds, prms);

        // Compat: certains de tes forms appellent NonQuery(...)
        public static int NonQuery(string cs, string sqlOrSp, CommandType type, params SqlParameter[] prms)
            => Exec(cs, sqlOrSp, type, 0, prms);

        // =========================
        // ✅ CORE
        // =========================
        public static DataTable Table(string cs, string sql, CommandType type, int timeoutSeconds, params SqlParameter[] prms)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = type;
                if (timeoutSeconds > 0) cmd.CommandTimeout = timeoutSeconds;
                if (prms != null && prms.Length > 0) cmd.Parameters.AddRange(prms);

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static object Scalar(string cs, string sql, CommandType type, int timeoutSeconds, params SqlParameter[] prms)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = type;
                if (timeoutSeconds > 0) cmd.CommandTimeout = timeoutSeconds;
                if (prms != null && prms.Length > 0) cmd.Parameters.AddRange(prms);

                con.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static int Exec(string cs, string sql, CommandType type, int timeoutSeconds, params SqlParameter[] prms)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = type;
                if (timeoutSeconds > 0) cmd.CommandTimeout = timeoutSeconds;
                if (prms != null && prms.Length > 0) cmd.Parameters.AddRange(prms);

                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}