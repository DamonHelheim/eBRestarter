using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Facade;
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
        private readonly AppConfig _currentConfig;

        // Nur noch EINE Abhängigkeit
        private readonly IOperatingSystemFacade _operatingSystemFacade;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly IDialogService _dialogService;

        // Die Liste für die Combobox (readonly, da sich die Optionen nicht ändern)
        public ReadOnlyCollection<BrowserCacheDeleteOption> BrowserDeleteCacheOptionList => BrowserCacheConstants.Options;

        // 1. Die Property an den Command binden
        // Das Attribut sagt: Wenn sich _username ändert, lade den Command neu!
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddeVVisitorUsernameCommand))]
        public partial string Username { get; set; } = string.Empty;

        [ObservableProperty] private partial string StandardBrowser { get; set; } = string.Empty;
        [ObservableProperty] public partial int RuntimePauseSeconds { get; set; }
        [ObservableProperty] public partial int RuntimeHours { get; set; }

        // Das aktuell ausgewählte Item
        [ObservableProperty] public partial BrowserCacheDeleteOption SelectedDeleteBrowserCacheOption { get; set; }
        [ObservableProperty] public partial bool StartBrowserWithProgrammStartIs { get; set; } = false;
        [ObservableProperty] public partial bool CheckBrowserIsAliveIsOn { get; set; } = false;


        // Min/Max Konstanten (können auch readonly properties sein)
        public int RuntimePauseSecondsMin { get; init; }
        public int RuntimePauseSecondsMax { get; init; }
        public int BrowserRuntimeHoursMin { get; init; }
        public int BrowserRuntimeHoursMax { get; init; }


        // Der Konstruktor ist extrem schlank
        public ViewModelRestarterProperties(
            IOperatingSystemFacade operatingSystemFacade,
            IEVisitorConfigService eVisitorConfigService,
            IDialogService dialogService)
        {
            _operatingSystemFacade = operatingSystemFacade;
            _eVisitorConfigService = eVisitorConfigService;
            _dialogService = dialogService;

            // 1. Config laden
            _currentConfig = _eVisitorConfigService.LoadConfig();

            // 2. Konstanten setzen
            RuntimePauseSecondsMin = 20;
            RuntimePauseSecondsMax = 60;
            BrowserRuntimeHoursMin = 1;
            BrowserRuntimeHoursMax = 12;

            // 3. UI-Properties aus Config befüllen (Mapping)

            // Zahlen
            RuntimePauseSeconds = _currentConfig.Browser.RuntimePauseSeconds;
            RuntimeHours = _currentConfig.Browser.RuntimeHours;

            // Bools (WICHTIG!)
            StartBrowserWithProgrammStartIs = _currentConfig.Browser.StartBrowserWithProgrammStart;
            CheckBrowserIsAliveIsOn = _currentConfig.Browser.CheckBrowserAliveRoutine;

            // ComboBox (WICHTIG!)
            // Wir suchen den Eintrag in der Liste, der den Tagen aus der Config entspricht.
            // Fallback auf Index 0, falls nichts gefunden (z.B. bei neuer Config).
            var configDays = _currentConfig.Browser.DeleteBrowserCacheIntervalDays;

            SelectedDeleteBrowserCacheOption = BrowserDeleteCacheOptionList.FirstOrDefault(x => x.Days == configDays) ?? BrowserDeleteCacheOptionList[0];
        }

        // Wenn sich die Auswahl ändert, kannst du hier reagieren
        partial void OnSelectedDeleteBrowserCacheOptionChanged(BrowserCacheDeleteOption value)
        {
            // Beispiel: Speichern des Integer-Wertes in die Config
            _currentConfig.Browser.DeleteBrowserCacheIntervalDays = value.Days;
            SaveSettings();
        }

        partial void OnStartBrowserWithProgrammStartIsChanged(bool value)
        {
            Debug.WriteLine($"StartBrowserWithProgrammStartIsChanged: {value}");
            _currentConfig.Browser.StartBrowserWithProgrammStart = value;
            SaveSettings();
        }

        partial void OnCheckBrowserIsAliveIsOnChanged(bool value)
        {
            Debug.WriteLine($"CheckBrowserIsAliveIsOnChanged: {value}");
            _currentConfig.Browser.CheckBrowserAliveRoutine = value;
            SaveSettings();
        }

        // Der Generator erstellt diese Methode 'partial' im Hintergrund und ruft sie im Setter auf.
        // Wir implementieren hier den "Rumpf".
        partial void OnRuntimePauseSecondsChanged(int value)
        {
            // 1. Clamping
            int clampedValue = Math.Clamp(value, RuntimePauseSecondsMin, RuntimePauseSecondsMax);

            // 2. Auto-Korrektur in der UI
            if (value != clampedValue)
            {
                // Das setzt die Property neu. 
                // WICHTIG: Da es eine partial Property ist, funktioniert der Setter hier rekursiv sicher.
                RuntimePauseSeconds = clampedValue;
                return;
            }

            // 3. Speichern
            if (_currentConfig.Browser.RuntimePauseSeconds != value)
            {
                _currentConfig.Browser.RuntimePauseSeconds = value;
                SaveSettings();
            }
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

        // 2. Der Command mit CanExecute-Prüfung
        [RelayCommand(CanExecute = nameof(CanAddUsername))]
        private void AddeVVisitorUsername()
        {
            _currentConfig.Username = Username;
            
            SaveSettings();

            // Senden des reinen Records
            WeakReferenceMessenger.Default.Send(new UsernameChangedMessage(Username));

            Username = string.Empty;
        }

        // 3. Die Logik: Wann darf der Button aktiv sein?
        private bool CanAddUsername()
        {
            // Button ist aktiv, wenn der String NICHT leer ist
            return !string.IsNullOrWhiteSpace(Username);
        }

        [RelayCommand]
        public void RegisterToEVisitor()
        {
            _operatingSystemFacade.WindowsProcessControlService.OpenUrlInBrowser(WebLinks.RegistrationLink);
        }

        [RelayCommand]
        private async Task ShowBrowserDeleteContent()
        {
            // Der Dialog öffnet sich, Code wartet hier, bis Dialog geschlossen wird
            await _dialogService.ShowBrowserDeleteContentDialogAsync();
        }

        [RelayCommand]
        private async Task ShowInstallAddOnDialog()
        {
            // Der Dialog öffnet sich, Code wartet hier, bis Dialog geschlossen wird
            await _dialogService.ShowInstallAddOnDialogAsync();
        }

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }
    }
}
