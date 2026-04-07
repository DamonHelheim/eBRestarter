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
    // EDGE BROWSER TESTS
    // =========================================================
    public class EdgeBrowserTests
    {
        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Bei Edge passiert oft der Fehler, dass man "Microsoft Edge" oder nur "Edge" als
        /// Verzeichnisnamen wählt. Der echte Ordnerbaum ist aber "Microsoft\Edge\User Data".
        ///
        /// WAS WIRD GETESTET?
        /// Identisch zu den anderen: Wir prüfen das korrekte String-Mapping des Edge-Root-Ordners.
        /// </summary>
        [Fact]
        public void GetPaths_ShouldGenerateCorrectDirectories_ForEdge()
        {
            // ARRANGE
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger<EdgeBrowser>>();
            var mockFileSystem = new Mock<IWindowsFileSystemService>();

            mockFileSystem.Setup(fs => fs.GetEnvironmentPath("LocalAppData")).Returns(@"C:\Local");
            mockFileSystem.Setup(fs => fs.CombinePaths(It.IsAny<string[]>())).Returns<string[]>(paths => string.Join(@"\", paths));

            string expectedDefaultProfilePath = @"C:\Local\Microsoft\Edge\User Data\Default";
            mockFileSystem.Setup(fs => fs.DirectoryExists(expectedDefaultProfilePath)).Returns(true);

            mockOs.Setup(os => os.WindowsFileSystemService).Returns(mockFileSystem.Object);
            var edgeBrowser = new EdgeBrowser(mockOs.Object, mockLogger.Object);

            // ACT
            var paths = edgeBrowser.GetPaths();

            // ASSERT
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\GPUCache");
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\Network");
        }
    }
}