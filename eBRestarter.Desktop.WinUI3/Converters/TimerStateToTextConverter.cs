using Microsoft.UI.Xaml.Data;
using System;

namespace eBRestarter.Desktop.WinUI3.Converters;

/// <summary>
/// Converts a boolean timer state to display text ("Stop" when true, "Start" when false).
/// </summary>
public partial class TimerStateToTextConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? "Stop" : "Start";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();

}
