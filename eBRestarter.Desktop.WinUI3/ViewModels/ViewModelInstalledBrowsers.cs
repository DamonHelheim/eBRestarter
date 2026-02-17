using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// View model for the "Installed Browsers" page. Keeps a list of <see cref="ViewModelBrowserItem"/>
    /// in sync with <see cref="IBrowserService.GetInstalledBrowsersAsync"/>: updates existing items
    /// when install state or version changes and adds new items when a new browser type appears.
    /// Refreshes on a 2-second timer so the list stays current (e.g. after download/install).
    /// </summary>
    public partial class ViewModelInstalledBrowsers : ObservableObject, IDisposable
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Fields und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IBrowserService _browserService;
        private readonly IBrowserDownloadService _downloadService;
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly ILocalizationService _localizationService;
        private readonly IOperatingSystemFacade _os;
        private readonly DispatcherTimer _refreshTimer;

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty]
        public partial ObservableCollection<ViewModelBrowserItem> Browsers { get; set; } = [];

        #endregion

        // =========================================================
        // 3. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Wires up services and a 2-second dispatcher timer that repeatedly calls
        /// <see cref="LoadBrowsersSmartAsync"/> so the browser list stays in sync with
        /// installed browsers and versions. Kicks off the first load immediately.
        /// </summary>
        public ViewModelInstalledBrowsers(
            IBrowserService browserService,
            IBrowserDownloadService downloadService,
            IOperatingSystemFacade os,
            IDialogService dialogService,
            ILocalizationService localizationService,
            IEVisitorConfigService eVisitorConfigService)
        {
            _downloadService = downloadService;
            _os = os;
            _browserService = browserService;
            _dialogService = dialogService;
            _localizationService = localizationService;
            _eVisitorConfigService = eVisitorConfigService;

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _refreshTimer.Tick += async (s, e) => await LoadBrowsersSmartAsync();
            _ = LoadBrowsersSmartAsync();
            _refreshTimer.Start();
        }

        #endregion

        // =========================================================
        // 4. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        /// <summary>
        /// Fetches the current list of installed browsers from the service. For each result,
        /// either updates the matching existing <see cref="ViewModelBrowserItem"/> (by browser type)
        /// or creates a new one and adds it, so the UI list reflects current install state and versions.
        /// </summary>
        public async Task LoadBrowsersSmartAsync()
        {
            var freshBrowserInfos = await _browserService.GetInstalledBrowsersAsync();

            foreach (var freshInfo in freshBrowserInfos)
            {
                var existingBrowserItem = Browsers.FirstOrDefault(browserItem => browserItem.BrowserType == freshInfo.Type);

                if (existingBrowserItem != null)
                {
                    existingBrowserItem.Update(freshInfo);
                }
                else
                {
                    var newBrowserItem = new ViewModelBrowserItem(
                        freshInfo,
                        _downloadService,
                        _os,
                        _eVisitorConfigService,
                        _dialogService,
                        _localizationService);
                    Browsers.Add(newBrowserItem);
                }
            }
        }

        /// <summary>Stops the refresh timer. Call when leaving the page or disposing the VM to avoid leaks.</summary>
        public void Dispose()
        {
            _refreshTimer?.Stop();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
