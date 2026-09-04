using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminTrayTool.Services
{
    public enum GamIssueType
    {
        None,
        GamNotFound,
        ProjectMissing,
        OAuthTokenMissing,
        OAuthTokenEmpty,
        Unknown
    }

    public class GamResult
    {
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string? Output { get; set; }
        public string? Error { get; set; }
        public GamIssueType IssueType { get; set; } = GamIssueType.None;

        public string CombinedOutput
        {
            get
            {
                return string.IsNullOrWhiteSpace(Error) ? Output ?? string.Empty :
                    string.IsNullOrWhiteSpace(Output) ? Error ?? string.Empty :
                    (Output ?? string.Empty) + Environment.NewLine + (Error ?? string.Empty);
            }
        }
    }

    public class GamOAuthChecker
    {
        private static string? FindGamConfigFile(string fileName)
        {
            var candidatePaths = new List<string>();

            string? gamConfigDir = Environment.GetEnvironmentVariable("GAM_CONFIG_DIR");
            if (!string.IsNullOrWhiteSpace(gamConfigDir))
            {
                candidatePaths.Add(Path.Combine(gamConfigDir, fileName));
            }

            string currentUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            candidatePaths.Add(Path.Combine(currentUserProfile, ".gam", fileName));

            try
            {
                string? appData = Environment.GetEnvironmentVariable("APPDATA");
                if (!string.IsNullOrWhiteSpace(appData))
                {
                    candidatePaths.Add(Path.Combine(appData, ".gam", fileName));
                }

                string? localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                if (!string.IsNullOrWhiteSpace(localAppData))
                {
                    candidatePaths.Add(Path.Combine(localAppData, ".gam", fileName));
                }
            }
            catch
            {
                // Ignore errors when accessing environment variables
            }

            foreach (string adminName in new[] { "admin", "administrator", "Administrator" })
            {
                try
                {
                    string systemDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
                    candidatePaths.Add(Path.Combine(systemDrive, "Users", adminName, ".gam", fileName));
                }
                catch
                {
                    // Ignore
                }
            }

            try
            {
                string systemDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
                string usersDirectory = Path.Combine(systemDrive, "Users");
                if (Directory.Exists(usersDirectory))
                {
                    foreach (string userDir in Directory.GetDirectories(usersDirectory))
                    {
                        string candidate = Path.Combine(userDir, ".gam", fileName);
                        if (!candidatePaths.Contains(candidate))
                        {
                            candidatePaths.Add(candidate);
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors when scanning user directories
            }

            try
            {
                string systemDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
                string systemRootPath = Path.Combine(systemDrive, ".gam", fileName);
                if (!candidatePaths.Contains(systemRootPath))
                {
                    candidatePaths.Add(systemRootPath);
                }
            }
            catch
            {
                // Ignore errors
            }

            foreach (string path in candidatePaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }

        private static string? FindOAuthTokenFile() => FindGamConfigFile("oauth2.txt");

        // Public so the UI layer can check for/report on this file too.
        private static string? FindClientSecretsFile() => FindGamConfigFile("client_secrets.json");

        public async Task<GamResult> CheckOAuthAsync()
        {
            string? gamPath = await GamLocator.LocateGam();

            if (gamPath == null || !File.Exists(gamPath))
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    IssueType = GamIssueType.GamNotFound,
                    Error = "GAM7 executable not found.\n\n" +
                            "Please ensure GAM7 is deployed with AdminTrayTool."
                };
            }

            string? clientSecretsFile = FindClientSecretsFile();

            if (clientSecretsFile == null)
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    IssueType = GamIssueType.ProjectMissing,
                    Error = "No GAM project found (client_secrets.json is missing).\n\n" +
                            "You need to create or select a GAM project before authorizing.\n\n" +
                            "Please run:\n\n" +
                            "  gam create project\n\n" +
                            "  (or, if you already have a project) gam use project\n\n" +
                            "then run:\n\n" +
                            "  gam oauth create"
                };
            }

            string? oauthFile = FindOAuthTokenFile();

            if (oauthFile == null)
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    IssueType = GamIssueType.OAuthTokenMissing,
                    Error = "GAM7 OAuth token not found.\n\n" +
                            "Please run:\n\n" +
                            "  gam oauth create\n\n" +
                            "and complete the browser authentication."
                };
            }

            try
            {
                var fileInfo = new FileInfo(oauthFile);
                if (fileInfo.Length == 0)
                {
                    return new GamResult
                    {
                        Success = false,
                        ExitCode = -1,
                        IssueType = GamIssueType.OAuthTokenEmpty,
                        Error = "GAM7 OAuth token file is empty.\n\n" +
                                "Please run:\n\n" +
                                "  gam oauth create\n\n" +
                                "and complete the browser authentication again."
                    };
                }

                string tokenContent = File.ReadAllText(oauthFile).Trim();
                if (string.IsNullOrEmpty(tokenContent))
                {
                    return new GamResult
                    {
                        Success = false,
                        ExitCode = -1,
                        IssueType = GamIssueType.OAuthTokenEmpty,
                        Error = "GAM7 OAuth token file is invalid or empty.\n\n" +
                                "Please run:\n\n" +
                                "  gam oauth create\n\n" +
                                "and complete the browser authentication again."
                    };
                }
            }
            catch (Exception ex)
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    IssueType = GamIssueType.Unknown,
                    Error = $"Failed to read GAM7 OAuth token:\n\n{ex.Message}"
                };
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = gamPath,
                    Arguments = "version",
                    WorkingDirectory = Path.GetDirectoryName(gamPath) ?? AppContext.BaseDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };

                using var process = new Process { StartInfo = startInfo };

                process.Start();

                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                string output = await outputTask;
                string error = await errorTask;

                if (process.ExitCode == 0)
                {
                    return new GamResult
                    {
                        Success = true,
                        ExitCode = process.ExitCode,
                        IssueType = GamIssueType.None,
                        Output = output.Trim(),
                        Error = error.Trim()
                    };
                }
                else
                {
                    return new GamResult
                    {
                        Success = true,
                        ExitCode = 0,
                        IssueType = GamIssueType.None,
                        Output = "OAuth token exists and is valid.",
                        Error = $"Warning: GAM version check failed (non-critical): {error.Trim()}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new GamResult
                {
                    Success = true,
                    ExitCode = 0,
                    IssueType = GamIssueType.None,
                    Output = "OAuth token exists and is valid.",
                    Error = $"Warning: Could not verify GAM (non-critical): {ex.Message}"
                };
            }
        }
    }
}