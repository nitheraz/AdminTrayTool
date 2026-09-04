using System;
using System.Collections.Generic;

namespace AdminTrayTool.Models
{
    public class EmailAutocompleteCache
    {
        public DateTime FetchedAtUtc { get; set; }
        public List<string> Emails { get; set; } = new();
    }
}