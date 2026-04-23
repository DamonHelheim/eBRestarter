using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.UseCases.ScheduleBrowserCleanup;
using eBRestarter.Core.Domain.Extensions;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Models;
using eBRestarter.Desktop.WinUI3.Models.Constants;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.Messages;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// View model for the "Restarter properties" / task configuration page. Manages runtime hours,
    /// pause seconds, browser-alive check, start-with-program, cache-delete interval, and username.
    /// Persists via <see cref="IEVisitorConfigService"/> and broadcasts changes with
    /// <see cref="WeakReferenceMessenger"/> so the restart task and other pages stay in sync.
    /// </summary>
    public partial class ViewModelRestarterProperties : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IScheduleBrowserCleanupUseCase _scheduleBrowserCleanupUseCase;
        private readonly AppConfig _currentConfig;
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly ILocalizationService _localizationService;
        private readonly IUIOptionsService _uiOptionsService;
        private readonly IOperatingSystemFacade _operatingSystemFacade;

        private readonly IBrowserService _browserService; // <--- NEU
        private readonly DispatcherQueue? _dispatcherQueue;  // <--- NEU
        private readonly DispatcherTimer _browserCheckTimer; // <--- NEU

        #endregion

        // =========================================================
        // 2. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial bool CheckBrowserIsAliveIsOn { get; set; } = false;
        [ObservableProperty] public partial int RuntimeHours { get; set; }
        [ObservableProperty] public partial int RuntimePauseSeconds { get; set; }
        [ObservableProperty] public partial BrowserCacheDeleteOption SelectedDeleteBrowserCacheOption { get; set; }
        [ObservableProperty] public partial bool StartBrowserWithProgrammStartIs { get; set; } = false;
        [ObservableProperty] private partial string StandardBrowser { get; set; } = string.Empty;
        [ObservableProperty] public partial Visibility Tbl_NoBrowserInstalledIsVisible { get; set; } = Visibility.Collapsed;
        [ObservableProperty] public partial bool BtnDeleteBrowserContentIsEnabled { get; set; } = true;
        [ObservableProperty] public partial bool BtnInstalleBesucherAddOnIsEnabled { get; set; } = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddEVisitorUsernameCommand))]
        public partial string Username { get; set; } = string.Empty;

        #endregion

        // =========================================================
        // 3. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        /// <summary>Read-only list of cache-delete interval options (e.g. daily, weekly) from localization.</summary>
        public ReadOnlyCollection<BrowserCacheDeleteOption> BrowserDeleteCacheOptionList { get; }
        /// <summary>Maximum allowed browser runtime in hours.</summary>
        public int BrowserRuntimeHoursMax { get; init; }
        /// <summary>Minimum allowed browser runtime in hours.</summary>
        public int BrowserRuntimeHoursMin { get; init; }
        /// <summary>Maximum allowed pause between runs in seconds.</summary>
        public int RuntimePauseSecondsMax { get; init; }
        /// <summary>Minimum allowed pause between runs in seconds.</summary>
        public int RuntimePauseSecondsMin { get; init; }

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Loads config and populates bounds and options from localization. Binds observable properties
        /// to saved values (runtime, pause, cache interval, start-with-program, browser-alive check).
        /// Does not send messages yet; property change handlers do that when the user edits.
        /// </summary>
        public ViewModelRestarterProperties(
            IScheduleBrowserCleanupUseCase scheduleBrowserCleanupUseCase,
            IOperatingSystemFacade operatingSystemFacade,
            IEVisitorConfigService eVisitorConfigService,
            ILocalizationService localizationService,
            IUIOptionsService uiOptionsService,
            IDialogService dialogService,
            IBrowserService browserService)
        {
            _scheduleBrowserCleanupUseCase = scheduleBrowserCleanupUseCase;
            _operatingSystemFacade = operatingSystemFacade;
            _eVisitorConfigService = eVisitorConfigService;
            _dialogService = dialogService;
            _localizationService = localizationService;
            _uiOptionsService = uiOptionsService;
            _browserService = browserService; // <--- NEU
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread(); // <--- NEU
            _currentConfig = _eVisitorConfigService.LoadConfig();

            RuntimePauseSecondsMin = 20;
            RuntimePauseSecondsMax = 60;
            BrowserRuntimeHoursMin = 1;
            BrowserRuntimeHoursMax = 12;

            RuntimePauseSeconds = _currentConfig.Browser.RuntimePauseSeconds;
            RuntimeHours = _currentConfig.Browser.RuntimeHours;
            StartBrowserWithProgrammStartIs = _currentConfig.Browser.StartBrowserWithProgrammStart;
            CheckBrowserIsAliveIsOn = _currentConfig.Browser.CheckBrowserAliveRoutine;

            BrowserDeleteCacheOptionList = new ReadOnlyCollection<BrowserCacheDeleteOption>([.. _uiOptionsService.GetBrowserCacheOptions()]); //new ReadOnlyCollection<BrowserCacheDeleteOption>(localizationService.GetBrowserCacheOptions().ToList());

            var configDays = _currentConfig.Browser.DeleteBrowserCacheIntervalDays;

            SelectedDeleteBrowserCacheOption = BrowserDeleteCacheOptionList.FirstOrDefault(option => option.Days == configDays) ?? BrowserDeleteCacheOptionList[0];

            // NEU: Timer einrichten, der alle 5 Sekunden im Hintergrund die Browser prüft
            _browserCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _browserCheckTimer.Tick += async (__, _) => await CheckInstalledBrowsersAsync();
            _browserCheckTimer.Start();

            // Den allerersten Check sofort auslösen (damit wir nicht 5 Sekunden auf die initiale UI warten müssen)
            CheckInstalledBrowsersAsync().Forget();
        }

        #endregion

        /// <summary>
        /// Prüft asynchron im Hintergrund, ob mindestens ein unterstützter Browser auf dem System
        /// installiert ist. Aktualisiert anschließend die UI-Bindings über den Dispatcher.
        /// </summary>
        private async Task CheckInstalledBrowsersAsync()
        {
            // WICHTIG: Wir nutzen Task.Run, um die Registry-Prüfung zwingend in einen
            // Hintergrund-Thread auszulagern! Sonst würde die App alle 5 Sekunden ruckeln.
            var browsers = await Task.Run(() => _browserService.GetInstalledBrowsersAsync());

            // Prüft, ob mindestens ein Browser die Eigenschaft "IsInstalled == true" hat
            bool hasInstalledBrowsers = browsers != null && browsers.Any(b => b.IsInstalled);

            // Aktualisiert die UI-Properties zwingend im Main-Thread
            _dispatcherQueue!.TryEnqueue(() =>
            {
                // Nur aktualisieren, wenn sich der Zustand auch wirklich geändert hat
                // (Das verhindert unnötiges Flackern in der UI)
                var newVisibility = hasInstalledBrowsers ? Visibility.Collapsed : Visibility.Visible;
                if (Tbl_NoBrowserInstalledIsVisible != newVisibility)
                {
                    Tbl_NoBrowserInstalledIsVisible = newVisibility;
                    BtnDeleteBrowserContentIsEnabled = hasInstalledBrowsers;
                    BtnInstalleBesucherAddOnIsEnabled = hasInstalledBrowsers;
                }
            });
        }

        // =========================================================
        // 5. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        /// <summary>
        /// Saves the current <see cref="Username"/> to config, sends <see cref="UsernameChangedMessage"/> so
        /// the restart task and other UIs update, then clears the username field for the next entry.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanAddUsername))]
        private void AddEVisitorUsername()
        {
            _currentConfig.Username = Username;

            SaveSettings();

            WeakReferenceMessenger.Default.Send(new UsernameChangedMessage(Username));

            Username = string.Empty;
        }

        /// <summary>Opens the eBesucher registration URL in the default browser.</summary>
        [RelayCommand]
        public void RegisterToEVisitor()
        {
            _operatingSystemFacade.WindowsProcessControlService.OpenUrlInBrowser(WebLinks.RegistrationLink, string.Empty);
        }

        /// <summary>Opens the delete-browser-content dialog in manual mode (no auto-start after completion).</summary>
        [RelayCommand]
        private async Task ShowBrowserDeleteContent()
        {
            await _dialogService.ShowDeleteBrowserContentDialogAsync(autoStart: false);
        }

        /// <summary>Opens the install add-on dialog so the user can install the extension in supported browsers.</summary>
        [RelayCommand]
        private async Task ShowInstallAddOnDialog()
        {
            await _dialogService.ShowInstallAddOnDialogAsync();
        }

        /// <summary>Opens the informational dialog about the add-on (what it does, why it is needed).</summary>
        [RelayCommand]
        private async Task ShowInstallAddOnInfoDialog()
        {
            await _dialogService.ShowInstallAddOnInfoDialogAsync();
        }

        /// <summary>Opens the Edge Startup Boost dialog so the user can disable Startup Boost to reduce background usage.</summary>
        [RelayCommand]
        private async Task OpenStartupBoostDialog()
        {
            await _dialogService.ShowTurnOffEdgeStartupBoostDialogAsync();
        }

        #endregion

        // =========================================================
        // 6. PUBLIC & PROTECTED METHODS (API)
        // =========================================================
        #region PublicAndProtectedMethods

        #endregion

        // =========================================================
        // 7. PROPERTY CHANGE HANDLERS (MVVM Hooks)
        // =========================================================
        #region PropertyChangeHandlers

        partial void OnCheckBrowserIsAliveIsOnChanged(bool value)
        {
            _currentConfig.Browser.CheckBrowserAliveRoutine = value;

            SaveSettings();
        }

        partial void OnRuntimeHoursChanged(int value)
        {
            int clampedValue = Math.Clamp(value, BrowserRuntimeHoursMin, BrowserRuntimeHoursMax);

            if (value != clampedValue)
            {
                RuntimeHours = clampedValue;
                return;
            }

            if (_currentConfig.Browser.RuntimeHours != value)
            {
                _currentConfig.Browser.RuntimeHours = value;
                SaveSettings();
            }

        }

        partial void OnRuntimePauseSecondsChanged(int value)
        {
            int clampedValue = Math.Clamp(value, RuntimePauseSecondsMin, RuntimePauseSecondsMax);

            if (value != clampedValue)
            {
                RuntimePauseSeconds = clampedValue;
                return;
            }

            if (_currentConfig.Browser.RuntimePauseSeconds != value)
            {
                _currentConfig.Browser.RuntimePauseSeconds = value;
                SaveSettings();
            }
        }

        partial void OnSelectedDeleteBrowserCacheOptionChanged(BrowserCacheDeleteOption value)
        {
            var response = _scheduleBrowserCleanupUseCase.UpdateSchedule(new ScheduleBrowserCleanupRequest(value.Days));

            // Keep the local instance of config up-to-date
            _currentConfig.Browser.DeleteBrowserCacheIntervalDays = value.Days;
            _currentConfig.Browser.NextBrowserDeleteCacheDate = response.NextDate ?? DateTime.MinValue;

            if (response.IsActive && response.NextDate.HasValue)
            {
                string formatPattern = _localizationService.GetString("Browser_NextDeleteDate_Format");
                string formattedDateString = string.Format(formatPattern, response.NextDate.Value);

                WeakReferenceMessenger.Default.Send(new NextDeletionProcess(_localizationService.GetString("NextDeletionProcess")));
                WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(formattedDateString));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.GetString("Activate")));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(true));
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new NextDeletionProcess(string.Empty));
                WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(string.Empty));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.GetString("Disabled")));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(false));
            }
        }

        partial void OnStartBrowserWithProgrammStartIsChanged(bool value)
        {
            _currentConfig.Browser.StartBrowserWithProgrammStart = value;
            SaveSettings();
        }

        #endregion

        // =========================================================
        // 8. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        /// <summary>Username can be added only when the field is non-empty.</summary>
        private bool CanAddUsername() => !string.IsNullOrWhiteSpace(Username);

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }

        #endregion
    }
}
