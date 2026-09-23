using AdminTrayTool.Services;

namespace AdminTrayTool
{
    public class ActiveDirectoryManagementForm : Form
    {
        private readonly WindowsManagementService _windowsManagementService;

        private TextBox _txtComputerName = null!;
        private Button _btnLookup = null!;

        private Label _lblComputerName = null!;
        private Label _lblOperatingSystem = null!;
        private Label _lblCurrentOu = null!;
        private Label _lblDistinguishedName = null!;

        private TextBox _txtOuFilter = null!;
        private ComboBox _cmbTargetOu = null!;
        private Button _btnMove = null!;

        private TextBox _txtLog = null!;

        private List<string> _organizationalUnits = [];

        public ActiveDirectoryManagementForm()
        {
            _windowsManagementService =
                new WindowsManagementService();

            InitializeForm();
            BuildUi();
        }

        private void InitializeForm()
        {
            Text = "Active Directory Management";
            StartPosition = FormStartPosition.CenterScreen;

            ClientSize = new Size(890, 800);
            MinimumSize = new Size(820, 700);

            BackColor = Color.FromArgb(10, 15, 25);
            ForeColor = Color.White;
            Font = new Font(
                "Segoe UI",
                10F,
                FontStyle.Regular);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
        }

        private void BuildUi()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                BackColor = Color.FromArgb(10, 15, 25)
            };

            Controls.Add(mainPanel);

            // ---------------------------------------------------------
            // HEADER
            // ---------------------------------------------------------

            var lblTitle = new Label
            {
                Text = "ACTIVE DIRECTORY",
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
                Text = "Manage Active Directory computers and organisational units",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9.5F),
                ForeColor = Color.FromArgb(150, 160, 175),
                Location = new Point(33, 62)
            };

            mainPanel.Controls.Add(lblSubtitle);

            var headerLine = new Panel
            {
                Location = new Point(30, 90),
                Size = new Size(820, 1),
                BackColor = Color.FromArgb(45, 55, 70)
            };

            mainPanel.Controls.Add(headerLine);

            // ---------------------------------------------------------
            // COMPUTER LOOKUP
            // ---------------------------------------------------------

            var lookupPanel =
                CreatePanel(
                    new Point(30, 105),
                    new Size(820, 105));

            mainPanel.Controls.Add(lookupPanel);

            var lblLookupTitle = new Label
            {
                Text = "COMPUTER LOOKUP",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15)
            };

            lookupPanel.Controls.Add(lblLookupTitle);

            var lblComputerNameTitle =
                CreateLabel(
                    "Computer Name",
                    new Point(20, 48));

            lookupPanel.Controls.Add(lblComputerNameTitle);

            _txtComputerName = new TextBox
            {
                Location = new Point(20, 68),
                Size = new Size(420, 27),
                BackColor = Color.FromArgb(20, 27, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _txtComputerName.KeyDown +=
                TxtComputerName_KeyDown;

            lookupPanel.Controls.Add(_txtComputerName);

            _btnLookup = new HudButton
            {
                Text = "LOOK UP",
                Location = new Point(455, 67),
                Size = new Size(130, 30)
            };

            _btnLookup.Click +=
                BtnLookup_Click;

            lookupPanel.Controls.Add(_btnLookup);

            // ---------------------------------------------------------
            // COMPUTER INFORMATION
            // ---------------------------------------------------------

            var infoPanel =
                CreatePanel(
                    new Point(30, 220),
                    new Size(820, 220));

            mainPanel.Controls.Add(infoPanel);

            var lblInfoTitle = new Label
            {
                Text = "COMPUTER INFORMATION",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15)
            };

            infoPanel.Controls.Add(lblInfoTitle);

            _lblComputerName =
                CreateInfoRow(
                    infoPanel,
                    "Computer:",
                    new Point(20, 50));

            _lblOperatingSystem =
                CreateInfoRow(
                    infoPanel,
                    "Operating System:",
                    new Point(20, 85));

            _lblCurrentOu =
                CreateInfoRow(
                    infoPanel,
                    "Current OU:",
                    new Point(20, 120));

            _lblDistinguishedName =
                CreateInfoRow(
                    infoPanel,
                    "Distinguished Name:",
                    new Point(20, 155));

            // ---------------------------------------------------------
            // ACTIVE DIRECTORY ACTIONS
            // ---------------------------------------------------------

            var adPanel =
                CreatePanel(
                    new Point(30, 450),
                    new Size(820, 125));

            mainPanel.Controls.Add(adPanel);

            var lblAdTitle = new Label
            {
                Text = "ACTIVE DIRECTORY",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15)
            };

            adPanel.Controls.Add(lblAdTitle);

            var lblFilter =
                CreateLabel(
                    "Filter OUs",
                    new Point(20, 47));

            adPanel.Controls.Add(lblFilter);

            _txtOuFilter = new TextBox
            {
                Location = new Point(20, 67),
                Size = new Size(250, 27),
                BackColor = Color.FromArgb(20, 27, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _txtOuFilter.TextChanged +=
                TxtOuFilter_TextChanged;

            adPanel.Controls.Add(_txtOuFilter);

            var lblTargetOu =
                CreateLabel(
                    "Target Organisational Unit",
                    new Point(285, 47));

            adPanel.Controls.Add(lblTargetOu);

            _cmbTargetOu = new ComboBox
            {
                Location = new Point(285, 67),
                Size = new Size(330, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 27, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            _cmbTargetOu.SelectedIndexChanged +=
                CmbTargetOu_SelectedIndexChanged;

            adPanel.Controls.Add(_cmbTargetOu);

            _btnMove = new HudButton
            {
                Text = "MOVE COMPUTER",
                Location = new Point(630, 66),
                Size = new Size(160, 32),
                Enabled = false
            };

            _btnMove.Click +=
                BtnMove_Click;

            adPanel.Controls.Add(_btnMove);

            // ---------------------------------------------------------
            // ACTIVITY LOG
            // ---------------------------------------------------------

            var logPanel =
                CreatePanel(
                    new Point(30, 590),
                    new Size(820, 110));

            mainPanel.Controls.Add(logPanel);

            var lblLogTitle = new Label
            {
                Text = "ACTIVITY LOG",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 10)
            };

            logPanel.Controls.Add(lblLogTitle);

            _txtLog = new TextBox
            {
                Location = new Point(20, 38),
                Size = new Size(780, 58),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(6, 10, 18),
                ForeColor = Color.FromArgb(150, 160, 175),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(
                    "Consolas",
                    8.5F)
            };

            logPanel.Controls.Add(_txtLog);

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


        private Label CreateLabel(
            string text,
            Point location)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 150, 165),
                Location = location
            };
        }

        private Label CreateInfoRow(
            Panel panel,
            string title,
            Point location)
        {
            var titleLabel = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 135, 150),
                Location = location
            };

            panel.Controls.Add(titleLabel);

            var valueLabel = new Label
            {
                Text = string.Empty,
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9.5F,
                    FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(
                    location.X + titleLabel.PreferredWidth + 8,
                    location.Y - 1)
            };

            panel.Controls.Add(valueLabel);

            return valueLabel;
        }

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
                    "Active Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                SetBusy(true);

                WriteLog(
                    $"Looking up '{computerName}' in Active Directory...");

                var result =
                    await _windowsManagementService
                        .GetComputerAsync(computerName);

                if (!result.Success)
                {
                    ClearComputerInformation();

                    WriteLog(
                        $"Lookup failed: {result.Error}");

                    MessageBox.Show(
                        result.Error,
                        "Active Directory Lookup",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                _lblComputerName.Text =
                    result.Name;

                _lblOperatingSystem.Text =
                    result.OperatingSystem;

                _lblCurrentOu.Text =
                    result.OrganizationalUnit;

                _lblDistinguishedName.Text =
                    result.DistinguishedName;

                WriteLog(
                    $"Computer found: {result.Name}");

                WriteLog(
                    $"Current OU: {result.OrganizationalUnit}");

                await LoadOrganizationalUnitsAsync();
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async System.Threading.Tasks.Task
            LoadOrganizationalUnitsAsync()
        {
            WriteLog(
                "Loading Active Directory organisational units...");

            var result =
                await _windowsManagementService
                    .GetOrganizationalUnitsAsync();

            if (!result.Success)
            {
                WriteLog(
                    $"Unable to load OUs: {result.Error}");

                MessageBox.Show(
                    result.Error,
                    "Active Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            _organizationalUnits =
                result.OrganizationalUnits;

            ApplyOuFilter();

            WriteLog(
                $"Loaded {_organizationalUnits.Count} organisational units.");
        }

        private void ApplyOuFilter()
        {
            if (_cmbTargetOu == null)
                return;

            string filter =
                _txtOuFilter?.Text.Trim() ??
                string.Empty;

            string? selectedOu =
                _cmbTargetOu.SelectedItem?.ToString();

            _cmbTargetOu.BeginUpdate();

            try
            {
                _cmbTargetOu.Items.Clear();

                foreach (string ou in _organizationalUnits)
                {
                    if (string.IsNullOrWhiteSpace(filter) ||
                        ou.Contains(
                            filter,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        _cmbTargetOu.Items.Add(ou);
                    }
                }

                if (!string.IsNullOrWhiteSpace(selectedOu))
                {
                    int index =
                        _cmbTargetOu.Items.IndexOf(
                            selectedOu);

                    if (index >= 0)
                    {
                        _cmbTargetOu.SelectedIndex =
                            index;
                    }
                }
            }
            finally
            {
                _cmbTargetOu.EndUpdate();
            }
        }

        private void TxtOuFilter_TextChanged(
            object? sender,
            EventArgs e)
        {
            ApplyOuFilter();
        }

        private void CmbTargetOu_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            if (_btnMove == null)
                return;

            _btnMove.Enabled =
                _cmbTargetOu.SelectedItem != null;
        }

        private async void BtnMove_Click(
            object? sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                    _txtComputerName.Text))
            {
                return;
            }

            if (_cmbTargetOu.SelectedItem == null)
            {
                MessageBox.Show(
                    "Select a target organisational unit.",
                    "Active Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string computerName =
                _txtComputerName.Text.Trim();

            string targetOu =
                _cmbTargetOu.SelectedItem
                    .ToString() ??
                string.Empty;

            DialogResult confirmation =
                MessageBox.Show(
                    $"Move computer '{computerName}' to:" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    targetOu +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    "Continue?",
                    "Confirm Computer Move",
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
                    $"Moving '{computerName}' to '{targetOu}'...");

                var result =
                    await _windowsManagementService
                        .MoveComputerToOuAsync(
                            computerName,
                            targetOu);

                if (!result.Success)
                {
                    WriteLog(
                        $"Move failed: {result.Error}");

                    MessageBox.Show(
                        result.Error,
                        "Active Directory",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                WriteLog(
                    $"Successfully moved '{computerName}'.");

                MessageBox.Show(
                    $"Computer '{computerName}' was moved successfully.",
                    "Active Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await LookupComputerAsync();
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ClearComputerInformation()
        {
            _lblComputerName.Text =
                string.Empty;

            _lblOperatingSystem.Text =
                string.Empty;

            _lblCurrentOu.Text =
                string.Empty;

            _lblDistinguishedName.Text =
                string.Empty;

            _organizationalUnits.Clear();

            _cmbTargetOu.Items.Clear();

            _btnMove.Enabled = false;
        }

        private void SetBusy(bool busy)
        {
            _btnLookup.Enabled = !busy;
            _txtComputerName.Enabled = !busy;
            _txtOuFilter.Enabled = !busy;
            _cmbTargetOu.Enabled = !busy;

            _btnMove.Enabled =
                !busy &&
                _cmbTargetOu.SelectedItem != null;

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
    }
}