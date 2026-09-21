using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdminTrayTool.Services;

namespace AdminTrayTool
{
    public class BulkChromebookManagementForm : Form
    {
        private readonly GamService _gamService;

        private Button _btnImport = null!;
        private Button _btnTemplate = null!;
        private Button _btnClear = null!;
        private Button _btnSelectAll = null!;
        private Button _btnSelectNone = null!;
        private Button _btnSelectFound = null!;
        private Button _btnValidate = null!;
        private Button _btnReview = null!;
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

        private readonly List<BulkChromebookItem> _devices = new();

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
                BackColor = Color.FromArgb(
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
                ForeColor = Color.FromArgb(
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
                BackColor = Color.FromArgb(
                    45,
                    55,
                    70)
            };

            mainPanel.Controls.Add(headerLine);

            // =========================================================
            // IMPORT PANEL
            // =========================================================

            var importPanel = new HudPanel
            {
                Location = new Point(30, 105),
                Size = new Size(820, 75)
            };

            mainPanel.Controls.Add(importPanel);

            _btnImport = new HudButton
            {
                Text = "IMPORT CSV",
                Location = new Point(20, 20),
                Size = new Size(150, 35)
            };

            _btnImport.Click +=
                BtnImport_Click;

            importPanel.Controls.Add(
                _btnImport);


            _btnTemplate = new HudButton
            {
                Text = "IMPORT TEMPLATE",
                Location = new Point(185, 20),
                Size = new Size(150, 35)
            };

            _btnTemplate.Click +=
                BtnTemplate_Click;

            importPanel.Controls.Add(
                _btnTemplate);


            _btnClear = new HudButton
            {
                Text = "CLEAR LIST",
                Location = new Point(350, 20),
                Size = new Size(150, 35),
                Enabled = false
            };

            _btnClear.Click +=
                BtnClear_Click;

            importPanel.Controls.Add(
                _btnClear);


            _lblStatus = new Label
            {
                Text = "No devices loaded.",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9F),
                ForeColor = Color.FromArgb(
                    150,
                    160,
                    175),
                Location = new Point(520, 29)
            };

            importPanel.Controls.Add(
                _lblStatus);

            // =========================================================
            // DEVICE LIST
            // =========================================================

            var listPanel = new HudPanel
            {
                Location = new Point(30, 195),
                Size = new Size(820, 340)
            };

            mainPanel.Controls.Add(listPanel);

            _gridDevices = new DataGridView
            {
                Location = new Point(15, 15),
                Size = new Size(790, 270),

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
                (s, e) =>
                {
                    UpdateSelectionStatus();
                    UpdateReviewButton();
                };
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
                            9F,
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
                            9F)
                };

            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "SerialNumber",
                    HeaderText = "SERIAL NUMBER",
                    DataPropertyName = "SerialNumber"
                });

            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "STATUS",
                    DataPropertyName = "Status"
                });

            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "AssetId",
                    HeaderText = "ASSET ID",
                    DataPropertyName = "AssetId"
                });
            
            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Result",
                    HeaderText = "RESULT",
                    DataPropertyName = "Result"
                });

            _gridDevices.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Error",
                    HeaderText = "ERROR",
                    DataPropertyName = "Error"
                });

            _gridDevices.DataSource =
                _devices;

            listPanel.Controls.Add(
                _gridDevices);

            // =========================================================
            // SELECTION CONTROLS
            // =========================================================

            _btnSelectAll = new HudButton
            {
                Text = "SELECT ALL",
                Location = new Point(15, 295),
                Size = new Size(125, 32),
                Enabled = false
            };

            _btnSelectAll.Click +=
                (s, e) =>
                {
                    SelectDevices(
                        device => true);
                };

            listPanel.Controls.Add(
                _btnSelectAll);


            _btnSelectFound = new HudButton
            {
                Text = "SELECT FOUND",
                Location = new Point(150, 295),
                Size = new Size(135, 32),
                Enabled = false
            };

            _btnSelectFound.Click +=
                (s, e) =>
                {
                    SelectDevices(
                        device =>
                            device.Status.Equals(
                                "FOUND",
                                StringComparison.OrdinalIgnoreCase));
                };

            listPanel.Controls.Add(
                _btnSelectFound);


            _btnSelectNone = new HudButton
            {
                Text = "SELECT NONE",
                Location = new Point(295, 295),
                Size = new Size(125, 32),
                Enabled = false
            };

            _btnSelectNone.Click +=
                (s, e) =>
                {
                    _gridDevices.ClearSelection();
                    UpdateSelectionStatus();
                };

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
                ForeColor = Color.FromArgb(
                    150,
                    160,
                    175),
                Location = new Point(440, 304)
            };

            listPanel.Controls.Add(
                _lblSelected);

            // =========================================================
            // BULK ACTION PANEL
            // =========================================================

            var actionPanel = new HudPanel
            {
                Location = new Point(30, 545),
                Size = new Size(820, 100)
            };

            mainPanel.Controls.Add(
                actionPanel);

            _lblAction = new Label
            {
                Text = "ACTION",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(
                    120,
                    135,
                    150),
                Location = new Point(20, 15)
            };

            _cmbAction = new ComboBox
            {
                Location = new Point(20, 40),
                Size = new Size(300, 32),

                BackColor = Color.FromArgb(
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

            _cmbAction.SelectedIndexChanged +=
                CmbAction_SelectedIndexChanged;

            actionPanel.Controls.Add(
                _cmbAction);

            _cmbAction.SelectedIndex = -1;

            actionPanel.Controls.Add(
                _lblAction);

            _lblBulkOrgUnit = new Label
            {
                Text = "ORGANISATIONAL UNIT",
                AutoSize = true,
                Visible = false,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(
                    120,
                    135,
                    150),
                Location = new Point(350, 15)
            };

            actionPanel.Controls.Add(
                _lblBulkOrgUnit);

            _cmbBulkOrgUnit = new ComboBox
            {
                Location = new Point(350, 40),
                Size = new Size(440, 32),

                BackColor = Color.FromArgb(
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

            actionPanel.Controls.Add(_cmbBulkOrgUnit);

            _cmbBulkOrgUnit.SelectedIndexChanged +=
                (s, e) =>
                {
                    UpdateReviewButton();
                };

            // =============================================================
            // PROGRESS
            // =============================================================

            _progressBar = new ProgressBar
            {
                Location = new Point(30, 655),
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
                ForeColor = Color.FromArgb(
                    150,
                    160,
                    175),
                Location = new Point(545, 655),
                Visible = false
            };

            mainPanel.Controls.Add(
                _lblProgress);

            // =========================================================
            // BOTTOM ACTIONS
            // =========================================================

            _btnValidate = new HudButton
            {
                Text = "VALIDATE DEVICES",
                Location = new Point(30, 690),
                Size = new Size(180, 40),
                Enabled = false
            };

            _btnReview = new HudButton
            {
                Text = "REVIEW ACTION",
                Location = new Point(225, 690),
                Size = new Size(180, 40),
                Enabled = false
            };

            _btnReview.Click +=
                BtnReview_Click;

            mainPanel.Controls.Add(
                _btnReview);

            _btnValidate.Click +=
                async (s, e) =>
                {
                    await ValidateDevicesAsync();
                };

            mainPanel.Controls.Add(
                _btnValidate);

            _btnExport = new HudButton
            {
                Text = "EXPORT RESULTS",
                Location = new Point(420, 690),
                Size = new Size(180, 40),
                Enabled = false
            };

            _btnExport.Click += BtnExport_Click;

            mainPanel.Controls.Add(
                _btnExport);


            _btnClose = new HudButton
            {
                Text = "CLOSE",
                Location = new Point(615, 690),
                Size = new Size(150, 40)
            };

            _btnClose.Click +=
                (s, e) =>
                {
                    Close();
                };

            mainPanel.Controls.Add(
                _btnClose);
        }

        private void UpdateReviewButton()
        {
            bool hasSelection =
                _gridDevices.SelectedRows.Count > 0;

            bool validSelection =
                _gridDevices.SelectedRows
                    .Cast<DataGridViewRow>()
                    .All(row =>
                    {
                        if (row.DataBoundItem
                            is not BulkChromebookItem device)
                        {
                            return false;
                        }

                        return device.Status.Equals(
                            "FOUND",
                            StringComparison.OrdinalIgnoreCase);
                    });

            bool hasAction =
                _cmbAction.SelectedIndex >= 0;

            bool moveToOu =
                _cmbAction.SelectedItem as string ==
                "Move to OU";

            bool hasOu =
                _cmbBulkOrgUnit.SelectedIndex >= 0 &&
                !string.IsNullOrWhiteSpace(
                    _cmbBulkOrgUnit.SelectedItem as string);

            _btnReview.Enabled =
                hasSelection &&
                validSelection &&
                hasAction &&
                (!moveToOu || hasOu);
        }

        // =============================================================
        // IMPORT CSV
        // =============================================================

        private void BtnImport_Click(
            object? sender,
            EventArgs e)
        {
            using var dialog =
                new OpenFileDialog
                {
                    Title =
                        "Import Chromebook Serial Numbers",

                    Filter =
                        "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*",

                    Multiselect = false
                };

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            try
            {
                ImportSerialNumbers(
                    dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Import Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private async void CmbAction_SelectedIndexChanged(
                object? sender,
                EventArgs e)
        {
            bool moveToOu =
                _cmbAction.SelectedItem as string ==
                "Move to OU";

            _lblBulkOrgUnit.Visible =
                moveToOu;

            _cmbBulkOrgUnit.Visible =
                moveToOu;

            if (!moveToOu)
            {
                _cmbBulkOrgUnit.Items.Clear();
                _cmbBulkOrgUnit.SelectedIndex = -1;
                return;
            }

            if (_loadingOrgUnits)
                return;

            await LoadBulkOrgUnitsAsync();

            UpdateReviewButton();
        }

        private async Task LoadBulkOrgUnitsAsync()
        {
            _loadingOrgUnits = true;

            try
            {
                _cmbBulkOrgUnit.Enabled = false;

                _cmbBulkOrgUnit.Items.Clear();

                _cmbBulkOrgUnit.Items.Add(
                    "Loading organisational units...");

                _cmbBulkOrgUnit.SelectedIndex = 0;

                _lblStatus.Text =
                    "Loading organisational units...";

                var result =
                    await _gamService
                        .GetAllOrgUnitPathsAsync();

                _cmbBulkOrgUnit.Items.Clear();

                if (!result.Success ||
                    result.OrgUnitPaths == null ||
                    result.OrgUnitPaths.Count == 0)
                {
                    string error =
                        string.IsNullOrWhiteSpace(result.Error)
                            ? "No organisational units were found."
                            : result.Error;

                    MessageBox.Show(
                        error,
                        "Move to OU",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    _lblStatus.Text =
                        "No organisational units found.";

                    return;
                }

                foreach (string orgUnitPath in
                    result.OrgUnitPaths
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x))
                        .Distinct(
                            StringComparer.OrdinalIgnoreCase)
                        .OrderBy(x => x))
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
                _cmbBulkOrgUnit.Enabled = true;
                _loadingOrgUnits = false;
            }
        }
        private void BtnTemplate_Click(
            object? sender,
            EventArgs e)
        {
            using var dialog =
                new SaveFileDialog
                {
                    Title =
                        "Save Chromebook Import Template",

                    Filter =
                        "CSV files (*.csv)|*.csv",

                    FileName =
                        "ChromebookImportTemplate.csv",

                    OverwritePrompt = true
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
                    "serialNumber" +
                    Environment.NewLine);

                MessageBox.Show(
                    "Import template created successfully.",
                    "Template",
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
        private void ImportSerialNumbers(
            string filePath)
        {
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

            // -------------------------------------------------------------
            // Find the serialNumber column from the CSV header.
            // This supports Google Admin exports where serialNumber is
            // one of many columns.
            // -------------------------------------------------------------

            string headerLine =
                lines[0]
                    .Trim()
                    .TrimStart('\uFEFF');

            string[] headers =
                headerLine
                    .Split(',')
                    .Select(
                        x => x
                            .Trim()
                            .Trim('"'))
                    .ToArray();

            int serialColumnIndex =
                Array.FindIndex(
                    headers,
                    x => string.Equals(
                        x,
                        "serialNumber",
                        StringComparison.OrdinalIgnoreCase));

            if (serialColumnIndex < 0)
            {
                MessageBox.Show(
                    "The selected CSV does not contain a " +
                    "'serialNumber' column.\r\n\r\n" +
                    "Please export the Chromebook list from " +
                    "Google Admin or use the Import Template.",
                    "Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            _devices.Clear();

            int importedCount = 0;
            int duplicateCount = 0;

            // -------------------------------------------------------------
            // Read each CSV row and extract only serialNumber.
            // -------------------------------------------------------------

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

                if (serialColumnIndex >= columns.Length)
                    continue;

                string serial =
                    columns[serialColumnIndex]
                        .Trim()
                        .Trim('"');

                if (string.IsNullOrWhiteSpace(serial))
                    continue;

                // Ignore duplicate serial numbers.
                if (_devices.Any(
                    x => string.Equals(
                        x.SerialNumber,
                        serial,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    duplicateCount++;
                    continue;
                }

                _devices.Add(
                    new BulkChromebookItem
                    {
                        SerialNumber = serial,
                        Status = "NOT VALIDATED",
                        AssetId = string.Empty,
                        Result = string.Empty,
                        Error = string.Empty
                    });

                importedCount++;
            }

            _gridDevices.DataSource = null;
            _gridDevices.DataSource = _devices;

            _lblStatus.Text =
                $"{importedCount} device(s) loaded.";

            if (duplicateCount > 0)
            {
                _lblStatus.Text +=
                    $" {duplicateCount} duplicate(s) skipped.";
            }

            _btnClear.Enabled =
                _devices.Count > 0;

            _btnValidate.Enabled =
                _devices.Count > 0;

            _btnSelectAll.Enabled =
                _devices.Count > 0;

            _btnSelectNone.Enabled =
                _devices.Count > 0;

            _btnSelectFound.Enabled =
                _devices.Count > 0;

            _btnExport.Enabled =
                _devices.Count > 0;
        }

        private void SelectDevices(
            Func<BulkChromebookItem, bool> predicate)
        {
            _gridDevices.ClearSelection();

            for (int i = 0;
                 i < _devices.Count;
                 i++)
            {
                if (predicate(_devices[i]))
                {
                    _gridDevices.Rows[i]
                        .Selected = true;
                }
            }

            UpdateSelectionStatus();
        }

        private void UpdateSelectionStatus()
        {
            _lblSelected.Text =
                $"SELECTED: {_gridDevices.SelectedRows.Count}";
        }
        // =============================================================
        // CLEAR
        // =============================================================

        private void BtnClear_Click(
            object? sender,
            EventArgs e)
        {
            _devices.Clear();

            _gridDevices.DataSource = null;
            _gridDevices.DataSource = _devices;

            _lblStatus.Text =
                "No devices loaded.";

            _btnSelectAll.Enabled = false;
            _btnSelectNone.Enabled = false;
            _btnSelectFound.Enabled = false;
            _btnExport.Enabled = false;

            _gridDevices.ClearSelection();

            UpdateSelectionStatus();
        }

        // =============================================================
        // VALIDATE
        // =============================================================

        private async Task ValidateDevicesAsync()
        {
            if (_devices.Count == 0)
                return;

            _btnImport.Enabled = false;
            _btnClear.Enabled = false;
            _btnValidate.Enabled = false;

            try
            {
                for (int i = 0;
                     i < _devices.Count;
                     i++)
                {
                    BulkChromebookItem device =
                        _devices[i];

                    _lblStatus.Text =
                        $"Validating {i + 1} of {_devices.Count}: " +
                        device.SerialNumber;

                    device.Status =
                        "CHECKING...";

                    device.Result =
                        string.Empty;

                    RefreshGrid();

                    try
                    {
                        var result =
                            await _gamService
                                .GetChromebookInfoAsync(
                                    device.SerialNumber);

                        if (result.Success &&
                            result.Device != null)
                        {
                            device.Status = "FOUND";

                            device.AssetId =
                                result.Device.AssetId
                                ?? string.Empty;
                        }
                        else
                        {
                            device.Status = "NOT FOUND";
                            device.AssetId = string.Empty;
                        }
                    }
                    catch
                    {
                        device.Status = "ERROR";
                        device.AssetId = string.Empty;
                    }

                    RefreshGrid();
                }

                _lblStatus.Text =
                    $"Validation complete: {_devices.Count} device(s).";
            }
            finally
            {
                _btnImport.Enabled = true;
                _btnClear.Enabled =
                    _devices.Count > 0;
                _btnValidate.Enabled =
                    _devices.Count > 0;
            }
        }

        private void RefreshGrid()
        {
            _gridDevices.DataSource = null;
            _gridDevices.DataSource = _devices;
            _gridDevices.Refresh();

            UpdateReviewButton();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
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

            using var dialog = new SaveFileDialog
            {
                Title = "Export Chromebook Results",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"Chromebook-Bulk-Results-{DateTime.Now:yyyyMMdd-HHmmss}.csv",
                DefaultExt = "csv",
                AddExtension = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                using var writer = new StreamWriter(dialog.FileName);

                writer.WriteLine(
                    "SerialNumber,Status,AssetId,Result,Error");

                foreach (BulkChromebookItem device in _devices)
                {
                    writer.WriteLine(
                        $"{CsvEscape(device.SerialNumber)}," +
                        $"{CsvEscape(device.Status)}," +
                        $"{CsvEscape(device.AssetId)}," +
                        $"{CsvEscape(device.Result)}," +
                        $"{CsvEscape(device.Error)}");
                }

                MessageBox.Show(
                    $"Results exported successfully.\r\n\r\n" +
                    $"Devices: {_devices.Count}\r\n" +
                    $"File:\r\n{dialog.FileName}",
                    "Export Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to export results.\r\n\r\n{ex.Message}",
                    "Export Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private static string CsvEscape(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(',') ||
                value.Contains('"') ||
                value.Contains('\r') ||
                value.Contains('\n'))
            {
                return "\"" +
                       value.Replace("\"", "\"\"") +
                       "\"";
            }

            return value;
        }

        // =============================================================
        // REVIEW ACTION
        // =============================================================

        private async void BtnReview_Click(
            object? sender,
            EventArgs e)
        {
            var selectedDevices =
                _gridDevices.SelectedRows
                    .Cast<DataGridViewRow>()
                    .Select(row => row.DataBoundItem as BulkChromebookItem)
                    .Where(device => device != null)
                    .Cast<BulkChromebookItem>()
                    .ToList();

            if (selectedDevices.Count == 0)
            {
                MessageBox.Show(
                    "Please select at least one Chromebook.",
                    "Review Action",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string action =
                _cmbAction.SelectedItem as string
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(action))
            {
                MessageBox.Show(
                    "Please select an action.",
                    "Review Action",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string orgUnit =
                _cmbBulkOrgUnit.SelectedItem as string
                ?? string.Empty;

            string deviceList =
                string.Join(
                    Environment.NewLine,
                    selectedDevices.Select(
                        device => $"• {device.SerialNumber}"));

            string message =
                $"ACTION\r\n" +
                $"{action}\r\n\r\n" +
                $"DEVICES ({selectedDevices.Count})\r\n" +
                $"{deviceList}";

            if (action == "Move to OU")
            {
                message +=
                    $"\r\n\r\nTARGET ORGANISATIONAL UNIT\r\n" +
                    $"{orgUnit}";
            }

            if (action == "Powerwash")
            {
                message +=
                    "\r\n\r\nWARNING\r\n" +
                    "Powerwash will remotely reset the selected Chromebooks.";
            }
            else if (action == "Clear Profiles")
            {
                message +=
                    "\r\n\r\nWARNING\r\n" +
                    "Clear Profiles will remove user profiles from the selected Chromebooks.";
            }

            DialogResult result =
                MessageBox.Show(
                    message,
                    "Review Bulk Action",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

            if (result != DialogResult.OK)
                return;

            await ExecuteBulkActionAsync(
                selectedDevices,
                action,
                orgUnit);
        }

        private async Task ExecuteBulkActionAsync(
            List<BulkChromebookItem> devices,
            string action,
            string orgUnit)
        {
            int successCount = 0;
            int failedCount = 0;

            _btnImport.Enabled = false;
            _btnTemplate.Enabled = false;
            _btnClear.Enabled = false;
            _btnSelectAll.Enabled = false;
            _btnSelectNone.Enabled = false;
            _btnSelectFound.Enabled = false;
            _btnValidate.Enabled = false;
            _btnReview.Enabled = false;
            _cmbAction.Enabled = false;
            _cmbBulkOrgUnit.Enabled = false;
            _btnClose.Enabled = false;

            _progressBar.Visible = true;
            _lblProgress.Visible = true;

            _progressBar.Minimum = 0;
            _progressBar.Maximum = devices.Count;
            _progressBar.Value = 0;

            try
            {
                for (int i = 0; i < devices.Count; i++)
                {
                    BulkChromebookItem device =
                        devices[i];

                    _lblProgress.Text =
                        $"PROCESSING {i + 1} OF {devices.Count}";

                    _lblStatus.Text =
                        $"Processing {i + 1} of {devices.Count}: " +
                        device.SerialNumber;

                    device.Result =
                        "PROCESSING...";
                    
                    device.Error =
                        string.Empty;

                    RefreshGrid();

                    try
                    {
                        bool success = false;

                        switch (action)
                        {
                            case "Disable Chromebook":
                                {
                                    var result =
                                        await _gamService
                                            .DisableChromebookAsync(
                                                device.SerialNumber);

                                    success = result.Success;

                                    if (!success)
                                        device.Error = result.Error ?? result.Output ?? "GAM command failed.";

                                    break;
                                }

                            case "Re-enable Chromebook":
                                {
                                    var result =
                                        await _gamService
                                            .ReenableChromebookAsync(
                                                device.SerialNumber);

                                    success = result.Success;

                                    if (!success)
                                        device.Error = result.Error ?? result.Output ?? "GAM command failed.";

                                    break;
                                }

                            case "Move to OU":
                                {
                                    var result =
                                        await _gamService
                                            .MoveChromebookToOuAsync(
                                                device.SerialNumber,
                                                orgUnit);

                                    success = result.Success;

                                    if (!success)
                                        device.Error = result.Error ?? result.Output ?? "GAM command failed.";

                                    break;
                                }

                            case "Powerwash":
                                {
                                    var result =
                                        await _gamService
                                            .PowerwashChromebookAsync(
                                                device.SerialNumber);

                                    success = result.Success;

                                    if (!success)
                                        device.Error = result.Error ?? result.Output ?? "GAM command failed.";

                                    break;
                                }

                            case "Clear Profiles":
                                {
                                    var result =
                                        await _gamService
                                            .ClearChromebookProfilesAsync(
                                                device.SerialNumber);

                                    success = result.Success;

                                    if (!success)
                                        device.Error = result.Error ?? result.Output ?? "GAM command failed.";

                                    break;
                                }
                        }

                        if (success)
                        {
                            device.Result = "SUCCESS";
                            successCount++;
                        }
                        else
                        {
                            device.Result = "FAILED";
                            failedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        device.Result = "FAILED";
                        device.Error = ex.Message;
                        failedCount++;
                    }

                    _progressBar.Value =
                        i + 1;

                    RefreshGrid();
                }

                _lblProgress.Text =
                    "COMPLETE";

                _lblStatus.Text =
                    $"Bulk action complete: " +
                    $"{successCount} successful, " +
                    $"{failedCount} failed.";

                MessageBox.Show(
                    $"Bulk action complete.\r\n\r\n" +
                    $"Action: {action}\r\n" +
                    $"Successful: {successCount}\r\n" +
                    $"Failed: {failedCount}",
                    "Bulk Action Complete",
                    MessageBoxButtons.OK,
                    failedCount == 0
                        ? MessageBoxIcon.Information
                        : MessageBoxIcon.Warning);
            }
            finally
            {
                _btnImport.Enabled = true;
                _btnTemplate.Enabled = true;
                _btnClear.Enabled =
                    _devices.Count > 0;

                _btnSelectAll.Enabled =
                    _devices.Count > 0;

                _btnSelectNone.Enabled =
                    _devices.Count > 0;

                _btnSelectFound.Enabled =
                    _devices.Count > 0;

                _btnValidate.Enabled =
                    _devices.Count > 0;

                _cmbAction.Enabled = true;
                _cmbBulkOrgUnit.Enabled = true;
                _btnClose.Enabled = true;

                UpdateReviewButton();
            }
        }
    }
    
    // =============================================================
    // BULK DEVICE MODEL
    // =============================================================

    public class BulkChromebookItem
    {
        public string SerialNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string AssetId { get; set; } = string.Empty;

        public string Result { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;
    }
}