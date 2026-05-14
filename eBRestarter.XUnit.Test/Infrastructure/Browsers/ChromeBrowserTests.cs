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
    /// Testet die hartkodierten Eigenheiten der konkreten ChromeBrowser Klasse.
    /// </summary>
    public class ChromeBrowserTests
    {
        /// <summary>
        /// eBesucher muss ganz spezifische Ordner leeren (z. B. "Cache_Data", "IndexedDB").
        /// Wir müssen sicherstellen, dass die ResolvePaths() Methode genau diese Ordner für
        /// das Standard-Profil von Chrome generiert.
        ///
        /// WAS WIRD GETESTET?
        /// Wir mocken 'CombinePaths', damit es Pfade berechenbar zusammenklebt.
        /// Dann prüfen wir, ob in der generierten Liste exakt die geforderten Ordner-Endungen stehen.
        /// </summary>
        [Fact]
        public void GetPaths_ShouldGenerateCorrectCacheAndCookieDirectories_ForDefaultProfile()
        {
            // ARRANGE
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger<ChromeBrowser>>();
            var mockFileSystem = new Mock<IWindowsFileSystemService>();

            // 1. LocalAppData vorgeben
            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("LocalAppData")).Returns(@"C:\Users\Test\AppData\Local");

            // 2. Einen dummen, aber vorhersehbaren CombinePaths-Mock bauen (klebt Strings mit \ zusammen)
            mockFileSystem
                .Setup(fs => fs.CombinePaths(It.IsAny<string[]>()))
                .Returns<string[]>(paths => string.Join(@"\", paths));

            // 3. Wir täuschen vor, dass das "Default"-Profilverzeichnis existiert!
            // Sonst bricht ChromeBrowser.cs in Zeile 42 ab.
            string expectedDefaultProfilePath = @"C:\Users\Test\AppData\Local\Google\Chrome\User Data\Default";
            mockFileSystem.Setup(fs => fs.DirectoryExists(expectedDefaultProfilePath)).Returns(true);

            mockOs.Setup(os => os.WindowsFileSystemService).Returns(mockFileSystem.Object);
            var chromeBrowser = new ChromeBrowser(mockOs.Object, mockLogger.Object);

            // ACT
            var paths = chromeBrowser.ResolvePaths();

            // ASSERT
            // Hat er den Cache-Ordner gefunden?
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\Cache\Cache_Data");
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\Service Worker");

            // Hat er die Cookie-Ordner (IndexedDB, Local Storage) gefunden?
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\IndexedDB");
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\Local Storage");
        }
    }
}
