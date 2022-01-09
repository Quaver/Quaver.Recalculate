using System;
using System.Drawing;
using System.IO;
using SimpleLogger;

namespace Quaver.Recalculate.Config
{
    public class Configuration
    {
        /// <summary>
        /// </summary>
        public static Configuration Instance { get; private set; }
        
        public string SqlHost { get; }
        
        public string SqlUsername { get; }
        
        public string SqlPassword { get; }
        
        public string SqlDatabase { get; }

        public string APIUrl { get; }
        

        /// <summary>
        ///     The path of the config file.
        /// </summary>
        private static string ConfigPath { get; } = $"./.env";
        
        /// <summary>
        /// </summary>
        public Configuration()
        {
            if (Instance != null)
                return;
            
            Logger.Log($"Loading config at path: `{ConfigPath}`...");

            if (!File.Exists(ConfigPath))
            {
                Logger.Log(Logger.Level.Error, $"No config file found at path: `{ConfigPath}`");
                Environment.Exit(-1);
                return;
            }
                
            DotNetEnv.Env.Load(ConfigPath);

            SqlHost = DotNetEnv.Env.GetString("SQLHost");
            SqlUsername = DotNetEnv.Env.GetString("SQLUsername");
            SqlPassword = DotNetEnv.Env.GetString("SQLPassword");
            SqlDatabase = DotNetEnv.Env.GetString("SQLDatabase");
            APIUrl = DotNetEnv.Env.GetString("APIUrl");
            
            Instance = this;
            
            Logger.Log($"Successfully loaded config file!");
        }
        
        public static Configuration Load() => new Configuration();
    }
}