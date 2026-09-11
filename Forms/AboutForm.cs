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
            ClientSize = new Size(420, 260);
            MinimumSize = new Size(420, 260);
            MaximumSize = new Size(420, 290);
            BackColor = Color.FromArgb(10, 15, 25);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
        }

        private void BuildInterface()
        {
            var mainPanel = new HudPanel
            {
                Location = new Point(6, 6),
                Size = new Size(ClientSize.Width - 12, ClientSize.Height - 12),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Padding = new Padding(24)
            };
            Controls.Add(mainPanel);

            var lblTitle = new Label
            {
                Text = "ADMINTRAYTOOL",
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(24, 20),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(lblTitle);

            string version = GetAppVersion();

            var lblVersion = new Label
            {
                Text = $"Version {version}",
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = Color.FromArgb(150, 190, 220),
                Location = new Point(27, 58),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(lblVersion);

            var headerLine = new Panel
            {
                Location = new Point(24, 90),
                Size = new Size(354, 1),
                BackColor = Color.FromArgb(120, 230, 255)
            };
            mainPanel.Controls.Add(headerLine);

            var lblDescription = new Label
            {
                Text = "IT Admin Quick Tools for Google Workspace management,\n" +
                       "Chromebook administration, and remote admin utilities.",
                AutoSize = false,
                Size = new Size(354, 50),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.White,
                Location = new Point(24, 105),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(lblDescription);

            var lblRepo = new LinkLabel
            {
                Text = RepoUrl,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                LinkColor = Color.FromArgb(120, 230, 255),
                Location = new Point(24, 165),
                BackColor = Color.Transparent
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

            var btnCheckUpdate = new HudButton
            {
                Text = "CHECK FOR UPDATES",
                Location = new Point(24, 200),
                Size = new Size(200, 32)
            };
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