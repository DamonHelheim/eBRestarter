using System;
using eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Helpers.Interfaces;
using Microsoft.UI;
using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.BehavioralUIComponents.Helpers;

/// <summary>
/// Helper for application window and title bar configuration.
/// </summary>
public sealed class AppWindowHelper : IAppWindowHelper
{
    /// <summary>
    /// Configures title bar colors and extends content into the title bar for the given window.
    /// </summary>
    /// <param name="window">The WinUI window to configure.</param>
    public void ConfigureTitleBarColors(Window window)
    {
        // ✅ .NET 10 Guard Clause
        ArgumentNullException.ThrowIfNull(window);

        var titleBar = window.AppWindow.TitleBar;
        titleBar.ExtendsContentIntoTitleBar = true;
        titleBar.BackgroundColor = ColorHelper.FromArgb(255, 32, 37, 54);

        window.ExtendsContentIntoTitleBar = true;
        window.SetTitleBar(null);
    }

    /// <summary>
    /// Updates the title bar button colors based on the requested theme.
    /// </summary>
    public void UpdateTitleBarTheme(Window window, ElementTheme theme)
    {
        // ✅ .NET 10 Guard Clause
        ArgumentNullException.ThrowIfNull(window);

        var titleBar = window.AppWindow.TitleBar;

        if (theme == ElementTheme.Dark)
        {
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonHoverForegroundColor = Colors.White;
            titleBar.ButtonHoverBackgroundColor = ColorHelper.FromArgb(51, 255, 255, 255);
            titleBar.ButtonPressedForegroundColor = Colors.White;
            titleBar.ButtonPressedBackgroundColor = ColorHelper.FromArgb(77, 255, 255, 255);
            titleBar.ButtonInactiveForegroundColor = Colors.Gray;
        }
        else
        {
            titleBar.ButtonForegroundColor = Colors.Black;
            titleBar.ButtonHoverForegroundColor = Colors.Black;
            titleBar.ButtonHoverBackgroundColor = ColorHelper.FromArgb(51, 0, 0, 0);
            titleBar.ButtonPressedForegroundColor = Colors.Black;
            titleBar.ButtonPressedBackgroundColor = ColorHelper.FromArgb(77, 0, 0, 0);
            titleBar.ButtonInactiveForegroundColor = Colors.Gray;
        }
    }
}
