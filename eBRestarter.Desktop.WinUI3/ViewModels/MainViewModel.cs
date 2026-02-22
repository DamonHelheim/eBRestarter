using eBRestarter.Desktop.WinUI3.Services.Interfaces;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// Root view model for the main application shell. Holds no page-specific state; navigation
    /// is delegated to <see cref="INavigationService"/> so the shell can switch between pages
    /// (e.g. Restart Task, Options, About) without holding view state here.
    /// </summary>
    public class MainViewModel
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly INavigationService _navigationService;

        #endregion

        // =========================================================
        // 2. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Creates the main view model and stores the navigation service for later use.
        /// </summary>
        /// <param name="navigationService">Service used to navigate between app pages. Must not be null.</param>
        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        #endregion
    }
}
