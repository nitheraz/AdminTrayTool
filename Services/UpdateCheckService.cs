using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace AdminTrayTool.Services
{
    public static class UpdateCheckService
    {
        private const string LatestReleaseApiUrl =
            "https://api.github.com/repos/nitheraz/AdminTrayTool/releases/latest";

        private const string UpdateCompletedArgument =
            "--update-complete";

        public static async Task CheckForUpdateAsync(
            string currentVersion,
            bool showUpToDateMessage = false)
        {
            try
            {
                using var client =
                    CreateHttpClient();

                string json =
                    await client.GetStringAsync(
                        LatestReleaseApiUrl);

                using var doc =
                    JsonDocument.Parse(json);

                JsonElement root =
                    doc.RootElement;

                string? tagName =
                    root.TryGetProperty(
                        "tag_name",
                        out var tagEl)
                        ? tagEl.GetString()
                        : null;

                if (string.IsNullOrWhiteSpace(tagName))
                    return;

                string latestVersion =
                    tagName.TrimStart(
                        'v',
                        'V');

                if (!IsNewerVersion(
                    latestVersion,
                    currentVersion))
                {
                    if (showUpToDateMessage)
                    {
                        MessageBox.Show(
                            $"You're running the latest version ({currentVersion}).",
                            "No Updates Available",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }

                    return;
                }

                JsonElement? msiAsset =
                    FindMsiAsset(
                        root,
                        latestVersion);

                if (msiAsset == null)
                {
                    MessageBox.Show(
                        $"Version {latestVersion} is available, " +
                        "but the MSI installer could not be found in the release.",
                        "Update Available",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                JsonElement asset =
                    msiAsset.Value;

                string? downloadUrl =
                    asset.TryGetProperty(
                        "browser_download_url",
                        out var downloadEl)
                        ? downloadEl.GetString()
                        : null;

                string? assetName =
                    asset.TryGetProperty(
                        "name",
                        out var nameEl)
                        ? nameEl.GetString()
                        : null;

                string? digest =
                    asset.TryGetProperty(
                        "digest",
                        out var digestEl)
                        ? digestEl.GetString()
                        : null;

                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    MessageBox.Show(
                        $"Version {latestVersion} is available, " +
                        "but the MSI download URL could not be found.",
                        "Update Available",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                DialogResult result =
                    MessageBox.Show(
                        "A newer version of AdminTrayTool is available.\n\n" +
                        $"Installed version: {currentVersion}\n" +
                        $"Latest version: {latestVersion}\n\n" +
                        "Would you like to download and install it now?",
                        "Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                if (result != DialogResult.Yes)
                    return;

                await DownloadAndInstallAsync(
                    client,
                    downloadUrl,
                    assetName ??
                        $"AdminTrayTool-{latestVersion}.msi",
                    digest,
                    latestVersion);
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
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var client =
                new HttpClient();

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "AdminTrayTool");

            return client;
        }

        private static JsonElement? FindMsiAsset(
            JsonElement root,
            string latestVersion)
        {
            if (!root.TryGetProperty(
                "assets",
                out var assets))
            {
                return null;
            }

            string expectedName =
                $"AdminTrayTool-{latestVersion}.msi";

            foreach (JsonElement asset in
                assets.EnumerateArray())
            {
                if (!asset.TryGetProperty(
                    "name",
                    out var nameElement))
                {
                    continue;
                }

                string? name =
                    nameElement.GetString();

                if (string.Equals(
                    name,
                    expectedName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return asset;
                }
            }

            return null;
        }

        private static async Task DownloadAndInstallAsync(
            HttpClient client,
            string downloadUrl,
            string assetName,
            string? digest,
            string latestVersion)
        {
            string tempDirectory =
                Path.Combine(
                    Path.GetTempPath(),
                    "AdminTrayTool");

            Directory.CreateDirectory(
                tempDirectory);

            string msiPath =
                Path.Combine(
                    tempDirectory,
                    assetName);

            try
            {
                MessageBox.Show(
                    $"AdminTrayTool {latestVersion} will now be downloaded.\n\n" +
                    "The application will close while Windows Installer " +
                    "installs the update.\n\n" +
                    "AdminTrayTool will automatically restart when the update " +
                    "has completed.",
                    "Installing Update",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                using HttpResponseMessage response =
                    await client.GetAsync(
                        downloadUrl,
                        HttpCompletionOption.ResponseHeadersRead);

                response.EnsureSuccessStatusCode();

                await using Stream source =
                    await response.Content.ReadAsStreamAsync();

                await using FileStream destination =
                    new(
                        msiPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None);

                await source.CopyToAsync(
                    destination);

                await destination.FlushAsync();

                if (!string.IsNullOrWhiteSpace(digest))
                {
                    bool verified =
                        await VerifySha256Async(
                            msiPath,
                            digest);

                    if (!verified)
                    {
                        TryDeleteFile(msiPath);

                        MessageBox.Show(
                            "The downloaded installer failed its " +
                            "SHA-256 integrity check.\n\n" +
                            "The update was cancelled.",
                            "Update Verification Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }
                }

                string applicationPath =
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "AdminTrayTool.exe");

                if (!File.Exists(applicationPath))
                {
                    throw new FileNotFoundException(
                        "The AdminTrayTool executable could not be found.",
                        applicationPath);
                }

                StartUpdateHelper(
                    msiPath,
                    applicationPath);

                // The helper now owns the rest of the update process.
                // It waits for MSI to finish and then launches the new
                // AdminTrayTool instance with --update-complete.
                Application.Exit();
            }
            catch
            {
                TryDeleteFile(msiPath);
                throw;
            }
        }

        private static void StartUpdateHelper(
            string msiPath,
            string applicationPath)
        {
            string escapedMsiPath =
                EscapePowerShellString(
                    msiPath);

            string escapedApplicationPath =
                EscapePowerShellString(
                    applicationPath);

            string command =
                "$ErrorActionPreference='Stop'; " +
                "$p=Start-Process " +
                "-FilePath 'msiexec.exe' " +
                $"-ArgumentList '/i \"{escapedMsiPath}\" /passive /norestart' " +
                "-Wait -PassThru; " +
                "if ($p.ExitCode -eq 0 -or $p.ExitCode -eq 3010) { " +
                $"Start-Process -FilePath '{escapedApplicationPath}' " +
                $"-ArgumentList '{UpdateCompletedArgument}' " +
                "}";

            var startInfo =
                new ProcessStartInfo
                {
                    FileName =
                        "powershell.exe",

                    Arguments =
                        "-NoProfile -NonInteractive " +
                        "-ExecutionPolicy Bypass " +
                        $"-WindowStyle Hidden -Command \"{command}\"",

                    UseShellExecute = false,

                    CreateNoWindow = true,

                    WorkingDirectory =
                        AppContext.BaseDirectory
                };

            Process process = Process.Start(startInfo) ?? throw new InvalidOperationException(
                    "The update helper process could not be started.");
            Process? helper =
                process;
        }

        private static string EscapePowerShellString(
            string value)
        {
            return value.Replace(
                "'",
                "''");
        }

        private static async Task<bool> VerifySha256Async(
            string filePath,
            string digest)
        {
            string expectedHash =
                digest;

            if (expectedHash.StartsWith(
                "sha256:",
                StringComparison.OrdinalIgnoreCase))
            {
                expectedHash =
                    expectedHash[
                        "sha256:".Length..];
            }

            expectedHash =
                expectedHash.Trim();

            await using FileStream stream =
                File.OpenRead(filePath);

            byte[] hash =
                await SHA256.HashDataAsync(
                    stream);

            string actualHash =
                Convert.ToHexString(
                    hash);

            return string.Equals(
                actualHash,
                expectedHash,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNewerVersion(
            string latest,
            string current)
        {
            if (!Version.TryParse(
                latest,
                out var latestVer))
            {
                return false;
            }

            if (!Version.TryParse(
                current,
                out var currentVer))
            {
                return false;
            }

            return latestVer > currentVer;
        }

        private static void TryDeleteFile(
            string filePath)
        {
            if (!File.Exists(filePath))
                return;

            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // Ignore cleanup errors.
            }
        }
    }
}
