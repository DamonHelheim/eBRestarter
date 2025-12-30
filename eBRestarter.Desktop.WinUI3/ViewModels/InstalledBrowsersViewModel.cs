using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class InstalledBrowsersViewModel : ObservableObject
    {
        private readonly IBrowserService _browserService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly IBrowserDownloadService _downloadService;
        private readonly IOperatingSystemFacade _os;

        [ObservableProperty]
        public partial ObservableCollection<BrowserItemViewModel> Browsers { get; set; } = [];

        public InstalledBrowsersViewModel(
            IBrowserService browserService,
            IBrowserDownloadService downloadService,
            IOperatingSystemFacade os,
            IEVisitorConfigService eVisitorConfigService)
        {
            _downloadService = downloadService;
            _os = os;
            _browserService = browserService;
            _eVisitorConfigService = eVisitorConfigService;
            // Initiale Ladung oder Start eines Timers
            LoadBrowsers();
        }

        public async void LoadBrowsers()
        {
            var browserInfos = await _browserService.GetInstalledBrowsersAsync();

            Browsers.Clear();

            foreach (var info in browserInfos)
            {
                Browsers.Add(new BrowserItemViewModel(info, _downloadService, _os, _eVisitorConfigService));
            }
        }
    }
}
