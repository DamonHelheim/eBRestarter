using eBRestarter.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models
{
    public class BrowserInfo
    {
        public BrowserType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool IsInstalled { get; set; }
        public string IconPath { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
    }
}
