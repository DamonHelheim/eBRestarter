using System;

using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace eBRestarter.Desktop.WinUI3.Converters;

/// <summary>
/// Converts a boolean value to a WinUI3 BitmapImage source using configurable image file paths for true and false states.
/// </summary>
public sealed class BoolToImageConverter : IValueConverter
{
    // ═══════════════════════════════════════════════════════
    //  6. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Gets or sets the image asset file path to display when the boolean value is false.
    /// </summary>
    public string? ImagePathWhenFalse { get; set; }

    /// <summary>
    /// Gets or sets the image asset file path to display when the boolean value is true.
    /// </summary>
    public string? ImagePathWhenTrue { get; set; }


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Converts a boolean value into a corresponding <see cref="BitmapImage"/> instance.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isTrue = value is bool boolValue && boolValue;
        string? imagePath = isTrue ? ImagePathWhenTrue : ImagePathWhenFalse;

        if (string.IsNullOrEmpty(imagePath))
        {
            return null!;
        }

        return new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
    }

    /// <summary>
    /// Reverse conversion is not supported for image sources.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
