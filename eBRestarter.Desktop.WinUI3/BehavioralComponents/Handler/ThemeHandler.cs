using System;

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

using Microsoft.UI.Xaml;

using eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Helpers.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Enums;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler;

/// <summary>
/// Handler implementation for managing and applying application theme changes across UI elements and chart controls.
/// </summary>
public sealed class ThemeHandler(
    IAppWindowHelper appWindowHelper,
    IMainWindowProvider mainWindowProvider) : IThemeHandler
{
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
    /// Gets the name of the currently active UI theme enum.
    /// </summary>
    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Applies the specified <see cref="AppTheme"/> enum to the main window root element, title bar, and chart controls.
    /// </summary>
    /// <param name="theme">The target <see cref="AppTheme"/> value.</param>
    public void SetTheme(AppTheme theme)
    {
        bool isDark = theme == AppTheme.Dark;
        var elementTheme = theme switch
        {
            AppTheme.Dark => ElementTheme.Dark,
            AppTheme.Light => ElementTheme.Light,
            _ => ElementTheme.Default
        };

        var mainWindow = _mainWindowProvider.MainWindow;

        if (mainWindow?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = elementTheme;
            _appWindowHelper.UpdateTitleBarTheme(mainWindow, elementTheme);
        }

        if (isDark)
        {
            LiveCharts.Configure(config => config.AddDarkTheme());
        }
        else
        {
            LiveCharts.Configure(config => config.AddLightTheme());
        }

        CurrentTheme = theme;
    }

    /// <summary>
    /// Applies the theme matching the specified theme name string.
    /// </summary>
    /// <param name="themeName">The target theme name string (e.g. "Dark" or "Light").</param>
    public void SetTheme(string themeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeName);

        AppTheme parsedTheme = Enum.TryParse<AppTheme>(themeName, true, out var result)
            ? result
            : AppTheme.Light;

        SetTheme(parsedTheme);
    }
}
