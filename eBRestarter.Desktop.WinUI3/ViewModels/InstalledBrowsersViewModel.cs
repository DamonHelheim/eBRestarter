using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces.Browser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class InstalledBrowsersViewModel : ObservableObject
    {
        private readonly IBrowserService _browserService;

        [ObservableProperty]
        public partial ObservableCollection<BrowserItemViewModel> Browsers { get; set; } = [];

        public InstalledBrowsersViewModel(IBrowserService browserService)
        {
            _browserService = browserService;
            // Initiale Ladung oder Start eines Timers
            LoadBrowsers();
        }

        public async void LoadBrowsers()
        {
            var browserInfos = await _browserService.GetInstalledBrowsersAsync();
            Browsers.Clear();
            foreach (var info in browserInfos)
            {
                Browsers.Add(new BrowserItemViewModel(info));
            }
        }
    }
}
