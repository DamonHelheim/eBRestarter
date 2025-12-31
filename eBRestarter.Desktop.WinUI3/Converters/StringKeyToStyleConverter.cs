using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Converters
{
    public class StringKeyToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Der value ist dein String aus dem ViewModel (z.B. "DownloadBrowserToggleButtonRed")
            if (value is string styleKey && !string.IsNullOrEmpty(styleKey))
            {
                // Versuche, die Ressource aus der App-weiten ResourceDictionary zu holen
                if (Application.Current.Resources.TryGetValue(styleKey, out object styleResource))
                {
                    return styleResource as Style;
                }
            }

            // Fallback: Wenn nichts gefunden wird, gib den Standard-Style oder null zurück
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
