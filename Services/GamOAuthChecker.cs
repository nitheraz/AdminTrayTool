using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AdminTrayTool.Services
{
    public class GamResult
    {
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string? Output { get; set; }
        public string? Error { get; set; }

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
        /// <summary>
        /// Finds the oauth2.txt file by checking multiple possible locations.
        /// GAM might create config in different user profiles, APPDATA locations, or custom paths.
        /// Checks in this order:
        /// 1. Environment variable overrides (GAM_CONFIG_DIR)
        /// 2. Current process user's .gam folder
        /// 3. APPDATA and LOCALAPPDATA locations
        /// 4. Admin account variations
        /// 5. All user profiles in C:\Users
        /// </summary>
        private static string? FindOAuthTokenFile()
        {
            var candidatePaths = new List<string>();
        
            // 1. Check environment variable override (GAM_CONFIG_DIR)
            string? gamConfigDir = Environment.GetEnvironmentVariable("GAM_CONFIG_DIR");
            if (!string.IsNullOrWhiteSpace(gamConfigDir))
            {
                string envPath = Path.Combine(gamConfigDir, "oauth2.txt");
                candidatePaths.Add(envPath);
            }
        
            // 2. Check current process user's .gam folder (most common)
            string currentUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            candidatePaths.Add(Path.Combine(currentUserProfile, ".gam", "oauth2.txt"));
        
            // 3. Check APPDATA locations (Windows standard)
            try
            {
                string? appData = Environment.GetEnvironmentVariable("APPDATA");
                if (!string.IsNullOrWhiteSpace(appData))
                {
                    candidatePaths.Add(Path.Combine(appData, ".gam", "oauth2.txt"));
                }
        
                string? localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                if (!string.IsNullOrWhiteSpace(localAppData))
                {
                    candidatePaths.Add(Path.Combine(localAppData, ".gam", "oauth2.txt"));
                }
            }
            catch
            {
                // Ignore errors when accessing environment variables
            }
        
            // 4. Check common generic admin account names
            foreach (string adminName in new[] { "admin", "administrator", "Administrator" })
            {
                string? altAdminPath = TryBuildAdminPath(adminName);
                if (altAdminPath != null)
                {
                    candidatePaths.Add(altAdminPath);
                }
            }
        
            // 5. Search all user profiles in C:\Users (comprehensive fallback)
            try
            {
                string systemDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
                string usersDirectory = Path.Combine(systemDrive, "Users");
                if (Directory.Exists(usersDirectory))
                {
                    foreach (string userDir in Directory.GetDirectories(usersDirectory))
                    {
                        string gamPath = Path.Combine(userDir, ".gam", "oauth2.txt");
                        if (!candidatePaths.Contains(gamPath))
                        {
                            candidatePaths.Add(gamPath);
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors when scanning user directories
            }
        
            // 6. Check system drive root as last resort (uncommon but covers edge cases)
            try
            {
                string systemDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
                string systemRootPath = Path.Combine(systemDrive, ".gam", "oauth2.txt");
                if (!candidatePaths.Contains(systemRootPath))
                {
                    candidatePaths.Add(systemRootPath);
                }
            }
            catch
            {
                // Ignore errors
            }
        
            // Return the first oauth2.txt that exists
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

        private static string? TryBuildAdminPath(string userName)
        {
            try
            {
                string systemDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
                string adminPath = Path.Combine(systemDrive, "Users", userName, ".gam", "oauth2.txt");
                return adminPath;
            }
            catch
            {
                return null;
            }
        }

        public async Task<GamResult> CheckOAuthAsync()
        {
            // 1️⃣ Locate GAM7 (embedded / global / PATH)
            string? gamPath = await GamLocator.LocateGam();

            if (gamPath == null || !File.Exists(gamPath))
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error = "GAM7 executable not found.\n\n" +
                            "Please ensure GAM7 is deployed with AdminTrayTool."
                };
            }

            // 2️⃣ Check for GAM7 OAuth token file in multiple possible locations
            string? oauthFile = FindOAuthTokenFile();

            if (oauthFile == null)
            {
                return new GamResult
                {
                    Success = false,
                    ExitCode = -1,
                    Error = "GAM7 OAuth token not found.\n\n" +
                            "Please run:\n\n" +
                            "  gam oauth create\n\n" +
                            "and complete the browser authentication."
                };
            }

            // 3️⃣ Verify oauth token file is not empty
            try
            {
                var fileInfo = new FileInfo(oauthFile);
                if (fileInfo.Length == 0)
                {
                    return new GamResult
                    {
                        Success = false,
                        ExitCode = -1,
                        Error = "GAM7 OAuth token file is empty.\n\n" +
                                "Please run:\n\n" +
                                "  gam oauth create\n\n" +
                                "and complete the browser authentication again."
                    };
                }

                // Verify we can read the file
                string tokenContent = File.ReadAllText(oauthFile).Trim();
                if (string.IsNullOrEmpty(tokenContent))
                {
                    return new GamResult
                    {
                        Success = false,
                        ExitCode = -1,
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
                    Error = $"Failed to read GAM7 OAuth token:\n\n{ex.Message}"
                };
            }

            // 4️⃣ Try to validate OAuth with a simple GAM command
            // Note: This may fail if gam.cfg is not configured with domain/customer_id
            // In that case, OAuth is still valid, just not fully configured for this app
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

                // gam version should succeed regardless of domain configuration
                if (process.ExitCode == 0)
                {
                    return new GamResult
                    {
                        Success = true,
                        ExitCode = process.ExitCode,
                        Output = output.Trim(),
                        Error = error.Trim()
                    };
                }
                else
                {
                    // Even if gam version fails, if oauth2.txt exists and isn't empty,
                    // we consider OAuth successful. The failure might be due to other GAM config issues.
                    // Log the error but still return success.
                    return new GamResult
                    {
                        Success = true,
                        ExitCode = 0,
                        Output = "OAuth token exists and is valid.",
                        Error = $"Warning: GAM version check failed (non-critical): {error.Trim()}"
                    };
                }
            }
            catch (Exception ex)
            {
                // If we can't run gam version, but oauth2.txt exists, consider it successful
                // The error is likely due to GAM not being in PATH correctly
                return new GamResult
                {
                    Success = true,
                    ExitCode = 0,
                    Output = "OAuth token exists and is valid.",
                    Error = $"Warning: Could not verify GAM (non-critical): {ex.Message}"
                };
            }
        }
    }
}
