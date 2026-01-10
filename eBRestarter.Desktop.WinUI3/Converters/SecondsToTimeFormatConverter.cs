using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Converters
{
    public class SecondsToTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Wir erwarten int (Sekunden) oder TimeSpan
            if (value is int seconds)
            {
                if (seconds < 0) return "00:00:00";
                TimeSpan time = TimeSpan.FromSeconds(seconds);
                return FormatTimeSpan(time);
            }

            if (value is TimeSpan timeSpan)
            {
                return FormatTimeSpan(timeSpan);
            }

            return "00:00:00";
        }

        private string FormatTimeSpan(TimeSpan time)
        {
            // Deine Logik von oben, aber kompakter
            if (time.TotalHours >= 1)
                return time.ToString(@"hh\:mm\:ss");

            return time.ToString(@"hh\:mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException(); // Brauchen wir nur OneWay
        }
    }
}
