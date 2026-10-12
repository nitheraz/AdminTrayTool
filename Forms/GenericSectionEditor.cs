using AdminTrayTool.Models;
using AdminTrayTool.UI;
using System.Text.Json;

namespace AdminTrayTool.Forms
{
    public class GenericSectionEditor(SectionSchema schema, string configPath)
    {
        private readonly SectionSchema _schema =
            schema ?? throw new ArgumentNullException(nameof(schema));

        private readonly string _configPath =
            configPath ?? throw new ArgumentNullException(nameof(configPath));

        private DataGridView _dgv = null!;

        private static readonly JsonSerializerOptions JsonSerializeOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly Color BackgroundColor = Color.FromArgb(10, 15, 25);
        private static readonly Color PanelColor = Color.FromArgb(16, 23, 36);
        private static readonly Color InputColor = Color.FromArgb(20, 27, 40);
        private static readonly Color BorderColor = Color.FromArgb(45, 55, 70);
        private static readonly Color SelectedColor = Color.FromArgb(45, 70, 115);
        private static readonly Color MutedColor = Color.FromArgb(150, 160, 175);

        private sealed class TemplateEntry
        {
            public string Name { get; set; } = string.Empty;
            public List<string> Groups { get; set; } = [];
        }

        private readonly List<TemplateEntry> _templates = [];

        private ListBox? _templateList;
        private TextBox? _searchBox;
        private TextBox? _templateNameBox;
        private ListBox? _groupList;
        private TextBox? _newGroupBox;

        private Button? _addTemplateButton;
        private Button? _editTemplateButton;
        private Button? _deleteTemplateButton;
        private Button? _addGroupButton;
        private Button? _removeGroupButton;
        private Button? _saveButton;
        private Button? _cancelButton;

        private TemplateEntry? _selectedTemplate;

        private bool _loading;
        private bool _isDirty;
        private bool _isNewTemplate;

        // =============================================================
        // BUILD TAB
        // =============================================================

        public TabPage BuildTabPage()
        {
            if (string.Equals(
                    _schema.SectionKey,
                    "templates",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BuildGroupTemplatesTab();
            }

            var tab = new TabPage(_schema.TabTitle);
            var panel = new Panel { Dock = DockStyle.Fill };

            _dgv = CreateGrid();

            var btnPanel = BuildButtonPanel();

            var lblInstruction = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                ForeColor = MutedColor,
                BackColor = PanelColor,
                Padding = new Padding(6),
                Text = _schema.InstructionText
            };

            panel.Controls.Add(_dgv);
            panel.Controls.Add(lblInstruction);
            panel.Controls.Add(btnPanel);

            tab.Controls.Add(panel);

            LoadFromDisk();

            return tab;
        }

        // =============================================================
        // GROUP TEMPLATES: TWO-PANEL EDITOR
        // =============================================================

        private TabPage BuildGroupTemplatesTab()
        {
            var tab = new TabPage(_schema.TabTitle)
            {
                BackColor = BackgroundColor,
                ForeColor = Color.White
            };

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = BackgroundColor,
                Padding = new Padding(8)
            };

            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var instruction = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = MutedColor,
                Text = _schema.InstructionText,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6,
                BackColor = BorderColor
            };

            split.Panel1.BackColor = PanelColor;
            split.Panel2.BackColor = BackgroundColor;

            // Assign the splitter distance after WinForms has assigned
            // the control's size to avoid InvalidOperationException.
            split.HandleCreated += (_, _) =>
                SetInitialSplitterDistance(split);

            split.SizeChanged += (_, _) =>
                SetInitialSplitterDistance(split);

            root.Controls.Add(instruction, 0, 0);
            root.Controls.Add(split, 0, 1);

            tab.Controls.Add(root);

            BuildTemplateListPanel(split.Panel1);
            BuildTemplateDetailsPanel(split.Panel2);

            LoadTemplates();

            return tab;
        }

        private static void SetInitialSplitterDistance(SplitContainer split)
        {
            if (!split.IsHandleCreated || split.ClientSize.Width <= 0)
                return;

            int availableWidth = split.ClientSize.Width - split.SplitterWidth;

            if (availableWidth < 2)
                return;

            split.Panel1MinSize = 0;
            split.Panel2MinSize = 0;

            int distance = Math.Clamp(270, 1, availableWidth - 1);

            if (split.SplitterDistance != distance)
                split.SplitterDistance = distance;
        }

        // =============================================================
        // LEFT PANEL: TEMPLATE LIST
        // =============================================================

        private void BuildTemplateListPanel(Control parent)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = PanelColor,
                Padding = new Padding(8)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            var heading = new Label
            {
                Text = "Templates",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _searchBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "Search templates...",
                BackColor = InputColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _templateList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = InputColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 42
            };

            _templateList.DrawItem += (_, e) =>
            {
                if (e.Index < 0)
                    return;

                e.DrawBackground();

                var item = (TemplateEntry)_templateList.Items[e.Index];
                bool selected = (e.State & DrawItemState.Selected) != 0;

                using var background = new SolidBrush(
                    selected ? SelectedColor : InputColor);

                e.Graphics.FillRectangle(background, e.Bounds);

                using var nameBrush = new SolidBrush(Color.White);
                using var countBrush = new SolidBrush(MutedColor);
                using var nameFont = new Font("Segoe UI", 9F, FontStyle.Regular);
                using var countFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);

                e.Graphics.DrawString(
                    item.Name,
                    nameFont,
                    nameBrush,
                    e.Bounds.Left + 8,
                    e.Bounds.Top + 4);

                e.Graphics.DrawString(
                    $"{item.Groups.Count} groups",
                    countFont,
                    countBrush,
                    e.Bounds.Left + 8,
                    e.Bounds.Top + 23);

                e.DrawFocusRectangle();
            };

            _templateList.SelectedIndexChanged += (_, _) =>
            {
                if (_loading)
                    return;

                if (_isDirty && !ConfirmDiscardChanges())
                {
                    _loading = true;

                    try
                    {
                        SelectTemplateInList(_selectedTemplate);
                    }
                    finally
                    {
                        _loading = false;
                    }

                    return;
                }

                // If the user discards changes to a newly created template,
                // remove that unsaved template before switching.
                if (_isDirty && _isNewTemplate && _selectedTemplate != null)
                {
                    _templates.Remove(_selectedTemplate);
                    _selectedTemplate = null;
                    _isDirty = false;
                    _isNewTemplate = false;

                    RefreshTemplateList();
                }

                if (_templateList.SelectedItem is TemplateEntry selected)
                {
                    LoadTemplateIntoEditor(selected);
                }
                else
                {
                    ClearTemplateEditor();
                }
            };

            _searchBox.TextChanged += (_, _) =>
            {
                if (_isDirty && !ConfirmDiscardChanges())
                {
                    _loading = true;

                    try
                    {
                        _searchBox.TextChanged -= SearchBoxTextChanged;
                    }
                    finally
                    {
                        _loading = false;
                    }

                    return;
                }

                RefreshTemplateList();
            };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = PanelColor,
                Padding = new Padding(0, 4, 0, 0)
            };

            _addTemplateButton = new HudButton
            {
                Text = "Add",
                Width = 60,
                Height = 30
            };

            _editTemplateButton = new HudButton
            {
                Text = "Edit",
                Width = 60,
                Height = 30,
                Enabled = false
            };

            _deleteTemplateButton = new HudButton
            {
                Text = "Delete",
                Width = 70,
                Height = 30,
                Enabled = false
            };

            _addTemplateButton.Click += (_, _) => AddTemplate();
            _editTemplateButton.Click += (_, _) => EditTemplate();
            _deleteTemplateButton.Click += (_, _) => DeleteTemplate();

            buttons.Controls.Add(_addTemplateButton);
            buttons.Controls.Add(_editTemplateButton);
            buttons.Controls.Add(_deleteTemplateButton);

            layout.Controls.Add(heading, 0, 0);
            layout.Controls.Add(_searchBox, 0, 1);
            layout.Controls.Add(_templateList, 0, 2);
            layout.Controls.Add(buttons, 0, 3);

            parent.Controls.Add(layout);
        }

        private void SearchBoxTextChanged(object? sender, EventArgs e)
        {
            RefreshTemplateList();
        }

        // =============================================================
        // RIGHT PANEL: TEMPLATE DETAILS
        // =============================================================

        private void BuildTemplateDetailsPanel(Control parent)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = BackgroundColor,
                Padding = new Padding(14, 8, 8, 8)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            var heading = new Label
            {
                Text = "Template Details",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var namePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = BackgroundColor
            };

            namePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            namePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var nameLabel = new Label
            {
                Text = "Template Name",
                Dock = DockStyle.Fill,
                ForeColor = MutedColor
            };

            _templateNameBox = new TextBox
            {
                Dock = DockStyle.Fill,
                BackColor = InputColor,
                ForeColor = MutedColor,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Enabled = false
            };

            namePanel.Controls.Add(nameLabel, 0, 0);
            namePanel.Controls.Add(_templateNameBox, 0, 1);

            var groupsHeader = new Label
            {
                Text = "Group Email Addresses",
                Dock = DockStyle.Fill,
                ForeColor = MutedColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _groupList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = InputColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                SelectionMode = SelectionMode.MultiExtended,
                Enabled = false
            };

            _groupList.SelectedIndexChanged += (_, _) =>
            {
                if (_removeGroupButton != null)
                {
                    _removeGroupButton.Enabled =
                        _groupList.Enabled &&
                        _groupList.SelectedItems.Count > 0;
                }
            };

            var groupAddPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = BackgroundColor
            };

            groupAddPanel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            groupAddPanel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 85));

            _newGroupBox = new TextBox
            {
                Dock = DockStyle.Fill,
                PlaceholderText = "group@yourdomain.org",
                BackColor = InputColor,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false
            };

            _newGroupBox.TextChanged += (_, _) =>
            {
                if (_addGroupButton != null)
                {
                    _addGroupButton.Enabled =
                        _groupList?.Enabled == true &&
                        !string.IsNullOrWhiteSpace(_newGroupBox.Text);
                }
            };

            _newGroupBox.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    AddGroup();
                }
            };

            _addGroupButton = new HudButton
            {
                Text = "Add Group",
                Dock = DockStyle.Fill,
                Enabled = false
            };

            _addGroupButton.Click += (_, _) => AddGroup();

            groupAddPanel.Controls.Add(_newGroupBox, 0, 0);
            groupAddPanel.Controls.Add(_addGroupButton, 1, 0);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = BackgroundColor,
                Padding = new Padding(0, 4, 0, 0)
            };

            _saveButton = new HudButton
            {
                Text = "Save",
                Width = 85,
                Height = 30,
                Enabled = false
            };

            _cancelButton = new HudButton
            {
                Text = "Cancel",
                Width = 85,
                Height = 30,
                Enabled = false
            };

            _removeGroupButton = new HudButton
            {
                Text = "Remove Group",
                Width = 115,
                Height = 30,
                Enabled = false
            };

            _saveButton.Click += (_, _) => SaveTemplate();
            _cancelButton.Click += (_, _) => CancelEditing();
            _removeGroupButton.Click += (_, _) => RemoveGroups();

            footer.Controls.Add(_saveButton);
            footer.Controls.Add(_cancelButton);
            footer.Controls.Add(_removeGroupButton);

            layout.Controls.Add(heading, 0, 0);
            layout.Controls.Add(namePanel, 0, 1);
            layout.Controls.Add(groupsHeader, 0, 2);
            layout.Controls.Add(_groupList, 0, 3);
            layout.Controls.Add(groupAddPanel, 0, 4);
            layout.Controls.Add(footer, 0, 5);

            parent.Controls.Add(layout);
        }

        // =============================================================
        // TEMPLATE LOADING AND SELECTION
        // =============================================================

        private void LoadTemplates()
        {
            _templates.Clear();

            if (File.Exists(_configPath))
            {
                try
                {
                    using var doc = JsonDocument.Parse(
                        File.ReadAllText(_configPath));

                    if (TryGetPropertyIgnoreCase(
                            doc.RootElement,
                            _schema.SectionKey,
                            out var section) &&
                        section.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in section.EnumerateArray())
                        {
                            if (!TryGetPropertyIgnoreCase(
                                    item, "name", out var nameElement))
                            {
                                continue;
                            }

                            string name = nameElement.GetString()?.Trim()
                                          ?? string.Empty;

                            if (string.IsNullOrWhiteSpace(name))
                                continue;

                            var entry = _templates.FirstOrDefault(t =>
                                string.Equals(
                                    t.Name, name,
                                    StringComparison.OrdinalIgnoreCase));

                            if (entry == null)
                            {
                                entry = new TemplateEntry { Name = name };
                                _templates.Add(entry);
                            }

                            if (TryGetPropertyIgnoreCase(
                                    item,
                                    _schema.NestedArrayPropertyName ?? "groups",
                                    out var groups) &&
                                groups.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var group in groups.EnumerateArray())
                                {
                                    string email = group.GetString()?.Trim()
                                                   ?? string.Empty;

                                    if (!string.IsNullOrWhiteSpace(email) &&
                                        !entry.Groups.Contains(
                                            email,
                                            StringComparer.OrdinalIgnoreCase))
                                    {
                                        entry.Groups.Add(email);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to load Group Templates: {ex.Message}",
                        "Configuration Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            RefreshTemplateList();

            if (_templates.Count > 0)
            {
                _loading = true;

                try
                {
                    SelectTemplateInList(_templates[0]);
                }
                finally
                {
                    _loading = false;
                }

                LoadTemplateIntoEditor(_templates[0]);
            }
            else
            {
                ClearTemplateEditor();
            }
        }

        private void RefreshTemplateList()
        {
            if (_templateList == null)
                return;

            string search = _searchBox?.Text.Trim() ?? string.Empty;
            TemplateEntry? selectedTemplate = _selectedTemplate;

            _loading = true;

            try
            {
                _templateList.BeginUpdate();
                _templateList.Items.Clear();

                foreach (var template in _templates
                             .Where(t => t.Name.Contains(
                                 search, StringComparison.OrdinalIgnoreCase))
                             .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase))
                {
                    _templateList.Items.Add(template);
                }

                SelectTemplateInList(selectedTemplate);
            }
            finally
            {
                _templateList.EndUpdate();
                _loading = false;
            }

            if (_selectedTemplate != null &&
                !_templateList.Items.Contains(_selectedTemplate))
            {
                ClearTemplateEditor();
            }

            _templateList.Invalidate();
        }

        private void SelectTemplateInList(TemplateEntry? template)
        {
            if (_templateList == null)
                return;

            _templateList.ClearSelected();

            if (template == null)
                return;

            for (int i = 0; i < _templateList.Items.Count; i++)
            {
                if (ReferenceEquals(_templateList.Items[i], template))
                {
                    _templateList.SelectedIndex = i;
                    return;
                }
            }
        }

        private void LoadTemplateIntoEditor(TemplateEntry template)
        {
            _loading = true;

            try
            {
                _selectedTemplate = template;
                _isNewTemplate = false;
                _isDirty = false;

                _templateNameBox!.Text = template.Name;

                // Template names are always locked.
                _templateNameBox.ReadOnly = true;
                _templateNameBox.Enabled = false;

                _groupList!.Items.Clear();

                foreach (string group in template.Groups)
                    _groupList.Items.Add(group);

                // Existing templates start in read-only mode.
                _groupList.Enabled = false;
                _newGroupBox!.Enabled = false;
                _newGroupBox.Clear();

                _addTemplateButton!.Enabled = true;
                _editTemplateButton!.Enabled = true;
                _deleteTemplateButton!.Enabled = true;

                _addGroupButton!.Enabled = false;
                _removeGroupButton!.Enabled = false;

                _saveButton!.Enabled = false;
                _cancelButton!.Enabled = false;
            }
            finally
            {
                _loading = false;
            }
        }

        private void ClearTemplateEditor()
        {
            _loading = true;

            try
            {
                _selectedTemplate = null;
                _isNewTemplate = false;
                _isDirty = false;

                _templateNameBox!.Clear();
                _templateNameBox.ReadOnly = true;
                _templateNameBox.Enabled = false;

                _groupList!.Items.Clear();
                _groupList.Enabled = false;

                _newGroupBox!.Clear();
                _newGroupBox.Enabled = false;

                _addGroupButton!.Enabled = false;
                _removeGroupButton!.Enabled = false;
                _saveButton!.Enabled = false;
                _cancelButton!.Enabled = false;

                _editTemplateButton!.Enabled = false;
                _deleteTemplateButton!.Enabled = false;
            }
            finally
            {
                _loading = false;
            }
        }

        // =============================================================
        // EDIT TEMPLATE
        // =============================================================

        private void EditTemplate()
        {
            if (_selectedTemplate == null)
                return;

            _isNewTemplate = false;
            _isDirty = false;

            // The template name must never be editable.
            _templateNameBox!.ReadOnly = true;
            _templateNameBox.Enabled = false;

            // Enable editing of group mappings only.
            _groupList!.Enabled = true;
            _newGroupBox!.Enabled = true;

            _addGroupButton!.Enabled =
                !string.IsNullOrWhiteSpace(_newGroupBox.Text);

            _removeGroupButton!.Enabled =
                _groupList.SelectedItems.Count > 0;

            _saveButton!.Enabled = false;
            _cancelButton!.Enabled = true;

            _editTemplateButton!.Enabled = false;

            _groupList.Focus();
        }

        // =============================================================
        // ADD TEMPLATE
        // =============================================================

        private void AddTemplate()
        {
            if (_isDirty)
            {
                if (!ConfirmDiscardChanges())
                    return;

                CancelEditing();
            }

            var template = new TemplateEntry
            {
                Name = GetUniqueTemplateName()
            };

            _templates.Add(template);

            RefreshTemplateList();
            SelectTemplateInList(template);
            LoadTemplateIntoEditor(template);

            _isNewTemplate = true;

            // Keep the generated name locked.
            _templateNameBox!.ReadOnly = true;
            _templateNameBox.Enabled = false;

            // A new template starts in editing mode.
            _groupList!.Enabled = true;
            _newGroupBox!.Enabled = true;

            _saveButton!.Enabled = false;
            _cancelButton!.Enabled = true;

            _editTemplateButton!.Enabled = false;
            _deleteTemplateButton!.Enabled = false;

            SetDirty();

            _newGroupBox.Focus();
        }

        private string GetUniqueTemplateName()
        {
            const string baseName = "New Template";

            if (!_templates.Any(t => string.Equals(
                    t.Name, baseName, StringComparison.OrdinalIgnoreCase)))
            {
                return baseName;
            }

            int number = 2;

            while (_templates.Any(t => string.Equals(
                       t.Name, $"{baseName} {number}",
                       StringComparison.OrdinalIgnoreCase)))
            {
                number++;
            }

            return $"{baseName} {number}";
        }

        // =============================================================
        // DELETE TEMPLATE
        // =============================================================

        private void DeleteTemplate()
        {
            if (_selectedTemplate == null)
                return;

            if (_isDirty)
            {
                if (!ConfirmDiscardChanges())
                    return;

                CancelEditing();

                if (_selectedTemplate == null)
                    return;
            }

            var template = _selectedTemplate;

            var result = MessageBox.Show(
                $"Delete template '{template.Name}' and its " +
                $"{template.Groups.Count} group mapping(s)?\r\n\r\n" +
                "This does not delete the actual Google Groups.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            _templates.Remove(template);

            if (!SaveTemplatesToDisk())
            {
                _templates.Add(template);
                RefreshTemplateList();
                SelectTemplateInList(template);
                LoadTemplateIntoEditor(template);
                return;
            }

            _selectedTemplate = null;
            _isDirty = false;

            RefreshTemplateList();

            if (_templates.Count > 0)
            {
                var next = _templates[0];

                SelectTemplateInList(next);
                LoadTemplateIntoEditor(next);
            }
            else
            {
                ClearTemplateEditor();
            }
        }

        // =============================================================
        // GROUP MAPPINGS
        // =============================================================

        private void AddGroup()
        {
            if (_selectedTemplate == null ||
                _newGroupBox == null ||
                _groupList == null ||
                !_groupList.Enabled)
            {
                return;
            }

            string email = _newGroupBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Enter a group email address.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!email.Contains('@') ||
                email.StartsWith('@') ||
                email.EndsWith('@') ||
                email.Any(char.IsWhiteSpace))
            {
                MessageBox.Show(
                    "Enter a valid group email address.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            bool existsInTemplate = _groupList.Items
                .Cast<string>()
                .Any(x => string.Equals(
                    x, email, StringComparison.OrdinalIgnoreCase));

            bool existsElsewhere = _templates
                .Where(t => !ReferenceEquals(t, _selectedTemplate))
                .Any(t => t.Groups.Any(x => string.Equals(
                    x, email, StringComparison.OrdinalIgnoreCase)));

            if (existsInTemplate || existsElsewhere)
            {
                MessageBox.Show(
                    existsInTemplate
                        ? "This group email is already in the selected template."
                        : "This group email is already assigned to another template.",
                    "Duplicate Group",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _groupList.Items.Add(email);
            _newGroupBox.Clear();

            SetDirty();
            _groupList.SelectedItem = email;
        }

        private void RemoveGroups()
        {
            if (_groupList == null ||
                !_groupList.Enabled ||
                _groupList.SelectedItems.Count == 0)
            {
                return;
            }

            var selected = _groupList.SelectedItems
                .Cast<string>()
                .ToList();

            foreach (string email in selected)
                _groupList.Items.Remove(email);

            SetDirty();
        }

        // =============================================================
        // SAVE / CANCEL
        // =============================================================

        private void SetDirty()
        {
            if (_loading)
                return;

            _isDirty = true;

            if (_saveButton != null)
                _saveButton.Enabled = true;

            if (_cancelButton != null)
                _cancelButton.Enabled = true;

            if (_editTemplateButton != null)
                _editTemplateButton.Enabled = false;

            if (_templateList != null)
                _templateList.Invalidate();
        }

        private bool ConfirmDiscardChanges()
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Discard them?",
                "Unsaved Changes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            return result == DialogResult.Yes;
        }

        private void CancelEditing()
        {
            if (_isNewTemplate && _selectedTemplate != null)
            {
                _templates.Remove(_selectedTemplate);
                _selectedTemplate = null;

                _isDirty = false;
                _isNewTemplate = false;

                RefreshTemplateList();

                if (_templates.Count > 0)
                {
                    var template = _templates[0];

                    SelectTemplateInList(template);
                    LoadTemplateIntoEditor(template);
                }
                else
                {
                    ClearTemplateEditor();
                }

                return;
            }

            if (_selectedTemplate != null)
            {
                LoadTemplateIntoEditor(_selectedTemplate);
            }
            else
            {
                ClearTemplateEditor();
            }
        }

        private void SaveTemplate()
        {
            if (_selectedTemplate == null)
                return;

            // The template name is generated when the template is created
            // and is intentionally not editable.
            string name = _selectedTemplate.Name;

            var groups = _groupList!.Items
                .Cast<string>()
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (groups.Count == 0)
            {
                MessageBox.Show(
                    "Add at least one group email address before saving.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            bool duplicateName = _templates.Any(t =>
                !ReferenceEquals(t, _selectedTemplate) &&
                string.Equals(
                    t.Name, name,
                    StringComparison.OrdinalIgnoreCase));

            if (duplicateName)
            {
                MessageBox.Show(
                    "A template with this name already exists.",
                    "Duplicate Template",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var originalGroups = _selectedTemplate.Groups.ToList();

            _selectedTemplate.Name = name;
            _selectedTemplate.Groups = groups;

            if (!SaveTemplatesToDisk())
            {
                _selectedTemplate.Groups = originalGroups;
                return;
            }

            _isDirty = false;
            _isNewTemplate = false;

            RefreshTemplateList();
            SelectTemplateInList(_selectedTemplate);
            LoadTemplateIntoEditor(_selectedTemplate);
        }

        // =============================================================
        // PERSIST GROUP TEMPLATES
        // =============================================================

        private bool SaveTemplatesToDisk()
        {
            string? tempPath = null;

            try
            {
                Dictionary<string, object> root;

                if (File.Exists(_configPath))
                {
                    using var doc = JsonDocument.Parse(
                        File.ReadAllText(_configPath));

                    root = JsonElementToDictionary(doc.RootElement);
                }
                else
                {
                    root = new Dictionary<string, object>(
                        StringComparer.OrdinalIgnoreCase);
                }

                string nestedProperty =
                    _schema.NestedArrayPropertyName ?? "groups";

                root[_schema.SectionKey] = _templates
                    .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(t => new Dictionary<string, object>(
                        StringComparer.OrdinalIgnoreCase)
                    {
                        ["name"] = t.Name,
                        [nestedProperty] = t.Groups.ToList()
                    })
                    .ToList();

                string json = JsonSerializer.Serialize(
                    root,
                    JsonSerializeOptions);

                string? directory = Path.GetDirectoryName(_configPath);

                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                tempPath = _configPath + ".tmp";

                File.WriteAllText(tempPath, json);

                if (File.Exists(_configPath))
                {
                    File.Replace(tempPath, _configPath, null);
                }
                else
                {
                    File.Move(tempPath, _configPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save Group Templates:\r\n\r\n{ex.Message}",
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            finally
            {
                if (tempPath != null && File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors.
                    }
                }
            }
        }

        // =============================================================
        // EXISTING GENERIC GRID EDITOR
        // =============================================================

        private DataGridView CreateGrid()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = PanelColor,
                GridColor = BorderColor
            };

            dgv.DefaultCellStyle.BackColor = InputColor;
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = SelectedColor;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = PanelColor;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(25, 35, 52);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 28;

            foreach (var col in _schema.Columns)
            {
                DataGridViewColumn gridCol =
                    col.FieldType == typeof(bool)
                        ? new DataGridViewCheckBoxColumn()
                        : new DataGridViewTextBoxColumn();

                gridCol.Name = col.Name;
                gridCol.HeaderText = col.DisplayName;
                gridCol.ToolTipText = col.ToolTip;

                dgv.Columns.Add(gridCol);
            }

            return dgv;
        }

        private FlowLayoutPanel BuildButtonPanel()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                Padding = new Padding(4),
                BackColor = PanelColor
            };

            var add = new HudButton { Text = "Add", Width = 80 };
            var edit = new HudButton { Text = "Edit", Width = 80 };
            var delete = new HudButton { Text = "Delete", Width = 80 };

            add.Click += (_, _) => OnAddClicked();
            edit.Click += (_, _) => OnEditClicked();
            delete.Click += (_, _) => OnDeleteClicked();

            panel.Controls.Add(add);
            panel.Controls.Add(edit);
            panel.Controls.Add(delete);

            return panel;
        }

        private void OnAddClicked()
        {
            var values = ShowItemDialog("Add Item", null);
            if (values == null)
                return;

            var row = new object[_schema.Columns.Count];

            for (int i = 0; i < _schema.Columns.Count; i++)
                row[i] = values[_schema.Columns[i].Name] ?? string.Empty;

            _dgv.Rows.Add(row);
            SaveToDisk();
        }

        private void OnEditClicked()
        {
            if (_dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Select a row to edit.",
                    "Edit",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var row = _dgv.SelectedRows[0];

            var current = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < _schema.Columns.Count; i++)
            {
                current[_schema.Columns[i].Name] =
                    row.Cells[i].Value?.ToString() ?? string.Empty;
            }

            var values = ShowItemDialog("Edit Item", current);
            if (values == null)
                return;

            for (int i = 0; i < _schema.Columns.Count; i++)
                row.Cells[i].Value =
                    values[_schema.Columns[i].Name] ?? string.Empty;

            SaveToDisk();
        }

        private void OnDeleteClicked()
        {
            if (_dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Select a row to delete.",
                    "Delete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(
                    "Delete the selected row?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            foreach (DataGridViewRow row in _dgv.SelectedRows)
            {
                if (!row.IsNewRow)
                    _dgv.Rows.Remove(row);
            }

            SaveToDisk();
        }

        private Dictionary<string, string>? ShowItemDialog(
            string title,
            Dictionary<string, string>? existingValues)
        {
            using var form = new Form
            {
                Width = 420,
                Height = 80 + (_schema.Columns.Count * 40),
                StartPosition = FormStartPosition.CenterParent,
                Text = title,
                BackColor = BackgroundColor,
                ForeColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var inputs = new Dictionary<string, Control>();
            int y = 15;

            foreach (var col in _schema.Columns)
            {
                var label = new Label
                {
                    Text = col.DisplayName + ":",
                    Left = 15,
                    Top = y + 4,
                    Width = 120,
                    ForeColor = Color.White
                };

                form.Controls.Add(label);

                Control input;

                if (col.FieldType == typeof(bool))
                {
                    bool isChecked = existingValues != null &&
                        existingValues.TryGetValue(col.Name, out var value) &&
                        bool.TryParse(value, out var parsed) && parsed;

                    input = new CheckBox
                    {
                        Left = 145,
                        Top = y,
                        Checked = isChecked
                    };
                }
                else
                {
                    string value = existingValues != null &&
                        existingValues.TryGetValue(col.Name, out var existing)
                            ? existing
                            : col.DefaultValue?.ToString() ?? string.Empty;

                    input = new TextBox
                    {
                        Left = 145,
                        Top = y,
                        Width = 250,
                        Text = value,
                        PlaceholderText = col.Placeholder
                    };
                }

                form.Controls.Add(input);
                inputs[col.Name] = input;
                y += 35;
            }

            var ok = new Button
            {
                Text = "OK",
                Left = 145,
                Top = y + 5,
                Width = 80,
                DialogResult = DialogResult.OK
            };

            var cancel = new Button
            {
                Text = "Cancel",
                Left = 235,
                Top = y + 5,
                Width = 80,
                DialogResult = DialogResult.Cancel
            };

            form.Controls.Add(ok);
            form.Controls.Add(cancel);
            form.AcceptButton = ok;
            form.CancelButton = cancel;

            if (form.ShowDialog() != DialogResult.OK)
                return null;

            var result = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var col in _schema.Columns)
            {
                Control control = inputs[col.Name];

                result[col.Name] = control is CheckBox checkBox
                    ? checkBox.Checked.ToString()
                    : ((TextBox)control).Text.Trim();
            }

            foreach (var col in _schema.Columns)
            {
                string value = result[col.Name];

                if (col.Required &&
                    col.FieldType != typeof(bool) &&
                    string.IsNullOrWhiteSpace(value))
                {
                    MessageBox.Show(
                        $"{col.DisplayName} is required.",
                        "Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return null;
                }

                if (col.Validator != null &&
                    !string.IsNullOrWhiteSpace(value) &&
                    !col.Validator(value))
                {
                    MessageBox.Show(
                        string.IsNullOrWhiteSpace(col.ValidationError)
                            ? $"{col.DisplayName} is invalid."
                            : col.ValidationError,
                        "Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return null;
                }
            }

            return result;
        }

        private void LoadFromDisk()
        {
            _dgv.Rows.Clear();

            if (!File.Exists(_configPath))
                return;

            try
            {
                using var doc = JsonDocument.Parse(
                    File.ReadAllText(_configPath));

                if (!TryGetPropertyIgnoreCase(
                        doc.RootElement, _schema.SectionKey, out var section) ||
                    section.ValueKind != JsonValueKind.Array)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(_schema.GroupByColumn))
                {
                    foreach (var item in section.EnumerateArray())
                    {
                        var row = new object[_schema.Columns.Count];

                        for (int i = 0; i < _schema.Columns.Count; i++)
                            row[i] = ReadJsonValue(item, _schema.Columns[i]);

                        _dgv.Rows.Add(row);
                    }
                }
                else
                {
                    string groupColumn = _schema.GroupByColumn;
                    string nestedProperty =
                        _schema.NestedArrayPropertyName ?? string.Empty;

                    foreach (var item in section.EnumerateArray())
                    {
                        string groupName = TryGetPropertyIgnoreCase(
                            item, "name", out var nameElement)
                                ? nameElement.GetString() ?? string.Empty
                                : string.Empty;

                        if (!TryGetPropertyIgnoreCase(
                                item, nestedProperty, out var nestedArray) ||
                            nestedArray.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        foreach (var nestedItem in nestedArray.EnumerateArray())
                        {
                            string value = nestedItem.GetString() ?? string.Empty;

                            var row = new object[_schema.Columns.Count];

                            for (int i = 0; i < _schema.Columns.Count; i++)
                            {
                                row[i] = string.Equals(
                                    _schema.Columns[i].Name,
                                    groupColumn,
                                    StringComparison.OrdinalIgnoreCase)
                                        ? groupName
                                        : value;
                            }

                            _dgv.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load {_schema.TabTitle}: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string ReadJsonValue(JsonElement item, ColumnSchema col)
        {
            if (!TryGetPropertyIgnoreCase(item, col.Name, out var element))
                return col.FieldType == typeof(bool) ? "False" : string.Empty;

            if (col.FieldType == typeof(bool))
                return element.ValueKind == JsonValueKind.True ? "True" : "False";

            return element.ValueKind == JsonValueKind.String
                ? element.GetString() ?? string.Empty
                : element.ToString();
        }

        private void SaveToDisk()
        {
            try
            {
                Dictionary<string, object> root;

                if (File.Exists(_configPath))
                {
                    using var doc = JsonDocument.Parse(
                        File.ReadAllText(_configPath));

                    root = JsonElementToDictionary(doc.RootElement);
                }
                else
                {
                    root = new Dictionary<string, object>(
                        StringComparer.OrdinalIgnoreCase);
                }

                if (string.IsNullOrWhiteSpace(_schema.GroupByColumn))
                {
                    var rows = new List<Dictionary<string, object>>();

                    foreach (DataGridViewRow row in _dgv.Rows)
                    {
                        if (row.IsNewRow)
                            continue;

                        var item = new Dictionary<string, object>(
                            StringComparer.OrdinalIgnoreCase);

                        for (int i = 0; i < _schema.Columns.Count; i++)
                        {
                            var col = _schema.Columns[i];
                            object? value = row.Cells[i].Value;

                            item[col.Name] = col.FieldType == typeof(bool)
                                ? Convert.ToBoolean(value ?? false)
                                : value?.ToString() ?? string.Empty;
                        }

                        rows.Add(item);
                    }

                    root[_schema.SectionKey] = rows;
                }
                else
                {
                    string groupColumn = _schema.GroupByColumn;
                    string nestedProperty =
                        _schema.NestedArrayPropertyName ?? "items";

                    int groupIndex = _schema.Columns.FindIndex(c =>
                        c.Name == groupColumn);

                    int valueIndex = _schema.Columns.FindIndex(c =>
                        c.Name != groupColumn);

                    var grouped = new Dictionary<string, List<string>>(
                        StringComparer.OrdinalIgnoreCase);

                    foreach (DataGridViewRow row in _dgv.Rows)
                    {
                        if (row.IsNewRow)
                            continue;

                        string name = row.Cells[groupIndex].Value?.ToString()
                                      ?.Trim() ?? string.Empty;

                        string value = row.Cells[valueIndex].Value?.ToString()
                                       ?.Trim() ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(name) ||
                            string.IsNullOrWhiteSpace(value))
                        {
                            continue;
                        }

                        if (!grouped.TryGetValue(name, out var values))
                        {
                            values = [];
                            grouped[name] = values;
                        }

                        if (!values.Contains(value,
                                StringComparer.OrdinalIgnoreCase))
                        {
                            values.Add(value);
                        }
                    }

                    root[_schema.SectionKey] = grouped.Select(pair =>
                        new Dictionary<string, object>(
                            StringComparer.OrdinalIgnoreCase)
                        {
                            ["name"] = pair.Key,
                            [nestedProperty] = pair.Value
                        }).ToList();
                }

                string json = JsonSerializer.Serialize(
                    root, JsonSerializeOptions);

                string? directory = Path.GetDirectoryName(_configPath);

                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save {_schema.TabTitle}: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static bool TryGetPropertyIgnoreCase(
            JsonElement element,
            string propertyName,
            out JsonElement value)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (string.Equals(
                            property.Name,
                            propertyName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        private static Dictionary<string, object> JsonElementToDictionary(
            JsonElement element)
        {
            var result = new Dictionary<string, object>(
                StringComparer.OrdinalIgnoreCase);

            if (element.ValueKind != JsonValueKind.Object)
                return result;

            foreach (var property in element.EnumerateObject())
            {
                result[property.Name] =
                    JsonElementToObjectGeneric(property.Value);
            }

            return result;
        }

        private static object JsonElementToObjectGeneric(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    return JsonElementToDictionary(element);

                case JsonValueKind.Array:
                    var list = new List<object>();

                    foreach (var item in element.EnumerateArray())
                        list.Add(JsonElementToObjectGeneric(item));

                    return list;

                case JsonValueKind.String:
                    return element.GetString() ?? string.Empty;

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.Number:
                    return element.TryGetInt64(out long number)
                        ? number
                        : element.GetDouble();

                default:
                    return string.Empty;
            }
        }
    }
}
