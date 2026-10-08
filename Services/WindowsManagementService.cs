using System.Diagnostics;
using System.Text;

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
                "$ErrorActionPreference = 'Stop';" +
                Environment.NewLine +
                "try {" +
                Environment.NewLine +
                "    $computer = Get-ADComputer " +
                "-Identity " +
                $"'{EscapePowerShellString(computerName)}' " +
                "-Properties DistinguishedName,Name,OperatingSystem " +
                "-ErrorAction Stop;" +
                Environment.NewLine +
                "}" +
                Environment.NewLine +
                "catch {" +
                Environment.NewLine +
                "    if ($_.CategoryInfo.Category -eq 'ObjectNotFound') {" +
                Environment.NewLine +
                "        exit 2;" +
                Environment.NewLine +
                "    }" +
                Environment.NewLine +
                "    throw;" +
                Environment.NewLine +
                "}" +
                Environment.NewLine +
                "if ($null -eq $computer) { exit 2 };" +
                Environment.NewLine +
                "[PSCustomObject]@{" +
                "Name=$computer.Name;" +
                "OperatingSystem=$computer.OperatingSystem;" +
                "DistinguishedName=$computer.DistinguishedName" +
                "} | ConvertTo-Json -Compress";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            if (result.ExitCode == 2)
            {
                return new WindowsComputerResult
                {
                    Success = false,
                    Error =
                        "Computer not found in Active Directory." +
                        Environment.NewLine +
                        Environment.NewLine +
                        $"The computer '{computerName}' could not be found in Active Directory." +
                        Environment.NewLine +
                        Environment.NewLine +
                        "Please check the computer name and make sure the device has an Active Directory computer account."
                };
            }

            if (!result.Success)
            {
                return new WindowsComputerResult
                {
                    Success = false,
                    Error =
                        GetFriendlyActiveDirectoryError(
                            computerName,
                            result.Error)
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
            const string script =
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
                    ['\r', '\n'],
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
                "$ErrorActionPreference = 'Stop';" +
                Environment.NewLine +
                "try {" +
                Environment.NewLine +
                "    $computer = Get-ADComputer " +
                "-Identity " +
                $"'{EscapePowerShellString(computerName)}' " +
                "-ErrorAction Stop;" +
                Environment.NewLine +
                "}" +
                Environment.NewLine +
                "catch {" +
                Environment.NewLine +
                "    if ($_.CategoryInfo.Category -eq 'ObjectNotFound') {" +
                Environment.NewLine +
                "        exit 2;" +
                Environment.NewLine +
                "    }" +
                Environment.NewLine +
                "    throw;" +
                Environment.NewLine +
                "}" +
                Environment.NewLine +
                "if ($null -eq $computer) { exit 2 };" +
                Environment.NewLine +
                "Move-ADObject " +
                "-Identity $computer.DistinguishedName " +
                "-TargetPath " +
                $"'{EscapePowerShellString(targetOrganizationalUnit)}';";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            if (result.ExitCode == 2)
            {
                return new WindowsActionResult
                {
                    Success = false,
                    Output = result.Output,
                    Error =
                        "Computer not found in Active Directory." +
                        Environment.NewLine +
                        Environment.NewLine +
                        $"The computer '{computerName}' could not be found in Active Directory."
                };
            }

            return new WindowsActionResult
            {
                Success = result.Success,
                Output = result.Output,
                Error = result.Success
                    ? string.Empty
                    : GetFriendlyActiveDirectoryActionError(
                        result.Error)
            };
        }

        // =============================================================
        // DELETE COMPUTER
        // =============================================================

        public async Task<WindowsActionResult> DeleteComputerAsync(
            string distinguishedName)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
            {
                return new WindowsActionResult
                {
                    Success = false,
                    Error = "Computer distinguished name is required."
                };
            }

            distinguishedName =
                distinguishedName.Trim();

            string script =
                "$ErrorActionPreference = 'Stop';" +
                Environment.NewLine +
                "try {" +
                Environment.NewLine +
                "    $computer = Get-ADComputer " +
                "-Identity " +
                $"'{EscapePowerShellString(distinguishedName)}' " +
                "-ErrorAction Stop;" +
                Environment.NewLine +
                "}" +
                Environment.NewLine +
                "catch {" +
                Environment.NewLine +
                "    if ($_.CategoryInfo.Category -eq 'ObjectNotFound') {" +
                Environment.NewLine +
                "        exit 2;" +
                Environment.NewLine +
                "    }" +
                Environment.NewLine +
                "    throw;" +
                Environment.NewLine +
                "}" +
                Environment.NewLine +
                "if ($null -eq $computer) { exit 2 };" +
                Environment.NewLine +
                "Remove-ADComputer " +
                "-Identity $computer.DistinguishedName " +
                "-Confirm:$false;";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            if (result.ExitCode == 2)
            {
                return new WindowsActionResult
                {
                    Success = false,
                    Output = result.Output,
                    Error =
                        "Computer not found in Active Directory." +
                        Environment.NewLine +
                        Environment.NewLine +
                        "The computer account could no longer be found in Active Directory."
                };
            }

            return new WindowsActionResult
            {
                Success = result.Success,
                Output = result.Output,
                Error =
                    result.Success
                        ? string.Empty
                        : GetFriendlyActiveDirectoryActionError(
                            result.Error)
            };
        }

        // =============================================================
        // ACTIVE DIRECTORY ERROR MESSAGES
        // =============================================================

        private static string GetFriendlyActiveDirectoryError(
            string computerName,
            string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return
                    $"Unable to look up computer '{computerName}' in Active Directory.";
            }

            if (ContainsAny(
                    error,
                    "Access is denied",
                    "UnauthorizedAccessException",
                    "permission",
                    "not authorized"))
            {
                return
                    "Access denied." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "You do not have permission to query Active Directory." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "Please check your Active Directory permissions and make sure you are connected to the school domain.";
            }

            if (ContainsAny(
                    error,
                    "Get-ADComputer",
                    "is not recognized",
                    "CommandNotFoundException",
                    "ActiveDirectory module"))
            {
                return
                    "Active Directory PowerShell module unavailable." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "The Active Directory PowerShell module could not be loaded on this computer." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "Please make sure the Active Directory management tools are installed." +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"Details: {error}";
            }

            if (ContainsAny(
                    error,
                    "server is not operational",
                    "Unable to contact the server",
                    "RPC server is unavailable",
                    "network path",
                    "network",
                    "connection",
                    "cannot contact",
                    "specified domain",
                    "domain does not exist",
                    "logon failure"))
            {
                return
                    "Unable to connect to Active Directory." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "AdminTrayTool could not connect to or query Active Directory." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "Please check your network connection, domain/VPN connection, and permissions." +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"Details: {error}";
            }

            return
                "Unable to query Active Directory." +
                Environment.NewLine +
                Environment.NewLine +
                $"AdminTrayTool could not complete the lookup for computer '{computerName}'." +
                Environment.NewLine +
                Environment.NewLine +
                $"Details: {error}";
        }

        private static string GetFriendlyActiveDirectoryActionError(
            string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return
                    "The Active Directory operation could not be completed.";
            }

            if (ContainsAny(
                    error,
                    "Access is denied",
                    "UnauthorizedAccessException",
                    "permission",
                    "not authorized"))
            {
                return
                    "Access denied." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "You do not have permission to modify this computer account in Active Directory." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "Please check your Active Directory permissions.";
            }

            if (ContainsAny(
                    error,
                    "server is not operational",
                    "Unable to contact the server",
                    "RPC server is unavailable",
                    "network path",
                    "network",
                    "connection",
                    "cannot contact",
                    "specified domain",
                    "domain does not exist"))
            {
                return
                    "Unable to connect to Active Directory." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "AdminTrayTool could not communicate with Active Directory." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "Please check your network connection, domain/VPN connection, and permissions." +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"Details: {error}";
            }

            return
                "Active Directory operation failed." +
                Environment.NewLine +
                Environment.NewLine +
                $"Details: {error}";
        }

        private static bool ContainsAny(
            string value,
            params string[] searchTerms)
        {
            foreach (string searchTerm in searchTerms)
            {
                if (value.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
                    new()
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

                    ExitCode = process.ExitCode,

                    Output = output.Trim(),

                    Error = error.Trim()
                };
            }
            catch (Exception ex)
            {
                return new PowerShellResult
                {
                    Success = false,

                    ExitCode = -1,

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

            public int ExitCode { get; set; }

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
