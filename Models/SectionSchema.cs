using System;
using System.Collections.Generic;

namespace AdminTrayTool.Models
{
    public class ColumnSchema
    {
        public string Name { get; set; } = string.Empty;      // JSON property name, e.g. "url"
        public string DisplayName { get; set; } = string.Empty; // Grid header, e.g. "URL"
        public Type FieldType { get; set; } = typeof(string);   // typeof(string) or typeof(bool)
        public bool Required { get; set; }
        public string Placeholder { get; set; } = string.Empty;
        public string ToolTip { get; set; } = string.Empty;
        public Func<string, bool>? Validator { get; set; }      // e.g. must start with http(s)://
        public string ValidationError { get; set; } = string.Empty;
        public object? DefaultValue { get; set; }
    }

    public class SectionSchema
    {
        public string TabTitle { get; set; } = string.Empty;
        public string SectionKey { get; set; } = string.Empty;   // JSON array key, e.g. "webPortals"
        public string DedupeKey { get; set; } = string.Empty;    // column Name used to detect duplicates/updates
        public List<ColumnSchema> Columns { get; set; } = new();
        public string InstructionText { get; set; } = string.Empty;

        // Optional: for one-to-many shapes like Group Templates (Template -> [Groups]).
        // When set, rows sharing the same value in this column are grouped into
        // one parent object with a nested array on save, and expanded back into
        // one row per array item on load.
        public string? GroupByColumn { get; set; }
        public string? NestedArrayPropertyName { get; set; }
    }
}