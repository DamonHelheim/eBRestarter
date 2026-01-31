using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml; // WICHTIG: Für DispatcherTimer in WinUI 3
using System;
using System.Collections.ObjectModel;
using System.Linq; // Wichtig für FirstOrDefault
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelInstalledBrowsers : ObservableObject, IDisposable
    {
        #region Fields
        private readonly IBrowserService _browserService;
        private readonly IDialogService _dialogService;
        private readonly IBrowserDownloadService _downloadService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly IOperatingSystemFacade _os;
        private readonly ILocalizationService _localizationService;

        // Timer für regelmäßige Updates
        private readonly DispatcherTimer _refreshTimer;
        #endregion

        #region Observable Properties
        [ObservableProperty]
        public partial ObservableCollection<ViewModelBrowserItem> Browsers { get; set; } = [];
        #endregion

        #region Constructors
        public ViewModelInstalledBrowsers(
            IBrowserService browserService,
            IBrowserDownloadService downloadService,
            IOperatingSystemFacade os,
            IDialogService dialogService,
            ILocalizationService localizationService,
            IEVisitorConfigService eVisitorConfigService
            )
        {
            _downloadService = downloadService;
            _os = os;
            _browserService = browserService;
            _dialogService = dialogService;
            _localizationService = localizationService;
            _eVisitorConfigService = eVisitorConfigService;

            // 1. Timer initialisieren
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2) // Alle 5 Sekunden prüfen
            };

            _refreshTimer.Tick += async (s, e) => await LoadBrowsersSmartAsync();

            // 2. Initial laden (Fire & Forget)
            _ = LoadBrowsersSmartAsync();

            // 3. Timer starten
            _refreshTimer.Start();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Lädt Browser-Informationen und aktualisiert die bestehende Liste, 
        /// anstatt sie zu löschen. Verhindert UI-Flackern.
        /// </summary>
        public async Task LoadBrowsersSmartAsync()
        {
            // Daten abrufen (Registry-Checks, läuft schnell)
            var freshBrowserInfos = await _browserService.GetInstalledBrowsersAsync();

            // Wir iterieren über die neuen Daten
            foreach (var freshInfo in freshBrowserInfos)
            {
                // Versuchen, das existierende ViewModel für diesen Browser-Typ zu finden
                var existingItem = Browsers.FirstOrDefault(vm => vm.BrowserType == freshInfo.Type);

                if (existingItem != null)
                {
                    // FALL A: Item existiert -> Update aufrufen
                    // Die Update-Methode im Item kümmert sich darum, nur PropertyChanged zu feuern, wenn nötig.
                    existingItem.Update(freshInfo);
                }
                else
                {
                    // FALL B: Item existiert noch nicht -> Neu hinzufügen
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

            // Optional: Entfernen von Browsern, die nicht mehr in der Liste sind (bei Enums selten nötig, aber sauber)
            // Wir prüfen, ob es Items in 'Browsers' gibt, deren Typ NICHT in 'freshBrowserInfos' vorkommt.
            //for (int i = Browsers.Count - 1; i >= 0; i--)
            //{
            //    var currentItem = Browsers[i];
            //    if (!freshBrowserInfos.Any(info => info.Type == currentItem.BrowserType))
            //    {
            //        Browsers.RemoveAt(i);
            //    }
            //}
        }

        public void Dispose()
        {
            _refreshTimer?.Stop();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}