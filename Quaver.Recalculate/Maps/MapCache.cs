using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Quaver.API.Maps;
using Quaver.Recalculate.Config;

namespace Quaver.Recalculate.Maps
{
    public class MapCache
    {
        private static string Dir => $"{Directory.GetCurrentDirectory()}/maps";

        private static Dictionary<int, object> IdLocks { get; } = new Dictionary<int, object>();

        public static Qua Fetch(int id)
        {
            Directory.CreateDirectory(Dir);
            
            var filePath = $"{Dir}/{id}.qua";

            try
            {
                lock (IdLocks)
                {
                    if (!IdLocks.ContainsKey(id))
                        IdLocks[id] = new object();
                }

                lock (IdLocks[id])
                {
                    if (File.Exists(filePath)) 
                        return Qua.Parse(filePath, false);
                    
                    using (var client = new WebClient())
                        client.DownloadFile($"{Configuration.Instance.APIUrl}/d/web/map/{id}", filePath);
                }
                
                return Qua.Parse(filePath, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                File.Delete(filePath);
                return null;
            }
        }
    }
}