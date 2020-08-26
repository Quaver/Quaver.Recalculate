using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Maps.Processors.Rating;
using Quaver.Recalculate.Config;
using Quaver.Recalculate.Database;
using Quaver.Recalculate.Maps;
using SimpleLogger;

namespace Quaver.Recalculate.Scores
{
    public static class ScoreRecalculator
    {
        public static void Run()
        {
            Configuration.Load();
            SQL.Initialize();
            
            Logger.Log(Logger.Level.Info, $"Starting score recalculator task...");
            Logger.Log(Logger.Level.Info, $"Difficulty Processor Version: `{DifficultyProcessorKeys.Version}`");
            Logger.Log(Logger.Level.Info, $"Performance Rating Processor Version: `{RatingProcessorKeys.Version}`");

            MapCache.DownloadAllMaps();
            RetrieveOutdatedScores();
        }

        /// <summary>
        ///     Retrieves personal best scores that are outdated in terms of performance rating and difficulty rating.
        /// </summary>
        private static void RetrieveOutdatedScores()
        {
            Logger.Log(Logger.Level.Info, $"Retrieving scores with outdated versions...");

            Task.Run(async () =>
            {
                var scores = new List<Score>();
                
                using (var conn = new MySqlConnection(SQL.ConnString))
                {
                    await conn.OpenAsync();
                    Logger.Log(Logger.Level.Fine, $"Database connection has been opened!");
                    
                    var cmd = new MySqlCommand()
                    {
                        Connection = conn,
                        CommandText = $"SELECT s.id, s.mods, s.accuracy, m.id AS map_id " +
                                      $"FROM scores s " + 
                                      $"INNER JOIN maps m ON m.md5 = s.map_md5 " + 
                                      $"WHERE personal_best = 1 AND failed = 0 " +
                                      $"AND (difficulty_processor_version IS NULL OR difficulty_processor_version <> @d " +
                                            $"OR performance_processor_version IS NULL OR performance_processor_version <> @p)"
                    };

                    await cmd.PrepareAsync();
                    cmd.Parameters.AddWithValue("d", DifficultyProcessorKeys.Version);
                    cmd.Parameters.AddWithValue("p", RatingProcessorKeys.Version);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            Logger.Log(Logger.Level.Warning, $"No outdated scores were found in the database!");
                            await conn.CloseAsync();
                            return;
                        }

                        while (reader.Read())
                        {
                            scores.Add(new Score()
                            {
                                Id = reader.GetInt32(0),
                                Mods = (ModIdentifier) reader.GetInt64(1),
                                Accuracy = reader.GetDouble(2),
                                MapId = reader.GetInt32(3)
                            });
                        }
                    }
                    
                    Logger.Log(Logger.Level.Fine, $"Found: {scores.Count} outdated scores!");
                    await RecalculateScores(conn, scores);
                    await conn.CloseAsync();
                }
            }).Wait();
        }

        /// <summary>
        ///     Handles the actual recalculation of all scores
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="scores"></param>
        /// <returns></returns>
        private static async Task RecalculateScores(MySqlConnection conn, List<Score> scores)
        {
            var done = 0;

            foreach (var score in scores)
            {
                try
                {
                    var map = MapCache.Fetch(score.MapId);

                    if (map == null)
                    {
                        done++;
                        Console.WriteLine($"{GetProgress(done, scores)} Unable to fetch map file: {score.MapId}.qua");
                        return;
                    }
                
                    var difficulty = map.SolveDifficulty(score.Mods);
                    var rating = new RatingProcessorKeys(difficulty.OverallDifficulty).CalculateRating(score.Accuracy);
                
                    var cmd = new MySqlCommand()
                    {
                        Connection = conn,
                        CommandText = $"UPDATE scores SET performance_rating = @pr, performance_processor_version = @pv, " +
                                      $"difficulty_processor_version = @d WHERE id = @id"
                    };

                    await cmd.PrepareAsync();
                    cmd.Parameters.AddWithValue("pr", rating);
                    cmd.Parameters.AddWithValue("pv", RatingProcessorKeys.Version);
                    cmd.Parameters.AddWithValue("d", DifficultyProcessorKeys.Version);
                    cmd.Parameters.AddWithValue("id", score.Id);

                    await cmd.ExecuteNonQueryAsync();

                    done++;
                    
                    Console.WriteLine($"{GetProgress(done, scores)} Score Id: {score.Id} | Perf. Rating: {rating:0.00} " +
                                      $"| Difficulty: {difficulty.OverallDifficulty:0.00}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        private static string GetProgress(int done, List<Score> scores) 
            => $"[{done}/{scores.Count} - {done / (float) scores.Count * 100f:0.00}%]";
    }
}