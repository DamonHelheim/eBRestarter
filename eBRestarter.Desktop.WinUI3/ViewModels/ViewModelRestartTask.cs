using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    public partial class ViewModelRestartTask : ObservableObject, IRecipient<UsernameChangedMessage>, IRecipient<BrowserChangedMessage>
    {
        #region Constants
        private const string _baseUrl = "https://www.ebesucher.com/surfbar/";
        private const int _initialDelaySeconds = 5;
        #endregion

        #region Fields
        private readonly IBrowserFactory _browserFactory;
        private readonly IEVisitorConfigService _configService;
        private readonly ILocalizationService _localizationService; // <--- NEU
        private readonly DispatcherQueue _dispatcherQueue;

        private IBrowser? _currentBrowser;
        private RestartTaskState _currentState = RestartTaskState.Idle;
        private int _pauseSeconds = 20;
        private int _runtimeSeconds = 3600;
        private DispatcherTimer? _uiTimer;
        #endregion

        #region Observable Properties

        // Initialwerte entfernen wir hier, da wir sie im Konstruktor setzen
        [ObservableProperty] public partial string ChoosenBrowser { get; set; }
        [ObservableProperty] public partial bool IsActive { get; set; } = false;
        [ObservableProperty] public partial int SecondsRemaining { get; set; }
        [ObservableProperty] public partial string StatusInfoText { get; set; }
        [ObservableProperty] public partial string Username { get; set; } = "-";

        #endregion

        #region Constructors
        public ViewModelRestartTask(
            IEVisitorConfigService configService,
            IBrowserFactory browserFactory,
            ILocalizationService localizationService) // <--- Injizieren
        {
            _configService = configService;
            _browserFactory = browserFactory;
            _localizationService = localizationService; // <--- Zuweisen
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Lokalisierte Standardwerte
            ChoosenBrowser = _localizationService.GetString("Task_DefaultBrowser"); // "Nicht gewählt"
            StatusInfoText = _localizationService.GetString("Task_StatusReady");    // "Bereit."

            LoadInitialData();
            WeakReferenceMessenger.Default.RegisterAll(this);
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void ExecuteStartTimerScheduler(bool? isChecked)
        {
            IsActive = isChecked ?? false;

            if (IsActive)
            {
                LoadInitialData();
                StartLoop();
            }
            else
            {
                StopLoop();
            }
        }
        #endregion

        #region Methods
        public void Receive(BrowserChangedMessage message) =>
            _dispatcherQueue.TryEnqueue(() => ChoosenBrowser = message.BrowserName ?? "-");

        public void Receive(UsernameChangedMessage message) =>
            _dispatcherQueue.TryEnqueue(() => Username = message.NewUsername ?? "-");

        private void CloseCurrentBrowser()
        {
            _currentBrowser?.Close();
            _currentBrowser = null;
        }

        private BrowserType GetBrowserTypeFromString(string browserName)
        {
            // Prüfung gegen den lokalisierten String oder leer
            string defaultText = _localizationService.GetString("Task_DefaultBrowser");

            if (string.IsNullOrWhiteSpace(browserName) || browserName == defaultText || browserName == "Nicht gewählt") // Fallback für Legacy
                return BrowserType.Chrome;

            if (Enum.TryParse(browserName, true, out BrowserType type))
            {
                return type;
            }

            return BrowserType.Chrome;
        }

        private void HandleStateTransition()
        {
            switch (_currentState)
            {
                case RestartTaskState.InitialDelay:
                    SwitchState(RestartTaskState.Running);
                    break;

                case RestartTaskState.Running:
                    SwitchState(RestartTaskState.Cooldown);
                    break;

                case RestartTaskState.Cooldown:
                    SwitchState(RestartTaskState.Running);
                    break;
            }
        }

        private void LaunchBrowser()
        {
            try
            {
                var browserType = GetBrowserTypeFromString(ChoosenBrowser);
                _currentBrowser = _browserFactory.Create(browserType);
                string url = $"{_baseUrl}{Username}";
                _currentBrowser.Start(url);
            }
            catch (Exception ex)
            {
                string errorFormat = _localizationService.GetString("General_ErrorPrefix");
                StatusInfoText = string.Format(errorFormat, ex.Message);
                IsActive = false;
            }
        }

        private void LoadInitialData()
        {
            var config = _configService.LoadConfig();
            Username = config.Username ?? "-";

            // Wenn in der Config nichts steht, den lokalisierten "Nicht gewählt" Text nehmen
            ChoosenBrowser = config.Browser?.Selected ?? _localizationService.GetString("Task_DefaultBrowser");

            _runtimeSeconds = config.Browser.RuntimeHours * 3600;
            _pauseSeconds = config.Browser.RuntimePauseSeconds;
        }

        private void OnTimerTick(object? sender, object e)
        {
            if (SecondsRemaining > 0)
            {
                SecondsRemaining--;
                UpdateDynamicStatusText();
            }
            else
            {
                HandleStateTransition();
            }
        }

        private void StartLoop()
        {
            if (_uiTimer == null)
            {
                _uiTimer = new DispatcherTimer();
                _uiTimer.Interval = TimeSpan.FromSeconds(1);
                _uiTimer.Tick += OnTimerTick;
            }

            SwitchState(RestartTaskState.InitialDelay);
            _uiTimer.Start();
        }

        private void StopLoop()
        {
            if (_uiTimer != null)
            {
                _uiTimer.Stop();
                _uiTimer.Tick -= OnTimerTick;
                _uiTimer = null;
            }

            CloseCurrentBrowser();
            SwitchState(RestartTaskState.Idle);
        }

        private void SwitchState(RestartTaskState newState)
        {
            _currentState = newState;

            switch (_currentState)
            {
                case RestartTaskState.Idle:
                    StatusInfoText = _localizationService.GetString("Task_StatusStopped"); // "Restarter gestoppt."
                    SecondsRemaining = 0;
                    break;

                case RestartTaskState.InitialDelay:
                    SecondsRemaining = _initialDelaySeconds;
                    UpdateDynamicStatusText();
                    break;

                case RestartTaskState.Running:
                    LaunchBrowser();
                    SecondsRemaining = _runtimeSeconds;
                    StatusInfoText = _localizationService.GetString("Task_StatusRunning"); // "Browser läuft."
                    break;

                case RestartTaskState.Cooldown:
                    CloseCurrentBrowser();
                    SecondsRemaining = _pauseSeconds;
                    UpdateDynamicStatusText();
                    break;
            }
        }

        private void UpdateDynamicStatusText()
        {
            if (_currentState == RestartTaskState.InitialDelay)
            {
                // Format: "Start in {0}s..."
                string format = _localizationService.GetString("Task_StatusStartIn");
                StatusInfoText = string.Format(format, SecondsRemaining);
            }
            else if (_currentState == RestartTaskState.Cooldown)
            {
                // Format: "Neustart in {0}s..."
                string format = _localizationService.GetString("Task_StatusRestartIn");
                StatusInfoText = string.Format(format, SecondsRemaining);
            }
        }
        #endregion
    }
}