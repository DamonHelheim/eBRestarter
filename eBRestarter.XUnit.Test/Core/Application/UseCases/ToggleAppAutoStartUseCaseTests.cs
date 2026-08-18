using NSubstitute;
using Shouldly;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.UseCases;
using eBRestarter.Core.Domain.ValueObjects;

namespace eBRestarter.Tests.Core.Application.UseCases.ToggleAppAutoStart
{
    /// <summary>
    /// Testet den ToggleAppAutoStartUseCase.
    /// Der Fokus liegt auf der korrekten Synchronisation zwischen der JSON-Config
    /// und den tats�chlichen Windows-Registry-Werten.
    /// </summary>
    public class ToggleAppAutoStartUseCaseTests
    {
        private readonly IOutboundPortAutoStartRepository _mockStartupManager;
        private readonly IOutboundPortEVisitorConfigRepository _mockConfigService;
        private readonly ToggleAppAutoStartUseCase _sut;

        public ToggleAppAutoStartUseCaseTests()
        {
            _mockStartupManager = Substitute.For<IOutboundPortAutoStartRepository>();
            _mockConfigService = Substitute.For<IOutboundPortEVisitorConfigRepository>();

            _sut = // Reihenfolge korrigiert: der Konstruktor nimmt (configService, startupManagerService).
            new ToggleAppAutoStartUseCase(
                _mockConfigService,
                _mockStartupManager);
        }
        // 1. INITIALISIERUNG & SYNCHRONISATION

        /// <summary>
        /// Wenn in der Config "StartWithWindows = true" steht, aber Windows meldet,
        /// dass der Autostart nicht aktiv ist (z.B. weil der User ihn im Task-Manager deaktiviert hat
        /// oder es der erste Start ist), muss die App ihn im Betriebssystem (nach)aktivieren.
        /// </summary>
        [Fact]
        public async Task InitializeAndGetStateAsync_ShouldEnableInOs_WhenConfigIsTrueAndOsIsFalse()
        {
            // ARRANGE
            var config = new AppConfig { Settings = new SettingsConfig { StartWithWindows = true } };
            _mockConfigService.LoadConfig().Returns(config);

            // OS sagt: Autostart ist momentan AUS
            _mockStartupManager.IsAutoStartEnabledAsync().Returns(false);

            // ACT
            var result = await _sut.InitializeAndGetStateAsync();

            // ASSERT
            result.ShouldBeTrue(); // Die Methode muss am Ende 'true' zur�ckgeben

            // Es muss zwingend der Befehl zur Aktivierung an Windows gesendet worden sein
            await _mockStartupManager.Received(1).EnableAutoStartAsync();
        }

        /// <summary>
        /// Wenn Config und OS bereits �bereinstimmen (beide true oder beide false)
        /// oder die Config false ist, soll keine unn�tige Aktion im OS ausgef�hrt werden.
        /// </summary>
        [Theory]
        [InlineData(true, true, true)]    // Config: An,  OS: An  -> Erwartet: An
        [InlineData(false, false, false)] // Config: Aus, OS: Aus -> Erwartet: Aus
        [InlineData(false, true, true)]   // Config: Aus, OS: An  -> Erwartet: An (Dein Code �berschreibt das OS hier nicht!)
        public async Task InitializeAndGetStateAsync_ShouldReturnOsState_WhenNoSyncNeeded(
            bool configState, bool osState, bool expectedResult)
        {
            // ARRANGE
            var config = new AppConfig { Settings = new SettingsConfig { StartWithWindows = configState } };
            _mockConfigService.LoadConfig().Returns(config);
            _mockStartupManager.IsAutoStartEnabledAsync().Returns(osState);

            // ACT
            var result = await _sut.InitializeAndGetStateAsync();

            // ASSERT
            result.ShouldBe(expectedResult);

            // Es darf keine Aktion zum Ver�ndern des OS-Status ausgef�hrt worden sein
            await _mockStartupManager.DidNotReceive().EnableAutoStartAsync();
            await _mockStartupManager.DidNotReceive().DisableAutoStartAsync();
        }
        // 2. TOGGLE-AKTIONEN DURCH DEN BENUTZER

        /// <summary>
        /// Wenn der Benutzer den Autostart in der UI aktiviert, m�ssen zwei Dinge passieren:
        /// 1. Der OS-Service muss aktiviert werden.
        /// 2. Die �nderung muss dauerhaft in der Config gespeichert werden.
        /// </summary>
        [Fact]
        public async Task ToggleAsync_ShouldEnableInOs_AndSaveConfig_WhenTrue()
        {
            // ARRANGE
            var initialConfig = new AppConfig { Settings = new SettingsConfig { StartWithWindows = false } };
            _mockConfigService.LoadConfig().Returns(initialConfig);

            AppConfig? savedConfig = null;
            _mockConfigService.When(s => s.SaveConfig(Arg.Any<AppConfig>()))
                .Do(ci => savedConfig = ci.Arg<AppConfig>());

            // ACT
            await _sut.ToggleAsync(true);

            // ASSERT
            // 1. OS-Befehl gepr�ft
            await _mockStartupManager.Received(1).EnableAutoStartAsync();
            await _mockStartupManager.DidNotReceive().DisableAutoStartAsync();

            // 2. Config-Speicherung gepr�ft
            _mockConfigService.Received(1).SaveConfig(Arg.Any<AppConfig>());
            savedConfig.ShouldNotBeNull();
            savedConfig.Settings.StartWithWindows.ShouldBeTrue();
        }

        /// <summary>
        /// Das genaue Gegenteil zum vorherigen Test: Wenn der Benutzer deaktiviert,
        /// muss das OS den Eintrag l�schen und die Config auf "false" gesetzt werden.
        /// </summary>
        [Fact]
        public async Task ToggleAsync_ShouldDisableInOs_AndSaveConfig_WhenFalse()
        {
            // ARRANGE
            var initialConfig = new AppConfig { Settings = new SettingsConfig { StartWithWindows = true } };
            _mockConfigService.LoadConfig().Returns(initialConfig);

            AppConfig? savedConfig = null;
            _mockConfigService.When(s => s.SaveConfig(Arg.Any<AppConfig>()))
                .Do(ci => savedConfig = ci.Arg<AppConfig>());

            // ACT
            await _sut.ToggleAsync(false);

            // ASSERT
            // 1. OS-Befehl gepr�ft
            await _mockStartupManager.Received(1).DisableAutoStartAsync();
            await _mockStartupManager.DidNotReceive().EnableAutoStartAsync();

            // 2. Config-Speicherung gepr�ft
            _mockConfigService.Received(1).SaveConfig(Arg.Any<AppConfig>());
            savedConfig.ShouldNotBeNull();
            savedConfig.Settings.StartWithWindows.ShouldBeFalse();
        }
    }
}



