using System.Collections.Generic;

namespace AdminTrayTool.Models
{
    public class GroupTemplate
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Groups { get; set; } = new();
    }

    public class GroupTemplateConfig
    {
        public List<GroupTemplate> Templates { get; set; } = new();
    }
}