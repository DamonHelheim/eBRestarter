using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models.Records.Config
{
    public record SchedulerConfig
    {
        public bool CheckBrowserAliveRoutine { get; set; } = false;
        public bool DeleteCookiesAndCache { get; set; } = false;
        public bool RestartComputer { get; set; } = false;
        public int RestartTimeHours { get; set; } = 1;
        public bool StartRestarterWithProgramStart { get; set; } = false;
    }
}
