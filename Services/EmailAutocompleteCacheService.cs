using AdminTrayTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AdminTrayTool.Services
{
    public static class EmailAutocompleteCacheService
    {
        private static string GetCacheFolder()
        {
            string programDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "AdminTrayTool");

            try { Directory.CreateDirectory(programDataDir); } catch { /* ignore */ }

            return programDataDir;
        }

        public static string GetGroupsCachePath() =>
            Path.Combine(GetCacheFolder(), "groupEmailCache.json");

        public static string GetUsersCachePath() =>
            Path.Combine(GetCacheFolder(), "userEmailCache.json");

        /// <summary>
        /// Loads a cached email list if it exists and is younger than maxAge.
        /// Returns null if there's no cache, it's expired, or it can't be read.
        /// </summary>
        public static List<string>? TryLoad(string path, TimeSpan maxAge)
        {
            try
            {
                if (!File.Exists(path))
                    return null;

                string json = File.ReadAllText(path);
                var cache = JsonSerializer.Deserialize<EmailAutocompleteCache>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (cache == null)
                    return null;

                bool expired = DateTime.UtcNow - cache.FetchedAtUtc > maxAge;

                return expired ? null : cache.Emails;
            }
            catch
            {
                return null;
            }
        }

        public static void Save(string path, List<string> emails)
        {
            try
            {
                var cache = new EmailAutocompleteCache
                {
                    FetchedAtUtc = DateTime.UtcNow,
                    Emails = emails
                };

                string json = JsonSerializer.Serialize(
                    cache,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(path, json);
            }
            catch
            {
                // Ignore write errors — caching is a convenience, not critical.
            }
        }
    }
}