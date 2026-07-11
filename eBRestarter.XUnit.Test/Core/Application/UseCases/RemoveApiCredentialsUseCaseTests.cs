using eBRestarter.Core.Application.Ports.Inbound.UseCases;
using eBRestarter.Core.Application.UseCases;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Domain.ValueObjects;
using Moq;
using Shouldly;
using System;
using Xunit;
using eBRestarter.Core.Application.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

namespace eBRestarter.Tests.Core.Application.UseCases.RemoveApiCredentials
{
    /// <summary>
    /// Testet den RemoveApiCredentialsUseCase.
    /// Der Fokus liegt hier auf dem korrekten �berschreiben der API-Felder (via "with" expression),
    /// ohne dass andere Konfigurationswerte versehentlich gel�scht oder ver�ndert werden.
    /// </summary>
    public class RemoveApiCredentialsUseCaseTests
    {
        private readonly Mock<IOutboundPortEVisitorConfigRepository> _mockConfigService;
        private readonly RemoveApiCredentialsUseCase _sut;

        public RemoveApiCredentialsUseCaseTests()
        {
            _mockConfigService = new Mock<IOutboundPortEVisitorConfigRepository>();
            _sut = new RemoveApiCredentialsUseCase(_mockConfigService.Object);
        }
        // 1. HAPPY PATH (KORREKTES L�SCHEN DER CREDENTIALS)

        /// <summary>
        /// Stellt sicher, dass die Methode ApiUsername und ApiKey auf string.Empty setzt,
        /// aber alle anderen Werte in der Config exakt beibeh�lt.
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
                    Theme = "Dark", // Darf nicht ver�ndert werden!
Language = 1    // Darf nicht ver�ndert werden!
                }
            };

            _mockConfigService.Setup(c => c.LoadConfig()).Returns(existingConfig);

            // Callback-Trick: Wir fangen das Objekt ab, das an SaveConfig �bergeben wird,
            // um es danach genau analysieren zu k�nnen.
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
        /// Da der Service keinen eigenen try-catch-Block hat, m�ssen Fehler vom
        /// ConfigService (z.B. wenn die config.json gel�scht oder blockiert wurde)
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

