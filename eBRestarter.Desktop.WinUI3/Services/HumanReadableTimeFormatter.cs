using eBRestarter.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services
{
    /// <summary>
    /// Formats time spans in a human-readable way (adapter for UI/Infrastructure).
    /// </summary>
    public class HumanReadableTimeFormatter : ITimeFormatter
    {
        // =========================================================
        // 1. PUBLIC METHODS
        // =========================================================
        #region PublicMethods

        public string Format(int seconds)
        {
            if (seconds < 0) return "0s";
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time switch
            {
                { TotalHours: >= 1 } => time.ToString(@"h\h\:\ m\m\:\ s\s"),
                { TotalMinutes: >= 1 } => time.ToString(@"m\m\:\ s\s"),
                _ => time.ToString(@"s\s")
            };
        }

        #endregion
    }
}
