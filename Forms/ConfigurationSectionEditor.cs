using AdminTrayTool.Models;
using AdminTrayTool.UI;
using System.Text.Json;

namespace AdminTrayTool.Forms
{
    public class ConfigurationSectionEditor : UserControl
    {
        private readonly string _path;
        private readonly string _sectionKey;
        private readonly SectionSchema _schema;

        private readonly Dictionary<string, Control> _controls = [];

        private Button _btnSave = null!;

        private static readonly JsonSerializerOptions JsonDeserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly JsonSerializerOptions JsonSerializeOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        public ConfigurationSectionEditor(
            SectionSchema schema,
            string path)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _sectionKey = schema.SectionKey;

            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(10, 15, 25);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            BuildInterface();
            LoadValues();
        }

        private void BuildInterface()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25),
                BackColor = Color.FromArgb(10, 15, 25)
            };

            var instructionLabel = new Label
            {
                Text = _schema.InstructionText,
                Dock = DockStyle.Top,
                Height = 55,
                AutoSize = false,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var fieldsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            int y = 20;

            foreach (var column in _schema.Columns)
            {
                var label = new Label
                {
                    Text = column.DisplayName,
                    Location = new Point(10, y),
                    Size = new Size(220, 30),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Control editor;

                if (column.FieldType == typeof(bool))
                {
                    editor = new CheckBox
                    {
                        Location = new Point(240, y + 3),
                        Size = new Size(400, 30),
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        Text = column.Placeholder
                    };
                }
                else
                {
                    editor = new TextBox
                    {
                        Location = new Point(240, y),
                        Size = new Size(500, 30),
                        BackColor = Color.FromArgb(25, 32, 45),
                        ForeColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    if (!string.IsNullOrWhiteSpace(column.Placeholder))
                    {
                        editor.AccessibleName = column.Placeholder;
                    }
                }

                if (!string.IsNullOrWhiteSpace(column.ToolTip))
                {
                    var toolTip = new ToolTip();
                    toolTip.SetToolTip(editor, column.ToolTip);
                }

                fieldsPanel.Controls.Add(label);
                fieldsPanel.Controls.Add(editor);

                _controls[column.Name] = editor;

                y += 65;
            }

            _btnSave = new HudButton
            {
                Text = "SAVE",
                Size = new Size(110, 38),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = Color.Transparent
            };

            bottomPanel.Controls.Add(_btnSave);

            bottomPanel.Resize += (sender, e) =>
            {
                _btnSave.Location =
                    new Point(
                        bottomPanel.Width - _btnSave.Width,
                        5);
            };

            _btnSave.Location =
                new Point(
                    bottomPanel.Width - _btnSave.Width,
                    5);

            _btnSave.Click += BtnSave_Click;

            mainPanel.Controls.Add(fieldsPanel);
            mainPanel.Controls.Add(bottomPanel);
            mainPanel.Controls.Add(instructionLabel);

            Controls.Add(mainPanel);
        }

        private void LoadValues()
        {
            try
            {
                if (!File.Exists(_path))
                    return;

                string json =
                    File.ReadAllText(_path);

                using JsonDocument document =
                    JsonDocument.Parse(json);

                if (!document.RootElement.TryGetProperty(
                        _sectionKey,
                        out JsonElement section))
                {
                    return;
                }

                foreach (var column in _schema.Columns)
                {
                    if (!section.TryGetProperty(
                            column.Name,
                            out JsonElement value))
                    {
                        continue;
                    }

                    if (!_controls.TryGetValue(
                            column.Name,
                            out Control? control))
                    {
                        continue;
                    }

                    if (column.FieldType == typeof(bool) &&
                        control is CheckBox checkBox)
                    {
                        if (value.ValueKind == JsonValueKind.True ||
                            value.ValueKind == JsonValueKind.False)
                        {
                            checkBox.Checked =
                                value.GetBoolean();
                        }
                    }
                    else if (control is TextBox textBox)
                    {
                        textBox.Text =
                            value.ValueKind == JsonValueKind.String
                                ? value.GetString() ?? string.Empty
                                : value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load {_sectionKey} configuration:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(
            object? sender,
            EventArgs e)
        {
            try
            {
                foreach (var column in _schema.Columns)
                {
                    if (!_controls.TryGetValue(
                            column.Name,
                            out Control? control))
                    {
                        continue;
                    }

                    string value;

                    if (control is CheckBox checkBox)
                    {
                        value = checkBox.Checked
                            ? "true"
                            : "false";
                    }
                    else if (control is TextBox textBox)
                    {
                        value = textBox.Text.Trim();
                    }
                    else
                    {
                        continue;
                    }

                    if (column.Required &&
                        string.IsNullOrWhiteSpace(value))
                    {
                        MessageBox.Show(
                            $"{column.DisplayName} is required.",
                            "Configuration",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        control.Focus();
                        return;
                    }

                    if (column.Validator != null &&
                        !column.Validator(value))
                    {
                        MessageBox.Show(
                            column.ValidationError,
                            "Configuration",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        control.Focus();
                        return;
                    }
                }

                string json =
                    File.ReadAllText(_path);

                using JsonDocument document =
                    JsonDocument.Parse(json);

                var root =
                    JsonSerializer.Deserialize<Dictionary<string, object>>(
                        json,
                        JsonDeserializeOptions)
                    ?? [];

                var section =
                    new Dictionary<string, object>();

                foreach (var column in _schema.Columns)
                {
                    Control control =
                        _controls[column.Name];

                    if (column.FieldType == typeof(bool) &&
                        control is CheckBox checkBox)
                    {
                        section[column.Name] =
                            checkBox.Checked;
                    }
                    else if (control is TextBox textBox)
                    {
                        section[column.Name] =
                            textBox.Text.Trim();
                    }
                }

                root[_sectionKey] = section;

                var options =
                    new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    };

                string output =
                    JsonSerializer.Serialize(
                        root,
                        JsonSerializeOptions);

                File.WriteAllText(
                    _path,
                    output);

                MessageBox.Show(
                    $"{_schema.TabTitle} configuration saved successfully.",
                    "Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "Access was denied while saving the configuration file. AdminTrayTool may need to be run as administrator.",
                    "Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save {_sectionKey} configuration:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
