using AdminTrayTool.Models;
using System.Text.Json;

namespace AdminTrayTool.Services
{
    public static class AssetIdCacheService
    {
        public static string GetCachePath()
        {
            string programDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "AdminTrayTool");

            try { Directory.CreateDirectory(programDataDir); } catch { /* ignore */ }

            return Path.Combine(programDataDir, "assetIdCache.json");
        }

        public static Dictionary<string, string>? TryLoad(TimeSpan maxAge)
        {
            string path = GetCachePath();

            try
            {
                if (!File.Exists(path))
                    return null;

                string json = File.ReadAllText(path);

                var cache = JsonSerializer.Deserialize<AssetIdCache>(
                    json,
                    DeserializeOptions);

                if (cache == null)
                    return null;

                bool expired = DateTime.UtcNow - cache.FetchedAtUtc > maxAge;

                return expired ? null : cache.AssetIdToSerial;
            }
            catch
            {
                return null;
            }
        }
        private static readonly JsonSerializerOptions DeserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly JsonSerializerOptions SerializeOptions = new()
        {
            WriteIndented = true
        };
        public static void Save(Dictionary<string, string> assetIdToSerial)
        {
            try
            {
                var cache = new AssetIdCache
                {
                    FetchedAtUtc = DateTime.UtcNow,
                    AssetIdToSerial = assetIdToSerial
                };

                string json = JsonSerializer.Serialize(
                    cache,
                    SerializeOptions);

                File.WriteAllText(GetCachePath(), json);
            }
            catch
            {
                // Ignore write errors - caching is a convenience, not critical.
            }
        }
    }
}
