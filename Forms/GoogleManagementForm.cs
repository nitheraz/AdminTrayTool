using AdminTrayTool.UI;
using System.Drawing;
using System.Windows.Forms;

namespace AdminTrayTool.Forms
{
    public class GoogleManagementForm : Form
    {
        private Button _btnChromebookManagement = null!;
        private Button _btnGroupManagement = null!;

        public GoogleManagementForm()
        {
            InitializeForm();
            BuildInterface();
        }

        private void InitializeForm()
        {
            Text = "Google Management";
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
                Text = "GOOGLE MANAGEMENT",
                Dock = DockStyle.Top,
                Height = 45,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var descriptionLabel = new Label
            {
                Text = "Manage Google Workspace Chromebooks and Google Groups.",
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

            // =========================================================
            // CHROMEBOOK MANAGEMENT
            // =========================================================

            var chromebookPanel = new HudPanel
            {
                Location = new Point(0, 20),
                Size = new Size(400, 250)
            };

            var chromebookTitle = new Label
            {
                Text = "CHROMEBOOK MANAGEMENT",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White
            };

            var chromebookDescription = new Label
            {
                Text =
                    "Look up Chromebooks, view device\\n" +
                    "details, manage users and OUs,\\n" +
                    "and perform device actions.",
                Location = new Point(20, 65),
                Size = new Size(350, 75),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.LightGray
            };

            _btnChromebookManagement = new HudButton
            {
                Text = "Chromebook Management",
                Location = new Point(20, 165),
                Size = new Size(350, 45)
            };

            _btnChromebookManagement.Click +=
                BtnChromebookManagement_Click;

            chromebookPanel.Controls.Add(chromebookTitle);
            chromebookPanel.Controls.Add(chromebookDescription);
            chromebookPanel.Controls.Add(_btnChromebookManagement);

            // =========================================================
            // GOOGLE GROUP MANAGEMENT
            // =========================================================

            var groupPanel = new HudPanel
            {
                Location = new Point(420, 20),
                Size = new Size(400, 250)
            };

            var groupTitle = new Label
            {
                Text = "GOOGLE GROUPS",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White
            };

            var groupDescription = new Label
            {
                Text =
                    "Manage Google Groups, view group\\n" +
                    "members, and add or remove\\n" +
                    "members using bulk operations.",
                Location = new Point(20, 65),
                Size = new Size(350, 75),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.LightGray
            };

            _btnGroupManagement = new HudButton
            {
                Text = "Google Group Management",
                Location = new Point(20, 165),
                Size = new Size(350, 45)
            };

            _btnGroupManagement.Click +=
                BtnGroupManagement_Click;

            groupPanel.Controls.Add(groupTitle);
            groupPanel.Controls.Add(groupDescription);
            groupPanel.Controls.Add(_btnGroupManagement);

            toolsPanel.Controls.Add(chromebookPanel);
            toolsPanel.Controls.Add(groupPanel);

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
        }

        private void BtnChromebookManagement_Click(
            object? sender,
            EventArgs e)
        {
            try
            {
                using var form =
                    new ChromebookManagementForm();

                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open Chromebook Management:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Chromebook Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnGroupManagement_Click(
            object? sender,
            EventArgs e)
        {
            try
            {
                using var form =
                    new GroupManagementForm();

                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open Google Group Management:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Google Group Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
