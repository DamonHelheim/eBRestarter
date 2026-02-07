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
        #region Fields
        private readonly AppConfig _currentConfig;
        private readonly IDialogService _dialogService;
        private readonly IEVisitorConfigService _eVisitorConfigService;
        private readonly ILocalizationService _localizationService;

        // Nur noch EINE Abhängigkeit
        private readonly IOperatingSystemFacade _operatingSystemFacade;
        #endregion

        #region Observable Properties
        [ObservableProperty] public partial bool CheckBrowserIsAliveIsOn { get; set; } = false;
        [ObservableProperty] public partial int RuntimeHours { get; set; }
        [ObservableProperty] public partial int RuntimePauseSeconds { get; set; }

        // Das aktuell ausgewählte Item
        [ObservableProperty] public partial BrowserCacheDeleteOption SelectedDeleteBrowserCacheOption { get; set; }
        [ObservableProperty] public partial bool StartBrowserWithProgrammStartIs { get; set; } = false;
        [ObservableProperty] private partial string StandardBrowser { get; set; } = string.Empty;

        // 1. Die Property an den Command binden
        // Das Attribut sagt: Wenn sich _username ändert, lade den Command neu!
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddeVVisitorUsernameCommand))]
        public partial string Username { get; set; } = string.Empty;
        #endregion

        #region Properties
        // Die Liste für die Combobox (readonly, da sich die Optionen nicht ändern)
        public ReadOnlyCollection<BrowserCacheDeleteOption> BrowserDeleteCacheOptionList { get; }

        public int BrowserRuntimeHoursMax { get; init; }
        public int BrowserRuntimeHoursMin { get; init; }
        public int RuntimePauseSecondsMax { get; init; }
        public int RuntimePauseSecondsMin { get; init; }
        #endregion

        #region Constructors
        // Der Konstruktor ist extrem schlank
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
            BrowserDeleteCacheOptionList = new ReadOnlyCollection<BrowserCacheDeleteOption>(
            localizationService.GetBrowserCacheOptions().ToList()
        );
            //SelectedDeleteBrowserCacheOption = BrowserDeleteCacheOptionList.FirstOrDefault(x => x.Days == configDays) ?? BrowserDeleteCacheOptionList[0];

            SelectedDeleteBrowserCacheOption = BrowserDeleteCacheOptionList.FirstOrDefault(x => x.Days == configDays) ?? BrowserDeleteCacheOptionList[0];
        }
        #endregion

        #region Commands
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

        [RelayCommand]
        public void RegisterToEVisitor()
        {
            _operatingSystemFacade.WindowsProcessControlService.OpenUrlInBrowser(WebLinks.RegistrationLink, string.Empty);
        }

        [RelayCommand]
        private async Task ShowBrowserDeleteContent()
        {
            // Der Dialog öffnet sich, Code wartet hier, bis Dialog geschlossen wird
            await _dialogService.ShowDeleteBrowserContentDialogAsync(autoStart: false);
        }

        [RelayCommand]
        private async Task ShowInstallAddOnDialog()
        {
            // Der Dialog öffnet sich, Code wartet hier, bis Dialog geschlossen wird
            await _dialogService.ShowInstallAddOnDialogAsync();
        }

        [RelayCommand]
        private async Task ShowInstallAddOnInfoDialog()
        {
            // Der Dialog öffnet sich, Code wartet hier, bis Dialog geschlossen wird
            await _dialogService.ShowInstallAddOnInfoDialogAsync();
        }


        #endregion

        #region Methods
        // 3. Die Logik: Wann darf der Button aktiv sein?
        private bool CanAddUsername()
        {
            // Button ist aktiv, wenn der String NICHT leer ist
            return !string.IsNullOrWhiteSpace(Username);
        }

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

        // Wenn sich die Auswahl ändert, kannst du hier reagieren
        partial void OnSelectedDeleteBrowserCacheOptionChanged(BrowserCacheDeleteOption value)
        {
            // 1. Intervall in Config übernehmen
            _currentConfig.Browser.DeleteBrowserCacheIntervalDays = value.Days;

            // 2. Prüfung: Ist das Intervall gültig (1, 3, 7, 14)?
            if (IsIntervalAllowed(value.Days))
            {
                // A) Datum berechnen (Heute + Tage)
                DateTime nextDate = DateTime.Today.AddDays(value.Days);

                // B) Config speichern
                _currentConfig.Browser.NextBrowserDeleteCacheDate = nextDate;

                // --- C) LOKALISIERUNG ---

                // 1. Format-String aus der Ressource holen 
                // (z.B. "am {0:dd.MM.yyyy} um 0 Uhr" ODER "on {0:MM/dd/yyyy} at 12 AM")
                string formatPattern = _localizationService.GetString("Browser_NextDeleteDate_Format");

                // 2. String formatieren
                // Wir übergeben das DateTime Objekt. Die Ressource kümmert sich um das Format (dd.MM vs MM/dd).
                string formattedDateString = string.Format(formatPattern, nextDate);

                // ------------------------

                // D) Nachrichten senden
                WeakReferenceMessenger.Default.Send(new NextDeletionProcess(_localizationService.GetString("NextDeletionProcess")));
                WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(formattedDateString));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.GetString("Activate")));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(true));
            }
            else
            {
                // Fall: Deaktiviert (z.B. bei 0 Tagen)
                // Optional: Datum in Config zurücksetzen (null oder MinValue, je nach deinem Model)
                _currentConfig.Browser.NextBrowserDeleteCacheDate = DateTime.MinValue;

                // Nachrichten senden (Deaktiviert-Status)
                WeakReferenceMessenger.Default.Send(new NextDeletionProcess(string.Empty));
                WeakReferenceMessenger.Default.Send(new NextDeletionProcessDate(string.Empty));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentActivateMessage(_localizationService.GetString("Disabled")));
                WeakReferenceMessenger.Default.Send(new DeleteBrowserContentIsActive(false));
            }

            // 3. Erst jetzt speichern, damit Intervall UND Datum persistiert werden
            SaveSettings();
        }

        // Deine Prüf-Methode (bleibt unverändert, ist gut so)
        public bool IsIntervalAllowed(int days) => days switch
        {
            1 or 3 or 7 or 14 => true,
            _ => false
        };


        partial void OnStartBrowserWithProgrammStartIsChanged(bool value)
        {
            Debug.WriteLine($"OnStartBrowserWithProgrammStartIsChanged: {value}");
            _currentConfig.Browser.StartBrowserWithProgrammStart = value;
            SaveSettings();
        }

        private void SaveSettings()
        {
            _eVisitorConfigService.SaveConfig(_currentConfig);
        }
        #endregion
    }
}
