using eBRestarter.Desktop.WinUI3.Helpers.Interfaces;
using eBRestarter.Desktop.WinUI3.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.Services;

public sealed class ThemeService : IThemeService
{
    private readonly IMainWindowProvider _mainWindowProvider;
    private readonly IAppWindowHelper _appWindowHelper;

#pragma warning disable IDE0290 // Primären Konstruktor verwenden
    public ThemeService(IMainWindowProvider mainWindowProvider, IAppWindowHelper appWindowHelper)
#pragma warning restore IDE0290 // Primären Konstruktor verwenden
    {
        _mainWindowProvider = mainWindowProvider;
        _appWindowHelper = appWindowHelper;
    }

    public string CurrentTheme { get; private set; } = "Light";

    public void SetTheme(string themeName)
    {
        var mainWindow = _mainWindowProvider.MainWindow;

        if (mainWindow?.Content is FrameworkElement rootElement)
        {
            var theme = themeName == "Dark" ? ElementTheme.Dark : ElementTheme.Light;
            rootElement.RequestedTheme = theme;

            _appWindowHelper.UpdateTitleBarTheme(mainWindow, theme);
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
