using eBRestarter.Core.Application.Ports.Outbound;
using eBRestarter.Core.Application.Ports.Outbound.Config;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Scheduling;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Core.Application.Handlers;

public partial class ComputerRestartHandler(
    IEVisitorConfigPort configService,
    IOsProcessControlPort processService,
    IApplicationLifetimePort applicationLifetime,
    TimeProvider timeProvider,
    ILogger<ComputerRestartHandler> logger) : IComputerRestartSchedulerPort, IDisposable
{
    private bool _disposed;
    // Dependencies (Dependency Inversion Principle)
    private readonly IEVisitorConfigPort _configService = configService;
    private readonly IOsProcessControlPort _processService = processService;
    private readonly IApplicationLifetimePort _applicationLifetime = applicationLifetime;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<ComputerRestartHandler> _logger = logger;

    // Controls for the background task
    private PeriodicTimer? _timer;
    private Task? _backgroundTask;
    private CancellationTokenSource? _cts;

    public event EventHandler<DateTime?>? OnNextRestartDateChanged;

    public void StartScheduler()
    {
        if (_backgroundTask is not null) return; // Already running

        _logger.LogInformation("Computer Restart Scheduler started.");

        _cts = new CancellationTokenSource();

        // Check every 30 seconds (sufficient precision for minute-based checks)
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        // Fire & Forget: Launch the background processing loop task
        _backgroundTask = Task.Run(async () => await LoopAsync(_cts.Token));
    }

    public async Task StopSchedulerAsync()
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
            _cts = null;
        }

        if (_backgroundTask is not null)
        {
            try
            {
                // Await clean exit completion of the background task loop thread
                await _backgroundTask;
            }
            catch (OperationCanceledException) { /* Expected behavior */ }

            _backgroundTask = null;
        }

        _logger.LogInformation("Computer Restart Scheduler stopped.");
    }

    public void Dispose()
    {
        // Invoke the core resource cleanup strategy method
        Dispose(true);

        // Instruct the Garbage Collector that finalizer execution tasks can be skipped
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // Prevent duplicate tear-down processing sequences
        if (!_disposed)
        {
            if (disposing)
            {
                // Release managed resource object allocations
                _timer?.Dispose();
                _cts?.Dispose();
            }

            _disposed = true;
        }
    }

    private async Task LoopAsync(CancellationToken token)
    {
        try
        {
            // Await next scheduled window pulse asynchronously without block-freezing processing threads
            while (await _timer!.WaitForNextTickAsync(token))
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
        // 1. Resolve configuration (guarantees evaluating the latest modified properties states)
        var config = _configService.LoadConfig();

        // 2. Validation constraints verification: Verify if valid targeted intervals are defined
        if (config.Computer.NextRestartDate is null || config.Computer.ComputerRestartIntervalDays <= 0)
        {
            return; // Feature tracking configuration node is inactive
        }

        var now = _timeProvider.GetLocalNow();
        var targetDateTime = config.Computer.NextRestartDate.Value.Date.AddHours(config.Computer.RestartClockTime);
        var today = now.Date;

        // Verify if the targeted date timeline structure points to a historical past frame
        if (now >= targetDateTime)
        {
            // If the targeted execution slot window passed, BUT the tracking timestamp does not map precisely
            // right NOW (incorporating threshold margins), then the target schedule slot was missed (e.g. system was off).
            // In this specific edge scenario, skip executing a force reboot cycle and shift the target window out.
            // Tolerance threshold configuration setup: Allow force reboots execution tasks if inside a 5-minute execution delay window.
            if (now > targetDateTime.AddMinutes(5))
            {
                _logger.LogWarning("Missed the targeted automated computer restart slot scheduled at {Target}. Recalculating new target execution window...", targetDateTime);

                // Compute next clean milestone execution slot using the tracking intervals configuration metadata
                DateTime newTargetDate;

                // If the clock hours metric tracking context already passed the defined threshold, add the interval delay bounds out from today.
                if (now.Hour >= config.Computer.RestartClockTime)
                {
                    newTargetDate = today.AddDays(config.Computer.ComputerRestartIntervalDays).AddHours(config.Computer.RestartClockTime);
                }
                else
                {
                    // Safe fall-back logic trace adjustment: If execution falls behind but remains ahead hour-wise, target the remaining day timeline
                    newTargetDate = today.AddHours(config.Computer.RestartClockTime);
                }

                config.Computer.SetNextRestartDate(newTargetDate);
                _configService.SaveConfig(config);

                // Notify UI systems that the scheduled threshold shifted onto a updated target tracking structure
                OnNextRestartDateChanged?.Invoke(this, newTargetDate);
                return;
            }

            // Execution state metrics fit within the correct target threshold bounds (inside the 5-minute maximum runtime window variation deviation check)
            // Perform safe validation tracks confirming targeted processing properties line values align perfectly
            var restartDate = config.Computer.NextRestartDate.Value.Date;
            bool isCorrectDay = restartDate == today;
            bool isCorrectTime = now.Hour == config.Computer.RestartClockTime;

            if (isCorrectDay && isCorrectTime)
            {
                await ExecuteRestartSequenceAsync();
            }
        }
    }

    private async Task ExecuteRestartSequenceAsync()
    {
        _logger.LogWarning("Initiating automated computer system hardware restart sequence...");

        try
        {
            // 1. Request graceful desktop applications termination
            _logger.LogInformation("Requesting graceful closure across all active desktop applications windows...");
            await _processService.CloseAllOpenProgramsAsync(30000); // Established maximum graceful wait threshold window: 30 seconds timeout

            // 2. Dispatch force reboot instructions
            _logger.LogInformation("Dispatching system level hardware reboot instruction sets...");
            _processService.ShutdownComputer();

            // 3. Gracefully kill own execution context boundaries to halt localized logs operations tracing blocks before the kernel handles absolute termination
            _applicationLifetime.ExitApplication(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A fatal fault aborted the smooth processing execution tracking tasks during the machine reboot phase sequence.");
        }
    }
}


