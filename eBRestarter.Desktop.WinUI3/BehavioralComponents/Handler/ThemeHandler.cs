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
/// <param name="appWindowHelper">Helper for updating application title bar theme.</param>
/// <param name="mainWindowProvider">Provider supplying access to the active main window instance.</param>
public sealed class ThemeHandler(
    IAppWindowHelper appWindowHelper,
    IMainWindowProvider mainWindowProvider) : IThemeHandler
{
    private readonly IAppWindowHelper _appWindowHelper = appWindowHelper ?? throw new ArgumentNullException(nameof(appWindowHelper));
    private readonly IMainWindowProvider _mainWindowProvider = mainWindowProvider ?? throw new ArgumentNullException(nameof(mainWindowProvider));

    /// <inheritdoc />
    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void SetTheme(string themeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeName);

        AppTheme parsedTheme = Enum.TryParse<AppTheme>(themeName, true, out var result)
            ? result
            : AppTheme.Light;

        SetTheme(parsedTheme);
    }
}
