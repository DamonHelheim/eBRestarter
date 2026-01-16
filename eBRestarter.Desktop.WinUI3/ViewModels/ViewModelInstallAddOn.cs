using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Timers;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelInstallAddOn : ObservableObject, IDisposable
    {
        #region Fields
        private readonly IBrowserFactory _browserFactory;
        private readonly Timer _timer;
        #endregion

        #region Properties
        public ObservableCollection<ViewModelBrowserAddonStatus> Browsers { get; } = [];
        #endregion

        #region Constructors
        public ViewModelInstallAddOn(IBrowserFactory browserFactory)
        {
            _browserFactory = browserFactory;

            // Liste initialisieren (Reihenfolge wie gewünscht)
            Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Chrome)));
            Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Firefox)));
            Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Edge)));
            Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Brave)));

            // Timer für Auto-Refresh (alle 2 Sekunden)
            _timer = new Timer(2000);
            _timer.Elapsed += (s, e) =>
            {
                foreach (var b in Browsers) b.RefreshStatus();
            };
            _timer.Start();
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
        #endregion
    }
}
