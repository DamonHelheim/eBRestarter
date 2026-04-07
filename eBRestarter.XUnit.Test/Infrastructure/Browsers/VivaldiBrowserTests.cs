using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Infrastructure.Browsers;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    // =========================================================
    // VIVALDI BROWSER TESTS
    // =========================================================
    public class VivaldiBrowserTests
    {
        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Vivaldi ist speziell: Es überschreibt die 'BrowserVersion' Property und sucht
        /// stattdessen in der Windows-Registry unter den Uninstall-Keys nach 'DisplayVersion'.
        /// Wir müssen sicherstellen, dass diese Kaskade (erst HKCU, dann HKLM, dann WOW64) funktioniert.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren, dass der Uninstall-Key im CurrentUser (HKCU) leer ist,
        /// aber im LocalMachine (HKLM) die Version "5.3.2679.55 (Stable channel)" steht.
        /// Der Test prüft, ob Vivaldi das findet und via CleanVersionString() korrekt bereinigt!
        /// </summary>
        [Fact]
        public void BrowserVersion_ShouldReturnVersionFromUninstallKey_AndCleanIt()
        {
            // ARRANGE
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger<VivaldiBrowser>>();
            var mockRegistry = new Mock<IWindowsRegistryService>();

            // 1. Simulation: In CurrentUser steht nichts (Rückgabe null)
            mockRegistry
                .Setup(reg => reg.GetCurrentUserValue(It.IsAny<string>(), "DisplayVersion"))
                .Returns(null);

            // 2. Simulation: In LocalMachine finden wir den String!
            mockRegistry
                .Setup(reg => reg.GetLocalMachineValue(It.IsAny<string>(), "DisplayVersion"))
                .Returns("5.3.2679.55 (Stable channel)"); // Typischer dreckiger Versionsstring

            mockOs.Setup(os => os.WindowsRegistryService).Returns(mockRegistry.Object);

            var vivaldiBrowser = new VivaldiBrowser(mockOs.Object, mockLogger.Object);

            // ACT
            string version = vivaldiBrowser.BrowserVersion;

            // ASSERT
            // Sollte gefunden und durch die Regex aus der Basisklasse bereinigt worden sein!
            version.ShouldBe("5.3.2679.55");
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wir prüfen das korrekte Pfad-Mapping für Vivaldi. Vivaldi speichert seine
        /// Daten unter AppData\Local\Vivaldi\User Data.
        /// </summary>
        [Fact]
        public void GetPaths_ShouldGenerateCorrectDirectories_ForVivaldi()
        {
            // ARRANGE
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger<VivaldiBrowser>>();
            var mockFileSystem = new Mock<IWindowsFileSystemService>();

            mockFileSystem.Setup(fs => fs.GetEnvironmentPath("LocalAppData")).Returns(@"C:\Local");
            mockFileSystem.Setup(fs => fs.CombinePaths(It.IsAny<string[]>())).Returns<string[]>(paths => string.Join(@"\", paths));

            string expectedDefaultProfilePath = @"C:\Local\Vivaldi\User Data\Default";
            mockFileSystem.Setup(fs => fs.DirectoryExists(expectedDefaultProfilePath)).Returns(true);

            mockOs.Setup(os => os.WindowsFileSystemService).Returns(mockFileSystem.Object);
            var vivaldiBrowser = new VivaldiBrowser(mockOs.Object, mockLogger.Object);

            // ACT
            var paths = vivaldiBrowser.GetPaths();

            // ASSERT
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\Service Worker");
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\IndexedDB");
        }
    }
}