using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.Helpers
{
    // Statische Helper-Klasse oder Extension Method
    public static class AppWindowHelper
    {
        public static void ConfigureTitleBarColors(this Window window)
        {
            var appWindow = window.AppWindow;
            var titleBar = appWindow.TitleBar;

            // ... (Deine Logik hier, z.B. Farben aus App-Ressourcen holen)
            var bg = Microsoft.UI.ColorHelper.FromArgb(255, 32, 37, 54);

            titleBar.ExtendsContentIntoTitleBar = true;
            titleBar.BackgroundColor = bg;
            // ... alle anderen Farbanpassungen

            // Für WinUI-Titlebar
            window.ExtendsContentIntoTitleBar = true;
            window.SetTitleBar(null);
        }
    }
}
