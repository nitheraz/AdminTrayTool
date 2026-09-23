using AdminTrayTool.Models;

namespace AdminTrayTool.Forms
{
    public enum ConfigEditorMode
    {
        AppConfig,
        GroupTemplates
    }

    public class ConfigEditorForm : Form
    {
        private readonly string _path;
        private readonly ConfigEditorMode _mode;

        public ConfigEditorForm(string path, ConfigEditorMode mode = ConfigEditorMode.AppConfig)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _mode = mode;

            InitializeForm();
            BuildInterface();
        }

        private void InitializeForm()
        {
            Text = _mode == ConfigEditorMode.AppConfig ? "Edit App Config" : "Edit Group Templates";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(10, 15, 25);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        }

        private void BuildInterface()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill };

            foreach (var schema in GetSchemasForMode())
            {
                var editor = new GenericSectionEditor(schema, _path);
                tabs.TabPages.Add(editor.BuildTabPage());
            }

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };

            var btnClose = new HudButton
            {
                Text = "CLOSE",
                Width = 100,
                Height = 32
            };
            btnClose.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            bottomPanel.Controls.Add(btnClose);

            bottomPanel.Resize += (s, e) =>
            {
                btnClose.Location = new Point(bottomPanel.Width - btnClose.Width - 15, 9);
            };
            btnClose.Location = new Point(bottomPanel.Width - btnClose.Width - 15, 9);

            Controls.Add(tabs);
            Controls.Add(bottomPanel);
        }

        private List<SectionSchema> GetSchemasForMode()
        {
            if (_mode == ConfigEditorMode.GroupTemplates)
            {
                return new List<SectionSchema> { BuildGroupTemplatesSchema() };
            }

            return new List<SectionSchema>
            {
                BuildWebPortalsSchema(),
                BuildRdpServersSchema(),
                BuildAdminToolsSchema()
            };
        }

        private static SectionSchema BuildWebPortalsSchema()
        {
            return new SectionSchema
            {
                TabTitle = "Web Portals",
                SectionKey = "webPortals",
                DedupeKey = "url",
                InstructionText = "Enter web portals: Name | URL | Profile",
                Columns = new List<ColumnSchema>
                {
                    new() { Name = "name", DisplayName = "Name", Required = true, Placeholder = "Display name", ToolTip = "Display name shown in the menu" },
                    new()
                    {
                        Name = "url", DisplayName = "Url", Required = true, Placeholder = "https://example.com",
                        ToolTip = "Full URL, e.g. https://admin.google.com",
                        Validator = v => v.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || v.StartsWith("https://", StringComparison.OrdinalIgnoreCase),
                        ValidationError = "URL must start with http:// or https://"
                    },
                    new() { Name = "profile", DisplayName = "Profile", Placeholder = "Profile 1", DefaultValue = "Profile 1", ToolTip = "Browser profile directory, e.g. Default or Profile 1" }
                }
            };
        }

        private static SectionSchema BuildRdpServersSchema()
        {
            return new SectionSchema
            {
                TabTitle = "RDP Servers",
                SectionKey = "rdpServers",
                DedupeKey = "host",
                InstructionText = "Enter RDP servers: Name | Host",
                Columns = new List<ColumnSchema>
                {
                    new() { Name = "name", DisplayName = "Name", Required = true, Placeholder = "Display name", ToolTip = "Display name shown in the menu" },
                    new() { Name = "host", DisplayName = "Host", Required = true, Placeholder = "10.1.1.1", ToolTip = "RDP host name or IP" }
                }
            };
        }

        private static SectionSchema BuildAdminToolsSchema()
        {
            return new SectionSchema
            {
                TabTitle = "Admin Tools",
                SectionKey = "adminTools",
                DedupeKey = "exe",
                InstructionText = "Enter admin tools: Name | Exe | Args | Elevated",
                Columns = new List<ColumnSchema>
                {
                    new() { Name = "name", DisplayName = "Name", Required = true, Placeholder = "Display name", ToolTip = "Display name shown in the menu" },
                    new() { Name = "exe", DisplayName = "Exe", Required = true, Placeholder = "putty.exe", ToolTip = "Executable name or full path" },
                    new() { Name = "args", DisplayName = "Args", Placeholder = "", ToolTip = "Command-line arguments (optional)" },
                }
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
                InstructionText = "One row per group. Rows sharing a Template Name become one template.",
                Columns = new List<ColumnSchema>
                {
                    new() { Name = "templateName", DisplayName = "Template Name", Required = true, Placeholder = "Primary Staff" },
                    new() { Name = "group", DisplayName = "Group Email", Required = true, Placeholder = "group@yourdomain.org" }
                }
            };
        }
    }
}