// Removed Strategies namespace
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.BehavioralComponents.Handlers;
using eBRestarter.Core.Application.BehavioralComponents.Services;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Handlers;
using eBRestarter.Core.Domain.ValueObjects;
using eBRestarter.Infrastructure.BehavioralComponents.Validators;
using eBRestarter.Infrastructure.Common.Statics;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Validators;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;

namespace eBRestarter.XUnit.Test.Core.Application.Services
{
    public class RestarterCycleServiceTests
    {
        /// <summary>
        /// Harte Obergrenze pro Test in dieser Klasse.
        /// </summary>
        /// <remarks>
        /// Diese Tests blockierten den gesamten Testlauf unbegrenzt. Ursache war ein
        /// Dispose-Race in <c>RestarterCycleService.Stop()</c>: die CancellationTokenSource
        /// wurde freigegeben, bevor die asynchron laufenden Cancellation-Callbacks gefeuert
        /// hatten, wodurch der wartende Delay nie zurueckkam. Behoben im Produktivcode.
        /// <para>
        /// Das Timeout bleibt als Netz stehen: Die Tests laufen jetzt in unter einer Sekunde
        /// durch, ein erneuter Hänger wäre also ein schneller, sichtbarer Fehlschlag statt eines
        /// blockierten CI-Laufs.
        /// </para>
        /// </remarks>
        private const int TestTimeoutMilliseconds = 15000;

        private readonly IOutboundPortBrowserFactory _mockBrowserFactory;
        private readonly IInboundPortLocalizationProvider _mockLocalizationService;
        private readonly IOutboundPortEVisitorConfigRepository _mockConfigService;
        private readonly IBrowserCleanupScheduleHandler _mockCleanupScheduleHandler;
        private readonly IOutboundPortOsProcessControl _mockProcessService;

        private readonly IOutboundPortBrowser _mockBrowser;
        private readonly FakeTimeProvider _fakeTimeProvider;
        private readonly RestarterCycleService _sut;

        public RestarterCycleServiceTests()
        {
            _mockBrowserFactory = Substitute.For<IOutboundPortBrowserFactory>();
            _mockLocalizationService = Substitute.For<IInboundPortLocalizationProvider>();
            _mockConfigService = Substitute.For<IOutboundPortEVisitorConfigRepository>();
            _mockCleanupScheduleHandler = Substitute.For<IBrowserCleanupScheduleHandler>();
            _mockProcessService = Substitute.For<IOutboundPortOsProcessControl>();
            _mockBrowser = Substitute.For<IOutboundPortBrowser>();

            // Exception-Runde E-1: IOutboundPortBrowser.Start gibt jetzt bool zurueck, damit ein
            // fehlgeschlagener Start beim Aufrufer ankommt statt still geschluckt zu werden.
            // Ein Substitute liefert dafuer standardmaessig false - der Zyklus laeuft dann in
            // jeder Iteration in den Fehlschlag-Zweig statt in die Laufzeitphase. Alle Tests hier
            // setzen einen erfolgreichen Start voraus, also wird er hier explizit konfiguriert.
            _mockBrowser.Start(Arg.Any<string>()).Returns(true);
            _mockBrowser.Start(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            _fakeTimeProvider = new FakeTimeProvider();

            _mockLocalizationService.RetrieveString(Arg.Any<string>()).Returns(ci => ci.Arg<string>());
            _mockBrowserFactory.Create(Arg.Any<BrowserType>()).Returns(_mockBrowser);
            // Browser = null ist hier bewusst gesetzt und nicht etwa nachlaessig: RestarterCycleService
            // uebernimmt bei nicht-null die Laufzeit aus appConfig.Browser.RuntimeHours. Ein blosses
            // "new BrowserConfig()" haette RuntimeHours = 0 und damit RuntimeSeconds = 0 bedeutet - die
            // Zyklen liefen sofort durch und die Tests pruefen nichts mehr. null modelliert ausserdem
            // real, was beim Deserialisieren einer Config mit "Browser": null herauskommt; der
            // Property-Initializer greift dabei nicht. Deshalb null! statt Umbau der Fixture.
            var dummyConfig = new AppConfig { Browser = null!, Username = "TestUser" };
            _mockConfigService.LoadConfig().Returns(dummyConfig);

            var delayStrategy = new DelayPhaseHandler(_fakeTimeProvider);
            // Signatur: (configService, processService, timeProvider).
            var runStrategy = new RunBrowserPhaseHandler(_mockConfigService, _mockProcessService, _fakeTimeProvider);

            // Signatur ist alphabetisch nach Parametername sortiert; zusaetzlich kam in der
            // Logging-Runde ein IOutboundPortApplicationLogger hinzu.
            _sut = new RestarterCycleService(
                _mockCleanupScheduleHandler,
                _mockBrowserFactory,
                _mockConfigService,
                delayStrategy,
                _mockLocalizationService,
                Substitute.For<IOutboundPortApplicationLogger<RestarterCycleService>>(),
                runStrategy,
                _fakeTimeProvider,
                new AdapterFluentValidationWrapper<ManageRestarterCycleRequest>(new ManageRestarterCycleValidator()));
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
        [Fact(Timeout = TestTimeoutMilliseconds)]
        public async Task StartAsync_ShouldStopCleanly_WhenStopIsCalled()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest(BrowserType.Firefox, "User", 10, 5, false);
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
            _mockBrowser.DidNotReceive().Close();
        }

        /// <summary>
        /// Dies ist der "Happy Path" Test. Er prÃ¼ft, ob die gesamte Logik der zeitlichen
        /// Phasen (VerzÃ¶gerung -> Starten -> Warten -> SchlieÃŸen) in der korrekten Reihenfolge ablÃ¤uft.
        /// </summary>
        [Fact(Timeout = TestTimeoutMilliseconds)]
        public async Task StartAsync_ShouldExecuteFullCycleAndLaunchBrowser()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest(BrowserType.Firefox, "TestUser", 10, 5, false);


            var cycleTask = _sut.StartAsync(request, () => Task.CompletedTask);

            // ACT & ASSERT

            // 1. Initial Delay Phase (5 Sekunden) + 1 Sekunde Puffer
            await AdvanceTimeAsync(6);

            _mockBrowser.Received(1).Start($"{WebLinks.EVisitorSurflink}TestUser", Arg.Any<string>());

            // 2. Running Phase (10 Sekunden) + 1 Sekunde Puffer
            await AdvanceTimeAsync(11);

            _mockBrowser.Received(1).Close();

            _sut.Stop();
            await cycleTask;
        }

        /// <summary>
        /// Wenn der User die "CheckBrowserAliveRoutine" aktiviert hat, muss das Programm merken,
        /// wenn der Browser abgestÃ¼rzt ist oder manuell geschlossen wurde.
        /// </summary>
        [Fact(Timeout = TestTimeoutMilliseconds)]
        public async Task RunBrowserPhase_ShouldDetectCrash_AndTriggerCrashCooldown()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest(BrowserType.Firefox, "TestUser", 60, 10, CheckBrowserAliveRoutine: true);
            var emittedEvents = new List<RestarterCycleProgress>();
            _sut.ProgressChanged += (s, e) => emittedEvents.Add(e);

            _mockProcessService.IsProcessAlive("firefox").Returns(false);

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
        [Fact(Timeout = TestTimeoutMilliseconds)]
        public async Task Cycle_ShouldTriggerCleanupCallback_AndSaveNewDate()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest(BrowserType.Firefox, "TestUser", 60, 10, false);
            bool callbackExecuted = false;

            // Hier geben wir gezielt ein Browser-Objekt mit, da wir explizit das Cleanup triggern wollen!
            var dummyConfig = new AppConfig { Browser = new BrowserConfig() };
            dummyConfig.Browser.UpdateCleanupSettings(7, _fakeTimeProvider);
            dummyConfig.Browser.SetNextCleanupDate(DateTime.MinValue);
            _mockConfigService.LoadConfig().Returns(dummyConfig);
            _mockCleanupScheduleHandler.ShouldRunCleanupNow(7, DateTime.MinValue).Returns(true);
            _mockCleanupScheduleHandler.CalculateNextCleanupDateAfterRun(Arg.Any<DateTime>(), 7).Returns(DateTime.UtcNow.AddDays(1));

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
            _mockBrowser.Received(1).Start($"{WebLinks.EVisitorSurflink}TestUser", Arg.Any<string>());

            _sut.Stop();
            await cycleTask;
        }

        /// <summary>
        /// Der Service lÃ¤uft in einer Endlosschleife. Wenn der Benutzer in der UI Einstellungen
        /// Ã¤ndert, sollen diese im *nÃ¤chsten* Zyklus automatisch Ã¼bernommen werden.
        /// </summary>
        [Fact(Timeout = TestTimeoutMilliseconds)]
        public async Task RunCycle_ShouldReloadConfig_BeforeEveryNewIteration()
        {
            // ARRANGE
            var request = new ManageRestarterCycleRequest(BrowserType.Chrome, "OldUser", 10, 5, false);

            var config1 = new AppConfig { Browser = null!, Username = "OldUser" };
            var config2 = new AppConfig { Browser = null!, Username = "NewUser" };

            int loadConfigCallCount = 0;
            _mockConfigService.LoadConfig().Returns(_ =>
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
            _mockBrowser.Received(1).Start($"{WebLinks.EVisitorSurflink}NewUser", Arg.Any<string>());

            _sut.Stop();
            await cycleTask;
        }
    }
}



















