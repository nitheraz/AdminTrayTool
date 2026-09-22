using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdminTrayTool
{
    public class WindowsManagementForm : Form
    {
        private Button _btnActiveDirectory = null!;
        private Button _btnMecm = null!;

        public WindowsManagementForm()
        {
            InitializeForm();
            BuildUi();
        }

        private void InitializeForm()
        {
            Text = "Windows Management";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(910, 600);

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
                Text = "WINDOWS MANAGEMENT",
                Location = new Point(25, 20),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    18F,
                    FontStyle.Bold),
                ForeColor = Color.White
            };

            Controls.Add(header);

            var subtitle = new Label
            {
                Text =
                    "Manage Windows computers, Active Directory and MECM",
                Location = new Point(27, 55),
                AutoSize = true,
                ForeColor = Color.LightGray
            };

            Controls.Add(subtitle);

            // Active Directory
            var adPanel = new Panel
            {
                Location = new Point(25, 105),
                Size = new Size(860, 170),
                BackColor = Color.FromArgb(20, 27, 40)
            };

            Controls.Add(adPanel);

            var adTitle = new Label
            {
                Text = "ACTIVE DIRECTORY",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    12F,
                    FontStyle.Bold)
            };

            adPanel.Controls.Add(adTitle);

            var adDescription = new Label
            {
                Text =
                    "Find computers, view Active Directory information " +
                    "and move computers between organisational units.",
                Location = new Point(20, 55),
                Size = new Size(570, 45),
                ForeColor = Color.LightGray
            };

            adPanel.Controls.Add(adDescription);

            _btnActiveDirectory = new Button
            {
                Text = "OPEN AD MANAGEMENT",
                Location = new Point(630, 65),
                Size = new Size(190, 40),
                BackColor = Color.FromArgb(40, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            _btnActiveDirectory.FlatAppearance.BorderColor =
                Color.FromArgb(80, 100, 130);

            _btnActiveDirectory.Click +=
                BtnActiveDirectory_Click;

            adPanel.Controls.Add(
                _btnActiveDirectory);

            // MECM
            var mecmPanel = new Panel
            {
                Location = new Point(25, 300),
                Size = new Size(860, 170),
                BackColor = Color.FromArgb(20, 27, 40)
            };

            Controls.Add(mecmPanel);

            var mecmTitle = new Label
            {
                Text = "MECM / SCCM",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    12F,
                    FontStyle.Bold)
            };

            mecmPanel.Controls.Add(mecmTitle);

            var mecmDescription = new Label
            {
                Text =
                    "Find computers in MECM and manage collection " +
                    "membership.",
                Location = new Point(20, 55),
                Size = new Size(570, 45),
                ForeColor = Color.LightGray
            };

            mecmPanel.Controls.Add(
                mecmDescription);

            _btnMecm = new Button
            {
                Text = "OPEN MECM MANAGEMENT",
                Location = new Point(630, 65),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(40, 50, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            _btnMecm.FlatAppearance.BorderColor =
                Color.FromArgb(80, 100, 130);

            _btnMecm.Click +=
                BtnMecm_Click;

            mecmPanel.Controls.Add(_btnMecm);

            var closeButton = new Button
            {
                Text = "CLOSE",
                Location = new Point(755, 510),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(35, 40, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            closeButton.FlatAppearance.BorderColor =
                Color.FromArgb(70, 80, 95);

            closeButton.Click +=
                (_, _) => Close();

            Controls.Add(closeButton);
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
                    $"Failed to open Active Directory Management:" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    ex.Message,
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
    }
}