using System;
using System.Collections.Generic;

namespace AdminTrayTool.Models
{
    public class AssetIdCache
    {
        public DateTime FetchedAtUtc { get; set; }
        public Dictionary<string, string> AssetIdToSerial { get; set; } = new();
    }
}