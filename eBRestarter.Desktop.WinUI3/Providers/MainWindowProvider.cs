using eBRestarter.Desktop.WinUI3.Providers.Interfaces;
using Microsoft.UI.Xaml;
using System;

namespace eBRestarter.Desktop.WinUI3.Providers;

public sealed class MainWindowProvider : IMainWindowProvider
{
    private Window? _mainWindow;

    public Window MainWindow => _mainWindow ?? throw new InvalidOperationException("MainWindow is not set yet.");

    public void SetMainWindow(Window window)
    {
        _mainWindow = window;
    }
}
