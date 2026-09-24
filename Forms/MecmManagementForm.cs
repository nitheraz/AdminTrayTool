using AdminTrayTool.Models;
using AdminTrayTool.Services;

namespace AdminTrayTool.Forms
{
    public class MecmManagementForm : Form
    {
        private readonly MecmManagementService
            _mecmManagementService;

        private TextBox _txtComputerName = null!;
        private Button _btnLookup = null!;

        private Label _lblComputerName = null!;
        private Label _lblResourceId = null!;
        private Label _lblClient = null!;
        private Label _lblClientVersion = null!;
        private Label _lblOperatingSystem = null!;
        private Label _lblManufacturer = null!;
        private Label _lblModel = null!;
        private Label _lblSerialNumber = null!;

        private ListBox _lstCurrentCollections = null!;

        private TextBox _txtCollectionFilter = null!;
        private ComboBox _cmbCollections = null!;
        private Button _btnAdd = null!;
        private Button _btnRemove = null!;

        private TextBox _txtActivity = null!;

        private readonly List<CollectionItem> _allCollections = [];

        private string _currentComputerName =
            string.Empty;

        private string _currentResourceId =
            string.Empty;

        public MecmManagementForm(
            MecmConfig config)
        {
            _mecmManagementService =
                new MecmManagementService(config);

            InitializeForm();
            BuildInterface();
        }

        private void InitializeForm()
        {
            Text = "MECM Management";

            StartPosition =
                FormStartPosition.CenterParent;

            ClientSize =
                new Size(980, 730);

            MinimumSize =
                new Size(900, 650);

            BackColor =
                Color.FromArgb(10, 15, 25);

            ForeColor =
                Color.White;

            Font =
                new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Regular);
        }

        private void BuildInterface()
        {
            var mainPanel =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(15),
                    BackColor =
                        Color.FromArgb(10, 15, 25)
                };

            Controls.Add(mainPanel);

            // ========================================================
            // HEADER
            // ========================================================

            var headerLabel =
                new Label
                {
                    Text = "MECM MANAGEMENT",
                    Left = 15,
                    Top = 10,
                    Width = 400,
                    Height = 32,
                    Font =
                        new Font(
                            "Segoe UI",
                            16F,
                            FontStyle.Bold),
                    ForeColor = Color.White
                };

            mainPanel.Controls.Add(
                headerLabel);

            // ========================================================
            // COMPUTER LOOKUP PANEL
            // ========================================================

            var lookupPanel =
                new HudPanel
                {
                    Left = 15,
                    Top = 50,
                    Width = 940,
                    Height = 75
                };

            var lblComputer =
                new Label
                {
                    Text = "Computer Name:",
                    Left = 20,
                    Top = 22,
                    Width = 130,
                    ForeColor = Color.White
                };

            _txtComputerName =
                new TextBox
                {
                    Left = 150,
                    Top = 18,
                    Width = 350,
                    Height = 30
                };

            _btnLookup =
                new HudButton
                {
                    Text = "LOOKUP",
                    Left = 520,
                    Top = 16,
                    Width = 120,
                    Height = 34
                };

            _btnLookup.Click +=
                async (s, e) =>
                    await LookupComputerAsync();

            lookupPanel.Controls.Add(
                lblComputer);

            lookupPanel.Controls.Add(
                _txtComputerName);

            lookupPanel.Controls.Add(
                _btnLookup);

            mainPanel.Controls.Add(
                lookupPanel);

            // ========================================================
            // COMPUTER INFORMATION PANEL
            // ========================================================

            var computerPanel =
                new HudPanel
                {
                    Left = 15,
                    Top = 140,
                    Width = 455,
                    Height = 285
                };

            var computerTitle =
                CreatePanelTitle(
                    "COMPUTER INFORMATION");

            computerPanel.Controls.Add(
                computerTitle);

            _lblComputerName =
                CreateInfoLabel(
                    "Computer:",
                    20,
                    50);

            _lblResourceId =
                CreateInfoLabel(
                    "Resource ID:",
                    20,
                    78);

            _lblClient =
                CreateInfoLabel(
                    "Client:",
                    20,
                    106);

            _lblClientVersion =
                CreateInfoLabel(
                    "Client Version:",
                    20,
                    134);

            _lblOperatingSystem =
                CreateInfoLabel(
                    "Operating System:",
                    20,
                    162);

            _lblManufacturer =
                CreateInfoLabel(
                    "Manufacturer:",
                    20,
                    190);

            _lblModel =
                CreateInfoLabel(
                    "Model:",
                    20,
                    218);

            _lblSerialNumber =
                CreateInfoLabel(
                    "Serial Number:",
                    20,
                    246);

            computerPanel.Controls.Add(
                _lblComputerName);

            computerPanel.Controls.Add(
                _lblResourceId);

            computerPanel.Controls.Add(
                _lblClient);

            computerPanel.Controls.Add(
                _lblClientVersion);

            computerPanel.Controls.Add(
                _lblOperatingSystem);

            computerPanel.Controls.Add(
                _lblManufacturer);

            computerPanel.Controls.Add(
                _lblModel);

            computerPanel.Controls.Add(
                _lblSerialNumber);

            mainPanel.Controls.Add(
                computerPanel);

            // ========================================================
            // CURRENT COLLECTION MEMBERSHIP PANEL
            // ========================================================

            var membershipPanel =
                new HudPanel
                {
                    Left = 485,
                    Top = 140,
                    Width = 470,
                    Height = 285
                };

            var membershipTitle =
                CreatePanelTitle(
                    "CURRENT COLLECTION MEMBERSHIP");

            membershipPanel.Controls.Add(
                membershipTitle);

            _lstCurrentCollections =
                new ListBox
                {
                    Left = 20,
                    Top = 52,
                    Width = 425,
                    Height = 210,

                    BackColor =
                        Color.FromArgb(
                            16,
                            23,
                            36),

                    ForeColor =
                        Color.White,

                    BorderStyle =
                        BorderStyle.FixedSingle
                };

            membershipPanel.Controls.Add(
                _lstCurrentCollections);

            mainPanel.Controls.Add(
                membershipPanel);

            // ========================================================
            // COLLECTION ACTION PANEL
            // ========================================================

            var actionPanel =
                new HudPanel
                {
                    Left = 15,
                    Top = 440,
                    Width = 940,
                    Height = 105
                };

            var actionTitle =
                CreatePanelTitle(
                    "COLLECTION ACTIONS");

            actionPanel.Controls.Add(
                actionTitle);

            // ========================================================
            // COLLECTION FILTER
            // ========================================================

            var lblFilter =
                new Label
                {
                    Text = "Filter:",
                    Left = 20,
                    Top = 52,
                    Width = 55,
                    ForeColor = Color.White
                };

            _txtCollectionFilter =
                new TextBox
                {
                    Left = 75,
                    Top = 48,
                    Width = 250,
                    Height = 28
                };

            _txtCollectionFilter.TextChanged +=
                (s, e) =>
                    FilterCollections();

            actionPanel.Controls.Add(
                lblFilter);

            actionPanel.Controls.Add(
                _txtCollectionFilter);

            // ========================================================
            // COLLECTION
            // ========================================================

            var lblCollections =
                new Label
                {
                    Text = "Collection:",
                    Left = 340,
                    Top = 52,
                    Width = 80,
                    ForeColor = Color.White
                };

            _cmbCollections =
                new ComboBox
                {
                    Left = 420,
                    Top = 48,
                    Width = 280,

                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    BackColor =
                        Color.FromArgb(
                            25,
                            35,
                            50),

                    ForeColor =
                        Color.White
                };

            _btnAdd =
                new HudButton
                {
                    Text = "ADD TO COLLECTION",
                    Left = 715,
                    Top = 46,
                    Width = 125,
                    Height = 34
                };

            _btnRemove =
                new HudButton
                {
                    Text = "REMOVE",
                    Left = 850,
                    Top = 46,
                    Width = 80,
                    Height = 34
                };

            _btnAdd.Click +=
                async (s, e) =>
                    await AddToCollectionAsync();

            _btnRemove.Click +=
                async (s, e) =>
                    await RemoveFromCollectionAsync();

            actionPanel.Controls.Add(
                lblCollections);

            actionPanel.Controls.Add(
                _cmbCollections);

            actionPanel.Controls.Add(
                _btnAdd);

            actionPanel.Controls.Add(
                _btnRemove);

            mainPanel.Controls.Add(
                actionPanel);

            // ========================================================
            // ACTIVITY PANEL
            // ========================================================

            var activityPanel =
                new HudPanel
                {
                    Left = 15,
                    Top = 560,
                    Width = 940,
                    Height = 145
                };

            var activityTitle =
                CreatePanelTitle(
                    "ACTIVITY");

            activityPanel.Controls.Add(
                activityTitle);

            _txtActivity =
                new TextBox
                {
                    Left = 20,
                    Top = 48,
                    Width = 900,
                    Height = 80,

                    Multiline = true,

                    ScrollBars =
                        ScrollBars.Vertical,

                    ReadOnly = true,

                    BackColor =
                        Color.FromArgb(
                            16,
                            23,
                            36),

                    ForeColor =
                        Color.White,

                    BorderStyle =
                        BorderStyle.FixedSingle
                };

            activityPanel.Controls.Add(
                _txtActivity);

            mainPanel.Controls.Add(
                activityPanel);

            // ========================================================
            // INITIAL STATE
            // ========================================================

            _btnAdd.Enabled = false;
            _btnRemove.Enabled = false;
            _cmbCollections.Enabled = false;
            _txtCollectionFilter.Enabled = false;
            _lstCurrentCollections.Enabled = false;
        }

        private static Label CreatePanelTitle(
            string text)
        {
            return new Label
            {
                Text = text,
                Left = 20,
                Top = 15,
                Width = 500,
                Height = 28,

                Font =
                    new Font(
                        "Segoe UI",
                        11F,
                        FontStyle.Bold),

                ForeColor =
                    Color.White
            };
        }

        private static Label CreateInfoLabel(
            string title,
            int left,
            int top)
        {
            return new Label
            {
                Text = title,
                Left = left,
                Top = top,
                Width = 410,
                Height = 24,
                ForeColor = Color.White
            };
        }

        // ============================================================
        // LOOKUP
        // ============================================================

        private async Task LookupComputerAsync()
        {
            string computerName =
                _txtComputerName.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                    computerName))
            {
                MessageBox.Show(
                    "Enter a computer name.",
                    "MECM Lookup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            SetBusy(true);

            try
            {
                ClearComputerInformation();

                AppendActivity(
                    $"Looking up computer '{computerName}'...");

                MecmComputerResult result =
                    await _mecmManagementService
                        .GetComputerAsync(
                            computerName);

                if (!result.Success)
                {
                    AppendActivity(
                        $"Lookup failed: {result.Error}");

                    MessageBox.Show(
                        result.Error,
                        "MECM Lookup",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                _currentComputerName =
                    result.Name;

                _currentResourceId =
                    result.ResourceId;

                _lblComputerName.Text =
                    $"Computer: {DisplayValue(result.Name)}";

                _lblResourceId.Text =
                    $"Resource ID: {DisplayValue(result.ResourceId)}";

                _lblClient.Text =
                    $"Client: {DisplayValue(result.Client)}";

                _lblClientVersion.Text =
                    $"Client Version: {DisplayValue(result.ClientVersion)}";

                _lblOperatingSystem.Text =
                    $"Operating System: {DisplayValue(result.OperatingSystem)}";

                _lblManufacturer.Text =
                    $"Manufacturer: {DisplayValue(result.Manufacturer)}";

                _lblModel.Text =
                    $"Model: {DisplayValue(result.Model)}";

                _lblSerialNumber.Text =
                    $"Serial Number: {DisplayValue(result.SerialNumber)}";

                AppendActivity(
                    $"Computer '{result.Name}' found. " +
                    $"Resource ID: {result.ResourceId}");

                await LoadCollectionsAsync();

                await LoadCurrentCollectionsAsync();
            }
            finally
            {
                SetBusy(false);
            }
        }

        // ============================================================
        // LOAD ALL COLLECTIONS
        // ============================================================

        private async Task LoadCollectionsAsync()
        {
            AppendActivity(
                "Loading MECM collections...");

            MecmCollectionResult result =
                await _mecmManagementService
                    .GetCollectionsAsync();

            if (!result.Success)
            {
                AppendActivity(
                    $"Failed to load collections: {result.Error}");

                MessageBox.Show(
                    result.Error,
                    "MECM Collections",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            _allCollections.Clear();

            foreach (MecmCollection collection
                in result.Collections)
            {
                _allCollections.Add(
                    new CollectionItem
                    {
                        CollectionId =
                            collection.CollectionId,

                        Name =
                            collection.Name
                    });
            }

            FilterCollections();

            AppendActivity(
                $"Loaded {result.Collections.Count} MECM collections.");
        }

        // ============================================================
        // FILTER COLLECTIONS
        // ============================================================

        private void FilterCollections()
        {
            if (_cmbCollections == null)
            {
                return;
            }

            string filter =
                _txtCollectionFilter?.Text.Trim()
                ?? string.Empty;

            CollectionItem? selectedCollection =
                _cmbCollections.SelectedItem
                    as CollectionItem;

            _cmbCollections.BeginUpdate();

            try
            {
                _cmbCollections.Items.Clear();

                IEnumerable<CollectionItem> filtered =
                    _allCollections;

                if (!string.IsNullOrWhiteSpace(
                        filter))
                {
                    filtered =
                        _allCollections.Where(
                            collection =>
                                collection.Name.Contains(
                                    filter,
                                    StringComparison.OrdinalIgnoreCase)
                                ||
                                collection.CollectionId.Contains(
                                    filter,
                                    StringComparison.OrdinalIgnoreCase));
                }

                foreach (CollectionItem collection
                    in filtered)
                {
                    _cmbCollections.Items.Add(
                        collection);
                }

                if (_cmbCollections.Items.Count == 0)
                {
                    _cmbCollections.Enabled = false;
                    _btnAdd.Enabled = false;
                    _btnRemove.Enabled = false;

                    return;
                }

                int selectedIndex =
                    FindCollectionIndex(
                        selectedCollection);

                _cmbCollections.SelectedIndex =
                    selectedIndex >= 0
                        ? selectedIndex
                        : 0;

                bool computerLoaded =
                    !string.IsNullOrWhiteSpace(
                        _currentComputerName);

                _cmbCollections.Enabled =
                    computerLoaded;

                _btnAdd.Enabled =
                    computerLoaded;

                _btnRemove.Enabled =
                    computerLoaded;
            }
            finally
            {
                _cmbCollections.EndUpdate();
            }
        }

        private int FindCollectionIndex(
            CollectionItem? collection)
        {
            if (collection == null)
            {
                return -1;
            }

            for (int i = 0;
                 i < _cmbCollections.Items.Count;
                 i++)
            {
                if (_cmbCollections.Items[i]
                    is CollectionItem item &&
                    item.CollectionId ==
                    collection.CollectionId)
                {
                    return i;
                }
            }

            return -1;
        }

        // ============================================================
        // LOAD CURRENT COMPUTER MEMBERSHIPS
        // ============================================================

        private async Task
            LoadCurrentCollectionsAsync()
        {
            _lstCurrentCollections.Items.Clear();

            if (string.IsNullOrWhiteSpace(
                    _currentResourceId))
            {
                return;
            }

            AppendActivity(
                "Loading current collection memberships...");

            MecmCollectionResult result =
                await _mecmManagementService
                    .GetComputerCollectionsAsync(
                        _currentResourceId);

            if (!result.Success)
            {
                AppendActivity(
                    "Failed to load current collection memberships: " +
                    result.Error);

                _lstCurrentCollections.Items.Add(
                    "Unable to load collection memberships.");

                return;
            }

            if (result.Collections.Count == 0)
            {
                _lstCurrentCollections.Items.Add(
                    "Computer is not currently a member of any collections.");

                AppendActivity(
                    "Computer is not currently a member of any MECM collections.");

                return;
            }

            foreach (MecmCollection collection
                in result.Collections)
            {
                _lstCurrentCollections.Items.Add(
                    new CollectionMembershipItem
                    {
                        CollectionId =
                            collection.CollectionId,

                        Name =
                            collection.Name
                    });
            }

            AppendActivity(
                "Computer is currently a member of " +
                $"{result.Collections.Count} MECM collection(s).");
        }

        // ============================================================
        // ADD TO COLLECTION
        // ============================================================

        private async Task AddToCollectionAsync()
        {
            if (_cmbCollections.SelectedItem is not CollectionItem collection)
            {
                MessageBox.Show(
                    "Select a collection.",
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    _currentComputerName))
            {
                MessageBox.Show(
                    "Look up a computer first.",
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            SetBusy(true);

            try
            {
                AppendActivity(
                    $"Adding '{_currentComputerName}' " +
                    $"to '{collection.Name}' " +
                    $"({collection.CollectionId})...");

                MecmActionResult result =
                    await _mecmManagementService
                        .AddComputerToCollectionAsync(
                            _currentComputerName,
                            collection.CollectionId);

                AppendActivity(
                    result.Success
                        ? result.Output
                        : $"Add failed: {result.Error}");

                MessageBox.Show(
                    result.Success
                        ? result.Output
                        : result.Error,
                    "MECM",
                    MessageBoxButtons.OK,
                    result.Success
                        ? MessageBoxIcon.Information
                        : MessageBoxIcon.Error);

                if (result.Success)
                {
                    await LoadCurrentCollectionsAsync();
                }
            }
            finally
            {
                SetBusy(false);
            }
        }

        // ============================================================
        // REMOVE FROM COLLECTION
        // ============================================================

        private async Task RemoveFromCollectionAsync()
        {
            if (_cmbCollections.SelectedItem is not CollectionItem collection)
            {
                MessageBox.Show(
                    "Select a collection.",
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    _currentComputerName))
            {
                MessageBox.Show(
                    "Look up a computer first.",
                    "MECM",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    $"Remove '{_currentComputerName}' from " +
                    "the direct membership of " +
                    $"'{collection.Name}'?",
                    "Confirm Removal",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (confirmation !=
                DialogResult.Yes)
            {
                return;
            }

            SetBusy(true);

            try
            {
                AppendActivity(
                    $"Removing '{_currentComputerName}' " +
                    $"from '{collection.Name}' " +
                    $"({collection.CollectionId})...");

                MecmActionResult result =
                    await _mecmManagementService
                        .RemoveComputerFromCollectionAsync(
                            _currentComputerName,
                            collection.CollectionId);

                AppendActivity(
                    result.Success
                        ? result.Output
                        : $"Remove failed: {result.Error}");

                MessageBox.Show(
                    result.Success
                        ? result.Output
                        : result.Error,
                    "MECM",
                    MessageBoxButtons.OK,
                    result.Success
                        ? MessageBoxIcon.Information
                        : MessageBoxIcon.Error);

                if (result.Success)
                {
                    await LoadCurrentCollectionsAsync();
                }
            }
            finally
            {
                SetBusy(false);
            }
        }

        // ============================================================
        // CLEAR COMPUTER INFORMATION
        // ============================================================

        private void ClearComputerInformation()
        {
            _currentComputerName =
                string.Empty;

            _currentResourceId =
                string.Empty;

            _lblComputerName.Text =
                "Computer:";

            _lblResourceId.Text =
                "Resource ID:";

            _lblClient.Text =
                "Client:";

            _lblClientVersion.Text =
                "Client Version:";

            _lblOperatingSystem.Text =
                "Operating System:";

            _lblManufacturer.Text =
                "Manufacturer:";

            _lblModel.Text =
                "Model:";

            _lblSerialNumber.Text =
                "Serial Number:";

            _lstCurrentCollections.Items.Clear();

            _allCollections.Clear();

            _cmbCollections.Items.Clear();

            _cmbCollections.Enabled =
                false;

            _txtCollectionFilter.Clear();

            _txtCollectionFilter.Enabled =
                false;

            _btnAdd.Enabled =
                false;

            _btnRemove.Enabled =
                false;
        }

        // ============================================================
        // BUSY STATE
        // ============================================================

        private void SetBusy(bool busy)
        {
            _txtComputerName.Enabled =
                !busy;

            _btnLookup.Enabled =
                !busy;

            if (busy)
            {
                _txtCollectionFilter.Enabled =
                    false;

                _cmbCollections.Enabled =
                    false;

                _btnAdd.Enabled =
                    false;

                _btnRemove.Enabled =
                    false;

                _lstCurrentCollections.Enabled =
                    false;

                return;
            }

            bool computerLoaded =
                !string.IsNullOrWhiteSpace(
                    _currentComputerName);

            bool collectionsLoaded =
                _cmbCollections.Items.Count > 0;

            _txtCollectionFilter.Enabled =
                computerLoaded &&
                _allCollections.Count > 0;

            _cmbCollections.Enabled =
                computerLoaded &&
                collectionsLoaded;

            _btnAdd.Enabled =
                computerLoaded &&
                collectionsLoaded;

            _btnRemove.Enabled =
                computerLoaded &&
                collectionsLoaded;

            _lstCurrentCollections.Enabled =
                computerLoaded;
        }

        // ============================================================
        // ACTIVITY
        // ============================================================

        private void AppendActivity(
            string message)
        {
            _txtActivity.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] " +
                message +
                Environment.NewLine);
        }

        // ============================================================
        // DISPLAY HELPERS
        // ============================================================

        private static string DisplayValue(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "Not available"
                : value;
        }

        // ============================================================
        // COLLECTION ITEM
        // ============================================================

        private sealed class CollectionItem
        {
            public string CollectionId { get; init; } =
                string.Empty;

            public string Name { get; init; } =
                string.Empty;

            public override string ToString()
            {
                return
                    $"{Name} ({CollectionId})";
            }
        }

        private sealed class CollectionMembershipItem
        {
            public string CollectionId { get; init; } =
                string.Empty;

            public string Name { get; init; } =
                string.Empty;

            public override string ToString()
            {
                return
                    $"{Name} ({CollectionId})";
            }
        }
    }
}
