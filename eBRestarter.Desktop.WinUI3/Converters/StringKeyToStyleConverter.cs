using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace eBRestarter.Desktop.WinUI3.Converters;

/// <summary>
/// Converts a string resource key to a Style from the application resource dictionary.
/// </summary>
public partial class StringKeyToStyleConverter : IValueConverter
{
    // =========================================================
    // 1. PUBLIC METHODS (API)
    // =========================================================
    #region PublicMethods

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string styleKey && !string.IsNullOrEmpty(styleKey))
        {
            if (Application.Current.Resources.TryGetValue(styleKey, out object styleResource))
                return styleResource as Style;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    #endregion
}
