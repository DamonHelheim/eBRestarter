using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;

/// <summary>
/// Provider interface for accessing and configuring the main application window instance.
/// </summary>
public interface IMainWindowProvider
{
    /// <summary>
    /// Gets the active main application window instance.
    /// </summary>
    Window MainWindow { get; }

    /// <summary>
    /// Sets the active main application window instance.
    /// </summary>
    /// <param name="window">The target <see cref="Window"/> instance to store.</param>
    void SetMainWindow(Window window);
}
