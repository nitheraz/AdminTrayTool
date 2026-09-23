using AdminTrayTool.Services;

namespace AdminTrayTool.Forms
{
    public class BulkChromebookManagementForm : Form
    {
        private readonly GamService _gamService;

        private readonly List<BulkChromebookItem> _devices = [];
        private readonly BindingSource _deviceBindingSource = [];

        private Button _btnImport = null!;
        private Button _btnTemplate = null!;
        private Button _btnClear = null!;
        private Button _btnSelectAll = null!;
        private Button _btnSelectNone = null!;
        private Button _btnPerform = null!;
        private Button _btnClose = null!;
        private Button _btnExport = null!;

        private DataGridView _gridDevices = null!;
        private Label _lblStatus = null!;
        private Label _lblSelected = null!;
        private ProgressBar _progressBar = null!;
        private Label _lblProgress = null!;

        private ComboBox _cmbAction = null!;
        private ComboBox _cmbBulkOrgUnit = null!;
        private Label _lblAction = null!;
        private Label _lblBulkOrgUnit = null!;

        private bool _loadingOrgUnits;
        private bool _bulkOperationRunning;

        public BulkChromebookManagementForm()
        {
            _gamService = new GamService();

            InitializeForm();
            BuildInterface();
        }

        // =============================================================
        // FORM INITIALISATION
        // =============================================================

        private void InitializeForm()
        {
            Text = "Bulk Chromebook Management";

            StartPosition =
                FormStartPosition.CenterParent;

            ClientSize =
                new Size(900, 790);

            MinimumSize =
                new Size(900, 790);

            MaximumSize =
                new Size(900, 790);

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
                    10F);

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;
        }

        // =============================================================
        // BUILD INTERFACE
        // =============================================================

        private void BuildInterface()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
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
                Text = "BULK CHROMEBOOK MANAGEMENT",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    20F,
                    FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 25)
            };

            mainPanel.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text =
                    "Import Chromebook serial numbers for bulk management",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9.5F),
                ForeColor =
                    Color.FromArgb(
                        150,
                        160,
                        175),
                Location = new Point(33, 62)
            };

            mainPanel.Controls.Add(lblSubtitle);

            var headerLine = new Panel
            {
                Location = new Point(30, 90),
                Size = new Size(820, 1),
                BackColor =
                    Color.FromArgb(
                        45,
                        55,
                        70)
            };

            mainPanel.Controls.Add(headerLine);

            // =========================================================
            // ACTION / IMPORT PANEL
            // =========================================================

            var actionPanel = new HudPanel
            {
                Location = new Point(30, 105),
                Size = new Size(820, 120)
            };

            mainPanel.Controls.Add(actionPanel);

            _lblAction = new Label
            {
                Text = "ACTION",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor =
                    Color.FromArgb(
                        120,
                        135,
                        150),
                Location = new Point(20, 12)
            };

            actionPanel.Controls.Add(_lblAction);

            _cmbAction = new ComboBox
            {
                Location = new Point(20, 36),
                Size = new Size(300, 32),
                BackColor =
                    Color.FromArgb(
                        20,
                        27,
                        40),
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    10F),
                DropDownStyle =
                    ComboBoxStyle.DropDownList,
                FlatStyle =
                    FlatStyle.Flat
            };

            _cmbAction.Items.Add(
                "Disable Chromebook");

            _cmbAction.Items.Add(
                "Re-enable Chromebook");

            _cmbAction.Items.Add(
                "Move to OU");

            _cmbAction.Items.Add(
                "Powerwash");

            _cmbAction.Items.Add(
                "Clear Profiles");

            _cmbAction.Items.Add(
                "Update Asset ID");

            _cmbAction.SelectedIndexChanged +=
                CmbAction_SelectedIndexChanged;

            actionPanel.Controls.Add(_cmbAction);

            // =========================================================
            // ORGANISATIONAL UNIT
            // =========================================================

            _lblBulkOrgUnit = new Label
            {
                Text = "ORGANISATIONAL UNIT",
                AutoSize = true,
                Visible = false,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor =
                    Color.FromArgb(
                        120,
                        135,
                        150),
                Location = new Point(340, 12)
            };

            actionPanel.Controls.Add(_lblBulkOrgUnit);

            _cmbBulkOrgUnit = new ComboBox
            {
                Location = new Point(340, 36),
                Size = new Size(450, 32),
                BackColor =
                    Color.FromArgb(
                        20,
                        27,
                        40),
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    9.5F),
                DropDownStyle =
                    ComboBoxStyle.DropDownList,
                FlatStyle =
                    FlatStyle.Flat,
                Visible = false
            };

            _cmbBulkOrgUnit.SelectedIndexChanged +=
                CmbBulkOrgUnit_SelectedIndexChanged;

            actionPanel.Controls.Add(
                _cmbBulkOrgUnit);

            // =========================================================
            // TEMPLATE BUTTON
            // =========================================================

            _btnTemplate = new HudButton
            {
                Text = "DOWNLOAD TEMPLATE",
                Location = new Point(20, 80),
                Size = new Size(180, 30),
                Enabled = false
            };

            _btnTemplate.Click +=
                BtnTemplate_Click;

            actionPanel.Controls.Add(
                _btnTemplate);

            // =========================================================
            // IMPORT BUTTON
            // =========================================================

            _btnImport = new HudButton
            {
                Text = "IMPORT CSV",
                Location = new Point(210, 80),
                Size = new Size(140, 30),
                Enabled = false
            };

            _btnImport.Click +=
                BtnImport_Click;

            actionPanel.Controls.Add(
                _btnImport);

            // =========================================================
            // CLEAR BUTTON
            // =========================================================

            _btnClear = new HudButton
            {
                Text = "CLEAR LIST",
                Location = new Point(360, 80),
                Size = new Size(140, 30),
                Enabled = false
            };

            _btnClear.Click +=
                BtnClear_Click;

            actionPanel.Controls.Add(
                _btnClear);

            // =========================================================
            // STATUS
            // =========================================================

            _lblStatus = new Label
            {
                Text =
                    "Select an action to begin.",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    8.5F),
                ForeColor =
                    Color.FromArgb(
                        150,
                        160,
                        175),
                Location = new Point(515, 88)
            };

            actionPanel.Controls.Add(
                _lblStatus);

            // =========================================================
            // DEVICE LIST
            // =========================================================

            var listPanel = new HudPanel
            {
                Location = new Point(30, 240),
                Size = new Size(820, 335)
            };

            mainPanel.Controls.Add(listPanel);

            _gridDevices = new DataGridView
            {
                Location = new Point(15, 15),
                Size = new Size(790, 265),

                BackgroundColor =
                    Color.FromArgb(
                        6,
                        10,
                        18),

                ForeColor =
                    Color.White,

                GridColor =
                    Color.FromArgb(
                        45,
                        55,
                        70),

                BorderStyle =
                    BorderStyle.None,

                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,

                AutoGenerateColumns = false,

                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,

                MultiSelect = true,

                ReadOnly = true,

                RowHeadersVisible = false,

                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill
            };

            _gridDevices.SelectionChanged +=
                GridDevices_SelectionChanged;

            _gridDevices.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor =
                        Color.FromArgb(
                            20,
                            27,
                            40),

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F,
                            FontStyle.Bold)
                };

            _gridDevices.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor =
                        Color.FromArgb(
                            10,
                            15,
                            25),

                    ForeColor =
                        Color.White,

                    SelectionBackColor =
                        Color.FromArgb(
                            35,
                            50,
                            70),

                    SelectionForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F)
                };

            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "SerialNumber",
                    HeaderText = "SERIAL NUMBER",
                    DataPropertyName = "SerialNumber",
                    FillWeight = 25
                });

            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "STATUS",
                    DataPropertyName = "Status",
                    FillWeight = 18
                });

            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "AssetId",
                    HeaderText = "CURRENT ASSET ID",
                    DataPropertyName = "AssetId",
                    FillWeight = 22
                });

            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "RequestedAssetId",
                    HeaderText = "NEW ASSET ID",
                    DataPropertyName = "RequestedAssetId",
                    FillWeight = 22
                });

            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Result",
                    HeaderText = "RESULT",
                    DataPropertyName = "Result",
                    FillWeight = 18
                });

            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Error",
                    HeaderText = "ERROR",
                    DataPropertyName = "Error",
                    FillWeight = 30
                });

            _deviceBindingSource.DataSource =
                _devices;

            _gridDevices.DataSource =
                _deviceBindingSource;

            listPanel.Controls.Add(
                _gridDevices);

            // =========================================================
            // SELECTION CONTROLS
            // =========================================================

            _btnSelectAll = new HudButton
            {
                Text = "SELECT ALL",
                Location = new Point(15, 292),
                Size = new Size(125, 30),
                Enabled = false
            };

            _btnSelectAll.Click +=
                BtnSelectAll_Click;

            listPanel.Controls.Add(
                _btnSelectAll);

            _btnSelectNone = new HudButton
            {
                Text = "SELECT NONE",
                Location = new Point(150, 292),
                Size = new Size(125, 30),
                Enabled = false
            };

            _btnSelectNone.Click +=
                BtnSelectNone_Click;

            listPanel.Controls.Add(
                _btnSelectNone);

            _lblSelected = new Label
            {
                Text = "SELECTED: 0",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                ForeColor =
                    Color.FromArgb(
                        150,
                        160,
                        175),
                Location = new Point(440, 301)
            };

            listPanel.Controls.Add(
                _lblSelected);

            // =========================================================
            // PROGRESS
            // =========================================================

            _progressBar = new ProgressBar
            {
                Location = new Point(30, 590),
                Size = new Size(500, 20),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Visible = false
            };

            mainPanel.Controls.Add(
                _progressBar);

            _lblProgress = new Label
            {
                Text = "READY",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                ForeColor =
                    Color.FromArgb(
                        150,
                        160,
                        175),
                Location = new Point(545, 590),
                Visible = false
            };

            mainPanel.Controls.Add(
                _lblProgress);

            // =========================================================
            // PERFORM ACTION
            // =========================================================

            _btnPerform = new HudButton
            {
                Text = "PERFORM ACTION",
                Location = new Point(30, 630),
                Size = new Size(220, 40),
                Enabled = false
            };

            _btnPerform.Click +=
                async (s, e) =>
                    await BtnPerform_ClickAsync();

            mainPanel.Controls.Add(
                _btnPerform);

            // =========================================================
            // EXPORT
            // =========================================================

            _btnExport = new HudButton
            {
                Text = "EXPORT RESULTS",
                Location = new Point(265, 630),
                Size = new Size(180, 40),
                Enabled = false
            };

            _btnExport.Click +=
                BtnExport_Click;

            mainPanel.Controls.Add(
                _btnExport);

            // =========================================================
            // CLOSE
            // =========================================================

            _btnClose = new HudButton
            {
                Text = "CLOSE",
                Location = new Point(615, 630),
                Size = new Size(150, 40)
            };

            _btnClose.Click +=
                (s, e) => Close();

            mainPanel.Controls.Add(
                _btnClose);

            UpdateSelectionStatus();
            UpdatePerformButton();
        }

        // =============================================================
        // ACTION SELECTION
        // =============================================================

        private async void CmbAction_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (_bulkOperationRunning)
                return;

            if (_cmbAction == null ||
                _cmbBulkOrgUnit == null ||
                _lblBulkOrgUnit == null ||
                _btnImport == null ||
                _btnTemplate == null ||
                _lblStatus == null)
            {
                return;
            }

            string action =
                _cmbAction.SelectedItem?.ToString()
                ?? string.Empty;

            bool hasAction =
                !string.IsNullOrWhiteSpace(action);

            bool moveToOu =
                string.Equals(
                    action,
                    "Move to OU",
                    StringComparison.OrdinalIgnoreCase);

            _lblBulkOrgUnit.Visible =
                hasAction &&
                moveToOu;

            _cmbBulkOrgUnit.Visible =
                hasAction &&
                moveToOu;

            _btnImport.Enabled =
                hasAction;

            _btnTemplate.Enabled =
                hasAction;

            if (!hasAction)
            {
                _cmbBulkOrgUnit.Items.Clear();

                _lblStatus.Text =
                    "Select an action to begin.";

                UpdatePerformButton();

                return;
            }

            if (!moveToOu)
            {
                _cmbBulkOrgUnit.Items.Clear();

                _lblStatus.Text =
                    $"{action} selected.";

                UpdatePerformButton();

                return;
            }

            if (_loadingOrgUnits)
                return;

            await LoadBulkOrgUnitsAsync();
        }

        // =============================================================
        // ORGANISATIONAL UNIT SELECTION
        // =============================================================

        private void CmbBulkOrgUnit_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (_bulkOperationRunning)
                return;

            UpdatePerformButton();
        }

        // =============================================================
        // LOAD ORGANISATIONAL UNITS
        // =============================================================

        private async Task LoadBulkOrgUnitsAsync()
        {
            if (_cmbBulkOrgUnit == null ||
                _lblStatus == null)
            {
                return;
            }

            _loadingOrgUnits = true;

            try
            {
                _cmbBulkOrgUnit.Enabled =
                    false;

                _cmbBulkOrgUnit.Items.Clear();

                _cmbBulkOrgUnit.Items.Add(
                    "Loading organisational units...");

                _cmbBulkOrgUnit.SelectedIndex = 0;

                _lblStatus.Text =
                    "Loading organisational units...";

                var (Success, OrgUnitPaths, Error) =
                    await _gamService
                        .GetAllOrgUnitPathsAsync();

                _cmbBulkOrgUnit.Items.Clear();

                if (!Success ||
                    OrgUnitPaths == null ||
                    OrgUnitPaths.Count == 0)
                {
                    string error =
                        string.IsNullOrWhiteSpace(
                            Error)
                            ? "No organisational units were found."
                            : Error;

                    MessageBox.Show(
                        error,
                        "Move to OU",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    _lblStatus.Text =
                        "No organisational units found.";

                    return;
                }

                IEnumerable<string> orgUnitPaths =
                    OrgUnitPaths
                        .Where(
                            x =>
                                !string.IsNullOrWhiteSpace(x))
                        .Distinct(
                            StringComparer.OrdinalIgnoreCase)
                        .Order();

                foreach (string orgUnitPath in orgUnitPaths)
                {
                    _cmbBulkOrgUnit.Items.Add(
                        orgUnitPath);
                }

                if (_cmbBulkOrgUnit.Items.Count > 0)
                {
                    _cmbBulkOrgUnit.SelectedIndex = 0;
                }

                _lblStatus.Text =
                    $"{_cmbBulkOrgUnit.Items.Count} organisational unit(s) loaded.";
            }
            catch (Exception ex)
            {
                _cmbBulkOrgUnit.Items.Clear();

                _lblStatus.Text =
                    "Failed to load organisational units.";

                MessageBox.Show(
                    ex.Message,
                    "Move to OU",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _cmbBulkOrgUnit.Enabled =
                    true;

                _loadingOrgUnits = false;

                UpdatePerformButton();
            }
        }

        // =============================================================
        // DOWNLOAD TEMPLATE
        // =============================================================

        private void BtnTemplate_Click(
            object? sender,
            EventArgs e)
        {
            string action =
                _cmbAction.SelectedItem?.ToString()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(action))
            {
                MessageBox.Show(
                    "Please select an action first.",
                    "Download Template",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string fileName;
            string header;
            string description;

            switch (action)
            {
                case "Move to OU":

                    fileName =
                        "Chromebook_MoveToOU_Template.csv";

                    header =
                        "serialNumber";

                    description =
                        "Enter one Chromebook serial number per row. " +
                        "The organisational unit is selected in the application.";

                    break;

                case "Update Asset ID":

                    fileName =
                        "Chromebook_UpdateAssetID_Template.csv";

                    header =
                        "serialNumber,AssetId";

                    description =
                        "Enter the Chromebook serial number and the new Asset ID for each device.";

                    break;

                default:

                    fileName =
                        "Chromebook_Import_Template.csv";

                    header =
                        "serialNumber";

                    description =
                        "Enter one Chromebook serial number per row.";

                    break;
            }

            using var dialog =
                new SaveFileDialog
                {
                    Title =
                        "Save Chromebook Import Template",

                    Filter =
                        "CSV files (*.csv)|*.csv|All files (*.*)|*.*",

                    FileName =
                        fileName,

                    DefaultExt =
                        "csv",

                    AddExtension =
                        true,

                    OverwritePrompt =
                        true
                };

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            try
            {
                File.WriteAllText(
                    dialog.FileName,
                    header +
                    Environment.NewLine);

                MessageBox.Show(
                    $"{description}\r\n\r\n" +
                    $"Template saved to:\r\n{dialog.FileName}",
                    "Template Created",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Template Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =============================================================
        // IMPORT CSV
        // =============================================================

        private void BtnImport_Click(
            object? sender,
            EventArgs e)
        {
            if (_bulkOperationRunning)
                return;

            string action =
                _cmbAction.SelectedItem?.ToString()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(action))
            {
                MessageBox.Show(
                    "Please select an action before importing a CSV.",
                    "Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using var dialog =
                new OpenFileDialog
                {
                    Title =
                        "Import Chromebook CSV",

                    Filter =
                        "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*",

                    Multiselect =
                        false
                };

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            try
            {
                ImportSerialNumbers(
                    dialog.FileName,
                    action);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to import the CSV.\r\n\r\n" +
                    ex.Message,
                    "Import Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ImportSerialNumbers(
            string filePath,
            string action)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            if (string.IsNullOrWhiteSpace(action))
                return;

            string[] lines =
                File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                MessageBox.Show(
                    "The selected file is empty.",
                    "Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            bool updateAssetId =
                string.Equals(
                    action,
                    "Update Asset ID",
                    StringComparison.OrdinalIgnoreCase);

            string headerLine =
                lines[0]
                    .Trim()
                    .TrimStart('\uFEFF');

            if (string.IsNullOrWhiteSpace(headerLine))
            {
                MessageBox.Show(
                    "The CSV does not contain a header row.",
                    "Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string[] headers =
                [.. headerLine
                    .Split(',')
                    .Select(
                        x =>
                            x
                                .Trim()
                                .Trim('"'))];

            int serialColumnIndex =
                Array.FindIndex(
                    headers,
                    x =>
                        string.Equals(
                            x,
                            "serialNumber",
                            StringComparison.OrdinalIgnoreCase));

            if (serialColumnIndex < 0)
            {
                MessageBox.Show(
                    "The selected CSV does not contain a " +
                    "'serialNumber' column.\r\n\r\n" +
                    "Please use the DOWNLOAD TEMPLATE button.",
                    "Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            int assetColumnIndex =
                Array.FindIndex(
                    headers,
                    x =>
                        string.Equals(
                            x,
                            "AssetId",
                            StringComparison.OrdinalIgnoreCase));

            if (updateAssetId &&
                assetColumnIndex < 0)
            {
                MessageBox.Show(
                    "The Update Asset ID CSV must contain both:\r\n\r\n" +
                    "serialNumber\r\n" +
                    "AssetId",
                    "Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var importedDevices =
                new List<BulkChromebookItem>();

            var importedSerials =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            int duplicateCount = 0;
            int invalidCount = 0;

            for (int i = 1;
                 i < lines.Length;
                 i++)
            {
                string line =
                    lines[i].Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] columns =
                    line.Split(',');

                if (serialColumnIndex >=
                    columns.Length)
                {
                    invalidCount++;
                    continue;
                }

                string serial =
                    columns[serialColumnIndex]
                        .Trim()
                        .Trim('"');

                if (string.IsNullOrWhiteSpace(serial))
                {
                    invalidCount++;
                    continue;
                }

                if (!importedSerials.Add(serial))
                {
                    duplicateCount++;
                    continue;
                }

                string requestedAssetId =
                    string.Empty;

                if (updateAssetId)
                {
                    if (assetColumnIndex >=
                        columns.Length)
                    {
                        invalidCount++;
                        continue;
                    }

                    requestedAssetId =
                        columns[assetColumnIndex]
                            .Trim()
                            .Trim('"');

                    if (string.IsNullOrWhiteSpace(
                        requestedAssetId))
                    {
                        invalidCount++;
                        continue;
                    }
                }

                importedDevices.Add(
                    new BulkChromebookItem
                    {
                        SerialNumber =
                            serial,

                        Status =
                            "IMPORTED",

                        AssetId =
                            string.Empty,

                        RequestedAssetId =
                            requestedAssetId,

                        Result =
                            string.Empty,

                        Error =
                            string.Empty
                    });
            }

            _devices.Clear();

            _devices.AddRange(
                importedDevices);

            _deviceBindingSource.ResetBindings(false);

            ClearGridSelection();

            int importedCount =
                importedDevices.Count;

            _lblStatus.Text =
                $"{importedCount} device(s) loaded.";

            if (duplicateCount > 0)
            {
                _lblStatus.Text +=
                    $" {duplicateCount} duplicate(s) skipped.";
            }

            if (invalidCount > 0)
            {
                _lblStatus.Text +=
                    $" {invalidCount} invalid row(s) skipped.";
            }

            bool hasDevices =
                _devices.Count > 0;

            _btnClear.Enabled =
                hasDevices;

            _btnSelectAll.Enabled =
                hasDevices;

            _btnSelectNone.Enabled =
                hasDevices;

            _btnExport.Enabled =
                hasDevices;

            UpdateSelectionStatus();
            UpdatePerformButton();
        }

        // =============================================================
        // SELECTION
        // =============================================================

        private void BtnSelectAll_Click(
            object? sender,
            EventArgs e)
        {
            if (_bulkOperationRunning)
                return;

            if (_gridDevices.Rows.Count == 0)
                return;

            _gridDevices.ClearSelection();

            foreach (DataGridViewRow row
                in _gridDevices.Rows)
            {
                if (!row.IsNewRow)
                    row.Selected = true;
            }

            UpdateSelectionStatus();
            UpdatePerformButton();
        }

        private void BtnSelectNone_Click(
            object? sender,
            EventArgs e)
        {
            if (_bulkOperationRunning)
                return;

            ClearGridSelection();

            UpdateSelectionStatus();
            UpdatePerformButton();
        }

        private void GridDevices_SelectionChanged(
            object? sender,
            EventArgs e)
        {
            if (_bulkOperationRunning)
                return;

            UpdateSelectionStatus();
            UpdatePerformButton();
        }

        private void ClearGridSelection()
        {
            if (_gridDevices == null)
                return;

            _gridDevices.ClearSelection();
        }

        private void UpdateSelectionStatus()
        {
            if (_gridDevices == null ||
                _lblSelected == null)
            {
                return;
            }

            _lblSelected.Text =
                $"SELECTED: {_gridDevices.SelectedRows.Count}";
        }

        // =============================================================
        // CLEAR LIST
        // =============================================================

        private void BtnClear_Click(
            object? sender,
            EventArgs e)
        {
            if (_bulkOperationRunning)
                return;

            _devices.Clear();

            _deviceBindingSource.ResetBindings(false);

            ClearGridSelection();

            _lblStatus.Text =
                "No devices loaded.";

            _btnClear.Enabled =
                false;

            _btnSelectAll.Enabled =
                false;

            _btnSelectNone.Enabled =
                false;

            _btnPerform.Enabled =
                false;

            _btnExport.Enabled =
                false;

            UpdateSelectionStatus();
        }

        // =============================================================
        // PERFORM BUTTON STATE
        // =============================================================

        private void UpdatePerformButton()
        {
            if (_gridDevices == null ||
                _cmbAction == null ||
                _btnPerform == null)
            {
                return;
            }

            if (_bulkOperationRunning)
            {
                _btnPerform.Enabled =
                    false;

                return;
            }

            string action =
                _cmbAction.SelectedItem?.ToString()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(action))
            {
                _btnPerform.Enabled =
                    false;

                return;
            }

            bool hasSelection =
                _gridDevices.SelectedRows.Count > 0;

            bool moveToOu =
                string.Equals(
                    action,
                    "Move to OU",
                    StringComparison.OrdinalIgnoreCase);

            bool hasOu =
                false;

            if (_cmbBulkOrgUnit?.SelectedIndex >= 0)
            {
                string selectedOu =
                    _cmbBulkOrgUnit.SelectedItem?.ToString()
                    ?? string.Empty;

                hasOu =
                    !string.IsNullOrWhiteSpace(
                        selectedOu);
            }

            bool validAssetIds =
                true;

            if (string.Equals(
                action,
                "Update Asset ID",
                StringComparison.OrdinalIgnoreCase))
            {
                validAssetIds =
                    _gridDevices.SelectedRows
                        .Cast<DataGridViewRow>()
                        .All(
                            row =>
                            {
                                if (row.DataBoundItem
                                    is not BulkChromebookItem device)
                                {
                                    return false;
                                }

                                return
                                    !string.IsNullOrWhiteSpace(
                                        device.RequestedAssetId);
                            });
            }

            _btnPerform.Enabled =
                hasSelection &&
                validAssetIds &&
                (!moveToOu || hasOu);
        }

        // =============================================================
        // PERFORM ACTION
        // =============================================================

        private async Task BtnPerform_ClickAsync()
        {
            if (_bulkOperationRunning)
            {
                return;
            }

            if (_gridDevices == null || _cmbAction == null)
            {
                return;
            }

            string action =
                _cmbAction.SelectedItem?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(action))
            {
                MessageBox.Show(
                    "Please select an action.",
                    "Bulk Chromebook Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            List<DataGridViewRow> selectedRows =
                [.. _gridDevices.Rows
                    .Cast<DataGridViewRow>()
                    .Where(row => row.Selected && !row.IsNewRow)
                    .OrderBy(row => row.Index)];

            if (selectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select at least one device.",
                    "Bulk Chromebook Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string? selectedOu = null;

            if (action == "Move to OU")
            {
                selectedOu =
                    _cmbBulkOrgUnit?.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(selectedOu))
                {
                    MessageBox.Show(
                        "Please select an Organizational Unit.",
                        "Bulk Chromebook Management",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }
            }

            if (action == "Update Asset ID")
            {
                foreach (DataGridViewRow row in selectedRows)
                {
                    string requestedAssetId =
                        row.Cells["RequestedAssetId"].Value?.ToString() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(requestedAssetId))
                    {
                        MessageBox.Show(
                            "Every selected device must have a Requested Asset ID.",
                            "Bulk Chromebook Management",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        return;
                    }
                }
            }

            DialogResult confirmation =
                MessageBox.Show(
                    $"You are about to perform '{action}' on {selectedRows.Count} device(s).\r\n\r\n" +
                    "The action will be performed in the order shown in the list.\r\n\r\n" +
                    "Do you want to continue?",
                    "Confirm Bulk Action",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            _bulkOperationRunning = true;

            SetControlsForBulkOperation(true);

            _progressBar.Minimum = 0;
            _progressBar.Maximum = selectedRows.Count;
            _progressBar.Value = 0;

            int successCount = 0;
            int failureCount = 0;

            try
            {
                for (int i = 0; i < selectedRows.Count; i++)
                {
                    DataGridViewRow row = selectedRows[i];

                    string serialNumber =
                        row.Cells["SerialNumber"].Value?.ToString() ?? string.Empty;

                    BulkChromebookItem? device =
                        _devices.FirstOrDefault(
                            item => string.Equals(
                                item.SerialNumber,
                                serialNumber,
                                StringComparison.OrdinalIgnoreCase));

                    if (device == null)
                    {
                        failureCount++;

                        row.Cells["Result"].Value = "FAILED";
                        row.Cells["Error"].Value = "Device could not be found in the imported list.";

                        _progressBar.Value = i + 1;

                        continue;
                    }

                    device.Status = "PROCESSING";
                    device.Result = string.Empty;
                    device.Error = string.Empty;

                    RefreshDeviceRow(device);

                    try
                    {
                        switch (action)
                        {
                            case "Disable Chromebook":
                                {
                                    var result =
                                        await _gamService.DisableChromebookAsync(
                                            device.SerialNumber);

                                    if (result.Success)
                                    {
                                        device.Result = "SUCCESS";
                                        device.Status = "COMPLETED";
                                        successCount++;
                                    }
                                    else
                                    {
                                        device.Result = "FAILED";
                                        device.Status = "FAILED";
                                        device.Error =
                                            result.Error ??
                                            result.Output ??
                                            "GAM command failed.";

                                        failureCount++;
                                    }

                                    break;
                                }

                            case "Re-enable Chromebook":
                                {
                                    var result =
                                        await _gamService.ReenableChromebookAsync(
                                            device.SerialNumber);

                                    if (result.Success)
                                    {
                                        device.Result = "SUCCESS";
                                        device.Status = "COMPLETED";
                                        successCount++;
                                    }
                                    else
                                    {
                                        device.Result = "FAILED";
                                        device.Status = "FAILED";
                                        device.Error =
                                            result.Error ??
                                            result.Output ??
                                            "GAM command failed.";

                                        failureCount++;
                                    }

                                    break;
                                }

                            case "Move to OU":
                                {
                                    var result =
                                        await _gamService.MoveChromebookToOuAsync(
                                            device.SerialNumber,
                                            selectedOu!);

                                    if (result.Success)
                                    {
                                        device.Result = "SUCCESS";
                                        device.Status = "COMPLETED";
                                        successCount++;
                                    }
                                    else
                                    {
                                        device.Result = "FAILED";
                                        device.Status = "FAILED";
                                        device.Error =
                                            result.Error ??
                                            result.Output ??
                                            "GAM command failed.";

                                        failureCount++;
                                    }

                                    break;
                                }

                            case "Powerwash":
                                {
                                    var result =
                                        await _gamService.PowerwashChromebookAsync(
                                            device.SerialNumber);

                                    if (result.Success)
                                    {
                                        device.Result = "SUCCESS";
                                        device.Status = "COMPLETED";
                                        successCount++;
                                    }
                                    else
                                    {
                                        device.Result = "FAILED";
                                        device.Status = "FAILED";
                                        device.Error =
                                            result.Error ??
                                            result.Output ??
                                            "GAM command failed.";

                                        failureCount++;
                                    }

                                    break;
                                }

                            case "Clear Profiles":
                                {
                                    var result =
                                        await _gamService.ClearChromebookProfilesAsync(
                                            device.SerialNumber);

                                    if (result.Success)
                                    {
                                        device.Result = "SUCCESS";
                                        device.Status = "COMPLETED";
                                        successCount++;
                                    }
                                    else
                                    {
                                        device.Result = "FAILED";
                                        device.Status = "FAILED";
                                        device.Error =
                                            result.Error ??
                                            result.Output ??
                                            "GAM command failed.";

                                        failureCount++;
                                    }

                                    break;
                                }

                            case "Update Asset ID":
                                {
                                    string requestedAssetId =
                                        device.RequestedAssetId ?? string.Empty;

                                    var result =
                                        await _gamService.UpdateAnnotatedAssetIdAsync(
                                            device.SerialNumber,
                                            requestedAssetId);

                                    if (result.Success)
                                    {
                                        device.Result = "SUCCESS";
                                        device.Status = "COMPLETED";
                                        device.AssetId = requestedAssetId;
                                        successCount++;
                                    }
                                    else
                                    {
                                        device.Result = "FAILED";
                                        device.Status = "FAILED";
                                        device.Error =
                                            result.Error ??
                                            result.Output ??
                                            "GAM command failed.";

                                        failureCount++;
                                    }

                                    break;
                                }

                            default:
                                device.Result = "FAILED";
                                device.Status = "FAILED";
                                device.Error = "Unknown action.";
                                failureCount++;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        device.Result = "FAILED";
                        device.Status = "FAILED";
                        device.Error = ex.Message;
                        failureCount++;
                    }

                    RefreshDeviceRow(device);

                    _progressBar.Value = i + 1;

                    await Task.Yield();
                }
            }
            finally
            {
                _bulkOperationRunning = false;

                SetControlsForBulkOperation(false);

                _progressBar.Value = _progressBar.Maximum;
            }

            MessageBox.Show(
                "Bulk action completed.\r\n\r\n" +
                $"Action: {action}\r\n" +
                $"Successful: {successCount}\r\n" +
                $"Failed: {failureCount}\r\n" +
                $"Total: {selectedRows.Count}",
                "Bulk Action Complete",
                MessageBoxButtons.OK,
                failureCount == 0
                    ? MessageBoxIcon.Information
                    : MessageBoxIcon.Warning);
        }

        // =============================================================
        // UI CONTROL STATE
        // =============================================================

        private void SetControlsForBulkOperation(
            bool enabled)
        {
            if (_cmbAction == null ||
                _btnImport == null ||
                _btnTemplate == null ||
                _btnClear == null ||
                _btnSelectAll == null ||
                _btnSelectNone == null ||
                _btnPerform == null ||
                _btnExport == null ||
                _cmbBulkOrgUnit == null ||
                _btnClose == null)
            {
                return;
            }

            _cmbAction.Enabled =
                enabled;

            _cmbBulkOrgUnit.Enabled =
                enabled &&
                !_loadingOrgUnits;

            bool hasAction =
                _cmbAction.SelectedIndex >= 0;

            bool hasDevices =
                _devices.Count > 0;

            _btnImport.Enabled =
                enabled &&
                hasAction;

            _btnTemplate.Enabled =
                enabled &&
                hasAction;

            _btnClear.Enabled =
                enabled &&
                hasDevices;

            _btnSelectAll.Enabled =
                enabled &&
                hasDevices;

            _btnSelectNone.Enabled =
                enabled &&
                hasDevices;

            _btnExport.Enabled =
                enabled &&
                hasDevices;

            _btnClose.Enabled =
                enabled;

            if (!enabled)
            {
                _btnPerform.Enabled =
                    false;
            }
            else
            {
                UpdatePerformButton();
            }
        }

        // =============================================================
        // REFRESH SINGLE DEVICE ROW
        // =============================================================

        private void RefreshDeviceRow(
            BulkChromebookItem device)
        {
            if (_gridDevices == null ||
                device == null)
            {
                return;
            }

            int index =
                _devices.IndexOf(device);

            if (index < 0 ||
                index >= _gridDevices.Rows.Count)
            {
                return;
            }

            DataGridViewRow row =
                _gridDevices.Rows[index];

            row.Cells["SerialNumber"].Value =
                device.SerialNumber;

            row.Cells["Status"].Value =
                device.Status;

            row.Cells["AssetId"].Value =
                device.AssetId;

            row.Cells["RequestedAssetId"].Value =
                device.RequestedAssetId;

            row.Cells["Result"].Value =
                device.Result;

            row.Cells["Error"].Value =
                device.Error;

            _gridDevices.InvalidateRow(index);
        }

        // =============================================================
        // EXPORT RESULTS
        // =============================================================

        private void BtnExport_Click(
            object? sender,
            EventArgs e)
        {
            if (_devices.Count == 0)
            {
                MessageBox.Show(
                    "There are no devices to export.",
                    "Export Results",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using var dialog =
                new SaveFileDialog
                {
                    Title =
                        "Export Chromebook Results",

                    Filter =
                        "CSV files (*.csv)|*.csv|All files (*.*)|*.*",

                    FileName =
                        $"Chromebook-Bulk-Results-{DateTime.Now:yyyyMMdd-HHmmss}.csv",

                    DefaultExt =
                        "csv",

                    AddExtension =
                        true
                };

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            try
            {
                using var writer =
                    new StreamWriter(
                        dialog.FileName);

                writer.WriteLine(
                    "SerialNumber,Status,CurrentAssetId,NewAssetId,Result,Error");

                foreach (BulkChromebookItem device
                    in _devices)
                {
                    writer.WriteLine(
                        $"{CsvEscape(device.SerialNumber)}," +
                        $"{CsvEscape(device.Status)}," +
                        $"{CsvEscape(device.AssetId)}," +
                        $"{CsvEscape(device.RequestedAssetId)}," +
                        $"{CsvEscape(device.Result)}," +
                        $"{CsvEscape(device.Error)}");
                }

                MessageBox.Show(
                    "Results exported successfully.\r\n\r\n" +
                    $"Devices: {_devices.Count}\r\n" +
                    $"File:\r\n{dialog.FileName}",
                    "Export Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to export results.\r\n\r\n" +
                    ex.Message,
                    "Export Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string CsvEscape(
            string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(',') ||
                value.Contains('"') ||
                value.Contains('\r') ||
                value.Contains('\n'))
            {
                return "\"" +
                       value.Replace(
                           "\"",
                           "\"\"") +
                       "\"";
            }

            return value;
        }

        // =============================================================
        // FORM CLOSING
        // =============================================================

        protected override void OnFormClosing(
            FormClosingEventArgs e)
        {
            if (_bulkOperationRunning)
            {
                e.Cancel = true;

                MessageBox.Show(
                    "A bulk action is currently running.\r\n\r\n" +
                    "Please wait for the operation to complete before closing.",
                    "Bulk Action Running",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            base.OnFormClosing(e);
        }
    }

    // =============================================================
    // BULK DEVICE MODEL
    // =============================================================

    public class BulkChromebookItem
    {
        public string SerialNumber { get; set; } =
            string.Empty;

        public string Status { get; set; } =
            string.Empty;

        public string AssetId { get; set; } =
            string.Empty;

        public string RequestedAssetId { get; set; } =
            string.Empty;

        public string Result { get; set; } =
            string.Empty;

        public string Error { get; set; } =
            string.Empty;
    }
}
