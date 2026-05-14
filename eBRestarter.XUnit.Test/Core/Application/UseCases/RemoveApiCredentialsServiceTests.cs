using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.UseCases.RemoveApiCredentials;
using eBRestarter.Core.Domain.Entities;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.Tests.Core.Application.UseCases.RemoveApiCredentials
{
    /// <summary>
    /// Testet den RemoveApiCredentialsService.
    /// Der Fokus liegt hier auf dem korrekten Überschreiben der API-Felder (via "with" expression),
    /// ohne dass andere Konfigurationswerte versehentlich gelöscht oder verändert werden.
    /// </summary>
    public class RemoveApiCredentialsServiceTests
    {
        private readonly Mock<IEVisitorConfigService> _mockConfigService;
        private readonly RemoveApiCredentialsService _sut;

        public RemoveApiCredentialsServiceTests()
        {
            _mockConfigService = new Mock<IEVisitorConfigService>();
            _sut = new RemoveApiCredentialsService(_mockConfigService.Object);
        }
        // 1. HAPPY PATH (KORREKTES LÖSCHEN DER CREDENTIALS)

        /// <summary>
        /// Stellt sicher, dass die Methode ApiUsername und ApiKey auf string.Empty setzt,
        /// aber alle anderen Werte in der Config exakt beibehält.
        /// </summary>
        [Fact]
        public void Execute_ShouldClearCredentials_AndKeepOtherSettingsIntact()
        {
            // ARRANGE
            var existingConfig = new AppConfig
            {
                Username = "HauptUser",
                Settings = new SettingsConfig
                {
                    ApiUsername = "MeinApiUser123",
                    ApiKey = "GeheimerSchluessel",
                    Theme = "Dark", // Darf nicht verändert werden!
                    Language = 1    // Darf nicht verändert werden!
                }
            };

            _mockConfigService.Setup(c => c.LoadConfig()).Returns(existingConfig);

            // Callback-Trick: Wir fangen das Objekt ab, das an SaveConfig übergeben wird,
            // um es danach genau analysieren zu können.
            AppConfig? savedConfig = null;
            _mockConfigService
                .Setup(c => c.SaveConfig(It.IsAny<AppConfig>()))
                .Callback<AppConfig>(config => savedConfig = config);

            // ACT
            _sut.Execute();

            // ASSERT
            _mockConfigService.Verify(c => c.LoadConfig(), Times.Once);
            _mockConfigService.Verify(c => c.SaveConfig(It.IsAny<AppConfig>()), Times.Once);

            savedConfig.ShouldNotBeNull();

            // 1. Check: Sind die API Daten leer?
            savedConfig.Settings.ApiUsername.ShouldBe(string.Empty);
            savedConfig.Settings.ApiKey.ShouldBe(string.Empty);

            // 2. Check: Sind die restlichen Daten unangetastet?
            savedConfig.Username.ShouldBe("HauptUser");
            savedConfig.Settings.Theme.ShouldBe("Dark");
            savedConfig.Settings.Language.ShouldBe(1);
        }
        // 2. EXCEPTION BUBBLING (FEHLER WEITERREICHEN)

        /// <summary>
        /// Da der Service keinen eigenen try-catch-Block hat, müssen Fehler vom
        /// ConfigService (z.B. wenn die config.json gelöscht oder blockiert wurde)
        /// sauber nach oben an den Aufrufer (z.B. das ViewModel) durchgereicht werden.
        /// </summary>
        [Fact]
        public void Execute_ShouldBubbleUpException_WhenLoadConfigFails()
        {
            // ARRANGE
            _mockConfigService
                .Setup(c => c.LoadConfig())
                .Throws(new InvalidOperationException("Datei ist blockiert."));

            // ACT & ASSERT
            var exception = Should.Throw<InvalidOperationException>(() => _sut.Execute());
            exception.Message.ShouldBe("Datei ist blockiert.");

            // Da das Laden fehlschlug, darf SaveConfig niemals aufgerufen werden
            _mockConfigService.Verify(c => c.SaveConfig(It.IsAny<AppConfig>()), Times.Never);
        }

        [Fact]
        public void Execute_ShouldBubbleUpException_WhenSaveConfigFails()
        {
            // ARRANGE
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(new AppConfig());
            _mockConfigService
                .Setup(c => c.SaveConfig(It.IsAny<AppConfig>()))
                .Throws(new UnauthorizedAccessException("Keine Schreibrechte."));

            // ACT & ASSERT
            var exception = Should.Throw<UnauthorizedAccessException>(() => _sut.Execute());
            exception.Message.ShouldBe("Keine Schreibrechte.");
        }
    }
}
