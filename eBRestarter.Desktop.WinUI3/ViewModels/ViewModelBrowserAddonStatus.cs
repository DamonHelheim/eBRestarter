using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using Microsoft.UI.Dispatching;
using System;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// View model for a single browser's row in the "Install Add-on" dialog. Shows whether the
    /// browser is installed and whether the eBesucher extension is present; exposes a command to
    /// open the store/extension page. Status is updated by the parent via <see cref="RefreshStatus"/>.
    /// </summary>
    public partial class ViewModelBrowserAddonStatus : ObservableObject
    {
        private const string AddonPlaceholderDash = "-";

        private const string AddonStatusColorExtensionInstalledHex = "#7ED422";

        private const string AddonStatusColorExtensionNotInstalledHex = "#E40E87";

        private const string AddonStatusIconDefault = "?";

        private const string AddonStatusIconEmpty = "";

        private const string AddonStatusIconExtensionInstalled = "\u2713";

        private const string AddonStatusIconExtensionNotInstalled = "\u2717";

        private const string InstallStatusColorBrowserInstalledHex = "#2e7d32";

        private const string InstallStatusColorBrowserNotInstalledHex = "#BA224D";

        private const string NeutralForegroundHex = "#000000";

        private readonly IBrowser _browser;

        private readonly DispatcherQueue _dispatcherQueue;

        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        public partial string AddonStatusColor { get; set; }

        [ObservableProperty]
        public partial string AddonStatusIcon { get; set; }

        [ObservableProperty]
        public partial string AddonStatusText { get; set; }

        [ObservableProperty]
        public partial string BrowserName { get; set; }

        [ObservableProperty]
        public partial string ButtonText { get; set; }

        [ObservableProperty]
        public partial string IconPath { get; set; }

        [ObservableProperty]
        public partial string InstallStatusColor { get; set; }

        [ObservableProperty]
        public partial string InstallStatusText { get; set; }


        [ObservableProperty]
        public partial bool IsButtonEnabled { get; set; }

        /// <summary>
        /// Initializes the row with a browser instance and localization, sets display name and icon path
        /// (adjusted for WinUI asset path), and runs an initial <see cref="RefreshStatus"/> so the first
        /// paint shows install/extension state.
        /// </summary>
        public ViewModelBrowserAddonStatus(
            IBrowser browser,
            ILocalizationService localizationService)
        {
            ArgumentNullException.ThrowIfNull(browser);
            ArgumentNullException.ThrowIfNull(localizationService);

            _browser = browser;
            _localizationService = localizationService;

            _dispatcherQueue =
                DispatcherQueue.GetForCurrentThread()
                ?? throw new InvalidOperationException(
                    $"{nameof(ViewModelBrowserAddonStatus)} must be constructed on a thread with a WinUI DispatcherQueue (UI thread).");

            BrowserName = _browser.DisplayName;
            IconPath = _browser.IconPath;
            AddonStatusColor = NeutralForegroundHex;
            AddonStatusIcon = AddonStatusIconDefault;
            InstallStatusColor = NeutralForegroundHex;
            AddonStatusText = _localizationService.GetString("Addon_StatusChecking");
            InstallStatusText = _localizationService.GetString("Addon_StatusChecking");
            ButtonText = _localizationService.GetString("Addon_Loading");

            RefreshStatus();
        }

        /// <summary>Opens the browser's extension/store URL in the browser if the URL is set (e.g. Chrome Web Store).</summary>
        [RelayCommand]
        private void OpenStore()
        {
            if (!string.IsNullOrWhiteSpace(_browser.ExtensionInstallUrl))
                _browser.Start(_browser.ExtensionInstallUrl);
        }

        /// <summary>
        /// Re-queries the browser instance for install and extension state on the UI thread, then
        /// updates all status text, colors, and button state so the row reflects current state
        /// (installed/not installed, extension installed/not installed, button open store or manage).
        /// </summary>
        public void RefreshStatus()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                string installedFormat = _localizationService.GetString("Addon_BrowserInstalled");
                string notInstalledFormat = _localizationService.GetString("Addon_BrowserNotInstalled");

                if (!_browser.IsInstalled)
                {
                    InstallStatusText = string.Format(notInstalledFormat, BrowserName);
                    InstallStatusColor = InstallStatusColorBrowserNotInstalledHex;
                    AddonStatusText = AddonPlaceholderDash;
                    AddonStatusIcon = AddonStatusIconEmpty;
                    IsButtonEnabled = false;
                    ButtonText = _localizationService.GetString("Addon_BtnBrowserMissing");
                    return;
                }

                InstallStatusText = string.Format(installedFormat, BrowserName);
                InstallStatusColor = InstallStatusColorBrowserInstalledHex;
                IsButtonEnabled = true;

                bool extensionIsInstalled = _browser.IsExtensionInstalled(string.Empty);

                if (extensionIsInstalled)
                {
                    AddonStatusText = _localizationService.GetString("Addon_ExtensionInstalled");
                    AddonStatusIcon = AddonStatusIconExtensionInstalled;
                    AddonStatusColor = AddonStatusColorExtensionInstalledHex;
                    ButtonText = _localizationService.GetString("Addon_BtnManage");
                }
                else
                {
                    AddonStatusText = _localizationService.GetString("Addon_ExtensionNotInstalled");
                    AddonStatusIcon = AddonStatusIconExtensionNotInstalled;
                    AddonStatusColor = AddonStatusColorExtensionNotInstalledHex;
                    ButtonText = _localizationService.GetString("Addon_BtnInstall");
                }
            });
        }
    }
}
