using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Interfaces.Browser; // Für IBrowserFactory & IBrowser
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Domain.Enums; // Für BrowserType Enum
using eBRestarter.Core.Domain.Models.Records;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml; // Für DispatcherTimer
using System;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelRestartTask : ObservableObject, IRecipient<UsernameChangedMessage>, IRecipient<BrowserChangedMessage>
    {
        // --- SERVICES ---
        private readonly IEVisitorConfigService _configService;
        private readonly IBrowserFactory _browserFactory; // NEU: Factory nutzen
        private readonly DispatcherQueue _dispatcherQueue;

        // --- INTERNE LOGIK ---
        private DispatcherTimer? _uiTimer;
        private RestartTaskState _currentState = RestartTaskState.Idle; // Zustände für unsere State-Machine
        private IBrowser? _currentBrowser; // Das aktuelle Browser-Objekt (Chrome, Firefox, etc.)

        // --- CONFIG CACHE ---
        private int _runtimeSeconds = 3600;
        private int _pauseSeconds = 20;
        private const int _initialDelaySeconds = 5;
        // Platzhalter URL - Hier später deine echte URL-Logik oder IWebLinks nutzen
        private const string _baseUrl = "https://www.ebesucher.com/surfbar/";

        // --- OBSERVABLE PROPERTIES ---
        [ObservableProperty] public partial int SecondsRemaining { get; set; }
        [ObservableProperty] public partial string Username { get; set; } = "-";
        [ObservableProperty] public partial string ChoosenBrowser { get; set; } = "Nicht gewählt";
        [ObservableProperty] public partial bool IsActive { get; set; } = false;
        [ObservableProperty] public partial string StatusInfoText { get; set; } = "Bereit.";

        public ViewModelRestartTask(IEVisitorConfigService configService, IBrowserFactory browserFactory)
        {
            _configService = configService;
            _browserFactory = browserFactory; // Factory wird injected
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            LoadInitialData();
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private void LoadInitialData()
        {
            var config = _configService.LoadConfig();
            Username = config.Username ?? "-";
            ChoosenBrowser = config.Browser?.Selected ?? "Nicht gewählt";

            // Zeiten aus Config laden
            _runtimeSeconds = config.Browser.RuntimeHours * 3600;
            _pauseSeconds = config.Browser.RuntimePauseSeconds;
        }

        // --- MESSENGER ---
        public void Receive(UsernameChangedMessage message) =>
            _dispatcherQueue.TryEnqueue(() => Username = message.NewUsername ?? "-");

        public void Receive(BrowserChangedMessage message) =>
            _dispatcherQueue.TryEnqueue(() => ChoosenBrowser = message.BrowserName ?? "-");

        // --- START / STOP COMMAND ---
        [RelayCommand]
        private void ExecuteStartTimerScheduler(bool? isChecked)
        {
            IsActive = isChecked ?? false;

            if (IsActive)
            {
                // Konfiguration neu laden, bevor es losgeht
                LoadInitialData();
                StartLoop();
            }
            else
            {
                StopLoop();
            }
        }

        // --- STATE MACHINE LOGIK ---

        private void StartLoop()
        {
            // Timer initialisieren
            if (_uiTimer == null)
            {
                _uiTimer = new DispatcherTimer();
                _uiTimer.Interval = TimeSpan.FromSeconds(1);
                _uiTimer.Tick += OnTimerTick;
            }

            // Start mit Phase 1
            SwitchState(RestartTaskState.InitialDelay);
            _uiTimer.Start();
        }

        private void StopLoop()
        {
            // Timer stoppen
            if (_uiTimer != null)
            {
                _uiTimer.Stop();
                _uiTimer.Tick -= OnTimerTick;
                _uiTimer = null;
            }

            // Browser schließen erzwingen
            CloseCurrentBrowser();

            // Status zurücksetzen
            SwitchState(RestartTaskState.Idle);
        }

        private void OnTimerTick(object? sender, object e)
        {
            if (SecondsRemaining > 0)
            {
                SecondsRemaining--;
                UpdateDynamicStatusText(); // Optional: Text aktualisieren
            }
            else
            {
                // Zeit abgelaufen -> Nächster Schritt
                HandleStateTransition();
            }
        }

        private void HandleStateTransition()
        {
            switch (_currentState)
            {
                case RestartTaskState.InitialDelay:
                    // 5 Sek vorbei -> Start Browser
                    SwitchState(RestartTaskState.Running);
                    break;

                case RestartTaskState.Running:
                    // Laufzeit vorbei -> Browser zu & Pause
                    SwitchState(RestartTaskState.Cooldown);
                    break;

                case RestartTaskState.Cooldown:
                    // Pause vorbei -> Browser wieder auf
                    SwitchState(RestartTaskState.Running);
                    break;
            }
        }

        private void SwitchState(RestartTaskState newState)
        {
            _currentState = newState;

            switch (_currentState)
            {
                case RestartTaskState.Idle:
                    StatusInfoText = "Restarter gestoppt.";
                    SecondsRemaining = 0;
                    break;

                case RestartTaskState.InitialDelay:
                    SecondsRemaining = _initialDelaySeconds;
                    StatusInfoText = $"Start in {SecondsRemaining}s...";
                    break;

                case RestartTaskState.Running:
                    // Browser starten
                    LaunchBrowser();
                    SecondsRemaining = _runtimeSeconds;
                    StatusInfoText = "Browser läuft.";
                    break;

                case RestartTaskState.Cooldown:
                    // Browser beenden
                    CloseCurrentBrowser();

                    // Hier könntest du später die "Cache Löschen" Logik einfügen, 
                    // da du jetzt IBrowser.GetPaths() hast!

                    SecondsRemaining = _pauseSeconds;
                    StatusInfoText = $"Neustart in {SecondsRemaining}s...";
                    break;
            }
        }

        private void UpdateDynamicStatusText()
        {
            // Nur für Initial und Cooldown macht ein Countdown im Text Sinn
            if (_currentState == RestartTaskState.InitialDelay)
                StatusInfoText = $"Start in {SecondsRemaining}s...";
            else if (_currentState == RestartTaskState.Cooldown)
                StatusInfoText = $"Neustart in {SecondsRemaining}s...";
        }

        // --- BROWSER STEUERUNG (BRIDGE ZU DEINEN KLASSEN) ---

        private void LaunchBrowser()
        {
            try
            {
                // 1. String aus Config in Enum wandeln
                var browserType = GetBrowserTypeFromString(ChoosenBrowser);

                // 2. Browser Instanz via Factory holen (DI in Action!)
                _currentBrowser = _browserFactory.Create(browserType);

                // 3. URL zusammenbauen
                string url = $"{_baseUrl}{Username}";

                // 4. Starten (BrowserBase kümmert sich um den Rest)
                _currentBrowser.Start(url);
            }
            catch (Exception ex)
            {
                // Logging wäre hier gut
                StatusInfoText = $"Fehler: {ex.Message}";
                IsActive = false; // Not-Aus
            }
        }

        private void CloseCurrentBrowser()
        {
            // BrowserBase.Close() ruft intern WindowsProcessService.CloseApplication() auf
            _currentBrowser?.Close();
            _currentBrowser = null;
        }

        // Kleiner Helper, um den String aus der Config in das Enum zu wandeln
        private BrowserType GetBrowserTypeFromString(string browserName)
        {
            // Falls der String "Nicht gewählt" oder leer ist, Default nehmen oder Fehler werfen
            if (string.IsNullOrWhiteSpace(browserName) || browserName == "Nicht gewählt")
                return BrowserType.Chrome; // oder Default

            // Case-Insensitive Parse
            if (Enum.TryParse(browserName, true, out BrowserType type))
            {
                return type;
            }

            return BrowserType.Chrome; // Fallback
        }
    }
}