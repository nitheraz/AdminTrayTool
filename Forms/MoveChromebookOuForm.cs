using AdminTrayTool.Services;
using AdminTrayTool.UI;

namespace AdminTrayTool.Forms
{
    public class MoveChromebookOuForm : Form
    {
        private readonly GamService _gamService;

        private ComboBox _cmbOrgUnits = null!;
        private Button _btnMove = null!;
        private Button _btnCancel = null!;
        private Label _lblStatus = null!;

        public string? SelectedOrgUnitPath { get; private set; }

        public MoveChromebookOuForm(string currentOu)
        {
            _gamService = new GamService();

            InitializeForm();

            BuildInterface();

            Shown +=
                async (s, e) => await LoadOrgUnitsAsync();
        }

        private void InitializeForm()
        {
            Text = "Move Chromebook";

            StartPosition =
                FormStartPosition.CenterParent;

            ClientSize =
                new Size(575, 270);

            MinimumSize =
                new Size(575, 270);

            MaximumSize =
                new Size(570, 270);

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
                    10F);

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;
        }

        private void BuildInterface()
        {
            var panel = new HudPanel
            {
                Location =
                    new Point(20, 20),

                Size =
                    new Size(520, 190)
            };

            Controls.Add(panel);

            var lblTitle = new Label
            {
                Text =
                    "MOVE CHROMEBOOK TO ORGANISATIONAL UNIT",

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

            panel.Controls.Add(lblTitle);

            var lblOu = new Label
            {
                Text =
                    "SELECT ORGANISATIONAL UNIT",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),

                ForeColor =
                    Color.FromArgb(
                        140,
                        150,
                        165),

                Location =
                    new Point(
                        20,
                        55)
            };

            panel.Controls.Add(lblOu);

            _cmbOrgUnits = new ComboBox
            {
                Location =
                    new Point(
                        20,
                        80),

                Size =
                    new Size(
                        480,
                        32),

                BackColor =
                    Color.FromArgb(
                        20,
                        27,
                        40),

                ForeColor =
                    Color.White,

                Font =
                    new Font(
                        "Segoe UI",
                        10F),

                DropDownStyle =
                    ComboBoxStyle.DropDownList,

                FlatStyle =
                    FlatStyle.Flat,

                Enabled = false
            };

            panel.Controls.Add(
                _cmbOrgUnits);

            _lblStatus = new Label
            {
                Text =
                    "Loading organisational units...",

                AutoSize = true,

                Font =
                    new Font(
                        "Segoe UI",
                        8.5F),

                ForeColor =
                    Color.FromArgb(
                        140,
                        150,
                        165),

                Location =
                    new Point(
                        20,
                        118)
            };

            panel.Controls.Add(
                _lblStatus);

            _btnCancel = new HudButton
            {
                Text = "CANCEL",

                Location =
                    new Point(
                        300,
                        145),

                Size =
                    new Size(
                        95,
                        35)
            };

            _btnCancel.Click +=
                (s, e) =>
                {
                    DialogResult =
                        DialogResult.Cancel;

                    Close();
                };

            panel.Controls.Add(
                _btnCancel);

            _btnMove = new HudButton
            {
                Text = "MOVE",

                Location =
                    new Point(
                        405,
                        145),

                Size =
                    new Size(
                        95,
                        35),

                Enabled = false
            };

            _btnMove.Click +=
                BtnMove_Click;

            panel.Controls.Add(
                _btnMove);
        }

        private async Task LoadOrgUnitsAsync()
        {
            try
            {
                var (Success, OrgUnitPaths, Error) =
                    await GamService.GetAllOrgUnitPathsAsync();

                if (!Success)
                {
                    _lblStatus.Text =
                        "Unable to load organisational units.";

                    MessageBox.Show(
                        Error,
                        "Load Organisational Units Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                _cmbOrgUnits.Items.Clear();

                foreach (string path in OrgUnitPaths)
                {
                    _cmbOrgUnits.Items.Add(path);
                }

                if (_cmbOrgUnits.Items.Count == 0)
                {
                    _lblStatus.Text =
                        "No organisational units were found.";

                    return;
                }

                _cmbOrgUnits.Enabled = true;
                _btnMove.Enabled = true;

                _lblStatus.Text =
                    $"{_cmbOrgUnits.Items.Count} organisational units loaded.";
            }
            catch (Exception ex)
            {
                _lblStatus.Text =
                    "Error loading organisational units.";

                MessageBox.Show(
                    ex.Message,
                    "Organisational Unit Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnMove_Click(
            object? sender,
            EventArgs e)
        {
            if (_cmbOrgUnits.SelectedItem is not string selectedOu)
            {
                MessageBox.Show(
                    "Please select an organisational unit.",
                    "Organisational Unit Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            SelectedOrgUnitPath =
                selectedOu;

            DialogResult =
                DialogResult.OK;

            Close();
        }
    }
}
