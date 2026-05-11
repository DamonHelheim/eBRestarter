using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using Microsoft.Extensions.Logging;

namespace eBRestarter.Infrastructure.Services.Scheduling;

public partial class ComputerRestartScheduler(
    IEVisitorConfigService configService,
    IWindowsProcessControlService processService,
    IApplicationLifetime applicationLifetime,
    TimeProvider timeProvider,
    ILogger<ComputerRestartScheduler> logger) : IComputerRestartScheduler, IDisposable
{
    // Abhängigkeiten (Dependency Inversion Principle)
    private readonly IEVisitorConfigService _configService = configService;
    private readonly IWindowsProcessControlService _processService = processService;
    private readonly IApplicationLifetime _applicationLifetime = applicationLifetime;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<ComputerRestartScheduler> _logger = logger;

    // Steuerung für den Hintergrund-Task
    private PeriodicTimer? _timer;
    private Task? _backgroundTask;
    private CancellationTokenSource? _cts;

    public event EventHandler<DateTime?>? OnNextRestartDateChanged;

    public void StartScheduler()
    {
        if (_backgroundTask != null) return; // Läuft bereits

        _logger.LogInformation("Computer Restart Scheduler gestartet.");

        _cts = new CancellationTokenSource();

        // Prüfe alle 30 Sekunden (ausreichend genau für Minuten-Checks)
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        // Fire & Forget: Task im Hintergrund starten
        _backgroundTask = Task.Run(async () => await LoopAsync(_cts.Token));
    }

    public async Task StopSchedulerAsync()
    {
        if (_cts != null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
            _cts = null;
        }

        if (_backgroundTask != null)
        {
            try
            {
                // Warten bis der Task sauber beendet ist
                await _backgroundTask;
            }
            catch (OperationCanceledException) { /* Erwartet */ }

            _backgroundTask = null;
        }

        _logger.LogInformation("Computer Restart Scheduler gestoppt.");
    }

    private async Task LoopAsync(CancellationToken token)
    {
        try
        {
            // Wartet asynchron auf den nächsten Tick (blockiert keinen Thread!)
            while (await _timer!.WaitForNextTickAsync(token))
            {
                await CheckAndExecuteRestartAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Scheduler wurde gestoppt
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler im Restart-Scheduler Loop.");
        }
    }

    private async Task CheckAndExecuteRestartAsync()
    {
        // 1. Config laden (immer aktuell)
        var config = _configService.LoadConfig();

        // 2. Validierung: Ist ein Datum gesetzt?
        if (config.Computer.NextRestartDate == null || config.Computer.ComputerRestartIntervalDays <= 0)
        {
            return; // Feature nicht aktiv
        }

        var now = _timeProvider.GetLocalNow();
        var targetDateTime = config.Computer.NextRestartDate.Value.Date.AddHours(config.Computer.RestartClockTime);
        var today = now.Date;

        // Prüfen, ob das geplante Datum und die Uhrzeit in der Vergangenheit liegen
        if (now >= targetDateTime)
        {
            // Wenn die geplante Zeit vorüber ist, ABER es nicht genau JETZT ist (mit einer gewissen Toleranz),
            // dann haben wir den Termin verpasst (z.B. App war zu). In dem Fall nicht neustarten, sondern verschieben.
            // Toleranz: Sagen wir, innerhalb von 5 Minuten nach der Zielzeit darf noch neugestartet werden.
            if (now > targetDateTime.AddMinutes(5))
            {
                _logger.LogWarning("Geplanter Neustart am {Target} wurde verpasst. Berechne neuen Termin...", targetDateTime);

                // Berechne neuen Termin basierend auf Intervall
                DateTime newTargetDate;

                // Wenn wir heute schon NACH der RestartClockTime sind, addiere die Interval-Tage auf heute.
                if (now.Hour >= config.Computer.RestartClockTime)
                {
                    newTargetDate = today.AddDays(config.Computer.ComputerRestartIntervalDays).AddHours(config.Computer.RestartClockTime);
                }
                else
                {
                    // Wenn wir heute noch VOR der Zeit sind, wäre der Restart eigentlich heute (passiert selten in dieser Logik-Branch, aber sicher ist sicher)
                    newTargetDate = today.AddHours(config.Computer.RestartClockTime);
                }

                config.Computer.SetNextRestartDate(newTargetDate);
                _configService.SaveConfig(config);

                // UI informieren, dass sich das Datum verschoben hat
                OnNextRestartDateChanged?.Invoke(this, newTargetDate);
                return;
            }

            // Wir sind genau am oder sehr nah am Ziel-Termin (innerhalb von 5 Minuten)
            // Prüfen wir zur Sicherheit nochmal, ob es auch heute ist
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
        _logger.LogWarning("Automatischer Neustart wird eingeleitet...");

        try
        {
            // 1. Programme schließen (Sanft)
            _logger.LogInformation("Schließe offene Anwendungen...");
            await _processService.CloseAllOpenProgramsAsync(30000); // 30 Sekunden Timeout

            // 2. Shutdown erzwingen
            _logger.LogInformation("Fahre System herunter...");
            _processService.ShutdownComputer(); //

            // 3. Anwendung beenden (damit nicht weiter geloggt/gearbeitet wird bis Windows killt)
            _applicationLifetime.ExitApplication(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Ausführen der Neustart-Sequenz.");
        }
    }

    private bool _disposed;
    public void Dispose()
    {
        // Ruft die eigentliche Aufräum-Methode auf
        Dispose(true);

        // Sagt dem Garbage Collector, dass er den Finalizer nicht mehr aufrufen muss (SonarQube Fix!)
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // Verhindert, dass doppelt abgebaut wird
        if (!_disposed)
        {
            if (disposing)
            {
                // Verwaltete Ressourcen (Managed Objects) freigeben
                _timer?.Dispose();
                _cts?.Dispose();
            }

            _disposed = true;
        }
    }
}
