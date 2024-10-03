using System;
using System.Windows;

namespace eBRestarter.Themes
{
    public static class ThemeManager
    {
        public static void ApplyLightMode()
        {
            ResourceDictionary lightmodeStyles = new()
            {
                Source = new Uri("/Resources/XAMLResources/Themes/LightTheme.xaml", UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries.Add(lightmodeStyles);
        }

        public static void ApplyDarkMode()
        {
            ResourceDictionary darkmodeStyles = new()
            {
                Source = new Uri("/Resources/XAMLResources/Themes/DarkTheme.xaml", UriKind.Relative)
            };

            Application.Current.Resources.MergedDictionaries.Add(darkmodeStyles);
        }
    }
}
