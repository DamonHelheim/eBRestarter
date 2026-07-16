using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using Moq;
using Shouldly;
using System.IO;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Infrastructure.BehavioralComponents.Providers;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den WindowsOsPathProvider.
    /// Hier prüfen wir, ob die Klasse die Basis-Pfade (aus dem IAppPathPort)
    /// korrekt mit den App-spezifischen Ordnern und Dateinamen kombiniert.
    /// </summary>
    public class WindowsOsPathProviderAdapterTests
    {
        private readonly Mock<IOutboundPortAppPathProvider> _mockPathProvider;
        private readonly WindowsAppPathProvider _sut;

        public WindowsOsPathProviderAdapterTests()
        {
            // Wir mocken den Provider, damit wir nicht auf echte Windows-Pfade angewiesen sind.
            // So funktionieren die Tests auf jedem PC exakt gleich.
            _mockPathProvider = new Mock<IOutboundPortAppPathProvider>();

            _sut = new WindowsAppPathProvider(_mockPathProvider.Object);
        }

        // 1. APPDATA PATH TESTS

        /// <summary>
        /// Die App speichert ihre Daten nicht direkt im Basis-Ordner, sondern in einem
        /// Firmen- ("Skylar") und App-Unterordner ("eBRestarter"). Das muss korrekt verkettet werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir geben der Klasse einen Fake-Basis-Pfad und prüfen, ob "Skylar" und "eBRestarter"
        /// fehlerfrei angehängt werden.
        /// </summary>
        [Fact]
        public void GetAppDataPath_ShouldCombineLocalAppData_WithSkylarAndAppFolder()
        {
            // ARRANGE
            string fakeLocalAppData = @"C:\FakeUsers\Max\AppData\Local";
            _mockPathProvider.Setup(p => p.RetrieveLocalAppDataDirectory()).Returns(fakeLocalAppData);

            string expectedPath = Path.Combine(fakeLocalAppData, "Skylar", "eBRestarter");

            // ACT
            string actualPath = _sut.RetrieveAppDataPath();

            // ASSERT
            actualPath.ShouldBe(expectedPath);
        }

        // 2. DOWNLOADS PATH TESTS

        /// <summary>
        /// Um den korrekten Download-Ordner des Nutzers zu finden, muss die Klasse vom
        /// UserProfile-Ordner ausgehen und "Downloads" anhängen.
        /// </summary>
        [Fact]
        public void GetDownloadsPath_ShouldCombineUserProfile_WithDownloadsFolder()
        {
            // ARRANGE
            string fakeUserProfile = @"C:\FakeUsers\Max";
            _mockPathProvider.Setup(p => p.RetrieveUserProfileDirectory()).Returns(fakeUserProfile);

            string expectedPath = Path.Combine(fakeUserProfile, "Downloads");

            // ACT
            string actualPath = _sut.RetrieveDownloadsPath();

            // ASSERT
            actualPath.ShouldBe(expectedPath);
        }

        // 3. FILE PATH TESTS (Config & Log)

        /// <summary>
        /// Die Konfigurationsdatei muss zwingend im AppData-Verzeichnis liegen und
        /// exakt "eBRestarterConfig.json" heißen.
        ///
        /// WAS WIRD GETESTET?
        /// Da RetrieveConfigFilePath intern RetrieveAppDataPath aufruft, müssen wir auch hier den
        /// LocalAppData-Pfad mocken. Danach prüfen wir die gesamte Kette.
        /// </summary>
        [Fact]
        public void GetConfigFilePath_ShouldCombineAppDataPath_WithConfigFileName()
        {
            // ARRANGE
            string fakeLocalAppData = @"C:\FakeUsers\Max\AppData\Local";
            _mockPathProvider.Setup(p => p.RetrieveLocalAppDataDirectory()).Returns(fakeLocalAppData);

            // Der erwartete Pfad baut sich aus Base + Skylar + eBRestarter + Config.json zusammen
            string expectedPath = Path.Combine(fakeLocalAppData, "Skylar", "eBRestarter", "eBRestarterConfig.json");

            // ACT
            string actualPath = _sut.RetrieveConfigFilePath();

            // ASSERT
            actualPath.ShouldBe(expectedPath);
        }

        /// <summary>
        /// Die Log-Datei muss zwingend im AppData-Verzeichnis liegen und "log.txt" heißen.
        /// </summary>
        [Fact]
        public void GetLogFilePath_ShouldCombineAppDataPath_WithLogFileName()
        {
            // ARRANGE
            string fakeLocalAppData = @"C:\FakeUsers\Max\AppData\Local";
            _mockPathProvider.Setup(p => p.RetrieveLocalAppDataDirectory()).Returns(fakeLocalAppData);

            // Der erwartete Pfad baut sich aus Base + Skylar + eBRestarter + log.txt zusammen
            string expectedPath = Path.Combine(fakeLocalAppData, "Skylar", "eBRestarter", "log.txt");

            // ACT
            string actualPath = _sut.RetrieveLogFilePath();

            // ASSERT
            actualPath.ShouldBe(expectedPath);
        }
    }
}








