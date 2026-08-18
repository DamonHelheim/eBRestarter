using System;

using Microsoft.UI.Xaml.Data;

namespace eBRestarter.Desktop.WinUI3.Converters;

/// <summary>
/// Converts an integer total second count or a <see cref="TimeSpan"/> object into a formatted time string (hh:mm:ss).
/// </summary>
public sealed class SecondsToTimeFormatConverter : IValueConverter
{
    private const string DefaultTimeFormat = "00:00:00";
    private const string TimeSpanFormatPattern = @"hh\:mm\:ss";

    /// <inheritdoc />
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

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> instance into standard hh:mm:ss string representation.
    /// </summary>
    /// <param name="time">The time span to format.</param>
    /// <returns>A formatted string.</returns>
    private static string FormatTimeSpan(TimeSpan time)
    {
        return time.ToString(TimeSpanFormatPattern);
    }
}
