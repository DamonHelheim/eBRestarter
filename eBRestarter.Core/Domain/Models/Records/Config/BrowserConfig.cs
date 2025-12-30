using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records.Config
{
    public record Browser
    {
        public bool StartBrowserWithProgrammStart { get; set; } = false;
        public bool CheckBrowserAliveRoutine { get; set; } = false;
        public string Selected { get; set; } = string.Empty;
        public int RuntimeHours { get; set; } = 1;
        public int RuntimePauseSeconds { get; set; } = 20;
        public int DeleteBrowserCacheIntervalDays { get; set; } = 0;
        public DateTime NextBrowserDeleteCacheDate { get; set; }

    }
}
