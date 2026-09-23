using AdminTrayTool.Models;
using AdminTrayTool.Services;
using System.Diagnostics;

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
        private Label _lblRecentUser = null!;

        private Button _btnLookup = null!;
        private Button _btnRefresh = null!;
        private Button _btnClear = null!;
        private Button _btnDisable = null!;
        private Button _btnReenable = null!;
        private Button _btnMoveOu = null!;
        private Button _btnPowerwash = null!;
        private Button _btnClearProfiles = null!;
        private Button _btnCopyMac = null!;
        private Button _btnEditAssetId = null!;
        private Button _btnSaveAssetId = null!;
        private Button _btnCancelAssetId = null!;
        private Button _btnBulkManagement = null!;

        private string _originalAssetId = string.Empty;

        private Label _lblDeviceStatus = null!;
        private TextBox _txtLog = null!;
        private ChromebookInfo? _currentDevice;
        private ComboBox _cmbSearchType = null!;

        public ChromebookManagementForm()
        {
            _gamService = new GamService();

            InitializeForm();
            BuildInterface();
            Load += ChromebookManagementForm_Load;
        }
        private async void ChromebookManagementForm_Load(object? sender, EventArgs e)
        {
            await ValidateGam7Async();
        }
        private async Task ValidateGam7Async()
        {
            var checker = new GamOAuthChecker();
            Services.GamResult result = await checker.CheckOAuthAsync();

            if (!result.Success)
            {
                if (result.IssueType == Services.GamIssueType.ProjectMissing)
                {
                    await OfferProjectSetupAsync(result);
                    return;
                }

                DialogResult answer = MessageBox.Show(
                    "GAM7 is not authenticated on this computer.\n\n" +
                    "Would you like to run the OAuth setup now?",
                    "GAM7 Authentication Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (answer == DialogResult.Yes)
                {
                    await RunOAuthSetupAsync();
                }
                else
                {
                    DisableChromebookUI();
                }

                return;
            }

            EnableChromebookUI();
        }

        private async Task OfferProjectSetupAsync(Services.GamResult result)
        {
            DialogResult answer = MessageBox.Show(
                "No GAM project was found on this computer.\n\n" +
                result.Error + "\n\n" +
                "Would you like to run 'gam create project' now? " +
                "(Choose No if you already have a project and want to run 'gam use project' instead.)",
                "GAM Project Required",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (answer == DialogResult.Cancel)
            {
                DisableChromebookUI();
                return;
            }

            string gamArgs = answer == DialogResult.Yes ? "create project" : "use project";

            string? gamPath = await GamLocator.LocateGam();
            if (gamPath == null)
            {
                MessageBox.Show("GAM executable not found.", "GAM Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisableChromebookUI();
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = gamPath,
                    Arguments = gamArgs,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                using Process? process = Process.Start(psi);
                if (process != null)
                {
                    await Task.Run(() => process.WaitForExit());
                }

                // After project setup, immediately try OAuth setup.
                await RunOAuthSetupAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while running GAM project setup.\n\n" + ex.Message,
                    "GAM Project Setup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                DisableChromebookUI();
            }
        }

        private async Task RunOAuthSetupAsync()
        {
            string? gamPath = await GamLocator.LocateGam();

            if (gamPath == null)
            {
                MessageBox.Show(
                    "GAM executable not found.",
                    "GAM Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DisableChromebookUI();
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = gamPath,
                    Arguments = "oauth create",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                using Process? process = Process.Start(psi);

                if (process == null)
                {
                    MessageBox.Show(
                        "Unable to start GAM OAuth setup.",
                        "GAM Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    DisableChromebookUI();
                    return;
                }

                await Task.Run(() => process.WaitForExit());

                // Check authentication again after GAM finishes.
                var checker = new GamOAuthChecker();
                var result = await checker.CheckOAuthAsync();

                // ⬇️ THIS IS THE BLOCK THAT REPLACES YOUR OLD if (result.Success) { ... } else { ... }
                if (result.Success)
                {
                    MessageBox.Show(
                        "GAM7 OAuth setup completed successfully.",
                        "GAM7 Authentication",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    EnableChromebookUI();
                }
                else if (result.IssueType == Services.GamIssueType.ProjectMissing)
                {
                    await OfferProjectSetupAsync(result);
                }
                else
                {
                    MessageBox.Show(
                        "OAuth setup did not complete.\n\n" + result.Error,
                        "GAM7 Authentication",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    DisableChromebookUI();
                }
                // ⬆️ END REPLACED BLOCK
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while starting GAM OAuth setup.\n\n" +
                    ex.Message,
                    "GAM OAuth Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DisableChromebookUI();
            }
        }
        private void ShowGam7SetupDialog(string message)
        {
            MessageBox.Show(
                message,
                "GAM7 Setup Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        private void DisableChromebookUI()
        {
            _btnLookup.Enabled = false;
            _btnRefresh.Enabled = false;
            _btnClear.Enabled = false;

            _btnDisable.Enabled = false;
            _btnReenable.Enabled = false;
            _btnMoveOu.Enabled = false;
            _btnPowerwash.Enabled = false;
            _btnClearProfiles.Enabled = false;

            _btnCopyMac.Enabled = false;

            _btnEditAssetId.Enabled = false;
            _btnSaveAssetId.Enabled = false;
            _btnCancelAssetId.Enabled = false;

            _btnBulkManagement.Enabled = false;

            _lblDeviceStatus.Text = "GAM7 not authenticated";
        }

        private void EnableChromebookUI()
        {
            _btnLookup.Enabled = true;
            _btnBulkManagement.Enabled = true;

            _lblDeviceStatus.Text = "Ready";
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
                new Size(890, 900);

            MinimumSize =
                new Size(820, 775);

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
                new Point(30, 100),
                new Size(820, 105));

            mainPanel.Controls.Add(
                lookupPanel);

            var lblSearchType = CreateLabel(
                "SEARCH BY",
                new Point(20, 18));

            lookupPanel.Controls.Add(
                lblSearchType);

            _cmbSearchType = new ComboBox
            {
                Location =
                    new Point(20, 45),

                Size =
                    new Size(160, 32),

                BackColor =
                    Color.FromArgb(20, 27, 40),

                ForeColor =
                    Color.White,

                Font =
                    new Font("Segoe UI", 10.5F),

                DropDownStyle =
                    ComboBoxStyle.DropDownList,

                FlatStyle =
                    FlatStyle.Flat
            };

            _cmbSearchType.Items.Add("Serial Number");
            _cmbSearchType.Items.Add("Asset ID");
            _cmbSearchType.SelectedIndex = 0;

            lookupPanel.Controls.Add(
                _cmbSearchType);

            var lblSearchValue = CreateLabel(
                "VALUE",
                new Point(195, 18));

            lookupPanel.Controls.Add(
                lblSearchValue);

            _txtSerial = new TextBox
            {
                Location =
                    new Point(
                        195,
                        45),

                Size =
                    new Size(
                        395,
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
                new Point(30, 215),
                new Size(820, 365));

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

            _txtAssetId.KeyDown += TxtAssetId_KeyDown;

            infoPanel.Controls.Add(
                _txtAssetId);

            _btnEditAssetId = CreateButton(
                "EDIT",
                new Point(430, 88),
                new Size(70, 30));

            _btnEditAssetId.Enabled = false;

            _btnEditAssetId.Click +=
                (s, e) =>
                {
                    EditAssetId();
                };

            infoPanel.Controls.Add(
                _btnEditAssetId);


            _btnSaveAssetId = CreateButton(
                "SAVE",
                new Point(505, 88),
                new Size(70, 30));

            _btnSaveAssetId.Enabled = false;

            _btnSaveAssetId.Click +=
                async (s, e) =>
                {
                    await SaveAssetIdAsync();
                };

            infoPanel.Controls.Add(
                _btnSaveAssetId);


            _btnCancelAssetId = CreateButton(
                "CANCEL",
                new Point(580, 88),
                new Size(85, 30));

            _btnCancelAssetId.Enabled = false;

            _btnCancelAssetId.Click +=
                (s, e) =>
                {
                    CancelAssetIdEdit();
                };

            infoPanel.Controls.Add(
                _btnCancelAssetId);

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
                new Point(430, 214),
                new Size(85, 30));

            _btnCopyMac.Enabled = false;

            _btnCopyMac.Click +=
                BtnCopyMac_Click;

            infoPanel.Controls.Add(
                _btnCopyMac);

            // =========================================================
            // ORGANISATION UNIT
            // =========================================================

            var lblOrgUnitName = CreateLabel(
                "ORGANISATION UNIT",
                new Point(20, 260));

            infoPanel.Controls.Add(
                lblOrgUnitName);

            _lblOrgUnit = new Label
            {
                Text = "—",

                Location =
                    new Point(
                        170,
                        259),

                AutoSize = false,

                Size =
                    new Size(
                        250,
                        30),

                Font =
                    new Font(
                        "Segoe UI",
                        9.5F),

                ForeColor =
                    Color.White
            };

            infoPanel.Controls.Add(
                _lblOrgUnit);

            _btnMoveOu = CreateButton(
                "MOVE OU",
                new Point(430, 253),
                new Size(110, 32));

            _btnMoveOu.Enabled = false;

            _btnMoveOu.Click +=
                async (s, e) =>
                {
                    await MoveChromebookOuAsync();
                };

            infoPanel.Controls.Add(
                _btnMoveOu);

            // =========================================================
            // Last User
            // =========================================================

            _lblRecentUser = AddInfoRow(
                infoPanel,
                "Recent User",
                305,
                out _,
                "—");

            // =========================================================
            // LAST SYNC
            // =========================================================

            _lblLastSync = AddInfoRow(
                infoPanel,
                "Last Sync",
                340,
                out _,
                "—");

            _lblLastSync.Location =
                new Point(
                    170,
                    339);

            // =========================================================
            // ACTION PANEL
            // =========================================================

            var actionPanel = CreatePanel(
                new Point(30, 590),
                new Size(820, 80));

            mainPanel.Controls.Add(
                actionPanel);

            var lblFormActions = new Label
            {
                Text = "FORM ACTIONS",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(
                    120,
                    135,
                    150),
                Location = new Point(20, 8)
            };

            actionPanel.Controls.Add(
                lblFormActions);

            // =========================================================
            // DEVICE ACTIONS
            // =========================================================

            var deviceActionPanel = CreatePanel(
                new Point(30, 680),
                new Size(820, 80));

            mainPanel.Controls.Add(
                deviceActionPanel);

            var lblDeviceActions = new Label
            {
                Text = "DEVICE ACTIONS",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(
                    120,
                    135,
                    150),
                Location = new Point(20, 8)
            };

            deviceActionPanel.Controls.Add(
                lblDeviceActions);

            _btnDisable = CreateButton(
                "DISABLE CHROMEBOOK",
                new Point(20, 30),
                new Size(240, 40));

            _btnDisable.Enabled = false;

            _btnDisable.Click +=
                async (s, e) =>
                {
                    await DisableChromebookAsync();
                };

            deviceActionPanel.Controls.Add(
                _btnDisable);


            _btnReenable = CreateButton(
                "RE-ENABLE CHROMEBOOK",
                new Point(270, 30),
                new Size(240, 40));

            _btnReenable.Enabled = false;

            _btnReenable.Click +=
                async (s, e) =>
                {
                    await ReenableChromebookAsync();
                };

            deviceActionPanel.Controls.Add(
                _btnReenable);

            // =========================================================
            // POWERWASH BUTTON
            // =========================================================

            _btnPowerwash = CreateButton(
                "POWERWASH",
                new Point(525, 30),
                new Size(120, 40));

            _btnPowerwash.Enabled = false;

            _btnPowerwash.Click +=
                async (s, e) =>
                {
                    await PowerwashChromebookAsync();
                };

            deviceActionPanel.Controls.Add(
                _btnPowerwash);


            // =========================================================
            // CLEAR PROFILES BUTTON
            // =========================================================

            _btnClearProfiles = CreateButton(
                "CLEAR PROFILES",
                new Point(655, 30),
                new Size(130, 40));

            _btnClearProfiles.Enabled = false;

            _btnClearProfiles.Click +=
                async (s, e) =>
                {
                    await ClearChromebookProfilesAsync();
                };

            deviceActionPanel.Controls.Add(
                _btnClearProfiles);

            // =========================================================
            // REFRESH BUTTON
            // =========================================================

            _btnRefresh = CreateButton(
                "REFRESH",
                new Point(20, 30),
                new Size(130, 40));

            _btnRefresh.Enabled = false;

            _btnRefresh.Click +=
                async (s, e) =>
                {
                    await RefreshChromebookAsync();
                };

            actionPanel.Controls.Add(
                _btnRefresh);


            /*// =========================================================
            // DISABLE BUTTON
            // =========================================================

            _btnDisable = CreateButton(
                "DISABLE",
                new Point(160, 20),
                new Size(180, 40));

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
                "RE-ENABLE",
                new Point(350, 20),
                new Size(180, 40));

            _btnReenable.Enabled = false;

            _btnReenable.Click +=
                async (s, e) =>
                {
                    await ReenableChromebookAsync();
                };

            actionPanel.Controls.Add(
                _btnReenable);*/

            // =========================================================
            // CLEAR BUTTON
            // =========================================================

            _btnClear = CreateButton(
                "CLEAR",
                new Point(160, 30),
                new Size(100, 40));

            _btnClear.Enabled = false;

            _btnClear.Click +=
                (s, e) =>
                {
                    ClearChromebook();
                };

            actionPanel.Controls.Add(
                _btnClear);

            // =========================================================
            // BULK MANAGEMENT BUTTON
            // =========================================================

            _btnBulkManagement = CreateButton(
                "BULK MANAGEMENT",
                new Point(270, 30),
                new Size(160, 40));

            _btnBulkManagement.Click +=
                (s, e) =>
                {
                    OpenBulkChromebookManagement();
                };

            actionPanel.Controls.Add(
                _btnBulkManagement);

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
                        775)
            };

            mainPanel.Controls.Add(
                lblLog);

            _txtLog = new TextBox
            {
                Location =
                    new Point(
                        30,
                        795),

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
            string searchValue =
                _txtSerial.Text.Trim();

            bool searchByAssetId =
                _cmbSearchType.SelectedItem as string == "Asset ID";

            if (string.IsNullOrWhiteSpace(searchValue))
            {
                MessageBox.Show(
                    searchByAssetId
                        ? "Please enter an Asset ID."
                        : "Please enter a Chromebook serial number.",
                    searchByAssetId ? "Asset ID Required" : "Serial Number Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                _txtSerial.Focus();

                return;
            }

            SetBusy(true);

            ClearDeviceInformation();

            WriteLog(
                searchByAssetId
                    ? $"Looking up Chromebook by Asset ID: {searchValue}"
                    : $"Looking up Chromebook by Serial: {searchValue}");

            try
            {
                var result = searchByAssetId
                    ? await _gamService.GetChromebookInfoByAssetIdAsync(searchValue)
                    : await _gamService.GetChromebookInfoAsync(searchValue);

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
                _btnMoveOu.Enabled = true;
                _btnClear.Enabled = true;
                _btnPowerwash.Enabled = true;
                _btnClearProfiles.Enabled = true;
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

            ClearDeviceInformation();

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
        private void ClearChromebook()
        {
            _txtSerial.Clear();

            ClearDeviceInformation();

            WriteLog("Chromebook information cleared.");
        }
        private void OpenBulkChromebookManagement()
        {
            using var form =
                new BulkChromebookManagementForm();

            form.ShowDialog(this);
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

            string assetId = device.AssetId?.Trim() ?? string.Empty;

            _txtAssetId.Text = assetId;
            _originalAssetId = assetId;

            SetAssetEditMode(false);

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
            // LAST USER
            // ---------------------------------------------------------

            _lblRecentUser.Text =
                string.IsNullOrWhiteSpace(
                    device.RecentUserEmail)
                    ? "—"
                    : device.RecentUserEmail;

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

        private void EditAssetId()
        {
            if (_currentDevice == null)
                return;

            _originalAssetId = _txtAssetId.Text;

            _txtAssetId.ReadOnly = false;

            _btnEditAssetId.Enabled = false;
            _btnSaveAssetId.Enabled = true;
            _btnCancelAssetId.Enabled = true;

            _txtAssetId.Focus();
            _txtAssetId.SelectAll();
        }

        private void CancelAssetIdEdit()
        {
            _txtAssetId.Text = _originalAssetId;

            SetAssetEditMode(false);
        }

        private void SetAssetEditMode(bool editing)
        {
            _txtAssetId.ReadOnly = !editing;

            bool hasDevice = _currentDevice != null;

            _btnEditAssetId.Enabled =
                hasDevice &&
                !editing;

            _btnSaveAssetId.Enabled =
                hasDevice &&
                editing;

            _btnCancelAssetId.Enabled =
                hasDevice &&
                editing;
        }

        private async void TxtAssetId_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter &&
                !_txtAssetId.ReadOnly &&
                _btnSaveAssetId.Enabled)
            {
                e.SuppressKeyPress = true;

                await SaveAssetIdAsync();
            }
            else if (e.KeyCode == Keys.Escape &&
                     !_txtAssetId.ReadOnly)
            {
                e.SuppressKeyPress = true;

                CancelAssetIdEdit();
            }
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

                Services.GamResult result =
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

                SetAssetEditMode(false);

                _btnSaveAssetId.Enabled =
                    false;

                _btnCancelAssetId.Enabled =
                    false;

                _originalAssetId =
                    string.Empty;

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

                Services.GamResult result =
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

                Services.GamResult result =
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

        private async Task MoveChromebookOuAsync()
        {
            if (_currentDevice == null)
                return;

            string serial =
                _currentDevice.SerialNumber?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(serial))
                return;

            string currentOu =
                _currentDevice.OrgUnitPath?.Trim()
                ?? string.Empty;

            using var dialog =
                new MoveChromebookOuForm(
                    currentOu);

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            string? newOu =
                dialog.SelectedOrgUnitPath;

            if (string.IsNullOrWhiteSpace(newOu))
                return;

            if (string.Equals(
                currentOu,
                newOu,
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "The Chromebook is already in this organisational unit.",
                    "Move Chromebook",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    $"Move this Chromebook to a different organisational unit?\n\n" +
                    $"Serial Number:\n{serial}\n\n" +
                    $"Current OU:\n{GetOuDisplay(currentOu)}\n\n" +
                    $"New OU:\n{newOu}",
                    "Confirm Move OU",
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
                    $"Moving Chromebook {serial}...");

                WriteLog(
                    $"Current OU: {GetOuDisplay(currentOu)}");

                WriteLog(
                    $"New OU: {newOu}");

                Services.GamResult result =
                    await _gamService
                        .MoveChromebookToOuAsync(
                            serial,
                            newOu);

                WriteLog(
                    result.CombinedOutput);

                if (!result.Success)
                {
                    MessageBox.Show(
                        result.CombinedOutput,
                        "Move Chromebook Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                MessageBox.Show(
                    $"Chromebook {serial} was moved to:\n\n{newOu}",
                    "Chromebook Moved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await RefreshChromebookAsync();
            }
            catch (Exception ex)
            {
                WriteLog(
                    "MOVE OU ERROR: " +
                    ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Move OU Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private static string GetOuDisplay(string ou)
        {
            return string.IsNullOrWhiteSpace(ou)
                ? "Unknown"
                : ou;
        }


        private async Task PowerwashChromebookAsync()
        {
            if (_currentDevice == null)
                return;

            string serial =
                _currentDevice.SerialNumber?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(serial))
                return;

            DialogResult confirmation =
                MessageBox.Show(
                    $"WARNING: This will POWERWASH the Chromebook.\n\n" +
                    $"Serial Number:\n{serial}\n\n" +
                    $"Asset ID:\n{GetAssetDisplay()}\n\n" +
                    "The Chromebook will be reset and local user data " +
                    "and profiles will be removed.\n\n" +
                    "This action cannot be undone.\n\n" +
                    "Do you want to continue?",
                    "Confirm Chromebook Powerwash",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirmation != DialogResult.Yes)
                return;

            SetBusy(true);

            try
            {
                WriteLog(
                    $"Powerwashing Chromebook {serial}...");

                Services.GamResult result =
                    await _gamService
                        .PowerwashChromebookAsync(serial);

                WriteLog(
                    result.CombinedOutput);

                if (result.Success)
                {
                    MessageBox.Show(
                        $"Powerwash command sent to Chromebook {serial}.",
                        "Powerwash Initiated",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await RefreshChromebookAsync();
                }
                else
                {
                    MessageBox.Show(
                        result.CombinedOutput,
                        "Powerwash Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                WriteLog(
                    "POWERWASH ERROR: " +
                    ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Powerwash Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ClearChromebookProfilesAsync()
        {
            if (_currentDevice == null)
                return;

            string serial =
                _currentDevice.SerialNumber?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(serial))
                return;

            DialogResult confirmation =
                MessageBox.Show(
                    $"WARNING: This will CLEAR USER PROFILES from the Chromebook.\n\n" +
                    $"Serial Number:\n{serial}\n\n" +
                    $"Asset ID:\n{GetAssetDisplay()}\n\n" +
                    "All locally stored ChromeOS user profiles and their " +
                    "local data will be removed from this device.\n\n" +
                    "This action cannot be undone.\n\n" +
                    "Do you want to continue?",
                    "Confirm Clear Profiles",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirmation != DialogResult.Yes)
                return;

            SetBusy(true);

            try
            {
                WriteLog(
                    $"Clearing user profiles from Chromebook {serial}...");

                Services.GamResult result =
                    await _gamService
                        .ClearChromebookProfilesAsync(serial);

                WriteLog(
                    result.CombinedOutput);

                if (result.Success)
                {
                    MessageBox.Show(
                        $"User profiles clear command sent to Chromebook {serial}.",
                        "Clear Profiles Initiated",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await RefreshChromebookAsync();
                }
                else
                {
                    MessageBox.Show(
                        result.CombinedOutput,
                        "Clear Profiles Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                WriteLog(
                    "CLEAR PROFILES ERROR: " +
                    ex.Message);

                MessageBox.Show(
                    ex.Message,
                    "Clear Profiles Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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

            _btnEditAssetId.Enabled = false;
            _btnSaveAssetId.Enabled = false;
            _btnCancelAssetId.Enabled = false;

            _lblDeviceId.Text =
                "—";

            _lblModel.Text =
                "—";

            _lblMac.Text =
                "—";

            _lblOrgUnit.Text =
                "—";

            _lblRecentUser.Text =
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

            _btnPowerwash.Enabled =
                false;

            _btnClearProfiles.Enabled =
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

            _cmbSearchType.Enabled =
                !busy;

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
                _btnRefresh.Enabled = false;

                _btnDisable.Enabled = false;

                _btnReenable.Enabled = false;

                _btnMoveOu.Enabled = false;
                _btnPowerwash.Enabled = false;
                _btnClearProfiles.Enabled = false;

                _btnCopyMac.Enabled = false;
            }
            else
            {
                _btnRefresh.Enabled = !busy;

                _btnDisable.Enabled = !busy;

                _btnReenable.Enabled = !busy;

                _btnMoveOu.Enabled = !busy;
                _btnPowerwash.Enabled = !busy;
                _btnClearProfiles.Enabled = !busy;

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

        private Panel CreatePanel(Point location, Size size)
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

        private Button CreateButton(string text, Point location, Size size)
        {
            return new HudButton
            {
                Text = text,
                Location = location,
                Size = size
            };
        }
    }
}