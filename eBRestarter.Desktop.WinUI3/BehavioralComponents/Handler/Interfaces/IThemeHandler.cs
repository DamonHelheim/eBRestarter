using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Enums;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Handler.Interfaces;

/// <summary>
/// Handler for applying and querying application UI themes across window elements and chart controls.
/// </summary>
public interface IThemeHandler
{
    /// <summary>
    /// Gets the currently active UI theme enum value.
    /// </summary>
    AppTheme CurrentTheme { get; }

    /// <summary>
    /// Applies the specified <see cref="AppTheme"/> to the application main window, title bar, and chart controls.
    /// </summary>
    /// <param name="theme">The target <see cref="AppTheme"/> enum value.</param>
    void SetTheme(AppTheme theme);

    /// <summary>
    /// Applies the theme matching the specified theme name string.
    /// </summary>
    /// <param name="themeName">The target theme name string (e.g., "Dark" or "Light").</param>
    void SetTheme(string themeName);
}
