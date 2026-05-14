using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Infrastructure.Browsers;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    /// <summary>
    /// Testet die hartkodierten Eigenheiten der FirefoxBrowser Klasse,
    /// insbesondere das Parsen der profiles.ini und die .xpi Extension-Logik.
    /// </summary>
    public class FirefoxBrowserTests
    {
        /// <summary>
        /// Firefox verwaltet seine Profile in einer 'profiles.ini'. Diese kann sowohl
        /// relative Pfade (Standard) als auch absolute Pfade (Benutzer hat das Profil auf eine andere Festplatte verschoben) enthalten.
        /// Wenn unsere Parsing-Logik hier fehlschlägt, leert eBesucher die falschen Ordner oder das Programm stürzt ab.
        ///
        /// WAS WIRD GETESTET?
        /// Wir füttern die Methode 'ResolvePaths()' mit einer fiktiven INI-Datei, die genau diese
        /// zwei Fälle (IsRelative=1 und IsRelative=0) enthält. Wir prüfen, ob die Cache-,
        /// Cookie- und Extension-Ordner für beide Profile korrekt zusammengebaut werden.
        /// </summary>
        [Fact]
        public void GetPaths_ShouldParseProfilesIni_AndGeneratePathsForRelativeAndAbsoluteProfiles()
        {
            // ARRANGE (Vorbereitung der Test-Umgebung)
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger<FirefoxBrowser>>();
            var mockFileSystem = new Mock<IWindowsFileSystemService>();

            // 1. Windows-Umgebungsvariablen simulieren (Roaming für Cookies, Local für Cache)
            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("AppData")).Returns(@"C:\Roaming");
            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("LocalAppData")).Returns(@"C:\Local");

            // 2. Universelles CombinePaths Mocking für 'params string[]'
            // Dies nimmt ein Array von Strings (egal wie viele) und klebt sie mit "\" zusammen.
            mockFileSystem
                .Setup(fs => fs.CombinePaths(It.IsAny<string[]>()))
                .Returns<string[]>(paths => string.Join(@"\", paths));

            // 3. Dem System vorgaukeln, dass die profiles.ini Datei an dem erwarteten Ort existiert
            string fakeIniPath = @"C:\Roaming\Mozilla\Firefox\profiles.ini";
            mockFileSystem.Setup(fs => fs.FileExists(fakeIniPath)).Returns(true);

            // 4. Den Inhalt der profiles.ini fälschen. Wir bauen absichtlich ein relatives und ein absolutes Profil ein.
            string[] fakeIniContent = new[]
            {
                "[Profile0]",
                "Name=default",
                "IsRelative=1", // Standard-Profil (Relativ zu AppData)
                "Path=Profiles/abc.default",
                "",
                "[Profile1]",
                "Name=CustomProfile",
                "IsRelative=0", // Verschobenes Profil (Absoluter Pfad auf D:\)
                @"Path=D:\Custom\FirefoxProfile"
            };

            // Wenn der FirefoxBrowser die Datei liest, geben wir ihm unser gefälschtes Array zurück
            mockFileSystem.Setup(fs => fs.ReadAllLines(fakeIniPath)).Returns(fakeIniContent);

            mockOs.Setup(os => os.WindowsFileSystemService).Returns(mockFileSystem.Object);
            var firefoxBrowser = new FirefoxBrowser(mockOs.Object, mockLogger.Object);
            // ACT (Ausführung der Logik)
            var paths = firefoxBrowser.ResolvePaths();
            // ASSERT (Prüfung der Ergebnisse)

            // Prüfung für Profile0 (Relativ):
            // Wir erwarten, dass der Cache in LocalAppData liegt und Cookies/Extensions in Roaming (AppData)
            paths.CacheDirs.ShouldContain(@"C:\Local\Mozilla\Firefox\Profiles\abc.default\cache2\entries");
            paths.CookiesDirs.ShouldContain(@"C:\Roaming\Mozilla\Firefox\Profiles\abc.default\storage\default");
            paths.ExtensionsDirs.ShouldContain(@"C:\Roaming\Mozilla\Firefox\Profiles\abc.default\extensions");

            // Prüfung für Profile1 (Absolut):
            // Wir erwarten, dass Cache den Best-Guess (LocalRoot + Ordnername) nutzt und Cookies/Extensions direkt im absoluten Pfad liegen
            paths.CacheDirs.ShouldContain(@"C:\Local\Mozilla\Firefox\Profiles\FirefoxProfile\cache2\entries");
            paths.CookiesDirs.ShouldContain(@"D:\Custom\FirefoxProfile\storage\default");
            paths.ExtensionsDirs.ShouldContain(@"D:\Custom\FirefoxProfile\extensions");
        }

        /// <summary>
        /// Im Gegensatz zu Chrome nutzt Firefox für Extensions standardmäßig Dateien mit der Endung .xpi.
        /// Wir müssen sicherstellen, dass die FirefoxBrowser-Klasse diese komprimierte Datei korrekt sucht.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren das Dateisystem so, dass die {ID}.xpi Datei im Extensions-Ordner existiert.
        /// Die Methode 'IsExtensionInstalled' MUSS dann 'true' zurückgeben.
        /// </summary>
        [Fact]
        public void IsExtensionInstalled_ShouldReturnTrue_WhenXpiFileExists()
        {
            // ARRANGE
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger<FirefoxBrowser>>();
            var mockFileSystem = new Mock<IWindowsFileSystemService>();

            string fakeRoamingExtensionsDir = @"C:\Roaming\Mozilla\Firefox\Profiles\abc.default\extensions";
            string expectedExtensionId = "{fef425dc-a60f-4484-954d-71ecf2544846}.xpi";
            string fullXpiPath = $@"{fakeRoamingExtensionsDir}\{expectedExtensionId}";

            // Grundlegende Pfade setzen, damit ResolvePaths() funktioniert
            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("AppData")).Returns(@"C:\Roaming");
            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("LocalAppData")).Returns(@"C:\Local");

            mockFileSystem
                .Setup(fs => fs.CombinePaths(It.IsAny<string[]>()))
                .Returns<string[]>(paths => string.Join(@"\", paths));

            // Ein Standard-Profil simulieren, damit ResolvePaths() überhaupt Ordner ausgibt
            mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(fs => fs.ReadAllLines(It.IsAny<string>())).Returns(new[] { "[Profile0]", "Path=Profiles/abc.default" });

            // Die kritischen Mocks für diesen Test:
            // 1. Der generelle Extensions-Ordner existiert
            mockFileSystem.Setup(fs => fs.DirectoryExists(fakeRoamingExtensionsDir)).Returns(true);
            // 2. Die .xpi Datei existiert!
            mockFileSystem.Setup(fs => fs.FileExists(fullXpiPath)).Returns(true);

            mockOs.Setup(os => os.WindowsFileSystemService).Returns(mockFileSystem.Object);
            var firefoxBrowser = new FirefoxBrowser(mockOs.Object, mockLogger.Object);
            // ACT
            bool isInstalled = firefoxBrowser.IsExtensionInstalled();
            // ASSERT
            isInstalled.ShouldBeTrue();
        }

        /// <summary>
        /// Bei Entwickler-Profilen oder Sideloading kann eine Firefox-Extension als entpackter Ordner (ohne .xpi Endung) vorliegen.
        /// Die Methode muss auch diesen "Fallback"-Fall abdecken.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren, dass die .xpi-Datei absichtlich NICHT existiert (Returns false).
        /// Stattdessen existiert aber ein Ordner mit dem exakten Namen der ID (ohne das '.xpi').
        /// Die Methode muss trotzdem erkennen, dass die Extension da ist und 'true' liefern.
        /// </summary>
        [Fact]
        public void IsExtensionInstalled_ShouldReturnTrue_WhenExtractedFolderExists()
        {
            // ARRANGE
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger<FirefoxBrowser>>();
            var mockFileSystem = new Mock<IWindowsFileSystemService>();

            string fakeRoamingExtensionsDir = @"C:\Roaming\Mozilla\Firefox\Profiles\abc.default\extensions";

            // Grundlegende Pfade setzen
            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("AppData")).Returns(@"C:\Roaming");
            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("LocalAppData")).Returns(@"C:\Local");

            mockFileSystem
                .Setup(fs => fs.CombinePaths(It.IsAny<string[]>()))
                .Returns<string[]>(paths => string.Join(@"\", paths));

            // Profil simulieren
            mockFileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(fs => fs.ReadAllLines(It.IsAny<string>())).Returns(new[] { "[Profile0]", "Path=Profiles/abc.default" });
            mockFileSystem.Setup(fs => fs.DirectoryExists(fakeRoamingExtensionsDir)).Returns(true);

            // Die kritischen Mocks für diesen Test:
            // 1. Die .xpi Datei existiert NICHT!
            string fullXpiPath = $@"{fakeRoamingExtensionsDir}\{{fef425dc-a60f-4484-954d-71ecf2544846}}.xpi";
            mockFileSystem.Setup(fs => fs.FileExists(fullXpiPath)).Returns(false);

            // 2. ABER der entpackte Ordner existiert! (Die Logik schneidet das .xpi beim Suchen ab)
            string extractedFolderPath = $@"{fakeRoamingExtensionsDir}\{{fef425dc-a60f-4484-954d-71ecf2544846}}";
            mockFileSystem.Setup(fs => fs.DirectoryExists(extractedFolderPath)).Returns(true);

            mockOs.Setup(os => os.WindowsFileSystemService).Returns(mockFileSystem.Object);
            var firefoxBrowser = new FirefoxBrowser(mockOs.Object, mockLogger.Object);
            // ACT
            bool isInstalled = firefoxBrowser.IsExtensionInstalled();
            // ASSERT
            isInstalled.ShouldBeTrue();
        }
    }
}
