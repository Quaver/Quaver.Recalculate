using Quaver.Recalculate.Config;

namespace Quaver.Recalculate.Database
{
    public static class SqlDatabase
    {
        public static string GetConnString(Configuration config)
        {
            return $"Server={config.SqlHost};" +
                   $"User ID={config.SqlUsername};" +
                   $"Password={config.SqlPassword};" +
                   $"Database={config.SqlDatabase}";       
        }
    }
}