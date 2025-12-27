using eBRestarter.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services
{
    /// <summary>
    /// Verantwortlichkeit: Konkrete Umsetzung der Formatierung für Menschen.
    /// Layer: Adapter (UI / Infrastructure)
    /// </summary>
    public class HumanReadableTimeFormatter : ITimeFormatter
    {
        public string Format(int seconds)
        {
            if (seconds < 0) return "0s";

            TimeSpan time = TimeSpan.FromSeconds(seconds);

            // Nutzung von Pattern Matching (modernes C#)
            return time switch
            {
                { TotalHours: >= 1 } => time.ToString(@"h\h\:\ m\m\:\ s\s"),
                { TotalMinutes: >= 1 } => time.ToString(@"m\m\:\ s\s"),
                _ => time.ToString(@"s\s")
            };
        }
    }
}
