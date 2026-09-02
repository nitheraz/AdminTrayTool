using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Threading;
//using AdminTrayTool.Forms;

namespace AdminTrayTool
{
    static class Program
    {
        private static ContextMenuStrip trayMenu = null!;
        private static Mutex? _appMutex;
        private static NotifyIcon trayIcon = null!;
        private static string AdminProfile = "Profile 1";

        [STAThread]
        static void Main()
        {
            _appMutex = new Mutex(true, "AdminTrayTool", out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show("AdminTrayTool is already running.", "AdminTrayTool", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ApplicationConfiguration.Initialize();

            trayMenu = new ContextMenuStrip();
            // ProgramData migration: prefer machine-wide config in %PROGRAMDATA%\AdminTrayTool\config.json
            string programDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AdminTrayTool");
            string programConfigPath = Path.Combine(programDataDir, "config.json");

            // Ensure program data dir exists (may require elevation on some systems)
            try { Directory.CreateDirectory(programDataDir); } catch { /* ignore - will handle write errors later */ }

            // If no program-wide config exists, but a per-user config exists, try to migrate it
            string userConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdminTrayTool", "config.json");
            if (!File.Exists(programConfigPath) && File.Exists(userConfigPath))
            {
                try { File.Copy(userConfigPath, programConfigPath); }
                catch { /* ignore copy errors - fallback to programConfigPath which may be created later */ }
            }

            // Load the config from programConfigPath. LoadConfig will create a default file if missing.
            var cfg = LoadConfig(programConfigPath);

            // Build menu from config (this also adds Edit config... using programConfigPath)
            BuildMenuFromConfig(trayMenu, cfg, programConfigPath);

            trayIcon = new NotifyIcon
            {
                Text = "IT Admin Quick Tools",
                Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "adminTray.ico")),
                ContextMenuStrip = trayMenu,
                Visible = true
            };

            Application.Run();
        }
        static Icon LoadTrayIcon()
        {
            var assembly = typeof(Program).Assembly;

            using Stream? stream = assembly.GetManifestResourceStream(
                "AdminTrayTool.adminTool.ico");

            if (stream == null)
                return SystemIcons.Application;

            return new Icon(stream);
        }

        class AppConfig
        {
            public string Editor { get; set; } = "notepad.exe";
            public List<WebPortal> WebPortals { get; set; } = new();
            public List<RdpEntry> RdpServers { get; set; } = new();
            public List<AdminTool> AdminTools { get; set; } = new();
        }

        class WebPortal { public string Name { get; set; } = string.Empty; public string Url { get; set; } = string.Empty; public string Profile { get; set; } = string.Empty; }
        class RdpEntry { public string Name { get; set; } = string.Empty; public string Host { get; set; } = string.Empty; }
        class AdminTool
        {
            public string Name { get; set; } = string.Empty;
            public string Exe { get; set; } = string.Empty;
            public string Args { get; set; } = string.Empty;
            public string Category { get; set; } = "Other";
            public bool Elevated { get; set; }
        }

        static AppConfig LoadConfig(string path)
        {
            // If config doesn't exist, create one with defaults (fresh install behavior)
            if (!File.Exists(path))
            {
                var def = new AppConfig
                {
                    Editor = "notepad.exe",
                    WebPortals = new List<WebPortal>(),
                    RdpServers = new List<RdpEntry>(),
                    AdminTools = new List<AdminTool>()
                };

                // append defaults to empty sections
                AppendDefaultEntries(def);

                var json = JsonSerializer.Serialize(
                    def,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                File.WriteAllText(path, json);
                return def;
            }

            try
            {
                var text = File.ReadAllText(path);
                var cfg = JsonSerializer.Deserialize<AppConfig>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AppConfig();

                // If any section is empty, append defaults for fresh-install convenience (do not overwrite existing items)
                AppendDefaultEntries(cfg);

                return cfg;
            }
            catch
            {
                return new AppConfig();
            }
        }

        static void AppendDefaultEntries(AppConfig cfg)
        {
            if (cfg == null) return;

            // default lists
            var defaultWeb = new List<WebPortal>
            {
                new WebPortal { Name = "Admin Console", Url = "https://admin.google.com", Profile = AdminProfile },
                new WebPortal { Name = "Dashboard", Url = "https://dashboard.example.com", Profile = AdminProfile }
            };

            var defaultRdp = new List<RdpEntry>
            {
                new RdpEntry { Name = "Server 1", Host = "Server 1" },
                new RdpEntry { Name = "Server 2", Host = "Server 2" }
            };

            var defaultTools = new List<AdminTool>
            {
                new AdminTool { Name = "PuTTY", Exe = "putty.exe", Args = "", Elevated = true },
                new AdminTool { Name = "PowerShell (Admin)", Exe = "powershell.exe", Args = "-NoExit", Elevated = true }
            };

            // merge without duplicates (case-insensitive on keys)
            bool webEmpty = cfg.WebPortals == null || cfg.WebPortals.Count == 0;
            bool rdpEmpty = cfg.RdpServers == null || cfg.RdpServers.Count == 0;
            bool toolsEmpty = cfg.AdminTools == null || cfg.AdminTools.Count == 0;

            if (cfg.WebPortals == null) cfg.WebPortals = new List<WebPortal>();
            if (cfg.RdpServers == null) cfg.RdpServers = new List<RdpEntry>();
            if (cfg.AdminTools == null) cfg.AdminTools = new List<AdminTool>();

            // Only append defaults when the section is empty (fresh install behavior)
            if (webEmpty)
            {
                foreach (var w in defaultWeb)
                {
                    if (!cfg.WebPortals.Exists(x => string.Equals(x.Url, w.Url, StringComparison.OrdinalIgnoreCase))) cfg.WebPortals.Add(w);
                }
            }

            if (rdpEmpty)
            {
                foreach (var r in defaultRdp)
                {
                    if (!cfg.RdpServers.Exists(x => string.Equals(x.Host, r.Host, StringComparison.OrdinalIgnoreCase))) cfg.RdpServers.Add(r);
                }
            }

            if (toolsEmpty)
            {
                foreach (var t in defaultTools)
                {
                    if (!cfg.AdminTools.Exists(x => string.Equals(x.Exe, t.Exe, StringComparison.OrdinalIgnoreCase))) cfg.AdminTools.Add(t);
                }
            }
        }

        static void BuildMenuFromConfig(ContextMenuStrip menu, AppConfig cfg, string configPath)
        {
            // Web portals
            if (cfg.WebPortals != null && cfg.WebPortals.Count > 0)
            {
                var webMenu = new ToolStripMenuItem("Admin Web Portals");
                foreach (var p in cfg.WebPortals)
                {
                    var profile = string.IsNullOrWhiteSpace(p.Profile) ? AdminProfile : p.Profile;
                    webMenu.DropDownItems.Add(p.Name ?? p.Url, null, (s, e) => LaunchWebpage(p.Url, profile));
                }
                menu.Items.Add(webMenu);
                menu.Items.Add(new ToolStripSeparator());
            }

            // RDP
            if (cfg.RdpServers != null && cfg.RdpServers.Count > 0)
            {
                var rdpMenu = new ToolStripMenuItem("Remote Desktop Connections");
                foreach (var r in cfg.RdpServers)
                {
                    rdpMenu.DropDownItems.Add(r.Name ?? r.Host, null, (s, e) => LaunchRdp(r.Host));
                }
                menu.Items.Add(rdpMenu);
                menu.Items.Add(new ToolStripSeparator());
            }

            // Admin Tools
            var tools = new ToolStripMenuItem("Admin Tools");
            if (cfg.AdminTools != null)
            {
                foreach (var t in cfg.AdminTools)
                {
                    var capture = t;
                    tools.DropDownItems.Add(capture.Name, null, (s, e) => LaunchProcess(capture.Exe, capture.Args, capture.Elevated));
                }
            }
            // Do not add hard-coded admin tools here. All admin tools are defined in the config so admins
            // can customize and avoid duplicate entries. Built-in helpers (Putty/PowerShell) are still
            // available as handler methods if referenced from the config.

            menu.Items.Add(tools);
            menu.Items.Add(new ToolStripSeparator());

            // Chromebook Management
            menu.Items.Add(
                "Chromebook Management",
                null,
                (s, e) =>
                {
                    try
                    {
                        using var form = new ChromebookManagementForm();
                        form.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Failed to open Chromebook Management:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                            "Chromebook Management",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                });

            menu.Items.Add(new ToolStripSeparator());

            // Edit config
            menu.Items.Add("Edit config...", null, (s, e) =>
            {
                string? serializedCfg = null;
                try { serializedCfg = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }); } catch { }

                using var form = new ConfigEditorForm(configPath, serializedCfg);
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    try
                    {
                        menu.Items.Clear();
                        var newCfg = LoadConfig(configPath);
                        BuildMenuFromConfig(menu, newCfg, configPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to reload config: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            });

            // Open config file / folder
            {
                var mi = new ToolStripMenuItem("Open config file...");
                mi.Click += (s, e) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo { FileName = configPath, UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open config file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                menu.Items.Add(mi);
            }

            {
                var mi = new ToolStripMenuItem("Open config folder...");
                mi.Click += (s, e) =>
                {
                    try
                    {
                        var folder = Path.GetDirectoryName(configPath);
                        if (string.IsNullOrEmpty(folder)) throw new InvalidOperationException("Config folder path is invalid");
                        Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open config folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                menu.Items.Add(mi);
            }

            // Exit
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Exit", null, (s, e) => Application.Exit());
        }

        static void LaunchProcess(string exe, string? args = null, bool elevated = false)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exe ?? string.Empty,
                    Arguments = args ?? string.Empty,
                    UseShellExecute = true
                };
                if (elevated) psi.Verb = "runas";
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch {exe}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void LaunchWebpage(string url, string profileDirectory)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            var browser = "chrome.exe";
            // Intentionally avoid coalescing profileDirectory here; coalescing is handled elsewhere and we keep this explicit.
            var args = string.IsNullOrWhiteSpace(profileDirectory)
                ? $"\"{url}\""
                : $"--profile-directory=\"{profileDirectory}\" \"{url}\"";

            try
            {
                Process.Start(new ProcessStartInfo { FileName = browser, Arguments = args, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void LaunchRdp(string host)
        {
            if (string.IsNullOrWhiteSpace(host)) return;
            try
            {
                Process.Start(new ProcessStartInfo { FileName = "mstsc.exe", Arguments = $"/v:{host}", UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start RDP: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void OnLaunchPutty(object sender, EventArgs e)
        {
            string[] candidates = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PuTTY", "putty.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "PuTTY", "putty.exe"),
                "putty.exe"
            };

            string? exe = null;
            foreach (var c in candidates) { if (File.Exists(c)) { exe = c; break; } }
            exe ??= "putty.exe";

            try { Process.Start(new ProcessStartInfo { FileName = exe, UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show($"Failed to launch PuTTY: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        static void OnLaunchPowerShell(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoExit",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch PowerShell: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void OnLaunchPowerShellElevated(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoExit",
                    UseShellExecute = true,
                    Verb = "runas"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch elevated PowerShell: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
