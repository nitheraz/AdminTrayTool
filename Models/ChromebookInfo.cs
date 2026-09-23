namespace AdminTrayTool.Models
{
    public class ChromebookInfo
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string AnnotatedAssetId { get; set; } = string.Empty;
        public string RecentUserEmail { get; set; } = string.Empty;

        public string AssetId
        {
            get => AnnotatedAssetId;
            set => AnnotatedAssetId = value;
        }

        public string Model { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string OrgUnitId { get; set; } = string.Empty;
        public string OrgUnitPath { get; set; } = string.Empty;
        public string LastSync { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string RawOutput { get; set; } = string.Empty;
    }
}
