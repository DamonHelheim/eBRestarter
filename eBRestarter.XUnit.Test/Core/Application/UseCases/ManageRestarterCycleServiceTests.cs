using eBRestarter.Core.Application.Contstants;
using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.UseCases.ManageRestarterCycle;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records.Config;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.Tests.Core.Application.UseCases.ManageRestarterCycle
{
    public class ManageRestarterCycleServiceTests
    {
        private readonly Mock<IBrowserFactory> _mockBrowserFactory;
        private readonly Mock<ILocalizationService> _mockLocalizationService;
        private readonly Mock<IBrowserDisplayNameResolver> _mockDisplayNameResolver;
        private readonly Mock<IEVisitorConfigService> _mockConfigService;
        private readonly Mock<IBrowserCleanupScheduleService> _mockCleanupScheduleService;
        private readonly Mock<IWindowsProcessControlService> _mockProcessService;

        private readonly Mock<IBrowser> _mockBrowser;
        private readonly FakeTimeProvider _fakeTimeProvider;
        private readonly ManageRestarterCycleService _sut;

        public ManageRestarterCycleServiceTests()
        {
            _mockBrowserFactory = new Mock<IBrowserFactory>();
            _mockLocalizationService = new Mock<ILocalizationService>();
            _mockDisplayNameResolver = new Mock<IBrowserDisplayNameResolver>();
            _mockConfigService = new Mock<IEVisitorConfigService>();
            _mockCleanupScheduleService = new Mock<IBrowserCleanupScheduleService>();
            _mockProcessService = new Mock<IWindowsProcessControlService>();
            _mockBrowser = new Mock<IBrowser>();

            _fakeTimeProvider = new FakeTimeProvider();

            _mockLocalizationService.Setup(l => l.GetString(It.IsAny<string>())).Returns((string key) => key);
            _mockBrowserFactory.Setup(f => f.Create(It.IsAny<BrowserType>())).Returns(_mockBrowser.Object);

            // FIX 1: Browser = null, damit der Service im Test nicht 3600 Sekunden (1 RuntimeHour) erzwingt,
            // sondern unsere Request-Parameter (10 Sekunden) respektiert!
            var dummyConfig = new AppConfig { Browser = null, Username = "TestUser" };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(dummyConfig);

            _sut = new ManageRestarterCycleService(
                _mockBrowserFactory.Object,
                _mockLocalizationService.Object,
                _mockDisplayNameResolver.Object,
                _mockConfigService.Object,
                _mockCleanupScheduleService.Object,
                _fakeTimeProvider,
                _mockProcessService.Object);
        }

        // =========================================================
        // HILFSMETHODE: ZEIT-KONTROLLE (WICHTIGER FIX)
        // =========================================================

        /// <summary>
        /// Spult die Zeit Schritt für Schritt vor und erlaubt der async/await StateMachine
        /// des Services, die nächsten Task.Delays korrekt zu planen.
        /// </summary>
        private async Task AdvanceTimeAsync(int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                _fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));

                // FIX 2: 10ms reales Warten. Task.Yield() reicht bei TimeProvider oft nicht aus,
                // um dem ThreadPool Zeit zu geben, die fortgesetzten Tasks abzuarbeiten.
                await Task.Delay(10);
            }
        }

        // =========================================================
        // 1. ZYKLUS-STEUERUNG & VERZÖGERUNGEN
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Stellt sicher, dass die Stop() Methode einen laufenden Zyklus sauber beendet,
        /// ohne dass die Anwendung durch eine unhandled TaskCanceledException abstürzt.
        /// </summary>
        [Fact]
        public async Task StartAsync_ShouldStopCleanly_WhenStopIsCalled()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest("Firefox", "User", 10, 5, false);
            var emittedEvents = new List<RestarterCycleProgress>();
            _sut.ProgressChanged += (s, e) => emittedEvents.Add(e);

            var cycleTask = _sut.StartAsync(request, () => Task.CompletedTask);

            // ACT
            await AdvanceTimeAsync(1); // Mitten im Initial Delay abbrechen
            _sut.Stop();
            await cycleTask;

            // ASSERT
            var finalEvent = emittedEvents.Last();
            finalEvent.State.ShouldBe(RestartTaskState.Idle);
            _mockBrowser.Verify(b => b.Close(), Times.Never);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Dies ist der "Happy Path" Test. Er prüft, ob die gesamte Logik der zeitlichen
        /// Phasen (Verzögerung -> Starten -> Warten -> Schließen) in der korrekten Reihenfolge abläuft.
        /// </summary>
        [Fact]
        public async Task StartAsync_ShouldExecuteFullCycleAndLaunchBrowser()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest("Firefox", "TestUser", 10, 5, false);
            _mockDisplayNameResolver.Setup(r => r.GetBrowserTypeFromDisplayName(It.IsAny<string>(), It.IsAny<string>())).Returns(BrowserType.Firefox);

            var cycleTask = _sut.StartAsync(request, () => Task.CompletedTask);

            // ACT & ASSERT

            // 1. Initial Delay Phase (5 Sekunden) + 1 Sekunde Puffer
            await AdvanceTimeAsync(6);

            _mockBrowser.Verify(b => b.Start($"{WebLinks.EVisitorSurflink}TestUser", It.IsAny<string>()), Times.Once);

            // 2. Running Phase (10 Sekunden) + 1 Sekunde Puffer
            await AdvanceTimeAsync(11);

            _mockBrowser.Verify(b => b.Close(), Times.Once);

            _sut.Stop();
            await cycleTask;
        }

        // =========================================================
        // 2. ALIVE CHECK (BROWSER CRASH)
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn der User die "CheckBrowserAliveRoutine" aktiviert hat, muss das Programm merken,
        /// wenn der Browser abgestürzt ist oder manuell geschlossen wurde.
        /// </summary>
        [Fact]
        public async Task RunBrowserPhase_ShouldDetectCrash_AndTriggerCrashCooldown()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest("Firefox", "TestUser", 60, 10, CheckBrowserAliveRoutine: true);
            var emittedEvents = new List<RestarterCycleProgress>();
            _sut.ProgressChanged += (s, e) => emittedEvents.Add(e);

            _mockProcessService.Setup(p => p.IsProcessAlive("firefox")).Returns(false);

            var cycleTask = _sut.StartAsync(request, () => Task.CompletedTask);

            // ACT
            await AdvanceTimeAsync(6); // Über den InitialDelay drüber
            await AdvanceTimeAsync(4); // Innerhalb der Running Phase greift der Alive-Check ab

            // ASSERT
            emittedEvents.ShouldContain(e => e.State == RestartTaskState.Cooldown && e.StatusMessage.Contains("wurde geschlossen"));

            _sut.Stop();
            await cycleTask;
        }

        // =========================================================
        // 3. BROWSER CLEANUP
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Prüft die automatisierte Browser-Bereinigung. Wenn der berechnete Tag erreicht ist,
        /// muss der Browser geschlossen und die Callback-Methode ausgeführt werden.
        /// </summary>
        [Fact]
        public async Task Cycle_ShouldTriggerCleanupCallback_AndSaveNewDate()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest("Firefox", "TestUser", 60, 10, false);
            bool callbackExecuted = false;

            // Hier geben wir gezielt ein Browser-Objekt mit, da wir explizit das Cleanup triggern wollen!
            var dummyConfig = new AppConfig { Browser = new Browser { DeleteBrowserCacheIntervalDays = 7, NextBrowserDeleteCacheDate = DateTime.MinValue } };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(dummyConfig);
            _mockCleanupScheduleService.Setup(c => c.ShouldRunCleanupNow(dummyConfig)).Returns(true);
            _mockCleanupScheduleService.Setup(c => c.GetNextCleanupDateAfterRun(It.IsAny<DateTime>(), 7)).Returns(new DateTime(2050, 1, 1));

            var cycleTask = _sut.StartAsync(request, () =>
            {
                callbackExecuted = true;
                return Task.CompletedTask;
            });

            // ACT
            await AdvanceTimeAsync(6); // Initial Delay
            await AdvanceTimeAsync(3); // Tick in der Running Phase triggert den Date-Check + Puffer für das interne Delay

            _sut.Stop();
            await cycleTask;

            // ASSERT
            callbackExecuted.ShouldBeTrue("Die Cleanup Callback Action wurde nicht aufgerufen.");
            _mockConfigService.Verify(c => c.SaveConfig(It.Is<AppConfig>(cfg => cfg.Browser.NextBrowserDeleteCacheDate.Year == 2050)), Times.Once);
        }

        // =========================================================
        // 4. LAZY CONFIG RELOAD (MID-CYCLE)
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Der Service läuft in einer Endlosschleife. Wenn der Benutzer in der UI Einstellungen
        /// ändert, sollen diese im *nächsten* Zyklus automatisch übernommen werden.
        /// </summary>
        [Fact]
        public async Task RunCycle_ShouldReloadConfig_BeforeEveryNewIteration()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest("Chrome", "OldUser", 10, 5, false);

            var config1 = new AppConfig { Username = "OldUser", Browser = null };
            var config2 = new AppConfig { Username = "NewUser", Browser = null };

            int loadConfigCallCount = 0;

            // FIX: Da LoadConfig() 3x pro Zyklus aufgerufen wird, müssen wir
            // die Config1 für die ersten 3 Aufrufe zurückgeben.
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(() =>
            {
                loadConfigCallCount++;
                return loadConfigCallCount <= 3 ? config1 : config2;
            });

            var cycleTask = _sut.StartAsync(request, () => Task.CompletedTask);

            // ACT
            await AdvanceTimeAsync(6);  // Initial Delay überstehen
            await AdvanceTimeAsync(11); // Runtime überstehen
            await AdvanceTimeAsync(7);  // Cooldown überstanden -> ZWEITER ZYKLUS STARTET HIER

            // ASSERT
            // Beim Start des zweiten Zyklus muss die neue URL (mit NewUser) aufgerufen werden!
            _mockBrowser.Verify(b => b.Start($"{WebLinks.EVisitorSurflink}NewUser", It.IsAny<string>()), Times.Once);

            _sut.Stop();
            await cycleTask;
        }
    }
}