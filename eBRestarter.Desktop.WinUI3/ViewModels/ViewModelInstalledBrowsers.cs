using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelInstalledBrowsers : ObservableObject
    {
        #region Fields
        private readonly IBrowserService _browserService;
        private readonly IDialogService _dialogService;
        private readonly IBrowserDownloadService _downloadService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly IOperatingSystemFacade _os;
        private readonly ILocalizationService _localizationService;
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
            // Initiale Ladung oder Start eines Timers
            LoadBrowsers();
        }
        #endregion

        #region Methods
        public async void LoadBrowsers()
        {
            var browserInfos = await _browserService.GetInstalledBrowsersAsync();

            Browsers.Clear();

            foreach (var info in browserInfos)
            {
                Browsers.Add(new ViewModelBrowserItem(info, _downloadService, _os, _eVisitorConfigService, _dialogService, _localizationService));
            }
        }
        #endregion
    }
}
