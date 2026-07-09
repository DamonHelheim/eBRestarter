using eBRestarter.Core.Application.Ports.Inbound.UseCases.ScheduleBrowserCleanup;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Domain.Entities;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using System;
using Xunit;
using eBRestarter.Core.Application.UseCases;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Config;

namespace eBRestarter.Tests.Core.Application.UseCases.ScheduleBrowserCleanup
{
    /// <summary>
    /// Testet den ScheduleBrowserCleanupUseCase.
    /// Nutzt den FakeTimeProvider, um das aktuelle Datum einzufrieren und
    /// die exakte Berechnung des nächsten Bereinigungs-Datums zu validieren.
    /// </summary>
    public class ScheduleBrowserCleanupUseCaseTests
    {
        private readonly Mock<IOutboundPortEVisitorConfigRepository> _mockConfigService;
        private readonly FakeTimeProvider _fakeTimeProvider;
        private readonly ScheduleBrowserCleanupUseCase _sut;

        public ScheduleBrowserCleanupUseCaseTests()
        {
            _mockConfigService = new Mock<IOutboundPortEVisitorConfigRepository>();
            _fakeTimeProvider = new FakeTimeProvider();

            _sut = new ScheduleBrowserCleanupUseCase(
                _mockConfigService.Object,
                _fakeTimeProvider);
        }
        // 1. GÜLTIGE INTERVALLE (AKTIVIERUNG)

        /// <summary>
        /// Die Anwendung erlaubt nur spezifische Intervalle für die Bereinigung (1, 3, 7, 14 Tage).
        /// Wenn ein gültiges Intervall gewählt wird, muss das nächste Datum exakt
        /// berechnet, in der Config gespeichert und in der Response zurückgegeben werden.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(7)]
        [InlineData(14)]
        public void UpdateSchedule_ShouldActivate_AndCalculateNextDate_WhenIntervalIsValid(int validIntervalDays)
        {
            // ARRANGE
            // 1. Wir frieren die Zeit künstlich auf den 10. Januar 2026 ein
            var testDate = new DateTimeOffset(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);
            _fakeTimeProvider.SetUtcNow(testDate);

            // 2. Mock-Setup für die Config
            var dummyConfig = new AppConfig { Browser = new BrowserConfig() };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(dummyConfig);

            // Callback-Trick, um das gespeicherte Objekt abzufangen
            AppConfig? savedConfig = null;
            _mockConfigService.Setup(c => c.SaveConfig(It.IsAny<AppConfig>()))
                              .Callback<AppConfig>(config => savedConfig = config);

            var request = new ScheduleBrowserCleanupRequest(validIntervalDays);

            // ACT
            var response = _sut.UpdateSchedule(request);

            // ASSERT - Response
            response.ShouldNotBeNull();
            response.IsActive.ShouldBeTrue();

            // Das erwartete Datum ist der 10. Januar + das Intervall (Uhrzeit abgeschnitten durch .Date)
            var expectedDate = testDate.Date.AddDays(validIntervalDays);
            response.NextDate.ShouldBe(expectedDate);

            // ASSERT - Gespeicherte Config
            savedConfig.ShouldNotBeNull();
            savedConfig.Browser.DeleteBrowserCacheIntervalDays.ShouldBe(validIntervalDays);
            savedConfig.Browser.NextBrowserDeleteCacheDate.ShouldBe(expectedDate);

            _mockConfigService.Verify(c => c.SaveConfig(It.IsAny<AppConfig>()), Times.Once);
        }
        // 2. UNGÜLTIGE INTERVALLE (DEAKTIVIERUNG)

        /// <summary>
        /// Wenn der User ein nicht-unterstütztes Intervall (wie 0, 5 oder -1) übermittelt,
        /// muss die Bereinigung deaktiviert werden. Das nächste Datum wird auf MinValue gesetzt.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(-1)]
        [InlineData(99)]
        public void UpdateSchedule_ShouldDeactivate_AndClearNextDate_WhenIntervalIsInvalid(int invalidIntervalDays)
        {
            // ARRANGE
            var dummyConfig = new AppConfig { Browser = new BrowserConfig() };
            _mockConfigService.Setup(c => c.LoadConfig()).Returns(dummyConfig);

            AppConfig? savedConfig = null;
            _mockConfigService.Setup(c => c.SaveConfig(It.IsAny<AppConfig>()))
                              .Callback<AppConfig>(config => savedConfig = config);

            var request = new ScheduleBrowserCleanupRequest(invalidIntervalDays);

            // ACT
            var response = _sut.UpdateSchedule(request);

            // ASSERT - Response
            response.ShouldNotBeNull();
            response.IsActive.ShouldBeFalse();
            response.NextDate.ShouldBeNull(); // Response gibt null zurück, wenn deaktiviert

            // ASSERT - Gespeicherte Config
            savedConfig.ShouldNotBeNull();
            // Der User-Input (z.B. 0) wird trotzdem als Setting gespeichert
            savedConfig.Browser.DeleteBrowserCacheIntervalDays.ShouldBe(0);
            // Aber das Datum muss sicher auf MinValue gesetzt werden
            savedConfig.Browser.NextBrowserDeleteCacheDate.ShouldBe(DateTime.MinValue);

            _mockConfigService.Verify(c => c.SaveConfig(It.IsAny<AppConfig>()), Times.Once);
        }
        // 3. FEHLERBEHANDLUNG

        /// <summary>
        /// Da die Geschäftlogik keinen eigenen Try-Catch-Block besitzt, müssen
        /// Systemfehler (z. B. wenn die config.json gesperrt ist) ungehindert nach
        /// oben an den Aufrufer weitergegeben werden (Exception Bubbling).
        /// </summary>
        [Fact]
        public void UpdateSchedule_ShouldBubbleUpException_WhenConfigFails()
        {
            // ARRANGE
            var request = new ScheduleBrowserCleanupRequest(7);

            // Wir simulieren einen Dateizugriffsfehler
            _mockConfigService.Setup(c => c.LoadConfig()).Throws(new UnauthorizedAccessException("Access denied"));

            // ACT & ASSERT
            var exception = Should.Throw<UnauthorizedAccessException>(() => _sut.UpdateSchedule(request));
            exception.Message.ShouldBe("Access denied");

            // Wenn das Laden fehlschlägt, darf das Speichern nie aufgerufen werden
            _mockConfigService.Verify(c => c.SaveConfig(It.IsAny<AppConfig>()), Times.Never);
        }
    }
}

