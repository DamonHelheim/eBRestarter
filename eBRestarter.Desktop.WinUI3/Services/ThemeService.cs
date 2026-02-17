using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using LiveChartsCore;
using Microsoft.UI.Xaml;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.Services
{
    public class ThemeService : IThemeService
    {
        // =========================================================
        // 1. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        public string CurrentTheme { get; private set; } = "Light";

        #endregion

        // =========================================================
        // 2. PUBLIC METHODS
        // =========================================================
        #region PublicMethods

        public void SetTheme(string themeName)
        {
            if (App.MainWindoweBRestarter?.Content is FrameworkElement rootElement)
                rootElement.RequestedTheme = themeName == "Dark" ? ElementTheme.Dark : ElementTheme.Light;

            if (themeName == "Dark")
                LiveCharts.Configure(config => config.AddDarkTheme());
            else
                LiveCharts.Configure(config => config.AddLightTheme());

            CurrentTheme = themeName;
        }

        #endregion
    }
}
