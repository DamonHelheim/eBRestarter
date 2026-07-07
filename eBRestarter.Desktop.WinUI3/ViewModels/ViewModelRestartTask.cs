using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Extensions;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Inbound.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Services;
using eBRestarter.Core.Application.Ports.Outbound.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Desktop.WinUI3.Messages;
using eBRestarter.Core.Application.Models;
using eBRestarter.Desktop.WinUI3.Models.Enums;
using eBRestarter.Desktop.WinUI3.Services.Interfaces;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using eBRestarter.Desktop.WinUI3.Models;

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
    private readonly IEVisitorConfigRepositoryOutboundPort _configService;

    private readonly IDialogService _dialogService;

    private readonly IBrowserDiscoveryProviderOutboundPort _browserService;
    private readonly eBRestarter.Desktop.WinUI3.Utilities.IBrowserDisplayNameResolverUtility _browserDisplayNameResolver;

    private readonly IInboundPortLocalizationProvider _localizationService;

    private readonly IRestarterCycleService _restarterCycleService;

    private readonly IInboundPortRestartTaskDisplayStateHandler _restartTaskDisplayStateHandler;

    private bool _checkBrowserAliveRoutine;

    private int _pauseSeconds = 20;

    private int _runtimeSeconds = 3600;

    private readonly DispatcherQueue _dispatcherQueue;

    private readonly DispatcherTimer _browserCheckTimer;

    [ObservableProperty]
    public partial bool HasInstalledBrowsers { get; set; } = true;

    [ObservableProperty]
    public partial string ChosenBrowser { get; set; }

    [ObservableProperty]
    public partial bool DeleteBrowserContentIsActive { get; set; }

    [ObservableProperty]
    public partial string DeleteIsActivatedMessage { get; set; } = "Disable";

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial string? NextDeletionProcessDateMessage { get; set; }

    [ObservableProperty]
    public partial string? NextDeletionProcessMessage { get; set; }

    [ObservableProperty]
    public partial int SecondsRemaining { get; set; }

    [ObservableProperty]
    public partial string StatusInfoText { get; set; }

    [ObservableProperty]
    public partial string Username { get; set; } = "-";

    /// <summary>
    /// Initializes the restart task view model with config and services, loads initial display state
    /// from <see cref="IInboundPortRestartTaskDisplayStateHandler"/>, and registers as recipient for app-wide
    /// messages so the UI stays in sync when username, browser, or delete-content settings change.
    /// </summary>
    public ViewModelRestartTask(
        IRestarterCycleService restarterCycleService,
        IEVisitorConfigRepositoryOutboundPort configService,
        IDialogService dialogService,
        IInboundPortLocalizationProvider LocalizationProvider,
        IInboundPortRestartTaskDisplayStateHandler restartTaskDisplayStateHandler,
        IBrowserDiscoveryProviderOutboundPort browserService, eBRestarter.Desktop.WinUI3.Utilities.IBrowserDisplayNameResolverUtility browserDisplayNameResolver)
    {
        ArgumentNullException.ThrowIfNull(restarterCycleService);
        ArgumentNullException.ThrowIfNull(configService);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(LocalizationProvider);
        ArgumentNullException.ThrowIfNull(restartTaskDisplayStateHandler);
        ArgumentNullException.ThrowIfNull(browserService);

        _restarterCycleService = restarterCycleService;
        _configService = configService;
        _dialogService = dialogService;
        _localizationService = LocalizationProvider;
        _restartTaskDisplayStateHandler = restartTaskDisplayStateHandler;
        _browserService = browserService;
        _browserDisplayNameResolver = browserDisplayNameResolver;

        _dispatcherQueue =
            DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException(
                $"{nameof(ViewModelRestartTask)} must be constructed on a thread with a WinUI DispatcherQueue (UI thread).");

        ChosenBrowser = _localizationService.RetrieveString("Task_DefaultBrowser");
        StatusInfoText = _localizationService.RetrieveString("Task_StatusReady");

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
            Interval = TimeSpan.FromSeconds(5)
        };

        _browserCheckTimer.Tick += async (_, _) =>
        {
            try
            {
                await CheckInstalledBrowsersAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        };

        _browserCheckTimer.Start();

        CheckInstalledBrowsersAsync().Forget();
    }

    [RelayCommand]
    private async Task ExecuteStartTimerScheduler(bool? isChecked)
    {
        var appConfig = _configService.LoadConfig();

        if (string.IsNullOrEmpty(appConfig.Username))
        {
            await _dialogService.ShowMessageAsync(
                _localizationService.RetrieveString("Task_Username"),
                _localizationService.RetrieveString("Task_NoUsernameFound"),
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

    public void Receive(BrowserChangedMessage message) => _dispatcherQueue.TryEnqueue(() => ChosenBrowser = message.BrowserName ?? "-");

    public void Receive(DeleteBrowserContentActivateMessage message) => _dispatcherQueue.TryEnqueue(() => DeleteIsActivatedMessage = message.ActivateMessage ?? "-");

    public void Receive(DeleteBrowserContentIsActive message) => _dispatcherQueue.TryEnqueue(() => DeleteBrowserContentIsActive = message.IsActiveOrNot);

    public void Receive(NextDeletionProcessMessage message) => _dispatcherQueue.TryEnqueue(() => NextDeletionProcessMessage = message.Message ?? "-");

    public void Receive(NextDeletionProcessDate message) => _dispatcherQueue.TryEnqueue(() => NextDeletionProcessDateMessage = message.NextDeletionProcessDateMessage ?? "-");

    public void Receive(UsernameChangedMessage message) => _dispatcherQueue.TryEnqueue(() => Username = message.NewUsername ?? "-");

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
            BrowserType: _browserDisplayNameResolver.ResolveBrowserTypeFromDisplayName(ChosenBrowser ?? string.Empty, "Edge"),
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
                    await _dialogService.ShowDeleteBrowserContentDialogAsync(autoStart: true);

                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    tcs.TrySetException(ex);
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
}















