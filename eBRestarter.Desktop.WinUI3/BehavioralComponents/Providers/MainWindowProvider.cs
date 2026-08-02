using System;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;
using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers;

public sealed class MainWindowProvider : IMainWindowProvider
{
    private Window? _mainWindow;

    public Window MainWindow => _mainWindow ?? throw new InvalidOperationException("MainWindow is not set yet.");

    public void SetMainWindow(Window window)
    {
        // ✅ .NET 10 Guard Clause
        ArgumentNullException.ThrowIfNull(window);

        _mainWindow = window;
    }
}
