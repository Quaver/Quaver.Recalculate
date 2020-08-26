using Quaver.Recalculate.Config;

namespace Quaver.Recalculate.Database
{
    public static class SQL
    {
        /// <summary>
        /// </summary>
        public static string ConnString { get; set; } 

        /// <summary>
        /// </summary>
        /// <param name="config"></param>
        public static void Initialize()
        {
            ConnString = $"Server={Configuration.Instance.SQLHost};" +
                         $"User ID={Configuration.Instance.SQLUsername};" +
                         $"Password={Configuration.Instance.SQLPassword};" +
                         $"Database={Configuration.Instance.SQLDatabase}";            
        }
    }
}