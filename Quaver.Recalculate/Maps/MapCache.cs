using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MySqlConnector;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Recalculate.Database;
using Quaver.Recalculate.Scores;
using SimpleLogger;

namespace Quaver.Recalculate.Maps
{
    public static class MapCache
    {
        private static string Dir => $"{Directory.GetCurrentDirectory()}/maps";

        private static Dictionary<int, Qua> QuaCache { get; } = new Dictionary<int, Qua>();

        public static Qua Fetch(int id)
        {
            Directory.CreateDirectory(Dir);
            
            var filePath = $"{Dir}/{id}.qua";

            try
            {
                if (QuaCache.ContainsKey(id))
                    return QuaCache[id];
                
                if (File.Exists(filePath))
                {
                    var qua = Qua.Parse(filePath);
                    
                    if (!QuaCache.ContainsKey(id))
                        QuaCache.Add(id, qua);
                    
                    return qua;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            return null;
        }

        public static void DownloadAllMaps()
        {
            Logger.Log(Logger.Level.Info, $"Caching all maps...");
            
            Task.Run(async () =>
            {
                var maps = new List<int>();
                
                using (var conn = new MySqlConnection(SQL.ConnString))
                {
                    await conn.OpenAsync();
                    Logger.Log(Logger.Level.Fine, $"Database connection has been opened!");
                    
                    var cmd = new MySqlCommand()
                    {
                        Connection = conn,
                        CommandText = $"SELECT id FROM maps"
                    };
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            Logger.Log(Logger.Level.Warning, $"No maps were found int the database!");
                            await conn.CloseAsync();
                            return;
                        }

                        while (reader.Read())
                            maps.Add(reader.GetInt32(0));
                    }
                    
                    await conn.CloseAsync();
                    
                    DownloadAllMaps(maps);
                }
            }).Wait();
        }
        
        private static void DownloadAllMaps(List<int> maps)
        {
            Parallel.ForEach(maps, async map =>
            {
                var filePath = $"{Dir}/{map}.qua";
                
                try
                {
                    if (File.Exists(filePath))
                        return;

                    using (var client = new WebClient())
                        client.DownloadFile($"https://api.quavergame.com/d/web/map/{map}", filePath);
                }
                catch (Exception e)
                {
                    File.WriteAllText(filePath, "");
                    Console.WriteLine($"Couldn't download file: {map}");
                }
            });
        }
    }
}