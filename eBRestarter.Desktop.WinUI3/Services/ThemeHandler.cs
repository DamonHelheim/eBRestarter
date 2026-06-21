using eBRestarter.Desktop.WinUI3.Helpers;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.Services;

public class ThemeHandler : IThemeHandler
{
    public string CurrentTheme { get; private set; } = "Light";

    public void SetTheme(string themeName)
    {
        if (App.MainWindoweBRestarter?.Content is FrameworkElement rootElement)
        {
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
