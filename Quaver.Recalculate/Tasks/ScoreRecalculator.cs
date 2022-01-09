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

namespace Quaver.Recalculate.Tasks
{
    public static class ScoreRecalculator
    {
        public static void Run()
        {
            var users = GetUserIds();

            foreach (var user in users)
                RecalculateUserScores(user, users.Count);
        }

        private static List<int> GetUserIds()
        {
            Logger.Log($"Fetching user ids from the database...");
            
            var users = new List<int>();

            using var conn = new MySqlConnection(SqlDatabase.GetConnString(Configuration.Instance));
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT id FROM users ORDER BY id ASC";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        users.Add(reader.GetInt32(0));
                }
            }

            Logger.Log($"Found {users.Count} users in the database!");
            return users;
        }

        private static void RecalculateUserScores(int userId, int totalUsers)
        {
            Logger.Log($"Recalculating scores for user: {userId}...");
            
            var scores = FetchUserScores(userId);
            var sum = 0;
            
            Parallel.ForEach(scores, score =>
            {
                var map = MapCache.Fetch(score.MapId);

                if (map == null)
                {
                    Console.WriteLine($"Skipping score: {score}. Failed to retrieve map!");
                    return;
                }
                
                var diff = map.SolveDifficulty(score.Mods, true);
                var rating = new RatingProcessorKeys(diff.OverallDifficulty).CalculateRating(score.Accuracy);

                using var conn = new MySqlConnection(SqlDatabase.GetConnString(Configuration.Instance));
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = "UPDATE scores SET performance_rating = @r, difficulty_processor_version = @d, " + 
                                      "performance_processor_version = @p " +
                                      "WHERE id = @i";
                    
                    cmd.Parameters.AddWithValue("@r", rating);
                    cmd.Parameters.AddWithValue("@d", DifficultyProcessorKeys.Version);
                    cmd.Parameters.AddWithValue("@p", RatingProcessorKeys.Version);
                    cmd.Parameters.AddWithValue("@i", score.Id);
                    
                    cmd.ExecuteNonQuery();
                }
                
                sum++;
                Console.WriteLine($"[{userId}/{totalUsers}] [{(float)sum / scores.Count:0.00%}%] #{score.Id} -> {rating}");
            });
        }

        private static List<Score> FetchUserScores(int id)
        {
            Logger.Log($"Fetching all scores for user: {id}...");

            var scores = new List<Score>();
            
            using var conn = new MySqlConnection(SqlDatabase.GetConnString(Configuration.Instance));
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT s.id, s.mods, s.accuracy, m.id AS map_id " +
                                  "FROM scores s " +
                                  "INNER JOIN maps m ON m.md5 = s.map_md5 " +
                                  "WHERE s.failed = 0 " +
                                  "AND s.user_id = @u " +
                                  "AND (s.difficulty_processor_version != @d OR s.performance_processor_version != @p)";

                cmd.Prepare();
                cmd.Parameters.AddWithValue("u", id);
                cmd.Parameters.AddWithValue("d", DifficultyProcessorKeys.Version);
                cmd.Parameters.AddWithValue("p", RatingProcessorKeys.Version);
                
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Logger.Log($"No scores found for user: {id}.");
                        return scores;
                    }

                    while (reader.Read())
                    {
                        scores.Add(new Score
                        {
                            Id = reader.GetInt32(0),
                            Mods = (ModIdentifier) reader.GetInt64(1),
                            Accuracy = reader.GetDouble(2),
                            MapId = reader.GetInt32(3)
                        });
                    }
                }
            }

            Logger.Log($"Found {scores.Count} scores form user: {id}!");
            return scores;
        }
    }
}