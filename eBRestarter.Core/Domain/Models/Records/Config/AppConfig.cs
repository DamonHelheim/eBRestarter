using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records.Config
{
    public record AppConfig
    {
        public string Username { get; set; } = string.Empty;
        public BrowserConfig Browser { get; set; } = new();
        public SettingsConfig Settings { get; set; } = new();
        public SchedulerConfig Scheduler { get; set; } = new();
    }
}
