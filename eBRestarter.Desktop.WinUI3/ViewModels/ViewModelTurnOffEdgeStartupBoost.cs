using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelTurnOffEdgeStartupBoost : ObservableObject
    {
        private readonly IWindowsStartupManagerService _startupService;
        private readonly IBrowserFactory _browserFactory;
        private readonly IDialogService _dialogService;
        private readonly ILocalizationService _localizationService;

        // Flag, um Rekursion beim Zurücksetzen im Fehlerfall zu vermeiden
        private bool _isRevertingState = false;

        [ObservableProperty]
        public partial bool IsStartupBoostEnabled { get; set; }

        // --- NEU: Eigenschaften für die InfoBar ---
        [ObservableProperty]
        public partial bool IsInfoBarOpen { get; set; } = false;

        [ObservableProperty]
        public partial string InfoBarTitle { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string InfoBarMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial InfoBarSeverity InfoBarSeverity { get; set; } = InfoBarSeverity.Informational;

        // --- NEU: Eigenschaft statt Command ---
        public ViewModelTurnOffEdgeStartupBoost(
            IWindowsStartupManagerService startupService,
            IBrowserFactory browserFactory,
            IDialogService dialogService,
            ILocalizationService localizationService)
        {
            _startupService = startupService;
            _browserFactory = browserFactory;
            _dialogService = dialogService;
            _localizationService = localizationService;

            // 1. Aktuellen Status beim Start laden
            // WICHTIG: Wir setzen das Feld direkt (_...), damit "OnChanged" beim Start NICHT feuert!
            IsStartupBoostEnabled = _startupService.IsEdgeStartupBoostEnabled();
        }

        // --- Diese Methode wird automatisch aufgerufen, wenn der ToggleSwitch geklickt wird ---
        // 'value' ist hier dein true/false Parameter!
        async partial void OnIsStartupBoostEnabledChanged(bool value)
        {
            if (_isRevertingState) return;

            try
            {
                IsInfoBarOpen = false;
                _startupService.SetEdgeStartupBoost(value);

                // Titel holen
                InfoBarTitle = _localizationService.GetString("StartupBoostDialog_SuccessTitle");

                // Nachricht je nach Status holen
                InfoBarMessage = value
                    ? _localizationService.GetString("StartupBoostDialog_SuccessStatus_Activated") // "Wurde aktiviert"
                    : _localizationService.GetString("StartupBoostDialog_SuccessMessage");         // "Wurde deaktiviert"

                InfoBarSeverity = InfoBarSeverity.Success;
                IsInfoBarOpen = true;
            }
            catch (Exception ex)
            {
                _isRevertingState = true;
                IsStartupBoostEnabled = !value;
                _isRevertingState = false;

                // Fehler Titel lokalisieren
                InfoBarTitle = _localizationService.GetString("StartupBoostDialog_ErrorTitle"); // "Fehler" / "Error"
                InfoBarMessage = ex.Message;
                InfoBarSeverity = InfoBarSeverity.Error;
                IsInfoBarOpen = true;
            }
        }

        // OPTION 2: Manuell (Alte Logik, modernisiert)
        [RelayCommand]
        private async Task CopyAndOpenEdge()
        {
            var edge = _browserFactory.Create(BrowserType.Edge);

            if (edge.IsInstalled)
            {
                // 1. Link in Zwischenablage (WinUI 3 Weg)
                var dataPackage = new DataPackage();
                dataPackage.SetText("edge://settings/?search=Startup-Boost");
                Clipboard.SetContent(dataPackage);

                // 2. Info anzeigen
                await _dialogService.ShowMessageAsync(
                    "Edge",
                    _localizationService.GetString("StartupBoostDialog_CopyMessage"),
                    DialogIcon.Information);

                // 3. Edge starten
                //edge.Start();
            }
            else
            {
                await _dialogService.ShowMessageAsync(
                    "Edge",
                    _localizationService.GetString("Browser_NotInstalled"),
                    DialogIcon.Error);
            }
        }
    }
}
