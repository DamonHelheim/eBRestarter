using Microsoft.UI.Xaml.Data;
using System;

namespace eBRestarter.Desktop.WinUI3.Converters;

/// <summary>
/// Converts a boolean timer state to display text ("Stop" when true, "Start" when false).
/// </summary>
public class TimerStateToTextConverter : IValueConverter
{
    // =========================================================
    // 1. PUBLIC METHODS (API)
    // =========================================================
    #region PublicMethods

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? "Stop" : "Start";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();

    #endregion
}
