using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AdminTrayTool
{
    public class ConfigEditorForm : Form
    {
        private readonly string _path;
        private readonly string? _initialJson;

        private TextBox _txtEditorPath = null!;
        private TabControl _tabs = null!;
        private DataGridView _dgvWeb = null!;
        private DataGridView _dgvRdp = null!;
        private DataGridView _dgvTools = null!;
        private Button _btnSave = null!;
        private Button _btnCancel = null!;
        private Label _statusLabel = null!;
        private System.Windows.Forms.Timer _statusTimer = null!;
        private bool _configValid = true; // true when the on-disk config parsed correctly

        public ConfigEditorForm(string path, string? initialJson = null)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _initialJson = initialJson;
            InitializeComponent();
            LoadJson();
        }

        // Show a non-modal status message in the editor and auto-clear after the specified seconds
        private void ShowStatus(string message, int seconds = 3)
        {
            try
            {
                if (_statusLabel != null && _statusTimer != null)
                {
                    _statusLabel.Text = message ?? string.Empty;
                    _statusTimer.Interval = Math.Max(500, seconds * 1000);
                    _statusTimer.Stop();
                    _statusTimer.Start();
                }
                else
                {
                    // Fallback to logging when UI not available
                    Logger.LogInfo(message ?? string.Empty);
                }
            }
            catch
            {
                // swallow any UI exceptions from status reporting
                try { Logger.LogInfo(message ?? string.Empty); } catch { }
            }
        }

        private Dictionary<string, object>? ShowEditItemDialog(DataGridView dgv, Dictionary<string, object> currentValues)
{
    if (currentValues is null) throw new ArgumentNullException(nameof(currentValues));

    using var form = new Form { Width = 420, Height = 220, StartPosition = FormStartPosition.CenterParent, Text = "Edit Item" };
    var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
    form.Controls.Add(panel);

    int y = 6;
    void AddLabel(string text)
    {
        var lbl = new Label { Text = text, Left = 8, Top = y + 4, Width = 100 };
        panel.Controls.Add(lbl);
    }
    TextBox? tb1 = null, tb2 = null, tb3 = null; CheckBox? cbElev = null;

    if (dgv == _dgvWeb)
    {
        AddLabel("Name:"); tb1 = new TextBox { Left = 110, Top = y, Width = 280, Text = currentValues.GetValueOrDefault("name")?.ToString() ?? string.Empty }; panel.Controls.Add(tb1); y += 30;
        AddLabel("URL:"); tb2 = new TextBox { Left = 110, Top = y, Width = 280, Text = currentValues.GetValueOrDefault("url")?.ToString() ?? string.Empty }; panel.Controls.Add(tb2); y += 30;
        AddLabel("Profile:"); tb3 = new TextBox { Left = 110, Top = y, Width = 280, Text = currentValues.GetValueOrDefault("profile")?.ToString() ?? "Profile 1" }; panel.Controls.Add(tb3); y += 30;
    }
    else if (dgv == _dgvRdp)
    {
        AddLabel("Name:"); tb1 = new TextBox { Left = 110, Top = y, Width = 280, Text = currentValues.GetValueOrDefault("name")?.ToString() ?? string.Empty }; panel.Controls.Add(tb1); y += 30;
        AddLabel("Host:"); tb2 = new TextBox { Left = 110, Top = y, Width = 280, Text = currentValues.GetValueOrDefault("host")?.ToString() ?? string.Empty }; panel.Controls.Add(tb2); y += 30;
    }
    else if (dgv == _dgvTools)
    {
        AddLabel("Name:"); tb1 = new TextBox { Left = 110, Top = y, Width = 280, Text = currentValues.GetValueOrDefault("name")?.ToString() ?? string.Empty }; panel.Controls.Add(tb1); y += 30;
        AddLabel("Exe:"); tb2 = new TextBox { Left = 110, Top = y, Width = 280, Text = currentValues.GetValueOrDefault("exe")?.ToString() ?? string.Empty }; panel.Controls.Add(tb2); y += 30;
        AddLabel("Args:"); tb3 = new TextBox { Left = 110, Top = y, Width = 280, Text = currentValues.GetValueOrDefault("args")?.ToString() ?? string.Empty }; panel.Controls.Add(tb3); y += 30;
        cbElev = new CheckBox { Text = "Elevated", Left = 110, Top = y, Width = 100, Checked = Convert.ToBoolean(currentValues.GetValueOrDefault("elevated") ?? false) }; panel.Controls.Add(cbElev); y += 30;
    }

    var btnOk = new Button { Text = "OK", Left = 220, Width = 80, Top = y + 6, DialogResult = DialogResult.OK };
    var btnCancel = new Button { Text = "Cancel", Left = 310, Width = 80, Top = y + 6, DialogResult = DialogResult.Cancel };
    panel.Controls.Add(btnOk); panel.Controls.Add(btnCancel);
    form.AcceptButton = btnOk; form.CancelButton = btnCancel;

    if (form.ShowDialog() != DialogResult.OK) return null;

    // Ensure required controls exist for the selected grid to avoid possible null dereference
    if (dgv == _dgvWeb && (tb1 is null || tb2 is null || tb3 is null)) throw new InvalidOperationException("Editor controls for Web are not initialized.");
    if (dgv == _dgvRdp && (tb1 is null || tb2 is null)) throw new InvalidOperationException("Editor controls for RDP are not initialized.");
    if (dgv == _dgvTools && (tb1 is null || tb2 is null || tb3 is null || cbElev is null)) throw new InvalidOperationException("Editor controls for Tools are not initialized.");

    string GetText(TextBox? tb) => tb?.Text?.Trim() ?? string.Empty;

    var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    if (dgv == _dgvWeb)
    {
        dict["name"] = GetText(tb1);
        dict["url"] = GetText(tb2);
        dict["profile"] = GetText(tb3);
    }
    else if (dgv == _dgvRdp)
    {
        dict["name"] = GetText(tb1);
        dict["host"] = GetText(tb2);
    }
    else if (dgv == _dgvTools)
    {
        dict["name"] = GetText(tb1);
        dict["exe"] = GetText(tb2);
        dict["args"] = GetText(tb3);
        dict["elevated"] = cbElev!.Checked;
    }

    // basic validation
    if (dgv == _dgvWeb)
    {
        if (string.IsNullOrWhiteSpace(dict.GetValueOrDefault("url")?.ToString())) { MessageBox.Show("URL is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return null; }
    }
    if (dgv == _dgvRdp)
    {
        if (string.IsNullOrWhiteSpace(dict.GetValueOrDefault("host")?.ToString())) { MessageBox.Show("Host is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return null; }
    }
    if (dgv == _dgvTools)
    {
        if (string.IsNullOrWhiteSpace(dict.GetValueOrDefault("exe")?.ToString())) { MessageBox.Show("Executable is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return null; }
    }

    return dict;
}

        private bool TryEditSelectedItem(Dictionary<string, object> item, string sectionKey, string dedupeKey, string? originalKey)
        {
            if (string.IsNullOrWhiteSpace(originalKey))
            {
                MessageBox.Show("Original key not found; cannot perform edit.", "Edit", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!File.Exists(_path)) { MessageBox.Show("Config file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }

            // load existing
            Dictionary<string, object> root = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var text = File.ReadAllText(_path);
                using var doc = JsonDocument.Parse(text);
                root = JsonElementToObject(doc.RootElement) as Dictionary<string, object> ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read config: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!root.TryGetValue(sectionKey, out var secObj) || secObj == null || !(secObj is List<object> secList))
            {
                MessageBox.Show("Section not found in config.", "Edit", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            bool updated = false;
            for (int i = 0; i < secList.Count; i++)
            {
                if (secList[i] is Dictionary<string, object> dict)
                {
                    if (dict.TryGetValue(dedupeKey, out var val) && val != null && string.Equals(NormalizeKey(dedupeKey, val.ToString()), NormalizeKey(dedupeKey, originalKey ?? string.Empty), StringComparison.Ordinal))
                    {
                        // update entries
                        foreach (var kv in item) dict[kv.Key] = kv.Value;
                        secList[i] = dict;
                        updated = true;
                        break;
                    }
                }
            }

            if (!updated)
            {
                // not found -> append as new
                secList.Add(item);
            }

            root[sectionKey] = secList;

            try
            {
                var backup = _path + ".bak";
                try { if (File.Exists(_path)) File.Copy(_path, backup, true); } catch { }
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(root, options);
                File.WriteAllText(_path, json);
                Logger.LogInfo($"Edited item in {sectionKey} (key={originalKey ?? string.Empty}) in {_path}");
                ShowStatus("Item updated");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save edited item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Normalize key for comparisons: trim, lower; for URLs remove trailing slash
        private static string NormalizeKey(string keyName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var v = value!.Trim();
            if (string.Equals(keyName, "url", StringComparison.OrdinalIgnoreCase))
            {
                // lowercase and remove trailing slash
                v = v.TrimEnd('/');
                return v.ToLowerInvariant();
            }
            // for host/exe/etc
            return v.ToLowerInvariant();
        }

        private void DeleteSelectedFromDisk(DataGridView dgv)
        {
            if (!File.Exists(_path)) throw new InvalidOperationException("Config file not found.");

            // determine section and key
            string sectionKey, keyColName;
            if (dgv == _dgvWeb) { sectionKey = "webPortals"; keyColName = "url"; }
            else if (dgv == _dgvRdp) { sectionKey = "rdpServers"; keyColName = "host"; }
            else { sectionKey = "adminTools"; keyColName = "exe"; }

            // collect keys to delete from selected rows
            var keysToDelete = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (dgv.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow r in dgv.SelectedRows)
                {
                    if (r.IsNewRow) continue;
                    var keyVal = r.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => string.Equals(dgv.Columns[c.ColumnIndex].Name, keyColName, StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(keyVal)) keysToDelete.Add(keyVal);
                }
            }
            else if (dgv.CurrentRow != null && !dgv.CurrentRow.IsNewRow)
            {
                var r = dgv.CurrentRow;
                var keyVal = r.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => string.Equals(dgv.Columns[c.ColumnIndex].Name, keyColName, StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(keyVal)) keysToDelete.Add(keyVal);
            }

            if (keysToDelete.Count == 0) { MessageBox.Show("No valid keys found to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            // load existing config
            Dictionary<string, object> root = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            using (var reader = File.OpenText(_path))
            {
                try
                {
                    var doc = JsonDocument.Parse(reader.ReadToEnd());
                    root = JsonElementToObject(doc.RootElement) as Dictionary<string, object> ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to parse config.json: {ex.Message}");
                }
            }

            if (!root.TryGetValue(sectionKey, out var secObj) || secObj == null || !(secObj is List<object> secList))
            {
                // nothing to delete on disk, just remove from UI
                var rowsToRemove = dgv.SelectedRows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToArray();
                foreach (var r in rowsToRemove) dgv.Rows.Remove(r);
                return;
            }

            // build new list excluding matches
            var newList = new List<object>();
            bool removedAny = false;
            foreach (var it in secList)
            {
                if (it is Dictionary<string, object> dict)
                {
                    if (dict.TryGetValue(keyColName, out var val))
                    {
                        // guard against val.ToString() being null
                        var keyStr = val?.ToString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(keyStr) && keysToDelete.Contains(keyStr))
                        {
                            removedAny = true;
                            continue; // skip (delete)
                        }
                    }
                }
                // ensure we don't add null entries into the new list
                newList.Add(it ?? new Dictionary<string, object>());
            }

            if (!removedAny)
            {
                // nothing removed on disk; inform user
                MessageBox.Show("No matching items found in the on-disk config to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // assign and write back
            root[sectionKey] = newList;
            try
            {
                var backup = _path + ".bak";
                try { if (File.Exists(_path)) File.Copy(_path, backup, true); } catch { }
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(root, options);
                File.WriteAllText(_path, json);
                Logger.LogInfo($"Deleted {keysToDelete.Count} entries from {sectionKey} in {_path}");
                ShowStatus("Deleted selected entries");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to write updated config.json: {ex.Message}");
            }

            // remove rows from UI that matched
            var rows = dgv.SelectedRows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToArray();
            foreach (var r in rows)
            {
                var keyVal = r.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => string.Equals(dgv.Columns[c.ColumnIndex].Name, keyColName, StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(keyVal) && keysToDelete.Contains(keyVal)) dgv.Rows.Remove(r);
            }
        }

        private void LoadGridFromConfig(DataGridView dgv)
        {
            if (!File.Exists(_path)) { MessageBox.Show("Config file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            try
            {
                var text = File.ReadAllText(_path);
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;

                if (dgv == _dgvWeb)
                {
                    _dgvWeb.Rows.Clear();
                    if (root.TryGetProperty("webPortals", out var wp) && wp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in wp.EnumerateArray())
                        {
                            var name = el.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
                            var url = el.TryGetProperty("url", out var u) ? u.GetString() ?? string.Empty : string.Empty;
                            var profile = el.TryGetProperty("profile", out var p) ? p.GetString() ?? string.Empty : string.Empty;
                            var r = _dgvWeb.Rows.Add(name ?? string.Empty, url ?? string.Empty, profile ?? string.Empty);
                            ClearPlaceholderTags(_dgvWeb.Rows[r]);
                        }
                    }
                }
                else if (dgv == _dgvRdp)
                {
                    _dgvRdp.Rows.Clear();
                    if (root.TryGetProperty("rdpServers", out var rs) && rs.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in rs.EnumerateArray())
                        {
                            var name = el.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
                            var host = el.TryGetProperty("host", out var h) ? h.GetString() ?? string.Empty : string.Empty;
                            var r = _dgvRdp.Rows.Add(name ?? string.Empty, host ?? string.Empty);
                            ClearPlaceholderTags(_dgvRdp.Rows[r]);
                        }
                    }
                }
                else if (dgv == _dgvTools)
                {
                    _dgvTools.Rows.Clear();
                    if (root.TryGetProperty("adminTools", out var at) && at.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var el in at.EnumerateArray())
                        {
                            var name = el.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
                            var exe = el.TryGetProperty("exe", out var e) ? e.GetString() ?? string.Empty : string.Empty;
                            var args = el.TryGetProperty("args", out var a) ? a.GetString() ?? string.Empty : string.Empty;
                            var elevated = el.TryGetProperty("elevated", out var ev) && ev.ValueKind == JsonValueKind.True;
                            var r = _dgvTools.Rows.Add(name ?? string.Empty, exe ?? string.Empty, args ?? string.Empty, elevated);
                            ClearPlaceholderTags(_dgvTools.Rows[r]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load config: {ex}");
                MessageBox.Show($"Failed to load config: {ex.Message}\n\nThe configuration file appears to be invalid JSON. Please fix the file (use 'Open config file...' from the tray) before saving or adding entries.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _configValid = false;
                // disable save to avoid overwriting the broken file
                try { if (_btnSave != null) _btnSave.Enabled = false; } catch { }
                return;
            }
        }

        private void ClearAllGrids()
        {
            _dgvWeb.Rows.Clear();
            _dgvRdp.Rows.Clear();
            _dgvTools.Rows.Clear();
        }

        private Dictionary<string, object>? ShowAddItemDialog(DataGridView dgv)
        {
            using var form = new Form { Width = 420, Height = 220, StartPosition = FormStartPosition.CenterParent, Text = "Add Item" };
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            form.Controls.Add(panel);

            int y = 6;
            void AddLabel(string text)
            {
                var lbl = new Label { Text = text, Left = 8, Top = y + 4, Width = 100 };
                panel.Controls.Add(lbl);
            }
            TextBox? tb1 = null; TextBox? tb2 = null; TextBox? tb3 = null; CheckBox? cbElev = null;

            if (dgv == _dgvWeb)
            {
                AddLabel("Name:"); tb1 = new TextBox { Left = 110, Top = y, Width = 280 }; panel.Controls.Add(tb1); y += 30;
                AddLabel("URL:"); tb2 = new TextBox { Left = 110, Top = y, Width = 280 }; panel.Controls.Add(tb2); y += 30;
                AddLabel("Profile:"); tb3 = new TextBox { Left = 110, Top = y, Width = 280, Text = "Profile 1" }; panel.Controls.Add(tb3); y += 30;
            }
            else if (dgv == _dgvRdp)
            {
                AddLabel("Name:"); tb1 = new TextBox { Left = 110, Top = y, Width = 280 }; panel.Controls.Add(tb1); y += 30;
                AddLabel("Host:"); tb2 = new TextBox { Left = 110, Top = y, Width = 280 }; panel.Controls.Add(tb2); y += 30;
            }
            else if (dgv == _dgvTools)
            {
                AddLabel("Name:"); tb1 = new TextBox { Left = 110, Top = y, Width = 280 }; panel.Controls.Add(tb1); y += 30;
                AddLabel("Exe:"); tb2 = new TextBox { Left = 110, Top = y, Width = 280 }; panel.Controls.Add(tb2); y += 30;
                AddLabel("Args:"); tb3 = new TextBox { Left = 110, Top = y, Width = 280 }; panel.Controls.Add(tb3); y += 30;
                cbElev = new CheckBox { Text = "Elevated", Left = 110, Top = y, Width = 100 }; panel.Controls.Add(cbElev); y += 30;
            }

            var btnOk = new Button { Text = "OK", Left = 220, Width = 80, Top = y + 6, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Cancel", Left = 310, Width = 80, Top = y + 6, DialogResult = DialogResult.Cancel };
            panel.Controls.Add(btnOk); panel.Controls.Add(btnCancel);
            form.AcceptButton = btnOk; form.CancelButton = btnCancel;

            if (form.ShowDialog() != DialogResult.OK) return null;

            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (dgv == _dgvWeb)
            {
                dict["name"] = tb1?.Text?.Trim() ?? string.Empty;
                dict["url"] = tb2?.Text?.Trim() ?? string.Empty;
                dict["profile"] = tb3?.Text?.Trim() ?? string.Empty;
            }
            else if (dgv == _dgvRdp)
            {
                dict["name"] = tb1?.Text?.Trim() ?? string.Empty;
                dict["host"] = tb2?.Text?.Trim() ?? string.Empty;
            }
            else if (dgv == _dgvTools)
            {
                dict["name"] = tb1?.Text?.Trim() ?? string.Empty;
                dict["exe"] = tb2?.Text?.Trim() ?? string.Empty;
                dict["args"] = tb3?.Text?.Trim() ?? string.Empty;
                dict["elevated"] = cbElev?.Checked ?? false;
            }

            // basic validation
            if (dgv == _dgvWeb)
            {
                if (string.IsNullOrWhiteSpace(dict.GetValueOrDefault("url")?.ToString())) { MessageBox.Show("URL is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return null; }
            }
            if (dgv == _dgvRdp)
            {
                if (string.IsNullOrWhiteSpace(dict.GetValueOrDefault("host")?.ToString())) { MessageBox.Show("Host is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return null; }
            }
            if (dgv == _dgvTools)
            {
                if (string.IsNullOrWhiteSpace(dict.GetValueOrDefault("exe")?.ToString())) { MessageBox.Show("Executable is required", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return null; }
            }

            return dict;
        }

        private bool TryAppendSingleItem(Dictionary<string, object> item, string sectionKey, string dedupeKey)
        {
            if (!_configValid)
            {
                MessageBox.Show("Configuration file is invalid. Fix the config file before adding items.", "Invalid config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            // Read existing config into a mutable dictionary (or create a new one)
            Dictionary<string, object> existingRootAppend = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(_path))
            {
                try
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(_path));
                    var obj = JsonElementToObject(doc.RootElement) as Dictionary<string, object>;
                    if (obj != null)
                    {
                        foreach (var kv in obj) existingRootAppend[kv.Key] = kv.Value;
                    }
                }
                catch { /* ignore parse errors, start fresh */ }
            }

            // Ensure section list exists
            if (!existingRootAppend.TryGetValue(sectionKey, out var sectionObj) || sectionObj == null || !(sectionObj is List<object> sectionList))
            {
                sectionList = new List<object>();
                existingRootAppend[sectionKey] = sectionList;
            }

            // Check duplicate by dedupeKey
            var incomingKeyVal = item.ContainsKey(dedupeKey) ? item[dedupeKey]?.ToString() : null;
            if (!string.IsNullOrEmpty(incomingKeyVal))
            {
                foreach (var it in sectionList)
                {
                    if (it is Dictionary<string, object> dict && dict.TryGetValue(dedupeKey, out var tv) && tv != null)
                    {
                        if (string.Equals(NormalizeKey(dedupeKey, tv.ToString()), NormalizeKey(dedupeKey, incomingKeyVal), StringComparison.Ordinal))
                        {
                            MessageBox.Show($"Item with the same {dedupeKey} already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }
                    }
                }
            }

            // Ensure putty elevated for adminTools
            if (string.Equals(sectionKey, "adminTools", StringComparison.OrdinalIgnoreCase))
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (!(!item.TryGetValue("exe", out object? exeVal) || exeVal == null || !exeVal.ToString().Contains("putty", StringComparison.OrdinalIgnoreCase)))
                {
                    item["elevated"] = true;
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

            // Append new item
            sectionList.Add(item ?? new Dictionary<string, object>());

            // Update editor value (prefer UI if present)
            existingRootAppend["editor"] = !string.IsNullOrWhiteSpace(_txtEditorPath.Text)
                ? (object)_txtEditorPath.Text
                : (existingRootAppend.TryGetValue("editor", out var exEd) && exEd != null ? exEd : (object)"notepad.exe");

            // Write back the whole existingRootAppend so unrelated sections are preserved
            try
            {
                var backup = _path + ".bak";
                try { if (File.Exists(_path)) File.Copy(_path, backup, true); } catch { }
                var options = new JsonSerializerOptions { WriteIndented = true };
                // Convert any Dictionary<string, object> entries in sectionList to plain object maps before serializing
                existingRootAppend[sectionKey] = sectionList;
                var json = JsonSerializer.Serialize(existingRootAppend, options);
                File.WriteAllText(_path, json);
                // Log and show a non-modal status instead of a blocking debug MessageBox
                Logger.LogInfo($"Appended item to config at {_path}");
                ShowStatus("Item added to configuration");
                return true;
            }
            catch (Exception saveEx)
            {
                MessageBox.Show($"Failed to save config: {saveEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void InitializeComponent()
        {
            Text = "Edit Configuration";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            // Layout: top editor panel, middle tabs, bottom buttons
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // editor
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // tabs
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // buttons

            // Editor panel
            var editorPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
            var lblEditor = new Label { Text = "Editor:", AutoSize = true, Location = new Point(6, 10) };
            _txtEditorPath = new TextBox { Location = new Point(70, 6), Width = 600, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            editorPanel.Controls.Add(lblEditor);
            editorPanel.Controls.Add(_txtEditorPath);

            // Tabs
            _tabs = new TabControl { Dock = DockStyle.Fill };
            var tabWeb = new TabPage("Web Portals");
            var tabRdp = new TabPage("RDP Servers");
            var tabTools = new TabPage("Admin Tools");

            // Web grid
            _dgvWeb = CreateGrid(new[] { ("Name", typeof(string)), ("Url", typeof(string)), ("Profile", typeof(string)) });
            AddGridToTab(tabWeb, _dgvWeb);
            // tag grids so Add button can insert context-aware placeholders
            _dgvWeb.Tag = "web";

            // RDP grid
            _dgvRdp = CreateGrid(new[] { ("Name", typeof(string)), ("Host", typeof(string)) });
            AddGridToTab(tabRdp, _dgvRdp);
            _dgvRdp.Tag = "rdp";

            // Tools grid
            _dgvTools = CreateGrid(new[] { ("Name", typeof(string)), ("Exe", typeof(string)), ("Args", typeof(string)), ("Elevated", typeof(bool)) });
            AddGridToTab(tabTools, _dgvTools);
            _dgvTools.Tag = "tools";

            _tabs.TabPages.Add(tabWeb);
            _tabs.TabPages.Add(tabRdp);
            _tabs.TabPages.Add(tabTools);

            // Bottom buttons
            var bottomPanel = new Panel { Dock = DockStyle.Fill };
            _btnSave = new Button { Text = "Save", Width = 100, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
            _btnCancel = new Button { Text = "Cancel", Width = 100, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
            _btnSave.Click += BtnSave_Click;
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            var _btnDeleteConfig = new Button { Text = "Delete Config File", Width = 140, Anchor = AnchorStyles.Left | AnchorStyles.Bottom };
            _btnDeleteConfig.Click += (s, e) =>
            {
                if (!File.Exists(_path)) { MessageBox.Show("Config file does not exist.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                var r = MessageBox.Show("Delete the config file? This will remove all saved entries.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes) return;
                try { File.Delete(_path); MessageBox.Show("Config file deleted.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information); ClearAllGrids(); _txtEditorPath.Text = "notepad.exe"; }
                catch (Exception ex) { MessageBox.Show($"Failed to delete config: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };
            // status label on left
            _statusLabel = new Label { Text = string.Empty, AutoSize = false, Width = 300, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Left, Padding = new Padding(6, 10, 0, 0) };
            bottomPanel.Controls.Add(_statusLabel);
            bottomPanel.Controls.Add(_btnSave);
            bottomPanel.Controls.Add(_btnCancel);
            _btnCancel.Location = new Point(bottomPanel.Width - _btnCancel.Width - 10, 10);
            _btnSave.Location = new Point(bottomPanel.Width - _btnCancel.Width - _btnSave.Width - 20, 10);
            bottomPanel.Resize += (s, e) =>
            {
                _btnCancel.Location = new Point(bottomPanel.Width - _btnCancel.Width - 10, 10);
                _btnSave.Location = new Point(bottomPanel.Width - _btnCancel.Width - _btnSave.Width - 20, 10);
            };

            // status timer
            _statusTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            _statusTimer.Tick += (s, e) => { _statusTimer.Stop(); if (_statusLabel != null) _statusLabel.Text = string.Empty; };

            layout.Controls.Add(editorPanel, 0, 0);
            layout.Controls.Add(_tabs, 0, 1);
            layout.Controls.Add(bottomPanel, 0, 2);

            Controls.Add(layout);
        }

        private DataGridView CreateGrid((string name, Type type)[] columns)
        {
            var dgv = new DataGridView();
            // Disable inline editing: users must use Add/Edit buttons. This prevents accidental duplicates
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false; // deletion handled via Delete button which also updates disk
            dgv.ReadOnly = true; // make grid non-editable inline
            // visually indicate non-editable cells by graying them out
            dgv.DefaultCellStyle.BackColor = SystemColors.ControlLight;
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.ControlLight;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToOrderColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ColumnHeadersVisible = true;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 28; // ensure visible header height
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            foreach (var col in columns)
            {
                DataGridViewColumn c;
                if (col.type == typeof(bool)) c = new DataGridViewCheckBoxColumn();
                else c = new DataGridViewTextBoxColumn();
                c.Name = col.name;
                c.HeaderText = col.name;
                c.DataPropertyName = col.name;
                // set helpful tooltip for each column header
                switch (col.name.ToLowerInvariant())
                {
                    case "name": c.ToolTipText = "Display name shown in the menu"; break;
                    case "url": c.ToolTipText = "Full URL, e.g. https://admin.google.com"; break;
                    case "profile": c.ToolTipText = "Browser profile directory, e.g. Default or Profile 1"; break;
                    case "host": c.ToolTipText = "RDP host name or IP"; break;
                    case "exe": c.ToolTipText = "Executable name or full path, e.g. putty.exe"; break;
                    case "args": c.ToolTipText = "Command-line arguments (optional)"; break;
                    case "elevated": c.ToolTipText = "Run elevated (requires UAC)"; break;
                }
                dgv.Columns.Add(c);
            }

            return dgv;
        }

        private void AddGridToTab(TabPage tab, DataGridView dgv)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4) };
            btnPanel.BackColor = SystemColors.Control; // match form background so it doesn't block headers visually
            var btnAdd = new Button { Text = "Add", Width = 80 };
            var btnDelete = new Button { Text = "Delete", Width = 80 };
            var btnLoadConfig = new Button { Text = "Load Config", Width = 100 };
            btnLoadConfig.Click += (s, e) =>
            {
                // reload this section from disk (replace current rows). Ask for confirmation because this will discard unsaved edits.
                var res = MessageBox.Show("Reload this section from disk? Unsaved changes will be lost.", "Reload", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res != DialogResult.Yes) return;
                LoadGridFromConfig(dgv);
            };
            btnAdd.Click += (s, e) =>
            {
                if (!_configValid)
                {
                    MessageBox.Show("Configuration file is invalid. Fix the config file before adding items.", "Invalid config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // show a small add dialog for the specific grid and save immediately
                var newItem = ShowAddItemDialog(dgv);
                if (newItem == null) return;

                // determine section and dedupe key
                if (dgv == _dgvWeb)
                {
                    if (TryAppendSingleItem(newItem, "webPortals", "url"))
                    {
                        var name = newItem.GetValueOrDefault("name")?.ToString() ?? string.Empty;
                        var url = newItem.GetValueOrDefault("url")?.ToString() ?? string.Empty;
                        var profile = newItem.GetValueOrDefault("profile")?.ToString() ?? string.Empty;
                        var r = _dgvWeb.Rows.Add(name, url, profile);
                        ClearPlaceholderTags(_dgvWeb.Rows[r]);
                    }
                }
                else if (dgv == _dgvRdp)
                {
                    if (TryAppendSingleItem(newItem, "rdpServers", "host"))
                    {
                        var r = _dgvRdp.Rows.Add(newItem.GetValueOrDefault("name")?.ToString() ?? string.Empty, newItem.GetValueOrDefault("host")?.ToString() ?? string.Empty);
                        ClearPlaceholderTags(_dgvRdp.Rows[r]);
                    }
                }
                else if (dgv == _dgvTools)
                {
                    if (TryAppendSingleItem(newItem, "adminTools", "exe"))
                    {
                        var r = _dgvTools.Rows.Add(newItem.GetValueOrDefault("name")?.ToString() ?? string.Empty, newItem.GetValueOrDefault("exe")?.ToString() ?? string.Empty, newItem.GetValueOrDefault("args")?.ToString() ?? string.Empty, Convert.ToBoolean(newItem.GetValueOrDefault("elevated") ?? false));
                        ClearPlaceholderTags(_dgvTools.Rows[r]);
                    }
                }
            };
            var btnEdit = new Button { Text = "Edit", Width = 80 };
            btnEdit.Click += (s, e) =>
            {
                // open edit dialog for selected row
                DataGridViewRow? row = null;
                if (dgv.SelectedRows.Count > 0) row = dgv.SelectedRows[0];
                else if (dgv.CurrentRow != null && !dgv.CurrentRow.IsNewRow) row = dgv.CurrentRow;
                if (row == null)
                {
                    MessageBox.Show("Select a row to edit.", "Edit", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var current = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    var key = dgv.Columns[i].Name;
                    current[key] = row.Cells[i].Value?.ToString() ?? string.Empty;
                }

                var edited = ShowEditItemDialog(dgv, current);
                if (edited == null) return;

                // determine section and dedupe key
                if (dgv == _dgvWeb)
                {
                    var originalKey = current.GetValueOrDefault("url")?.ToString() ?? string.Empty;
                    if (TryEditSelectedItem(edited, "webPortals", "url", originalKey))
                    {
                        // update UI
                        row.SetValues(edited.GetValueOrDefault("name")?.ToString() ?? string.Empty, edited.GetValueOrDefault("url")?.ToString() ?? string.Empty, edited.GetValueOrDefault("profile")?.ToString() ?? string.Empty);
                        ClearPlaceholderTags(row);
                    }
                }
                if (dgv == _dgvRdp)
                {
                    var originalKey = current.GetValueOrDefault("host")?.ToString() ?? string.Empty;
                    if (TryEditSelectedItem(edited, "rdpServers", "host", originalKey))
                    {
                        row.SetValues(edited.GetValueOrDefault("name")?.ToString() ?? string.Empty, edited.GetValueOrDefault("host")?.ToString() ?? string.Empty);
                        ClearPlaceholderTags(row);
                    }
                }
                if (dgv == _dgvTools)
                {
                    var originalKey = current.GetValueOrDefault("exe")?.ToString() ?? string.Empty;
                    if (TryEditSelectedItem(edited, "adminTools", "exe", originalKey))
                    {
                        row.SetValues(edited.GetValueOrDefault("name")?.ToString() ?? string.Empty, edited.GetValueOrDefault("exe")?.ToString() ?? string.Empty, edited.GetValueOrDefault("args")?.ToString() ?? string.Empty, Convert.ToBoolean(edited.GetValueOrDefault("elevated") ?? false));
                        ClearPlaceholderTags(row);
                    }
                }
            };
            btnDelete.Click += (s, e) =>
            {
                // delete selected rows and remove them from the on-disk config as well
                if ((dgv.SelectedRows.Count == 0 && (dgv.CurrentRow == null || dgv.CurrentRow.IsNewRow)))
                {
                    MessageBox.Show("No row selected to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var confirm = MessageBox.Show("Delete selected row(s) from configuration? This will remove them from disk.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes) return;

                try
                {
                    DeleteSelectedFromDisk(dgv);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete selected entries: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            btnPanel.Controls.Add(btnAdd);
            btnPanel.Controls.Add(btnEdit);
            btnPanel.Controls.Add(btnDelete);
            btnPanel.Controls.Add(btnLoadConfig);

            dgv.Dock = DockStyle.Fill;

            // attach placeholder handlers and custom painting for empty cells
            dgv.CellBeginEdit += Dgv_CellBeginEdit;
            dgv.CellEndEdit += Dgv_CellEndEdit;
            dgv.CellPainting += Dgv_CellPainting;

            // instruction label below buttons to show placeholders/help
            var lblInstruction = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                ForeColor = Color.DarkSlateGray,
                Padding = new Padding(6, 6, 6, 6),
                Text = GetInstructionTextForGrid(dgv)
            };
            lblInstruction.BackColor = SystemColors.Control;

            // add controls in reverse order so docking (Top/Fill) stacks correctly:
            // add dgv (Fill) first, then instruction (Top), then buttons (Top)
            panel.Controls.Add(dgv);
            panel.Controls.Add(lblInstruction);
            panel.Controls.Add(btnPanel);

            tab.Controls.Add(panel);
        }

        private void LoadJson()
        {
            try
            {
                string text;
                if (!string.IsNullOrEmpty(_initialJson)) text = _initialJson!; // prefer supplied initial JSON
                else
                {
                    if (!File.Exists(_path))
                    {
                        _txtEditorPath.Text = "notepad.exe";
                        // Do not auto-insert defaults. Leave grids empty so admins explicitly add entries.
                        return;
                    }
                    text = File.ReadAllText(_path);
                }

                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;

                if (root.TryGetProperty("editor", out var ed)) _txtEditorPath.Text = ed.GetString() ?? "notepad.exe";

                _dgvWeb.Rows.Clear(); _dgvRdp.Rows.Clear(); _dgvTools.Rows.Clear();

                if (root.TryGetProperty("webPortals", out var wp) && wp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in wp.EnumerateArray())
                    {
                        var name = el.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
                        var url = el.TryGetProperty("url", out var u) ? u.GetString() ?? string.Empty : string.Empty;
                        var profile = el.TryGetProperty("profile", out var p) ? p.GetString() ?? string.Empty : string.Empty;
                        var r = _dgvWeb.Rows.Add(name ?? string.Empty, url ?? string.Empty, profile ?? string.Empty);
                        ClearPlaceholderTags(_dgvWeb.Rows[r]);
                    }
                }

                if (root.TryGetProperty("rdpServers", out var rs) && rs.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in rs.EnumerateArray())
                    {
                        var name = el.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
                        var host = el.TryGetProperty("host", out var h) ? h.GetString() ?? string.Empty : string.Empty;
                        _dgvRdp.Rows.Add(name ?? string.Empty, host ?? string.Empty);
                    }
                }

                if (root.TryGetProperty("adminTools", out var at) && at.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in at.EnumerateArray())
                    {
                        var name = el.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;
                        var exe = el.TryGetProperty("exe", out var e) ? e.GetString() ?? string.Empty : string.Empty;
                        var args = el.TryGetProperty("args", out var a) ? a.GetString() ?? string.Empty : string.Empty;
                        var elevated = el.TryGetProperty("elevated", out var ev) && ev.ValueKind == JsonValueKind.True;
                        _dgvTools.Rows.Add(name ?? string.Empty, exe ?? string.Empty, args ?? string.Empty, elevated);
                    }
                }
                // Do not auto-insert defaults; leave grids empty so admins explicitly add entries.
                // Use InsertSampleDefaults() if sample rows are desired.
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load config: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyPlaceholderStyle(DataGridView dgv, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgv.Rows.Count) return;
            var row = dgv.Rows[rowIndex];
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.ForeColor = Color.Gray;
                cell.Tag = "placeholder";
            }
        }

        private void Dgv_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (sender is not DataGridView dgv) return;
            var cell = dgv[e.ColumnIndex, e.RowIndex];
            if (cell.Tag is string t && t == "placeholder")
            {
                cell.Value = string.Empty;
                cell.Style.ForeColor = Color.Black;
                cell.Tag = null;
            }
        }

        private void Dgv_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (sender is not DataGridView dgv) return;
            var cell = dgv[e.ColumnIndex, e.RowIndex];
            if (cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()))
            {
                // restore placeholder
                var ph = GetPlaceholderForColumn(dgv, e.ColumnIndex) ?? string.Empty;
                cell.Value = ph;
                cell.Style.ForeColor = Color.Gray;
                cell.Tag = "placeholder";
            }
            else
            {
                cell.Style.ForeColor = Color.Black;
                cell.Tag = null;
            }
        }

        private bool IsPlaceholderForColumn(DataGridView dgv, int colIndex, string value)
        {
            var ph = GetPlaceholderForColumn(dgv, colIndex);
            return string.Equals(ph, value, StringComparison.Ordinal);
        }

        private string GetPlaceholderForColumn(DataGridView dgv, int colIndex)
        {
            var colName = dgv.Columns[colIndex].Name.ToLowerInvariant();
            if (dgv.Tag is string tag)
            {
                if (tag == "web")
                {
                    return colName switch
                    {
                        "name" => "Display name",
                        "url" => "https://example.com",
                        "profile" => "Profile 1",
                        _ => string.Empty
                    };
                }
                if (tag == "rdp")
                {
                    return colName switch
                    {
                        "name" => "Display name",
                        "host" => "10.1.1.1",
                        _ => string.Empty
                    };
                }
                if (tag == "tools")
                {
                    return colName switch
                    {
                        "name" => "Display name",
                        "exe" => "putty.exe",
                        "args" => "",
                        "elevated" => "false",
                        _ => string.Empty
                    };
                }
            }
            // default
            return colName switch
            {
                "name" => "Display name",
                "url" => "https://example.com",
                _ => string.Empty
            };
        }

        private string GetInstructionTextForGrid(DataGridView dgv)
        {
            if (dgv == null) return string.Empty;
            if (dgv.Tag is string tag)
            {
                if (tag == "web") return "Enter web portals: Name | URL (https://...) | Profile";
                if (tag == "rdp") return "Enter RDP servers: Name | Host";
                if (tag == "tools") return "Enter admin tools: Name | Exe | Args | Elevated (true/false)";
            }
            return "Enter items in the table. Use Add to insert a new row.";
        }

        private void Dgv_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (sender is not DataGridView dgv) return;

            var col = dgv.Columns[e.ColumnIndex];
            // don't draw placeholders for checkbox columns
            if (col is DataGridViewCheckBoxColumn) return;

            var cell = dgv[e.ColumnIndex, e.RowIndex];
            var value = cell.Value?.ToString();

            // if cell has a real value, let default painting occur
            if (!string.IsNullOrWhiteSpace(value)) return;

            // if cell is the current editing cell, skip placeholder
            var cur = dgv.CurrentCell;
            if (dgv.IsCurrentCellInEditMode && cur != null && cur.RowIndex == e.RowIndex && cur.ColumnIndex == e.ColumnIndex)
                return;

            // draw default background and borders
            e.PaintBackground(e.CellBounds, true);
            e.Paint(e.CellBounds, DataGridViewPaintParts.Border);

            // draw placeholder text
            var ph = GetPlaceholderForColumn(dgv, e.ColumnIndex) ?? string.Empty;
            if (string.IsNullOrEmpty(ph)) return;

            var rect = e.CellBounds;
            rect.Inflate(-4, 0);
            using var brush = new SolidBrush(Color.Gray);
            var sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };
            var font = e.CellStyle?.Font ?? SystemFonts.DefaultFont;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            e.Graphics.DrawString(ph, font, brush, rect, sf);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            e.Handled = true;
        }
        // Helper to insert example rows; retained for manual use only.
        private void AddDefaults()
        {
            // Insert example rows into each grid. Not called automatically.
            _dgvWeb.Rows.Add("Admin Console", "https://admin.google.com", "Profile 1");
            _dgvWeb.Rows.Add("Dashboard", "https://dashboard.example.com", "Profile 1");
            _dgvRdp.Rows.Add("C39-FS", "C39-FS");
            _dgvRdp.Rows.Add("C39-AP", "C39-AP");
            _dgvTools.Rows.Add("PuTTY", "putty.exe", "", false);
            _dgvTools.Rows.Add("PowerShell (Admin)", "powershell.exe", "-NoExit", true);
        }

        private void InsertDefaultsAppend()
        {
            // append defaults to each grid without removing existing rows
            // Only append PuTTY default (elevated)
            _dgvTools.Rows.Add("PuTTY", "putty.exe", "", true);
        }

        private void AppendDefaultsForGrid(DataGridView dgv)
        {
            if (dgv == null) return;
            if (dgv == _dgvWeb)
            {
                var r1 = _dgvWeb.Rows.Add("Admin Console", "https://admin.google.com", "Profile 1");
                var r2 = _dgvWeb.Rows.Add("Dashboard", "https://dashboard.example.com", "Profile 1");
                // ensure these are treated as real values (not placeholders)
                ClearPlaceholderTags(_dgvWeb.Rows[r1]);
                ClearPlaceholderTags(_dgvWeb.Rows[r2]);
            }
            else if (dgv == _dgvRdp)
            {
                var r1 = _dgvRdp.Rows.Add("C39-FS", "C39-FS");
                var r2 = _dgvRdp.Rows.Add("C39-AP", "C39-AP");
                ClearPlaceholderTags(_dgvRdp.Rows[r1]);
                ClearPlaceholderTags(_dgvRdp.Rows[r2]);
            }
            else if (dgv == _dgvTools)
            {
                var r1 = _dgvTools.Rows.Add("PuTTY", "putty.exe", "", false);
                var r2 = _dgvTools.Rows.Add("PowerShell (Admin)", "powershell.exe", "-NoExit", true);
                ClearPlaceholderTags(_dgvTools.Rows[r1]);
                ClearPlaceholderTags(_dgvTools.Rows[r2]);
            }
        }

        private void ClearPlaceholderTags(DataGridViewRow row)
        {
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Tag = null;
                cell.Style.ForeColor = Color.Black;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                // First validate all rows: require Name and key fields (URL/Host/Exe) not empty or placeholder
                var errors = new List<string>();

                // helper to check cell value and placeholder
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                static string CellString(DataGridViewRow r, int idx) => (r.Cells[idx].Value ?? string.Empty).ToString().Trim();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                // Validate Web
                int ri = 0;
                foreach (DataGridViewRow r in _dgvWeb.Rows)
                {
                    if (r.IsNewRow) { ri++; continue; }
                    var nameCell = r.Cells[0];
                    var urlCell = r.Cells[1];
                    var name = CellString(r, 0);
                    var url = CellString(r, 1);
                    if (string.IsNullOrWhiteSpace(name) || (nameCell.Tag is string t1 && t1 == "placeholder")) errors.Add($"Web Portals row {ri + 1}: name is required");
                    if (string.IsNullOrWhiteSpace(url) || (urlCell.Tag is string t2 && t2 == "placeholder") || !(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                        errors.Add($"Web Portals row {ri + 1}: invalid URL (must start with http:// or https://)");
                    ri++;
                }

                // Validate RDP
                ri = 0;
                foreach (DataGridViewRow r in _dgvRdp.Rows)
                {
                    if (r.IsNewRow) { ri++; continue; }
                    var nameCellR = r.Cells[0];
                    var hostCell = r.Cells[1];
                    var name = CellString(r, 0);
                    var host = CellString(r, 1);
                    if (string.IsNullOrWhiteSpace(name) || (nameCellR.Tag is string t1 && t1 == "placeholder")) errors.Add($"RDP Servers row {ri + 1}: name is required");
                    if (string.IsNullOrWhiteSpace(host) || (hostCell.Tag is string t2 && t2 == "placeholder")) errors.Add($"RDP Servers row {ri + 1}: host is required");
                    ri++;
                }

                // Validate Tools
                ri = 0;
                foreach (DataGridViewRow r in _dgvTools.Rows)
                {
                    if (r.IsNewRow) { ri++; continue; }
                    var nameCellT = r.Cells[0];
                    var exeCell = r.Cells[1];
                    var name = CellString(r, 0);
                    var exe = CellString(r, 1);
                    if (string.IsNullOrWhiteSpace(name) || (nameCellT.Tag is string t1 && t1 == "placeholder")) errors.Add($"Admin Tools row {ri + 1}: name is required");
                    if (string.IsNullOrWhiteSpace(exe) || (exeCell.Tag is string t2 && t2 == "placeholder")) errors.Add($"Admin Tools row {ri + 1}: executable is required");
                    ri++;
                }

                if (errors.Count > 0)
                {
                    var msg = "Please fix the following issues before saving:\n" + string.Join("\n", errors);
                    MessageBox.Show(msg, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Build lists after validation
                var webList = new List<Dictionary<string, object>>();
                foreach (DataGridViewRow r in _dgvWeb.Rows)
                {
                    if (r.IsNewRow) continue;
                    var name = CellString(r, 0);
                    var url = CellString(r, 1);
                    var profile = CellString(r, 2);
                    webList.Add(new Dictionary<string, object> { ["name"] = name, ["url"] = url, ["profile"] = profile });
                }

                var rdpList = new List<Dictionary<string, object>>();
                foreach (DataGridViewRow r in _dgvRdp.Rows)
                {
                    if (r.IsNewRow) continue;
                    var name = CellString(r, 0);
                    var host = CellString(r, 1);
                    rdpList.Add(new Dictionary<string, object> { ["name"] = name, ["host"] = host });
                }

                var toolsList = new List<Dictionary<string, object>>();
                foreach (DataGridViewRow r in _dgvTools.Rows)
                {
                    if (r.IsNewRow) continue;
                    var name = CellString(r, 0);
                    var exe = CellString(r, 1);
                    var args = CellString(r, 2);
                    var elevated = false;
                    try { elevated = Convert.ToBoolean(r.Cells[3].Value); } catch { }
                    toolsList.Add(new Dictionary<string, object> { ["name"] = name, ["exe"] = exe, ["args"] = args, ["elevated"] = elevated });
                }

                var editor = string.IsNullOrWhiteSpace(_txtEditorPath.Text) ? "notepad.exe" : _txtEditorPath.Text;

                // Merge with existing on-disk config so empty tabs do not wipe existing sections
                Dictionary<string, object>? existingRootFromDisk = null;
                if (File.Exists(_path))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(File.ReadAllText(_path));
                        existingRootFromDisk = JsonElementToObject(doc.RootElement) as Dictionary<string, object>;
                    }
                    catch { /* ignore parse errors and treat as no existing config */ }
                }

                // Merge strategy: keep existing entries and append new ones, avoiding duplicates.
                List<Dictionary<string, object>> existingWeb = new();
                List<Dictionary<string, object>> existingRdp = new();
                List<Dictionary<string, object>> existingTools = new();
                if (existingRootFromDisk != null)
                {
                    if (existingRootFromDisk.TryGetValue("webPortals", out var exW) && exW is List<object> lwe)
                    {
                        foreach (var it in lwe)
                            if (it is Dictionary<string, object> dict) existingWeb.Add(dict);
                    }
                    if (existingRootFromDisk.TryGetValue("rdpServers", out var exR) && exR is List<object> lre)
                    {
                        foreach (var it in lre)
                            if (it is Dictionary<string, object> dict) existingRdp.Add(dict);
                    }
                    if (existingRootFromDisk.TryGetValue("adminTools", out var exT) && exT is List<object> lte)
                    {
                        foreach (var it in lte)
                            if (it is Dictionary<string, object> dict) existingTools.Add(dict);
                    }
                }

                // MergeByKey was previously declared here but not used; merge logic now handled by UpsertByKey

                // Upsert: update existing entries when key matches, otherwise append
                static void UpsertByKey(List<Dictionary<string, object>> target, List<Dictionary<string, object>> incoming, string key)
                {
                    foreach (var inc in incoming)
                    {
                        if (!inc.TryGetValue(key, out var val) || val == null)
                        {
                            target.Add(inc);
                            continue;
                        }
                        var sval = val.ToString();
                        bool updated = false;
                        for (int i = 0; i < target.Count; i++)
                        {
                            var t = target[i];
                            if (t.TryGetValue(key, out var tv) && tv != null && string.Equals(NormalizeKey(key, tv.ToString()), NormalizeKey(key, sval), StringComparison.Ordinal))
                            {
                                // replace fields from inc into existing entry
                                foreach (var kv in inc)
                                {
                                    t[kv.Key] = kv.Value;
                                }
                                target[i] = t;
                                updated = true;
                                break;
                            }
                        }
                        if (!updated) target.Add(inc);
                    }
                }

                // Read existing on-disk config into mutable dictionary so we update only sections the user edited
                Dictionary<string, object> existingRoot = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(_path))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(File.ReadAllText(_path));
                        var obj = JsonElementToObject(doc.RootElement) as Dictionary<string, object>;
                        if (obj != null)
                        {
                            foreach (var kv in obj) existingRoot[kv.Key] = kv.Value;
                        }
                    }
                    catch { /* ignore parse errors, start fresh */ }
                }

                // Merge behavior: when saving, preserve intent to delete items if user edited the grid.
                // For each section:
                // - If the user edited the grid and wants to delete/modify entries, we replace the section with the grid contents.
                // - If the grid contains only newly added rows (append-only), append unique rows to the existing section.
                // We detect intent by comparing the grid rows to the existing on-disk entries: if there is any difference (missing entries), treat as replace.

                // Helper to convert existing list<object> to List<Dictionary<string,object>>
                static List<Dictionary<string, object>> ToDictList(object section)
                {
                    var outL = new List<Dictionary<string, object>>();
                    if (section is List<object> lo)
                    {
                        foreach (var it in lo)
                        {
                            if (it is Dictionary<string, object> dict) outL.Add(dict);
                        }
                    }
                    return outL;
                }

                var diskWeb = existingRootFromDisk != null && existingRootFromDisk.TryGetValue("webPortals", out var dw) ? ToDictList(dw) : new List<Dictionary<string, object>>();
                var diskRdp = existingRootFromDisk != null && existingRootFromDisk.TryGetValue("rdpServers", out var dr) ? ToDictList(dr) : new List<Dictionary<string, object>>();
                var diskTools = existingRootFromDisk != null && existingRootFromDisk.TryGetValue("adminTools", out var dt) ? ToDictList(dt) : new List<Dictionary<string, object>>();

                // Decide per-section whether to replace or append. If grid row count is zero, preserve disk section.
                if (webList.Count == 0)
                {
                    // keep existing
                    if (diskWeb.Count > 0) existingRoot["webPortals"] = diskWeb;
                }
                else
                {
                    // Always append: keep existing disk entries and add unique new entries from the grid
                    var merged = new List<Dictionary<string, object>>(diskWeb);
                    // Upsert: update existing entries when key matches, otherwise append
                    UpsertByKey(merged, webList, "url");
                    existingRoot["webPortals"] = merged;
                }

                if (rdpList.Count == 0)
                {
                    if (diskRdp.Count > 0) existingRoot["rdpServers"] = diskRdp;
                }
                else
                {
                    // Always append: keep existing disk entries and add unique new entries from the grid
                    var mergedR = new List<Dictionary<string, object>>(diskRdp);
                    UpsertByKey(mergedR, rdpList, "host");
                    existingRoot["rdpServers"] = mergedR;
                }

                if (toolsList.Count == 0)
                {
                    if (diskTools.Count > 0) existingRoot["adminTools"] = diskTools;
                }
                else
                {
                    // Always append: keep existing disk entries and add unique new entries from the grid
                    var mergedT = new List<Dictionary<string, object>>(diskTools);
                    UpsertByKey(mergedT, toolsList, "exe");
                    existingRoot["adminTools"] = mergedT;
                }

                // Editor: prefer UI value if provided, otherwise keep existing
                if (!string.IsNullOrWhiteSpace(_txtEditorPath.Text)) existingRoot["editor"] = _txtEditorPath.Text;

                var confirm = MessageBox.Show("Save changes to config.json?", "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                var backup = _path + ".bak";
                try { if (File.Exists(_path)) File.Copy(_path, backup, true); } catch { }

                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(existingRoot, options);
                    File.WriteAllText(_path, json);
                    Logger.LogInfo($"Config saved to {_path}");
                    ShowStatus("Configuration saved");
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception saveEx)
                {
                    Logger.LogError($"Failed to save config: {saveEx}");
                    MessageBox.Show($"Failed to save config: {saveEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save config: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper to convert JsonElement -> CLR object (Dictionary/List/string/number/bool/null)
        private static object? JsonElementToObject(JsonElement elem)
        {
            switch (elem.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (var p in elem.EnumerateObject())
#pragma warning disable CS8601 // Possible null reference assignment.
                        dict[p.Name] = JsonElementToObject(p.Value);
#pragma warning restore CS8601 // Possible null reference assignment.
                    return dict;
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var i in elem.EnumerateArray())
#pragma warning disable CS8601 // Possible null reference assignmen
#pragma warning disable CS8604 // Possible null reference argument.
                        list.Add(item: JsonElementToObject(i));
#pragma warning restore CS8604 // Possible null reference argument.
                    return list;
                case JsonValueKind.String:
                    return elem.GetString();
                case JsonValueKind.Number:
                    if (elem.TryGetInt64(out var l)) return l;
                    if (elem.TryGetDouble(out var d)) return d;
                    return elem.GetDecimal();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                default:
                    return null;
            }
        }
    }
}
