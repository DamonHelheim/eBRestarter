using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Facade;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Desktop.WinUI3.Services;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelRestarterProperties : ObservableObject
    {
        // =========================================================
        // 1. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly AppConfig _currentConfig;
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly ILocalizationService _localizationService;
        private readonly IOperatingSystemFacade _operatingSystemFacade;

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
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddeVVisitorUsernameCommand))]
        public partial string Username { get; set; } = string.Empty;

        #endregion

        // =========================================================
        // 3. PUBLIC PROPERTIES (Data & State)
        // =========================================================
        #region PublicProperties

        public ReadOnlyCollection<BrowserCacheDeleteOption> BrowserDeleteCacheOptionList { get; }
        public int BrowserRuntimeHoursMax { get; init; }
        public int BrowserRuntimeHoursMin { get; init; }
        public int RuntimePauseSecondsMax { get; init; }
        public int RuntimePauseSecondsMin { get; init; }

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        public ViewModelRestarterProperties(
            IOperatingSystemFacade operatingSystemFacade,
            IEVisitorConfigService eVisitorConfigService,
            ILocalizationService localizationService,
            IDialogService dialogService)
        {
            _operatingSystemFacade = operatingSystemFacade;
            _eVisitorConfigService = eVisitorConfigService;
            _dialogService = dialogService;
            _localizationService = localizationService;
            _currentConfig = _eVisitorConfigService.LoadConfig();

            RuntimePauseSecondsMin = 20;
            RuntimePauseSecondsMax = 60;
            BrowserRuntimeHoursMin = 1;
            BrowserRuntimeHoursMax = 12;

            RuntimePauseSeconds = _currentConfig.Browser.RuntimePauseSeconds;
            RuntimeHours = _currentConfig.Browser.RuntimeHours;
            StartBrowserWithProgrammStartIs = _currentConfig.Browser.StartBrowserWithProgrammStart;
            CheckBrowserIsAliveIsOn = _currentConfig.Browser.CheckBrowserAliveRoutine;

            BrowserDeleteCacheOptionList = new ReadOnlyCollection<BrowserCacheDeleteOption>(
                localizationService.GetBrowserCacheOptions().ToList());
            var configDays = _currentConfig.Browser.DeleteBrowserCacheIntervalDays;
            SelectedDeleteBrowserCacheOption = BrowserDeleteCacheOptionList.FirstOrDefault(x => x.Days == configDays) ?? BrowserDeleteCacheOptionList[0];
        }

        #endregion

        // =========================================================
        // 5. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        [RelayCommand(CanExecute = nameof(CanAddUsername))]
        private void AddeVVisitorUsername()
        {
            _currentConfig.Username = Username;
            SaveSettings();
            WeakReferenceMessenger.Default.Send(new UsernameChangedMessage(Username));
            Username = string.Empty;
        }

        [RelayCommand]
        public void RegisterToEVisitor()
        {
            _operatingSystemFacade.WindowsProcessControlService.OpenUrlInBrowser(WebLinks.RegistrationLink, string.Empty);
        }

        [RelayCommand]
        private async Task ShowBrowserDeleteContent()
        {
            await _dialogService.ShowDeleteBrowserContentDialogAsync(autoStart: false);
        }

        [RelayCommand]
        private async Task ShowInstallAddOnDialog()
        {
            await _dialogService.ShowInstallAddOnDialogAsync();
        }

        [RelayCommand]
        private async Task ShowInstallAddOnInfoDialog()
        {
            await _dialogService.ShowInstallAddOnInfoDialogAsync();
        }

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

        public bool IsIntervalAllowed(int days) => days switch
        {
            1 or 3 or 7 or 14 => true,
            _ => false
        };

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
            _currentConfig.Browser.DeleteBrowserCacheIntervalDays = value.Days;

            if (IsIntervalAllowed(value.Days))
            {
                DateTime nextDate = DateTime.Today.AddDays(value.Days);
                _currentConfig.Browser.NextBrowserDeleteCacheDate = nextDate;
                string formatPattern = _localizationService.GetString("Browser_NextDeleteDate_Format");
                string formattedDateString = string.Format(formatPattern, nextDate);

                WeakReferenceMessenger.Default.Send(new NextDeletionProcess(_localizationService.GetString("NextDeletionProcess")));
                WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(formattedDateString));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.GetString("Activate")));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(true));
            }
            else
            {
                _currentConfig.Browser.NextBrowserDeleteCacheDate = DateTime.MinValue;
                WeakReferenceMessenger.Default.Send(new NextDeletionProcess(string.Empty));
                WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(string.Empty));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.GetString("Disabled")));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(false));
            }
            SaveSettings();
        }

        partial void OnStartBrowserWithProgrammStartIsChanged(bool value)
        {
            Debug.WriteLine($"OnStartBrowserWithProgrammStartIsChanged: {value}");
            _currentConfig.Browser.StartBrowserWithProgrammStart = value;
            SaveSettings();
        }

        #endregion

        // =========================================================
        // 8. PRIVATE HELPER METHODS (Interne Hilfsmethoden)
        // =========================================================
        #region PrivateHelperMethods

        private bool CanAddUsername() => !string.IsNullOrWhiteSpace(Username);

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }

        #endregion
    }
}
