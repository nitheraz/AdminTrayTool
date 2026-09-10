using AdminTrayTool.Services;
using AdminTrayTool.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace AdminTrayTool
{
    public class GroupManagementForm : Form
    {
        private const string CustomOption = "Custom (enter group manually)";

        private readonly GamService _gamService;
        private GroupTemplateConfig _templateConfig = new();

        private ComboBox _cmbRole = null!;
        private TextBox _txtGroupEmail = null!;
        private TextBox _txtStaffEmail = null!;
        private Button _btnAddToGroup = null!;
        private Label _lblStatus = null!;
        private TextBox _txtLog = null!;

        public GroupManagementForm()
        {
            _gamService = new GamService();

            InitializeForm();
            BuildInterface();
            Load += GroupManagementForm_Load;
        }
        private static readonly TimeSpan AutocompleteCacheMaxAge = TimeSpan.FromHours(4);

        private async void GroupManagementForm_Load(object? sender, EventArgs e)
        {
            LoadTemplates();
            await ValidateGam7Async();

            _lblStatus.Text = "Loading group/user lists...";

            var groupTask = LoadGroupAutocompleteAsync();
            var userTask = LoadUserAutocompleteAsync();
            await Task.WhenAll(groupTask, userTask);

            if (IsDisposed || !IsHandleCreated)
                return;

            if (_btnAddToGroup.Enabled)
            {
                _lblStatus.Text = "Ready";
            }
        }

        private async Task LoadGroupAutocompleteAsync()
        {
            try
            {
                string cachePath = EmailAutocompleteCacheService.GetGroupsCachePath();
                List<string>? emails = EmailAutocompleteCacheService.TryLoad(cachePath, AutocompleteCacheMaxAge);

                if (emails != null)
                {
                    ApplyGroupAutocomplete(emails);
                    WriteLog($"Loaded {emails.Count} group(s) from cache.");
                    return;
                }

                var (success, groupEmails, error) = await _gamService.GetAllGroupEmailsAsync();

                if (!success)
                {
                    WriteLog("Could not load group list for autocomplete: " + error);
                    return;
                }

                ApplyGroupAutocomplete(groupEmails);
                EmailAutocompleteCacheService.Save(cachePath, groupEmails);
                WriteLog($"Loaded {groupEmails.Count} group(s) from GAM and cached them.");
            }
            catch (Exception ex)
            {
                WriteLog("ERROR loading group autocomplete: " + ex.Message);
            }
        }

        private async Task LoadUserAutocompleteAsync()
        {
            try
            {
                string cachePath = EmailAutocompleteCacheService.GetUsersCachePath();
                List<string>? emails = EmailAutocompleteCacheService.TryLoad(cachePath, AutocompleteCacheMaxAge);

                if (emails != null)
                {
                    ApplyUserAutocomplete(emails);
                    WriteLog($"Loaded {emails.Count} user(s) from cache.");
                    return;
                }

                var (success, userEmails, error) = await _gamService.GetAllUserEmailsAsync();

                if (!success)
                {
                    WriteLog("Could not load user list for autocomplete: " + error);
                    return;
                }

                ApplyUserAutocomplete(userEmails);
                EmailAutocompleteCacheService.Save(cachePath, userEmails);
                WriteLog($"Loaded {userEmails.Count} user(s) from GAM and cached them.");
            }
            catch (Exception ex)
            {
                WriteLog("ERROR loading user autocomplete: " + ex.Message);
            }
        }

        private void ApplyGroupAutocomplete(List<string> emails)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            var source = new AutoCompleteStringCollection();
            source.AddRange(emails.ToArray());

            _txtGroupEmail.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _txtGroupEmail.AutoCompleteSource = AutoCompleteSource.CustomSource;
            _txtGroupEmail.AutoCompleteCustomSource = source;
        }

        private void ApplyUserAutocomplete(List<string> emails)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            var source = new AutoCompleteStringCollection();
            source.AddRange(emails.ToArray());

            _txtStaffEmail.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _txtStaffEmail.AutoCompleteSource = AutoCompleteSource.CustomSource;
            _txtStaffEmail.AutoCompleteCustomSource = source;
        }
        private void LoadTemplates()
        {
            _templateConfig = GroupTemplateService.Load();

            _cmbRole.Items.Clear();

            foreach (var template in _templateConfig.Templates)
            {
                _cmbRole.Items.Add(template.Name);
            }

            _cmbRole.Items.Add(CustomOption);

            if (_cmbRole.Items.Count > 0)
            {
                _cmbRole.SelectedIndex = 0;
            }
        }

        // =============================================================
        // GAM AUTH VALIDATION (same pattern as ChromebookManagementForm)
        // =============================================================

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
                    DisableGroupUI();
                }

                return;
            }

            EnableGroupUI();
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
                DisableGroupUI();
                return;
            }

            string gamArgs = answer == DialogResult.Yes ? "create project" : "use project";

            string? gamPath = await GamLocator.LocateGam();
            if (gamPath == null)
            {
                MessageBox.Show("GAM executable not found.", "GAM Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisableGroupUI();
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

                await RunOAuthSetupAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while running GAM project setup.\n\n" + ex.Message,
                    "GAM Project Setup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                DisableGroupUI();
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

                DisableGroupUI();
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

                    DisableGroupUI();
                    return;
                }

                await Task.Run(() => process.WaitForExit());

                var checker = new GamOAuthChecker();
                Services.GamResult result = await checker.CheckOAuthAsync();

                if (result.Success)
                {
                    MessageBox.Show(
                        "GAM7 OAuth setup completed successfully.",
                        "GAM7 Authentication",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    EnableGroupUI();
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
                    DisableGroupUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while starting GAM OAuth setup.\n\n" + ex.Message,
                    "GAM OAuth Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DisableGroupUI();
            }
        }

        private void DisableGroupUI()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            _btnAddToGroup.Enabled = false;
            _lblStatus.Text = "GAM7 not authenticated";
        }

        private void EnableGroupUI()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            _btnAddToGroup.Enabled = true;
            _lblStatus.Text = "Ready";
        }

        // =============================================================
        // FORM INITIALISATION
        // =============================================================

        private void InitializeForm()
        {
            Text = "Group Management";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(575, 500);
            MinimumSize = new Size(560, 430);
            BackColor = Color.FromArgb(10, 15, 25);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            FormBorderStyle = FormBorderStyle.FixedSingle;
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
                Padding = new Padding(30),
                BackColor = Color.FromArgb(10, 15, 25)
            };

            Controls.Add(mainPanel);

            var lblTitle = new Label
            {
                Text = "GROUP MANAGEMENT",
                AutoSize = true,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 25)
            };
            mainPanel.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = "Add staff to Google Workspace groups by role",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(150, 160, 175),
                Location = new Point(33, 62)
            };
            mainPanel.Controls.Add(lblSubtitle);

            var headerLine = new Panel
            {
                Location = new Point(30, 90),
                Size = new Size(520, 1),
                BackColor = Color.FromArgb(45, 55, 70)
            };
            mainPanel.Controls.Add(headerLine);

            var formPanel = CreatePanel(new Point(30, 110), new Size(520, 230));
            mainPanel.Controls.Add(formPanel);

            var lblRole = CreateLabel("STAFF TYPE", new Point(20, 18));
            formPanel.Controls.Add(lblRole);

            _cmbRole = new ComboBox
            {
                Location = new Point(20, 42),
                Size = new Size(480, 32),
                BackColor = Color.FromArgb(20, 27, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            _cmbRole.SelectedIndexChanged += CmbRole_SelectedIndexChanged;
            formPanel.Controls.Add(_cmbRole);

            var lblGroup = CreateLabel("GROUP EMAIL (CUSTOM ONLY)", new Point(20, 90));
            formPanel.Controls.Add(lblGroup);

            _txtGroupEmail = new TextBox
            {
                Location = new Point(20, 114),
                Size = new Size(480, 32),
                BackColor = Color.FromArgb(20, 27, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11F),
                PlaceholderText = "e.g. staff-all@yourdomain.org",
                Enabled = false
            };
            formPanel.Controls.Add(_txtGroupEmail);

            var lblStaff = CreateLabel("STAFF EMAIL", new Point(20, 158));
            formPanel.Controls.Add(lblStaff);

            _txtStaffEmail = new TextBox
            {
                Location = new Point(20, 182),
                Size = new Size(480, 32),
                BackColor = Color.FromArgb(20, 27, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11F),
                PlaceholderText = "e.g. jsmith@yourdomain.org"
            };
            _txtStaffEmail.KeyDown += TxtStaffEmail_KeyDown;
            formPanel.Controls.Add(_txtStaffEmail);

            _btnAddToGroup = CreateButton("ADD TO GROUP(S)", new Point(30, 350), new Size(200, 36));
            _btnAddToGroup.Click += async (s, e) => await AddToGroupAsync();
            mainPanel.Controls.Add(_btnAddToGroup);

            _lblStatus = new Label
            {
                Text = "NOT READY",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 150, 165),
                Location = new Point(260, 358)
            };
            mainPanel.Controls.Add(_lblStatus);

            var lblLog = new Label
            {
                Text = "GAM OUTPUT",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 160, 175),
                Location = new Point(30, 400)
            };
            mainPanel.Controls.Add(lblLog);

            _txtLog = new TextBox
            {
                Location = new Point(30, 425),
                Size = new Size(520, 60),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(6, 10, 18),
                ForeColor = Color.FromArgb(150, 160, 175),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 8.5F),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
            };
            mainPanel.Controls.Add(_txtLog);

            _cmbRole.Focus();
        }

        private void CmbRole_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool isCustom = _cmbRole.SelectedItem as string == CustomOption;
            _txtGroupEmail.Enabled = isCustom;

            if (!isCustom)
            {
                _txtGroupEmail.Clear();
            }
        }

        private void TxtStaffEmail_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _ = AddToGroupAsync();
            }
        }

        // =============================================================
        // ADD TO GROUP
        // =============================================================

        private async Task AddToGroupAsync()
        {
            string staffEmail = _txtStaffEmail.Text.Trim();
            string? selectedRole = _cmbRole.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(staffEmail))
            {
                MessageBox.Show("Please enter the staff member's email.", "Staff Email Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtStaffEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedRole))
            {
                MessageBox.Show("Please select a staff type.", "Staff Type Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] groupsToAdd;

            if (selectedRole == CustomOption)
            {
                string customGroup = _txtGroupEmail.Text.Trim();

                if (string.IsNullOrWhiteSpace(customGroup))
                {
                    MessageBox.Show("Please enter the group email.", "Group Email Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtGroupEmail.Focus();
                    return;
                }

                groupsToAdd = new[] { customGroup };
            }
            else
            {
                var template = _templateConfig.Templates
                    .FirstOrDefault(t => t.Name == selectedRole);

                if (template == null || template.Groups.Count == 0)
                {
                    MessageBox.Show(
                        $"The '{selectedRole}' template has no groups configured.\n\n" +
                        "Edit groupTemplates.json to add groups for this role.",
                        "No Groups Configured",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                groupsToAdd = template.Groups.ToArray();
            }

            string groupListDisplay = string.Join("\n", groupsToAdd);

            DialogResult confirmation = MessageBox.Show(
                $"Add this staff member to the following group(s)?\n\n" +
                $"Staff:\n{staffEmail}\n\n" +
                $"Group(s):\n{groupListDisplay}",
                "Confirm Add to Group(s)",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes)
                return;

            SetBusy(true);

            var summary = new StringBuilder();
            int successCount = 0;
            int failCount = 0;

            foreach (string group in groupsToAdd)
            {
                WriteLog($"Adding {staffEmail} to {group}...");

                try
                {
                    Services.GamResult result = await _gamService.AddUserToGroupAsync(group, staffEmail);
                    WriteLog(result.CombinedOutput);

                    if (result.Success)
                    {
                        successCount++;
                        summary.AppendLine($"✔ {group}");
                    }
                    else
                    {
                        failCount++;
                        summary.AppendLine($"✘ {group} — {result.Error}");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    summary.AppendLine($"✘ {group} — {ex.Message}");
                    WriteLog("ERROR: " + ex.Message);
                }
            }

            SetBusy(false);

            MessageBoxIcon icon = failCount == 0
                ? MessageBoxIcon.Information
                : successCount == 0
                    ? MessageBoxIcon.Error
                    : MessageBoxIcon.Warning;

            MessageBox.Show(
                $"Finished adding {staffEmail}.\n\n{summary}",
                failCount == 0 ? "Staff Added" : "Completed With Errors",
                MessageBoxButtons.OK,
                icon);

            if (failCount == 0)
            {
                _txtStaffEmail.Clear();
                _txtStaffEmail.Focus();
            }
        }

        private void SetBusy(bool busy)
        {
            _btnAddToGroup.Enabled = !busy;
            _cmbRole.Enabled = !busy;
            _txtGroupEmail.Enabled = !busy && (_cmbRole.SelectedItem as string == CustomOption);
            _txtStaffEmail.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        private void WriteLog(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (IsDisposed || !IsHandleCreated)
                return;

            if (_txtLog.TextLength > 0)
            {
                _txtLog.AppendText(Environment.NewLine + Environment.NewLine);
            }

            _txtLog.AppendText(text);
        }

        // =============================================================
        // UI HELPERS
        // =============================================================

        private Panel CreatePanel(Point location, Size size)
        {
            return new Panel
            {
                Location = location,
                Size = size,
                BackColor = Color.FromArgb(16, 23, 36),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Label CreateLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 150, 165)
            };
        }

        private Button CreateButton(string text, Point location, Size size)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 35, 52),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderColor = Color.FromArgb(65, 85, 110);
            button.FlatAppearance.BorderSize = 1;

            return button;
        }
    }
}