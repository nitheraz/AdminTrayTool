using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
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
            BuildInterface();
        }

        // =============================================================
        // FORM INITIALISATION
        // =============================================================

        private void InitializeForm()
        {
            Text = "MECM Management";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(890, 800);

            MinimumSize =
                new Size(820, 700);

            BackColor =
                Color.FromArgb(
                    10,
                    15,
                    25);

            ForeColor =
                Color.White;

            Font =
                new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Regular);

            FormBorderStyle =
                FormBorderStyle.FixedSingle;

            MaximizeBox = false;
        }

        // =============================================================
        // BUILD INTERFACE
        // =============================================================

        private void BuildInterface()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,

                Padding =
                    new Padding(30),

                BackColor =
                    Color.FromArgb(
                        10,
                        15,
                        25)
            };

            Controls.Add(mainPanel);

            // =========================================================
            // HEADER
            // =========================================================

            var lblTitle = new Label
            {
                Text =
                    "MECM / SCCM",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        20F,
                        FontStyle.Bold),

                ForeColor =
                    Color.White,

                Location =
                    new Point(
                        30,
                        25)
            };

            mainPanel.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text =
                    "Find Windows computers and manage MECM collection membership",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        9.5F),

                ForeColor =
                    Color.FromArgb(
                        150,
                        160,
                        175),

                Location =
                    new Point(
                        33,
                        62)
            };

            mainPanel.Controls.Add(lblSubtitle);

            var headerLine = new Panel
            {
                Location =
                    new Point(
                        30,
                        90),

                Size =
                    new Size(
                        820,
                        1),

                BackColor =
                    Color.FromArgb(
                        45,
                        55,
                        70)
            };

            mainPanel.Controls.Add(headerLine);

            // =========================================================
            // COMPUTER LOOKUP
            // =========================================================

            var lookupPanel = CreatePanel(
                new Point(30, 105),
                new Size(820, 105));

            mainPanel.Controls.Add(
                lookupPanel);

            var lblComputerName = CreateLabel(
                "COMPUTER NAME",
                new Point(
                    20,
                    18));

            lookupPanel.Controls.Add(
                lblComputerName);

            _txtComputerName = new TextBox
            {
                Location =
                    new Point(
                        20,
                        45),

                Size =
                    new Size(
                        400,
                        32),

                BackColor =
                    Color.FromArgb(
                        20,
                        27,
                        40),

                ForeColor =
                    Color.White,

                BorderStyle =
                    BorderStyle.FixedSingle,

                Font =
                    new Font(
                        "Segoe UI",
                        10.5F)
            };

            _txtComputerName.KeyDown +=
                TxtComputerName_KeyDown;

            lookupPanel.Controls.Add(
                _txtComputerName);

            _btnLookup = CreateButton(
                "LOOK UP",
                new Point(
                    610,
                    43),
                new Size(
                    180,
                    36));

            _btnLookup.Click +=
                BtnLookup_Click;

            lookupPanel.Controls.Add(
                _btnLookup);

            // =========================================================
            // COMPUTER INFORMATION
            // =========================================================

            var infoPanel = CreatePanel(
                new Point(30, 220),
                new Size(820, 220));

            mainPanel.Controls.Add(
                infoPanel);

            var lblInfoTitle = new Label
            {
                Text =
                    "MECM COMPUTER INFORMATION",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        11F,
                        FontStyle.Bold),

                ForeColor =
                    Color.White,

                Location =
                    new Point(
                        20,
                        18)
            };

            infoPanel.Controls.Add(
                lblInfoTitle);

            _lblComputerName =
                AddInfoRow(
                    infoPanel,
                    "Computer",
                    55,
                    out _,
                    "—");

            _lblResourceId =
                AddInfoRow(
                    infoPanel,
                    "Resource ID",
                    85,
                    out _,
                    "—");

            _lblClient =
                AddInfoRow(
                    infoPanel,
                    "MECM Client",
                    115,
                    out _,
                    "—");

            _lblClientVersion =
                AddInfoRow(
                    infoPanel,
                    "Client Version",
                    145,
                    out _,
                    "—");

            _lblOperatingSystem =
                AddInfoRow(
                    infoPanel,
                    "Operating System",
                    175,
                    out _,
                    "—");

            // =========================================================
            // COLLECTION MANAGEMENT
            // =========================================================

            var collectionPanel = CreatePanel(
                new Point(30, 450),
                new Size(820, 125));

            mainPanel.Controls.Add(
                collectionPanel);

            var lblCollectionTitle = new Label
            {
                Text =
                    "COLLECTION MANAGEMENT",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        11F,
                        FontStyle.Bold),

                ForeColor =
                    Color.White,

                Location =
                    new Point(
                        20,
                        18)
            };

            collectionPanel.Controls.Add(
                lblCollectionTitle);

            var lblFilter = CreateLabel(
                "FILTER",
                new Point(
                    20,
                    50));

            collectionPanel.Controls.Add(
                lblFilter);

            _txtCollectionFilter = new TextBox
            {
                Location =
                    new Point(
                        20,
                        72),

                Size =
                    new Size(
                        220,
                        30),

                BackColor =
                    Color.FromArgb(
                        20,
                        27,
                        40),

                ForeColor =
                    Color.White,

                BorderStyle =
                    BorderStyle.FixedSingle,

                Font =
                    new Font(
                        "Segoe UI",
                        9.5F),

                Enabled = false
            };

            _txtCollectionFilter.TextChanged +=
                TxtCollectionFilter_TextChanged;

            collectionPanel.Controls.Add(
                _txtCollectionFilter);

            var lblCollection = CreateLabel(
                "COLLECTION",
                new Point(
                    255,
                    50));

            collectionPanel.Controls.Add(
                lblCollection);

            _cmbCollection = new ComboBox
            {
                Location =
                    new Point(
                        255,
                        72),

                Size =
                    new Size(
                        310,
                        30),

                DropDownStyle =
                    ComboBoxStyle.DropDownList,

                BackColor =
                    Color.FromArgb(
                        20,
                        27,
                        40),

                ForeColor =
                    Color.White,

                FlatStyle =
                    FlatStyle.Flat,

                Enabled = false
            };

            _cmbCollection.SelectedIndexChanged +=
                CmbCollection_SelectedIndexChanged;

            collectionPanel.Controls.Add(
                _cmbCollection);

            _btnAddToCollection = CreateButton(
                "ADD",
                new Point(
                    580,
                    70),
                new Size(
                    105,
                    34));

            _btnAddToCollection.Enabled = false;

            _btnAddToCollection.Click +=
                BtnAddToCollection_Click;

            collectionPanel.Controls.Add(
                _btnAddToCollection);

            _btnRemoveFromCollection = CreateButton(
                "REMOVE",
                new Point(
                    695,
                    70),
                new Size(
                    105,
                    34));

            _btnRemoveFromCollection.Enabled = false;

            _btnRemoveFromCollection.Click +=
                BtnRemoveFromCollection_Click;

            collectionPanel.Controls.Add(
                _btnRemoveFromCollection);

            // =========================================================
            // ACTIVITY LOG
            // =========================================================

            var logPanel = CreatePanel(
                new Point(30, 590),
                new Size(820, 110));

            mainPanel.Controls.Add(
                logPanel);

            var lblLogTitle = new Label
            {
                Text =
                    "ACTIVITY LOG",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),

                ForeColor =
                    Color.FromArgb(
                        120,
                        135,
                        150),

                Location =
                    new Point(
                        20,
                        10)
            };

            logPanel.Controls.Add(
                lblLogTitle);

            _txtLog = new TextBox
            {
                Location =
                    new Point(
                        20,
                        32),

                Size =
                    new Size(
                        780,
                        60),

                Multiline = true,

                ReadOnly = true,

                ScrollBars =
                    ScrollBars.Vertical,

                BackColor =
                    Color.FromArgb(
                        6,
                        10,
                        18),

                ForeColor =
                    Color.FromArgb(
                        150,
                        160,
                        175),

                BorderStyle =
                    BorderStyle.FixedSingle,

                Font =
                    new Font(
                        "Consolas",
                        8.5F)
            };

            logPanel.Controls.Add(
                _txtLog);

            _txtComputerName.Focus();
                      
            var actionsPanel =
                CreatePanel(
                    new Point(30, 715),
                    new Size(820, 70));

                        mainPanel.Controls.Add(actionsPanel);

                        var lblActionsTitle = new Label
                        {
                            Text = "FORM ACTIONS",
                            AutoSize = true,
                            Font = new Font(
                                "Segoe UI",
                                8.5F,
                                FontStyle.Bold),
                            ForeColor = Color.FromArgb(140, 150, 165),
                            Location = new Point(20, 15)
                        };

                        actionsPanel.Controls.Add(lblActionsTitle);

                        var btnClose = new HudButton
                        {
                            Text = "CLOSE",
                            Location = new Point(650, 13),
                            Size = new Size(150, 32)
                        };

                        btnClose.Click +=
                            (_, _) => Close();

                        actionsPanel.Controls.Add(btnClose);


        }

        // =============================================================
        // LOOKUP
        // =============================================================

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

        private async Task LookupComputerAsync()
        {
            string computerName =
                _txtComputerName.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                computerName))
            {
                MessageBox.Show(
                    "Enter a computer name.",
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                _txtComputerName.Focus();

                return;
            }

            try
            {
                SetBusy(true);

                ClearCollectionControls();

                ClearComputerInformationOnly();

                _currentComputerName =
                    computerName;

                WriteLog(
                    $"Looking up '{computerName}' in MECM...");

                var result =
                    await _mecmManagementService
                        .GetComputerAsync(
                            computerName);

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

                WriteLog(
                    $"Operating System: {result.OperatingSystem}");

                await LoadCollectionsAsync();
            }
            catch (Exception ex)
            {
                WriteLog(
                    $"Lookup error: {ex.Message}");

                MessageBox.Show(
                    ex.Message,
                    "MECM Lookup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // =============================================================
        // COLLECTIONS
        // =============================================================

        private async Task LoadCollectionsAsync()
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
                _txtCollectionFilter?.Text.Trim()
                ?? string.Empty;

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
                            _cmbCollection.SelectedIndex =
                                i;

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

        // =============================================================
        // ADD TO COLLECTION
        // =============================================================

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
            catch (Exception ex)
            {
                WriteLog(
                    $"Add error: {ex.Message}");

                MessageBox.Show(
                    ex.Message,
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // =============================================================
        // REMOVE FROM COLLECTION
        // =============================================================

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
            catch (Exception ex)
            {
                WriteLog(
                    $"Remove error: {ex.Message}");

                MessageBox.Show(
                    ex.Message,
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // =============================================================
        // CLEAR COLLECTION CONTROLS
        // =============================================================

        private void ClearCollectionControls()
        {
            _collections.Clear();

            _txtCollectionFilter.Clear();

            _txtCollectionFilter.Enabled =
                false;

            _cmbCollection.Items.Clear();

            _cmbCollection.SelectedIndex =
                -1;

            _cmbCollection.Enabled =
                false;

            _btnAddToCollection.Enabled =
                false;

            _btnRemoveFromCollection.Enabled =
                false;
        }

        // =============================================================
        // CLEAR COMPUTER INFORMATION
        // =============================================================

        private void ClearComputerInformationOnly()
        {
            _lblComputerName.Text =
                "Computer: —";

            _lblResourceId.Text =
                "Resource ID: —";

            _lblClient.Text =
                "MECM Client: —";

            _lblClientVersion.Text =
                "Client Version: —";

            _lblOperatingSystem.Text =
                "Operating System: —";
        }

        private void ClearComputerInformation()
        {
            ClearComputerInformationOnly();

            _currentComputerName =
                string.Empty;

            ClearCollectionControls();
        }

        // =============================================================
        // BUSY STATE
        // =============================================================

        private void SetBusy(bool busy)
        {
            _btnLookup.Enabled =
                !busy;

            _txtComputerName.Enabled =
                !busy;

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

            Cursor =
                busy
                    ? Cursors.WaitCursor
                    : Cursors.Default;
        }

        // =============================================================
        // ACTIVITY LOG
        // =============================================================

        private void WriteLog(
            string message)
        {
            if (_txtLog == null)
                return;

            if (string.IsNullOrWhiteSpace(message))
                return;

            _txtLog.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] {message}" +
                Environment.NewLine);
        }

        // =============================================================
        // UI HELPERS
        // =============================================================

        private Panel CreatePanel(
            Point location,
            Size size)
        {
            return new HudPanel
            {
                Location = location,
                Size = size
            };
        }

        private Label CreateLabel(
            string text,
            Point location)
        {
            return new Label
            {
                Text =
                    text,

                Location =
                    location,

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),

                ForeColor =
                    Color.FromArgb(
                        140,
                        150,
                        165)
            };
        }

        private Label AddInfoRow(
            Panel parent,
            string name,
            int y,
            out Label valueLabel,
            string defaultValue)
        {
            var nameLabel = new Label
            {
                Text =
                    name.ToUpperInvariant(),

                Location =
                    new Point(
                        20,
                        y),

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),

                ForeColor =
                    Color.FromArgb(
                        120,
                        135,
                        150)
            };

            parent.Controls.Add(
                nameLabel);

            valueLabel = new Label
            {
                Text =
                    $"{name}: {defaultValue}",

                Location =
                    new Point(
                        170,
                        y - 1),

                AutoSize = true,

                MaximumSize =
                    new Size(
                        620,
                        0),

                Font =
                    new Font(
                        "Segoe UI",
                        9.5F),

                ForeColor =
                    Color.White
            };

            parent.Controls.Add(
                valueLabel);

            return valueLabel;
        }

        private Button CreateButton(
            string text,
            Point location,
            Size size)
        {
            return new HudButton
            {
                Text = text,
                Location = location,
                Size = size
            };
        }

        // =============================================================
        // COLLECTION DISPLAY ITEM
        // =============================================================

        private class CollectionDisplayItem
        {
            public MecmCollection Collection { get; }

            private readonly string _displayText;

            public CollectionDisplayItem(
                MecmCollection collection,
                string displayText)
            {
                Collection =
                    collection;

                _displayText =
                    displayText;
            }

            public override string ToString()
            {
                return _displayText;
            }
        }
    }
}