using AdminTrayTool.Models;
using System.Diagnostics;

namespace AdminTrayTool.Services
{
    public class GamService
    {
        private static string? SelectedGamPath;

        private static readonly TimeSpan AssetIdCacheMaxAge =
            TimeSpan.FromHours(4);

        // ============================================================
        // GAM INSTALLATION CHECK
        // ============================================================

        public async Task<bool> IsGamInstalledAsync()
        {
            SelectedGamPath = await GetGamExecutablePathAsync();

            return SelectedGamPath != null &&
                   File.Exists(SelectedGamPath);
        }

        public async Task<string?> GetGamPathAsync()
        {
            return await GetGamExecutablePathAsync();
        }

        public async Task<GamResult> CheckGamHealthAsync()
        {
            if (!await IsGamInstalledAsync())
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error = "GAM is not installed or not detected."
                };
            }

            return await RunGamAsync("version");
        }

        // ============================================================
        // GET CHROMEBOOK INFORMATION
        // ============================================================

        public async Task<(bool Success, ChromebookInfo? Device, string Error)>
            GetChromebookInfoAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return (
                    false,
                    null,
                    "A Chromebook serial number is required.");
            }

            serialNumber = serialNumber.Trim();

            GamResult result =
                await RunGamAsync(
                    $"info cros cros_sn {QuoteArgument(serialNumber)}");

            if (!result.Success)
            {
                string error =
                    !string.IsNullOrWhiteSpace(result.Error)
                        ? result.Error!
                        : result.Output ??
                          "GAM returned no output.";

                return (
                    false,
                    null,
                    error);
            }

            string output =
                result.Output ?? string.Empty;

            /*
             * GAM can return exit code 0 even when the requested
             * Chromebook does not exist. Therefore, a successful GAM
             * process is not enough to consider the device found.
             *
             * Parse the output first and then verify that an actual
             * serial number was returned.
             */
            ChromebookInfo device =
                ParseChromebookInfo(
                    output,
                    string.Empty);

            if (string.IsNullOrWhiteSpace(device.SerialNumber))
            {
                return (
                    false,
                    null,
                    $"No Chromebook found with serial number '{serialNumber}'.");
            }

            if (!string.Equals(
                device.SerialNumber.Trim(),
                serialNumber,
                StringComparison.OrdinalIgnoreCase))
            {
                return (
                    false,
                    null,
                    $"No Chromebook found with serial number '{serialNumber}'.");
            }

            return (
                true,
                device,
                string.Empty);
        }

        // ============================================================
        // GET CHROMEBOOK BY ASSET ID
        // ============================================================

        public async Task<(bool Success, ChromebookInfo? Device, string Error)>
            GetChromebookInfoByAssetIdAsync(
                string assetId,
                bool forceRefresh = false)
        {
            if (string.IsNullOrWhiteSpace(assetId))
            {
                return (
                    false,
                    null,
                    "An Asset ID is required.");
            }

            assetId = assetId.Trim();

            Dictionary<string, string>? map =
                forceRefresh
                    ? null
                    : AssetIdCacheService.TryLoad(
                        AssetIdCacheMaxAge);

            if (map == null)
            {
                GamResult listResult =
                    await RunGamAsync(
                        "print cros fields serialNumber,annotatedAssetId");

                if (!listResult.Success)
                {
                    string error =
                        !string.IsNullOrWhiteSpace(
                            listResult.Error)
                            ? listResult.Error!
                            : listResult.Output ??
                              "GAM returned no output.";

                    return (
                        false,
                        null,
                        error);
                }

                map =
                    BuildAssetIdMapFromCsv(
                        listResult.Output ?? string.Empty);

                AssetIdCacheService.Save(map);
            }

            if (!map.TryGetValue(
                assetId,
                out string? serial) ||
                string.IsNullOrWhiteSpace(serial))
            {
                return (
                    false,
                    null,
                    $"No Chromebook found with Asset ID '{assetId}'.");
            }

            return await GetChromebookInfoAsync(serial);
        }

        private static Dictionary<string, string>
            BuildAssetIdMapFromCsv(string csvOutput)
        {
            var map =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            string[] lines =
                csvOutput.Split(
                    ['\r', '\n'],
                    StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
                return map;

            string[] headers =
                lines[0].Split(',');

            int serialColumnIndex =
                Array.FindIndex(
                    headers,
                    h => string.Equals(
                        h.Trim(),
                        "serialNumber",
                        StringComparison.OrdinalIgnoreCase));

            int assetColumnIndex =
                Array.FindIndex(
                    headers,
                    h => string.Equals(
                        h.Trim(),
                        "annotatedAssetId",
                        StringComparison.OrdinalIgnoreCase));

            if (serialColumnIndex == -1 ||
                assetColumnIndex == -1)
            {
                return map;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] fields =
                    lines[i].Split(',');

                if (assetColumnIndex >= fields.Length ||
                    serialColumnIndex >= fields.Length)
                {
                    continue;
                }

                string rowAssetId =
                    fields[assetColumnIndex]
                        .Trim()
                        .Trim('"');

                string rowSerial =
                    fields[serialColumnIndex]
                        .Trim()
                        .Trim('"');

                if (!string.IsNullOrWhiteSpace(rowAssetId) &&
                    !string.IsNullOrWhiteSpace(rowSerial))
                {
                    map[rowAssetId] = rowSerial;
                }
            }

            return map;
        }

        // ============================================================
        // UPDATE ANNOTATED ASSET ID
        // ============================================================

        public async Task<GamResult> UpdateAnnotatedAssetIdAsync(
            string serialNumber,
            string assetId)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new GamResult
                {
                    Success = false,
                    Error = "Serial number required."
                };
            }

            if (string.IsNullOrWhiteSpace(assetId))
            {
                return new GamResult
                {
                    Success = false,
                    Error = "Asset ID required."
                };
            }

            serialNumber = serialNumber.Trim();
            assetId = assetId.Trim();

            return await RunGamAsync(
                $"update cros cros_sn {QuoteArgument(serialNumber)} " +
                $"annotatedassetid {QuoteArgument(assetId)}");
        }

        // ============================================================
        // DISABLE / ENABLE CHROMEBOOK
        // ============================================================

        public async Task<GamResult> DisableChromebookAsync(
            string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new GamResult
                {
                    Success = false,
                    Error = "Serial number required."
                };
            }

            serialNumber = serialNumber.Trim();

            return await RunGamAsync(
                $"update cros cros_sn {QuoteArgument(serialNumber)} " +
                "action disable");
        }

        public async Task<GamResult> ReenableChromebookAsync(
            string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new GamResult
                {
                    Success = false,
                    Error = "Serial number required."
                };
            }

            serialNumber = serialNumber.Trim();

            return await RunGamAsync(
                $"update cros cros_sn {QuoteArgument(serialNumber)} " +
                "action reenable");
        }

        public async Task<GamResult> PowerwashChromebookAsync(
            string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new GamResult
                {
                    Success = false,
                    Error = "Serial number required."
                };
            }

            serialNumber = serialNumber.Trim();

            return await RunGamAsync(
                $"issuecommand cros cros_sn {QuoteArgument(serialNumber)} " +
                "command remote_powerwash doit");
        }

        public async Task<GamResult> ClearChromebookProfilesAsync(
            string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new GamResult
                {
                    Success = false,
                    Error = "Serial number required."
                };
            }

            serialNumber = serialNumber.Trim();

            return await RunGamAsync(
                $"issuecommand cros cros_sn {QuoteArgument(serialNumber)} " +
                "command wipe_users doit");
        }

        public async Task<GamResult> MoveChromebookToOuAsync(
            string serialNumber,
            string orgUnitPath)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new GamResult
                {
                    Success = false,
                    Error = "Serial number required."
                };
            }

            if (string.IsNullOrWhiteSpace(orgUnitPath))
            {
                return new GamResult
                {
                    Success = false,
                    Error = "Organizational Unit path required."
                };
            }

            serialNumber = serialNumber.Trim();
            orgUnitPath = orgUnitPath.Trim();

            return await RunGamAsync(
                $"update org {QuoteArgument(orgUnitPath)} " +
                $"move cros_sn {QuoteArgument(serialNumber)}");
        }

        // ============================================================
        // GET ORGANISATIONAL UNITS
        // ============================================================

        public async Task<(bool Success, List<string> OrgUnitPaths, string Error)>
            GetAllOrgUnitPathsAsync()
        {
            GamResult result =
                await RunGamAsync("print orgs");

            if (!result.Success)
            {
                string error =
                    !string.IsNullOrWhiteSpace(result.Error)
                        ? result.Error!
                        : result.Output ??
                          "GAM returned no output.";

                return (
                    false,
                    new List<string>(),
                    error);
            }

            var orgUnitPaths =
                ParseOrgUnitPathsFromCsv(
                    result.Output ?? string.Empty);

            return (
                true,
                orgUnitPaths,
                string.Empty);
        }

        // ============================================================
        // ADD USER TO GROUP
        // ============================================================

        public async Task<GamResult> AddUserToGroupAsync(
            string groupEmail,
            string userEmail)
        {
            if (string.IsNullOrWhiteSpace(groupEmail))
            {
                return new GamResult
                {
                    Success = false,
                    Error = "Group email is required."
                };
            }

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return new GamResult
                {
                    Success = false,
                    Error = "Staff email is required."
                };
            }

            groupEmail = groupEmail.Trim();
            userEmail = userEmail.Trim();

            return await RunGamAsync(
                $"update group {QuoteArgument(groupEmail)} " +
                $"add member {QuoteArgument(userEmail)}");
        }

        public async Task<(bool Success, List<string> GroupEmails, string Error)>
            GetAllGroupEmailsAsync()
        {
            GamResult result =
                await RunGamAsync("print groups");

            if (!result.Success)
            {
                string error =
                    !string.IsNullOrWhiteSpace(result.Error)
                        ? result.Error!
                        : result.Output ??
                          "GAM returned no output.";

                return (
                    false,
                    new List<string>(),
                    error);
            }

            var emails =
                ParseEmailColumnFromCsv(
                    result.Output ?? string.Empty);

            return (
                true,
                emails,
                string.Empty);
        }

        // ============================================================
        // LIST ALL USERS
        // ============================================================

        public async Task<(bool Success, List<string> UserEmails, string Error)>
            GetAllUserEmailsAsync()
        {
            GamResult result =
                await RunGamAsync("print users");

            if (!result.Success)
            {
                string error =
                    !string.IsNullOrWhiteSpace(result.Error)
                        ? result.Error!
                        : result.Output ??
                          "GAM returned no output.";

                return (
                    false,
                    new List<string>(),
                    error);
            }

            var emails =
                ParseEmailColumnFromCsv(
                    result.Output ?? string.Empty);

            return (
                true,
                emails,
                string.Empty);
        }

        private static List<string>
            ParseEmailColumnFromCsv(string csvOutput)
        {
            var emails =
                new List<string>();

            string[] lines =
                csvOutput.Split(
                    ['\r', '\n'],
                    StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
                return emails;

            string[] headers =
                lines[0].Split(',');

            int emailColumnIndex =
                Array.FindIndex(
                    headers,
                    h =>
                        string.Equals(
                            h.Trim(),
                            "email",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        string.Equals(
                            h.Trim(),
                            "primaryEmail",
                            StringComparison.OrdinalIgnoreCase));

            if (emailColumnIndex == -1)
                return emails;

            for (int i = 1; i < lines.Length; i++)
            {
                string[] fields =
                    lines[i].Split(',');

                if (emailColumnIndex >= fields.Length)
                    continue;

                string email =
                    fields[emailColumnIndex]
                        .Trim()
                        .Trim('"');

                if (!string.IsNullOrWhiteSpace(email))
                {
                    emails.Add(email);
                }
            }

            return emails;
        }

        // ============================================================
        // RUN GAM
        // ============================================================

        private async Task<GamResult> RunGamAsync(
            string arguments)
        {
            string? gamPath =
                await GetGamExecutablePathAsync();

            if (gamPath == null ||
                !File.Exists(gamPath))
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error = "GAM executable not found."
                };
            }

            try
            {
                var startInfo =
                    new ProcessStartInfo
                    {
                        FileName = gamPath,

                        Arguments = arguments,

                        WorkingDirectory =
                            Path.GetDirectoryName(gamPath)
                            ?? AppContext.BaseDirectory,

                        UseShellExecute = false,

                        CreateNoWindow = true,

                        RedirectStandardOutput = true,

                        RedirectStandardError = true
                    };

                using var process =
                    new Process
                    {
                        StartInfo = startInfo
                    };

                process.Start();

                Task<string> outputTask =
                    process.StandardOutput
                        .ReadToEndAsync();

                Task<string> errorTask =
                    process.StandardError
                        .ReadToEndAsync();

                await process.WaitForExitAsync();

                string output =
                    await outputTask;

                string error =
                    await errorTask;

                return new GamResult
                {
                    Success =
                        process.ExitCode == 0,

                    ExitCode =
                        process.ExitCode,

                    Output =
                        output.Trim(),

                    Error =
                        error.Trim()
                };
            }
            catch (Exception ex)
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error =
                        $"Failed to execute GAM:\n\n{ex.Message}"
                };
            }
        }

        // ============================================================
        // GAM EXECUTABLE PATH
        // ============================================================

        private static async Task<string?>
            GetGamExecutablePathAsync()
        {
            if (SelectedGamPath == null)
            {
                SelectedGamPath =
                    await GamLocator.LocateGam();
            }

            return SelectedGamPath;
        }

        // ============================================================
        // PARSE GAM OUTPUT
        // ============================================================

        private static ChromebookInfo ParseChromebookInfo(
            string output,
            string requestedSerial)
        {
            var device =
                new ChromebookInfo
                {
                    SerialNumber = requestedSerial,
                    RawOutput = output
                };

            string[] lines =
                output.Split(
                    ['\r', '\n'],
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawLine in lines)
            {
                string line =
                    rawLine.Trim();

                if (line.StartsWith(
                    "CrOS Device:",
                    StringComparison.OrdinalIgnoreCase))
                {
                    device.DeviceId =
                        line[
                                "CrOS Device:".Length..]
                            .Trim();

                    continue;
                }

                ParseValue(
                    line,
                    "serialNumber:",
                    v => device.SerialNumber = v);

                ParseValue(
                    line,
                    "annotatedAssetId:",
                    v => device.AnnotatedAssetId = v);

                ParseValue(
                    line,
                    "model:",
                    v => device.Model = v);

                ParseValue(
                    line,
                    "macAddress:",
                    v => device.MacAddress = v);

                ParseValue(
                    line,
                    "orgUnitId:",
                    v => device.OrgUnitId = v);

                ParseValue(
                    line,
                    "orgUnitPath:",
                    v => device.OrgUnitPath = v);

                ParseValue(
                    line,
                    "annotatedUser:",
                    v => device.RecentUserEmail = v);

                ParseValue(
                    line,
                    "lastSync:",
                    v => device.LastSync = v);

                ParseValue(
                    line,
                    "status:",
                    v => device.Status = v);
            }

            if (string.IsNullOrWhiteSpace(
                device.SerialNumber))
            {
                device.SerialNumber = requestedSerial;
            }

            device.Model =
                NormalizeChromebookModel(
                    device.Model);

            device.MacAddress =
                FormatMacAddress(
                    device.MacAddress);

            return device;
        }

        // ============================================================
        // PARSE ORGANISATIONAL UNITS
        // ============================================================

        private static List<string>
            ParseOrgUnitPathsFromCsv(
                string csvOutput)
        {
            var orgUnitPaths =
                new List<string>();

            string[] lines =
                csvOutput.Split(
                    ['\r', '\n'],
                    StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
                return orgUnitPaths;

            string[] headers =
                lines[0].Split(',');

            int orgUnitPathColumnIndex =
                Array.FindIndex(
                    headers,
                    h => string.Equals(
                        h.Trim(),
                        "orgUnitPath",
                        StringComparison.OrdinalIgnoreCase));

            if (orgUnitPathColumnIndex == -1)
                return orgUnitPaths;

            for (int i = 1; i < lines.Length; i++)
            {
                string[] fields =
                    lines[i].Split(',');

                if (orgUnitPathColumnIndex >=
                    fields.Length)
                {
                    continue;
                }

                string path =
                    fields[orgUnitPathColumnIndex]
                        .Trim()
                        .Trim('"');

                if (!string.IsNullOrWhiteSpace(path))
                {
                    orgUnitPaths.Add(path);
                }
            }

            return orgUnitPaths;
        }

        // ============================================================
        // NORMALIZE MODEL
        // ============================================================

        private static string NormalizeChromebookModel(
            string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                return string.Empty;

            string cleaned =
                model.Trim();

            if (string.Equals(
                cleaned,
                "Intel(R) N150",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "ASUS Chromebook CR11 (CR1104CTA) / " +
                    "ASUS Chromebook CR12 (CR1204CTA)";
            }

            return cleaned;
        }

        // ============================================================
        // PARSE VALUE
        // ============================================================

        private static void ParseValue(
            string line,
            string key,
            Action<string> setter)
        {
            if (!line.StartsWith(
                key,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string value =
                line[
                        key.Length..]
                    .Trim();

            if (!string.IsNullOrWhiteSpace(value))
            {
                setter(value);
            }
        }

        // ============================================================
        // FORMAT MAC ADDRESS
        // ============================================================

        private static string FormatMacAddress(
            string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
                return string.Empty;

            string cleaned =
                mac.Replace(":", "")
                   .Replace("-", "")
                   .Replace(".", "")
                   .Trim();

            if (cleaned.Length != 12)
                return mac;

            try
            {
                return string.Join(
                    ":",
                    cleaned[..2]
                        .ToUpperInvariant(),

                    cleaned.Substring(2, 2)
                        .ToUpperInvariant(),

                    cleaned.Substring(4, 2)
                        .ToUpperInvariant(),

                    cleaned.Substring(6, 2)
                        .ToUpperInvariant(),

                    cleaned.Substring(8, 2)
                        .ToUpperInvariant(),

                    cleaned.Substring(10, 2)
                        .ToUpperInvariant());
            }
            catch
            {
                return mac;
            }
        }

        // ============================================================
        // QUOTE GAM ARGUMENT
        // ============================================================

        private static string QuoteArgument(
            string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            return
                "\"" +
                value.Replace(
                    "\"",
                    "\\\"") +
                "\"";
        }
    }
}
