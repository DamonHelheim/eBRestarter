using eBRestarter.Infrastructure.Adapters.WindowsOS;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.Outbound.Browsers;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    // EDGE BROWSER TESTS
    public class EdgeBrowserTests
    {
        /// <summary>
        /// Bei Edge passiert oft der Fehler, dass man "Microsoft Edge" oder nur "Edge" als
        /// Verzeichnisnamen w�hlt. Der echte Ordnerbaum ist aber "Microsoft\Edge\User Data".
        ///
        /// WAS WIRD GETESTET?
        /// Identisch zu den anderen: Wir pr�fen das korrekte String-Mapping des Edge-Root-Ordners.
        /// </summary>
        [Fact]
        public void GetPaths_ShouldGenerateCorrectDirectories_ForEdge()
        {
            // ARRANGE
            var mockProcess = new Mock<IOutboundPortOsProcessControl>();
            var mockSettings = new Mock<IOutboundPortSystemConfigurationRepository>();
            var mockFileSystem = new Mock<IOutboundPortFileSystem>();
            var mockLogger = new Mock<ILogger<AdapterEdgeBrowser>>();
            

            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("LocalAppData")).Returns(@"C:\Local");
            mockFileSystem.Setup(fs => fs.CombinePaths(It.IsAny<string[]>())).Returns<string[]>(paths => string.Join(@"\", paths));

            string expectedDefaultProfilePath = @"C:\Local\Microsoft\Edge\User Data\Default";
            mockFileSystem.Setup(fs => fs.DirectoryExists(expectedDefaultProfilePath)).Returns(true);

            var EdgeBrowser = new AdapterEdgeBrowser(mockProcess.Object, mockSettings.Object, mockFileSystem.Object, mockLogger.Object);

            // ACT
            var paths = EdgeBrowser.ResolvePaths();

            // ASSERT
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\GPUCache");
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\Network");
        }
    }
}





