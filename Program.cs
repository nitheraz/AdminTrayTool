using AdminTrayTool.Forms;
using AdminTrayTool.Models;
using AdminTrayTool.Services;
using System.Diagnostics;
using System.Text.Json;

namespace AdminTrayTool
{
    static class Program
    {
        private static Mutex? _appMutex;
        private static NotifyIcon trayIcon = null!;

        private const string AdminProfile = "Profile 1";
        private const string UpdateCompletedArgument = "--update-complete";

        [STAThread]
        static void Main(string[] args)
        {
            bool updateCompleted = args.Any(arg =>
                string.Equals(
                    arg,
                    UpdateCompletedArgument,
                    StringComparison.OrdinalIgnoreCase));

            _appMutex = new Mutex(
                true,
                "AdminTrayTool",
                out bool createdNew);

            if (!createdNew)
            {
                MessageBox.Show(
                    "AdminTrayTool is already running.",
                    "AdminTrayTool",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            ApplicationConfiguration.Initialize();

            var trayMenu =
                new ContextMenuStrip();

            string programDataDir =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.CommonApplicationData),
                    "AdminTrayTool");

            string programConfigPath =
                Path.Combine(
                    programDataDir,
                    "config.json");

            try
            {
                Directory.CreateDirectory(
                    programDataDir);
            }
            catch
            {
                // Ignore.
            }

            string userConfigPath =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData),
                    "AdminTrayTool",
                    "config.json");

            if (!File.Exists(programConfigPath) &&
                File.Exists(userConfigPath))
            {
                try
                {
                    File.Copy(
                        userConfigPath,
                        programConfigPath);
                }
                catch
                {
                    // Ignore migration failure.
                }
            }

            AppConfig cfg =
                LoadConfig(
                    programConfigPath);

            BuildMenuFromConfig(
                trayMenu,
                cfg,
                programConfigPath);

            trayIcon =
                new NotifyIcon
                {
                    Text = "IT Admin Quick Tools",

                    Icon =
                        new Icon(
                            Path.Combine(
                                AppContext.BaseDirectory,
                                "adminTray.ico")),

                    ContextMenuStrip =
                        trayMenu,

                    Visible = true
                };

            if (updateCompleted)
            {
                trayIcon.BalloonTipTitle =
                    "AdminTrayTool Update Complete";

                trayIcon.BalloonTipText =
                    $"AdminTrayTool has been successfully updated to version {GetApplicationVersion()}.";

                trayIcon.BalloonTipIcon =
                    ToolTipIcon.Info;

                trayIcon.ShowBalloonTip(5000);

                MessageBox.Show(
                    $"AdminTrayTool has been successfully updated to version {GetApplicationVersion()}.",
                    "Update Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            string startupVersionString =
                GetApplicationVersion();

            _ = UpdateCheckService.CheckForUpdateAsync(
                startupVersionString,
                showUpToDateMessage: false);

            Application.Run();
        }

        // ============================================================
        // APPLICATION VERSION
        // ============================================================

        private static string GetApplicationVersion()
        {
            Version? version =
                typeof(Program)
                    .Assembly
                    .GetName()
                    .Version;

            return version != null
                ? $"{version.Major}.{version.Minor}.{version.Build}"
                : "0.0.0";
        }

        // ============================================================
        // JSON CONFIGURATION
        // ============================================================

        private static readonly JsonSerializerOptions JsonOptions =
            new()
            {
                WriteIndented = true,
                PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase
            };

        private static AppConfig LoadConfig(
            string path)
        {
            if (!File.Exists(path))
            {
                var def =
                    CreateDefaultConfig();

                string json =
                    JsonSerializer.Serialize(
                        def,
                        JsonOptions);

                File.WriteAllText(
                    path,
                    json);

                return def;
            }

            try
            {
                string text =
                    File.ReadAllText(path);

                AppConfig cfg =
                    JsonSerializer.Deserialize<AppConfig>(
                        text,
                        JsonOptions)
                    ?? new AppConfig();

                ApplyConfigDefaults(cfg);

                return cfg;
            }
            catch
            {
                return CreateDefaultConfig();
            }
        }

        private static AppConfig CreateDefaultConfig()
        {
            var config =
                new AppConfig
                {
                    Editor = "notepad.exe",

                    WebPortals = [],

                    RdpServers = [],

                    AdminTools = [],

                    ActiveDirectory =
                        new ActiveDirectoryConfig
                        {
                            Enabled = true
                        },

                    Mecm =
                        new MecmConfig
                        {
                            Enabled = false,
                            SiteCode = string.Empty,
                            SmsProviderServer = string.Empty
                        }
                };

            AddDefaultWebPortals(config);
            AddDefaultRdpServers(config);
            AddDefaultAdminTools(config);

            return config;
        }

        private static void ApplyConfigDefaults(
            AppConfig cfg)
        {
            cfg.WebPortals ??= [];
            cfg.RdpServers ??= [];
            cfg.AdminTools ??= [];

            cfg.ActiveDirectory ??=
                new ActiveDirectoryConfig
                {
                    Enabled = true
                };

            cfg.Mecm ??=
                new MecmConfig
                {
                    Enabled = false,
                    SiteCode = string.Empty,
                    SmsProviderServer = string.Empty
                };

            cfg.Mecm.SiteCode ??=
                string.Empty;

            cfg.Mecm.SmsProviderServer ??=
                string.Empty;

            AddDefaultWebPortals(cfg);
            AddDefaultRdpServers(cfg);
            AddDefaultAdminTools(cfg);
        }

        // ============================================================
        // DEFAULT WEB PORTALS
        // ============================================================

        private static void AddDefaultWebPortals(
            AppConfig cfg)
        {
            if (cfg.WebPortals.Count > 0)
                return;

            var defaults =
                new[]
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
                    string.Equals(
                        x.Url,
                        web.Url,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    cfg.WebPortals.Add(web);
                }
            }
        }

        // ============================================================
        // DEFAULT RDP SERVERS
        // ============================================================

        private static void AddDefaultRdpServers(
            AppConfig cfg)
        {
            if (cfg.RdpServers.Count > 0)
                return;

            var defaults =
                new[]
                {
                    new RdpEntry
                    {
                        Name = "Server 1",
                        Host = "Server 1"
                    },

                    new RdpEntry
                    {
                        Name = "Server 2",
                        Host = "Server 2"
                    }
                };

            foreach (var rdp in defaults)
            {
                if (!cfg.RdpServers.Exists(x =>
                    string.Equals(
                        x.Host,
                        rdp.Host,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    cfg.RdpServers.Add(rdp);
                }
            }
        }

        // ============================================================
        // DEFAULT ADMIN TOOLS
        // ============================================================

        private static void AddDefaultAdminTools(
            AppConfig cfg)
        {
            if (cfg.AdminTools.Count > 0)
                return;

            var defaults =
                new[]
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
                    string.Equals(
                        x.Exe,
                        tool.Exe,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    cfg.AdminTools.Add(tool);
                }
            }
        }

        // ============================================================
        // MENU
        // ============================================================

        private static void BuildMenuFromConfig(
            ContextMenuStrip menu,
            AppConfig cfg,
            string configPath)
        {
            AddWebPortalMenu(
                menu,
                cfg);

            AddRdpMenu(
                menu,
                cfg);

            AddAdminToolsMenu(
                menu,
                cfg);

            menu.Items.Add(
                new ToolStripSeparator());

            AddManagementMenuItems(
                menu,
                cfg);

            AddConfigSettingsMenu(
                menu,
                configPath);

            AddAboutAndExitItems(
                menu);
        }

        // ============================================================
        // WEB PORTALS
        // ============================================================

        private static void AddWebPortalMenu(
            ContextMenuStrip menu,
            AppConfig cfg)
        {
            if (cfg.WebPortals == null ||
                cfg.WebPortals.Count == 0)
            {
                return;
            }

            var webMenu =
                new ToolStripMenuItem(
                    "Admin Web Portals");

            foreach (var portal in cfg.WebPortals)
            {
                string profile =
                    string.IsNullOrWhiteSpace(
                        portal.Profile)
                        ? AdminProfile
                        : portal.Profile;

                webMenu.DropDownItems.Add(
                    portal.Name ?? portal.Url,
                    null,
                    (s, e) =>
                        LaunchWebpage(
                            portal.Url,
                            profile));
            }

            menu.Items.Add(webMenu);

            menu.Items.Add(
                new ToolStripSeparator());
        }

        // ============================================================
        // RDP
        // ============================================================

        private static void AddRdpMenu(
            ContextMenuStrip menu,
            AppConfig cfg)
        {
            if (cfg.RdpServers == null ||
                cfg.RdpServers.Count == 0)
            {
                return;
            }

            var rdpMenu =
                new ToolStripMenuItem(
                    "Remote Desktop Connections");

            foreach (var server in cfg.RdpServers)
            {
                rdpMenu.DropDownItems.Add(
                    server.Name ?? server.Host,
                    null,
                    (s, e) =>
                        LaunchRdp(
                            server.Host));
            }

            menu.Items.Add(rdpMenu);

            menu.Items.Add(
                new ToolStripSeparator());
        }

        // ============================================================
        // MANAGEMENT MENU
        // ============================================================

        private static void AddManagementMenuItems(
            ContextMenuStrip menu,
            AppConfig cfg)
        {
            menu.Items.Add(
                "Chromebook Management",
                null,
                (s, e) =>
                    OpenManagementForm<ChromebookManagementForm>(
                        "Chromebook Management"));

            menu.Items.Add(
                "Group Management",
                null,
                (s, e) =>
                    OpenManagementForm<GroupManagementForm>(
                        "Group Management"));

            menu.Items.Add(
                "Windows Management",
                null,
                (s, e) =>
                    OpenWindowsManagementForm(
                        cfg));

            menu.Items.Add(
                new ToolStripSeparator());
        }

        private static void OpenWindowsManagementForm(
            AppConfig cfg)
        {
            try
            {
                using var form =
                    new WindowsManagementForm(
                        cfg);

                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open Windows Management:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Windows Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void OpenManagementForm<TForm>(
            string formName)
            where TForm : Form, new()
        {
            try
            {
                using var form =
                    new TForm();

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

        // ============================================================
        // CONFIGURATION SETTINGS
        // ============================================================

        private static void AddAppConfigItems(
            ToolStripMenuItem configMenu,
            ContextMenuStrip menu,
            string configPath)
        {
            configMenu.DropDownItems.Add(
                "Edit App Config...",
                null,
                (s, e) =>
                {
                    using var form =
                        new ConfigEditorForm(
                            configPath,
                            ConfigEditorMode.AppConfig);

                    DialogResult result =
                        form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        ReloadMenu(
                            menu,
                            configPath);
                    }
                });

            configMenu.DropDownItems.Add(
                "Open App Config File...",
                null,
                (s, e) =>
                    OpenFile(
                        configPath,
                        "config file"));

            configMenu.DropDownItems.Add(
                "Open App Config Folder...",
                null,
                (s, e) =>
                    OpenFolder(
                        Path.GetDirectoryName(
                            configPath),
                        "config folder"));
        }

        private static void AddGroupTemplateItems(
            ToolStripMenuItem configMenu)
        {
            configMenu.DropDownItems.Add(
                "Edit Group Templates...",
                null,
                (s, e) =>
                {
                    string templatesPath =
                        GroupTemplateService.GetDefaultPath();

                    using var form =
                        new ConfigEditorForm(
                            templatesPath,
                            ConfigEditorMode.GroupTemplates);

                    form.ShowDialog();
                });

            configMenu.DropDownItems.Add(
                "Open Group Templates File...",
                null,
                (s, e) =>
                    OpenFile(
                        GroupTemplateService.GetDefaultPath(),
                        "group templates file"));

            configMenu.DropDownItems.Add(
                "Open Group Templates Folder...",
                null,
                (s, e) =>
                    OpenFolder(
                        Path.GetDirectoryName(
                            GroupTemplateService.GetDefaultPath()),
                        "group templates folder"));
        }

        // ============================================================
        // FILE / FOLDER HELPERS
        // ============================================================

        private static void OpenFile(
            string filePath,
            string description)
        {
            try
            {
                Process.Start(
                    new ProcessStartInfo
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

        private static void OpenFolder(
            string? folder,
            string description)
        {
            try
            {
                if (string.IsNullOrEmpty(folder))
                {
                    throw new InvalidOperationException(
                        $"{description} path is invalid");
                }

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(
                        folder);
                }

                Process.Start(
                    new ProcessStartInfo
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

        // ============================================================
        // RELOAD CONFIGURATION
        // ============================================================

        private static void ReloadMenu(
            ContextMenuStrip menu,
            string configPath)
        {
            try
            {
                menu.Items.Clear();

                AppConfig newCfg =
                    LoadConfig(
                        configPath);

                BuildMenuFromConfig(
                    menu,
                    newCfg,
                    configPath);
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

        // ============================================================
        // ABOUT / EXIT
        // ============================================================

        private static void AddAboutAndExitItems(
            ContextMenuStrip menu)
        {
            menu.Items.Add(
                "About AdminTrayTool",
                null,
                (s, e) =>
                {
                    using var form =
                        new AboutForm();

                    form.ShowDialog();
                });

            menu.Items.Add(
                new ToolStripSeparator());

            menu.Items.Add(
                "Exit",
                null,
                (s, e) =>
                    Application.Exit());
        }

        // ============================================================
        // CONFIG MENU
        // ============================================================

        private static void AddConfigSettingsMenu(
            ContextMenuStrip menu,
            string configPath)
        {
            var configSettingsMenu =
                new ToolStripMenuItem(
                    "Config Settings");

            AddAppConfigItems(
                configSettingsMenu,
                menu,
                configPath);

            configSettingsMenu.DropDownItems.Add(
                new ToolStripSeparator());

            AddGroupTemplateItems(
                configSettingsMenu);

            menu.Items.Add(
                configSettingsMenu);

            menu.Items.Add(
                new ToolStripSeparator());
        }

        // ============================================================
        // ADMIN TOOLS
        // ============================================================

        private static void AddAdminToolsMenu(
            ContextMenuStrip menu,
            AppConfig cfg)
        {
            var toolsMenu =
                new ToolStripMenuItem(
                    "Admin Tools");

            if (cfg.AdminTools != null)
            {
                foreach (var tool in cfg.AdminTools)
                {
                    var capture =
                        tool;

                    toolsMenu.DropDownItems.Add(
                        capture.Name,
                        null,
                        (s, e) =>
                            LaunchProcess(
                                capture.Exe,
                                capture.Args,
                                capture.Elevated));
                }
            }

            menu.Items.Add(
                toolsMenu);
        }

        // ============================================================
        // PROCESS LAUNCHING
        // ============================================================

        private static void LaunchProcess(
            string exe,
            string? args = null,
            bool elevated = false)
        {
            try
            {
                var psi =
                    new ProcessStartInfo
                    {
                        FileName =
                            exe ?? string.Empty,

                        Arguments =
                            args ?? string.Empty,

                        UseShellExecute = true
                    };

                if (elevated)
                {
                    psi.Verb = "runas";
                }

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch {exe}: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ============================================================
        // WEB BROWSER
        // ============================================================

        private static void LaunchWebpage(
            string url,
            string profileDirectory)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                string arguments =
                    string.IsNullOrWhiteSpace(
                        profileDirectory)
                        ? $"\"{url}\""
                        : $"--profile-directory=\"{profileDirectory}\" \"{url}\"";

                Type? shellType =
                    Type.GetTypeFromProgID(
                        "Shell.Application")
                    ?? throw new InvalidOperationException(
                        "Windows Shell.Application could not be found.");

                dynamic shell =
                    Activator.CreateInstance(
                        shellType)
                    ?? throw new InvalidOperationException(
                        "Could not create Windows Shell.Application.");

                shell.ShellExecute(
                    "chrome.exe",
                    arguments,
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.UserProfile),
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

        // ============================================================
        // RDP
        // ============================================================

        private static void LaunchRdp(
            string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return;

            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "mstsc.exe",
                        Arguments = $"/v:{host}",
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to start RDP: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
