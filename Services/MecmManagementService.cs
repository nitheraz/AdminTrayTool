using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace AdminTrayTool.Services
{
    public class MecmManagementService
    {
        // ============================================================
        // MECM CONFIGURATION
        // ============================================================

        // Example: ABC
        private const string SiteCode = "C39";

        // Example: MECM01.contoso.local
        private const string SmsProviderServer = "C39-CM.AD.TSV.LOCAL";

        // ============================================================

        private string Namespace =>
            $@"root\sms\site_{SiteCode}";

        public async Task<MecmComputerResult> GetComputerAsync(
            string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
            {
                return new MecmComputerResult
                {
                    Success = false,
                    Error = "Computer name is required."
                };
            }

            computerName = computerName.Trim();

            string escapedName =
                EscapePowerShellString(computerName);

            string script = $@"
                    $computer = Get-CimInstance `
                        -ComputerName '{EscapePowerShellString(SmsProviderServer)}' `
                        -Namespace '{EscapePowerShellString(Namespace)}' `
                        -ClassName SMS_R_System `
                        -Filter ""Name = '{escapedName}'"";

                    if ($null -eq $computer) {{
                        exit 1
                    }}

                    [PSCustomObject]@{{
                        ResourceID = $computer.ResourceID
                        Name = $computer.Name
                        ResourceType = $computer.ResourceType
                        Client = $computer.Client
                        ClientVersion = $computer.ClientVersion
                        OperatingSystemNameandVersion = $computer.OperatingSystemNameandVersion
                    }} | ConvertTo-Json -Compress
                    ";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            if (!result.Success)
            {
                return new MecmComputerResult
                {
                    Success = false,
                    Error = string.IsNullOrWhiteSpace(result.Error)
                        ? $"Computer '{computerName}' was not found in MECM."
                        : result.Error
                };
            }

            try
            {
                using var document =
                    System.Text.Json.JsonDocument.Parse(
                        result.Output);

                var root = document.RootElement;

                return new MecmComputerResult
                {
                    Success = true,
                    ResourceId =
                        GetJsonString(root, "ResourceID"),
                    Name =
                        GetJsonString(root, "Name"),
                    ResourceType =
                        GetJsonString(root, "ResourceType"),
                    Client =
                        GetJsonString(root, "Client"),
                    ClientVersion =
                        GetJsonString(root, "ClientVersion"),
                    OperatingSystem =
                        GetJsonString(
                            root,
                            "OperatingSystemNameandVersion")
                };
            }
            catch (Exception ex)
            {
                return new MecmComputerResult
                {
                    Success = false,
                    Error =
                        "Unable to read MECM computer information: " +
                        ex.Message
                };
            }
        }

        public async Task<MecmCollectionResult>
            GetCollectionsAsync()
        {
            string script = $@"
                    $collections = Get-CimInstance `
                        -ComputerName '{EscapePowerShellString(SmsProviderServer)}' `
                        -Namespace '{EscapePowerShellString(Namespace)}' `
                        -ClassName SMS_Collection |
                        Select-Object CollectionID, Name, CollectionType |
                        Sort-Object Name

                    $collections | ConvertTo-Json -Compress
                    ";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            if (!result.Success)
            {
                return new MecmCollectionResult
                {
                    Success = false,
                    Error = string.IsNullOrWhiteSpace(result.Error)
                        ? "Unable to retrieve MECM collections."
                        : result.Error
                };
            }

            var collections =
                new List<MecmCollection>();

            try
            {
                using var document =
                    System.Text.Json.JsonDocument.Parse(
                        result.Output);

                var root = document.RootElement;

                if (root.ValueKind ==
                    System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in root.EnumerateArray())
                    {
                        collections.Add(
                            ParseCollection(item));
                    }
                }
                else if (root.ValueKind ==
                         System.Text.Json.JsonValueKind.Object)
                {
                    collections.Add(
                        ParseCollection(root));
                }

                return new MecmCollectionResult
                {
                    Success = true,
                    Collections = collections
                };
            }
            catch (Exception ex)
            {
                return new MecmCollectionResult
                {
                    Success = false,
                    Error =
                        "Unable to read MECM collections: " +
                        ex.Message
                };
            }
        }

        public async Task<MecmActionResult>
            AddComputerToCollectionAsync(
                string computerName,
                string collectionId)
        {
            if (string.IsNullOrWhiteSpace(computerName))
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error = "Computer name is required."
                };
            }

            if (string.IsNullOrWhiteSpace(collectionId))
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error = "Collection ID is required."
                };
            }

            var computer =
                await GetComputerAsync(computerName);

            if (!computer.Success)
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error = computer.Error
                };
            }

            string script = $@"
                    $namespace = '{EscapePowerShellString(Namespace)}'

                    $collection = Get-CimInstance `
                        -ComputerName '{EscapePowerShellString(SmsProviderServer)}' `
                        -Namespace $namespace `
                        -ClassName SMS_Collection `
                        -Filter ""CollectionID = '{EscapePowerShellString(collectionId)}'""

                    if ($null -eq $collection) {{
                        throw 'MECM collection was not found.'
                    }}

                    $ruleClass = Get-CimClass `
                        -ComputerName '{EscapePowerShellString(SmsProviderServer)}' `
                        -Namespace $namespace `
                        -ClassName SMS_CollectionRuleDirect

                    $rule = New-CimInstance `
                        -CimClass $ruleClass `
                        -ClientOnly `
                        -Property @{{ ResourceClassName = 'SMS_R_System'; ResourceID = {computer.ResourceId}; RuleName = '{EscapePowerShellString(computerName)}' }}

                    Invoke-CimMethod `
                        -ComputerName '{EscapePowerShellString(SmsProviderServer)}' `
                        -Namespace $namespace `
                        -ClassName SMS_Collection `
                        -MethodName AddMembershipRule `
                        -Arguments @{{ collectionRule = $rule }}

                    Write-Output 'Computer added to collection successfully.'
                    ";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            return new MecmActionResult
            {
                Success = result.Success,
                Output = result.Output,
                Error = result.Error
            };
        }

        public async Task<MecmActionResult>
            RemoveComputerFromCollectionAsync(
                string computerName,
                string collectionId)
        {
            if (string.IsNullOrWhiteSpace(computerName))
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error = "Computer name is required."
                };
            }

            if (string.IsNullOrWhiteSpace(collectionId))
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error = "Collection ID is required."
                };
            }

            var computer =
                await GetComputerAsync(computerName);

            if (!computer.Success)
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error = computer.Error
                };
            }

            string script = $@"
                    $namespace = '{EscapePowerShellString(Namespace)}'

                    $collection = Get-CimInstance `
                        -ComputerName '{EscapePowerShellString(SmsProviderServer)}' `
                        -Namespace $namespace `
                        -ClassName SMS_Collection `
                        -Filter ""CollectionID = '{EscapePowerShellString(collectionId)}'""

                    if ($null -eq $collection) {{
                        throw 'MECM collection was not found.'
                    }}

                    $rules = Get-CimInstance `
                        -ComputerName '{EscapePowerShellString(SmsProviderServer)}' `
                        -Namespace $namespace `
                        -ClassName SMS_CollectionRuleDirect

                    $matchingRule = $rules |
                        Where-Object {{
                            $_.CollectionID -eq '{EscapePowerShellString(collectionId)}' -and
                            $_.ResourceID -eq {computer.ResourceId}
                        }} |
                        Select-Object -First 1

                    if ($null -eq $matchingRule) {{
                        throw 'Computer is not a direct membership rule in this collection.'
                    }}

                    Invoke-CimMethod `
                        -ComputerName '{EscapePowerShellString(SmsProviderServer)}' `
                        -Namespace $namespace `
                        -ClassName SMS_Collection `
                        -MethodName DeleteMembershipRule `
                        -Arguments @{{ collectionRule = $matchingRule }}

                    Write-Output 'Computer removed from collection successfully.'
                    ";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            return new MecmActionResult
            {
                Success = result.Success,
                Output = result.Output,
                Error = result.Error
            };
        }

        private static MecmCollection ParseCollection(
            System.Text.Json.JsonElement item)
        {
            return new MecmCollection
            {
                CollectionId =
                    GetJsonString(item, "CollectionID"),

                Name =
                    GetJsonString(item, "Name"),

                CollectionType =
                    GetJsonString(item, "CollectionType")
            };
        }

        private static string GetJsonString(
            System.Text.Json.JsonElement element,
            string propertyName)
        {
            if (!element.TryGetProperty(
                    propertyName,
                    out var property))
            {
                return string.Empty;
            }

            return property.ValueKind ==
                   System.Text.Json.JsonValueKind.String
                ? property.GetString() ?? string.Empty
                : property.ToString();
        }

        private async Task<PowerShellResult>
            RunPowerShellAsync(string script)
        {
            try
            {
                var startInfo =
                    new ProcessStartInfo
                    {
                        FileName = "powershell.exe",

                        Arguments =
                            "-NoProfile -NonInteractive " +
                            "-ExecutionPolicy Bypass " +
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
                    Success =
                        process.ExitCode == 0,

                    Output =
                        output.Trim(),

                    Error =
                        error.Trim()
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

        private static string EscapePowerShellString(
            string value)
        {
            return value.Replace("'", "''");
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

        private class PowerShellResult
        {
            public bool Success { get; set; }

            public string Output { get; set; } =
                string.Empty;

            public string Error { get; set; } =
                string.Empty;
        }
    }

    public class MecmComputerResult
    {
        public bool Success { get; set; }

        public string ResourceId { get; set; } =
            string.Empty;

        public string Name { get; set; } =
            string.Empty;

        public string ResourceType { get; set; } =
            string.Empty;

        public string Client { get; set; } =
            string.Empty;

        public string ClientVersion { get; set; } =
            string.Empty;

        public string OperatingSystem { get; set; } =
            string.Empty;

        public string Error { get; set; } =
            string.Empty;
    }

    public class MecmCollection
    {
        public string CollectionId { get; set; } =
            string.Empty;

        public string Name { get; set; } =
            string.Empty;

        public string CollectionType { get; set; } =
            string.Empty;
    }

    public class MecmCollectionResult
    {
        public bool Success { get; set; }

        public List<MecmCollection> Collections { get; set; } =
            new();

        public string Error { get; set; } =
            string.Empty;
    }

    public class MecmActionResult
    {
        public bool Success { get; set; }

        public string Output { get; set; } =
            string.Empty;

        public string Error { get; set; } =
            string.Empty;
    }
}