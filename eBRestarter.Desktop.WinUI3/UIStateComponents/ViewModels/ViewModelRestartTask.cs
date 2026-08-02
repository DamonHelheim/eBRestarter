using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using eBRestarter.Core.Application.BehavioralComponents.Extensions;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.ObjectArchetypes.Models;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Services.Interfaces;
using eBRestarter.Desktop.WinUI3.BehavioralComponents.Utilities.Interfaces;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.DTOs.SignalDTO.Messages;
using eBRestarter.Desktop.WinUI3.ObjectArchetypes.Enums;

namespace eBRestarter.Desktop.WinUI3.ViewModels;

/// <summary>
/// View model for the restart task page. Drives the cyclic workflow: initial delay, launch browser
/// with eBesucher surfbar URL, run for configured runtime, cooldown, repeat. Listens to app-wide
/// messages (username, browser, delete-content state) and optionally triggers browser cache cleanup
/// when the schedule demands it.
/// </summary>
public sealed partial class ViewModelRestartTask : ObservableObject,
                                            IRecipient<UsernameChangedMessage>,
                                            IRecipient<BrowserChangedMessage>,
                                            IRecipient<DeleteBrowserContentActivateMessage>,
                                            IRecipient<DeleteBrowserContentIsActive>,
                                            IRecipient<NextDeletionProcessMessage>,
                                            IRecipient<NextDeletionProcessDate>
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    private const int BrowserInstallCheckIntervalSeconds = 5;
    private const string DefaultBrowserNameFallback = "Edge";
    private const string DefaultDisableMessage = "Disable";
    private const string DefaultFallbackPlaceholder = "-";
    private const int DefaultPauseSeconds = 20;
    private const int DefaultRuntimeSeconds = 3600;
    private const string TaskDefaultBrowserResourceKey = "Task_DefaultBrowser";
    private const string TaskNoUsernameFoundResourceKey = "Task_NoUsernameFound";
    private const string TaskStatusReadyResourceKey = "Task_StatusReady";
    private const string TaskUsernameResourceKey = "Task_Username";

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (Dependencies) ──
    private readonly IBrowserDisplayNameResolverUtility _browserDisplayNameResolver;
    private readonly IOutboundPortBrowserDiscoveryProvider _browserService;
    private readonly IOutboundPortEVisitorConfigRepository _configService;
    private readonly IDialogService _dialogService;
    private readonly IInboundPortLocalizationProvider _localizationService;
    private readonly IInboundPortRestarterCycleService _restarterCycleService;
    private readonly IInboundPortRestartTaskDisplayStateHandler _restartTaskDisplayStateHandler;

    // ── Block 2: Primitive Typen & Strings ──
    private bool _checkBrowserAliveRoutine;
    private int _pauseSeconds = DefaultPauseSeconds;
    private int _runtimeSeconds = DefaultRuntimeSeconds;

    // ── Block 4: Komplexe Typen, Collections & UI-Elemente ──
    private readonly DispatcherTimer _browserCheckTimer;
    private readonly DispatcherQueue _dispatcherQueue;

    // ═══════════════════════════════════════════════════════
    //  3. Observable Properties
    // ═══════════════════════════════════════════════════════
    // ── Block 2: Primitive Typen & Strings ──
    [ObservableProperty] public partial string ChosenBrowser { get; set; }
    [ObservableProperty] public partial bool DeleteBrowserContentIsActive { get; set; }
    [ObservableProperty] public partial string DeleteIsActivatedMessage { get; set; } = DefaultDisableMessage;
    [ObservableProperty] public partial bool HasInstalledBrowsers { get; set; } = true;
    [ObservableProperty] public partial bool IsActive { get; set; }
    [ObservableProperty] public partial string? NextDeletionProcessDateMessage { get; set; }
    [ObservableProperty] public partial string? NextDeletionProcessMessage { get; set; }
    [ObservableProperty] public partial int SecondsRemaining { get; set; }
    [ObservableProperty] public partial string StatusInfoText { get; set; }
    [ObservableProperty] public partial string Username { get; set; } = DefaultFallbackPlaceholder;

    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Initializes the restart task view model with config and services, loads initial display state
    /// from <see cref="IInboundPortRestartTaskDisplayStateHandler"/>, and registers as recipient for app-wide
    /// messages so the UI stays in sync when username, browser, or delete-content settings change.
    /// </summary>
    public ViewModelRestartTask(
        IBrowserDisplayNameResolverUtility browserDisplayNameResolver,
        IOutboundPortBrowserDiscoveryProvider browserService,
        IOutboundPortEVisitorConfigRepository configService,
        IDialogService dialogService,
        IInboundPortLocalizationProvider localizationProvider,
        IInboundPortRestarterCycleService restarterCycleService,
        IInboundPortRestartTaskDisplayStateHandler restartTaskDisplayStateHandler)
    {
        ArgumentNullException.ThrowIfNull(browserDisplayNameResolver);
        ArgumentNullException.ThrowIfNull(browserService);
        ArgumentNullException.ThrowIfNull(configService);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(localizationProvider);
        ArgumentNullException.ThrowIfNull(restarterCycleService);
        ArgumentNullException.ThrowIfNull(restartTaskDisplayStateHandler);

        _browserDisplayNameResolver = browserDisplayNameResolver;
        _browserService = browserService;
        _configService = configService;
        _dialogService = dialogService;
        _localizationService = localizationProvider;
        _restarterCycleService = restarterCycleService;
        _restartTaskDisplayStateHandler = restartTaskDisplayStateHandler;

        _dispatcherQueue =
            DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException(
                $"{nameof(ViewModelRestartTask)} must be constructed on a thread with a WinUI DispatcherQueue (UI thread).");

        ChosenBrowser = _localizationService.RetrieveString(TaskDefaultBrowserResourceKey);
        StatusInfoText = _localizationService.RetrieveString(TaskStatusReadyResourceKey);

        _restarterCycleService.ProgressChanged += OnCycleProgressChanged;

        LoadInitialConfigData();

        WeakReferenceMessenger.Default.RegisterAll(this);

        var appConfig = _configService.LoadConfig();
        if (appConfig.Browser?.StartBrowserWithProgrammStart == true)
        {
            IsActive = true;
            StartLoop();
        }

        _browserCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(BrowserInstallCheckIntervalSeconds)
        };

        _browserCheckTimer.Tick += OnBrowserCheckTimerTick;
        _browserCheckTimer.Start();

        CheckInstalledBrowsersAsync().Forget();
    }

    // ═══════════════════════════════════════════════════════
    //  7. Commands
    // ═══════════════════════════════════════════════════════
    [RelayCommand]
    private async Task ExecuteStartTimerSchedulerAsync(bool? isChecked)
    {
        var appConfig = _configService.LoadConfig();

        if (string.IsNullOrEmpty(appConfig.Username))
        {
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString(TaskUsernameResourceKey),
                _localizationService.RetrieveString(TaskNoUsernameFoundResourceKey),
                DialogIcon.Error);

            IsActive = false;

            return;
        }

        IsActive = isChecked ?? false;

        if (IsActive)
        {
            LoadInitialConfigData();

            StartLoop();
        }
        else
        {
            StopLoop();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  8. Methods (public → private)
    // ═══════════════════════════════════════════════════════
    // ── Public Interface Implementations (Receive) ──
    public void Receive(BrowserChangedMessage message) => _dispatcherQueue.TryEnqueue(() => ChosenBrowser = message.BrowserName ?? DefaultFallbackPlaceholder);

    public void Receive(DeleteBrowserContentActivateMessage message) => _dispatcherQueue.TryEnqueue(() => DeleteIsActivatedMessage = message.ActivateMessage ?? DefaultFallbackPlaceholder);

    public void Receive(DeleteBrowserContentIsActive message) => _dispatcherQueue.TryEnqueue(() => DeleteBrowserContentIsActive = message.IsActiveOrNot);

    public void Receive(NextDeletionProcessMessage message) => _dispatcherQueue.TryEnqueue(() => NextDeletionProcessMessage = message.Message ?? DefaultFallbackPlaceholder);

    public void Receive(NextDeletionProcessDate message) => _dispatcherQueue.TryEnqueue(() => NextDeletionProcessDateMessage = message.NextDeletionProcessDateMessage ?? DefaultFallbackPlaceholder);

    public void Receive(UsernameChangedMessage message) => _dispatcherQueue.TryEnqueue(() => Username = message.NewUsername ?? DefaultFallbackPlaceholder);

    // ── Private Helper Methods ──
    private async Task CheckInstalledBrowsersAsync()
    {
        IEnumerable<BrowserInfo>? installedBrowsers = await Task.Run(() => _browserService.FindInstalledBrowsersAsync()).ConfigureAwait(false);

        bool hasInstalledBrowsers = installedBrowsers?.Any(browser => browser.IsInstalled) == true;

        _dispatcherQueue.TryEnqueue(() =>
        {
            if (HasInstalledBrowsers != hasInstalledBrowsers)
            {
                HasInstalledBrowsers = hasInstalledBrowsers;

                if (!hasInstalledBrowsers && IsActive)
                {
                    IsActive = false;

                    StopLoop();
                }
            }
        });
    }

    private void LoadInitialConfigData()
    {
        var appConfig = _configService.LoadConfig();
        var currentConfigState = _restartTaskDisplayStateHandler.RetrieveInitialState(appConfig);

        Username = currentConfigState.Username;
        ChosenBrowser = currentConfigState.ChosenBrowser;
        _checkBrowserAliveRoutine = currentConfigState.CheckBrowserAliveRoutine;
        _pauseSeconds = currentConfigState.PauseSeconds;
        _runtimeSeconds = currentConfigState.RuntimeSeconds;
        DeleteBrowserContentIsActive = currentConfigState.DeleteBrowserContentIsActive;
        DeleteIsActivatedMessage = currentConfigState.DeleteIsActivatedMessage;
        NextDeletionProcessMessage = currentConfigState.NextDeletionProcessMessage;
        NextDeletionProcessDateMessage = currentConfigState.NextDeletionProcessDateMessage;
    }

    private async void OnBrowserCheckTimerTick(object? sender, object eventArgs)
    {
        try
        {
            await CheckInstalledBrowsersAsync();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private void OnCycleProgressChanged(object? sender, RestarterCycleProgress cycleProgress)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            SecondsRemaining = cycleProgress.SecondsRemaining;
            StatusInfoText = cycleProgress.StatusMessage;

            if (cycleProgress.State == RestartTaskState.Idle && IsActive)
            {
                IsActive = false;
            }
        });
    }

    private void StartLoop()
    {
        var manageRestarterCycleRequest = new ManageRestarterCycleRequest(
            BrowserType: _browserDisplayNameResolver.ResolveBrowserTypeFromDisplayName(ChosenBrowser ?? string.Empty, DefaultBrowserNameFallback),
            Username: Username,
            RuntimeSeconds: _runtimeSeconds,
            PauseSeconds: _pauseSeconds,
            CheckBrowserAliveRoutine: _checkBrowserAliveRoutine);

        _restarterCycleService.StartAsync(manageRestarterCycleRequest, async () =>
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            _dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    await _dialogService.ShowDeleteBrowserContentDialogAsync(shouldAutoStart: true);

                    tcs.TrySetResult();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);

                    tcs.TrySetException(exception);
                }
                finally
                {
                    LoadInitialConfigData();
                }
            });

            await tcs.Task.ConfigureAwait(false);

        }).Forget();
    }

    private void StopLoop()
    {
        _restarterCycleService.Stop();
    }
}
