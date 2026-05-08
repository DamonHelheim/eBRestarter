using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Infrastructure.Browsers.Abstract;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    /// <summary>
    /// Testet die gemeinsame Logik aller Browser aus der abstrakten BrowserBase.
    /// Indem wir die Basisklasse testen, sparen wir uns redundante Tests für Chrome, Firefox etc.
    /// </summary>
    public class BrowserBaseTests
    {
        // 1. DER TEST-DUMMY

        /// <summary>
        /// Da BrowserBase abstrakt ist, können wir nicht einfach 'new BrowserBase()' aufrufen.
        /// Wir erstellen hier eine minimale, konkrete Implementierung NUR für unsere Tests.
        /// Sie dient als "Hülle", um an die Methoden der Basisklasse heranzukommen.
        /// </summary>
        private class TestDummyBrowser : BrowserBase
        {
            public TestDummyBrowser(IOperatingSystemFacade os, ILogger logger) : base(os, logger) { }

            // Dummy-Werte für die abstrakten Properties. Diese Werte sind für die Tests gröÃŸtenteils egal,
            // sie befriedigen nur den Compiler, damit die Klasse instanziiert werden kann.
            public override BrowserType Type => BrowserType.Firefox;
            public override string DisplayName => "TestBrowser";
            public override string IconPath => "";
            public override string DownloadUrl => "";
            public override string ExtensionInstallUrl => "";
            protected override string ProcessName => "testbrowser";
            protected override string RegistryKeyVersion => @"Software\Test";

            // Wir gaukeln der Basisklasse vor, dass unser Test-Browser nur diesen einen Pfad hat.
            protected override List<string> ExecutablePaths => new() { @"C:\FakePath\browser.exe" };

            public override bool IsExtensionInstalled(string? extensionId = null) => false;
            public override BrowserPaths GetPaths() => new(new List<string>(), new List<string>(), new List<string>());

            // WICHTIG: CleanVersionString ist in der Basisklasse 'protected'.
            // Ein Unit-Test darf "von auÃŸen" nicht auf protected Methoden zugreifen.
            // Diese öffentliche Wrapper-Methode ist ein klassischer Architektur-Trick, um sie testbar zu machen.
            public string ExposeCleanVersionString(string? raw)
            {
                return CleanVersionString(raw);
            }
        }
        // 2. DIE TESTS

        /// <summary>
        /// Wenn wir die Browser-Version aus der Windows-Registry auslesen, schreiben verschiedene Browser
        /// oft "Müll" dazu (z. B. "120.0.1 (x64 de)"). Wenn wir versuchen, diesen String in ein C# Version-Objekt
        /// umzuwandeln, stürzt das Programm ab. Die Regex-Logik in CleanVersionString muss robust sein.
        ///
        /// WAS WIRD GETESTET?
        /// Wir füttern die Methode mit typischen, schmutzigen Strings von Chrome und Firefox sowie mit Edge-Cases
        /// wie NULL, leeren Strings und Strings mit vielen Leerzeichen. Wir prüfen, ob nur die reine Versionsnummer
        /// ("Zahlen und Punkte") zurückkommt.
        /// </summary>
        [Theory]
        [InlineData("120.0.1 (x64 de)", "120.0.1")] // Typischer Firefox-String
        [InlineData("121.0.6167.161 (Official Build) (64-bit)", "121.0.6167.161")] // Typischer Chrome-String
        [InlineData("  115.0.3  ", "115.0.3")] // Testet das Trim() der Basisklasse
        [InlineData("10.0", "10.0")] // Saubere Version (darf nicht kaputt gemacht werden)
        [InlineData(null, "Unknown")] // Null-Check
        [InlineData("", "Unknown")] // Leer-Check
        public void CleanVersionString_ShouldExtractVersionNumberCorrectly(string? rawVersion, string expectedResult)
        {
            // ARRANGE (Vorbereitung):
            // Da diese Regex-Methode das Betriebssystem gar nicht nutzt, reichen leere Mock-Objekte aus.
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger>();
            var dummyBrowser = new TestDummyBrowser(mockOs.Object, mockLogger.Object);

            // ACT (Ausführung):
            // Wir rufen unsere Wrapper-Methode mit dem Test-Datum (InlineData) auf.
            var result = dummyBrowser.ExposeCleanVersionString(rawVersion);

            // ASSERT (Prüfung):
            // Shouldly vergleicht das Ergebnis mit dem expectedResult aus dem [InlineData].
            result.ShouldBe(expectedResult);
        }

        /// <summary>
        /// Die Start-Methode ist geschäftskritisch. Sie muss aus dem Executable-Pfad, der eBesucher-URL
        /// und möglichen Parametern einen fehlerfreien Aufruf für Windows zusammenbauen.
        /// Gleichzeitig wollen wir in einem Unit-Test NICHT, dass sich plötzlich ein echter Browser auf deinem Monitor öffnet!
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren (mocken) das Dateisystem und den Windows-Prozess-Service.
        /// Wir prüfen, ob das Programm die URL und Argumente richtig mit einem Leerzeichen verbindet und
        /// diesen kombinierten String an die Ausführungs-Methode ('OpenUrlInBrowser') übergibt.
        /// </summary>
        [Fact]
        public void Start_ShouldFindExecutableAndCallProcessService_WithUrlAndArguments()
        {
            // ARRANGE (Vorbereitung):
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger>();

            // SCHRITT 1: FileSystem mocken
            // Die Start()-Methode ruft intern GetExecutablePath() auf. Diese sucht in unserer Liste
            // von ExecutablePaths nach einer Datei, die wirklich existiert. Wir weisen unseren Fake-File-Service
            // an, immer 'true' zu antworten, wenn nach "C:\FakePath\browser.exe" gefragt wird.
            var mockFileSystem = new Mock<IWindowsFileSystemService>();
            mockFileSystem.
                Setup(fs => fs.FileExists(@"C:\FakePath\browser.exe")).
                Returns(true);

            // Hängen den Fake-File-Service in unsere Fake-OS-Fassade ein
            mockOs.Setup(os => os.WindowsFileSystemService).Returns(mockFileSystem.Object);

            // SCHRITT 2: ProcessService mocken
            // Das ist der Service, der normalerweise den Browser wirklich startet. Wir machen ihn "stumm".
            var mockProcessService = new Mock<IWindowsProcessControlService>();
            mockOs.Setup(os => os.WindowsProcessControlService).Returns(mockProcessService.Object);

            var dummyBrowser = new TestDummyBrowser(mockOs.Object, mockLogger.Object);

            const string testUrl = "https://www.ebesucher.com/surfbar/test";
            const string testArgs = "--incognito";

            // ACT (Ausführung):
            // Wir feuern die Startmethode ab. Da alles gemockt ist, passiert im echten Windows gar nichts.
            dummyBrowser.Start(testUrl, testArgs);

            // ASSERT (Prüfung):
            // Anstatt Rückgabewerte zu prüfen, prüfen wir hier "Verhalten" (Behavior Verification).
            // Wir fragen Moq: "Wurde auf dem ProcessService die Methode 'OpenUrlInBrowser' exakt 1x aufgerufen?
            // Und waren die Parameter exakt der Pfad und der zusammengebaute String?"
            mockProcessService.Verify(p => p.OpenUrlInBrowser(
                @"C:\FakePath\browser.exe",
                "https://www.ebesucher.com/surfbar/test --incognito"
            ), Times.Once);
        }

        /// <summary>
        /// Ein Nutzer könnte den Browser deinstalliert haben, aber unsere App glaubt noch, er sei da.
        /// In diesem Fall findet 'GetExecutablePath()' die Datei nicht und wirft eine FileNotFoundException.
        /// Unsere App darf deswegen nicht komplett abstürzen! Die Start-Methode hat dafür einen try-catch-Block.
        ///
        /// WAS WIRD GETESTET?
        /// Wir zwingen den FileSystem-Mock dazu, 'false' (Datei existiert nicht) zurückzugeben.
        /// Wir prüfen, ob die Methode den Fehler im try-catch fängt (der Test stürzt also nicht ab)
        /// und sicherstellen, dass KEIN fehlerhafter Startbefehl an Windows gesendet wird.
        /// </summary>
        [Fact]
        public void Start_ShouldLogError_WhenExecutableIsNotFound()
        {
            // ARRANGE (Vorbereitung):
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger>();

            // Diesmal bringen wir dem FileSystem bei: "Egal welcher Pfad gefragt wird, antworte mit 'false'!"
            var mockFileSystem = new Mock<IWindowsFileSystemService>();
            mockFileSystem
                .Setup(fs => fs.FileExists(It.IsAny<string>()))
                .Returns(false);

            mockOs.Setup(os => os.WindowsFileSystemService).Returns(mockFileSystem.Object);

            var mockProcessService = new Mock<IWindowsProcessControlService>();
            mockOs.Setup(os => os.WindowsProcessControlService).Returns(mockProcessService.Object);

            var dummyBrowser = new TestDummyBrowser(mockOs.Object, mockLogger.Object);

            // ACT (Ausführung):
            // Start() wird aufgerufen. Da Datei fehlt, wirft GetExecutablePath() einen Fehler.
            // Dieser wird im catch-Block von Start() gefangen und geloggt.
            dummyBrowser.Start("https://test.com");

            // ASSERT (Prüfung):
            // Da die Ausführung im try-catch abgebrochen wurde, darf OpenUrlInBrowser niemals aufgerufen worden sein!
            // 'Times.Never' bestätigt, dass kein fehlerhafter Startversuch ins Betriebssystem geleakt ist.
            mockProcessService.Verify(p => p.OpenUrlInBrowser(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}