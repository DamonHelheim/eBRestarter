using eBRestarter.Infrastructure.Adapters.WindowsOS;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System.Collections.Generic;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

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
        /// Chromium-Browser suchen Extensions in spezifischen Ordnern. Die Methode muss
        /// erkennen, wenn der Ordner f�r eine bestimmte Extension-ID auf der Festplatte existiert.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren, dass ResolvePaths() einen Extension-Ordner zur�ckgibt.
        /// Wir bringen dem FileSystem-Mock bei, dass dieser Ordner inkl. der Extension-ID
        /// physisch auf der Platte liegt. Die Methode MUSS dann 'true' zur�ckgeben.
        /// </summary>
        [Fact]
        public void IsExtensionInstalled_ShouldReturnTrue_WhenExtensionFolderExists()
        {
            // ARRANGE
            var mockProcess = new Mock<IOutboundPortOsProcessControl>();
            var mockSettings = new Mock<IOutboundPortSystemConfigurationRepository>();
            var mockFileSystem = new Mock<IOutboundPortFileSystem>();
            var mockLogger = new Mock<ILogger<AdapterChromeBrowserWrapper>>();
            

            // 1. LocalAppData vorgeben
            string localAppData = @"C:\Users\Test\AppData\Local";
            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("LocalAppData")).Returns(localAppData);

            // 2. WICHTIG: Einen universellen Mock f�r CombinePaths bauen,
            // der beliebig viele Strings mit "\" zusammenklebt.
            mockFileSystem
                .Setup(fs => fs.CombinePaths(It.IsAny<string[]>()))
                .Returns<string[]>(paths => string.Join(@"\", paths));

            // 3. Damit ChromeBrowser.ResolvePaths() �berhaupt Pfade zur�ckgibt,
            // m�ssen wir vort�uschen, dass das "Default"-Profil existiert!
            string defaultProfilePath = $@"{localAppData}\Google\Chrome\User Data\Default";
            mockFileSystem.Setup(fs => fs.DirectoryExists(defaultProfilePath)).Returns(true);

            // 4. Den Pfad der Extension definieren (wie er von ResolvePaths() generiert wird)
            string chromeExtensionId = "agchmcconfdfcenopioeilpgjngelefk"; // ID aus ChromeBrowser.cs
            string expectedExtensionDir = $@"{defaultProfilePath}\Extensions\{chromeExtensionId}";

            // 5. Dem Dateisystem sagen, dass DIESER Extension-Ordner physisch existiert
            mockFileSystem.Setup(fs => fs.DirectoryExists(expectedExtensionDir)).Returns(true);

            var ChromeBrowser = new AdapterChromeBrowserWrapper(mockProcess.Object, mockSettings.Object, mockFileSystem.Object, mockLogger.Object);

            // ACT
            // ChromeBrowser holt erst ResolvePaths() -> Findet das "Default" Profil -> Generiert den Extension-Pfad.
            // ChromiumBrowserBase iteriert dann dar�ber und pr�ft, ob der Pfad existiert -> JA!
            bool result = ChromeBrowser.IsExtensionInstalled();

            // ASSERT
            result.ShouldBeTrue();
        }

        /// <summary>
        /// Wenn der Nutzer die Extension gel�scht hat oder der Profil-Ordner leer ist,
        /// darf die Methode auf keinen Fall versehentlich 'true' zur�ckgeben.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren, dass der Ordner mit der Extension-ID NICHT existiert (Returns false).
        /// </summary>
        [Fact]
        public void IsExtensionInstalled_ShouldReturnFalse_WhenExtensionFolderIsMissing()
        {
            // ARRANGE
            var mockProcess = new Mock<IOutboundPortOsProcessControl>();
            var mockSettings = new Mock<IOutboundPortSystemConfigurationRepository>();
            var mockFileSystem = new Mock<IOutboundPortFileSystem>();
            var mockLogger = new Mock<ILogger<AdapterChromeBrowserWrapper>>();
            

            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath(It.IsAny<string>())).Returns(@"C:\FakeAppData");

            // WICHTIG: Die Methode fragt ab, ob der kombinierte Pfad existiert. Wir sagen NEIN.
            mockFileSystem.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

            var ChromeBrowser = new AdapterChromeBrowserWrapper(mockProcess.Object, mockSettings.Object, mockFileSystem.Object, mockLogger.Object);

            // ACT
            bool result = ChromeBrowser.IsExtensionInstalled();

            // ASSERT
            result.ShouldBeFalse();
        }
    }
}






