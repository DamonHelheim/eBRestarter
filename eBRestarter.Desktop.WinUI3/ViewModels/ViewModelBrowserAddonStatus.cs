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
        #region Fields

        private readonly IBrowser _browser;
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ILocalizationService _localizationService; // <--- NEU

        #endregion

        #region Observable Properties

        [ObservableProperty] public partial string AddonStatusColor { get; set; } = "#000000";
        [ObservableProperty] public partial string AddonStatusIcon { get; set; } = "?";

        // Initialwert wird im Constructor gesetzt
        [ObservableProperty] public partial string AddonStatusText { get; set; }
        [ObservableProperty] public partial string BrowserName { get; set; } = string.Empty;

        // Initialwert wird im Constructor gesetzt
        [ObservableProperty] public partial string ButtonText { get; set; }
        [ObservableProperty] public partial string IconPath { get; set; } = string.Empty;
        [ObservableProperty] public partial string InstallStatusColor { get; set; } = "#000000";

        // Initialwert wird im Constructor gesetzt
        [ObservableProperty] public partial string InstallStatusText { get; set; }
        [ObservableProperty] public partial bool IsButtonEnabled { get; set; } = false;

        #endregion

        #region Constructors

        public ViewModelBrowserAddonStatus(
            IBrowser browser,
            ILocalizationService localizationService) // <--- Injizieren
        {
            _browser = browser;
            _localizationService = localizationService; // <--- Zuweisen
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            BrowserName = _browser.DisplayName;

            // Pfad-Korrektur für WinUI
            IconPath = browser.IconPath.Replace("/Resources/Visuals", "ms-appx:///Assets/Visuals");

            // Lokalisierte Standardwerte
            AddonStatusText = _localizationService.GetString("Addon_StatusChecking"); // "Prüfe..."
            InstallStatusText = _localizationService.GetString("Addon_StatusChecking"); // "Prüfe..."
            ButtonText = _localizationService.GetString("Addon_Loading"); // "Lade..."

            RefreshStatus();
        }

        #endregion

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

        #region Methods

        public void RefreshStatus()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                // Texte laden
                string installedFormat = _localizationService.GetString("Addon_BrowserInstalled"); // "{0} ist installiert"
                string notInstalledFormat = _localizationService.GetString("Addon_BrowserNotInstalled"); // "{0} ist nicht installiert"

                // 1. Browser installiert?
                if (_browser.IsInstalled)
                {
                    InstallStatusText = string.Format(installedFormat, BrowserName);
                    InstallStatusColor = "{ThemeResource TextFillColorPrimaryBrush}";
                    IsButtonEnabled = true;

                    // 2. Addon installiert?
                    bool hasAddon = _browser.IsExtensionInstalled(null);

                    if (hasAddon)
                    {
                        AddonStatusText = _localizationService.GetString("Addon_ExtensionInstalled"); // "Add-on ist installiert"
                        AddonStatusIcon = "✓";
                        AddonStatusColor = "#7ED422"; // Grün
                        ButtonText = _localizationService.GetString("Addon_BtnManage"); // "Add-on verwalten"
                    }
                    else
                    {
                        AddonStatusText = _localizationService.GetString("Addon_ExtensionNotInstalled"); // "Add-on ist nicht installiert"
                        AddonStatusIcon = "✘";
                        AddonStatusColor = "#E40E87"; // Rot/Pink
                        ButtonText = _localizationService.GetString("Addon_BtnInstall"); // "Add-on installieren"
                    }
                }
                else
                {
                    InstallStatusText = string.Format(notInstalledFormat, BrowserName);
                    InstallStatusColor = "#BA224D"; // Dunkelrot

                    AddonStatusText = "-";
                    AddonStatusIcon = "";
                    IsButtonEnabled = false;
                    ButtonText = _localizationService.GetString("Addon_BtnBrowserMissing"); // "Browser fehlt"
                }
            });
        }

        #endregion
    }
}