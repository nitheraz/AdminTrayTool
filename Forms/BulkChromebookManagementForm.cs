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
        private Button _btnClose = null!;


        private DataGridView _gridDevices = null!;
        private Label _lblStatus = null!;
        private Label _lblSelected = null!;

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
                new Size(900, 740);

            MinimumSize =
                new Size(900, 740);

            MaximumSize =
                new Size(900, 740);

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
            // BOTTOM ACTIONS
            // =========================================================

            _btnValidate = new HudButton
            {
                Text = "VALIDATE DEVICES",
                Location = new Point(30, 645),
                Size = new Size(180, 40),
                Enabled = false
            };

            _btnValidate.Click +=
                async (s, e) =>
                {
                    await ValidateDevicesAsync();
                };

            mainPanel.Controls.Add(
                _btnValidate);

            _btnClose = new HudButton
            {
                Text = "CLOSE",
                Location = new Point(700, 645),
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
                        AssetId = string.Empty
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
    }
}