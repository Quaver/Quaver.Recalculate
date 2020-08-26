using System;
using Quaver.Recalculate.Scores;
using SimpleLogger;
using SimpleLogger.Logging.Handlers;

namespace Quaver.Recalculate
{
    internal static class Program
    {
        /// <summary>
        ///     Main Execution
        /// </summary>
        /// <param name="args"></param>
        internal static void Main(string[] args)
        {
            InitializeLogger();
            ScoreRecalculator.Run();
        }

        /// <summary>
        /// </summary>
        private static void InitializeLogger()
        {
            Logger.LoggerHandlerManager
                .AddHandler(new ConsoleLoggerHandler())
                .AddHandler(new FileLoggerHandler());
        }
    }
}