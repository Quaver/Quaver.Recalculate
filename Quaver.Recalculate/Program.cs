using System;
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
        internal static void Main()
        {
            InitializeLogger();
            Configuration.Load();
            ScoreRecalculator.Run();
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