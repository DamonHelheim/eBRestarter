using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Infrastructure.Browsers.Abstract;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelBrowserAddonStatus : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IBrowser _browser;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ILocalizationService _localizationService;

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial string AddonStatusColor { get; set; } = "#000000";
        [ObservableProperty] public partial string AddonStatusIcon { get; set; } = "?";
        [ObservableProperty] public partial string AddonStatusText { get; set; }
        [ObservableProperty] public partial string BrowserName { get; set; } = string.Empty;
        [ObservableProperty] public partial string ButtonText { get; set; }
        [ObservableProperty] public partial string IconPath { get; set; } = string.Empty;
        [ObservableProperty] public partial string InstallStatusColor { get; set; } = "#000000";
        [ObservableProperty] public partial string InstallStatusText { get; set; }
        [ObservableProperty] public partial bool IsButtonEnabled { get; set; } = false;

        #endregion

        // =========================================================
        // 3. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public ViewModelBrowserAddonStatus(
            IBrowser browser,
            ILocalizationService localizationService)
        {
            _browser = browser;
            _localizationService = localizationService;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            BrowserName = _browser.DisplayName;
            IconPath = browser.IconPath.Replace("/Resources/Visuals", "ms-appx:///Assets/Visuals");
            AddonStatusText = _localizationService.GetString("Addon_StatusChecking");
            InstallStatusText = _localizationService.GetString("Addon_StatusChecking");
            ButtonText = _localizationService.GetString("Addon_Loading");

            RefreshStatus();
        }

        #endregion

        // =========================================================
        // 4. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        [RelayCommand]
        private void OpenStore()
        {
            if (!string.IsNullOrEmpty(_browser.ExtensionInstallUrl))
            {
                _browser.Start(_browser.ExtensionInstallUrl);
            }
        }

        #endregion

        // =========================================================
        // 5. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        public void RefreshStatus()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                string installedFormat = _localizationService.GetString("Addon_BrowserInstalled");
                string notInstalledFormat = _localizationService.GetString("Addon_BrowserNotInstalled");

                if (_browser.IsInstalled)
                {
                    InstallStatusText = string.Format(installedFormat, BrowserName);
                    InstallStatusColor = "{ThemeResource TextFillColorPrimaryBrush}";
                    IsButtonEnabled = true;

                    bool hasAddon = _browser.IsExtensionInstalled(null);

                    if (hasAddon)
                    {
                        AddonStatusText = _localizationService.GetString("Addon_ExtensionInstalled");
                        AddonStatusIcon = "✓";
                        AddonStatusColor = "#7ED422";
                        ButtonText = _localizationService.GetString("Addon_BtnManage");
                    }
                    else
                    {
                        AddonStatusText = _localizationService.GetString("Addon_ExtensionNotInstalled");
                        AddonStatusIcon = "✘";
                        AddonStatusColor = "#E40E87";
                        ButtonText = _localizationService.GetString("Addon_BtnInstall");
                    }
                }
                else
                {
                    InstallStatusText = string.Format(notInstalledFormat, BrowserName);
                    InstallStatusColor = "#BA224D";
                    AddonStatusText = "-";
                    AddonStatusIcon = "";
                    IsButtonEnabled = false;
                    ButtonText = _localizationService.GetString("Addon_BtnBrowserMissing");
                }
            });
        }

        #endregion
    }
}
