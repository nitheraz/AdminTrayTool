using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdminTrayTool
{
    public class ChromebookManagementForm : Form
    {
        private readonly GamService _gamService;

        private TextBox _txtSerial = null!;
        private TextBox _txtAssetId = null!;

        private Label _lblSerial = null!;
        private Label _lblDeviceId = null!;
        private Label _lblModel = null!;
        private Label _lblMac = null!;
        private Label _lblOrgUnit = null!;
        private Label _lblLastSync = null!;

        private Button _btnLookup = null!;
        private Button _btnRefresh = null!;
        private Button _btnDisable = null!;
        private Button _btnReenable = null!;
        private Button _btnCopyMac = null!;
        private Button _btnSaveAssetId = null!;

        private Label _lblDeviceStatus = null!;
        private TextBox _txtLog = null!;

        private ChromebookInfo? _currentDevice;

        public ChromebookManagementForm()
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
            Text = "Chromebook Management";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(900, 750);

            MinimumSize =
                new Size(820, 680);

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
                    "CHROMEBOOK MANAGEMENT",

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
                    "Manage Google Workspace ChromeOS devices",

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
            // LOOKUP SECTION
            // =========================================================

            var lookupPanel = CreatePanel(
                new Point(30, 115),
                new Size(820, 105));

            mainPanel.Controls.Add(
                lookupPanel);

            var lblSerial = CreateLabel(
                "SERIAL NUMBER",
                new Point(20, 18));

            lookupPanel.Controls.Add(
                lblSerial);

            _txtSerial = new TextBox
            {
                Location =
                    new Point(
                        20,
                        45),

                Size =
                    new Size(
                        570,
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
                        11F)
            };

            _txtSerial.KeyDown +=
                TxtSerial_KeyDown;

            lookupPanel.Controls.Add(
                _txtSerial);

            _btnLookup = CreateButton(
                "LOOK UP",
                new Point(610, 43),
                new Size(180, 36));

            _btnLookup.Click +=
                async (s, e) =>
                {
                    await LookupChromebookAsync();
                };

            lookupPanel.Controls.Add(
                _btnLookup);

            // =========================================================
            // DEVICE INFORMATION
            // =========================================================

            var infoPanel = CreatePanel(
                new Point(30, 240),
                new Size(820, 300));

            mainPanel.Controls.Add(
                infoPanel);

            var lblInfoTitle = new Label
            {
                Text =
                    "DEVICE INFORMATION",

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

            // =========================================================
            // DEVICE STATUS
            // =========================================================

            _lblDeviceStatus = new Label
            {
                Text =
                    "NO DEVICE LOADED",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold),

                ForeColor =
                    Color.FromArgb(
                        140,
                        150,
                        165),

                Location =
                    new Point(
                        600,
                        20)
            };

            infoPanel.Controls.Add(
                _lblDeviceStatus);

            // =========================================================
            // SERIAL
            // =========================================================

            _lblSerial = AddInfoRow(
                infoPanel,
                "Serial Number",
                55,
                out _,
                "—");

            // =========================================================
            // ASSET ID
            // =========================================================

            var lblAsset = CreateLabel(
                "ASSET ID",
                new Point(20, 92));

            infoPanel.Controls.Add(
                lblAsset);

            _txtAssetId = new TextBox
            {
                Location =
                    new Point(
                        170,
                        89),

                Size =
                    new Size(
                        250,
                        28),

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

                ReadOnly = true
            };

            infoPanel.Controls.Add(
                _txtAssetId);

            _btnSaveAssetId = CreateButton(
                "SAVE",
                new Point(430, 88),
                new Size(90, 30));

            _btnSaveAssetId.Enabled = false;

            _btnSaveAssetId.Click +=
                async (s, e) =>
                {
                    await SaveAssetIdAsync();
                };

            infoPanel.Controls.Add(
                _btnSaveAssetId);

            // =========================================================
            // GOOGLE DEVICE ID
            // =========================================================

            _lblDeviceId = AddInfoRow(
                infoPanel,
                "Google Device ID",
                130,
                out _,
                "—");

            // =========================================================
            // MODEL
            // =========================================================

            var lblModelName = CreateLabel(
                "MODEL",
                new Point(20, 165));

            infoPanel.Controls.Add(
                lblModelName);

            _lblModel = new Label
            {
                Text = "—",

                Location =
                    new Point(
                        170,
                        164),

                AutoSize = false,

                Size =
                    new Size(
                        620,
                        48),

                Font =
                    new Font(
                        "Segoe UI",
                        9.5F),

                ForeColor =
                    Color.White
            };

            infoPanel.Controls.Add(
                _lblModel);

            // =========================================================
            // WI-FI MAC
            // =========================================================

            var lblMacName = CreateLabel(
                "WI-FI MAC",
                new Point(20, 220));

            infoPanel.Controls.Add(
                lblMacName);

            _lblMac = new Label
            {
                Text = "—",

                Location =
                    new Point(
                        170,
                        219),

                AutoSize = true,

                Font =
                    new Font(
                        "Consolas",
                        10F,
                        FontStyle.Bold),

                ForeColor =
                    Color.White
            };

            infoPanel.Controls.Add(
                _lblMac);

            _btnCopyMac = CreateButton(
                "COPY",
                new Point(420, 214),
                new Size(85, 30));

            _btnCopyMac.Enabled = false;

            _btnCopyMac.Click +=
                BtnCopyMac_Click;

            infoPanel.Controls.Add(
                _btnCopyMac);

            // =========================================================
            // ORGANISATION UNIT
            // =========================================================

            _lblOrgUnit = AddInfoRow(
                infoPanel,
                "Organisation Unit",
                260,
                out _,
                "—");

            // =========================================================
            // LAST SYNC
            // =========================================================

            _lblLastSync = AddInfoRow(
                infoPanel,
                "Last Sync",
                260,
                out _,
                "—");

            _lblLastSync.Location =
                new Point(
                    570,
                    260);

            // =========================================================
            // ACTION PANEL
            // =========================================================

            var actionPanel = CreatePanel(
                new Point(30, 555),
                new Size(820, 80));

            mainPanel.Controls.Add(
                actionPanel);

            // =========================================================
            // REFRESH BUTTON
            // =========================================================

            _btnRefresh = CreateButton(
                "REFRESH",
                new Point(20, 20),
                new Size(150, 40));

            _btnRefresh.Enabled = false;

            _btnRefresh.Click +=
                async (s, e) =>
                {
                    await RefreshChromebookAsync();
                };

            actionPanel.Controls.Add(
                _btnRefresh);

            // =========================================================
            // DISABLE BUTTON
            // =========================================================

            _btnDisable = CreateButton(
                "DISABLE CHROMEBOOK",
                new Point(190, 20),
                new Size(200, 40));

            _btnDisable.Enabled = false;

            _btnDisable.Click +=
                async (s, e) =>
                {
                    await DisableChromebookAsync();
                };

            actionPanel.Controls.Add(
                _btnDisable);

            // =========================================================
            // RE-ENABLE BUTTON
            // =========================================================

            _btnReenable = CreateButton(
                "RE-ENABLE CHROMEBOOK",
                new Point(410, 20),
                new Size(200, 40));

            _btnReenable.Enabled = false;

            _btnReenable.Click +=
                async (s, e) =>
                {
                    await ReenableChromebookAsync();
                };

            actionPanel.Controls.Add(
                _btnReenable);

            // =========================================================
            // LOG
            // =========================================================

            var lblLog = new Label
            {
                Text =
                    "GAM OUTPUT",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),

                ForeColor =
                    Color.FromArgb(
                        150,
                        160,
                        175),

                Location =
                    new Point(
                        30,
                        650)
            };

            mainPanel.Controls.Add(
                lblLog);

            _txtLog = new TextBox
            {
                Location =
                    new Point(
                        30,
                        675),

                Size =
                    new Size(
                        820,
                        45),

                Multiline = true,

                ReadOnly = true,

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

                ScrollBars =
                    ScrollBars.Vertical,

                Font =
                    new Font(
                        "Consolas",
                        8.5F)
            };

            mainPanel.Controls.Add(
                _txtLog);

            _txtSerial.Focus();
        }

        // =============================================================
        // LOOKUP
        // =============================================================

        private async Task LookupChromebookAsync()
        {
            string serial =
                _txtSerial.Text.Trim();

            if (string.IsNullOrWhiteSpace(serial))
            {
                MessageBox.Show(
                    "Please enter a Chromebook serial number.",
                    "Serial Number Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                _txtSerial.Focus();

                return;
            }

            SetBusy(true);

            ClearDeviceInformation();

            WriteLog(
                $"Looking up Chromebook: {serial}");

            try
            {
                var result =
                    await _gamService
                        .GetChromebookInfoAsync(
                            serial);

                if (!result.Success ||
                    result.Device == null)
                {
                    WriteLog(
                        "GAM ERROR: " +
                        result.Error);

                    MessageBox.Show(
                        result.Error,
                        "Chromebook Lookup Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                _currentDevice =
                    result.Device;

                DisplayDeviceInformation(
                    result.Device);

                WriteLog(
                    result.Device.RawOutput);

                _btnRefresh.Enabled = true;
                _btnDisable.Enabled = true;
                _btnReenable.Enabled = true;
            }
            catch (Exception ex)
            {
                WriteLog(
                    "ERROR: " +
                    ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // =============================================================
        // REFRESH
        // =============================================================

        private async Task RefreshChromebookAsync()
        {
            if (_currentDevice == null)
                return;

            string serial =
                _currentDevice.SerialNumber;

            if (string.IsNullOrWhiteSpace(serial))
                return;

            SetBusy(true);

            WriteLog(
                $"Refreshing Chromebook: {serial}");

            try
            {
                var result =
                    await _gamService
                        .GetChromebookInfoAsync(
                            serial);

                if (!result.Success ||
                    result.Device == null)
                {
                    WriteLog(
                        "GAM REFRESH ERROR: " +
                        result.Error);

                    MessageBox.Show(
                        result.Error,
                        "Chromebook Refresh Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                _currentDevice =
                    result.Device;

                DisplayDeviceInformation(
                    result.Device);

                WriteLog(
                    "Chromebook information refreshed.");

                WriteLog(
                    result.Device.RawOutput);
            }
            catch (Exception ex)
            {
                WriteLog(
                    "REFRESH ERROR: " +
                    ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Refresh Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // =============================================================
        // DISPLAY DEVICE INFORMATION
        // =============================================================

        private void DisplayDeviceInformation(
            ChromebookInfo device)
        {
            // ---------------------------------------------------------
            // SERIAL
            // ---------------------------------------------------------

            _lblSerial.Text =
                string.IsNullOrWhiteSpace(
                    device.SerialNumber)
                    ? "—"
                    : device.SerialNumber;

            // ---------------------------------------------------------
            // ASSET ID
            // ---------------------------------------------------------

            string assetId =
                device.AssetId?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(assetId))
            {
                _txtAssetId.Text =
                    string.Empty;

                _txtAssetId.ReadOnly =
                    false;

                _btnSaveAssetId.Enabled =
                    true;
            }
            else
            {
                _txtAssetId.Text =
                    assetId;

                _txtAssetId.ReadOnly =
                    true;

                _btnSaveAssetId.Enabled =
                    false;
            }

            // ---------------------------------------------------------
            // GOOGLE DEVICE ID
            // ---------------------------------------------------------

            _lblDeviceId.Text =
                string.IsNullOrWhiteSpace(
                    device.DeviceId)
                    ? "—"
                    : device.DeviceId;

            // ---------------------------------------------------------
            // MODEL
            // ---------------------------------------------------------

            string model =
                GetCorrectModel(device);

            _lblModel.Text =
                string.IsNullOrWhiteSpace(model)
                    ? "—"
                    : model;

            // ---------------------------------------------------------
            // MAC ADDRESS
            // ---------------------------------------------------------

            _lblMac.Text =
                string.IsNullOrWhiteSpace(
                    device.MacAddress)
                    ? "—"
                    : device.MacAddress;

            _btnCopyMac.Enabled =
                !string.IsNullOrWhiteSpace(
                    device.MacAddress);

            // ---------------------------------------------------------
            // ORGANISATION UNIT
            // ---------------------------------------------------------

            _lblOrgUnit.Text =
                string.IsNullOrWhiteSpace(
                    device.OrgUnitPath)
                    ? "—"
                    : device.OrgUnitPath;

            // ---------------------------------------------------------
            // LAST SYNC
            // ---------------------------------------------------------

            _lblLastSync.Text =
                string.IsNullOrWhiteSpace(
                    device.LastSync)
                    ? "—"
                    : device.LastSync;

            // ---------------------------------------------------------
            // STATUS
            // ---------------------------------------------------------

            _lblDeviceStatus.Text =
                string.IsNullOrWhiteSpace(
                    device.Status)
                    ? "DEVICE FOUND"
                    : device.Status.ToUpperInvariant();

            UpdateStatusAppearance(
                device.Status);
        }

        // =============================================================
        // SAVE ASSET ID
        // =============================================================

        private async Task SaveAssetIdAsync()
        {
            if (_currentDevice == null)
                return;

            string serial =
                _currentDevice.SerialNumber?.Trim()
                ?? string.Empty;

            string assetId =
                _txtAssetId.Text.Trim();

            if (string.IsNullOrWhiteSpace(serial))
            {
                MessageBox.Show(
                    "The Chromebook serial number is missing.",
                    "Asset ID Update",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(assetId))
            {
                MessageBox.Show(
                    "Please enter an Asset ID.",
                    "Asset ID Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                _txtAssetId.Focus();

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    $"Update the Chromebook Asset ID?\n\n" +
                    $"Serial Number:\n{serial}\n\n" +
                    $"New Asset ID:\n{assetId}",
                    "Confirm Asset ID",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirmation !=
                DialogResult.Yes)
            {
                return;
            }

            SetBusy(true);

            try
            {
                WriteLog(
                    $"Updating Asset ID for {serial}...");

                WriteLog(
                    $"New Asset ID: {assetId}");

                GamResult result =
                    await _gamService
                        .UpdateAnnotatedAssetIdAsync(
                            serial,
                            assetId);

                WriteLog(
                    result.CombinedOutput);

                if (!result.Success)
                {
                    MessageBox.Show(
                        result.CombinedOutput,
                        "Asset ID Update Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                MessageBox.Show(
                    $"Asset ID successfully updated to:\n\n{assetId}",
                    "Asset ID Updated",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                _txtAssetId.ReadOnly =
                    true;

                _btnSaveAssetId.Enabled =
                    false;

                // Refresh Google Admin information.
                await RefreshChromebookAsync();
            }
            catch (Exception ex)
            {
                WriteLog(
                    "ASSET ID UPDATE ERROR: " +
                    ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Asset ID Update Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // =============================================================
        // MODEL PARSING
        // =============================================================

        private static string GetCorrectModel(
            ChromebookInfo device)
        {
            string model =
                device.Model?.Trim()
                ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(model) &&
                model.Contains(
                    "Chromebook",
                    StringComparison.OrdinalIgnoreCase))
            {
                return model;
            }

            if (!string.IsNullOrWhiteSpace(
                device.RawOutput))
            {
                string[] lines =
                    device.RawOutput.Split(
                        new[]
                        {
                            '\r',
                            '\n'
                        },
                        StringSplitOptions.RemoveEmptyEntries);

                foreach (string rawLine in lines)
                {
                    string line =
                        rawLine.Trim();

                    if (!line.StartsWith(
                        "model:",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string candidate =
                        line.Substring(
                            "model:".Length)
                            .Trim();

                    if (candidate.Contains(
                        "Chromebook",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return candidate;
                    }
                }
            }

            return model;
        }

        // =============================================================
        // STATUS APPEARANCE
        // =============================================================

        private void UpdateStatusAppearance(
            string? status)
        {
            string currentStatus =
                status?.Trim()
                .ToUpperInvariant()
                ?? string.Empty;

            if (currentStatus == "ACTIVE")
            {
                _lblDeviceStatus.Text =
                    "ACTIVE";

                _lblDeviceStatus.ForeColor =
                    Color.FromArgb(
                        80,
                        220,
                        140);

                return;
            }

            if (currentStatus == "DISABLED")
            {
                _lblDeviceStatus.Text =
                    "DISABLED";

                _lblDeviceStatus.ForeColor =
                    Color.FromArgb(
                        255,
                        180,
                        70);

                return;
            }

            _lblDeviceStatus.Text =
                string.IsNullOrWhiteSpace(
                    currentStatus)
                    ? "DEVICE FOUND"
                    : currentStatus;

            _lblDeviceStatus.ForeColor =
                Color.FromArgb(
                    80,
                    220,
                    140);
        }

        // =============================================================
        // COPY MAC ADDRESS
        // =============================================================

        private void BtnCopyMac_Click(
            object? sender,
            EventArgs e)
        {
            if (_currentDevice == null)
                return;

            string mac =
                _currentDevice.MacAddress?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(mac))
            {
                MessageBox.Show(
                    "No MAC address is available.",
                    "MAC Address",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                Clipboard.SetText(mac);

                WriteLog(
                    $"MAC address copied: {mac}");

                _btnCopyMac.Text =
                    "COPIED";

                var timer =
                    new System.Windows.Forms.Timer
                    {
                        Interval = 1500
                    };

                timer.Tick +=
                    (s, args) =>
                    {
                        _btnCopyMac.Text =
                            "COPY";

                        timer.Stop();
                        timer.Dispose();
                    };

                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to copy the MAC address.\n\n" +
                    ex.Message,
                    "Copy Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =============================================================
        // DISABLE
        // =============================================================

        private async Task DisableChromebookAsync()
        {
            if (_currentDevice == null)
                return;

            string serial =
                _currentDevice.SerialNumber;

            DialogResult confirmation =
                MessageBox.Show(
                    $"Are you sure you want to DISABLE this Chromebook?\n\n" +
                    $"Serial Number:\n{serial}\n\n" +
                    $"Asset ID:\n{GetAssetDisplay()}\n\n" +
                    "The device will no longer be usable by the student " +
                    "while disabled.",
                    "Confirm Chromebook Disable",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirmation !=
                DialogResult.Yes)
            {
                return;
            }

            SetBusy(true);

            try
            {
                WriteLog(
                    $"Disabling Chromebook {serial}...");

                GamResult result =
                    await _gamService
                        .DisableChromebookAsync(
                            serial);

                WriteLog(
                    result.CombinedOutput);

                if (result.Success)
                {
                    MessageBox.Show(
                        $"Chromebook {serial} has been disabled.",
                        "Chromebook Disabled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await RefreshChromebookAsync();
                }
                else
                {
                    MessageBox.Show(
                        result.CombinedOutput,
                        "Failed to Disable Chromebook",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                SetBusy(false);
            }
        }

        // =============================================================
        // RE-ENABLE
        // =============================================================

        private async Task ReenableChromebookAsync()
        {
            if (_currentDevice == null)
                return;

            string serial =
                _currentDevice.SerialNumber;

            DialogResult confirmation =
                MessageBox.Show(
                    $"Are you sure you want to RE-ENABLE this Chromebook?\n\n" +
                    $"Serial Number:\n{serial}\n\n" +
                    $"Asset ID:\n{GetAssetDisplay()}",
                    "Confirm Chromebook Re-enable",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirmation !=
                DialogResult.Yes)
            {
                return;
            }

            SetBusy(true);

            try
            {
                WriteLog(
                    $"Re-enabling Chromebook {serial}...");

                GamResult result =
                    await _gamService
                        .ReenableChromebookAsync(
                            serial);

                WriteLog(
                    result.CombinedOutput);

                if (result.Success)
                {
                    MessageBox.Show(
                        $"Chromebook {serial} has been re-enabled.",
                        "Chromebook Re-enabled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await RefreshChromebookAsync();
                }
                else
                {
                    MessageBox.Show(
                        result.CombinedOutput,
                        "Failed to Re-enable Chromebook",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                SetBusy(false);
            }
        }

        // =============================================================
        // GET ASSET DISPLAY
        // =============================================================

        private string GetAssetDisplay()
        {
            if (_currentDevice == null)
                return "Not assigned";

            return string.IsNullOrWhiteSpace(
                _currentDevice.AssetId)
                    ? "Not assigned"
                    : _currentDevice.AssetId;
        }

        // =============================================================
        // CLEAR DEVICE INFORMATION
        // =============================================================

        private void ClearDeviceInformation()
        {
            _currentDevice = null;

            _lblSerial.Text =
                "—";

            _txtAssetId.Text =
                string.Empty;

            _txtAssetId.ReadOnly =
                true;

            _btnSaveAssetId.Enabled =
                false;

            _lblDeviceId.Text =
                "—";

            _lblModel.Text =
                "—";

            _lblMac.Text =
                "—";

            _lblOrgUnit.Text =
                "—";

            _lblLastSync.Text =
                "—";

            _btnCopyMac.Enabled =
                false;

            _btnCopyMac.Text =
                "COPY";

            _lblDeviceStatus.Text =
                "NO DEVICE LOADED";

            _lblDeviceStatus.ForeColor =
                Color.FromArgb(
                    140,
                    150,
                    165);

            _btnRefresh.Enabled =
                false;

            _btnDisable.Enabled =
                false;

            _btnReenable.Enabled =
                false;
        }

        // =============================================================
        // BUSY STATE
        // =============================================================

        private void SetBusy(bool busy)
        {
            _btnLookup.Enabled =
                !busy;

            _txtSerial.Enabled =
                !busy;

            _btnSaveAssetId.Enabled =
                !busy &&
                _currentDevice != null &&
                !_txtAssetId.ReadOnly &&
                !string.IsNullOrWhiteSpace(
                    _txtAssetId.Text);

            if (busy)
            {
                Cursor =
                    Cursors.WaitCursor;
            }
            else
            {
                Cursor =
                    Cursors.Default;
            }

            if (_currentDevice == null)
            {
                _btnRefresh.Enabled =
                    false;

                _btnDisable.Enabled =
                    false;

                _btnReenable.Enabled =
                    false;

                _btnCopyMac.Enabled =
                    false;
            }
            else
            {
                _btnRefresh.Enabled =
                    !busy;

                _btnDisable.Enabled =
                    !busy;

                _btnReenable.Enabled =
                    !busy;

                _btnCopyMac.Enabled =
                    !busy &&
                    !string.IsNullOrWhiteSpace(
                        _currentDevice.MacAddress);
            }
        }

        // =============================================================
        // WRITE LOG
        // =============================================================

        private void WriteLog(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (_txtLog.TextLength > 0)
            {
                _txtLog.AppendText(
                    Environment.NewLine +
                    Environment.NewLine);
            }

            _txtLog.AppendText(text);
        }

        // =============================================================
        // SERIAL ENTER KEY
        // =============================================================

        private void TxtSerial_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (e.KeyCode ==
                Keys.Enter)
            {
                e.SuppressKeyPress = true;

                _ = LookupChromebookAsync();
            }
        }

        // =============================================================
        // UI HELPERS
        // =============================================================

        private Panel CreatePanel(
            Point location,
            Size size)
        {
            return new Panel
            {
                Location =
                    location,

                Size =
                    size,

                BackColor =
                    Color.FromArgb(
                        16,
                        23,
                        36),

                BorderStyle =
                    BorderStyle.FixedSingle
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
                    defaultValue,

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
            var button = new Button
            {
                Text =
                    text,

                Location =
                    location,

                Size =
                    size,

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    Color.FromArgb(
                        25,
                        35,
                        52),

                ForeColor =
                    Color.White,

                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),

                Cursor =
                    Cursors.Hand
            };

            button.FlatAppearance.BorderColor =
                Color.FromArgb(
                    65,
                    85,
                    110);

            button.FlatAppearance.BorderSize =
                1;

            return button;
        }
    }
}