using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using Quaver.Recalculate.Config;
using Quaver.Recalculate.Database;
using Quaver.Recalculate.Maps;
using SimpleLogger;

namespace Quaver.Recalculate.Tasks
{
    public static class MapRecalculator
    {
        public static void Run()
        {
            var maps = GetMapIds();
            maps.ForEach(RecalculateMap);
        }

        private static List<int> GetMapIds()
        {
            Logger.Log($"Fetching map ids from the database...");

            var maps = new List<int>();
            
            using var conn = new MySqlConnection(SqlDatabase.GetConnString(Configuration.Instance));
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT id FROM maps WHERE mapset_id != -1 ORDER BY id ASC";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        maps.Add(reader.GetInt32(0));
                }
            }

            Logger.Log($"Found {maps.Count} users in the database!");
            return maps;
        }

        private static void RecalculateMap(int id)
        {
            var map = MapCache.Fetch(id);

            if (map == null)
            {
                Console.WriteLine($"Could not fetch map: {id}");
                return;
            }

            var diff = map.SolveDifficulty().OverallDifficulty;
            
            using var conn = new MySqlConnection(SqlDatabase.GetConnString(Configuration.Instance));
            using var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = "UPDATE maps SET difficulty_rating = @d WHERE id = @i";
            cmd.Parameters.AddWithValue("@d", diff);
            cmd.Parameters.AddWithValue("@i", id);
            cmd.ExecuteNonQuery();
            
            Console.WriteLine($"#{id} -> {diff}");
        }
    }
}