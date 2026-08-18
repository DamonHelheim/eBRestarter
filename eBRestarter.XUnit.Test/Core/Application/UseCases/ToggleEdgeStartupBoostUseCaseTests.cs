using NSubstitute;
using Shouldly;
using System;
using Xunit;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;
using eBRestarter.Core.Application.ObjectArchetypes.Enums;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.UseCases;
using NSubstitute.ExceptionExtensions;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Logging;

namespace eBRestarter.Tests.Core.Application.UseCases.ToggleEdgeStartupBoost
{
    /// <summary>
    /// Testet den ToggleEdgeStartupBoostUseCase.
    /// Der Fokus liegt auf der Verifizierung, ob die Befehle korrekt an die
    /// Windows-Services weitergeleitet werden und Fehler beim Umschalten (Toggle)
    /// sicher abgefangen werden.
    /// </summary>
    public class ToggleEdgeStartupBoostUseCaseTests
    {
        private readonly IOutboundPortBrowserConfigRepository _mockStartupService;
        private readonly IOutboundPortBrowserFactory _mockBrowserFactory;
        private readonly IOutboundPortBrowser _mockEdgeBrowser;
        private readonly ToggleEdgeStartupBoostUseCase _sut;

        public ToggleEdgeStartupBoostUseCaseTests()
        {
            _mockStartupService = Substitute.For<IOutboundPortBrowserConfigRepository>();
            _mockBrowserFactory = Substitute.For<IOutboundPortBrowserFactory>();
            _mockEdgeBrowser = Substitute.For<IOutboundPortBrowser>();

            // Standard-Setup: Wenn die Factory nach Edge gefragt wird, liefern wir unseren Mock zur�ck
            _mockBrowserFactory
                .Create(BrowserType.Edge)
                .Returns(_mockEdgeBrowser);

            _sut = // Signatur: (browserFactory, logger, startupService).
            new ToggleEdgeStartupBoostUseCase(
                _mockBrowserFactory,
                Substitute.For<IOutboundPortApplicationLogger<ToggleEdgeStartupBoostUseCase>>(),
                _mockStartupService);
        }
        // 1. IS ENABLED / IS INSTALLED - TESTS

        /// <summary>
        /// Stellt sicher, dass der Status des Startup-Boosts 1:1 vom Windows-Service
        /// durchgereicht wird, ohne dass die Logik ihn verf�lscht.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsEnabled_ShouldReturnExactStateFromStartupService(bool expectedState)
        {
            // ARRANGE
            _mockStartupService.IsBrowserStartupBoostEnabled().Returns(expectedState);

            // ACT
            var result = _sut.IsEnabled();

            // ASSERT
            result.ShouldBe(expectedState);
            _mockStartupService.Received(1).IsBrowserStartupBoostEnabled();
        }

        /// <summary>
        /// Die UI muss wissen, ob Edge �berhaupt installiert ist, bevor sie den Schalter anzeigt.
        /// Der Test pr�ft, ob der Service den Edge-Browser aus der Factory anfordert und
        /// dessen 'IsInstalled' Property korrekt auswertet.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsEdgeInstalled_ShouldReturnStateFromEdgeBrowserInstance(bool isInstalled)
        {
            // ARRANGE
            _mockEdgeBrowser.IsInstalled.Returns(isInstalled);

            // ACT
            var result = _sut.IsEdgeInstalled();

            // ASSERT
            result.ShouldBe(isInstalled);

            // Verifizieren, dass explizit Edge angefordert wurde (nicht Chrome o.�.)
            _mockBrowserFactory.Received(1).Create(BrowserType.Edge);
        }
        // 2. TOGGLE (UMSCHALTEN) - TESTS

        /// <summary>
        /// Der "Happy Path": Wenn der Benutzer den Schalter umlegt (egal ob an oder aus),
        /// muss der Windows-Service mit exakt diesem Wert aufgerufen werden und eine
        /// Erfolgs-Response zur�ckliefern.
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
            _mockStartupService.Received(1).SetBrowserStartupBoost(targetState);

            // 2. Stimmt das Response-Objekt?
            result.Success.ShouldBeTrue();
            result.NewState.ShouldBe(targetState);
            result.ErrorMessage.ShouldBeEmpty();
        }

        /// <summary>
        /// Fehlerbehandlung: Wenn es beim �ndern der Registry/Richtlinie knallt
        /// (z.B. fehlende Admin-Rechte / UnauthorizedAccessException), darf die
        /// App nicht abst�rzen. Das Response-Objekt muss den Fehler sauber verpacken.
        /// </summary>
        [Fact]
        public void Toggle_ShouldCatchException_AndReturnFailureResponse()
        {
            // ARRANGE
            // Wir simulieren: Der User will es AKTIVIEREN (true), aber es knallt.
            _mockStartupService
                .When(s => s.SetBrowserStartupBoost(Arg.Any<bool>()))
                .Do(_ => throw new UnauthorizedAccessException("Zugriff auf Registry verweigert."));

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








