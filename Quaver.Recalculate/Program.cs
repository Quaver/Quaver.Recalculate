using System;
using System.Net;
using Quaver.Recalculate.Config;
using Quaver.Recalculate.Tasks;
using SimpleLogger;
using SimpleLogger.Logging.Handlers;

namespace Quaver.Recalculate
{
    internal static class Program
    {
        /// <summary>
        ///     Main Execution
        /// </summary>
        internal static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine($"Please provide a task to run: `scores`, `maps`");
                return;
            }
            
            ServicePointManager.DefaultConnectionLimit = 100;
            InitializeLogger();
            Configuration.Load();

            switch (args[0].ToLower())
            {
                case "scores":
                    ScoreRecalculator.Run();
                    break;
                case "maps":
                    MapRecalculator.Run();
                    break;
            }
        }

        /// <summary>
        /// </summary>
        private static void InitializeLogger()
        {
            Logger.LoggerHandlerManager
                .AddHandler(new ConsoleLoggerHandler());
        }
    }
}