using System;

namespace eBRestarter.Desktop.WinUI3.Utilities;

/// <summary>
/// Formats time spans in a human-readable way for the Presentation Layer.
/// </summary>
public static class HumanReadableTimeUtility
{
    public static string Format(int seconds)
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
