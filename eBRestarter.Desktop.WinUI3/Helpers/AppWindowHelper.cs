using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.Helpers;

/// <summary>
/// Static helper for application window and title bar configuration.
/// </summary>
public static class AppWindowHelper
{
    /// <summary>
    /// Configures title bar colors and extends content into the title bar for the given window.
    /// </summary>
    /// <param name="window">The WinUI window to configure.</param>
    public static void ConfigureTitleBarColors(this Window window)
    {
        var appWindow = window.AppWindow;
        var titleBar = appWindow.TitleBar;

        var bg = Microsoft.UI.ColorHelper.FromArgb(255, 32, 37, 54);

        titleBar.ExtendsContentIntoTitleBar = true;
        titleBar.BackgroundColor = bg;

        window.ExtendsContentIntoTitleBar = true;
        window.SetTitleBar(null);
    }

    /// <summary>
    /// Updates the title bar button colors based on the requested theme.
    /// </summary>
    public static void UpdateTitleBarTheme(this Window window, ElementTheme theme)
    {
        var titleBar = window.AppWindow.TitleBar;

        if (theme == ElementTheme.Dark)
        {
            titleBar.ButtonForegroundColor = Microsoft.UI.Colors.White;
            titleBar.ButtonHoverForegroundColor = Microsoft.UI.Colors.White;
            titleBar.ButtonHoverBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(51, 255, 255, 255);
            titleBar.ButtonPressedForegroundColor = Microsoft.UI.Colors.White;
            titleBar.ButtonPressedBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(77, 255, 255, 255);
            titleBar.ButtonInactiveForegroundColor = Microsoft.UI.Colors.Gray;
        }
        else
        {
            titleBar.ButtonForegroundColor = Microsoft.UI.Colors.Black;
            titleBar.ButtonHoverForegroundColor = Microsoft.UI.Colors.Black;
            titleBar.ButtonHoverBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(51, 0, 0, 0);
            titleBar.ButtonPressedForegroundColor = Microsoft.UI.Colors.Black;
            titleBar.ButtonPressedBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(77, 0, 0, 0);
            titleBar.ButtonInactiveForegroundColor = Microsoft.UI.Colors.Gray;
        }
    }
}
