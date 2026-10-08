using AdminTrayTool.Models;
using AdminTrayTool.Services;
using AdminTrayTool.UI;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Text;

namespace AdminTrayTool.Forms
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
        private Button _btnBulkAddMembers = null!;
        private Label _lblStatus = null!;
        private TextBox _txtLog = null!;

        public GroupManagementForm()
        {
            _gamService = new GamService();

            InitializeForm();
            BuildInterface();

            Load += GroupManagementForm_Load;
        }

        private static readonly TimeSpan AutocompleteCacheMaxAge =
            TimeSpan.FromHours(4);

        // =============================================================
        // FORM LOAD
        // =============================================================

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

            if (_btnAddToGroup.Enabled && _btnBulkAddMembers.Enabled)
            {
                _lblStatus.Text = "Ready";
            }
        }

        // =============================================================
        // AUTOCOMPLETE
        // =============================================================

        private async Task LoadGroupAutocompleteAsync()
        {
            try
            {
                string cachePath =
                    EmailAutocompleteCacheService.GetGroupsCachePath();

                List<string>? emails =
                    EmailAutocompleteCacheService.TryLoad(
                        cachePath,
                        AutocompleteCacheMaxAge);

                if (emails != null)
                {
                    ApplyGroupAutocomplete(emails);
                    WriteLog($"Loaded {emails.Count} group(s) from cache.");
                    return;
                }

                var (success, groupEmails, error) =
                    await _gamService.GetAllGroupEmailsAsync();

                if (!success)
                {
                    WriteLog(
                        "Could not load group list for autocomplete: " +
                        error);

                    return;
                }

                ApplyGroupAutocomplete(groupEmails);

                EmailAutocompleteCacheService.Save(
                    cachePath,
                    groupEmails);

                WriteLog(
                    $"Loaded {groupEmails.Count} group(s) from GAM and cached them.");
            }
            catch (Exception ex)
            {
                WriteLog(
                    "ERROR loading group autocomplete: " +
                    ex.Message);
            }
        }

        private async Task LoadUserAutocompleteAsync()
        {
            try
            {
                string cachePath =
                    EmailAutocompleteCacheService.GetUsersCachePath();

                List<string>? emails =
                    EmailAutocompleteCacheService.TryLoad(
                        cachePath,
                        AutocompleteCacheMaxAge);

                if (emails != null)
                {
                    ApplyUserAutocomplete(emails);
                    WriteLog($"Loaded {emails.Count} user(s) from cache.");
                    return;
                }

                var (success, userEmails, error) =
                    await _gamService.GetAllUserEmailsAsync();

                if (!success)
                {
                    WriteLog(
                        "Could not load user list for autocomplete: " +
                        error);

                    return;
                }

                ApplyUserAutocomplete(userEmails);

                EmailAutocompleteCacheService.Save(
                    cachePath,
                    userEmails);

                WriteLog(
                    $"Loaded {userEmails.Count} user(s) from GAM and cached them.");
            }
            catch (Exception ex)
            {
                WriteLog(
                    "ERROR loading user autocomplete: " +
                    ex.Message);
            }
        }

        private void ApplyGroupAutocomplete(List<string> emails)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            var source = new AutoCompleteStringCollection();
            source.AddRange([.. emails]);

            _txtGroupEmail.AutoCompleteMode =
                AutoCompleteMode.SuggestAppend;

            _txtGroupEmail.AutoCompleteSource =
                AutoCompleteSource.CustomSource;

            _txtGroupEmail.AutoCompleteCustomSource =
                source;
        }

        private void ApplyUserAutocomplete(List<string> emails)
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            var source = new AutoCompleteStringCollection();
            source.AddRange([.. emails]);

            _txtStaffEmail.AutoCompleteMode =
                AutoCompleteMode.SuggestAppend;

            _txtStaffEmail.AutoCompleteSource =
                AutoCompleteSource.CustomSource;

            _txtStaffEmail.AutoCompleteCustomSource =
                source;
        }

        // =============================================================
        // GROUP TEMPLATES
        // =============================================================

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
        // GAM AUTH VALIDATION
        // =============================================================

        private async Task ValidateGam7Async()
        {
            var checker = new GamOAuthChecker();

            Services.GamResult result =
                await checker.CheckOAuthAsync();

            if (!result.Success)
            {
                if (result.IssueType ==
                    Services.GamIssueType.ProjectMissing)
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

        private async Task OfferProjectSetupAsync(
            Services.GamResult result)
        {
            DialogResult answer = MessageBox.Show(
                "No GAM project was found on this computer.\n\n" +
                result.Error +
                "\n\n" +
                "Would you like to run 'gam create project' now? " +
                "(Choose No if you already have a project and want to " +
                "run 'gam use project' instead.)",
                "GAM Project Required",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (answer == DialogResult.Cancel)
            {
                DisableGroupUI();
                return;
            }

            string gamArgs =
                answer == DialogResult.Yes
                    ? "create project"
                    : "use project";

            string? gamPath =
                await GamLocator.LocateGam();

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
                    Arguments = gamArgs,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                using Process? process =
                    Process.Start(psi);

                if (process != null)
                {
                    await Task.Run(
                        () => process.WaitForExit());
                }

                await RunOAuthSetupAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while running GAM project setup.\n\n" +
                    ex.Message,
                    "GAM Project Setup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DisableGroupUI();
            }
        }

        private async Task RunOAuthSetupAsync()
        {
            string? gamPath =
                await GamLocator.LocateGam();

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

                using Process? process =
                    Process.Start(psi);

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

                await Task.Run(
                    () => process.WaitForExit());

                var checker = new GamOAuthChecker();

                Services.GamResult result =
                    await checker.CheckOAuthAsync();

                if (result.Success)
                {
                    MessageBox.Show(
                        "GAM7 OAuth setup completed successfully.",
                        "GAM7 Authentication",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    EnableGroupUI();
                }
                else if (result.IssueType ==
                         Services.GamIssueType.ProjectMissing)
                {
                    await OfferProjectSetupAsync(result);
                }
                else
                {
                    MessageBox.Show(
                        "OAuth setup did not complete.\n\n" +
                        result.Error,
                        "GAM7 Authentication",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    DisableGroupUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while starting GAM OAuth setup.\n\n" +
                    ex.Message,
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
            _btnBulkAddMembers.Enabled = false;

            _lblStatus.Text =
                "GAM7 not authenticated";
        }

        private void EnableGroupUI()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            _btnAddToGroup.Enabled = true;
            _btnBulkAddMembers.Enabled = true;

            _lblStatus.Text = "Ready";
        }

        // =============================================================
        // FORM INITIALISATION
        // =============================================================

        private void InitializeForm()
        {
            Text = "Group Management";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(650, 650);

            MinimumSize =
                new Size(650, 650);

            MaximumSize =
                new Size(650, 650);

            BackColor =
                Color.FromArgb(10, 15, 25);

            ForeColor =
                Color.White;

            Font =
                new Font("Segoe UI", 10F);

            FormBorderStyle =
                FormBorderStyle.FixedSingle;

            MaximizeBox = false;
            MinimizeBox = true;
        }

        // =============================================================
        // BUILD INTERFACE
        // =============================================================

        private void BuildInterface()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(10, 15, 25)
            };

            Controls.Add(mainPanel);

            // ---------------------------------------------------------
            // TITLE
            // ---------------------------------------------------------

            var lblTitle = new Label
            {
                Text = "GROUP MANAGEMENT",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    20F,
                    FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 24)
            };

            mainPanel.Controls.Add(lblTitle);

            // ---------------------------------------------------------
            // SUBTITLE
            // ---------------------------------------------------------

            var lblSubtitle = new Label
            {
                Text =
                    "Add staff to Google Workspace groups by role or CSV",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9.5F),
                ForeColor =
                    Color.FromArgb(150, 160, 175),
                Location =
                    new Point(33, 62)
            };

            mainPanel.Controls.Add(lblSubtitle);

            // ---------------------------------------------------------
            // HEADER LINE
            // ---------------------------------------------------------

            var headerLine = new Panel
            {
                Location =
                    new Point(30, 90),
                Size =
                    new Size(580, 1),
                BackColor =
                    Color.FromArgb(45, 55, 70)
            };

            mainPanel.Controls.Add(headerLine);

            // ---------------------------------------------------------
            // NORMAL ADD PANEL
            // ---------------------------------------------------------

            var formPanel =
                CreatePanel(
                    new Point(30, 110),
                    new Size(580, 220));

            mainPanel.Controls.Add(formPanel);

            // STAFF TYPE

            var lblRole =
                CreateLabel(
                    "STAFF TYPE",
                    new Point(20, 16));

            formPanel.Controls.Add(lblRole);

            _cmbRole = new ComboBox
            {
                Location =
                    new Point(20, 40),

                Size =
                    new Size(540, 32),

                BackColor =
                    Color.FromArgb(20, 27, 40),

                ForeColor =
                    Color.White,

                Font =
                    new Font("Segoe UI", 11F),

                DropDownStyle =
                    ComboBoxStyle.DropDownList,

                FlatStyle =
                    FlatStyle.Flat
            };

            _cmbRole.SelectedIndexChanged +=
                CmbRole_SelectedIndexChanged;

            formPanel.Controls.Add(_cmbRole);

            // GROUP EMAIL

            var lblGroup =
                CreateLabel(
                    "GROUP EMAIL (CUSTOM ONLY)",
                    new Point(20, 88));

            formPanel.Controls.Add(lblGroup);

            _txtGroupEmail = new TextBox
            {
                Location =
                    new Point(20, 112),

                Size =
                    new Size(540, 32),

                BackColor =
                    Color.FromArgb(20, 27, 40),

                ForeColor =
                    Color.White,

                BorderStyle =
                    BorderStyle.FixedSingle,

                Font =
                    new Font("Segoe UI", 11F),

                PlaceholderText =
                    "e.g. staff-all@yourdomain.org",

                Enabled = false
            };

            formPanel.Controls.Add(_txtGroupEmail);

            // STAFF EMAIL

            var lblStaff =
                CreateLabel(
                    "STAFF EMAIL",
                    new Point(20, 158));

            formPanel.Controls.Add(lblStaff);

            _txtStaffEmail = new TextBox
            {
                Location =
                    new Point(20, 182),

                Size =
                    new Size(540, 32),

                BackColor =
                    Color.FromArgb(20, 27, 40),

                ForeColor =
                    Color.White,

                BorderStyle =
                    BorderStyle.FixedSingle,

                Font =
                    new Font("Segoe UI", 11F),

                PlaceholderText =
                    "e.g. jsmith@yourdomain.org"
            };

            _txtStaffEmail.KeyDown +=
                TxtStaffEmail_KeyDown;

            formPanel.Controls.Add(_txtStaffEmail);

            // ---------------------------------------------------------
            // ACTION BUTTONS
            // ---------------------------------------------------------

            _btnAddToGroup =
                CreateButton(
                    "ADD TO GROUP(S)",
                    new Point(30, 345),
                    new Size(187, 38));

            _btnAddToGroup.Click +=
                async (s, e) =>
                    await AddToGroupAsync();

            mainPanel.Controls.Add(
                _btnAddToGroup);

            _btnBulkAddMembers =
                CreateButton(
                    "BULK ADD MEMBERS",
                    new Point(226, 345),
                    new Size(187, 38));

            _btnBulkAddMembers.Click +=
                async (s, e) =>
                    await BulkAddMembersFromCsvAsync();

            mainPanel.Controls.Add(
                _btnBulkAddMembers);

            var btnDownloadTemplate =
                CreateButton(
                    "DOWNLOAD CSV TEMPLATE",
                    new Point(422, 345),
                    new Size(188, 38));

            btnDownloadTemplate.Click +=
                (s, e) =>
                    DownloadCsvTemplate();

            mainPanel.Controls.Add(
                btnDownloadTemplate);

            // ---------------------------------------------------------
            // BULK WARNING
            // ---------------------------------------------------------

            var lblBulkWarning = new Label
            {
                Text =
                    "IMPORTANT: The Google Group(s) must already exist before using Bulk Add Members. " +
                    "This function only adds members to existing groups.",

                Location =
                    new Point(30, 395),

                Size =
                    new Size(580, 34),

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),

                ForeColor =
                    Color.FromArgb(220, 175, 90),

                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            mainPanel.Controls.Add(
                lblBulkWarning);

            // ---------------------------------------------------------
            // STATUS
            // ---------------------------------------------------------

            var lblStatusCaption = new Label
            {
                Text = "STATUS",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    8.5F,
                    FontStyle.Bold),
                ForeColor =
                    Color.FromArgb(140, 150, 165),
                Location =
                    new Point(30, 435)
            };

            mainPanel.Controls.Add(
                lblStatusCaption);

            _lblStatus = new Label
            {
                Text = "NOT READY",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                ForeColor =
                    Color.FromArgb(140, 150, 165),
                Location =
                    new Point(90, 433)
            };

            mainPanel.Controls.Add(
                _lblStatus);

            // ---------------------------------------------------------
            // GAM OUTPUT LABEL
            // ---------------------------------------------------------

            var lblLog = new Label
            {
                Text = "GAM OUTPUT",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                ForeColor =
                    Color.FromArgb(150, 160, 175),
                Location =
                    new Point(30, 462)
            };

            mainPanel.Controls.Add(
                lblLog);

            // ---------------------------------------------------------
            // LOG
            // ---------------------------------------------------------

            _txtLog = new TextBox
            {
                Location =
                    new Point(30, 487),

                Size =
                    new Size(580, 103),

                Multiline = true,

                ReadOnly = true,

                BackColor =
                    Color.FromArgb(6, 10, 18),

                ForeColor =
                    Color.FromArgb(150, 160, 175),

                BorderStyle =
                    BorderStyle.FixedSingle,

                ScrollBars =
                    ScrollBars.Vertical,

                Font =
                    new Font("Consolas", 8.5F)
            };

            mainPanel.Controls.Add(
                _txtLog);

            _cmbRole.Focus();
        }

        // =============================================================
        // ROLE SELECTION
        // =============================================================

        private void CmbRole_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            bool isCustom =
                _cmbRole.SelectedItem as string ==
                CustomOption;

            _txtGroupEmail.Enabled =
                isCustom;

            if (!isCustom)
            {
                _txtGroupEmail.Clear();
            }
        }

        // =============================================================
        // ENTER KEY
        // =============================================================

        private void TxtStaffEmail_KeyDown(
            object? sender,
            KeyEventArgs e)
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
            string staffEmail =
                _txtStaffEmail.Text.Trim();

            string? selectedRole =
                _cmbRole.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(staffEmail))
            {
                MessageBox.Show(
                    "Please enter the staff member's email.",
                    "Staff Email Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                _txtStaffEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedRole))
            {
                MessageBox.Show(
                    "Please select a staff type.",
                    "Staff Type Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string[] groupsToAdd;

            if (selectedRole == CustomOption)
            {
                string customGroup =
                    _txtGroupEmail.Text.Trim();

                if (string.IsNullOrWhiteSpace(customGroup))
                {
                    MessageBox.Show(
                        "Please enter the group email.",
                        "Group Email Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    _txtGroupEmail.Focus();
                    return;
                }

                groupsToAdd =
                    [customGroup];
            }
            else
            {
                var template =
                    _templateConfig.Templates
                        .FirstOrDefault(
                            t => t.Name == selectedRole);

                if (template == null ||
                    template.Groups.Count == 0)
                {
                    MessageBox.Show(
                        $"The '{selectedRole}' template has no groups configured.\n\n" +
                        "Edit groupTemplates.json to add groups for this role.",
                        "No Groups Configured",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                groupsToAdd =
                    [.. template.Groups];
            }

            string groupListDisplay =
                string.Join(
                    "\n",
                    groupsToAdd);

            DialogResult confirmation =
                MessageBox.Show(
                    "Add this staff member to the following group(s)?\n\n" +
                    $"Staff:\n{staffEmail}\n\n" +
                    $"Group(s):\n{groupListDisplay}",
                    "Confirm Add to Group(s)",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes)
                return;

            SetBusy(true);

            var summary =
                new StringBuilder();

            int successCount = 0;
            int failCount = 0;

            foreach (string group in groupsToAdd)
            {
                WriteLog(
                    $"Adding {staffEmail} to {group}...");

                try
                {
                    Services.GamResult result =
                        await _gamService.AddUserToGroupAsync(
                            group,
                            staffEmail);

                    WriteLog(
                        result.CombinedOutput);

                    if (result.Success)
                    {
                        successCount++;

                        summary.AppendLine(
                            $"✔ {group}");
                    }
                    else
                    {
                        failCount++;

                        summary.AppendLine(
                            $"✘ {group} — {result.Error}");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;

                    summary.AppendLine(
                        $"✘ {group} — {ex.Message}");

                    WriteLog(
                        "ERROR: " +
                        ex.Message);
                }
            }

            SetBusy(false);

            MessageBoxIcon icon;

            if (failCount == 0)
            {
                icon =
                    MessageBoxIcon.Information;
            }
            else if (successCount == 0)
            {
                icon =
                    MessageBoxIcon.Error;
            }
            else
            {
                icon =
                    MessageBoxIcon.Warning;
            }

            MessageBox.Show(
                $"Finished adding {staffEmail}.\n\n{summary}",
                failCount == 0
                    ? "Staff Added"
                    : "Completed With Errors",
                MessageBoxButtons.OK,
                icon);

            if (failCount == 0)
            {
                _txtStaffEmail.Clear();
                _txtStaffEmail.Focus();
            }
        }

        // =============================================================
        // BULK ADD MEMBERS FROM CSV
        // =============================================================

        private async Task BulkAddMembersFromCsvAsync()
        {
            DialogResult warning =
                MessageBox.Show(
                    "IMPORTANT\n\n" +
                    "The Google Group(s) must already exist before using " +
                    "Bulk Add Members.\n\n" +
                    "This function does not create groups. It only adds " +
                    "members to existing Google Groups.\n\n" +
                    "Do you want to continue?",
                    "Bulk Add Members",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (warning != DialogResult.Yes)
                return;

            using var dialog =
                new OpenFileDialog
                {
                    Title =
                        "Select Google Groups Members CSV",

                    Filter =
                        "CSV files (*.csv)|*.csv|" +
                        "All files (*.*)|*.*",

                    FilterIndex = 1,

                    CheckFileExists = true,

                    Multiselect = false
                };

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            List<BulkGroupMember> members;

            try
            {
                members =
                    ReadBulkMembersCsv(
                        dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "The CSV file could not be read.\n\n" +
                    ex.Message,
                    "CSV Import Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            if (members.Count == 0)
            {
                MessageBox.Show(
                    "No valid member records were found in the CSV file.",
                    "No Members Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            int groupCount =
                members
                    .Select(m => m.GroupEmail)
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .Count();

            DialogResult confirmation =
                MessageBox.Show(
                    $"The CSV contains {members.Count} member record(s) " +
                    $"across {groupCount} group(s).\n\n" +
                    "The groups must already exist in Google Workspace.\n\n" +
                    "Do you want to add these members now?",
                    "Confirm Bulk Add",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (confirmation !=
                DialogResult.Yes)
            {
                return;
            }

            SetBusy(true);

            int successCount = 0;
            int failCount = 0;

            var summary =
                new StringBuilder();

            try
            {
                for (int i = 0;
                     i < members.Count;
                     i++)
                {
                    BulkGroupMember member =
                        members[i];

                    WriteLog(
                        $"[{i + 1}/{members.Count}] " +
                        $"Adding {member.MemberEmail} to " +
                        $"{member.GroupEmail}...");

                    try
                    {
                        Services.GamResult result =
                            await _gamService.AddUserToGroupAsync(
                                member.GroupEmail,
                                member.MemberEmail);

                        if (!string.IsNullOrWhiteSpace(
                            result.CombinedOutput))
                        {
                            WriteLog(
                                result.CombinedOutput);
                        }

                        if (result.Success)
                        {
                            successCount++;

                            summary.AppendLine(
                                $"✔ {member.MemberEmail} → " +
                                $"{member.GroupEmail}");
                        }
                        else
                        {
                            failCount++;

                            summary.AppendLine(
                                $"✘ {member.MemberEmail} → " +
                                $"{member.GroupEmail} — " +
                                $"{result.Error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failCount++;

                        WriteLog(
                            "ERROR: " +
                            ex.Message);

                        summary.AppendLine(
                            $"✘ {member.MemberEmail} → " +
                            $"{member.GroupEmail} — " +
                            $"{ex.Message}");
                    }
                }
            }
            finally
            {
                SetBusy(false);
            }

            MessageBoxIcon icon;

            if (failCount == 0)
            {
                icon =
                    MessageBoxIcon.Information;
            }
            else if (successCount == 0)
            {
                icon =
                    MessageBoxIcon.Error;
            }
            else
            {
                icon =
                    MessageBoxIcon.Warning;
            }

            MessageBox.Show(
                "Bulk add completed.\n\n" +
                $"Successful: {successCount}\n" +
                $"Failed: {failCount}\n\n" +
                summary,
                failCount == 0
                    ? "Bulk Add Complete"
                    : "Bulk Add Completed With Errors",
                MessageBoxButtons.OK,
                icon);
        }

        // =============================================================
        // READ BULK CSV
        // =============================================================

        private static List<BulkGroupMember> ReadBulkMembersCsv(
            string filePath)
        {
            var members =
                new List<BulkGroupMember>();

            using var parser =
                new TextFieldParser(filePath);

            parser.TextFieldType =
                FieldType.Delimited;

            parser.SetDelimiters(",");

            parser.HasFieldsEnclosedInQuotes =
                true;

            string[]? headers =
                parser.ReadFields();

            if (headers == null ||
                headers.Length == 0)
            {
                throw new InvalidDataException(
                    "The CSV file does not contain a header row.");
            }

            var headerMap =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0;
                 i < headers.Length;
                 i++)
            {
                string header =
                    headers[i].Trim();

                headerMap.TryAdd(header, i);
            }

            const string requiredGroupHeader =
                "Group Email [Required]";

            const string memberEmailHeader =
                "Member Email";

            if (!headerMap.TryGetValue(
                    requiredGroupHeader, out int groupIndex))
            {
                throw new InvalidDataException(
                    $"The required CSV column '{requiredGroupHeader}' " +
                    "was not found.");
            }

            if (!headerMap.TryGetValue(
                    memberEmailHeader, out int memberIndex))
            {
                throw new InvalidDataException(
                    $"The required CSV column '{memberEmailHeader}' " +
                    "was not found.");
            }

            int memberTypeIndex =
                headerMap.TryGetValue(
                    "Member Type",
                    out int typeIndex)
                    ? typeIndex
                    : -1;

            int memberRoleIndex =
                headerMap.TryGetValue(
                    "Member Role",
                    out int roleIndex)
                    ? roleIndex
                    : -1;

            var duplicateCheck =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            int rowNumber = 1;

            while (!parser.EndOfData)
            {
                rowNumber++;

                string[]? fields =
                    parser.ReadFields();

                if (fields == null ||
                    fields.Length == 0)
                {
                    continue;
                }

                string groupEmail =
                    GetCsvField(
                        fields,
                        groupIndex);

                string memberEmail =
                    GetCsvField(
                        fields,
                        memberIndex);

                if (string.IsNullOrWhiteSpace(
                        groupEmail) &&
                    string.IsNullOrWhiteSpace(
                        memberEmail))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                        groupEmail))
                {
                    throw new InvalidDataException(
                        $"Row {rowNumber}: Group Email [Required] " +
                        "cannot be blank.");
                }

                if (string.IsNullOrWhiteSpace(
                        memberEmail))
                {
                    throw new InvalidDataException(
                        $"Row {rowNumber}: Member Email " +
                        "cannot be blank.");
                }

                if (!IsValidEmail(groupEmail))
                {
                    throw new InvalidDataException(
                        $"Row {rowNumber}: '{groupEmail}' " +
                        "does not appear to be a valid group email.");
                }

                if (!IsValidEmail(memberEmail))
                {
                    throw new InvalidDataException(
                        $"Row {rowNumber}: '{memberEmail}' " +
                        "does not appear to be a valid member email.");
                }

                string memberType =
                    GetCsvField(
                        fields,
                        memberTypeIndex);

                string memberRole =
                    GetCsvField(
                        fields,
                        memberRoleIndex);

                string duplicateKey =
                    groupEmail +
                    "\n" +
                    memberEmail;

                if (!duplicateCheck.Add(
                        duplicateKey))
                {
                    continue;
                }

                members.Add(
                    new BulkGroupMember
                    {
                        GroupEmail =
                            groupEmail,

                        MemberEmail =
                            memberEmail,

                        MemberType =
                            memberType,

                        MemberRole =
                            memberRole
                    });
            }

            return members;
        }

        private static string GetCsvField(
            string[] fields,
            int index)
        {
            if (index < 0 ||
                index >= fields.Length)
            {
                return string.Empty;
            }

            return fields[index].Trim();
        }

        private static bool IsValidEmail(
            string email)
        {
            try
            {
                var address =
                    new System.Net.Mail.MailAddress(
                        email);

                return string.Equals(
                    address.Address,
                    email,
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        // =============================================================
        // DOWNLOAD CSV TEMPLATE
        // =============================================================

        private void DownloadCsvTemplate()
        {
            using var dialog =
                new SaveFileDialog
                {
                    Title =
                        "Save Google Groups CSV Template",

                    FileName =
                        "GoogleGroupsMembersTemplate.csv",

                    Filter =
                        "CSV files (*.csv)|*.csv|" +
                        "All files (*.*)|*.*",

                    FilterIndex = 1,

                    AddExtension = true,

                    OverwritePrompt = true
                };

            if (dialog.ShowDialog(this) !=
                DialogResult.OK)
            {
                return;
            }

            try
            {
                string[] headers =
                [
                    "Group Email [Required]",
                    "Member Email",
                    "Member Type",
                    "Member Role"
                ];

                File.WriteAllText(
                    dialog.FileName,
                    string.Join(
                        ",",
                        headers) +
                    Environment.NewLine,
                    new UTF8Encoding(true));

                MessageBox.Show(
                    "CSV template saved successfully.",
                    "Template Downloaded",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                WriteLog(
                    "CSV template saved to: " +
                    dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "The CSV template could not be saved.\n\n" +
                    ex.Message,
                    "Template Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =============================================================
        // BUSY STATE
        // =============================================================

        private void SetBusy(bool busy)
        {
            _btnAddToGroup.Enabled =
                !busy;

            _btnBulkAddMembers.Enabled =
                !busy;

            _cmbRole.Enabled =
                !busy;

            _txtGroupEmail.Enabled =
                !busy &&
                (_cmbRole.SelectedItem as string ==
                 CustomOption);

            _txtStaffEmail.Enabled =
                !busy;

            Cursor =
                busy
                    ? Cursors.WaitCursor
                    : Cursors.Default;
        }

        // =============================================================
        // LOGGING
        // =============================================================

        private void WriteLog(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (IsDisposed ||
                !IsHandleCreated)
            {
                return;
            }

            if (_txtLog.TextLength > 0)
            {
                _txtLog.AppendText(
                    Environment.NewLine +
                    Environment.NewLine);
            }

            _txtLog.AppendText(text);

            _txtLog.SelectionStart =
                _txtLog.TextLength;

            _txtLog.ScrollToCaret();
        }

        // =============================================================
        // UI HELPERS
        // =============================================================

        private static HudPanel CreatePanel(
            Point location,
            Size size)
        {
            return new HudPanel
            {
                Location = location,
                Size = size
            };
        }

        private static Label CreateLabel(
            string text,
            Point location)
        {
            return new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                Font = new Font(
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

        private static HudButton CreateButton(
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
        // BULK CSV MODEL
        // =============================================================

        private sealed class BulkGroupMember
        {
            public string GroupEmail { get; init; } =
                string.Empty;

            public string MemberEmail { get; init; } =
                string.Empty;

            public string MemberType { get; init; } =
                string.Empty;

            public string MemberRole { get; init; } =
                string.Empty;
        }
    }
}
