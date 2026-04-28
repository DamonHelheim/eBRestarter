using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.UseCases.ToggleAppAutoStart;
using eBRestarter.Core.Application.Models.Config;
using Moq;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.Tests.Core.Application.UseCases.ToggleAppAutoStart
{
    /// <summary>
    /// Testet den ToggleAppAutoStartService.
    /// Der Fokus liegt auf der korrekten Synchronisation zwischen der JSON-Config
    /// und den tatsächlichen Windows-Registry-Werten.
    /// </summary>
    public class ToggleAppAutoStartServiceTests
    {
        private readonly Mock<IWindowsStartupManagerService> _mockStartupManager;
        private readonly Mock<IEVisitorConfigService> _mockConfigService;
        private readonly ToggleAppAutoStartService _sut;

        public ToggleAppAutoStartServiceTests()
        {
            _mockStartupManager = new Mock<IWindowsStartupManagerService>();
            _mockConfigService = new Mock<IEVisitorConfigService>();

            _sut = new ToggleAppAutoStartService(
                _mockStartupManager.Object,
                _mockConfigService.Object);
        }

        // =========================================================
        // 1. INITIALISIERUNG & SYNCHRONISATION
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn in der Config "StartWithWindows = true" steht, aber Windows meldet,
        /// dass der Autostart nicht aktiv ist (z.B. weil der User ihn im Task-Manager deaktiviert hat
        /// oder es der erste Start ist), muss die App ihn im Betriebssystem (nach)aktivieren.
        /// </summary>
        [Fact]
        public async Task InitializeAndGetStateAsync_ShouldEnableInOs_WhenConfigIsTrueAndOsIsFalse()
        {
            // ARRANGE
            var config = new AppConfig { Settings = new SettingsConfig { StartWithWindows = true } };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(config);

            // OS sagt: Autostart ist momentan AUS
            _mockStartupManager.Setup(s => s.IsAutoStartEnabledAsync()).ReturnsAsync(false);

            // ACT
            var result = await _sut.InitializeAndGetStateAsync();

            // ASSERT
            result.ShouldBeTrue(); // Die Methode muss am Ende 'true' zurückgeben

            // Es muss zwingend der Befehl zur Aktivierung an Windows gesendet worden sein
            _mockStartupManager.Verify(s => s.EnableAutoStartAsync(), Times.Once);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn Config und OS bereits übereinstimmen (beide true oder beide false)
        /// oder die Config false ist, soll keine unnötige Aktion im OS ausgeführt werden.
        /// </summary>
        [Theory]
        [InlineData(true, true, true)]    // Config: An,  OS: An  -> Erwartet: An
        [InlineData(false, false, false)] // Config: Aus, OS: Aus -> Erwartet: Aus
        [InlineData(false, true, true)]   // Config: Aus, OS: An  -> Erwartet: An (Dein Code überschreibt das OS hier nicht!)
        public async Task InitializeAndGetStateAsync_ShouldReturnOsState_WhenNoSyncNeeded(
            bool configState, bool osState, bool expectedResult)
        {
            // ARRANGE
            var config = new AppConfig { Settings = new SettingsConfig { StartWithWindows = configState } };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(config);
            _mockStartupManager.Setup(s => s.IsAutoStartEnabledAsync()).ReturnsAsync(osState);

            // ACT
            var result = await _sut.InitializeAndGetStateAsync();

            // ASSERT
            result.ShouldBe(expectedResult);

            // Es darf keine Aktion zum Verändern des OS-Status ausgeführt worden sein
            _mockStartupManager.Verify(s => s.EnableAutoStartAsync(), Times.Never);
            _mockStartupManager.Verify(s => s.DisableAutoStartAsync(), Times.Never);
        }

        // =========================================================
        // 2. TOGGLE-AKTIONEN DURCH DEN BENUTZER
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn der Benutzer den Autostart in der UI aktiviert, müssen zwei Dinge passieren:
        /// 1. Der OS-Service muss aktiviert werden.
        /// 2. Die Änderung muss dauerhaft in der Config gespeichert werden.
        /// </summary>
        [Fact]
        public async Task ToggleAsync_ShouldEnableInOs_AndSaveConfig_WhenTrue()
        {
            // ARRANGE
            var initialConfig = new AppConfig { Settings = new SettingsConfig { StartWithWindows = false } };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(initialConfig);

            AppConfig? savedConfig = null;
            _mockConfigService.Setup(c => c.SaveConfig(It.IsAny<AppConfig>()))
                              .Callback<AppConfig>(c => savedConfig = c);

            // ACT
            await _sut.ToggleAsync(true);

            // ASSERT
            // 1. OS-Befehl geprüft
            _mockStartupManager.Verify(s => s.EnableAutoStartAsync(), Times.Once);
            _mockStartupManager.Verify(s => s.DisableAutoStartAsync(), Times.Never);

            // 2. Config-Speicherung geprüft
            _mockConfigService.Verify(c => c.SaveConfig(It.IsAny<AppConfig>()), Times.Once);
            savedConfig.ShouldNotBeNull();
            savedConfig.Settings.StartWithWindows.ShouldBeTrue();
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Das genaue Gegenteil zum vorherigen Test: Wenn der Benutzer deaktiviert,
        /// muss das OS den Eintrag löschen und die Config auf "false" gesetzt werden.
        /// </summary>
        [Fact]
        public async Task ToggleAsync_ShouldDisableInOs_AndSaveConfig_WhenFalse()
        {
            // ARRANGE
            var initialConfig = new AppConfig { Settings = new SettingsConfig { StartWithWindows = true } };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(initialConfig);

            AppConfig? savedConfig = null;
            _mockConfigService.Setup(c => c.SaveConfig(It.IsAny<AppConfig>()))
                              .Callback<AppConfig>(c => savedConfig = c);

            // ACT
            await _sut.ToggleAsync(false);

            // ASSERT
            // 1. OS-Befehl geprüft
            _mockStartupManager.Verify(s => s.DisableAutoStartAsync(), Times.Once);
            _mockStartupManager.Verify(s => s.EnableAutoStartAsync(), Times.Never);

            // 2. Config-Speicherung geprüft
            _mockConfigService.Verify(c => c.SaveConfig(It.IsAny<AppConfig>()), Times.Once);
            savedConfig.ShouldNotBeNull();
            savedConfig.Settings.StartWithWindows.ShouldBeFalse();
        }
    }
}