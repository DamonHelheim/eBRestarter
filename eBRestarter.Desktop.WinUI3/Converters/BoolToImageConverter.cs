using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.Converters
{
    /// <summary>
    /// Converts a boolean to an image source; paths are configurable via XAML properties.
    /// </summary>
    public class BoolToImageConverter : IValueConverter
    {
        // =========================================================
        // 1. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        public string? ImagePathWhenFalse { get; set; }
        public string? ImagePathWhenTrue { get; set; }

        #endregion

        // =========================================================
        // 2. PUBLIC METHODS (API)
        // =========================================================
        #region PublicMethods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool flag = value is bool b && b;
            string? path = flag ? ImagePathWhenTrue : ImagePathWhenFalse;
            if (string.IsNullOrEmpty(path))
                return null!;
            return new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
