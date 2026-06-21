using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Infrastructure.Browsers;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Browsers
{
    // BRAVE BROWSER TESTS
    public class BraveBrowserTests
    {
        /// <summary>
        /// Auch wenn Brave fast identisch zu Chrome ist, hat es einen etwas exotischeren
        /// Root-Pfad: AppData\Local\BraveSoftware\Brave-Browser\User Data.
        /// Ein simpler Vertipper im Code würde dazu führen, dass der Cache nie gelöscht wird.
        ///
        /// WAS WIRD GETESTET?
        /// Wir prüfen, ob die Wurzel für die Cache- und Cookie-Verzeichnisse exakt den
        /// speziellen Brave-Ordnerbaum nutzt.
        /// </summary>
        [Fact]
        public void GetPaths_ShouldGenerateCorrectDirectories_ForBrave()
        {
            // ARRANGE
            var mockOs = new Mock<IOperatingSystemFacade>();
            var mockLogger = new Mock<ILogger<BraveBrowserAdapter>>();
            var mockFileSystem = new Mock<IWindowsFileSystemService>();

            mockFileSystem.Setup(fs => fs.ResolveEnvironmentPath("LocalAppData")).Returns(@"C:\Local");
            mockFileSystem.Setup(fs => fs.CombinePaths(It.IsAny<string[]>())).Returns<string[]>(paths => string.Join(@"\", paths));

            // Dies ist der entscheidende Pfad für Brave!
            string expectedDefaultProfilePath = @"C:\Local\BraveSoftware\Brave-Browser\User Data\Default";
            mockFileSystem.Setup(fs => fs.DirectoryExists(expectedDefaultProfilePath)).Returns(true);

            mockOs.Setup(os => os.WindowsFileSystemServiceAdapter).Returns(mockFileSystem.Object);
            var BraveBrowserAdapter = new BraveBrowserAdapter(mockOs.Object, mockLogger.Object);

            // ACT
            var paths = BraveBrowserAdapter.ResolvePaths();

            // ASSERT
            paths.CacheDirs.ShouldContain($@"{expectedDefaultProfilePath}\Cache\Cache_Data");
            paths.CookiesDirs.ShouldContain($@"{expectedDefaultProfilePath}\Local Storage");
        }
    }
}


