using AdminTrayTool.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace AdminTrayTool.Forms
{
    public class GenericSectionEditor
    {
        private readonly SectionSchema _schema;
        private readonly string _configPath;
        private DataGridView _dgv = null!;

        public GenericSectionEditor(SectionSchema schema, string configPath)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
        }

        // =============================================================
        // BUILD TAB
        // =============================================================

        public TabPage BuildTabPage()
        {
            var tab = new TabPage(_schema.TabTitle);
            var panel = new Panel { Dock = DockStyle.Fill };

            _dgv = CreateGrid();

            var btnPanel = BuildButtonPanel();

            var lblInstruction = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                ForeColor = Color.FromArgb(150, 160, 175),
                BackColor = Color.FromArgb(16, 23, 36),
                Padding = new Padding(6, 6, 6, 6),
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
        // GRID CREATION
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
                BackgroundColor = Color.FromArgb(16, 23, 36),
                GridColor = Color.FromArgb(45, 55, 70)
            };

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(20, 27, 40);
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 70, 115);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(16, 23, 36);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 35, 52);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 28;

            // One grid column per schema column - this is what makes the
            // same code work for Web Portals (3 columns), RDP (2), Tools (4),
            // and Group Templates (2), just by changing the schema passed in.
            foreach (var col in _schema.Columns)
            {
                DataGridViewColumn gridCol = col.FieldType == typeof(bool)
                    ? new DataGridViewCheckBoxColumn()
                    : new DataGridViewTextBoxColumn();

                gridCol.Name = col.Name;
                gridCol.HeaderText = col.DisplayName;
                gridCol.ToolTipText = col.ToolTip;

                dgv.Columns.Add(gridCol);
            }

            return dgv;
        }

        // =============================================================
        // BUTTON PANEL (Add / Edit / Delete)
        // =============================================================

        private FlowLayoutPanel BuildButtonPanel()
        {
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                Padding = new Padding(4),
                BackColor = Color.FromArgb(16, 23, 36)
            };

            var btnAdd = new HudButton { Text = "Add", Width = 80 };
            var btnEdit = new HudButton { Text = "Edit", Width = 80 };
            var btnDelete = new HudButton { Text = "Delete", Width = 80 };

            btnAdd.Click += (s, e) => OnAddClicked();
            btnEdit.Click += (s, e) => OnEditClicked();
            btnDelete.Click += (s, e) => OnDeleteClicked();

            btnPanel.Controls.Add(btnAdd);
            btnPanel.Controls.Add(btnEdit);
            btnPanel.Controls.Add(btnDelete);

            return btnPanel;
        }

        // =============================================================
        // ADD
        // =============================================================

        private void OnAddClicked()
        {
            var values = ShowItemDialog("Add Item", null);
            if (values == null) return;

            var row = new object[_schema.Columns.Count];
            for (int i = 0; i < _schema.Columns.Count; i++)
            {
                row[i] = values[_schema.Columns[i].Name] ?? string.Empty;
            }

            _dgv.Rows.Add(row);
            SaveToDisk();
        }

        // =============================================================
        // EDIT
        // =============================================================

        private void OnEditClicked()
        {
            if (_dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a row to edit.", "Edit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var row = _dgv.SelectedRows[0];

            var currentValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _schema.Columns.Count; i++)
            {
                currentValues[_schema.Columns[i].Name] = row.Cells[i].Value?.ToString() ?? string.Empty;
            }

            var values = ShowItemDialog("Edit Item", currentValues);
            if (values == null) return;

            for (int i = 0; i < _schema.Columns.Count; i++)
            {
                row.Cells[i].Value = values[_schema.Columns[i].Name] ?? string.Empty;
            }

            SaveToDisk();
        }

        // =============================================================
        // DELETE
        // =============================================================

        private void OnDeleteClicked()
        {
            if (_dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a row to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                "Delete the selected row?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            foreach (DataGridViewRow row in _dgv.SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    _dgv.Rows.Remove(row);
                }
            }

            SaveToDisk();
        }

        // =============================================================
        // ADD/EDIT DIALOG (built generically from _schema.Columns)
        // =============================================================

        private Dictionary<string, string>? ShowItemDialog(string title, Dictionary<string, string>? existingValues)
        {
            using var form = new Form
            {
                Width = 420,
                Height = 80 + (_schema.Columns.Count * 40),
                StartPosition = FormStartPosition.CenterParent,
                Text = title,
                BackColor = Color.FromArgb(10, 15, 25),
                ForeColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var inputs = new Dictionary<string, Control>();
            int y = 15;

            foreach (var col in _schema.Columns)
            {
                var lbl = new Label
                {
                    Text = col.DisplayName + ":",
                    Left = 15,
                    Top = y + 4,
                    Width = 120,
                    ForeColor = Color.White
                };
                form.Controls.Add(lbl);

                Control input;

                if (col.FieldType == typeof(bool))
                {
                    bool isChecked = existingValues != null &&
                        existingValues.TryGetValue(col.Name, out var v) &&
                        bool.TryParse(v, out var b) && b;

                    input = new CheckBox { Left = 145, Top = y, Checked = isChecked };
                }
                else
                {
                    string text = existingValues != null && existingValues.TryGetValue(col.Name, out var existing)
                        ? existing
                        : (col.DefaultValue?.ToString() ?? string.Empty);

                    input = new TextBox { Left = 145, Top = y, Width = 250, Text = text };
                }

                form.Controls.Add(input);
                inputs[col.Name] = input;

                y += 35;
            }

            var btnOk = new Button { Text = "OK", Left = 145, Top = y + 5, Width = 80, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Cancel", Left = 235, Top = y + 5, Width = 80, DialogResult = DialogResult.Cancel };

            form.Controls.Add(btnOk);
            form.Controls.Add(btnCancel);
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            if (form.ShowDialog() != DialogResult.OK) return null;

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in _schema.Columns)
            {
                var control = inputs[col.Name];

                string value = control is CheckBox cb
                    ? cb.Checked.ToString()
                    : ((TextBox)control).Text.Trim();

                result[col.Name] = value;
            }

            // Validate after collecting all values
            foreach (var col in _schema.Columns)
            {
                string value = result[col.Name];

                if (col.Required && col.FieldType != typeof(bool) && string.IsNullOrWhiteSpace(value))
                {
                    MessageBox.Show($"{col.DisplayName} is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                if (col.Validator != null && !string.IsNullOrWhiteSpace(value) && !col.Validator(value))
                {
                    MessageBox.Show(
                        string.IsNullOrWhiteSpace(col.ValidationError) ? $"{col.DisplayName} is invalid." : col.ValidationError,
                        "Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return null;
                }
            }

            return result;
        }

        // =============================================================
        // LOAD FROM DISK
        // =============================================================

        private void LoadFromDisk()
        {
            _dgv.Rows.Clear();

            if (!File.Exists(_configPath)) return;

            try
            {
                string text = File.ReadAllText(_configPath);
                using var doc = JsonDocument.Parse(text);

                if (!doc.RootElement.TryGetProperty(_schema.SectionKey, out var sectionElement))
                    return;

                if (sectionElement.ValueKind != JsonValueKind.Array)
                    return;

                if (string.IsNullOrWhiteSpace(_schema.GroupByColumn))
                {
                    // Flat shape: one JSON object per row (Web Portals, RDP, Tools)
                    foreach (var item in sectionElement.EnumerateArray())
                    {
                        var row = new object[_schema.Columns.Count];
                        for (int i = 0; i < _schema.Columns.Count; i++)
                        {
                            var col = _schema.Columns[i];
                            row[i] = ReadJsonValue(item, col);
                        }
                        _dgv.Rows.Add(row);
                    }
                }
                else
                {
                    // Grouped/nested shape: each JSON object has a name +
                    // a nested array (Group Templates: Template -> [Groups])
                    string groupByCol = _schema.GroupByColumn;
                    string nestedProp = _schema.NestedArrayPropertyName ?? string.Empty;

                    var groupColumn = _schema.Columns.First(c => c.Name == groupByCol);
                    var valueColumn = _schema.Columns.First(c => c.Name != groupByCol);

                    foreach (var item in sectionElement.EnumerateArray())
                    {
                        string groupName = item.TryGetProperty(GetJsonPropertyNameForGroup(), out var nameEl)
                            ? nameEl.GetString() ?? string.Empty
                            : string.Empty;

                        if (item.TryGetProperty(nestedProp, out var nestedArray) && nestedArray.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var nestedItem in nestedArray.EnumerateArray())
                            {
                                string value = nestedItem.GetString() ?? string.Empty;
                                _dgv.Rows.Add(groupName, value);
                            }
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
            if (!item.TryGetProperty(col.Name, out var el))
                return col.FieldType == typeof(bool) ? "False" : string.Empty;

            if (col.FieldType == typeof(bool))
                return (el.ValueKind == JsonValueKind.True).ToString();

            return el.GetString() ?? string.Empty;
        }

        // For the grouped case, the "name" property in the JSON object is
        // assumed to be called "name" (e.g. { "name": "Primary Staff", "groups": [...] }).
        private static string GetJsonPropertyNameForGroup() => "name";

        // =============================================================
        // SAVE TO DISK
        // =============================================================

        private void SaveToDisk()
        {
            Dictionary<string, object> root;

            if (File.Exists(_configPath))
            {
                try
                {
                    string existingText = File.ReadAllText(_configPath);
                    using var doc = JsonDocument.Parse(existingText);
                    root = JsonElementToDictionary(doc.RootElement);
                }
                catch
                {
                    root = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                }
            }
            else
            {
                root = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            if (string.IsNullOrWhiteSpace(_schema.GroupByColumn))
            {
                // Flat shape
                var list = new List<Dictionary<string, object>>();

                foreach (DataGridViewRow row in _dgv.Rows)
                {
                    if (row.IsNewRow) continue;

                    var obj = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < _schema.Columns.Count; i++)
                    {
                        var col = _schema.Columns[i];
                        var cellValue = row.Cells[i].Value;

                        obj[col.Name] = col.FieldType == typeof(bool)
                            ? (object)Convert.ToBoolean(cellValue ?? false)
                            : (object)(cellValue?.ToString() ?? string.Empty);
                    }
                    list.Add(obj);
                }

                root[_schema.SectionKey] = list;
            }
            else
            {
                // Grouped/nested shape
                string groupByCol = _schema.GroupByColumn;
                string nestedProp = _schema.NestedArrayPropertyName ?? "items";

                int groupColIndex = _schema.Columns.FindIndex(c => c.Name == groupByCol);
                int valueColIndex = _schema.Columns.FindIndex(c => c.Name != groupByCol);

                var grouped = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

                foreach (DataGridViewRow row in _dgv.Rows)
                {
                    if (row.IsNewRow) continue;

                    string groupName = row.Cells[groupColIndex].Value?.ToString()?.Trim() ?? string.Empty;
                    string value = row.Cells[valueColIndex].Value?.ToString()?.Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(value))
                        continue;

                    if (!grouped.TryGetValue(groupName, out var list))
                    {
                        list = new List<string>();
                        grouped[groupName] = list;
                    }

                    if (!list.Contains(value, StringComparer.OrdinalIgnoreCase))
                    {
                        list.Add(value);
                    }
                }

                var sectionList = grouped.Select(kv => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["name"] = kv.Key,
                    [nestedProp] = kv.Value
                }).ToList();

                root[_schema.SectionKey] = sectionList;
            }

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(root, options);
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

        // Helper: JsonElement -> plain Dictionary/List/string/bool tree,
        // needed so we can merge sections without disturbing others in the same file.
        private static Dictionary<string, object> JsonElementToDictionary(JsonElement elem)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (elem.ValueKind != JsonValueKind.Object)
                return dict;

            foreach (var prop in elem.EnumerateObject())
            {
                dict[prop.Name] = JsonElementToObjectGeneric(prop.Value);
            }

            return dict;
        }

        private static object JsonElementToObjectGeneric(JsonElement elem)
        {
            switch (elem.ValueKind)
            {
                case JsonValueKind.Object:
                    return JsonElementToDictionary(elem);
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in elem.EnumerateArray())
                        list.Add(JsonElementToObjectGeneric(item));
                    return list;
                case JsonValueKind.String:
                    return elem.GetString() ?? string.Empty;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Number:
                    return elem.TryGetInt64(out var l) ? l : elem.GetDouble();
                default:
                    return string.Empty;
            }
        }
    }
}