using System;

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using Microsoft.UI.Xaml;

using eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Helpers.Interfaces;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler;

/// <summary>
/// Handler implementation for managing and applying application theme changes across UI elements and chart controls.
/// </summary>
public sealed class ThemeHandler(
    IAppWindowHelper appWindowHelper,
    IMainWindowProvider mainWindowProvider) : IThemeHandler
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private const string ThemeDark = "Dark";
    private const string ThemeLight = "Light";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IAppWindowHelper _appWindowHelper = appWindowHelper ?? throw new ArgumentNullException(nameof(appWindowHelper));
    private readonly IMainWindowProvider _mainWindowProvider = mainWindowProvider ?? throw new ArgumentNullException(nameof(mainWindowProvider));


    // ═══════════════════════════════════════════════════════
    //  6. Properties
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Gets the name of the currently active UI theme.
    /// </summary>
    public string CurrentTheme { get; private set; } = ThemeLight;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Applies the specified theme name to the main window root element, title bar, and chart controls.
    /// </summary>
    /// <param name="themeName">The target theme name (e.g. "Dark" or "Light").</param>
    public void SetTheme(string themeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeName);

        bool isDark = string.Equals(themeName, ThemeDark, StringComparison.OrdinalIgnoreCase);
        var theme = isDark ? ElementTheme.Dark : ElementTheme.Light;

        var mainWindow = _mainWindowProvider.MainWindow;

        if (mainWindow?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme;
            _appWindowHelper.UpdateTitleBarTheme(mainWindow, theme);
        }

        if (isDark)
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
