using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AdminTrayTool
{
    // ================================================================
    // GAM RESULT
    // ================================================================

    public class GamResult
    {
        public bool Success { get; set; }

        public int ExitCode { get; set; }

        public string Output { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;

        public string CombinedOutput
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Error))
                    return Output;

                if (string.IsNullOrWhiteSpace(Output))
                    return Error;

                return Output +
                       Environment.NewLine +
                       Error;
            }
        }
    }

    // ================================================================
    // CHROMEBOOK INFORMATION
    // ================================================================

    public class ChromebookInfo
    {
        // ------------------------------------------------------------
        // Primary identifier
        // ------------------------------------------------------------

        public string SerialNumber { get; set; } = string.Empty;

        // ------------------------------------------------------------
        // Google ChromeOS Device ID
        // ------------------------------------------------------------

        public string DeviceId { get; set; } = string.Empty;

        // ------------------------------------------------------------
        // Google Workspace annotated asset ID
        //
        // GAM returns:
        //
        // annotatedAssetId: A0016931
        // ------------------------------------------------------------

        public string AnnotatedAssetId { get; set; } = string.Empty;

        // ------------------------------------------------------------
        // Compatibility property
        //
        // Existing UI code can continue to use:
        //
        // device.AssetId
        //
        // It actually maps to AnnotatedAssetId.
        // ------------------------------------------------------------

        public string AssetId
        {
            get => AnnotatedAssetId;
            set => AnnotatedAssetId = value;
        }

        // ------------------------------------------------------------
        // Chromebook model
        // ------------------------------------------------------------

        public string Model { get; set; } = string.Empty;

        // ------------------------------------------------------------
        // Wi-Fi MAC address
        // ------------------------------------------------------------

        public string MacAddress { get; set; } = string.Empty;

        // ------------------------------------------------------------
        // Organisation Unit
        // ------------------------------------------------------------

        public string OrgUnitId { get; set; } = string.Empty;

        public string OrgUnitPath { get; set; } = string.Empty;

        // ------------------------------------------------------------
        // Last sync
        // ------------------------------------------------------------

        public string LastSync { get; set; } = string.Empty;

        // ------------------------------------------------------------
        // Device status
        // ------------------------------------------------------------

        public string Status { get; set; } = string.Empty;

        // ------------------------------------------------------------
        // Complete GAM output
        // ------------------------------------------------------------

        public string RawOutput { get; set; } = string.Empty;
    }

    // ================================================================
    // GAM SERVICE
    // ================================================================

    public class GamService
    {
        private static readonly string GamPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "GAM7",
                "gam.exe");

        // ============================================================
        // GAM INSTALLATION
        // ============================================================

        public bool IsGamInstalled()
        {
            return File.Exists(GamPath);
        }

        public string GetGamPath()
        {
            return GamPath;
        }

        // ============================================================
        // GET CHROMEBOOK INFORMATION
        // ============================================================

        public async Task<(bool Success,
                           ChromebookInfo? Device,
                           string Error)>
            GetChromebookInfoAsync(
                string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return (
                    false,
                    null,
                    "A Chromebook serial number is required."
                );
            }

            serialNumber =
                serialNumber.Trim();

            GamResult result =
                await RunGamAsync(
                    $"info cros cros_sn {QuoteArgument(serialNumber)}"
                );

            if (!result.Success)
            {
                string error =
                    !string.IsNullOrWhiteSpace(result.Error)
                        ? result.Error
                        : result.Output;

                return (
                    false,
                    null,
                    error
                );
            }

            ChromebookInfo device =
                ParseChromebookInfo(
                    result.Output,
                    serialNumber
                );

            return (
                true,
                device,
                string.Empty
            );
        }

        // ============================================================
        // UPDATE ANNOTATED ASSET ID
        //
        // Stage 2
        //
        // Updates Google's annotatedAssetId field.
        // ============================================================

        public async Task<GamResult>
            UpdateAnnotatedAssetIdAsync(
                string serialNumber,
                string assetId)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error =
                        "A Chromebook serial number is required."
                };
            }

            if (string.IsNullOrWhiteSpace(assetId))
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error =
                        "An Asset ID is required."
                };
            }

            serialNumber =
                serialNumber.Trim();

            assetId =
                assetId.Trim();

            return await RunGamAsync(
                $"update cros cros_sn " +
                $"{QuoteArgument(serialNumber)} " +
                $"annotatedassetid " +
                $"{QuoteArgument(assetId)}"
            );
        }

        // ============================================================
        // DISABLE CHROMEBOOK
        // ============================================================

        public async Task<GamResult>
            DisableChromebookAsync(
                string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error =
                        "A Chromebook serial number is required."
                };
            }

            serialNumber =
                serialNumber.Trim();

            return await RunGamAsync(
                $"update cros cros_sn " +
                $"{QuoteArgument(serialNumber)} " +
                $"action disable"
            );
        }

        // ============================================================
        // RE-ENABLE CHROMEBOOK
        // ============================================================

        public async Task<GamResult>
            ReenableChromebookAsync(
                string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error =
                        "A Chromebook serial number is required."
                };
            }

            serialNumber =
                serialNumber.Trim();

            return await RunGamAsync(
                $"update cros cros_sn " +
                $"{QuoteArgument(serialNumber)} " +
                $"action reenable"
            );
        }

        // ============================================================
        // TEST GAM / GOOGLE WORKSPACE ACCESS
        // ============================================================

        public async Task<GamResult> TestConnectionAsync()
        {
            return await RunGamAsync("info currentprojectid");
        }

        // ============================================================
        // RUN GAM
        // ============================================================

        private async Task<GamResult>
            RunGamAsync(
                string arguments)
        {
            if (!IsGamInstalled())
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error =
                        $"GAM could not be found at:" +
                        Environment.NewLine +
                        GamPath
                };
            }

            try
            {
                var startInfo =
                    new ProcessStartInfo
                    {
                        FileName = GetGamExecutablePath(),

                        Arguments =
                            arguments,

                        WorkingDirectory =
                            Path.GetDirectoryName(
                                GamPath)
                            ?? @"C:\GAM7",

                        UseShellExecute = false,

                        CreateNoWindow = true,

                        RedirectStandardOutput = true,

                        RedirectStandardError = true
                    };

                using var process =
                    new Process
                    {
                        StartInfo =
                            startInfo
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
                        $"Failed to execute GAM:" +
                        Environment.NewLine +
                        Environment.NewLine +
                        ex.Message
                };
            }
        }

        private static string GetGamExecutablePath()
        {
            string bundledGam =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "GAM7",
                    "gam.exe");

            if (File.Exists(bundledGam))
            {
                return bundledGam;
            }

            // Fallback for development/testing if the bundled
            // GAM executable isn't present.
            string systemGam = "gam.exe";

            return systemGam;
        }

        // ============================================================
        // PARSE GAM OUTPUT
        // ============================================================

        private static ChromebookInfo
            ParseChromebookInfo(
                string output,
                string requestedSerial)
        {
            var device =
                new ChromebookInfo
                {
                    SerialNumber =
                        requestedSerial,

                    RawOutput =
                        output
                };

            string[] lines =
                output.Split(
                    new[]
                    {
                        '\r',
                        '\n'
                    },
                    StringSplitOptions.RemoveEmptyEntries
                );

            foreach (string rawLine in lines)
            {
                string line =
                    rawLine.Trim();

                // ----------------------------------------------------
                // Google Device ID
                // ----------------------------------------------------

                if (line.StartsWith(
                    "CrOS Device:",
                    StringComparison.OrdinalIgnoreCase))
                {
                    device.DeviceId =
                        line.Substring(
                            "CrOS Device:".Length)
                            .Trim();

                    continue;
                }

                // ----------------------------------------------------
                // Serial Number
                // ----------------------------------------------------

                ParseValue(
                    line,
                    "serialNumber:",
                    value =>
                        device.SerialNumber = value
                );

                // ----------------------------------------------------
                // Annotated Asset ID
                // ----------------------------------------------------

                ParseValue(
                    line,
                    "annotatedAssetId:",
                    value =>
                        device.AnnotatedAssetId = value
                );

                // ----------------------------------------------------
                // Model
                // ----------------------------------------------------

                ParseValue(
                    line,
                    "model:",
                    value =>
                        device.Model = value
                );

                // ----------------------------------------------------
                // MAC Address
                // ----------------------------------------------------

                ParseValue(
                    line,
                    "macAddress:",
                    value =>
                        device.MacAddress = value
                );

                // ----------------------------------------------------
                // Organisation Unit ID
                // ----------------------------------------------------

                ParseValue(
                    line,
                    "orgUnitId:",
                    value =>
                        device.OrgUnitId = value
                );

                // ----------------------------------------------------
                // Organisation Unit Path
                // ----------------------------------------------------

                ParseValue(
                    line,
                    "orgUnitPath:",
                    value =>
                        device.OrgUnitPath = value
                );

                // ----------------------------------------------------
                // Last Sync
                // ----------------------------------------------------

                ParseValue(
                    line,
                    "lastSync:",
                    value =>
                        device.LastSync = value
                );

                // ----------------------------------------------------
                // Status
                // ----------------------------------------------------

                ParseValue(
                    line,
                    "status:",
                    value =>
                        device.Status = value
                );
            }

            // --------------------------------------------------------
            // Safety fallback for serial
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                device.SerialNumber))
            {
                device.SerialNumber =
                    requestedSerial;
            }

            // --------------------------------------------------------
            // MODEL CORRECTION
            //
            // GAM can sometimes report the CPU instead of the actual
            // Chromebook model.
            //
            // Example:
            //
            // model: Intel(R) N150
            //
            // For your ASUS CR11 / CR12 devices, display the proper
            // Chromebook model instead.
            // --------------------------------------------------------

            device.Model =
                NormalizeChromebookModel(
                    device.Model
                );

            // --------------------------------------------------------
            // FORMAT MAC
            // --------------------------------------------------------

            device.MacAddress =
                FormatMacAddress(
                    device.MacAddress
                );

            return device;
        }

        // ============================================================
        // MODEL NORMALIZATION
        // ============================================================

        private static string
            NormalizeChromebookModel(
                string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                return string.Empty;

            string cleaned =
                model.Trim();

            // --------------------------------------------------------
            // ASUS Chromebook CR11 / CR12
            //
            // GAM reports the processor as:
            //
            // Intel(R) N150
            //
            // Display the actual school Chromebook models.
            // --------------------------------------------------------

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
        // PARSE KEY / VALUE
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
                line.Substring(
                    key.Length)
                    .Trim();

            if (!string.IsNullOrWhiteSpace(value))
            {
                setter(value);
            }
        }

        // ============================================================
        // FORMAT MAC ADDRESS
        // ============================================================

        private static string
            FormatMacAddress(
                string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
                return string.Empty;

            string cleaned =
                mac
                    .Replace(":", "")
                    .Replace("-", "")
                    .Replace(".", "")
                    .Trim();

            if (cleaned.Length != 12)
                return mac;

            try
            {
                return
                    cleaned.Substring(0, 2)
                        .ToUpperInvariant() + ":" +

                    cleaned.Substring(2, 2)
                        .ToUpperInvariant() + ":" +

                    cleaned.Substring(4, 2)
                        .ToUpperInvariant() + ":" +

                    cleaned.Substring(6, 2)
                        .ToUpperInvariant() + ":" +

                    cleaned.Substring(8, 2)
                        .ToUpperInvariant() + ":" +

                    cleaned.Substring(10, 2)
                        .ToUpperInvariant();
            }
            catch
            {
                return mac;
            }
        }

        // ============================================================
        // QUOTE ARGUMENT
        // ============================================================

        private static string
            QuoteArgument(
                string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            return "\"" +
                   value.Replace(
                       "\"",
                       "\\\"") +
                   "\"";
        }
    }
}