using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.UseCases.ToggleEdgeStartupBoost;
using eBRestarter.Core.Application.Enums;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.Tests.Core.Application.UseCases.ToggleEdgeStartupBoost
{
    /// <summary>
    /// Testet den ToggleEdgeStartupBoostService.
    /// Der Fokus liegt auf der Verifizierung, ob die Befehle korrekt an die
    /// Windows-Services weitergeleitet werden und Fehler beim Umschalten (Toggle)
    /// sicher abgefangen werden.
    /// </summary>
    public class ToggleEdgeStartupBoostServiceTests
    {
        private readonly Mock<IWindowsStartupManagerService> _mockStartupService;
        private readonly Mock<IBrowserFactory> _mockBrowserFactory;
        private readonly Mock<IBrowser> _mockEdgeBrowser;
        private readonly ToggleEdgeStartupBoostService _sut;

        public ToggleEdgeStartupBoostServiceTests()
        {
            _mockStartupService = new Mock<IWindowsStartupManagerService>();
            _mockBrowserFactory = new Mock<IBrowserFactory>();
            _mockEdgeBrowser = new Mock<IBrowser>();

            // Standard-Setup: Wenn die Factory nach Edge gefragt wird, liefern wir unseren Mock zurück
            _mockBrowserFactory
                .Setup(f => f.Create(BrowserType.Edge))
                .Returns(_mockEdgeBrowser.Object);

            _sut = new ToggleEdgeStartupBoostService(
                _mockStartupService.Object,
                _mockBrowserFactory.Object);
        }

        // =========================================================
        // 1. IS ENABLED / IS INSTALLED - TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Stellt sicher, dass der Status des Startup-Boosts 1:1 vom Windows-Service
        /// durchgereicht wird, ohne dass die Logik ihn verfälscht.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsEnabled_ShouldReturnExactStateFromStartupService(bool expectedState)
        {
            // ARRANGE
            _mockStartupService.Setup(s => s.IsEdgeStartupBoostEnabled()).Returns(expectedState);

            // ACT
            var result = _sut.IsEnabled();

            // ASSERT
            result.ShouldBe(expectedState);
            _mockStartupService.Verify(s => s.IsEdgeStartupBoostEnabled(), Times.Once);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Die UI muss wissen, ob Edge überhaupt installiert ist, bevor sie den Schalter anzeigt.
        /// Der Test prüft, ob der Service den Edge-Browser aus der Factory anfordert und
        /// dessen 'IsInstalled' Property korrekt auswertet.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsEdgeInstalled_ShouldReturnStateFromEdgeBrowserInstance(bool isInstalled)
        {
            // ARRANGE
            _mockEdgeBrowser.Setup(b => b.IsInstalled).Returns(isInstalled);

            // ACT
            var result = _sut.IsEdgeInstalled();

            // ASSERT
            result.ShouldBe(isInstalled);

            // Verifizieren, dass explizit Edge angefordert wurde (nicht Chrome o.ä.)
            _mockBrowserFactory.Verify(f => f.Create(BrowserType.Edge), Times.Once);
        }

        // =========================================================
        // 2. TOGGLE (UMSCHALTEN) - TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Der "Happy Path": Wenn der Benutzer den Schalter umlegt (egal ob an oder aus),
        /// muss der Windows-Service mit exakt diesem Wert aufgerufen werden und eine
        /// Erfolgs-Response zurückliefern.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Toggle_ShouldCallStartupService_AndReturnSuccess(bool targetState)
        {
            // ACT
            var result = _sut.Toggle(targetState);

            // ASSERT
            // 1. Wurde der korrekte Wert an das OS gesendet?
            _mockStartupService.Verify(s => s.SetEdgeStartupBoost(targetState), Times.Once);

            // 2. Stimmt das Response-Objekt?
            result.Success.ShouldBeTrue();
            result.NewState.ShouldBe(targetState);
            result.ErrorMessage.ShouldBeEmpty();
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Fehlerbehandlung: Wenn es beim Ã„ndern der Registry/Richtlinie knallt
        /// (z.B. fehlende Admin-Rechte / UnauthorizedAccessException), darf die
        /// App nicht abstürzen. Das Response-Objekt muss den Fehler sauber verpacken.
        /// </summary>
        [Fact]
        public void Toggle_ShouldCatchException_AndReturnFailureResponse()
        {
            // ARRANGE
            // Wir simulieren: Der User will es AKTIVIEREN (true), aber es knallt.
            _mockStartupService
                .Setup(s => s.SetEdgeStartupBoost(It.IsAny<bool>()))
                .Throws(new UnauthorizedAccessException("Zugriff auf Registry verweigert."));

            // ACT
            var result = _sut.Toggle(true);

            // ASSERT
            result.Success.ShouldBeFalse();

            // WICHTIG: Da es fehlgeschlagen ist (Ziel war 'true'),
            // muss der gemeldete neue Status '!enable' (also 'false') sein!
            result.NewState.ShouldBeFalse();

            // Fehlermeldung muss durchgereicht werden
            result.ErrorMessage.ShouldBe("Zugriff auf Registry verweigert.");
        }
    }
}