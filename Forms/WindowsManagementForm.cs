namespace AdminTrayTool
{
    public class WindowsManagementForm : Form
    {
        private Button _btnActiveDirectory = null!;
        private Button _btnMecm = null!;

        public WindowsManagementForm()
        {
            InitializeForm();
            BuildInterface();
        }

        // =============================================================
        // FORM INITIALISATION
        // =============================================================

        private void InitializeForm()
        {
            Text = "Windows Management";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(890, 600);

            MinimumSize =
                new Size(820, 550);

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
                    "WINDOWS MANAGEMENT",

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
                    "Manage Windows computers, Active Directory and MECM",

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
            // ACTIVE DIRECTORY
            // =========================================================

            var adPanel = CreatePanel(
                new Point(30, 105),
                new Size(820, 145));

            mainPanel.Controls.Add(adPanel);

            var lblAdTitle = new Label
            {
                Text =
                    "ACTIVE DIRECTORY",

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

            adPanel.Controls.Add(lblAdTitle);

            var lblAdDescription = new Label
            {
                Text =
                    "Find Windows computers, view Active Directory information " +
                    "and move computers between organisational units.",

                Location =
                    new Point(
                        20,
                        52),

                Size =
                    new Size(
                        520,
                        45),

                Font =
                    new Font(
                        "Segoe UI",
                        9.5F),

                ForeColor =
                    Color.FromArgb(
                        150,
                        160,
                        175)
            };

            adPanel.Controls.Add(lblAdDescription);

            var lblAdFeatures = new Label
            {
                Text =
                    "Computer search  •  AD information  •  OU management",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F),

                ForeColor =
                    Color.FromArgb(
                        120,
                        135,
                        150),

                Location =
                    new Point(
                        20,
                        105)
            };

            adPanel.Controls.Add(lblAdFeatures);

            _btnActiveDirectory = CreateButton(
                "OPEN AD MANAGEMENT",
                new Point(590, 52),
                new Size(200, 36));

            _btnActiveDirectory.Click +=
                BtnActiveDirectory_Click;

            adPanel.Controls.Add(
                _btnActiveDirectory);

            // =========================================================
            // MECM / SCCM
            // =========================================================

            var mecmPanel = CreatePanel(
                new Point(30, 265),
                new Size(820, 145));

            mainPanel.Controls.Add(mecmPanel);

            var lblMecmTitle = new Label
            {
                Text =
                    "MECM / SCCM",

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

            mecmPanel.Controls.Add(lblMecmTitle);

            var lblMecmDescription = new Label
            {
                Text =
                    "Find Windows computers in Microsoft Endpoint Configuration " +
                    "Manager and manage collection membership.",

                Location =
                    new Point(
                        20,
                        52),

                Size =
                    new Size(
                        520,
                        45),

                Font =
                    new Font(
                        "Segoe UI",
                        9.5F),

                ForeColor =
                    Color.FromArgb(
                        150,
                        160,
                        175)
            };

            mecmPanel.Controls.Add(lblMecmDescription);

            var lblMecmFeatures = new Label
            {
                Text =
                    "Computer search  •  Device information  •  Collections",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F),

                ForeColor =
                    Color.FromArgb(
                        120,
                        135,
                        150),

                Location =
                    new Point(
                        20,
                        105)
            };

            mecmPanel.Controls.Add(lblMecmFeatures);

            _btnMecm = CreateButton(
                "OPEN MECM MANAGEMENT",
                new Point(570, 52),
                new Size(220, 36));

            _btnMecm.Click +=
                BtnMecm_Click;

            mecmPanel.Controls.Add(
                _btnMecm);

            // =========================================================
            // FORM ACTIONS
            // =========================================================

            var actionPanel = CreatePanel(
                new Point(30, 425),
                new Size(820, 80));

            mainPanel.Controls.Add(actionPanel);

            var lblFormActions = new Label
            {
                Text =
                    "FORM ACTIONS",

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
                        150),

                Location =
                    new Point(
                        20,
                        10)
            };

            actionPanel.Controls.Add(lblFormActions);

            var closeButton = CreateButton(
                "CLOSE",
                new Point(20, 32),
                new Size(110, 34));

            closeButton.Click +=
                (_, _) => Close();

            actionPanel.Controls.Add(
                closeButton);
        }

        // =============================================================
        // ACTIVE DIRECTORY
        // =============================================================

        private void BtnActiveDirectory_Click(
            object? sender,
            EventArgs e)
        {
            try
            {
                using var form =
                    new ActiveDirectoryManagementForm();

                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open Active Directory Management:" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    ex.Message,
                    "Active Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =============================================================
        // MECM
        // =============================================================

        private void BtnMecm_Click(
            object? sender,
            EventArgs e)
        {
            try
            {
                using var form =
                    new MecmManagementForm();

                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open MECM Management:" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    ex.Message,
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
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
    }
}