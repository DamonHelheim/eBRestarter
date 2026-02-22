using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.Helpers;

/// <summary>
/// Static helper for application window and title bar configuration.
/// </summary>
public static class AppWindowHelper
{
    // =========================================================
    // 1. PUBLIC METHODS (API)
    // =========================================================
    #region PublicMethods

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

    #endregion
}
