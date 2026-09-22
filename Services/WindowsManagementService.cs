using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace AdminTrayTool.Services
{
    public class WindowsManagementService
    {
        // =============================================================
        // FIND COMPUTER
        // =============================================================

        public async Task<WindowsComputerResult> GetComputerAsync(
            string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
            {
                return new WindowsComputerResult
                {
                    Success = false,
                    Error = "Computer name is required."
                };
            }

            computerName = computerName.Trim();

            string script =
                "$computer = Get-ADComputer " +
                "-Identity " +
                $"'{EscapePowerShellString(computerName)}' " +
                "-Properties DistinguishedName,Name,OperatingSystem;" +
                Environment.NewLine +
                "if ($null -eq $computer) { exit 1 };" +
                Environment.NewLine +
                "[PSCustomObject]@{" +
                "Name=$computer.Name;" +
                "OperatingSystem=$computer.OperatingSystem;" +
                "DistinguishedName=$computer.DistinguishedName" +
                "} | ConvertTo-Json -Compress";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            if (!result.Success)
            {
                return new WindowsComputerResult
                {
                    Success = false,
                    Error = string.IsNullOrWhiteSpace(result.Error)
                        ? $"Computer '{computerName}' was not found in Active Directory."
                        : result.Error
                };
            }

            try
            {
                using var document =
                    System.Text.Json.JsonDocument.Parse(
                        result.Output);

                var root = document.RootElement;

                string name =
                    root.TryGetProperty("Name", out var nameProperty)
                        ? nameProperty.GetString() ?? string.Empty
                        : string.Empty;

                string operatingSystem =
                    root.TryGetProperty("OperatingSystem", out var osProperty)
                        ? osProperty.GetString() ?? string.Empty
                        : string.Empty;

                string distinguishedName =
                    root.TryGetProperty("DistinguishedName", out var dnProperty)
                        ? dnProperty.GetString() ?? string.Empty
                        : string.Empty;

                return new WindowsComputerResult
                {
                    Success = true,
                    Name = name,
                    OperatingSystem = operatingSystem,
                    DistinguishedName = distinguishedName,
                    OrganizationalUnit =
                        GetOrganizationalUnitFromDistinguishedName(
                            distinguishedName)
                };
            }
            catch (Exception ex)
            {
                return new WindowsComputerResult
                {
                    Success = false,
                    Error =
                        "Unable to read Active Directory computer information: " +
                        ex.Message
                };
            }
        }

        // =============================================================
        // GET ORGANIZATIONAL UNITS
        // =============================================================

        public async Task<(bool Success, List<string> OrganizationalUnits, string Error)>
            GetOrganizationalUnitsAsync()
        {
            string script =
                "Get-ADOrganizationalUnit -Filter * " +
                "-Properties DistinguishedName | " +
                "Select-Object -ExpandProperty DistinguishedName";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            if (!result.Success)
            {
                return (
                    false,
                    new List<string>(),
                    string.IsNullOrWhiteSpace(result.Error)
                        ? "Unable to retrieve Active Directory organisational units."
                        : result.Error);
            }

            var organizationalUnits =
                new List<string>();

            string[] lines =
                result.Output.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string ou =
                    line.Trim();

                if (!string.IsNullOrWhiteSpace(ou))
                {
                    organizationalUnits.Add(ou);
                }
            }

            organizationalUnits.Sort(
                StringComparer.OrdinalIgnoreCase);

            return (
                true,
                organizationalUnits,
                string.Empty);
        }

        // =============================================================
        // MOVE COMPUTER
        // =============================================================

        public async Task<WindowsActionResult> MoveComputerToOuAsync(
            string computerName,
            string targetOrganizationalUnit)
        {
            if (string.IsNullOrWhiteSpace(computerName))
            {
                return new WindowsActionResult
                {
                    Success = false,
                    Error = "Computer name is required."
                };
            }

            if (string.IsNullOrWhiteSpace(targetOrganizationalUnit))
            {
                return new WindowsActionResult
                {
                    Success = false,
                    Error = "Target organisational unit is required."
                };
            }

            computerName =
                computerName.Trim();

            targetOrganizationalUnit =
                targetOrganizationalUnit.Trim();

            string script =
                "$computer = Get-ADComputer " +
                "-Identity " +
                $"'{EscapePowerShellString(computerName)}';" +
                Environment.NewLine +
                "if ($null -eq $computer) { " +
                "throw 'Computer was not found in Active Directory.' " +
                "};" +
                Environment.NewLine +
                "Move-ADObject " +
                "-Identity $computer.DistinguishedName " +
                "-TargetPath " +
                $"'{EscapePowerShellString(targetOrganizationalUnit)}';";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            return new WindowsActionResult
            {
                Success = result.Success,
                Output = result.Output,
                Error = result.Error
            };
        }

        // =============================================================
        // POWERSHELL EXECUTION
        // =============================================================

        private async Task<PowerShellResult> RunPowerShellAsync(
            string script)
        {
            try
            {
                var startInfo =
                    new ProcessStartInfo
                    {
                        FileName = "powershell.exe",

                        Arguments =
                            "-NoProfile -NonInteractive -ExecutionPolicy Bypass " +
                            "-Command " +
                            QuoteArgument(script),

                        UseShellExecute = false,

                        CreateNoWindow = true,

                        RedirectStandardOutput = true,

                        RedirectStandardError = true,

                        StandardOutputEncoding =
                            Encoding.UTF8,

                        StandardErrorEncoding =
                            Encoding.UTF8
                    };

                using Process process =
                    new Process
                    {
                        StartInfo = startInfo
                    };

                process.Start();

                Task<string> outputTask =
                    process.StandardOutput.ReadToEndAsync();

                Task<string> errorTask =
                    process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                string output =
                    await outputTask;

                string error =
                    await errorTask;

                return new PowerShellResult
                {
                    Success = process.ExitCode == 0,

                    Output = output.Trim(),

                    Error = error.Trim()
                };
            }
            catch (Exception ex)
            {
                return new PowerShellResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        // =============================================================
        // DISTINGUISHED NAME → OU
        // =============================================================

        private static string GetOrganizationalUnitFromDistinguishedName(
            string distinguishedName)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
                return string.Empty;

            string[] parts =
                distinguishedName.Split(',');

            var ouParts =
                new List<string>();

            foreach (string part in parts)
            {
                string value =
                    part.Trim();

                if (value.StartsWith(
                    "OU=",
                    StringComparison.OrdinalIgnoreCase))
                {
                    ouParts.Add(value);
                }
            }

            if (ouParts.Count == 0)
                return string.Empty;

            return string.Join(",", ouParts);
        }

        // =============================================================
        // POWERSHELL STRING ESCAPING
        // =============================================================

        private static string EscapePowerShellString(
            string value)
        {
            return value.Replace(
                "'",
                "''");
        }

        private static string QuoteArgument(
            string value)
        {
            return "\"" +
                   value.Replace(
                       "\"",
                       "\\\"") +
                   "\"";
        }

        // =============================================================
        // RESULT CLASSES
        // =============================================================

        private class PowerShellResult
        {
            public bool Success { get; set; }

            public string Output { get; set; } =
                string.Empty;

            public string Error { get; set; } =
                string.Empty;
        }
    }

    public class WindowsComputerResult
    {
        public bool Success { get; set; }

        public string Name { get; set; } =
            string.Empty;

        public string OperatingSystem { get; set; } =
            string.Empty;

        public string DistinguishedName { get; set; } =
            string.Empty;

        public string OrganizationalUnit { get; set; } =
            string.Empty;

        public string Error { get; set; } =
            string.Empty;
    }

    public class WindowsActionResult
    {
        public bool Success { get; set; }

        public string Output { get; set; } =
            string.Empty;

        public string Error { get; set; } =
            string.Empty;
    }
}