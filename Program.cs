using AdminTrayTool.Forms;
using AdminTrayTool.Services;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace AdminTrayTool
{
    /// <summary>
    /// Provides the application entry point and system-tray functionality for AdminTrayTool.
    /// Handles application startup, configuration loading, tray menu creation,
    /// and launching administrative tools.
    /// </summary>
    static class Program
    {
        private static Mutex? _appMutex;
        private static NotifyIcon trayIcon = null!;
        private const string AdminProfile = "Profile 1";

        /// <summary>
        /// Starts AdminTrayTool, initializes the system-tray application,
        /// loads the application configuration, builds the tray menu,
        /// and starts the Windows Forms message loop.
        /// </summary>
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

            var trayMenu = new ContextMenuStrip();
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

            // Build menu from config (this also adds Config Settings using programConfigPath)
            BuildMenuFromConfig(trayMenu, cfg, programConfigPath);

            trayIcon = new NotifyIcon
            {
                Text = "IT Admin Quick Tools",
                Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "adminTray.ico")),
                ContextMenuStrip = trayMenu,
                Visible = true
            };

            // Silent background update check on startup
            var startupVersion = typeof(Program).Assembly.GetName().Version;
            string startupVersionString = startupVersion != null
                ? $"{startupVersion.Major}.{startupVersion.Minor}.{startupVersion.Build}"
                : "0.0.0";
            _ = UpdateCheckService.CheckForUpdateAsync(startupVersionString, showUpToDateMessage: false);

            Application.Run();
        }
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Represents the application configuration.
        /// </summary>
        class AppConfig
        {
            public string Editor { get; set; } = "notepad.exe";
            public List<WebPortal> WebPortals { get; set; } = [];
            public List<RdpEntry> RdpServers { get; set; } = [];
            public List<AdminTool> AdminTools { get; set; } = [];
        }

        /// <summary>
        /// Represents a web portal available from the AdminTrayTool tray menu.
        /// </summary>
        class WebPortal
        {
            public string Name { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string Profile { get; set; } = string.Empty;
        }

        /// <summary>
        /// Represents a Remote Desktop connection available from the tray menu.
        /// </summary>
        class RdpEntry
        {
            public string Name { get; set; } = string.Empty;
            public string Host { get; set; } = string.Empty;
        }

        /// <summary>
        /// Represents a locally installed administrative tool that can be launched
        /// from the AdminTrayTool tray menu.
        /// </summary>
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
                    WebPortals = [],
                    RdpServers = [],
                    AdminTools = []
                };

                // append defaults to empty sections
                AppendDefaultEntries(def);

                 var json = JsonSerializer.Serialize(
                    def,
                    JsonOptions);

                File.WriteAllText(path, json);
                return def;
            }

            try
            {
                var text = File.ReadAllText(path);
                var cfg = JsonSerializer.Deserialize<AppConfig>(text, JsonOptions) ?? new AppConfig();

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

            cfg.WebPortals ??= [];
            cfg.RdpServers ??= [];
            cfg.AdminTools ??= [];

            AddDefaultWebPortals(cfg);
            AddDefaultRdpServers(cfg);
            AddDefaultAdminTools(cfg);
        }
        static void AddDefaultWebPortals(AppConfig cfg)
        {
            if (cfg.WebPortals.Count > 0) return;

            var defaults = new[]
            {
                new WebPortal
                {
                    Name = "Admin Console",
                    Url = "https://admin.google.com",
                    Profile = AdminProfile
                },
                new WebPortal
                {
                    Name = "Dashboard",
                    Url = "https://dashboard.example.com",
                    Profile = AdminProfile
                }
            };

            foreach (var web in defaults)
            {
                if (!cfg.WebPortals.Exists(x =>
                    string.Equals(x.Url, web.Url, StringComparison.OrdinalIgnoreCase)))
                {
                    cfg.WebPortals.Add(web);
                }
            }
        }

        static void AddDefaultRdpServers(AppConfig cfg)
        {
            if (cfg.RdpServers.Count > 0) return;

            var defaults = new[]
            {
                new RdpEntry { Name = "Server 1", Host = "Server 1" },
                new RdpEntry { Name = "Server 2", Host = "Server 2" }
            };

            foreach (var rdp in defaults)
            {
                if (!cfg.RdpServers.Exists(x =>
                    string.Equals(x.Host, rdp.Host, StringComparison.OrdinalIgnoreCase)))
                {
                    cfg.RdpServers.Add(rdp);
                }
            }
        }

        static void AddDefaultAdminTools(AppConfig cfg)
        {
            if (cfg.AdminTools.Count > 0) return;

            var defaults = new[]
            {
        new AdminTool
        {
            Name = "PuTTY",
            Exe = "putty.exe",
            Args = "",
            Elevated = true
        },
        new AdminTool
        {
            Name = "PowerShell (Admin)",
            Exe = "powershell.exe",
            Args = "-NoExit",
            Elevated = true
        }
    };

            foreach (var tool in defaults)
            {
                if (!cfg.AdminTools.Exists(x =>
                    string.Equals(x.Exe, tool.Exe, StringComparison.OrdinalIgnoreCase)))
                {
                    cfg.AdminTools.Add(tool);
                }
            }
        }

        static void BuildMenuFromConfig(ContextMenuStrip menu, AppConfig cfg, string configPath)
        {
            AddWebPortalMenu(menu, cfg);
            AddRdpMenu(menu, cfg);
            AddAdminToolsMenu(menu, cfg);

            menu.Items.Add(new ToolStripSeparator());

            AddManagementMenuItems(menu);
            AddConfigSettingsMenu(menu, configPath);

            AddAboutAndExitItems(menu);
        }

        static void AddWebPortalMenu(ContextMenuStrip menu, AppConfig cfg)
        {
            if (cfg.WebPortals == null || cfg.WebPortals.Count == 0)
                return;

            var webMenu = new ToolStripMenuItem("Admin Web Portals");

            foreach (var portal in cfg.WebPortals)
            {
                var profile = string.IsNullOrWhiteSpace(portal.Profile)
                    ? AdminProfile
                    : portal.Profile;

                webMenu.DropDownItems.Add(
                    portal.Name ?? portal.Url,
                    null,
                    (s, e) => LaunchWebpage(portal.Url, profile));
            }

            menu.Items.Add(webMenu);
            menu.Items.Add(new ToolStripSeparator());
        }

        static void AddRdpMenu(ContextMenuStrip menu, AppConfig cfg)
        {
            if (cfg.RdpServers == null || cfg.RdpServers.Count == 0)
                return;

            var rdpMenu = new ToolStripMenuItem("Remote Desktop Connections");

            foreach (var server in cfg.RdpServers)
            {
                rdpMenu.DropDownItems.Add(
                    server.Name ?? server.Host,
                    null,
                    (s, e) => LaunchRdp(server.Host));
            }

            menu.Items.Add(rdpMenu);
            menu.Items.Add(new ToolStripSeparator());
        }

        static void AddManagementMenuItems(ContextMenuStrip menu)
        {
            menu.Items.Add(
                "Chromebook Management",
                null,
                (s, e) => OpenManagementForm<ChromebookManagementForm>(
                    "Chromebook Management"));

            menu.Items.Add(
                "Group Management",
                null,
                (s, e) => OpenManagementForm<GroupManagementForm>(
                    "Group Management"));

            menu.Items.Add(
                "Windows Management",
                null,
                (s, e) => OpenManagementForm<WindowsManagementForm>(
                    "Windows Management"));

            menu.Items.Add(new ToolStripSeparator());
        }

        static void OpenManagementForm<TForm>(string formName)
            where TForm : Form, new()
        {
            try
            {
                using var form = new TForm();
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open {formName}:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    formName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        static void AddAppConfigItems(
            ToolStripMenuItem configMenu,
            ContextMenuStrip menu,
            string configPath)
        {
            configMenu.DropDownItems.Add(
                "Edit App Config...",
                null,
                (s, e) =>
                {
                    using var form = new ConfigEditorForm(
                        configPath,
                        ConfigEditorMode.AppConfig);

                    var result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        ReloadMenu(menu, configPath);
                    }
                });

            configMenu.DropDownItems.Add(
                "Open App Config File...",
                null,
                (s, e) => OpenFile(configPath, "config file"));

            configMenu.DropDownItems.Add(
                "Open App Config Folder...",
                null,
                (s, e) => OpenFolder(
                    Path.GetDirectoryName(configPath),
                    "config folder"));
        }

        static void AddGroupTemplateItems(ToolStripMenuItem configMenu)
        {
            configMenu.DropDownItems.Add(
                "Edit Group Templates...",
                null,
                (s, e) =>
                {
                    string templatesPath = GroupTemplateService.GetDefaultPath();

                    using var form = new ConfigEditorForm(
                        templatesPath,
                        ConfigEditorMode.GroupTemplates);

                    form.ShowDialog();
                });

            configMenu.DropDownItems.Add(
                "Open Group Templates File...",
                null,
                (s, e) => OpenFile(
                    GroupTemplateService.GetDefaultPath(),
                    "group templates file"));

            configMenu.DropDownItems.Add(
                "Open Group Templates Folder...",
                null,
                (s, e) => OpenFolder(
                    Path.GetDirectoryName(
                        GroupTemplateService.GetDefaultPath()),
                    "group templates folder"));
        }

        static void OpenFile(string filePath, string description)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open {description}: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        static void OpenFolder(string? folder, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(folder))
                {
                    throw new InvalidOperationException(
                        $"{description} path is invalid");
                }

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{folder}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open {description}: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        static void ReloadMenu(ContextMenuStrip menu, string configPath)
        {
            try
            {
                menu.Items.Clear();

                var newCfg = LoadConfig(configPath);
                BuildMenuFromConfig(menu, newCfg, configPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to reload config: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        static void AddAboutAndExitItems(ContextMenuStrip menu)
        {
            menu.Items.Add(
                "About AdminTrayTool",
                null,
                (s, e) =>
                {
                    using var form = new AboutForm();
                    form.ShowDialog();
                });

            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add(
                "Exit",
                null,
                (s, e) => Application.Exit());
        }
        static void AddConfigSettingsMenu(ContextMenuStrip menu, string configPath)
        {
            var configSettingsMenu = new ToolStripMenuItem("Config Settings");

            AddAppConfigItems(configSettingsMenu, menu, configPath);
            configSettingsMenu.DropDownItems.Add(new ToolStripSeparator());
            AddGroupTemplateItems(configSettingsMenu);

            menu.Items.Add(configSettingsMenu);
            menu.Items.Add(new ToolStripSeparator());
        }

        static void AddAdminToolsMenu(ContextMenuStrip menu, AppConfig cfg)
        {
            var toolsMenu = new ToolStripMenuItem("Admin Tools");

            if (cfg.AdminTools != null)
            {
                foreach (var tool in cfg.AdminTools)
                {
                    var capture = tool;

                    toolsMenu.DropDownItems.Add(
                        capture.Name,
                        null,
                        (s, e) => LaunchProcess(
                            capture.Exe,
                            capture.Args,
                            capture.Elevated));
                }
            }

            menu.Items.Add(toolsMenu);
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
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                string arguments = string.IsNullOrWhiteSpace(profileDirectory)
                    ? $"\"{url}\""
                    : $"--profile-directory=\"{profileDirectory}\" \"{url}\"";

                // Ask the user's normal Explorer process to launch Chrome.
                // This prevents the elevated AdminTrayTool process from
                // launching Chrome as Administrator.
                Type? shellType = Type.GetTypeFromProgID("Shell.Application") ?? throw new InvalidOperationException("Windows Shell.Application could not be found.");
                dynamic shell = Activator.CreateInstance(shellType)
                    ?? throw new InvalidOperationException("Could not create Windows Shell.Application.");

                shell.ShellExecute(
                    "chrome.exe",
                    arguments,
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "open",
                    1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open URL: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
    }
}
