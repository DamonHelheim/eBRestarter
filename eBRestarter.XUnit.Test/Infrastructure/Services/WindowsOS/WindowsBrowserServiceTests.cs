using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models;
using eBRestarter.Infrastructure.Services.WindowsOS;
using Moq;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den WindowsBrowserService.
    /// Hier wird hauptsÃ¤chlich die Logik geprÃ¼ft, wie die Factory aufgerufen wird,
    /// wie die Mappings stattfinden und wie der Service mit Fehlern umgeht.
    /// </summary>
    public class WindowsBrowserServiceTests
    {
        private readonly Mock<IBrowserFactory> _mockBrowserFactory;

        public WindowsBrowserServiceTests()
        {
            _mockBrowserFactory = new Mock<IBrowserFactory>();
        }
        // 1. CONSTRUCTOR TESTS

        /// <summary>
        /// Dependency Injection Regel: Ein Service darf nicht instanziiert werden kÃ¶nnen,
        /// wenn seine zwingenden AbhÃ¤ngigkeiten (hier IBrowserFactory) null sind.
        ///
        /// WAS WIRD GETESTET?
        /// Wir Ã¼bergeben "null" an den Konstruktor und erwarten eine ArgumentNullException.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenFactoryIsNull()
        {
            // ACT
            Action act = () => new WindowsBrowserService(null!);

            // ASSERT
            act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("browserFactory");
        }
        // 2. MAPPING TESTS (Happy Path)

        /// <summary>
        /// Wenn ein Browser auf dem System installiert ist, mÃ¼ssen dessen korrekte Version
        /// und Pfade in das finale BrowserInfo-Objekt gemappt werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir zwingen die Factory dazu, fÃ¼r JEDEN BrowserType im Enum einen simulierten,
        /// "installierten" Browser zurÃ¼ckzugeben. Danach prÃ¼fen wir, ob die Properties
        /// (IsInstalled, Version, etc.) korrekt in die Liste Ã¼bernommen wurden.
        /// </summary>
        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldMapInstalledBrowserCorrectly()
        {
            // ARRANGE
            var service = new WindowsBrowserService(_mockBrowserFactory.Object);
            int enumCount = Enum.GetValues<BrowserType>().Length;

            // Wir simulieren einen installierten Browser
            var mockInstalledBrowser = new Mock<IBrowser>();
            mockInstalledBrowser.Setup(b => b.IsInstalled).Returns(true);
            mockInstalledBrowser.Setup(b => b.BrowserVersion).Returns("120.0.6099.109");
            mockInstalledBrowser.Setup(b => b.DisplayName).Returns("Mocked Browser");
            mockInstalledBrowser.Setup(b => b.IconPath).Returns(@"C:\icon.exe");
            mockInstalledBrowser.Setup(b => b.DownloadUrl).Returns("https://download.com");

            // Die Factory liefert fÃ¼r JEDEN abgerufenen Typen unseren Mock zurÃ¼ck
            _mockBrowserFactory
                .Setup(f => f.Create(It.IsAny<BrowserType>()))
                .Returns(mockInstalledBrowser.Object);

            // ACT
            var result = await service.FindInstalledBrowsersAsync();

            // ASSERT
            var browserList = result.ToList();

            // Es mÃ¼ssen genauso viele Ergebnisse zurÃ¼ckkommen, wie das Enum EintrÃ¤ge hat
            browserList.Count.ShouldBe(enumCount);

            // Wir prÃ¼fen exemplarisch den ersten Eintrag in der Liste
            var firstBrowser = browserList.First();
            firstBrowser.IsInstalled.ShouldBeTrue();
            firstBrowser.Version.ShouldBe("120.0.6099.109"); // Originale Version wurde Ã¼bernommen
            firstBrowser.Name.ShouldBe("Mocked Browser");
            firstBrowser.IconPath.ShouldBe(@"C:\icon.exe");
            firstBrowser.DownloadUrl.ShouldBe("https://download.com");
        }

        /// <summary>
        /// Wenn ein Browser NICHT installiert ist, greift eine Fallback-Logik in deinem Service:
        /// Die Version wird hardcodiert auf "Nicht installiert" gesetzt, anstatt was auch immer
        /// das IBrowser-Objekt liefern wÃ¼rde.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren un-installierte Browser. Wir prÃ¼fen explizit, ob die "Version" Property
        /// im resultierenden BrowserInfo Objekt den Text "Nicht installiert" enthÃ¤lt.
        /// </summary>
        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldMapUninstalledBrowserCorrectly()
        {
            // ARRANGE
            var service = new WindowsBrowserService(_mockBrowserFactory.Object);

            var mockUninstalledBrowser = new Mock<IBrowser>();
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
            firstBrowser.Version.ShouldBe("Nicht installiert");
        }
        // 3. EXCEPTION HANDLING TESTS

        /// <summary>
        /// Wenn du einen neuen Browser zum Enum hinzufÃ¼gst, aber vergisst, ihn in der Factory
        /// zu registrieren, wirft die Factory (hoffentlich) eine NotSupportedException.
        /// Der Service darf dadurch NICHT abstÃ¼rzen, sondern muss diesen Browser Ã¼berspringen.
        ///
        /// WAS WIRD GETESTET?
        /// Wir zwingen die Factory dazu, prinzipiell immer eine NotSupportedException zu werfen.
        /// Wir prÃ¼fen, ob die Methode sauber durchlÃ¤uft und eine leere Liste zurÃ¼ckgibt.
        /// </summary>
        [Fact]
        public async Task GetInstalledBrowsersAsync_ShouldSkipBrowser_WhenFactoryThrowsNotSupportedException()
        {
            // ARRANGE
            var service = new WindowsBrowserService(_mockBrowserFactory.Object);

            // Egal welcher Typ reinkommt -> Werfe Exception
            _mockBrowserFactory
                .Setup(f => f.Create(It.IsAny<BrowserType>()))
                .Throws<NotSupportedException>();

            // ACT
            var result = await service.FindInstalledBrowsersAsync();

            // ASSERT
            // Die Schleife fÃ¤ngt den Fehler auf ('continue'), daher kommt eine leere Liste zurÃ¼ck
            result.ShouldBeEmpty();
        }

        /// <summary>
        /// Ein Registry-Check in der jeweiligen Browser-Implementierung (z.B. ChromeBrowserAdapter)
        /// kÃ¶nnte fehlschlagen und eine allgemeine Exception (z.B. SecurityException) werfen.
        /// Auch dann darf der Rest des Programms nicht abstÃ¼rzen, andere Browser sollen
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
            var service = new WindowsBrowserService(_mockBrowserFactory.Object);

            var buggyBrowserMock = new Mock<IBrowser>();
            // Wir simulieren, dass beim Zugriff auf die Property ein Fehler passiert (z.B. Registry defekt)
            buggyBrowserMock.Setup(b => b.IsInstalled).Throws(new InvalidOperationException("Registry Error"));

            _mockBrowserFactory
                .Setup(f => f.Create(It.IsAny<BrowserType>()))
                .Returns(buggyBrowserMock.Object);

            // ACT
            var result = await service.FindInstalledBrowsersAsync();

            // ASSERT
            // Methode darf nicht abstÃ¼rzen, fÃ¤ngt den Fehler ab und liefert eine leere Liste
            result.ShouldBeEmpty();
        }
    }
}

