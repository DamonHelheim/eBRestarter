using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Facade;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Models.Records.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelRestarterProperties : ObservableObject
    {
        // Nur noch EINE Abhängigkeit
        private readonly IOperatingSystemFacade _operatingSystemFacade;
        private readonly IEVisitorConfigService _eVisitorConfigService;

        private readonly AppConfig _currentConfig;

        // Min/Max Konstanten (können auch readonly properties sein)
        public int RuntimePauseSecondsMin { get; init; }
        public int RuntimePauseSecondsMax { get; init; }
        public int BrowserRuntimeHoursMin { get; init; }
        public int BrowserRuntimeHoursMax { get; init; }

        // 1. Die Property an den Command binden
        // Das Attribut sagt: Wenn sich _username ändert, lade den Command neu!
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddeVVisitorUsernameCommand))]
        public partial string Username { get; set; } = string.Empty;

        [ObservableProperty] private partial string StandardBrowser { get; set; } = string.Empty;

        [ObservableProperty] public partial int RuntimePauseSeconds { get; set; }

        [ObservableProperty] public partial int RuntimeHours { get; set; }

        // Der Konstruktor ist extrem schlank
        public ViewModelRestarterProperties(IOperatingSystemFacade operatingSystemFacade, IEVisitorConfigService eVisitorConfigService)
        {
            _operatingSystemFacade = operatingSystemFacade;
            _eVisitorConfigService = eVisitorConfigService;

            _currentConfig = _eVisitorConfigService.LoadConfig();

            RuntimePauseSecondsMin = 20;
            RuntimePauseSecondsMax = 60;
            BrowserRuntimeHoursMin = 1;
            BrowserRuntimeHoursMax = 12;

            RuntimePauseSeconds = _currentConfig.Browser.RuntimePauseSeconds;
            RuntimeHours = _currentConfig.Browser.RuntimeHours;

            //// Zugriff erfolgt nun hierarchisch: _os.SystemInfo...
            //StandardBrowser = _operatingSystemFacade.WindowsSystemInfoService.GetCurrentStandardBrowserName();
        }

        // 2. Der Command mit CanExecute-Prüfung
        [RelayCommand(CanExecute = nameof(CanAddUsername))]
        private void AddeVVisitorUsername()
        {
            _currentConfig.Username = Username;

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

        // Der Generator erstellt diese Methode 'partial' im Hintergrund und ruft sie im Setter auf.
        // Wir implementieren hier den "Rumpf".

        partial void OnRuntimePauseSecondsChanged(int value)
        {
            // 1. Clamping
            int clampedValue = Math.Clamp(value, RuntimePauseSecondsMin, RuntimePauseSecondsMax);

            Debug.WriteLine($"RestMinutes geändert: {value} -> Korrigiert auf: {clampedValue}");

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
            int clampedValue = Math.Clamp(value, BrowserRuntimeHoursMin, BrowserRuntimeHoursMin);

            Debug.WriteLine($"RestMinutes geändert: {value} -> Korrigiert auf: {clampedValue}");

            if (value != clampedValue)
            {
                RuntimeHours = clampedValue;
                return;
            }

            if (_currentConfig.Browser.RuntimeHours != value)
            {
                RuntimeHours = value;
                _currentConfig.Browser.RuntimeHours = value;
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }
    }
}
