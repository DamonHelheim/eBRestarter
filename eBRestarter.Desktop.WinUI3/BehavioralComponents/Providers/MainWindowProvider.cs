using System;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers;

/// <summary>
/// Implementation of <see cref="IMainWindowProvider"/> storing reference to the active main window.
/// </summary>
public sealed class MainWindowProvider : IMainWindowProvider
{
    private Window? _mainWindow;

    /// <inheritdoc />
    public Window MainWindow => _mainWindow ?? throw new InvalidOperationException("MainWindow is not set yet.");

    /// <inheritdoc />
    public void SetMainWindow(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        _mainWindow = window;
    }
}
