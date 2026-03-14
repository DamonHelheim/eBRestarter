using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.UseCases.GetSystemInformation;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// View model for the Infocenter / system info page. Loads hardware and OS information
    /// from <see cref="IHardwareInfoService"/>, <see cref="IOsEditionService"/>, and
    /// <see cref="IWindowsSystemInfoService"/> and exposes them as localized text for display.
    /// Also provides commands to open the support website and the About dialog.
    /// </summary>
    public partial class ViewModelInfocenter : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Fields and DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IGetSystemInformationUseCase _getSystemInformationUseCase;
        private readonly IDialogService _dialogService;
        private readonly ILocalizationService _localizationService;

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial string BrowserText { get; set; }
        [ObservableProperty] public partial string GraphicsText { get; set; }
        [ObservableProperty] public partial string OsBuildText { get; set; }
        [ObservableProperty] public partial string OsEditionText { get; set; }
        [ObservableProperty] public partial string OsVersionText { get; set; }
        [ObservableProperty] public partial string ProcessorText { get; set; }
        [ObservableProperty] public partial string RamText { get; set; }

        #endregion

        // =========================================================
        // 3. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        /// <summary>Localized title for the Infocenter page.</summary>
        public string Title { get; private set; }

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Initializes the VM with hardware, OS, and dialog services; sets all info fields to a
        /// loading placeholder and starts async load so the page shows data as it becomes available.
        /// </summary>
        public ViewModelInfocenter(
            IGetSystemInformationUseCase getSystemInformationUseCase,
            IDialogService dialogService,
            ILocalizationService localizationService)
        {
            _getSystemInformationUseCase = getSystemInformationUseCase;
            _dialogService = dialogService;
            _localizationService = localizationService;

            string loading = _localizationService.GetString("Infocenter_Loading");
            BrowserText = loading;
            GraphicsText = loading;
            OsBuildText = loading;
            OsEditionText = loading;
            OsVersionText = loading;
            ProcessorText = loading;
            RamText = loading;
            //Title = _localizationService.GetString("Infocenter_Title");

            _ = LoadDataAsync();
        }

        #endregion

        // =========================================================
        // 5. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        /// <summary>
        /// Opens the given URL in the default browser via the shell. No-op if url is null or whitespace.
        /// </summary>
        /// <param name="url">Full URL to open (e.g. support or registration). If null or empty, nothing happens.</param>
        [RelayCommand]
        public void OpenSupportWebsite(string? url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch { }
            }
        }

        /// <summary>Opens the About dialog (version and credits) via the dialog service.</summary>
        [RelayCommand]
        private async Task ShowAboutInfo()
        {
            await _dialogService.ShowAboutDialogAsync();
        }

        #endregion

        // =========================================================
        // 6. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        /// <summary>Loads hardware and OS info from services and assigns localized strings to the observable properties.</summary>
        private async Task LoadDataAsync()
        {
            var response = await _getSystemInformationUseCase.ExecuteAsync();

            ProcessorText = $"{_localizationService.GetString("Infocenter_ProcessorPrefix")} {response.ProcessorName}";
            GraphicsText = $"{_localizationService.GetString("Infocenter_GraphicsPrefix")} {response.GraphicsCardName}";
            RamText = $"{_localizationService.GetString("Infocenter_RamPrefix")} {response.InstalledRam}";

            OsEditionText = $"{_localizationService.GetString("Infocenter_EditionPrefix")} {response.OsEdition}";

            OsVersionText = $"{_localizationService.GetString("Infocenter_VersionPrefix")} {response.OsDisplayVersion}";
            OsBuildText = $"{_localizationService.GetString("Infocenter_BuildPrefix")} {response.OsBuildVersion}";
            BrowserText = $"{_localizationService.GetString("Infocenter_BrowserPrefix")} {response.StandardBrowserName}";
        }

        #endregion
    }
}
