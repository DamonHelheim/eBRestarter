using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Formatters;
using System;

namespace eBRestarter.Desktop.WinUI3.Utilities;

/// <summary>
/// Formats time spans in a human-readable way (adapter for UI/Infrastructure).
/// </summary>
public sealed class HumanReadableTimeUtility : ITimeFormatter
{
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
}
