using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.ViewModels
{
    /// <summary>
    /// View model for the restart task page. Drives the cyclic workflow: initial delay → launch browser
    /// with eBesucher surfbar URL → run for configured runtime → cooldown → repeat. Listens to app-wide
    /// messages (username, browser, delete-content state) and optionally triggers browser cache cleanup
    /// when the schedule demands it.
    /// </summary>
    public partial class ViewModelRestartTask : ObservableObject,
                                                IRecipient<UsernameChangedMessage>,
                                                IRecipient<BrowserChangedMessage>,
                                                IRecipient<DeleteBrowserContentActivateMessage>,
                                                IRecipient<DeleteBrowserContentIsActive>,
                                                IRecipient<NextDeletionProcess>,
                                                IRecipient<NextDeletionProcessDate>
    {
        // =========================================================
        // 1. CONSTANTS & STATICS (Konstanten)
        // =========================================================
        #region ConstantsAndStatics

        private const string _baseUrl = "https://www.ebesucher.com/surfbar/";
        private const int _initialDelaySeconds = 5;

        #endregion

        // =========================================================
        // 2. FIELDS & INJECTED SERVICES (Backing-Felder und DI)
        // =========================================================
        #region FieldsAndInjectedServices

        private readonly IBrowserFactory _browserFactory;
        private readonly IEVisitorConfigService _configService;
        private readonly IDialogService _dialogService;
        private readonly ILocalizationService _localizationService;
        private readonly IRestartTaskDisplayStateService _restartTaskDisplayStateService;
        private readonly IBrowserCleanupScheduleService _browserCleanupScheduleService;
        private readonly IBrowserDisplayNameResolver _browserDisplayNameResolver;
        private readonly DispatcherQueue _dispatcherQueue;

        private IBrowser? _currentBrowser;
        private RestartTaskState _currentState = RestartTaskState.Idle;
        private int _pauseSeconds = 20;
        private int _runtimeSeconds = 3600;
        private DispatcherTimer? _uiTimer;

        private bool _isTestMode = false;
        private int _testRuntimeSeconds = 20;

        #endregion

        // =========================================================
        // 3. OBSERVABLE PROPERTIES (MVVM State)
        // =========================================================
        #region ObservableProperties

        [ObservableProperty] public partial string ChoosenBrowser { get; set; }
        [ObservableProperty] public partial bool IsActive { get; set; } = false;
        [ObservableProperty] public partial bool DeleteBrowserContentIsActive { get; set; } = false;

        [ObservableProperty] public partial string NextDeletionProcessMessage { get; set; }
        [ObservableProperty] public partial string NextDeletionProcessDateMessage { get; set; }

        [ObservableProperty] public partial int SecondsRemaining { get; set; }
        [ObservableProperty] public partial string StatusInfoText { get; set; }
        [ObservableProperty] public partial string Username { get; set; } = "-";
        [ObservableProperty] public partial string DeleteIsActivatedMessage { get; set; } = "Disable";

        #endregion

        // =========================================================
        // 4. CONSTRUCTOR & FINALIZER (Ctor)
        // =========================================================
        #region ConstructorAndFinalizer

        /// <summary>
        /// Initializes the restart task view model with config and services, loads initial display state
        /// from <see cref="IRestartTaskDisplayStateService"/>, and registers as recipient for app-wide
        /// messages so the UI stays in sync when username, browser, or delete-content settings change.
        /// </summary>
        public ViewModelRestartTask(
            IEVisitorConfigService configService,
            IBrowserFactory browserFactory,
            IDialogService dialogService,
            ILocalizationService localizationService,
            IRestartTaskDisplayStateService restartTaskDisplayStateService,
            IBrowserCleanupScheduleService browserCleanupScheduleService,
            IBrowserDisplayNameResolver browserDisplayNameResolver)
        {
            _configService = configService;
            _browserFactory = browserFactory;
            _dialogService = dialogService;
            _localizationService = localizationService;
            _restartTaskDisplayStateService = restartTaskDisplayStateService;
            _browserCleanupScheduleService = browserCleanupScheduleService;
            _browserDisplayNameResolver = browserDisplayNameResolver;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            ChoosenBrowser = _localizationService.GetString("Task_DefaultBrowser");
            StatusInfoText = _localizationService.GetString("Task_StatusReady");

            LoadInitialData();
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        #endregion

        // =========================================================
        // 5. COMMANDS (MVVM Actions)
        // =========================================================
        #region Commands

        /// <summary>
        /// Invoked when the user toggles the task on or off. Starts the delay→run→cooldown loop
        /// when checked; stops the timer and closes the browser when unchecked.
        /// </summary>
        /// <param name="isChecked">True to start the scheduler, false to stop. Null is treated as false.</param>
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

        // =========================================================
        // 6. PUBLIC METHODS (Messenger Receive)
        // =========================================================
        #region PublicAndProtectedMethods

        /// <summary>Updates the chosen browser name on the UI thread when a <see cref="BrowserChangedMessage"/> is received.</summary>
        public void Receive(BrowserChangedMessage message) => _dispatcherQueue.TryEnqueue(() => ChoosenBrowser = message.BrowserName ?? "-");
        /// <summary>Updates the displayed username on the UI thread when a <see cref="UsernameChangedMessage"/> is received.</summary>
        public void Receive(UsernameChangedMessage message) => _dispatcherQueue.TryEnqueue(() => Username = message.NewUsername ?? "-");
        /// <summary>Updates the delete-activation label when a <see cref="DeleteBrowserContentActivateMessage"/> is received.</summary>
        public void Receive(DeleteBrowserContentActivateMessage message) => _dispatcherQueue.TryEnqueue(() => DeleteIsActivatedMessage = message.ActivateMessage ?? "-");
        /// <summary>Updates whether delete-content is active when a <see cref="DeleteBrowserContentIsActive"/> message is received.</summary>
        public void Receive(DeleteBrowserContentIsActive message) => _dispatcherQueue.TryEnqueue(() => DeleteBrowserContentIsActive = message.IsActiveOrNot);
        /// <summary>Updates the next deletion process text when a <see cref="NextDeletionProcess"/> message is received.</summary>
        public void Receive(NextDeletionProcess message) => _dispatcherQueue.TryEnqueue(() => NextDeletionProcessMessage = message.NextDeletionProcessMessage ?? "-");
        /// <summary>Updates the next deletion date text when a <see cref="NextDeletionProcessDate"/> message is received.</summary>
        public void Receive(NextDeletionProcessDate message) => _dispatcherQueue.TryEnqueue(() => NextDeletionProcessDateMessage = message.NextDeletionProcessDateMessage ?? "-");

        #endregion

        // =========================================================
        // 7. PRIVATE HELPER METHODS
        // =========================================================
        #region PrivateHelperMethods

        private void CloseCurrentBrowser()
        {
            _currentBrowser?.Close();
            _currentBrowser = null;
        }

        /// <summary>Maps the display name (e.g. from combo) to <see cref="BrowserType"/> using the resolver; uses default text for "no selection".</summary>
        private BrowserType GetBrowserTypeFromString(string browserName)
        {
            string defaultText = _localizationService.GetString("Task_DefaultBrowser");
            return _browserDisplayNameResolver.GetBrowserTypeFromDisplayName(browserName, defaultText);
        }

        /// <summary>Advances the state machine: InitialDelay→Running, Running→Cooldown (after optional cleanup), Cooldown→Running.</summary>
        private async void HandleStateTransition()
        {
            switch (_currentState)
            {
                case RestartTaskState.InitialDelay:
                    SwitchState(RestartTaskState.Running);
                    break;

                case RestartTaskState.Running:
                    await CheckAndExecuteBrowserCleanup();
                    SwitchState(RestartTaskState.Cooldown);
                    break;

                case RestartTaskState.Cooldown:
                    SwitchState(RestartTaskState.Running);
                    break;
            }
        }

        /// <summary>If the cleanup schedule says it's time, closes the browser, shows the delete-content dialog (auto-start),
        /// then persists the next cleanup date so the UI and schedule stay consistent.</summary>
        private async Task CheckAndExecuteBrowserCleanup()
        {
            var config = _configService.LoadConfig();
            if (!_browserCleanupScheduleService.ShouldRunCleanupNow(config))
                return;

            CloseCurrentBrowser();
            await Task.Delay(1000);

            await _dialogService.ShowDeleteBrowserContentDialogAsync(autoStart: true);

            var newDate = _browserCleanupScheduleService.GetNextCleanupDateAfterRun(DateTime.Today, config.Browser.DeleteBrowserCacheIntervalDays);
            config.Browser.NextBrowserDeleteCacheDate = newDate;
            _configService.SaveConfig(config);

            LoadInitialData();
        }

        /// <summary>Starts the selected browser with the eBesucher surfbar URL for the current username. On failure, sets status text and deactivates the task.</summary>
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

        /// <summary>Loads username, browser, runtime/pause, and delete-content state from config and display-state service so the UI matches saved settings.</summary>
        private void LoadInitialData()
        {
            var config = _configService.LoadConfig();
            var state = _restartTaskDisplayStateService.GetInitialState(config);

            Username = state.Username;
            ChoosenBrowser = state.ChoosenBrowser;

            if (_isTestMode)
            {
                _runtimeSeconds = _testRuntimeSeconds;
                _pauseSeconds = 5;
                StatusInfoText = $"[TEST] Runtime: {_runtimeSeconds}s";
            }
            else
            {
                _runtimeSeconds = state.RuntimeSeconds;
                _pauseSeconds = state.PauseSeconds;
            }

            DeleteBrowserContentIsActive = state.DeleteBrowserContentIsActive;
            DeleteIsActivatedMessage = state.DeleteIsActivatedMessage;
            NextDeletionProcessMessage = state.NextDeletionProcessMessage;
            NextDeletionProcessDateMessage = state.NextDeletionProcessDateMessage;
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

        /// <summary>Creates the 1-second UI timer if needed and starts the state machine from InitialDelay.</summary>
        private void StartLoop()
        {
            if (_uiTimer == null)
            {
                _uiTimer = new DispatcherTimer();
                _uiTimer.Interval = TimeSpan.FromMilliseconds(1000);
                _uiTimer.Tick += OnTimerTick;
            }

            SwitchState(RestartTaskState.InitialDelay);
            _uiTimer.Start();
        }

        /// <summary>Stops the timer, unsubscribes from Tick, disposes the browser, and sets state to Idle.</summary>
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

        /// <summary>Sets current state and updates status text, remaining seconds, and starts/stops browser or cooldown as required.</summary>
        private void SwitchState(RestartTaskState newState)
        {
            _currentState = newState;

            switch (_currentState)
            {
                case RestartTaskState.Idle:
                    StatusInfoText = _localizationService.GetString("Task_StatusStopped");
                    SecondsRemaining = 0;
                    break;

                case RestartTaskState.InitialDelay:
                    SecondsRemaining = _initialDelaySeconds;
                    UpdateDynamicStatusText();
                    break;

                case RestartTaskState.Running:
                    LaunchBrowser();
                    SecondsRemaining = _runtimeSeconds;
                    StatusInfoText = _localizationService.GetString("Task_StatusRunning");
                    break;

                case RestartTaskState.Cooldown:
                    CloseCurrentBrowser();
                    SecondsRemaining = _pauseSeconds;
                    UpdateDynamicStatusText();
                    break;
            }
        }

        /// <summary>Updates status text for InitialDelay or Cooldown using localized "start in" / "restart in" format and current seconds.</summary>
        private void UpdateDynamicStatusText()
        {
            if (_currentState == RestartTaskState.InitialDelay)
            {
                string format = _localizationService.GetString("Task_StatusStartIn");
                StatusInfoText = string.Format(format, SecondsRemaining);
            }
            else if (_currentState == RestartTaskState.Cooldown)
            {
                string format = _localizationService.GetString("Task_StatusRestartIn");
                StatusInfoText = string.Format(format, SecondsRemaining);
            }
        }

        #endregion
    }
}
