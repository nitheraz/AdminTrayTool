using AdminTrayTool.Models;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace AdminTrayTool.Services
{
    public class MecmManagementService(MecmConfig config)
    {
        private readonly MecmConfig _config =
                config ??
                throw new ArgumentNullException(nameof(config));

        private string Namespace =>
            $@"root\sms\site_{_config.SiteCode}";

        private MecmActionResult ValidateConfiguration()
        {
            if (!_config.Enabled)
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error =
                        "MECM management is disabled in the " +
                        "application configuration."
                };
            }

            if (string.IsNullOrWhiteSpace(_config.SiteCode))
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error = "MECM Site Code is not configured."
                };
            }

            if (string.IsNullOrWhiteSpace(_config.SmsProviderServer))
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error =
                        "MECM SMS Provider Server is not configured."
                };
            }

            return new MecmActionResult
            {
                Success = true
            };
        }

        private MecmComputerResult ValidateComputerConfiguration()
        {
            MecmActionResult validation =
                ValidateConfiguration();

            if (validation.Success)
            {
                return new MecmComputerResult
                {
                    Success = true
                };
            }

            return new MecmComputerResult
            {
                Success = false,
                Error = validation.Error
            };
        }

        // =============================================================
        // COMPUTER LOOKUP
        // =============================================================

        public async Task<MecmComputerResult> GetComputerAsync(
            string computerName)
        {
            MecmComputerResult configuration =
                ValidateComputerConfiguration();

            if (!configuration.Success)
            {
                return configuration;
            }

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

            string script = $$"""

$ErrorActionPreference = 'Stop'

$computer = Get-CimInstance `
    -ComputerName '{{EscapePowerShellString(_config.SmsProviderServer)}}' `
    -Namespace '{{EscapePowerShellString(Namespace)}}' `
    -ClassName SMS_R_System `
    -Filter "Name = '{{escapedName}}'"

if ($null -eq $computer)
{
    throw "Computer '{{EscapePowerShellString(computerName)}}' was not found in MECM."
}

[PSCustomObject]@{
    ResourceID = $computer.ResourceID
    Name = $computer.Name
    ResourceType = $computer.ResourceType
    Client = $computer.Client
    ClientVersion = $computer.ClientVersion
    OperatingSystemNameandVersion = $computer.OperatingSystemNameandVersion
    Manufacturer = $computer.Manufacturer
    Model = $computer.Model
    SerialNumber = $computer.SerialNumber
} | ConvertTo-Json -Compress

""";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            if (!result.Success)
            {
                return new MecmComputerResult
                {
                    Success = false,
                    Error =
                        string.IsNullOrWhiteSpace(result.Error)
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
                        GetJsonString(
                            root,
                            "ResourceID"),

                    Name =
                        GetJsonString(
                            root,
                            "Name"),

                    ResourceType =
                        GetJsonString(
                            root,
                            "ResourceType"),

                    Client =
                        GetJsonString(
                            root,
                            "Client"),

                    ClientVersion =
                        GetJsonString(
                            root,
                            "ClientVersion"),

                    OperatingSystem =
                        GetJsonString(
                            root,
                            "OperatingSystemNameandVersion"),

                    Manufacturer =
                        GetJsonString(
                            root,
                            "Manufacturer"),

                    Model =
                        GetJsonString(
                            root,
                            "Model"),

                    SerialNumber =
                        GetJsonString(
                            root,
                            "SerialNumber")
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

        // =============================================================
        // ALL COLLECTIONS
        // =============================================================

        public async Task<MecmCollectionResult> GetCollectionsAsync()
        {
            MecmActionResult validation =
                ValidateConfiguration();

            if (!validation.Success)
            {
                return new MecmCollectionResult
                {
                    Success = false,
                    Error = validation.Error
                };
            }

            string script = $@"
$ErrorActionPreference = 'Stop'

$collections = Get-CimInstance `
    -ComputerName '{EscapePowerShellString(_config.SmsProviderServer)}' `
    -Namespace '{EscapePowerShellString(Namespace)}' `
    -ClassName SMS_Collection |
    Select-Object CollectionID, Name, CollectionType |
    Sort-Object Name

@($collections) |
    ConvertTo-Json -Compress
";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            if (!result.Success)
            {
                return new MecmCollectionResult
                {
                    Success = false,
                    Error =
                        string.IsNullOrWhiteSpace(result.Error)
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

        // =============================================================
        // COMPUTER COLLECTION MEMBERSHIP
        // =============================================================

        public async Task<MecmCollectionResult>
            GetComputerCollectionsAsync(
                string resourceId)
        {
            MecmActionResult validation =
                ValidateConfiguration();

            if (!validation.Success)
            {
                return new MecmCollectionResult
                {
                    Success = false,
                    Error = validation.Error
                };
            }

            if (!int.TryParse(
                    resourceId,
                    out int parsedResourceId))
            {
                return new MecmCollectionResult
                {
                    Success = false,
                    Error =
                        $"Invalid MECM Resource ID '{resourceId}'."
                };
            }

            string script = $$"""

$ErrorActionPreference = 'Stop'

$memberships = Get-CimInstance `
    -ComputerName '{{EscapePowerShellString(_config.SmsProviderServer)}}' `
    -Namespace '{{EscapePowerShellString(Namespace)}}' `
    -ClassName SMS_FullCollectionMembership `
    -Filter "ResourceID = {{parsedResourceId}}"

$collectionIds = @(
    $memberships |
    Select-Object -ExpandProperty CollectionID -Unique
)

$collections = foreach ($collectionId in $collectionIds)
{
    Get-CimInstance `
        -ComputerName '{{EscapePowerShellString(_config.SmsProviderServer)}}' `
        -Namespace '{{EscapePowerShellString(Namespace)}}' `
        -ClassName SMS_Collection `
        -Filter "CollectionID = '$collectionId'"
}

@($collections) |
    Select-Object CollectionID, Name, CollectionType |
    Sort-Object Name |
    ConvertTo-Json -Compress

""";

            PowerShellResult result =
                await RunPowerShellAsync(script);

            if (!result.Success)
            {
                return new MecmCollectionResult
                {
                    Success = false,
                    Error =
                        string.IsNullOrWhiteSpace(result.Error)
                            ? "Unable to retrieve the computer's MECM collection memberships."
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

                collections =
                    [.. collections
                        .OrderBy(
                            collection =>
                                collection.Name,
                            StringComparer.OrdinalIgnoreCase)];

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
                        "Unable to read the computer's MECM collection memberships: " +
                        ex.Message
                };
            }
        }

        // =============================================================
        // ADD COMPUTER TO COLLECTION
        // =============================================================

        public async Task<MecmActionResult>
            AddComputerToCollectionAsync(
                string computerName,
                string collectionId)
        {
            MecmActionResult validation =
                ValidateConfiguration();

            if (!validation.Success)
            {
                return validation;
            }

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

            computerName =
                computerName.Trim();

            collectionId =
                collectionId.Trim();

            MecmComputerResult computer =
                await GetComputerAsync(
                    computerName);

            if (!computer.Success)
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error = computer.Error
                };
            }

            if (!int.TryParse(
                    computer.ResourceId,
                    out int resourceId))
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error =
                        $"Invalid MECM Resource ID '{computer.ResourceId}' " +
                        $"for computer '{computerName}'."
                };
            }

            string script = $$"""

$ErrorActionPreference = 'Stop'

$collection = Get-CimInstance `
    -ComputerName '{{EscapePowerShellString(_config.SmsProviderServer)}}' `
    -Namespace '{{EscapePowerShellString(Namespace)}}' `
    -ClassName SMS_Collection `
    -Filter "CollectionID = '{{EscapePowerShellString(collectionId)}}'"

if ($null -eq $collection)
{
    throw "MECM collection '{{EscapePowerShellString(collectionId)}}' was not found."
}

if ('{{EscapePowerShellString(collectionId)}}' -like 'SMS*')
{
    throw "The collection '{{EscapePowerShellString(collectionId)}}' is a default MECM collection and cannot be modified with a direct membership rule."
}

$ruleClass = Get-CimClass `
    -ComputerName '{{EscapePowerShellString(_config.SmsProviderServer)}}' `
    -Namespace '{{EscapePowerShellString(Namespace)}}' `
    -ClassName SMS_CollectionRuleDirect

$rule = New-CimInstance `
    -CimClass $ruleClass `
    -ClientOnly `
    -Property @{
        ResourceClassName = 'SMS_R_System'
        ResourceID = {{resourceId}}
    }

$result = Invoke-CimMethod `
    -InputObject $collection `
    -MethodName AddMembershipRule `
    -Arguments @{
        collectionRule = $rule
    }

if ($null -eq $result)
{
    throw "MECM AddMembershipRule returned no result."
}

if ($result.ReturnValue -ne 0)
{
    throw "MECM AddMembershipRule failed with return value $($result.ReturnValue)."
}

Write-Output 'Direct membership rule created.'

""";

            PowerShellResult result =
                await RunPowerShellAsync(
                    script);

            if (!result.Success)
            {
                return new MecmActionResult
                {
                    Success = false,
                    Output = result.Output,
                    Error =
                        string.IsNullOrWhiteSpace(result.Error)
                            ? "MECM failed to add the direct membership rule."
                            : result.Error
                };
            }

            // Do NOT perform a second SMS_CollectionRuleDirect
            // verification here.
            //
            // AddMembershipRule returned ReturnValue = 0,
            // which means MECM accepted the membership rule.
            //
            // The UI can refresh the computer's collection
            // membership list separately.

            return new MecmActionResult
            {
                Success = true,
                Output =
                    $"Computer '{computerName}' was added as a direct " +
                    $"member of collection '{collectionId}'."
            };
        }

        // =============================================================
        // REMOVE COMPUTER FROM COLLECTION
        // =============================================================

        public async Task<MecmActionResult>
            RemoveComputerFromCollectionAsync(
                string computerName,
                string collectionId)
        {
            MecmActionResult validation =
                ValidateConfiguration();

            if (!validation.Success)
            {
                return validation;
            }

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

            computerName =
                computerName.Trim();

            collectionId =
                collectionId.Trim();

            MecmComputerResult computer =
                await GetComputerAsync(
                    computerName);

            if (!computer.Success)
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error = computer.Error
                };
            }

            if (!int.TryParse(
                    computer.ResourceId,
                    out int resourceId))
            {
                return new MecmActionResult
                {
                    Success = false,
                    Error =
                        $"Invalid MECM Resource ID '{computer.ResourceId}' " +
                        $"for computer '{computerName}'."
                };
            }

            string script = $$"""

$ErrorActionPreference = 'Stop'

$collection = Get-CimInstance `
    -ComputerName '{{EscapePowerShellString(_config.SmsProviderServer)}}' `
    -Namespace '{{EscapePowerShellString(Namespace)}}' `
    -ClassName SMS_Collection `
    -Filter "CollectionID = '{{EscapePowerShellString(collectionId)}}'"

if ($null -eq $collection)
{
    throw "MECM collection '{{EscapePowerShellString(collectionId)}}' was not found."
}

$rules = @(
    $collection.CollectionRules
)

$matchingRule = $rules |
    Where-Object {
        $_.ResourceID -eq {{resourceId}}
    } |
    Select-Object -First 1

if ($null -eq $matchingRule)
{
    throw "Computer '{{EscapePowerShellString(computerName)}}' is not a direct member of collection '{{EscapePowerShellString(collectionId)}}'."
}

$result = Invoke-CimMethod `
    -InputObject $collection `
    -MethodName DeleteMembershipRule `
    -Arguments @{
        collectionRule = $matchingRule
    }

if ($null -eq $result)
{
    throw "MECM DeleteMembershipRule returned no result."
}

if ($result.ReturnValue -ne 0)
{
    throw "MECM DeleteMembershipRule failed with return value $($result.ReturnValue)."
}

Write-Output 'Direct membership rule deleted.'

""";

            PowerShellResult result =
                await RunPowerShellAsync(
                    script);

            if (!result.Success)
            {
                return new MecmActionResult
                {
                    Success = false,
                    Output = result.Output,
                    Error =
                        string.IsNullOrWhiteSpace(result.Error)
                            ? "MECM failed to remove the direct membership rule."
                            : result.Error
                };
            }

            return new MecmActionResult
            {
                Success = true,
                Output =
                    $"Computer '{computerName}' was removed from the direct " +
                    $"membership of collection '{collectionId}'."
            };
        }

        // =============================================================
        // JSON HELPERS
        // =============================================================

        private static MecmCollection ParseCollection(
            System.Text.Json.JsonElement item)
        {
            return new MecmCollection
            {
                CollectionId =
                    GetJsonString(
                        item,
                        "CollectionID"),

                Name =
                    GetJsonString(
                        item,
                        "Name"),

                CollectionType =
                    GetJsonString(
                        item,
                        "CollectionType")
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

        // =============================================================
        // POWERSHELL
        // =============================================================

        private static async Task<PowerShellResult>
            RunPowerShellAsync(
                string script)
        {
            try
            {
                var startInfo =
                    new ProcessStartInfo
                    {
                        FileName =
                            "powershell.exe",

                        Arguments =
                            "-NoProfile " +
                            "-NonInteractive " +
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
                    new()
                    {
                        StartInfo = startInfo
                    };

                process.Start();

                Task<string> outputTask =
                    process
                        .StandardOutput
                        .ReadToEndAsync();

                Task<string> errorTask =
                    process
                        .StandardError
                        .ReadToEndAsync();

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
        // INTERNAL POWERSHELL RESULT
        // =============================================================

        private sealed class PowerShellResult
        {
            public bool Success { get; set; }

            public string Output { get; set; } =
                string.Empty;

            public string Error { get; set; } =
                string.Empty;
        }
    }

    // =============================================================
    // COMPUTER RESULT
    // =============================================================

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

        public string Manufacturer { get; set; } =
            string.Empty;

        public string Model { get; set; } =
            string.Empty;

        public string SerialNumber { get; set; } =
            string.Empty;

        public string Error { get; set; } =
            string.Empty;
    }

    // =============================================================
    // COLLECTION
    // =============================================================

    public class MecmCollection
    {
        public string CollectionId { get; set; } =
            string.Empty;

        public string Name { get; set; } =
            string.Empty;

        public string CollectionType { get; set; } =
            string.Empty;
    }

    // =============================================================
    // COLLECTION RESULT
    // =============================================================

    public class MecmCollectionResult
    {
        public bool Success { get; set; }

        public List<MecmCollection> Collections { get; set; } =
            [];

        public string Error { get; set; } =
            string.Empty;
    }

    // =============================================================
    // ACTION RESULT
    // =============================================================

    public class MecmActionResult
    {
        public bool Success { get; set; }

        public string Output { get; set; } =
            string.Empty;

        public string Error { get; set; } =
            string.Empty;
    }
}
