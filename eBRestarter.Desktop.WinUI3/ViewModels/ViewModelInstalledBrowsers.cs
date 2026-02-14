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
    public partial class ViewModelInstalledBrowsers : ObservableObject, IDisposable
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IBrowserService _browserService;
        private readonly IDialogService _dialogService;
        private readonly IBrowserDownloadService _downloadService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly IOperatingSystemFacade _os;
        private readonly ILocalizationService _localizationService;
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

        public async Task LoadBrowsersSmartAsync()
        {
            var freshBrowserInfos = await _browserService.GetInstalledBrowsersAsync();

            foreach (var freshInfo in freshBrowserInfos)
            {
                var existingItem = Browsers.FirstOrDefault(vm => vm.BrowserType == freshInfo.Type);

                if (existingItem != null)
                {
                    existingItem.Update(freshInfo);
                }
                else
                {
                    var newItem = new ViewModelBrowserItem(
                        freshInfo,
                        _downloadService,
                        _os,
                        _eVisitorConfigService,
                        _dialogService,
                        _localizationService);
                    Browsers.Add(newItem);
                }
            }
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
