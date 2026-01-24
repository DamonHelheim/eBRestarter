using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services
{
    public class ThemeService : IThemeService
    {
        // Wir brauchen keine Pfade mehr im Service!

        public string CurrentTheme { get; private set; } = "Light";

        public void SetTheme(string themeName)
        {
            // Sicherstellen, dass wir auf dem UI Thread sind (normalerweise durch Command gegeben)
            if (App.MainWindoweBRestarter?.Content is FrameworkElement rootElement)
            {
                // Das ist alles, was nötig ist.
                // WinUI schaut in App.xaml -> ThemeDictionaries -> Sucht den Key ("Dark" oder "Light")
                // und wendet die Ressourcen automatisch an.
                rootElement.RequestedTheme = themeName == "Dark" ? ElementTheme.Dark : ElementTheme.Light;

                // Auch die TitleBar anpassen (optional, falls du custom TitleBar hast)
                // UpdateTitleBar(themeName); 
            }

            CurrentTheme = themeName;
        }
    }
}
