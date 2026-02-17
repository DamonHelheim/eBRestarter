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
    /// <summary>
    /// View model for the "Install Add-on" dialog. Holds a fixed list of browser add-on status
    /// view models (Chrome, Firefox, Edge, Brave) and refreshes their install/extension state
    /// on a timer so the user sees up-to-date status without manually refreshing.
    /// </summary>
    public partial class ViewModelInstallAddOn : ObservableObject, IDisposable
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Fields an DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IBrowserFactory _browserFactory;
        private readonly Timer _timer;

        #endregion

        // =========================================================
        // 2. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        /// <summary>One entry per supported browser (Chrome, Firefox, Edge, Brave), each showing install and extension status.</summary>
        public ObservableCollection<ViewModelBrowserAddonStatus> Browsers { get; } = [];

        #endregion

        // =========================================================
        // 3. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Creates browser instances for all supported types, wraps each in a
        /// <see cref="ViewModelBrowserAddonStatus"/>, and starts a 2-second timer that refreshes
        /// each entry so install/extension state stays current (e.g. after user installs the add-on).
        /// </summary>
        /// <param name="browserFactory">Used to create browser instances for status checks. Must not be null.</param>
        /// <param name="localizationService">Passed to each ViewModelBrowserAddonStatus for localized strings. Must not be null.</param>
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

        /// <summary>Stops and disposes the refresh timer so the dialog can close without background updates.</summary>
        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }

        #endregion
    }
}
