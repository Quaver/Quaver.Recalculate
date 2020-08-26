using System;
using System.Drawing;
using SimpleLogger;

namespace Quaver.Recalculate.Config
{
    public class Configuration
    {
        /// <summary>
        /// </summary>
        public static Configuration Instance { get; private set; }
        
        public string SQLHost { get; }
        
        public string SQLUsername { get; }
        
        public string SQLPassword { get; }
        
        public string SQLDatabase { get; }

        /// <summary>
        ///     The path of the config file.
        /// </summary>
        private static string ConfigPath { get; } = $"./.env";
        
        /// <summary>
        /// </summary>
        public Configuration()
        {
            try
            {
                DotNetEnv.Env.Load(ConfigPath);

                SQLHost = DotNetEnv.Env.GetString("SQLHost");
                SQLUsername = DotNetEnv.Env.GetString("SQLUsername");
                SQLPassword = DotNetEnv.Env.GetString("SQLPassword");
                SQLDatabase = DotNetEnv.Env.GetString("SQLDatabase");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Environment.Exit(-1);
            }

            Logger.Log(Logger.Level.Fine, $"Config file has successfully loaded!");
            Instance = this;
        }

        // ReSharper disable once ObjectCreationAsStatement
        public static void Load() => new Configuration();
    }
}