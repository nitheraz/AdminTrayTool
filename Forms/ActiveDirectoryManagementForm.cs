using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
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

        private List<string> _organizationalUnits = new();

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
                Text = "ACTIVE DIRECTORY",
                Location = new Point(25, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White
            };

            Controls.Add(header);

            var subtitle = new Label
            {
                Text = "Manage Active Directory computers and organisational units",
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
                Size = new Size(850, 170),
                BackColor = Color.FromArgb(20, 27, 40)
            };

            Controls.Add(infoPanel);

            var infoTitle = new Label
            {
                Text = "COMPUTER INFORMATION",
                Location = new Point(15, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            infoPanel.Controls.Add(infoTitle);

            _lblComputerName =
                CreateInfoLabel(
                    "Computer:",
                    new Point(15, 45));

            _lblOperatingSystem =
                CreateInfoLabel(
                    "Operating System:",
                    new Point(15, 75));

            _lblCurrentOu =
                CreateInfoLabel(
                    "Current OU:",
                    new Point(15, 105));

            _lblDistinguishedName =
                CreateInfoLabel(
                    "Distinguished Name:",
                    new Point(15, 135));

            infoPanel.Controls.Add(_lblComputerName);
            infoPanel.Controls.Add(_lblOperatingSystem);
            infoPanel.Controls.Add(_lblCurrentOu);
            infoPanel.Controls.Add(_lblDistinguishedName);

            // Active Directory actions
            var adPanel = new Panel
            {
                Location = new Point(25, 370),
                Size = new Size(850, 130),
                BackColor = Color.FromArgb(20, 27, 40)
            };

            Controls.Add(adPanel);

            var adTitle = new Label
            {
                Text = "ACTIVE DIRECTORY",
                Location = new Point(15, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            adPanel.Controls.Add(adTitle);

            var filterLabel = new Label
            {
                Text = "Filter OUs",
                Location = new Point(15, 45),
                AutoSize = true
            };

            adPanel.Controls.Add(filterLabel);

            _txtOuFilter = new TextBox
            {
                Location = new Point(15, 68),
                Size = new Size(250, 27),
                BackColor = Color.FromArgb(30, 38, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _txtOuFilter.TextChanged +=
                TxtOuFilter_TextChanged;

            adPanel.Controls.Add(_txtOuFilter);

            var targetLabel = new Label
            {
                Text = "Target Organisational Unit",
                Location = new Point(280, 45),
                AutoSize = true
            };

            adPanel.Controls.Add(targetLabel);

            _cmbTargetOu = new ComboBox
            {
                Location = new Point(280, 68),
                Size = new Size(390, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 38, 52),
                ForeColor = Color.White
            };

            _cmbTargetOu.SelectedIndexChanged +=
                CmbTargetOu_SelectedIndexChanged;

            adPanel.Controls.Add(_cmbTargetOu);

            _btnMove = new Button
            {
                Text = "MOVE COMPUTER",
                Location = new Point(680, 67),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(40, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };

            _btnMove.FlatAppearance.BorderColor =
                Color.FromArgb(80, 100, 130);

            _btnMove.Click += BtnMove_Click;

            adPanel.Controls.Add(_btnMove);

            // Activity log
            var logTitle = new Label
            {
                Text = "ACTIVITY LOG",
                Location = new Point(25, 520),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            Controls.Add(logTitle);

            _txtLog = new TextBox
            {
                Location = new Point(25, 550),
                Size = new Size(850, 100),
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
                    $"Computer: {result.Name}";

                _lblOperatingSystem.Text =
                    $"Operating System: {result.OperatingSystem}";

                _lblCurrentOu.Text =
                    $"Current OU: {result.OrganizationalUnit}";

                _lblDistinguishedName.Text =
                    $"Distinguished Name: {result.DistinguishedName}";

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
                "Computer:";

            _lblOperatingSystem.Text =
                "Operating System:";

            _lblCurrentOu.Text =
                "Current OU:";

            _lblDistinguishedName.Text =
                "Distinguished Name:";

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