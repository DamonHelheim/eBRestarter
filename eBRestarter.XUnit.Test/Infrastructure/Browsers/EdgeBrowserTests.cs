using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Infrastructure.Browsers;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    // EDGE BROWSER TESTS
    public class EdgeBrowserTests
    {
        /// <summary>
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
            var mockLogger = new Mock<ILogger<EdgeBrowserAdapter>>();
            var mockFileSystem = new Mock<IWindowsFileSystemService>();

            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("LocalAppData")).Returns(@"C:\Local");
            mockFileSystem.Setup(fs => fs.CombinePaths(It.IsAny<string[]>())).Returns<string[]>(paths => string.Join(@"\", paths));

            string expectedDefaultProfilePath = @"C:\Local\Microsoft\Edge\User Data\Default";
            mockFileSystem.Setup(fs => fs.DirectoryExists(expectedDefaultProfilePath)).Returns(true);

            mockOs.Setup(os => os.WindowsFileSystemServiceAdapter).Returns(mockFileSystem.Object);
            var EdgeBrowserAdapter = new EdgeBrowserAdapter(mockOs.Object, mockLogger.Object);

            // ACT
            var paths = EdgeBrowserAdapter.ResolvePaths();

            // ASSERT
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\GPUCache");
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\Network");
        }
    }
}


