using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Helpers.Interfaces;

/// <summary>
/// Helper interface for configuring WinUI application window title bar visual attributes and themes.
/// </summary>
public interface IAppWindowHelper
{
    /// <summary>
    /// Configures title bar colors and extends window content into the title bar area.
    /// </summary>
    /// <param name="window">The target WinUI <see cref="Window"/> instance.</param>
    void ConfigureTitleBarColors(Window window);

    /// <summary>
    /// Updates title bar caption button colors to align with the requested element theme.
    /// </summary>
    /// <param name="window">The target WinUI <see cref="Window"/> instance.</param>
    /// <param name="theme">The requested <see cref="ElementTheme"/> value.</param>
    void UpdateTitleBarTheme(Window window, ElementTheme theme);
}
