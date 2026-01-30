using System.Data;

namespace MRP.Repositories.Ext
{
    internal static class RepositoryExt
    {
        public static IDbCommand BindParam(this IDbCommand cmd, string param, object? value)
        {
            IDataParameter p = cmd.CreateParameter();
            p.ParameterName = param;
            p.Value = value;
            cmd.Parameters.Add(p);

            return cmd;
        }

        public static string GetString(this IDataReader re, string fieldName)
        {
            int idx = re.GetOrdinal(fieldName);
            if (re.IsDBNull(idx)) { return string.Empty; }
            return re.GetString(idx);
        }

        public static int GetInt(this IDataReader re, string fieldName)
        {
            return re.GetInt32(re.GetOrdinal(fieldName));
        }

        public static Guid GetGuid(this IDataReader re, string fieldName)
        {
            int idx = re.GetOrdinal(fieldName);

            if (re.IsDBNull(idx))
                return Guid.Empty;

            object val = re.GetValue(idx);

            // PostgreSQL uuid → Guid
            if (val is Guid g)
                return g;

            // fallback: string → Guid
            if (val is string s)
                return Guid.Parse(s);

            throw new InvalidCastException(
                $"Field '{fieldName}' is not a Guid (actual type: {val.GetType()})"
            );
        }


        public static double GetDouble(this IDataReader re, string fieldName)
        {
            int idx = re.GetOrdinal(fieldName);
            return re.IsDBNull(idx) ? 0.0 : re.GetDouble(idx);
        }

        public static bool GetBool(this IDataReader re, string fieldName)
        {
            return re.GetBoolean(re.GetOrdinal(fieldName));
        }

        public static DateTime GetDateTime(this IDataReader re, string fieldName)
        {
            return re.GetDateTime(re.GetOrdinal(fieldName));
        }

    }

}
