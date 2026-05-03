using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using LiveChartsCore;
using Microsoft.UI.Xaml;
using LiveChartsCore.SkiaSharpView;

namespace eBRestarter.Desktop.WinUI3.Services;

public class ThemeService : IThemeService
{
    public string CurrentTheme { get; private set; } = "Light";

    public void SetTheme(string themeName)
    {
        if (App.MainWindoweBRestarter?.Content is FrameworkElement rootElement) {

            rootElement.RequestedTheme = themeName == "Dark" ? ElementTheme.Dark : ElementTheme.Light;

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
