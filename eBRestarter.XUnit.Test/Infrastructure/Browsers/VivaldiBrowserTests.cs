using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.WindowsOS;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Infrastructure.Adapters.Browsers;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    // VIVALDI BROWSER TESTS
    public class VivaldiBrowserTests
    {
        /// <summary>
        /// Vivaldi ist speziell: Es �berschreibt die 'BrowserVersion' Property und sucht
        /// stattdessen in der Windows-Registry unter den Uninstall-Keys nach 'DisplayVersion'.
        /// Wir m�ssen sicherstellen, dass diese Kaskade (erst HKCU, dann HKLM, dann WOW64) funktioniert.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren, dass der Uninstall-Key im CurrentUser (HKCU) leer ist,
        /// aber im LocalMachine (HKLM) die Version "5.3.2679.55 (Stable channel)" steht.
        /// Der Test pr�ft, ob Vivaldi das findet und via CleanVersionString() korrekt bereinigt!
        /// </summary>
        [Fact]
        public void BrowserVersion_ShouldReturnVersionFromUninstallKey_AndCleanIt()
        {
            // ARRANGE
            var mockProcess = new Mock<IOsProcessControlOutboundPort>();
            var mockSettings = new Mock<ISettingsRepositoryOutboundPort>();
            var mockFileSystem = new Mock<IFileSystemOutboundPort>();
            var mockLogger = new Mock<ILogger<VivaldiBrowser>>();
            

            // 1. Simulation: In CurrentUser steht nichts (R�ckgabe null)
            mockSettings.Setup(reg => reg.GetUserValue(It.IsAny<string>(), "DisplayVersion"))
                .Returns(null);

            // 2. Simulation: In LocalMachine finden wir den String!
            mockSettings.Setup(reg => reg.GetSystemValue(It.IsAny<string>(), "DisplayVersion"))
                .Returns("5.3.2679.55 (Stable channel)"); // Typischer dreckiger Versionsstring

            var VivaldiBrowser = new VivaldiBrowser(mockProcess.Object, mockSettings.Object, mockFileSystem.Object, mockLogger.Object);

            // ACT
            string version = VivaldiBrowser.BrowserVersion;

            // ASSERT
            // Sollte gefunden und durch die Regex aus der Basisklasse bereinigt worden sein!
            version.ShouldBe("5.3.2679.55");
        }

        /// <summary>
        /// Wir pr�fen das korrekte Pfad-Mapping f�r Vivaldi. Vivaldi speichert seine
        /// Daten unter AppData\Local\Vivaldi\User Data.
        /// </summary>
        [Fact]
        public void GetPaths_ShouldGenerateCorrectDirectories_ForVivaldi()
        {
            // ARRANGE
            var mockProcess = new Mock<IOsProcessControlOutboundPort>();
            var mockSettings = new Mock<ISettingsRepositoryOutboundPort>();
            
            var mockLogger = new Mock<ILogger<VivaldiBrowser>>();
            var mockFileSystem = new Mock<IFileSystemOutboundPort>();

            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("LocalAppData")).Returns(@"C:\Local");
            mockFileSystem.Setup(fs => fs.CombinePaths(It.IsAny<string[]>())).Returns<string[]>(paths => string.Join(@"\", paths));

            string expectedDefaultProfilePath = @"C:\Local\Vivaldi\User Data\Default";
            mockFileSystem.Setup(fs => fs.DirectoryExists(expectedDefaultProfilePath)).Returns(true);

            var VivaldiBrowser = new VivaldiBrowser(mockProcess.Object, mockSettings.Object, mockFileSystem.Object, mockLogger.Object);

            // ACT
            var paths = VivaldiBrowser.ResolvePaths();

            // ASSERT
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\Service Worker");
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\IndexedDB");
        }
    }
}






