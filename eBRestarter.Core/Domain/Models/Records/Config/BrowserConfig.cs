using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records.Config
{
    public record BrowserConfig
    {
        public string SelectedBrowser { get; set; } = "Chrome";
        public int RuntimeHours { get; set; } = 1;
        public int RuntimePauseSeconds { get; set; } = 20;
    }
}
