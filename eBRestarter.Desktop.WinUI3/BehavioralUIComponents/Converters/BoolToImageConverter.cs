using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace eBRestarter.Desktop.WinUI3.Converters;

/// <summary>
/// Converts a boolean to an image source; paths are configurable via XAML properties.
/// </summary>
public sealed partial class BoolToImageConverter : IValueConverter
{
    public string? ImagePathWhenFalse { get; set; }
    public string? ImagePathWhenTrue { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool flag = value is bool b && b;

        string? path = flag ? ImagePathWhenTrue : ImagePathWhenFalse;

        if (string.IsNullOrEmpty(path)) return null!;

        return new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
