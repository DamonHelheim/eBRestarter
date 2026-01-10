using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI; // Hinzugefügt für Color-Parsing

namespace eBRestarter.Desktop.WinUI3.Converters
{
    public class TimerStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // true = Timer läuft = Stop = Rot
            if ((bool)value)
            {
                // Hex-String zu Color parsen
                var color = ColorHelper.FromArgb(179, 12, 46, 0); // #FF0000
                return new SolidColorBrush(color);
            }

            // false = Timer steht = Start = Grün/Blau
            // AliceBlue: #F0F8FF
            var startColor = ColorHelper.FromArgb(13, 178, 144, 0);
            return new SolidColorBrush(startColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
