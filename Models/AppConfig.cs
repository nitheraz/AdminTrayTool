namespace AdminTrayTool.Models
{
    public class AppConfig
    {
        public string Editor { get; set; } = "notepad.exe";

        public List<WebPortal> WebPortals { get; set; } = [];

        public List<RdpEntry> RdpServers { get; set; } = [];

        public List<AdminTool> AdminTools { get; set; } = [];

        public ActiveDirectoryConfig ActiveDirectory { get; set; } = new();

        public MecmConfig Mecm { get; set; } = new();
    }

    public class WebPortal
    {
        public string Name { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Profile { get; set; } = string.Empty;
    }

    public class RdpEntry
    {
        public string Name { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;
    }

    public class AdminTool
    {
        public string Name { get; set; } = string.Empty;

        public string Exe { get; set; } = string.Empty;

        public string Args { get; set; } = string.Empty;

        public string Category { get; set; } = "Other";

        public bool Elevated { get; set; }
    }
}
