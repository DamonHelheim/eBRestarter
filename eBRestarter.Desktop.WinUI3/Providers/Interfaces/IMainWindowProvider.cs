using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.Providers.Interfaces;

public interface IMainWindowProvider
{
    Window MainWindow { get; }
    void SetMainWindow(Window window);
}
