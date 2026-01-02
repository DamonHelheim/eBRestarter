using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records.Config
{
    public record SettingsConfig
    {
        public string Theme { get; set; } = "Light";
        public int Language { get; set; }
        public bool StartWithWindows { get; set; } = false;
        public bool  StartRestarterWithProgramStart { get; set; } = false;
    }
}
