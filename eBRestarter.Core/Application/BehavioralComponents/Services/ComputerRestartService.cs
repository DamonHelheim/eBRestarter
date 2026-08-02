using System;
using System.Threading;
using System.Threading.Tasks;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Core.Application.BehavioralComponents.Services;

/// <summary>
/// Service responsible for managing automated computer system restarts according to configuration rules.
/// </summary>
public sealed class ComputerRestartService(
    IOutboundPortApplicationLifetime applicationLifetime,
    IOutboundPortEVisitorConfigRepository configService,
    IOutboundPortApplicationLogger<ComputerRestartService> logger,
    IOutboundPortOsProcessControl processService,
    TimeProvider timeProvider) : IInboundPortComputerRestartService, IDisposable
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════
    private static readonly TimeSpan SchedulerTimerInterval = TimeSpan.FromSeconds(30);

    private const int ApplicationExitSuccessCode = 0;
    private const int MissedSlotToleranceMinutes = 5;
    private const int ProcessCloseTimeoutMilliseconds = 30000;

    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    // ── Block 1: Injizierte Abhängigkeiten (alphabetisch A–Z) ──
    private readonly IOutboundPortApplicationLifetime _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
    private readonly IOutboundPortEVisitorConfigRepository _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    private readonly IOutboundPortApplicationLogger<ComputerRestartService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOutboundPortOsProcessControl _processService = processService ?? throw new ArgumentNullException(nameof(processService));
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

    // ── Block 2: Primitive Typen & Strings ──
    private bool _disposed;

    // ── Block 4: Komplexe Typen & Sync-Elemente (alphabetisch A–Z) ──
    private Task? _backgroundTask;
    private CancellationTokenSource? _cts;
    private readonly System.Threading.Lock _syncLock = new();
    private PeriodicTimer? _timer;


    // ═══════════════════════════════════════════════════════
    //  5. Events & Delegates
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Occurs when the scheduled next restart date is updated or recalculated.
    /// </summary>
    public event EventHandler<DateTime?>? NextRestartDateChanged;


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Starts the background restart scheduler timer loop.
    /// </summary>
    public void StartScheduler()
    {
        lock (_syncLock)
        {
            if (_backgroundTask is not null) return;

            _logger.LogInformation("Computer Restart Scheduler started.");

            _cts = new CancellationTokenSource();
            _timer = new PeriodicTimer(SchedulerTimerInterval, _timeProvider);

            PeriodicTimer timer = _timer;
            CancellationToken token = _cts.Token;

            _backgroundTask = Task.Run(() => LoopAsync(timer, token));
        }
    }

    /// <summary>
    /// Stops the background restart scheduler gracefully.
    /// </summary>
    public async Task StopSchedulerAsync()
    {
        CancellationTokenSource? cts;
        Task? backgroundTask;
        PeriodicTimer? timer;

        lock (_syncLock)
        {
            if (_backgroundTask is null) return;

            cts = _cts;
            backgroundTask = _backgroundTask;
            timer = _timer;

            _cts = null;
            _backgroundTask = null;
            _timer = null;
        }

        if (cts is not null)
        {
            await cts.CancelAsync();
            cts.Dispose();
        }

        timer?.Dispose();

        if (backgroundTask is not null)
        {
            try
            {
                await backgroundTask;
            }
            catch (OperationCanceledException)
            {
                // Expected behavior during shutdown
            }
        }

        _logger.LogInformation("Computer Restart Scheduler stopped.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_syncLock)
        {
            if (_disposed) return;
            _disposed = true;

            _cts?.Cancel();
            _cts?.Dispose();
            _timer?.Dispose();
        }
    }

    private void OnNextRestartDateChanged(DateTime? nextRestartDate)
    {
        NextRestartDateChanged?.Invoke(this, nextRestartDate);
    }

    private async Task LoopAsync(PeriodicTimer timer, CancellationToken cancellationToken)
    {
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await CheckAndExecuteRestartAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Scheduler context loop has been requested to terminate
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled fault occurred within the restart scheduler loop runtime.");
        }
    }

    private async Task CheckAndExecuteRestartAsync()
    {
        var appConfig = _configService.LoadConfig();

        if (appConfig.Computer.NextRestartDate is null || appConfig.Computer.ComputerRestartIntervalDays <= 0)
        {
            return;
        }

        var now = _timeProvider.GetLocalNow();
        var targetDateTime = appConfig.Computer.NextRestartDate.Value.Date.AddHours(appConfig.Computer.RestartClockTime);

        if (now < targetDateTime)
        {
            return;
        }

        var today = now.Date;

        if (now > targetDateTime.AddMinutes(MissedSlotToleranceMinutes))
        {
            _logger.LogWarning("Missed the targeted automated computer restart slot scheduled at {Target}. Recalculating new target execution window...", targetDateTime);

            DateTime newTargetDate = now.Hour >= appConfig.Computer.RestartClockTime
                ? today.AddDays(appConfig.Computer.ComputerRestartIntervalDays).AddHours(appConfig.Computer.RestartClockTime)
                : today.AddHours(appConfig.Computer.RestartClockTime);

            appConfig.Computer.SetNextRestartDate(newTargetDate);
            _configService.SaveConfig(appConfig);

            OnNextRestartDateChanged(newTargetDate);
            return;
        }

        var restartDate = appConfig.Computer.NextRestartDate.Value.Date;
        bool isCorrectDay = restartDate == today;
        bool isCorrectTime = now.Hour == appConfig.Computer.RestartClockTime;

        if (isCorrectDay && isCorrectTime)
        {
            await ExecuteRestartSequenceAsync();
        }
    }

    private async Task ExecuteRestartSequenceAsync()
    {
        _logger.LogWarning("Initiating automated computer system hardware restart sequence...");

        try
        {
            _logger.LogInformation("Requesting graceful closure across all active desktop applications windows...");
            await _processService.CloseAllOpenProgramsAsync(ProcessCloseTimeoutMilliseconds);

            _logger.LogInformation("Dispatching system level hardware reboot instruction sets...");
            _processService.ShutdownComputer();

            _applicationLifetime.ExitApplication(ApplicationExitSuccessCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A fatal fault aborted the smooth processing execution tracking tasks during the machine reboot phase sequence.");
        }
    }
}
