using Microsoft.UI.Xaml.Data;
using System;

namespace eBRestarter.Desktop.WinUI3.Converters;

/// <summary>
/// Converts seconds (int) or TimeSpan to a formatted time string (e.g. hh:mm:ss).
/// </summary>
public partial class SecondsToTimeFormatConverter : IValueConverter
{
    // =========================================================
    // 1. PUBLIC METHODS (API)
    // =========================================================
    #region PublicMethods

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int seconds)
        {
            if (seconds < 0) return "00:00:00";
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return FormatTimeSpan(time);
        }
        if (value is TimeSpan timeSpan)
            return FormatTimeSpan(timeSpan);
        return "00:00:00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    #endregion

    // =========================================================
    // 2. PRIVATE HELPER METHODS (Internal helpers)
    // =========================================================
    #region PrivateHelperMethods

    private string FormatTimeSpan(TimeSpan time)
    {
        return time.ToString(@"hh\:mm\:ss");
    }

    #endregion
}
