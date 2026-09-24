using AdminTrayTool.Models;
using AdminTrayTool.UI;
using System.Drawing;
using System.Windows.Forms;

namespace AdminTrayTool.Forms
{
    public class WindowsManagementForm : Form
    {
        private readonly AppConfig _config;

        private Button _btnActiveDirectory = null!;
        private Button _btnMecm = null!;

        public WindowsManagementForm(AppConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            InitializeForm();
            BuildInterface();
        }

        private void InitializeForm()
        {
            Text = "Windows Management";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(890, 480);
            MinimumSize = new Size(890, 480);

            BackColor = Color.FromArgb(10, 15, 25);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        }

        private void BuildInterface()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25),
                BackColor = Color.FromArgb(10, 15, 25)
            };

            var headerLabel = new Label
            {
                Text = "WINDOWS MANAGEMENT",
                Dock = DockStyle.Top,
                Height = 45,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var descriptionLabel = new Label
            {
                Text = "Manage Windows computers using Active Directory and Microsoft Endpoint Configuration Manager.",
                Dock = DockStyle.Top,
                Height = 45,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var toolsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 20, 0, 20),
                BackColor = Color.Transparent
            };

            var adPanel = new HudPanel
            {
                Location = new Point(0, 20),
                Size = new Size(400, 250)
            };

            var adTitle = new Label
            {
                Text = "ACTIVE DIRECTORY",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White
            };

            var adDescription = new Label
            {
                Text =
                    "Look up Windows computers,\\n" +
                    "view their details, and move\\n" +
                    "computers between OUs.",
                Location = new Point(20, 65),
                Size = new Size(350, 75),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.LightGray
            };

            _btnActiveDirectory = new HudButton
            {
                Text = "OPEN ACTIVE DIRECTORY",
                Location = new Point(20, 165),
                Size = new Size(350, 45)
            };

            _btnActiveDirectory.Click += BtnActiveDirectory_Click;

            adPanel.Controls.Add(adTitle);
            adPanel.Controls.Add(adDescription);
            adPanel.Controls.Add(_btnActiveDirectory);

            var mecmPanel = new HudPanel
            {
                Location = new Point(420, 20),
                Size = new Size(400, 250)
            };

            var mecmTitle = new Label
            {
                Text = "MECM",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White
            };

            var mecmDescription = new Label
            {
                Text =
                    "Look up Windows devices,\\n" +
                    "view collections, and manage\\n" +
                    "direct collection membership.",
                Location = new Point(20, 65),
                Size = new Size(350, 75),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.LightGray
            };

            _btnMecm = new HudButton
            {
                Text = "OPEN MECM",
                Location = new Point(20, 165),
                Size = new Size(350, 45)
            };

            _btnMecm.Click += BtnMecm_Click;

            mecmPanel.Controls.Add(mecmTitle);
            mecmPanel.Controls.Add(mecmDescription);
            mecmPanel.Controls.Add(_btnMecm);

            toolsPanel.Controls.Add(adPanel);
            toolsPanel.Controls.Add(mecmPanel);

            var actionsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = Color.Transparent
            };

            mainPanel.Controls.Add(toolsPanel);
            mainPanel.Controls.Add(actionsPanel);
            mainPanel.Controls.Add(descriptionLabel);
            mainPanel.Controls.Add(headerLabel);

            Controls.Add(mainPanel);

            UpdateMecmButtonState();
        }

        private void UpdateMecmButtonState()
        {
            bool configured =
                _config.Mecm?.Enabled == true &&
                !string.IsNullOrWhiteSpace(_config.Mecm.SiteCode) &&
                !string.IsNullOrWhiteSpace(_config.Mecm.SmsProviderServer);

            _btnMecm.Enabled = configured;

            if (!configured)
            {
                _btnMecm.Text = "MECM NOT CONFIGURED";
            }
        }

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
                    $"Failed to open Active Directory Management:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Active Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnMecm_Click(
            object? sender,
            EventArgs e)
        {
            try
            {
                if (_config.Mecm?.Enabled != true)
                {
                    MessageBox.Show(
                        "MECM is not enabled in the AdminTrayTool configuration.",
                        "MECM",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                using var form =
                    new MecmManagementForm(
                        _config.Mecm);

                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open MECM Management:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
