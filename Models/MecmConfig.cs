namespace AdminTrayTool.Models
{
    public class MecmConfig
    {
        public bool Enabled { get; set; }

        public string SiteCode { get; set; } = string.Empty;

        public string SmsProviderServer { get; set; } = string.Empty;
    }
}
