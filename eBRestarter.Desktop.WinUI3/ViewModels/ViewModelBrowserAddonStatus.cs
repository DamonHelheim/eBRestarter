using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        #region Fields (Private Felder OHNE [ObservableProperty])

        private readonly IBrowser _browser;
        private readonly DispatcherQueue _dispatcherQueue;

        #endregion

        #region Observable Properties (Felder MIT [ObservableProperty])

        [ObservableProperty] public partial string AddonStatusColor { get; set; } = "#000000";
        [ObservableProperty] public partial string AddonStatusIcon { get; set; } = "?";
        [ObservableProperty] public partial string AddonStatusText { get; set; } = "Prüfe...";
        [ObservableProperty] public partial string BrowserName { get; set; } = string.Empty;
        [ObservableProperty] public partial string ButtonText { get; set; } = "Lade...";
        [ObservableProperty] public partial string IconPath { get; set; } = string.Empty;
        [ObservableProperty] public partial string InstallStatusColor { get; set; } = "#000000"; // Default
        [ObservableProperty] public partial string InstallStatusText { get; set; } = "Prüfe...";
        [ObservableProperty] public partial bool IsButtonEnabled { get; set; } = false;

        #endregion

        #region Constructors

        public ViewModelBrowserAddonStatus(IBrowser browser)
        {
            _browser = browser;

            _dispatcherQueue = DispatcherQueue.GetForCurrentThread(); // UI Thread merken

            BrowserName = _browser.DisplayName;

            // Pfad-Korrektur für WinUI, falls IconPath im WPF-Format ("/Resources/...") kommt
            IconPath = browser.IconPath.Replace("/Resources/Visuals", "ms-appx:///Assets/Visuals");

            RefreshStatus();
        }

        #endregion

        #region Commands (Methoden MIT [RelayCommand])

        [RelayCommand]
        private void OpenStore()
        {
            if (!string.IsNullOrEmpty(_browser.ExtensionInstallUrl))
            {
                // Öffnet den Link direkt mit dem passenden Browser
                _browser.Start(_browser.ExtensionInstallUrl);
            }
        }

        #endregion

        #region Methods (Restliche Methoden)

        public void RefreshStatus()
        {
            // Da dies vom Timer aufgerufen werden kann, müssen wir sicherstellen,
            // dass Property-Changes auf dem UI-Thread passieren.
            _dispatcherQueue.TryEnqueue(() =>
            {
                // 1. Browser installiert?
                if (_browser.IsInstalled)
                {
                    InstallStatusText = $"{BrowserName} ist installiert";
                    InstallStatusColor = "{ThemeResource TextFillColorPrimaryBrush}"; // Oder Hex-Code wenn keine Resource
                    IsButtonEnabled = true;

                    // 2. Addon installiert? (null = Default ID des Browsers nutzen)
                    bool hasAddon = _browser.IsExtensionInstalled(null);

                    if (hasAddon)
                    {
                        AddonStatusText = "Add-on ist installiert";
                        AddonStatusIcon = "✓";
                        AddonStatusColor = "#7ED422"; // Grün
                        ButtonText = "Add-on verwalten"; // Deinstallieren geht meist nur manuell im Browser
                    }
                    else
                    {
                        AddonStatusText = "Add-on ist nicht installiert";
                        AddonStatusIcon = "✘";
                        AddonStatusColor = "#E40E87"; // Rot/Pink
                        ButtonText = "Add-on installieren";
                    }
                }
                else
                {
                    InstallStatusText = $"{BrowserName} ist nicht installiert";
                    InstallStatusColor = "#BA224D"; // Dunkelrot

                    AddonStatusText = "-";
                    AddonStatusIcon = "";
                    IsButtonEnabled = false;
                    ButtonText = "Browser fehlt";
                }
            });
        }

        #endregion
    }
}
