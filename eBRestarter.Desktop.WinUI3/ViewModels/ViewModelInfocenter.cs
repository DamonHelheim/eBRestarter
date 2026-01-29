using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelInfocenter : ObservableObject
    {
        #region Fields
        private readonly IDialogService _dialogService;
        private readonly ILocalizationService _localizationService; // <--- NEU: Service Feld

        // Services
        private readonly IHardwareInfoService _hardwareService;
        private readonly IOsEditionService _osEditionService;
        private readonly IWindowsSystemInfoService _systemInfoService;
        #endregion

        #region Observable Properties

        // Initialwerte entfernen wir hier, da wir sie im Konstruktor setzen
        [ObservableProperty] public partial string BrowserText { get; set; }
        [ObservableProperty] public partial string GraphicsText { get; set; }
        [ObservableProperty] public partial string OsBuildText { get; set; }
        [ObservableProperty] public partial string OsEditionText { get; set; }
        [ObservableProperty] public partial string OsVersionText { get; set; }
        [ObservableProperty] public partial string ProcessorText { get; set; }
        [ObservableProperty] public partial string RamText { get; set; }
        #endregion

        #region Properties
        // Titel dynamisch machen (kein ObservableProperty nötig, da er sich nach Start nicht ändert)
        public string Title { get; private set; }
        #endregion

        #region Constructors
        public ViewModelInfocenter(
            IHardwareInfoService hardwareService,
            IOsEditionService osEditionService,
            IWindowsSystemInfoService systemInfoService,
            IDialogService dialogService,
            ILocalizationService localizationService) // <--- Injizieren
        {
            _hardwareService = hardwareService;
            _osEditionService = osEditionService;
            _systemInfoService = systemInfoService;
            _dialogService = dialogService;
            _localizationService = localizationService; // <--- Zuweisen

            // Lokalisierte Standardwerte setzen ("Lade..." / "Loading...")
            string loading = _localizationService.GetString("Infocenter_Loading");

            BrowserText = loading;
            GraphicsText = loading;
            OsBuildText = loading;
            OsEditionText = loading;
            OsVersionText = loading;
            ProcessorText = loading;
            RamText = loading;

            Title = _localizationService.GetString("Infocenter_Title"); // "Infocenter"

            // Startet das Laden asynchron
            _ = LoadDataAsync();
        }
        #endregion

        #region Commands
        [RelayCommand]
        public void OpenSupportWebsite(string? url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch {/* Logging könnte hier hin */}
            }
        }

        [RelayCommand]
        private async Task ShowAboutInfo()
        {
            await _dialogService.ShowAboutDialogAsync();
        }
        #endregion

        #region Methods
        private async Task LoadDataAsync()
        {
            // 1. Hardware Laden
            var hardware = await _hardwareService.GetHardwareInfoAsync();

            ProcessorText = $"{_localizationService.GetString("Infocenter_ProcessorPrefix")} {hardware.ProcessorName}";
            GraphicsText = $"{_localizationService.GetString("Infocenter_GraphicsPrefix")} {hardware.GraphicsCardName}";
            RamText = $"{_localizationService.GetString("Infocenter_RamPrefix")} {hardware.InstalledRam}";

            // 2. OS Edition Laden
            var edition = await _osEditionService.GetOsEditionAsync();
            OsEditionText = $"{_localizationService.GetString("Infocenter_EditionPrefix")} {edition}";

            // 3. System Details Laden
            OsVersionText = $"{_localizationService.GetString("Infocenter_VersionPrefix")} {_systemInfoService.GetCurrentOsDisplayVersion()}";
            OsBuildText = $"{_localizationService.GetString("Infocenter_BuildPrefix")} {_systemInfoService.GetCurrentOsBuildVersion()}";
            BrowserText = $"{_localizationService.GetString("Infocenter_BrowserPrefix")} {_systemInfoService.GetCurrentStandardBrowserName()}";
        }
        #endregion
    }
}