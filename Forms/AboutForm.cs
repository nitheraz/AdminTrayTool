using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace AdminTrayTool
{
    public class AboutForm : Form
    {
        private const string RepoUrl = "https://github.com/nitheraz/AdminTrayTool";

        public AboutForm()
        {
            InitializeForm();
            BuildInterface();
        }

        private void InitializeForm()
        {
            Text = "About AdminTrayTool";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(420, 280);
            MinimumSize = new Size(420, 280);
            MaximumSize = new Size(420, 280);
            BackColor = Color.FromArgb(10, 15, 25);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
        }

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
                Text = "ADMINTRAYTOOL",
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 20)
            };
            mainPanel.Controls.Add(lblTitle);

            string version = GetAppVersion();

            var lblVersion = new Label
            {
                Text = $"Version {version}",
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = Color.FromArgb(150, 160, 175),
                Location = new Point(33, 58)
            };
            mainPanel.Controls.Add(lblVersion);

            var headerLine = new Panel
            {
                Location = new Point(30, 90),
                Size = new Size(360, 1),
                BackColor = Color.FromArgb(45, 55, 70)
            };
            mainPanel.Controls.Add(headerLine);

            var lblDescription = new Label
            {
                Text = "IT Admin Quick Tools for Google Workspace management,\n" +
                       "Chromebook administration, and remote admin utilities.",
                AutoSize = false,
                Size = new Size(360, 50),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.White,
                Location = new Point(30, 105)
            };
            mainPanel.Controls.Add(lblDescription);

            var lblRepo = new LinkLabel
            {
                Text = RepoUrl,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                LinkColor = Color.FromArgb(100, 170, 255),
                Location = new Point(30, 165)
            };
            lblRepo.LinkClicked += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = RepoUrl, UseShellExecute = true });
                }
                catch { /* ignore - non-critical */ }
            };
            mainPanel.Controls.Add(lblRepo);

            var btnCheckUpdate = new Button
            {
                Text = "CHECK FOR UPDATES",
                Location = new Point(30, 200),
                Size = new Size(200, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 35, 52),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCheckUpdate.FlatAppearance.BorderColor = Color.FromArgb(65, 85, 110);
            btnCheckUpdate.FlatAppearance.BorderSize = 1;
            btnCheckUpdate.Click += async (s, e) =>
            {
                btnCheckUpdate.Enabled = false;
                btnCheckUpdate.Text = "CHECKING...";
                await UpdateCheckService.CheckForUpdateAsync(version, showUpToDateMessage: true);
                btnCheckUpdate.Enabled = true;
                btnCheckUpdate.Text = "CHECK FOR UPDATES";
            };
            mainPanel.Controls.Add(btnCheckUpdate);
        }

        private static string GetAppVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version == null)
                return "Unknown";

            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}