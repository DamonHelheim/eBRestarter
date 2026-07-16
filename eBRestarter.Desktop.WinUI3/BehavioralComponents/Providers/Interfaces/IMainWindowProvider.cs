using Microsoft.UI.Xaml;

namespace eBRestarter.Desktop.WinUI3.BehavioralComponents.Providers.Interfaces;

public interface IMainWindowProvider
{
    Window MainWindow { get; }
    void SetMainWindow(Window window);
}
