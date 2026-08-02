using System;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Utilities;

/// <summary>
/// Formats time spans in a human-readable way for the Presentation Layer.
/// </summary>
public static class HumanReadableTimeUtility
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const int MinimumThreshold = 1;
    private const string ZeroSecondsText = "0s";


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Formats a time span given in total seconds into a human-readable string representation (e.g., "1h: 30m: 15s").
    /// </summary>
    /// <param name="seconds">Total duration in seconds.</param>
    /// <returns>A formatted human-readable time string.</returns>
    public static string Format(int seconds)
    {
        if (seconds <= 0)
        {
            return ZeroSecondsText;
        }

        TimeSpan time = TimeSpan.FromSeconds(seconds);
        int totalHours = (int)time.TotalHours;

        if (totalHours >= MinimumThreshold)
        {
            return $"{totalHours}h: {time.Minutes}m: {time.Seconds}s";
        }

        if (time.TotalMinutes >= MinimumThreshold)
        {
            return $"{time.Minutes}m: {time.Seconds}s";
        }

        return $"{time.Seconds}s";
    }
}
