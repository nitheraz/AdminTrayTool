using AdminTrayTool.Models;
using AdminTrayTool.UI;
using AdminTrayTool.Services;
using System.Text.Json;

namespace AdminTrayTool.Forms
{
    public enum ConfigEditorMode
    {
        AppConfig = 0,
        GroupTemplates = 1
    }

    public class ConfigEditorForm : Form
    {
        private readonly string _path;
        private readonly ConfigEditorMode _mode;

        public ConfigEditorForm(
            string path,
            ConfigEditorMode mode = ConfigEditorMode.AppConfig)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _mode = mode;

            InitializeForm();
            BuildInterface();
        }

        private void InitializeForm()
        {
            Text =
                _mode == ConfigEditorMode.AppConfig
                    ? "Edit App Config"
                    : "Edit Group Templates";

            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            BackColor = Color.FromArgb(10, 15, 25);
            ForeColor = Color.White;
            Font = new Font(
                "Segoe UI",
                10F,
                FontStyle.Regular);
        }

        private void BuildInterface()
        {
            var tabs =
                new TabControl
                {
                    Dock = DockStyle.Fill
                };

            foreach (var schema in GetSchemasForMode())
            {
                if (IsObjectConfigurationSchema(schema))
                {
                    var tabPage =
                        new TabPage(schema.TabTitle)
                        {
                            BackColor = Color.FromArgb(10, 15, 25),
                            ForeColor = Color.White
                        };

                    Func<Task<bool>>? testConnection = null;

                    if (string.Equals(
                            schema.SectionKey,
                            "mecm",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        testConnection = TestMecmConnectionAsync;
                    }

                    var editor =
                        new ConfigurationSectionEditor(
                            schema,
                            _path,
                            testConnection);

                    tabPage.Controls.Add(editor);

                    tabs.TabPages.Add(tabPage);
                }
                else
                {
                    var editor =
                        new GenericSectionEditor(
                            schema,
                            _path);

                    TabPage tabPage =
                        editor.BuildTabPage();

                    tabs.TabPages.Add(tabPage);
                }
            }

            var bottomPanel =
                new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    BackColor = Color.FromArgb(10, 15, 25)
                };

            var btnClose =
                new HudButton
                {
                    Text = "CLOSE",
                    Width = 100,
                    Height = 32
                };

            btnClose.Click += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            bottomPanel.Controls.Add(btnClose);

            bottomPanel.Resize += (s, e) =>
            {
                btnClose.Location =
                    new Point(
                        bottomPanel.Width -
                        btnClose.Width -
                        15,
                        9);
            };

            btnClose.Location =
                new Point(
                    bottomPanel.Width -
                    btnClose.Width -
                    15,
                    9);

            Controls.Add(tabs);
            Controls.Add(bottomPanel);
        }
        private async Task<bool> TestMecmConnectionAsync()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    MessageBox.Show(
                        this,
                        "The application configuration file does not exist.",
                        "MECM Connection Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return false;
                }

                string json =
                    await File.ReadAllTextAsync(_path);

                AppConfig config =
                    JsonSerializer.Deserialize<AppConfig>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        })
                    ?? new AppConfig();

                MecmManagementService service =
                    new(config.Mecm);

                MecmActionResult result =
                    await service.TestConnectionAsync();

                if (result.Success)
                {
                    MessageBox.Show(
                        this,
                        result.Output,
                        "MECM Connection Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return true;
                }

                MessageBox.Show(
                    this,
                    result.Error,
                    "MECM Connection Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Unable to test the MECM connection.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "MECM Connection Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }
        private static bool IsObjectConfigurationSchema(
            SectionSchema schema)
        {
            return string.Equals(
                       schema.SectionKey,
                       "activeDirectory",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   string.Equals(
                       schema.SectionKey,
                       "mecm",
                       StringComparison.OrdinalIgnoreCase);
        }

        private List<SectionSchema> GetSchemasForMode()
        {
            if (_mode == ConfigEditorMode.GroupTemplates)
            {
                return
                [
                    BuildGroupTemplatesSchema()
                ];
            }

            return
            [
                BuildWebPortalsSchema(),
                BuildRdpServersSchema(),
                BuildAdminToolsSchema(),
                BuildActiveDirectorySchema(),
                BuildMecmSchema()
            ];
        }

        private static SectionSchema BuildWebPortalsSchema()
        {
            return new SectionSchema
            {
                TabTitle = "Web Portals",
                SectionKey = "webPortals",
                DedupeKey = "url",
                InstructionText =
                    "Enter web portals: Name | URL | Profile",
                Columns =
                [
                    new()
                    {
                        Name = "name",
                        DisplayName = "Name",
                        Required = true,
                        Placeholder = "Display name",
                        ToolTip =
                            "Display name shown in the menu"
                    },

                    new()
                    {
                        Name = "url",
                        DisplayName = "Url",
                        Required = true,
                        Placeholder = "https://example.com",
                        ToolTip =
                            "Full URL, e.g. https://admin.google.com",
                        Validator = v =>
                            v.StartsWith(
                                "http://",
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            v.StartsWith(
                                "https://",
                                StringComparison.OrdinalIgnoreCase),
                        ValidationError =
                            "URL must start with http:// or https://"
                    },

                    new()
                    {
                        Name = "profile",
                        DisplayName = "Profile",
                        Placeholder = "Profile 1",
                        DefaultValue = "Profile 1",
                        ToolTip =
                            "Browser profile directory, e.g. Default or Profile 1"
                    }
                ]
            };
        }

        private static SectionSchema BuildRdpServersSchema()
        {
            return new SectionSchema
            {
                TabTitle = "RDP Servers",
                SectionKey = "rdpServers",
                DedupeKey = "host",
                InstructionText =
                    "Enter RDP servers: Name | Host",
                Columns =
                [
                    new()
                    {
                        Name = "name",
                        DisplayName = "Name",
                        Required = true,
                        Placeholder = "Display name",
                        ToolTip =
                            "Display name shown in the menu"
                    },

                    new()
                    {
                        Name = "host",
                        DisplayName = "Host",
                        Required = true,
                        Placeholder = "10.1.1.1",
                        ToolTip =
                            "RDP host name or IP"
                    }
                ]
            };
        }

        private static SectionSchema BuildAdminToolsSchema()
        {
            return new SectionSchema
            {
                TabTitle = "Admin Tools",
                SectionKey = "adminTools",
                DedupeKey = "exe",
                InstructionText =
                    "Enter admin tools: Name | Exe | Args | Elevated",
                Columns =
                [
                    new()
                    {
                        Name = "name",
                        DisplayName = "Name",
                        Required = true,
                        Placeholder = "Display name",
                        ToolTip =
                            "Display name shown in the menu"
                    },

                    new()
                    {
                        Name = "exe",
                        DisplayName = "Exe",
                        Required = true,
                        Placeholder = "putty.exe",
                        ToolTip =
                            "Executable name or full path"
                    },

                    new()
                    {
                        Name = "args",
                        DisplayName = "Args",
                        Placeholder = "",
                        ToolTip =
                            "Command-line arguments (optional)"
                    },

                    new()
                    {
                        Name = "elevated",
                        DisplayName = "Elevated",
                        FieldType = typeof(bool),
                        Placeholder =
                            "Run this application as administrator",
                        ToolTip =
                            "Launch the application using UAC elevation"
                    }
                ]
            };
        }

        private static SectionSchema BuildActiveDirectorySchema()
        {
            return new SectionSchema
            {
                TabTitle = "Active Directory",
                SectionKey = "activeDirectory",
                InstructionText =
                    "Configure whether Active Directory management is available on this computer.",
                Columns =
                [
                    new()
                    {
                        Name = "enabled",
                        DisplayName = "Enabled",
                        FieldType = typeof(bool),
                        Placeholder =
                            "Enable Active Directory Management",
                        ToolTip =
                            "Enable Windows computer management through Active Directory"
                    }
                ]
            };
        }

        private static SectionSchema BuildMecmSchema()
        {
            return new SectionSchema
            {
                TabTitle = "MECM",
                SectionKey = "mecm",
                InstructionText =
                    "Configure the Microsoft Endpoint Configuration Manager site used by AdminTrayTool.",
                Columns =
                [
                    new()
                    {
                        Name = "enabled",
                        DisplayName = "Enabled",
                        FieldType = typeof(bool),
                        Placeholder =
                            "Enable MECM Management",
                        ToolTip =
                            "Enable MECM management in Windows Management"
                    },

                    new()
                    {
                        Name = "siteCode",
                        DisplayName = "Site Code",
                        Required = true,
                        Placeholder = "C39",
                        ToolTip =
                            "MECM site code, for example C39"
                    },

                    new()
                    {
                        Name = "smsProviderServer",
                        DisplayName = "SMS Provider Server",
                        Required = true,
                        Placeholder = "C39-CM.AD.TSV.LOCAL",
                        ToolTip =
                            "MECM SMS Provider server name"
                    }
                ]
            };
        }

        private static SectionSchema BuildGroupTemplatesSchema()
        {
            return new SectionSchema
            {
                TabTitle = "Group Templates",
                SectionKey = "templates",
                DedupeKey = "group",
                GroupByColumn = "templateName",
                NestedArrayPropertyName = "groups",
                InstructionText =
                    "One row per group. Rows sharing a Template Name become one template.",
                Columns =
                [
                    new()
                    {
                        Name = "templateName",
                        DisplayName = "Template Name",
                        Required = true,
                        Placeholder = "Primary Staff"
                    },

                    new()
                    {
                        Name = "group",
                        DisplayName = "Group Email",
                        Required = true,
                        Placeholder = "group@yourdomain.org"
                    }
                ]
            };
        }
    }
}
