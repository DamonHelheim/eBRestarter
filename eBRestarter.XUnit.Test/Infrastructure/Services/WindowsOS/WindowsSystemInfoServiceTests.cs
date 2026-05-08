using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Infrastructure.Services.WindowsOS;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Testet den WindowsSystemInfoService.
    /// Dank des Registry-Wrappers können wir alle Systeminformationen (OS-Version, Standardbrowser)
    /// vollständig simulieren und auch Fehlerfälle (z.B. fehlende Keys) sicher testen.
    /// </summary>
    public class WindowsSystemInfoServiceTests
    {
        private readonly Mock<ILogger<WindowsSystemInfoService>> _mockLogger;
        private readonly Mock<IWindowsRegistryService> _mockRegistry;
        private readonly WindowsSystemInfoService _sut;

        // Die exakten Pfade aus der Originalklasse
        private const string RegistryPathCurrentVersion = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        private const string RegistryPathUserChoiceHttp = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
        private const string RegistryPathUserChoiceHttps = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice";

        public WindowsSystemInfoServiceTests()
        {
            _mockLogger = new Mock<ILogger<WindowsSystemInfoService>>();
            _mockRegistry = new Mock<IWindowsRegistryService>();

            _sut = new WindowsSystemInfoService(_mockLogger.Object, _mockRegistry.Object);
        }
        // 1. OS DISPLAY VERSION TESTS

        /// <summary>
        /// Wenn der Registry-Key existiert, muss die Display-Version (z.B. "22H2" oder "21H1")
        /// exakt als String zurückgegeben werden.
        /// </summary>
        [Fact]
        public void GetCurrentOsDisplayVersion_ShouldReturnVersion_WhenRegistryKeyExists()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetLocalMachineValue(RegistryPathCurrentVersion, "DisplayVersion"))
                         .Returns("22H2");

            // ACT
            var result = _sut.GetCurrentOsDisplayVersion();

            // ASSERT
            result.ShouldBe("22H2");
        }

        /// <summary>
        /// Auf manchen (älteren) Systemen existiert der Key "DisplayVersion" vielleicht nicht.
        /// In diesem Fall darf es nicht knallen, sondern es muss "Unknown" zurückkommen.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetCurrentOsDisplayVersion_ShouldReturnUnknown_WhenKeyIsMissingOrEmpty(object? invalidValue)
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetLocalMachineValue(RegistryPathCurrentVersion, "DisplayVersion"))
                         .Returns(invalidValue);

            // ACT
            var result = _sut.GetCurrentOsDisplayVersion();

            // ASSERT
            result.ShouldBe("Unknown");
        }

        /// <summary>
        /// Wenn die Registry eine Exception wirft (z.B. Rechteproblem), muss die Methode
        /// den Fehler loggen und als Fallback "Error" zurückgeben.
        /// </summary>
        [Fact]
        public void GetCurrentOsDisplayVersion_ShouldReturnError_OnException()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetLocalMachineValue(It.IsAny<string>(), It.IsAny<string>()))
                         .Throws(new UnauthorizedAccessException("No rights"));

            // ACT
            var result = _sut.GetCurrentOsDisplayVersion();

            // ASSERT
            result.ShouldBe("Error");
            // Wir prüfen nicht explizit den Logger mit Verify, da das Rückgabeergebnis bereits beweist,
            // dass der catch-Block erfolgreich betreten wurde.
        }
        // 2. OS BUILD VERSION TESTS

        /// <summary>
        /// Die Methode setzt den Build-String aus zwei Teilen zusammen:
        /// Dem statischen Environment-Build und der Update Build Revision (UBR) aus der Registry.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren eine UBR von "1234" und prüfen, ob sie korrekt mit dem
        /// echten Environment.OSVersion.Version.Build String verkettet wird.
        /// </summary>
        [Fact]
        public void GetCurrentOsBuildVersion_ShouldCombineEnvironmentBuild_WithUbrFromRegistry()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetLocalMachineValue(RegistryPathCurrentVersion, "UBR"))
                         .Returns(1234); // UBR wird oft als DWord (int) gespeichert

            // Da Environment.OSVersion statisch ist, lesen wir es für den Test dynamisch aus
            string expectedBuild = $"{Environment.OSVersion.Version.Build}.1234";

            // ACT
            var result = _sut.GetCurrentOsBuildVersion();

            // ASSERT
            result.ShouldBe(expectedBuild);
        }

        /// <summary>
        /// Fehlt der UBR-Wert in der Registry, lautet der Fallback-Wert ".0".
        /// </summary>
        [Fact]
        public void GetCurrentOsBuildVersion_ShouldDefaultToZero_WhenUbrIsMissing()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetLocalMachineValue(RegistryPathCurrentVersion, "UBR"))
                         .Returns(null);

            string expectedBuild = $"{Environment.OSVersion.Version.Build}.0";

            // ACT
            var result = _sut.GetCurrentOsBuildVersion();

            // ASSERT
            result.ShouldBe(expectedBuild);
        }

        [Fact]
        public void GetCurrentOsBuildVersion_ShouldReturnError_OnException()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetLocalMachineValue(It.IsAny<string>(), It.IsAny<string>()))
                         .Throws(new Exception("Registry defekt"));

            // ACT
            var result = _sut.GetCurrentOsBuildVersion();

            // ASSERT
            result.ShouldBe("Error");
        }
        // 3. STANDARD BROWSER TESTS

        /// <summary>
        /// Die ProgId aus der Registry muss in lesbare Browsernamen übersetzt werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir testen alle bekannten ProgIds durch InlineData und prüfen das Mapping.
        /// </summary>
        [Theory]
        [InlineData("ChromeHTML", "Chrome")]
        [InlineData("FirefoxURL-308046B0AF4A39CB", "Firefox")] // Firefox hat oft Hash-Werte hinten dran
        [InlineData("MSEdgeHTM", "Edge")]
        [InlineData("OperaStable", "Opera")]
        [InlineData("BraveHTML", "Brave")]
        [InlineData("UnbekannterBrowserXYZ", "-")] // Unbekannte ProgId ergibt "-"
        public void GetCurrentStandardBrowserName_ShouldMapProgIdCorrectly(string progId, string expectedBrowserName)
        {
            // ARRANGE
            // Simulieren: HTTP und HTTPS nutzen denselben Browser (Standardfall)
            _mockRegistry.Setup(r => r.GetCurrentUserValue(RegistryPathUserChoiceHttp, "ProgId")).Returns(progId);
            _mockRegistry.Setup(r => r.GetCurrentUserValue(RegistryPathUserChoiceHttps, "ProgId")).Returns(progId);

            // ACT
            var result = _sut.GetCurrentStandardBrowserName();

            // ASSERT
            result.ShouldBe(expectedBrowserName);
        }

        /// <summary>
        /// Wenn jemand manuell herumgepfuscht hat und HTTP/HTTPS verschiedene Browser haben,
        /// loggt der Service das, gewinnt aber die Information primär aus HTTP.
        /// </summary>
        [Fact]
        public void GetCurrentStandardBrowserName_ShouldHandleMismatch_BetweenHttpAndHttps()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetCurrentUserValue(RegistryPathUserChoiceHttp, "ProgId")).Returns("ChromeHTML");
            _mockRegistry.Setup(r => r.GetCurrentUserValue(RegistryPathUserChoiceHttps, "ProgId")).Returns("FirefoxURL");

            // ACT
            var result = _sut.GetCurrentStandardBrowserName();

            // ASSERT
            // HTTP gewinnt in deiner aktuellen Implementierung (weil progIdHttp zuerst in die Mapping-Methode geht)
            result.ShouldBe("Chrome");
        }

        /// <summary>
        /// Wenn gar kein Standard-Browser gesetzt ist (Wert = null oder leer),
        /// muss die Methode sofort mit "-" abbrechen.
        /// </summary>
        [Theory]
        [InlineData(null, "ChromeHTML")] // HTTP fehlt
        [InlineData("ChromeHTML", "")]   // HTTPS fehlt
        [InlineData(null, null)]         // Beide fehlen
        public void GetCurrentStandardBrowserName_ShouldReturnDash_WhenAnyProgIdIsMissing(string? httpProgId, string? httpsProgId)
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetCurrentUserValue(RegistryPathUserChoiceHttp, "ProgId")).Returns(httpProgId);
            _mockRegistry.Setup(r => r.GetCurrentUserValue(RegistryPathUserChoiceHttps, "ProgId")).Returns(httpsProgId);

            // ACT
            var result = _sut.GetCurrentStandardBrowserName();

            // ASSERT
            result.ShouldBe("-");
        }

        /// <summary>
        /// Der private Helper "GetRegistryValueAsString" fängt Exceptions ab.
        /// Wenn die Registry crasht, muss null geliefert werden, was in "-" resultiert.
        /// </summary>
        [Fact]
        public void GetCurrentStandardBrowserName_ShouldReturnDash_OnException()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetCurrentUserValue(It.IsAny<string>(), It.IsAny<string>()))
                         .Throws(new UnauthorizedAccessException("No rights for HKCU"));

            // ACT
            var result = _sut.GetCurrentStandardBrowserName();

            // ASSERT
            result.ShouldBe("-");
        }
    }
}