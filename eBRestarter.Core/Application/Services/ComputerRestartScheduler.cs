using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Services
{
    public class ComputerRestartScheduler : IComputerRestartScheduler, IDisposable
    {
        // Abhängigkeiten (Dependency Inversion Principle)
        private readonly IEVisitorConfigService _configService;
        private readonly IWindowsProcessControlService _processService;
        private readonly ILogger<ComputerRestartScheduler> _logger;

        // Steuerung für den Hintergrund-Task
        private PeriodicTimer? _timer;
        private Task? _backgroundTask;
        private CancellationTokenSource? _cts;

        public ComputerRestartScheduler(
            IEVisitorConfigService configService,
            IWindowsProcessControlService processService,
            ILogger<ComputerRestartScheduler> logger)
        {
            _configService = configService;
            _processService = processService;
            _logger = logger;
        }

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
                _cts.Cancel();
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
                    CheckAndExecuteRestart();
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

        private void CheckAndExecuteRestart()
        {
            // 1. Config laden (immer aktuell)
            var config = _configService.LoadConfig();

            // 2. Validierung: Ist ein Datum gesetzt?
            if (config.Computer.NextRestartDate == null)
            {
                return; // Feature nicht aktiv
            }

            var today = DateTime.Today;
            var now = DateTime.Now;
            var restartDate = config.Computer.NextRestartDate.Value.Date;

            // Deine Logik aus dem alten Code: Prüfen ob heute der Tag ist
            // UND ob die Uhrzeit stimmt.
            // Annahme: RestartClockTime ist eine volle Stunde (int), z.B. 9 für 09:00 Uhr.

            bool isCorrectDay = restartDate == today;

            // Wir prüfen, ob wir in der richtigen Stunde sind und die Minute 0 ist.
            // Der Timer läuft alle 30s, also treffen wir Minute 0 garantiert zweimal.
            // Da der PC herunterfährt, ist das doppelte Treffen egal.
            bool isCorrectTime = now.Hour == config.Computer.RestartClockTime && now.Minute == 0;

            if (isCorrectDay && isCorrectTime)
            {
                //ExecuteRestartSequence();
            }
        }

        private void ExecuteRestartSequence()
        {
            _logger.LogWarning("Automatischer Neustart wird eingeleitet...");

            try
            {
                // 1. Programme schließen (Sanft)
                _logger.LogInformation("Schließe offene Anwendungen...");
                _processService.CloseAllOpenPrograms(); //

                // Kurze Wartezeit (synchron hier okay, da wir im Hintergrund-Task sind)
                Thread.Sleep(3000);

                // 2. Shutdown erzwingen
                _logger.LogInformation("Fahre System herunter...");
                _processService.ShutdownComputer(); //

                // 3. Anwendung beenden (damit nicht weiter geloggt/gearbeitet wird bis Windows killt)
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Ausführen der Neustart-Sequenz.");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _cts?.Dispose();
        }
    }
}
