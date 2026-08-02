using System;

using Microsoft.UI.Xaml.Data;

namespace eBRestarter.Desktop.WinUI3.Converters;

/// <summary>
/// Converts an integer total second count or a <see cref="TimeSpan"/> object into a formatted time string (hh:mm:ss).
/// </summary>
public sealed class SecondsToTimeFormatConverter : IValueConverter
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string DefaultTimeFormat = "00:00:00";
    private const string TimeSpanFormatPattern = @"hh\:mm\:ss";


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Converts a total second count (int) or <see cref="TimeSpan"/> into a formatted time string representation.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int seconds)
        {
            if (seconds < 0)
            {
                return DefaultTimeFormat;
            }

            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return FormatTimeSpan(time);
        }

        if (value is TimeSpan timeSpan)
        {
            return FormatTimeSpan(timeSpan);
        }

        return DefaultTimeFormat;
    }

    /// <summary>
    /// Reverse conversion is not supported.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private static string FormatTimeSpan(TimeSpan time)
    {
        return time.ToString(TimeSpanFormatPattern);
    }
}
