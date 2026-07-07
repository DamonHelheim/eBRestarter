using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models;
using Moq;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Browser;
using eBRestarter.Infrastructure.Adapters.Outbound.WindowsOS.Providers;

namespace eBRestarter.Tests.Infrastructure.Adapters.WindowsOS
{
    /// <summary>
    /// Testet den WindowsBrowserDiscoveryProvider.
    /// Hier wird hauptsächlich die Logik geprüft, wie die Factory aufgerufen wird,
    /// wie die Mappings stattfinden und wie der Service mit Fehlern umgeht.
    /// </summary>
    public class WindowsBrowserDiscoveryProviderTests
    {
        private readonly Mock<IOutboundPortBrowserFactory> _mockBrowserFactory;

        public WindowsBrowserDiscoveryProviderTests()
        {
            _mockBrowserFactory = new Mock<IOutboundPortBrowserFactory>();
        }
        // 1. CONSTRUCTOR TESTS

        /// <summary>
        /// Dependency Injection Regel: Ein Service darf nicht instanziiert werden können,
        /// wenn seine zwingenden Abhängigkeiten (hier IBrowserFactoryPort) null sind.
        ///
        /// WAS WIRD GETESTET?
        /// Wir übergeben "null" an den Konstruktor und erwarten eine ArgumentNullException.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenFactoryIsNull()
        {
            // ACT
            Action act = () => new AdapterWindowsBrowserDiscoveryProvider(null!, Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsBrowserDiscoveryProvider>.Instance);

            // ASSERT
            act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("browserFactoryPort");
        }
        // 2. MAPPING TESTS (Happy Path)

        /// <summary>
        /// Wenn ein Browser auf dem System installiert ist, müssen dessen korrekte Version
        /// und Pfade in das finale BrowserInfo-Objekt gemappt werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir zwingen die Factory dazu, für JEDEN BrowserType im Enum einen simulierten,
        /// "installierten" Browser zurückzugeben. Danach prüfen wir, ob die Properties
        /// (IsInstalled, Version, etc.) korrekt in die Liste übernommen wurden.
        /// </summary>
        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldMapInstalledBrowserCorrectly()
        {
            // ARRANGE
            var service = new AdapterWindowsBrowserDiscoveryProvider(_mockBrowserFactory.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsBrowserDiscoveryProvider>.Instance);
            int enumCount = Enum.GetValues<BrowserType>().Length;

            // Wir simulieren einen installierten Browser
            var mockInstalledBrowser = new Mock<IOutboundPortBrowser>();
            mockInstalledBrowser.Setup(b => b.IsInstalled).Returns(true);
            mockInstalledBrowser.Setup(b => b.BrowserVersion).Returns("120.0.6099.109");
            mockInstalledBrowser.Setup(b => b.DisplayName).Returns("Mocked Browser");
            mockInstalledBrowser.Setup(b => b.IconPath).Returns(@"C:\icon.exe");
            mockInstalledBrowser.Setup(b => b.DownloadUrl).Returns("https://download.com");

            // Die Factory liefert für JEDEN abgerufenen Typen unseren Mock zurück
            _mockBrowserFactory
                .Setup(f => f.Create(It.IsAny<BrowserType>()))
                .Returns(mockInstalledBrowser.Object);

            // ACT
            var result = await service.FindInstalledBrowsersAsync();

            // ASSERT
            var browserList = result.ToList();

            // Es müssen genauso viele Ergebnisse zurückkommen, wie das Enum Einträge hat
            browserList.Count.ShouldBe(enumCount);

            // Wir prüfen exemplarisch den ersten Eintrag in der Liste
            var firstBrowser = browserList.First();
            firstBrowser.IsInstalled.ShouldBeTrue();
            firstBrowser.Version.ShouldBe("120.0.6099.109"); // Originale Version wurde übernommen
            firstBrowser.Name.ShouldBe("Mocked Browser");
            firstBrowser.IconPath.ShouldBe(@"C:\icon.exe");
            firstBrowser.DownloadUrl.ShouldBe("https://download.com");
        }

        /// <summary>
        /// Wenn ein Browser Not installed ist, greift eine Fallback-Logik in deinem Service:
        /// Die Version wird hardcodiert auf "Not installed" gesetzt, anstatt was auch immer
        /// das IBrowserPort-Objekt liefern würde.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren un-installierte Browser. Wir prüfen explizit, ob die "Version" Property
        /// im resultierenden BrowserInfo Objekt den Text "Not installed" enthält.
        /// </summary>
        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldMapUninstalledBrowserCorrectly()
        {
            // ARRANGE
            var service = new AdapterWindowsBrowserDiscoveryProvider(_mockBrowserFactory.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsBrowserDiscoveryProvider>.Instance);

            var mockUninstalledBrowser = new Mock<IOutboundPortBrowser>();
            mockUninstalledBrowser.Setup(b => b.IsInstalled).Returns(false);
            mockUninstalledBrowser.Setup(b => b.BrowserVersion).Returns("Sollte ignoriert werden");
            mockUninstalledBrowser.Setup(b => b.DisplayName).Returns("Missing Browser");

            _mockBrowserFactory
                .Setup(f => f.Create(It.IsAny<BrowserType>()))
                .Returns(mockUninstalledBrowser.Object);

            // ACT
            var result = await service.FindInstalledBrowsersAsync();

            // ASSERT
            var firstBrowser = result.First();
            firstBrowser.IsInstalled.ShouldBeFalse();

            // HIER IST DER KERN DES TESTS: Wurde die Fallback-Logik angewendet?
            firstBrowser.Version.ShouldBe("Not installed");
        }
        // 3. EXCEPTION HANDLING TESTS

        /// <summary>
        /// Wenn du einen neuen Browser zum Enum hinzufügst, aber vergisst, ihn in der Factory
        /// zu registrieren, wirft die Factory (hoffentlich) eine NotSupportedException.
        /// Der Service darf dadurch NICHT abstürzen, sondern muss diesen Browser überspringen.
        ///
        /// WAS WIRD GETESTET?
        /// Wir zwingen die Factory dazu, prinzipiell immer eine NotSupportedException zu werfen.
        /// Wir prüfen, ob die Methode sauber durchläuft und eine leere Liste zurückgibt.
        /// </summary>
        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldSkipBrowser_WhenFactoryThrowsNotSupportedException()
        {
            // ARRANGE
            var service = new AdapterWindowsBrowserDiscoveryProvider(_mockBrowserFactory.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsBrowserDiscoveryProvider>.Instance);

            // Egal welcher Typ reinkommt -> Werfe Exception
            _mockBrowserFactory
                .Setup(f => f.Create(It.IsAny<BrowserType>()))
                .Throws<NotSupportedException>();

            // ACT
            var result = await service.FindInstalledBrowsersAsync();

            // ASSERT
            // Die Schleife fängt den Fehler auf ('continue'), daher kommt eine leere Liste zurück
            result.ShouldBeEmpty();
        }

        /// <summary>
        /// Ein Registry-Check in der jeweiligen Browser-Implementierung (z.B. ChromeBrowser)
        /// könnte fehlschlagen und eine allgemeine Exception (z.B. SecurityException) werfen.
        /// Auch dann darf der Rest des Programms nicht abstürzen, andere Browser sollen
        /// weiterhin geladen werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir zwingen den Getter einer Browser-Property (hier IsInstalled), eine Exception zu werfen.
        /// Der globale Catch-Block im Service muss diese fangen. Die Liste ist am Ende leer, aber
        /// die Exception durfte nicht nach "oben" (in den Testrunner/die App) entkommen.
        /// </summary>
        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldHandleGeneralExceptions_Gracefully()
        {
            // ARRANGE
            var service = new AdapterWindowsBrowserDiscoveryProvider(_mockBrowserFactory.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<AdapterWindowsBrowserDiscoveryProvider>.Instance);

            var buggyBrowserMock = new Mock<IOutboundPortBrowser>();
            // Wir simulieren, dass beim Zugriff auf die Property ein Fehler passiert (z.B. Registry defekt)
            buggyBrowserMock.Setup(b => b.IsInstalled).Throws(new InvalidOperationException("Registry Error"));

            _mockBrowserFactory
                .Setup(f => f.Create(It.IsAny<BrowserType>()))
                .Returns(buggyBrowserMock.Object);

            // ACT
            var result = await service.FindInstalledBrowsersAsync();

            // ASSERT
            // Methode darf nicht abstürzen, fängt den Fehler ab und liefert eine leere Liste
            result.ShouldBeEmpty();
        }
    }
}









