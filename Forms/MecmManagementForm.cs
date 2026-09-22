using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AdminTrayTool.Services;

namespace AdminTrayTool
{
    public class MecmManagementForm : Form
    {
        private readonly MecmManagementService _mecmManagementService;

        private TextBox _txtComputerName = null!;
        private Button _btnLookup = null!;

        private Label _lblComputerName = null!;
        private Label _lblResourceId = null!;
        private Label _lblClient = null!;
        private Label _lblClientVersion = null!;
        private Label _lblOperatingSystem = null!;

        private TextBox _txtCollectionFilter = null!;
        private ComboBox _cmbCollection = null!;
        private Button _btnAddToCollection = null!;
        private Button _btnRemoveFromCollection = null!;

        private TextBox _txtLog = null!;

        private List<MecmCollection> _collections = new();

        private string _currentComputerName = string.Empty;

        public MecmManagementForm()
        {
            _mecmManagementService =
                new MecmManagementService();

            InitializeForm();
            BuildUi();
        }

        private void InitializeForm()
        {
            Text = "MECM Management";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(900, 700);

            BackColor = Color.FromArgb(10, 15, 25);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
        }

        private void BuildUi()
        {
            var header = new Label
            {
                Text = "MECM / SCCM",
                Location = new Point(25, 20),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    18F,
                    FontStyle.Bold),
                ForeColor = Color.White
            };

            Controls.Add(header);

            var subtitle = new Label
            {
                Text =
                    "Find Windows computers and manage MECM collection membership",
                Location = new Point(27, 55),
                AutoSize = true,
                ForeColor = Color.LightGray
            };

            Controls.Add(subtitle);

            // Computer lookup
            var lookupPanel = new Panel
            {
                Location = new Point(25, 95),
                Size = new Size(850, 75),
                BackColor = Color.FromArgb(20, 27, 40)
            };

            Controls.Add(lookupPanel);

            var computerLabel = new Label
            {
                Text = "Computer Name",
                Location = new Point(15, 12),
                AutoSize = true
            };

            lookupPanel.Controls.Add(computerLabel);

            _txtComputerName = new TextBox
            {
                Location = new Point(15, 35),
                Size = new Size(300, 27),
                BackColor = Color.FromArgb(30, 38, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lookupPanel.Controls.Add(_txtComputerName);

            _btnLookup = new Button
            {
                Text = "LOOK UP",
                Location = new Point(330, 34),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(40, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            _btnLookup.FlatAppearance.BorderColor =
                Color.FromArgb(80, 100, 130);

            _btnLookup.Click += BtnLookup_Click;

            lookupPanel.Controls.Add(_btnLookup);

            // Computer information
            var infoPanel = new Panel
            {
                Location = new Point(25, 185),
                Size = new Size(850, 200),
                BackColor = Color.FromArgb(20, 27, 40)
            };

            Controls.Add(infoPanel);

            var infoTitle = new Label
            {
                Text = "MECM COMPUTER INFORMATION",
                Location = new Point(15, 12),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold)
            };

            infoPanel.Controls.Add(infoTitle);

            _lblComputerName =
                CreateInfoLabel(
                    "Computer:",
                    new Point(15, 45));

            _lblResourceId =
                CreateInfoLabel(
                    "Resource ID:",
                    new Point(15, 75));

            _lblClient =
                CreateInfoLabel(
                    "MECM Client:",
                    new Point(15, 105));

            _lblClientVersion =
                CreateInfoLabel(
                    "Client Version:",
                    new Point(15, 135));

            _lblOperatingSystem =
                CreateInfoLabel(
                    "Operating System:",
                    new Point(15, 165));

            infoPanel.Controls.Add(_lblComputerName);
            infoPanel.Controls.Add(_lblResourceId);
            infoPanel.Controls.Add(_lblClient);
            infoPanel.Controls.Add(_lblClientVersion);
            infoPanel.Controls.Add(_lblOperatingSystem);

            // Collection management
            var collectionPanel = new Panel
            {
                Location = new Point(25, 405),
                Size = new Size(850, 105),
                BackColor = Color.FromArgb(20, 27, 40)
            };

            Controls.Add(collectionPanel);

            var collectionTitle = new Label
            {
                Text = "COLLECTION MANAGEMENT",
                Location = new Point(15, 12),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold)
            };

            collectionPanel.Controls.Add(collectionTitle);

            var filterLabel = new Label
            {
                Text = "Filter",
                Location = new Point(15, 45),
                AutoSize = true
            };

            collectionPanel.Controls.Add(filterLabel);

            _txtCollectionFilter = new TextBox
            {
                Location = new Point(15, 68),
                Size = new Size(220, 27),
                BackColor = Color.FromArgb(30, 38, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false
            };

            _txtCollectionFilter.TextChanged +=
                TxtCollectionFilter_TextChanged;

            collectionPanel.Controls.Add(
                _txtCollectionFilter);

            var collectionLabel = new Label
            {
                Text = "Collection",
                Location = new Point(250, 45),
                AutoSize = true
            };

            collectionPanel.Controls.Add(collectionLabel);

            _cmbCollection = new ComboBox
            {
                Location = new Point(250, 68),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 38, 52),
                ForeColor = Color.White,
                Enabled = false
            };

            _cmbCollection.SelectedIndexChanged +=
                CmbCollection_SelectedIndexChanged;

            collectionPanel.Controls.Add(
                _cmbCollection);

            _btnAddToCollection = new Button
            {
                Text = "ADD",
                Location = new Point(565, 67),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(40, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            _btnAddToCollection.FlatAppearance.BorderColor =
                Color.FromArgb(80, 100, 130);

            _btnAddToCollection.Click +=
                BtnAddToCollection_Click;

            collectionPanel.Controls.Add(
                _btnAddToCollection);

            _btnRemoveFromCollection = new Button
            {
                Text = "REMOVE",
                Location = new Point(690, 67),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(40, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            _btnRemoveFromCollection.FlatAppearance.BorderColor =
                Color.FromArgb(80, 100, 130);

            _btnRemoveFromCollection.Click +=
                BtnRemoveFromCollection_Click;

            collectionPanel.Controls.Add(
                _btnRemoveFromCollection);

            // Activity log
            var logTitle = new Label
            {
                Text = "ACTIVITY LOG",
                Location = new Point(25, 530),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold)
            };

            Controls.Add(logTitle);

            _txtLog = new TextBox
            {
                Location = new Point(25, 560),
                Size = new Size(850, 85),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(15, 20, 30),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };

            Controls.Add(_txtLog);

            _txtComputerName.KeyDown +=
                TxtComputerName_KeyDown;
        }

        private Label CreateInfoLabel(
            string title,
            Point location)
        {
            return new Label
            {
                Text = title,
                Location = location,
                AutoSize = true,
                ForeColor = Color.LightGray
            };
        }

        private async void BtnLookup_Click(
            object? sender,
            EventArgs e)
        {
            await LookupComputerAsync();
        }

        private async void TxtComputerName_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LookupComputerAsync();
            }
        }

        private async System.Threading.Tasks.Task
            LookupComputerAsync()
        {
            string computerName =
                _txtComputerName.Text.Trim();

            if (string.IsNullOrWhiteSpace(computerName))
            {
                MessageBox.Show(
                    "Enter a computer name.",
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                SetBusy(true);

                ClearCollectionControls();

                _currentComputerName =
                    computerName;

                WriteLog(
                    $"Looking up '{computerName}' in MECM...");

                var result =
                    await _mecmManagementService
                        .GetComputerAsync(computerName);

                if (!result.Success)
                {
                    ClearComputerInformation();

                    WriteLog(
                        $"Lookup failed: {result.Error}");

                    MessageBox.Show(
                        result.Error,
                        "MECM Lookup",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                _lblComputerName.Text =
                    $"Computer: {result.Name}";

                _lblResourceId.Text =
                    $"Resource ID: {result.ResourceId}";

                _lblClient.Text =
                    $"MECM Client: {result.Client}";

                _lblClientVersion.Text =
                    $"Client Version: {result.ClientVersion}";

                _lblOperatingSystem.Text =
                    $"Operating System: {result.OperatingSystem}";

                WriteLog(
                    $"Computer found: {result.Name}");

                WriteLog(
                    $"Resource ID: {result.ResourceId}");

                WriteLog(
                    $"MECM Client: {result.Client}");

                WriteLog(
                    $"Client Version: {result.ClientVersion}");

                await LoadCollectionsAsync();
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async System.Threading.Tasks.Task
            LoadCollectionsAsync()
        {
            WriteLog(
                "Loading MECM collections...");

            var result =
                await _mecmManagementService
                    .GetCollectionsAsync();

            if (!result.Success)
            {
                WriteLog(
                    $"Unable to load collections: {result.Error}");

                MessageBox.Show(
                    result.Error,
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            _collections =
                result.Collections;

            ApplyCollectionFilter();

            _txtCollectionFilter.Enabled = true;
            _cmbCollection.Enabled =
                _cmbCollection.Items.Count > 0;

            WriteLog(
                $"Loaded {_collections.Count} MECM collections.");

            if (_collections.Count == 0)
            {
                WriteLog(
                    "No MECM collections were returned.");
            }
        }

        private void ApplyCollectionFilter()
        {
            if (_cmbCollection == null)
                return;

            string filter =
                _txtCollectionFilter?.Text.Trim() ??
                string.Empty;

            string? selectedCollection =
                _cmbCollection.SelectedItem
                    ?.ToString();

            _cmbCollection.BeginUpdate();

            try
            {
                _cmbCollection.Items.Clear();

                foreach (MecmCollection collection
                    in _collections)
                {
                    string displayText =
                        $"{collection.Name} ({collection.CollectionId})";

                    if (string.IsNullOrWhiteSpace(filter) ||
                        displayText.Contains(
                            filter,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        _cmbCollection.Items.Add(
                            new CollectionDisplayItem(
                                collection,
                                displayText));
                    }
                }

                if (!string.IsNullOrWhiteSpace(
                        selectedCollection))
                {
                    for (int i = 0;
                         i < _cmbCollection.Items.Count;
                         i++)
                    {
                        if (_cmbCollection.Items[i]
                                ?.ToString() ==
                            selectedCollection)
                        {
                            _cmbCollection.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            finally
            {
                _cmbCollection.EndUpdate();
            }

            UpdateCollectionButtons();
        }

        private void TxtCollectionFilter_TextChanged(
            object? sender,
            EventArgs e)
        {
            ApplyCollectionFilter();
        }

        private void CmbCollection_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            UpdateCollectionButtons();
        }

        private void UpdateCollectionButtons()
        {
            bool enabled =
                _cmbCollection?.Enabled == true &&
                _cmbCollection.SelectedItem != null;

            _btnAddToCollection.Enabled =
                enabled;

            _btnRemoveFromCollection.Enabled =
                enabled;
        }

        private async void BtnAddToCollection_Click(
            object? sender,
            EventArgs e)
        {
            CollectionDisplayItem? selected =
                _cmbCollection.SelectedItem
                    as CollectionDisplayItem;

            if (selected == null)
                return;

            string collectionName =
                selected.Collection.Name;

            string collectionId =
                selected.Collection.CollectionId;

            DialogResult confirmation =
                MessageBox.Show(
                    $"Add computer '{_currentComputerName}' to:" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    $"{collectionName}" +
                    $"{Environment.NewLine}" +
                    $"Collection ID: {collectionId}" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    "Continue?",
                    "Confirm Collection Change",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirmation !=
                DialogResult.Yes)
            {
                return;
            }

            try
            {
                SetBusy(true);

                WriteLog(
                    $"Adding '{_currentComputerName}' to '{collectionName}'...");

                var result =
                    await _mecmManagementService
                        .AddComputerToCollectionAsync(
                            _currentComputerName,
                            collectionId);

                if (!result.Success)
                {
                    WriteLog(
                        $"Add failed: {result.Error}");

                    MessageBox.Show(
                        result.Error,
                        "MECM",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                WriteLog(
                    $"Successfully added '{_currentComputerName}' to '{collectionName}'.");

                MessageBox.Show(
                    $"Computer '{_currentComputerName}' was added to the collection.",
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void BtnRemoveFromCollection_Click(
            object? sender,
            EventArgs e)
        {
            CollectionDisplayItem? selected =
                _cmbCollection.SelectedItem
                    as CollectionDisplayItem;

            if (selected == null)
                return;

            string collectionName =
                selected.Collection.Name;

            string collectionId =
                selected.Collection.CollectionId;

            DialogResult confirmation =
                MessageBox.Show(
                    $"Remove computer '{_currentComputerName}' from:" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    $"{collectionName}" +
                    $"{Environment.NewLine}" +
                    $"Collection ID: {collectionId}" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    "Continue?",
                    "Confirm Collection Change",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirmation !=
                DialogResult.Yes)
            {
                return;
            }

            try
            {
                SetBusy(true);

                WriteLog(
                    $"Removing '{_currentComputerName}' from '{collectionName}'...");

                var result =
                    await _mecmManagementService
                        .RemoveComputerFromCollectionAsync(
                            _currentComputerName,
                            collectionId);

                if (!result.Success)
                {
                    WriteLog(
                        $"Remove failed: {result.Error}");

                    MessageBox.Show(
                        result.Error,
                        "MECM",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                WriteLog(
                    $"Successfully removed '{_currentComputerName}' from '{collectionName}'.");

                MessageBox.Show(
                    $"Computer '{_currentComputerName}' was removed from the collection.",
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ClearCollectionControls()
        {
            _collections.Clear();

            _txtCollectionFilter.Clear();

            _txtCollectionFilter.Enabled =
                false;

            _cmbCollection.Items.Clear();

            _cmbCollection.SelectedIndex = -1;

            _cmbCollection.Enabled =
                false;

            _btnAddToCollection.Enabled =
                false;

            _btnRemoveFromCollection.Enabled =
                false;
        }

        private void ClearComputerInformation()
        {
            _lblComputerName.Text =
                "Computer:";

            _lblResourceId.Text =
                "Resource ID:";

            _lblClient.Text =
                "MECM Client:";

            _lblClientVersion.Text =
                "Client Version:";

            _lblOperatingSystem.Text =
                "Operating System:";

            _currentComputerName =
                string.Empty;

            ClearCollectionControls();
        }

        private void SetBusy(bool busy)
        {
            _btnLookup.Enabled = !busy;
            _txtComputerName.Enabled = !busy;

            _txtCollectionFilter.Enabled =
                !busy &&
                _collections.Count > 0;

            _cmbCollection.Enabled =
                !busy &&
                _cmbCollection.Items.Count > 0;

            if (busy)
            {
                _btnAddToCollection.Enabled =
                    false;

                _btnRemoveFromCollection.Enabled =
                    false;
            }
            else
            {
                UpdateCollectionButtons();
            }

            Cursor = busy
                ? Cursors.WaitCursor
                : Cursors.Default;
        }

        private void WriteLog(string message)
        {
            _txtLog.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] {message}" +
                Environment.NewLine);
        }

        private class CollectionDisplayItem
        {
            public MecmCollection Collection { get; }

            private readonly string _displayText;

            public CollectionDisplayItem(
                MecmCollection collection,
                string displayText)
            {
                Collection = collection;
                _displayText = displayText;
            }

            public override string ToString()
            {
                return _displayText;
            }
        }
    }
}