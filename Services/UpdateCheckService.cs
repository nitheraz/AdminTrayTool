using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdminTrayTool
{
    public static class UpdateCheckService
    {
        private const string LatestReleaseApiUrl =
            "https://api.github.com/repos/nitheraz/AdminTrayTool/releases/latest";

        public static async Task CheckForUpdateAsync(string currentVersion, bool showUpToDateMessage = false)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("AdminTrayTool");

                string json = await client.GetStringAsync(LatestReleaseApiUrl);

                using var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                string? tagName = root.TryGetProperty("tag_name", out var tagEl) ? tagEl.GetString() : null;
                string? htmlUrl = root.TryGetProperty("html_url", out var urlEl) ? urlEl.GetString() : null;

                if (string.IsNullOrWhiteSpace(tagName))
                    return;

                string latestVersion = tagName.TrimStart('v', 'V');

                if (IsNewerVersion(latestVersion, currentVersion))
                {
                    DialogResult result = MessageBox.Show(
                        $"A newer version is available.\n\n" +
                        $"Installed version: {currentVersion}\n" +
                        $"Latest version: {latestVersion}\n\n" +
                        "Open the release page to download it?",
                        "Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes && !string.IsNullOrWhiteSpace(htmlUrl))
                    {
                        Process.Start(new ProcessStartInfo { FileName = htmlUrl, UseShellExecute = true });
                    }
                }
                else if (showUpToDateMessage)
                {
                    MessageBox.Show(
                        $"You're running the latest version ({currentVersion}).",
                        "No Updates Available",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                if (showUpToDateMessage)
                {
                    MessageBox.Show(
                        $"Could not check for updates.\n\n{ex.Message}",
                        "Update Check Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                // Silent failure on startup check - don't bother the user with connectivity issues.
            }
        }

        private static bool IsNewerVersion(string latest, string current)
        {
            if (!Version.TryParse(latest, out var latestVer))
                return false;

            if (!Version.TryParse(current, out var currentVer))
                return false;

            return latestVer > currentVer;
        }
    }
}