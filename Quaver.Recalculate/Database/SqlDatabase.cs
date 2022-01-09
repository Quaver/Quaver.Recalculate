using MySqlConnector;
using Quaver.Recalculate.Config;

namespace Quaver.Recalculate.Database
{
    public static class SqlDatabase
    {
        public static string GetConnString(Configuration config)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = config.SqlHost,
                Port = 3306,
                UserID = config.SqlUsername,
                Password = config.SqlPassword,
                Database = config.SqlDatabase
            };

            return builder.ConnectionString;
        }
    }
}