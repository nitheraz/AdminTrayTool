using AdminTrayTool.Models;
using System.Text.Json;

namespace AdminTrayTool.Services
{
    public static class GroupTemplateService
    {
        public static string GetDefaultPath()
        {
            string programDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "AdminTrayTool");

            try { Directory.CreateDirectory(programDataDir); } catch { /* ignore */ }

            return Path.Combine(programDataDir, "groupTemplates.json");
        }

        public static GroupTemplateConfig Load(string? path = null)
        {
            path ??= GetDefaultPath();

            if (!File.Exists(path))
            {
                var def = CreateDefault();
                Save(def, path);
                return def;
            }

            try
            {
                string text = File.ReadAllText(path);
                var cfg = JsonSerializer.Deserialize<GroupTemplateConfig>(
                    text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return cfg ?? CreateDefault();
            }
            catch
            {
                return CreateDefault();
            }
        }

        public static void Save(GroupTemplateConfig config, string? path = null)
        {
            path ??= GetDefaultPath();

            try
            {
                string json = JsonSerializer.Serialize(
                    config,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                File.WriteAllText(path, json);
            }
            catch
            {
                // Ignore write errors — caller can decide how to surface this.
            }
        }

        private static GroupTemplateConfig CreateDefault()
        {
            return new GroupTemplateConfig
            {
                Templates = new List<GroupTemplate>
                {
                    new GroupTemplate
                    {
                        Name = "Primary Staff",
                        Groups = new List<string>
                        {
                            "primary-staff@yourdomain.org",
                            "all-staff@yourdomain.org"
                        }
                    },
                    new GroupTemplate
                    {
                        Name = "Secondary Staff",
                        Groups = new List<string>
                        {
                            "secondary-staff@yourdomain.org",
                            "all-staff@yourdomain.org"
                        }
                    },
                    new GroupTemplate
                    {
                        Name = "School Officer",
                        Groups = new List<string>
                        {
                            "school-officers@yourdomain.org",
                            "all-staff@yourdomain.org"
                        }
                    }
                }
            };
        }
    }
}