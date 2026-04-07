using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Infrastructure.Browsers;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    /// <summary>
    /// Testet die Chromium-spezifische Logik (Extensions & Pfade).
    /// Wir nutzen den ChromeBrowser als "Vehikel", um die Logik der abstrakten
    /// ChromiumBrowserBase-Klasse zu testen.
    /// </summary>
    public class ChromiumBrowserBaseTests
    {
        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Chromium-Browser suchen Extensions in spezifischen Ordnern. Die Methode muss
        /// erkennen, wenn der Ordner für eine bestimmte Extension-ID auf der Festplatte existiert.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren, dass GetPaths() einen Extension-Ordner zurückgibt.
        /// Wir bringen dem FileSystem-Mock bei, dass dieser Ordner inkl. der Extension-ID
        /// physisch auf der Platte liegt. Die Methode MUSS dann 'true' zurückgeben.
        /// </summary>
        [Fact]
        public void IsExtensionInstalled_ShouldReturnTrue_WhenExtensionFolderExists()
        {
            // ARRANGE
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger<ChromeBrowser>>();
            var mockFileSystem = new Mock<IWindowsFileSystemService>();

            // 1. LocalAppData vorgeben
            string localAppData = @"C:\Users\Test\AppData\Local";
            mockFileSystem.Setup(fs => fs.GetEnvironmentPath("LocalAppData")).Returns(localAppData);

            // 2. WICHTIG: Einen universellen Mock für CombinePaths bauen,
            // der beliebig viele Strings mit "\" zusammenklebt.
            mockFileSystem
                .Setup(fs => fs.CombinePaths(It.IsAny<string[]>()))
                .Returns<string[]>(paths => string.Join(@"\", paths));

            // 3. Damit ChromeBrowser.GetPaths() überhaupt Pfade zurückgibt,
            // müssen wir vortäuschen, dass das "Default"-Profil existiert!
            string defaultProfilePath = $@"{localAppData}\Google\Chrome\User Data\Default";
            mockFileSystem.Setup(fs => fs.DirectoryExists(defaultProfilePath)).Returns(true);

            // 4. Den Pfad der Extension definieren (wie er von GetPaths() generiert wird)
            string chromeExtensionId = "agchmcconfdfcenopioeilpgjngelefk"; // ID aus ChromeBrowser.cs
            string expectedExtensionDir = $@"{defaultProfilePath}\Extensions\{chromeExtensionId}";

            // 5. Dem Dateisystem sagen, dass DIESER Extension-Ordner physisch existiert
            mockFileSystem.Setup(fs => fs.DirectoryExists(expectedExtensionDir)).Returns(true);

            mockOs.Setup(os => os.WindowsFileSystemService).Returns(mockFileSystem.Object);

            var chromeBrowser = new ChromeBrowser(mockOs.Object, mockLogger.Object);

            // ACT
            // ChromeBrowser holt erst GetPaths() -> Findet das "Default" Profil -> Generiert den Extension-Pfad.
            // ChromiumBrowserBase iteriert dann darüber und prüft, ob der Pfad existiert -> JA!
            bool result = chromeBrowser.IsExtensionInstalled();

            // ASSERT
            result.ShouldBeTrue();
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn der Nutzer die Extension gelöscht hat oder der Profil-Ordner leer ist,
        /// darf die Methode auf keinen Fall versehentlich 'true' zurückgeben.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren, dass der Ordner mit der Extension-ID NICHT existiert (Returns false).
        /// </summary>
        [Fact]
        public void IsExtensionInstalled_ShouldReturnFalse_WhenExtensionFolderIsMissing()
        {
            // ARRANGE
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger<ChromeBrowser>>();
            var mockFileSystem = new Mock<IWindowsFileSystemService>();

            mockFileSystem.Setup(fs => fs.GetEnvironmentPath(It.IsAny<string>())).Returns(@"C:\FakeAppData");

            // WICHTIG: Die Methode fragt ab, ob der kombinierte Pfad existiert. Wir sagen NEIN.
            mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

            mockOs.Setup(os => os.WindowsFileSystemService).Returns(mockFileSystem.Object);

            var chromeBrowser = new ChromeBrowser(mockOs.Object, mockLogger.Object);

            // ACT
            bool result = chromeBrowser.IsExtensionInstalled();

            // ASSERT
            result.ShouldBeFalse();
        }
    }
}