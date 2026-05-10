using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using LiveChartsCore;
using Microsoft.UI.Xaml;
using LiveChartsCore.SkiaSharpView;
using eBRestarter.Desktop.WinUI3.Helpers;

namespace eBRestarter.Desktop.WinUI3.Services;

public class ThemeService : IThemeService
{
    public string CurrentTheme { get; private set; } = "Light";

    public void SetTheme(string themeName)
    {
        if (App.MainWindoweBRestarter?.Content is FrameworkElement rootElement) {

            var theme = themeName == "Dark" ? ElementTheme.Dark : ElementTheme.Light;
            rootElement.RequestedTheme = theme;
            
            App.MainWindoweBRestarter.UpdateTitleBarTheme(theme);
        }

        if (themeName == "Dark")
        {
            LiveCharts.Configure(config => config.AddDarkTheme());
        }
        else
        {
            LiveCharts.Configure(config => config.AddLightTheme());
        }

        CurrentTheme = themeName;
    }
}
