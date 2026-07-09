using eBRestarter.Core.Application.Providers;
using eBRestarter.Core.Domain.Handlers;
using eBRestarter.Core.Application.Ports.Inbound.Validators;
using eBRestarter.Infrastructure.Adapters.Validators;
using eBRestarter.Infrastructure.Api;
using eBRestarter.Infrastructure.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Application;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Inbound.Services;
using eBRestarter.Core.Application.Services;
using eBRestarter.Core.Application.Handlers;
// Removed Strategies namespace
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Domain.Entities;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Common.Statics;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

namespace eBRestarter.XUnit.Test.Core.Application.Services
{
    public class RestarterCycleServiceTests
    {
        private readonly Mock<IOutboundPortBrowserFactory> _mockBrowserFactory;
        private readonly Mock<IInboundPortLocalizationProvider> _mockLocalizationService;
        private readonly Mock<IOutboundPortEVisitorConfigRepository> _mockConfigService;
        private readonly Mock<IBrowserCleanupScheduleHandler> _mockCleanupScheduleHandler;
        private readonly Mock<IOutboundPortOsProcessControl> _mockProcessService;

        private readonly Mock<IOutboundPortBrowser> _mockBrowser;
        private readonly FakeTimeProvider _fakeTimeProvider;
        private readonly RestarterCycleService _sut;

        public RestarterCycleServiceTests()
        {
            _mockBrowserFactory = new Mock<IOutboundPortBrowserFactory>();
            _mockLocalizationService = new Mock<IInboundPortLocalizationProvider>();
            _mockConfigService = new Mock<IOutboundPortEVisitorConfigRepository>();
            _mockCleanupScheduleHandler = new Mock<IBrowserCleanupScheduleHandler>();
            _mockProcessService = new Mock<IOutboundPortOsProcessControl>();
            _mockBrowser = new Mock<IOutboundPortBrowser>();

            _fakeTimeProvider = new FakeTimeProvider();

            _mockLocalizationService.Setup(l => l.RetrieveString(It.IsAny<string>())).Returns((string key) => key);
            _mockBrowserFactory.Setup(f => f.Create(It.IsAny<BrowserType>())).Returns(_mockBrowser.Object);
            var dummyConfig = new AppConfig { Browser = null, Username = "TestUser" };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(dummyConfig);

            var delayStrategy = new DelayPhaseHandler(_fakeTimeProvider);
            var runStrategy = new RunBrowserPhaseHandler(_fakeTimeProvider, _mockProcessService.Object, _mockConfigService.Object);

            _sut = new RestarterCycleService(
                _mockBrowserFactory.Object,
                _mockLocalizationService.Object,
                _mockConfigService.Object,
                _mockCleanupScheduleHandler.Object,
                _fakeTimeProvider,
                new AdapterFluentValidation<ManageRestarterCycleRequest>(new ManageRestarterCycleValidator()),
                delayStrategy,
                runStrategy);
        }

        /// <summary>
        /// Spult die Zeit Schritt fÃ¼r Schritt vor und erlaubt der async/await StateMachine
        /// des Services, die nÃ¤chsten Task.Delays korrekt zu planen.
        /// </summary>
        private async Task AdvanceTimeAsync(int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                _fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
                await Task.Delay(10);
            }
        }

        /// <summary>
        /// Stellt sicher, dass die Stop() Methode einen laufenden Zyklus sauber beendet,
        /// ohne dass die Anwendung durch eine unhandled TaskCanceledException abstÃ¼rzt.
        /// </summary>
        [Fact]
        public async Task StartAsync_ShouldStopCleanly_WhenStopIsCalled()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest(eBRestarter.Core.Application.Enums.BrowserType.Firefox, "User", 10, 5, false);
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
        /// Dies ist der "Happy Path" Test. Er prÃ¼ft, ob die gesamte Logik der zeitlichen
        /// Phasen (VerzÃ¶gerung -> Starten -> Warten -> SchlieÃŸen) in der korrekten Reihenfolge ablÃ¤uft.
        /// </summary>
        [Fact]
        public async Task StartAsync_ShouldExecuteFullCycleAndLaunchBrowser()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest(eBRestarter.Core.Application.Enums.BrowserType.Firefox, "TestUser", 10, 5, false);


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

        /// <summary>
        /// Wenn der User die "CheckBrowserAliveRoutine" aktiviert hat, muss das Programm merken,
        /// wenn der Browser abgestÃ¼rzt ist oder manuell geschlossen wurde.
        /// </summary>
        [Fact]
        public async Task RunBrowserPhase_ShouldDetectCrash_AndTriggerCrashCooldown()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest(eBRestarter.Core.Application.Enums.BrowserType.Firefox, "TestUser", 60, 10, CheckBrowserAliveRoutine: true);
            var emittedEvents = new List<RestarterCycleProgress>();
            _sut.ProgressChanged += (s, e) => emittedEvents.Add(e);

            _mockProcessService.Setup(p => p.IsProcessAlive("firefox")).Returns(false);

            var cycleTask = _sut.StartAsync(request, () => Task.CompletedTask);

            // ACT
            await AdvanceTimeAsync(6); // Ãœber den InitialDelay drÃ¼ber
            await AdvanceTimeAsync(4); // Innerhalb der Running Phase greift der Alive-Check ab

            // ASSERT
            emittedEvents.ShouldContain(e => e.State == RestartTaskState.Cooldown && e.StatusMessage.Contains("wurde geschlossen"));

            _sut.Stop();
            await cycleTask;
        }

        /// <summary>
        /// PrÃ¼ft die automatisierte Browser-Bereinigung. Wenn der berechnete Tag erreicht ist,
        /// muss der Browser geschlossen und die Callback-Methode ausgefÃ¼hrt werden.
        /// </summary>
        [Fact]
        public async Task Cycle_ShouldTriggerCleanupCallback_AndSaveNewDate()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest(eBRestarter.Core.Application.Enums.BrowserType.Firefox, "TestUser", 60, 10, false);
            bool callbackExecuted = false;

            // Hier geben wir gezielt ein Browser-Objekt mit, da wir explizit das Cleanup triggern wollen!
            var dummyConfig = new AppConfig { Browser = new BrowserConfig() };
            dummyConfig.Browser.UpdateCleanupSettings(7, _fakeTimeProvider);
            dummyConfig.Browser.SetNextCleanupDate(DateTime.MinValue);
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(dummyConfig);
            _mockCleanupScheduleHandler.Setup(c => c.ShouldRunCleanupNow(7, DateTime.MinValue)).Returns(true);
            _mockCleanupScheduleHandler.Setup(c => c.CalculateNextCleanupDateAfterRun(It.IsAny<DateTime>(), 7)).Returns(DateTime.UtcNow.AddDays(1));

            var cycleTask = _sut.StartAsync(request, () =>
            {
                callbackExecuted = true;
                return Task.CompletedTask;
            });

            // ACT
            await AdvanceTimeAsync(6);  // Initial Delay Ã¼berstehen
            await AdvanceTimeAsync(11); // Runtime Ã¼berstehen
            await AdvanceTimeAsync(7);  // Cooldown Ã¼berstanden -> ZWEITER ZYKLUS STARTET HIER

            // ASSERT
            callbackExecuted.ShouldBeTrue();
            _mockBrowser.Verify(b => b.Start($"{WebLinks.EVisitorSurflink}TestUser", It.IsAny<string>()), Times.Once);

            _sut.Stop();
            await cycleTask;
        }

        /// <summary>
        /// Der Service lÃ¤uft in einer Endlosschleife. Wenn der Benutzer in der UI Einstellungen
        /// Ã¤ndert, sollen diese im *nÃ¤chsten* Zyklus automatisch Ã¼bernommen werden.
        /// </summary>
        [Fact]
        public async Task RunCycle_ShouldReloadConfig_BeforeEveryNewIteration()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest(eBRestarter.Core.Application.Enums.BrowserType.Chrome, "OldUser", 10, 5, false);

            var config1 = new AppConfig { Username = "OldUser", Browser = null };
            var config2 = new AppConfig { Username = "NewUser", Browser = null };

            int loadConfigCallCount = 0;
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(() =>
            {
                loadConfigCallCount++;
                return loadConfigCallCount <= 3 ? config1 : config2;
            });

            var cycleTask = _sut.StartAsync(request, () => Task.CompletedTask);

            // ACT
            await AdvanceTimeAsync(6);  // Initial Delay Ã¼berstehen
            await AdvanceTimeAsync(11); // Runtime Ã¼berstehen
            await AdvanceTimeAsync(7);  // Cooldown Ã¼berstanden -> ZWEITER ZYKLUS STARTET HIER

            // ASSERT
            // Beim Start des zweiten Zyklus muss die neue URL (mit NewUser) aufgerufen werden!
            _mockBrowser.Verify(b => b.Start($"{WebLinks.EVisitorSurflink}NewUser", It.IsAny<string>()), Times.Once);

            _sut.Stop();
            await cycleTask;
        }
    }
}



















