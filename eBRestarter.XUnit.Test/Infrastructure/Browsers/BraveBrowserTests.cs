using eBRestarter.Infrastructure.Adapters.WindowsOS;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Wrapper.Browsers;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    // BRAVE BROWSER TESTS
    public class BraveBrowserTests
    {
        /// <summary>
        /// Auch wenn Brave fast identisch zu Chrome ist, hat es einen etwas exotischeren
        /// Root-Pfad: AppData\Local\BraveSoftware\Brave-Browser\User Data.
        /// Ein simpler Vertipper im Code w�rde dazu f�hren, dass der Cache nie gel�scht wird.
        ///
        /// WAS WIRD GETESTET?
        /// Wir pr�fen, ob die Wurzel f�r die Cache- und Cookie-Verzeichnisse exakt den
        /// speziellen Brave-Ordnerbaum nutzt.
        /// </summary>
        [Fact]
        public void GetPaths_ShouldGenerateCorrectDirectories_ForBrave()
        {
            // ARRANGE
            var mockProcess = new Mock<IOutboundPortOsProcessControl>();
            var mockSettings = new Mock<IOutboundPortSystemConfigurationRepository>();
            var mockFileSystem = new Mock<IOutboundPortFileSystem>();
            var mockLogger = new Mock<ILogger<AdapterBraveBrowserWrapper>>();
            

            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("LocalAppData")).Returns(@"C:\Local");
            mockFileSystem.Setup(fs => fs.CombinePaths(It.IsAny<string[]>())).Returns<string[]>(paths => string.Join(@"\", paths));

            // Dies ist der entscheidende Pfad f�r Brave!
            string expectedDefaultProfilePath = @"C:\Local\BraveSoftware\Brave-Browser\User Data\Default";
            mockFileSystem.Setup(fs => fs.DirectoryExists(expectedDefaultProfilePath)).Returns(true);

            var BraveBrowser = new AdapterBraveBrowserWrapper(mockProcess.Object, mockSettings.Object, mockFileSystem.Object, mockLogger.Object);

            // ACT
            var paths = BraveBrowser.ResolvePaths();

            // ASSERT
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\Cache\Cache_Data");
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\Local Storage");
        }
    }
}





