using eBRestarter.Desktop.WinUI3.Services.Interfaces;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// Root view model for the main application shell. Holds no page-specific state; navigation
    /// is delegated to <see cref="INavigationService"/> so the shell can switch between pages
    /// (e.g. Restart Task, Options, About) without holding view state here.
    /// </summary>
    public class MainViewModel;
}
