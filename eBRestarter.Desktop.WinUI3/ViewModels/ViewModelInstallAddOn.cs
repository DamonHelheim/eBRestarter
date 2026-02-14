using CommunityToolkit.Mvvm.ComponentModel;
using eBRestarter.Core.Application.Interfaces;
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
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IBrowserFactory _browserFactory;
        private readonly Timer _timer;

        #endregion

        // =========================================================
        // 2. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        public ObservableCollection<ViewModelBrowserAddonStatus> Browsers { get; } = [];

        #endregion

        // =========================================================
        // 3. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public ViewModelInstallAddOn(IBrowserFactory browserFactory, ILocalizationService localizationService)
        {
            _browserFactory = browserFactory;

            Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Chrome), localizationService));
            Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Firefox), localizationService));
            Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Edge), localizationService));
            Browsers.Add(new ViewModelBrowserAddonStatus(_browserFactory.Create(BrowserType.Brave), localizationService));

            _timer = new Timer(2000);
            _timer.Elapsed += (s, e) =>
            {
                foreach (var b in Browsers) b.RefreshStatus();
            };
            _timer.Start();
        }

        #endregion

        // =========================================================
        // 4. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }

        #endregion
    }
}
