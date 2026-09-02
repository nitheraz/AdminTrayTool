using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AdminTrayTool.Models;   

namespace AdminTrayTool.Services
{
    public class GamService
    {
        private static string? SelectedGamPath;

        // ============================================================
        // GAM INSTALLATION CHECK
        // ============================================================

        public async Task<bool> IsGamInstalledAsync()
        {
            SelectedGamPath = await GetGamExecutablePathAsync();
            return SelectedGamPath != null && File.Exists(SelectedGamPath);
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
                return (false, null, "A Chromebook serial number is required.");
            }

            serialNumber = serialNumber.Trim();

            GamResult result =
                await RunGamAsync($"info cros cros_sn {QuoteArgument(serialNumber)}");

            if (!result.Success)
            {
                string error =
                    !string.IsNullOrWhiteSpace(result.Error)
                        ? result.Error
                        : result.Output;

                return (false, null, error);
            }

            ChromebookInfo device =
                ParseChromebookInfo(result.Output, serialNumber);

            return (true, device, string.Empty);
        }

        // ============================================================
        // UPDATE ANNOTATED ASSET ID
        // ============================================================

        public async Task<GamResult> UpdateAnnotatedAssetIdAsync(
            string serialNumber, string assetId)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return new GamResult { Success = false, Error = "Serial number required." };

            if (string.IsNullOrWhiteSpace(assetId))
                return new GamResult { Success = false, Error = "Asset ID required." };

            serialNumber = serialNumber.Trim();
            assetId = assetId.Trim();

            return await RunGamAsync(
                $"update cros cros_sn {QuoteArgument(serialNumber)} annotatedassetid {QuoteArgument(assetId)}");
        }

        // ============================================================
        // DISABLE / ENABLE CHROMEBOOK
        // ============================================================

        public async Task<GamResult> DisableChromebookAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return new GamResult { Success = false, Error = "Serial number required." };

            serialNumber = serialNumber.Trim();

            return await RunGamAsync(
                $"update cros cros_sn {QuoteArgument(serialNumber)} action disable");
        }

        public async Task<GamResult> ReenableChromebookAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return new GamResult { Success = false, Error = "Serial number required." };

            serialNumber = serialNumber.Trim();

            return await RunGamAsync(
                $"update cros cros_sn {QuoteArgument(serialNumber)} action reenable");
        }

        // ============================================================
        // RUN GAM (HYBRID SELECTION)
        // ============================================================

        private async Task<GamResult> RunGamAsync(string arguments)
        {
            string? gamPath = await GetGamExecutablePathAsync();

            if (gamPath == null || !File.Exists(gamPath))
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
                var startInfo = new ProcessStartInfo
                {
                    FileName = gamPath,
                    Arguments = arguments,
                    WorkingDirectory = Path.GetDirectoryName(gamPath) ?? AppContext.BaseDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = new Process { StartInfo = startInfo };

                process.Start();

                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                string output = await outputTask;
                string error = await errorTask;

                return new GamResult
                {
                    Success = process.ExitCode == 0,
                    ExitCode = process.ExitCode,
                    Output = output.Trim(),
                    Error = error.Trim()
                };
            }
            catch (Exception ex)
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error = $"Failed to execute GAM:\n\n{ex.Message}"
                };
            }
        }

        // ============================================================
        // HYBRID GAM EXECUTABLE PATH
        // ============================================================

        private static async Task<string?> GetGamExecutablePathAsync()
        {
            if (SelectedGamPath == null)
            {
                SelectedGamPath = await GamLocator.LocateGam();
            }

            return SelectedGamPath;
        }

        // ============================================================
        // PARSE GAM OUTPUT
        // ============================================================

        private static ChromebookInfo ParseChromebookInfo(string output, string requestedSerial)
        {
            var device = new ChromebookInfo
            {
                SerialNumber = requestedSerial,
                RawOutput = output
            };

            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                if (line.StartsWith("CrOS Device:", StringComparison.OrdinalIgnoreCase))
                {
                    device.DeviceId = line.Substring("CrOS Device:".Length).Trim();
                    continue;
                }

                ParseValue(line, "serialNumber:", v => device.SerialNumber = v);
                ParseValue(line, "annotatedAssetId:", v => device.AnnotatedAssetId = v);
                ParseValue(line, "model:", v => device.Model = v);
                ParseValue(line, "macAddress:", v => device.MacAddress = v);
                ParseValue(line, "orgUnitId:", v => device.OrgUnitId = v);
                ParseValue(line, "orgUnitPath:", v => device.OrgUnitPath = v);
                ParseValue(line, "lastSync:", v => device.LastSync = v);
                ParseValue(line, "status:", v => device.Status = v);
            }

            if (string.IsNullOrWhiteSpace(device.SerialNumber))
                device.SerialNumber = requestedSerial;

            device.Model = NormalizeChromebookModel(device.Model);
            device.MacAddress = FormatMacAddress(device.MacAddress);

            return device;
        }

        private static string NormalizeChromebookModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                return string.Empty;

            string cleaned = model.Trim();

            if (string.Equals(cleaned, "Intel(R) N150", StringComparison.OrdinalIgnoreCase))
            {
                return "ASUS Chromebook CR11 (CR1104CTA) / ASUS Chromebook CR12 (CR1204CTA)";
            }

            return cleaned;
        }

        private static void ParseValue(string line, string key, Action<string> setter)
        {
            if (!line.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                return;

            string value = line.Substring(key.Length).Trim();

            if (!string.IsNullOrWhiteSpace(value))
                setter(value);
        }

        private static string FormatMacAddress(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
                return string.Empty;

            string cleaned = mac.Replace(":", "").Replace("-", "").Replace(".", "").Trim();

            if (cleaned.Length != 12)
                return mac;

            try
            {
                return string.Join(":",
                    cleaned.Substring(0, 2).ToUpperInvariant(),
                    cleaned.Substring(2, 2).ToUpperInvariant(),
                    cleaned.Substring(4, 2).ToUpperInvariant(),
                    cleaned.Substring(6, 2).ToUpperInvariant(),
                    cleaned.Substring(8, 2).ToUpperInvariant(),
                    cleaned.Substring(10, 2).ToUpperInvariant());
            }
            catch
            {
                return mac;
            }
        }

        private static string QuoteArgument(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }
    }
}
