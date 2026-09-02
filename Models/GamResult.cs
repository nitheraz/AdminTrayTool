using System;
using System.Collections.Generic;
using System.Text;

namespace AdminTrayTool.Models
{
    public class GamResult
    {
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;

        public string CombinedOutput
        {
            get
            {
                return string.IsNullOrWhiteSpace(Error) ? Output :
                    string.IsNullOrWhiteSpace(Output) ? Error :
                    Output + Environment.NewLine + Error;
            }
        }
    }
}
