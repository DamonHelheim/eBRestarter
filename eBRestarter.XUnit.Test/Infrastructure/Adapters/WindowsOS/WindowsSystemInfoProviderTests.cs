using eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;
using eBRestarter.Infrastructure.Adapters.WindowsOS;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using eBRestarter.Core.Application.Ports.Outbound.Network;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.Tests.Infrastructure.Adapters.WindowsOS
{
    /// <summary>
    /// Testet den WindowsSystemInfoProvider.
    /// Dank des Registry-Wrappers k�nnen wir alle Systeminformationen (OS-Version, Standardbrowser)
    /// vollst�ndig simulieren und auch Fehlerf�lle (z.B. fehlende Keys) sicher testen.
    /// </summary>
    public class WindowsSystemInfoProviderTests
    {
        private readonly Mock<ILogger<WindowsSystemInfoProvider>> _mockLogger;
        private readonly Mock<ISettingsRepositoryOutboundPort> _mockRegistry;
        private readonly WindowsSystemInfoProvider _sut;

        // Die exakten Pfade aus der Originalklasse
        private const string RegistryPathCurrentVersion = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        private const string RegistryPathUserChoiceHttp = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
        private const string RegistryPathUserChoiceHttps = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice";

        public WindowsSystemInfoProviderTests()
        {
            _mockLogger = new Mock<ILogger<WindowsSystemInfoProvider>>();
            _mockRegistry = new Mock<ISettingsRepositoryOutboundPort>();

            _sut = new WindowsSystemInfoProvider(_mockLogger.Object, _mockRegistry.Object);
        }
        // 1. OS DISPLAY VERSION TESTS

        /// <summary>
        /// Wenn der Registry-Key existiert, muss die Display-Version (z.B. "22H2" oder "21H1")
        /// exakt als String zur�ckgegeben werden.
        /// </summary>
        [Fact]
        public void GetCurrentOsDisplayVersion_ShouldReturnVersion_WhenRegistryKeyExists()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetSystemValue(RegistryPathCurrentVersion, "DisplayVersion"))
                         .Returns("22H2");

            // ACT
            var result = _sut.RetrieveCurrentOsDisplayVersion();

            // ASSERT
            result.ShouldBe("22H2");
        }

        /// <summary>
        /// Auf manchen (�lteren) Systemen existiert der Key "DisplayVersion" vielleicht nicht.
        /// In diesem Fall darf es nicht knallen, sondern es muss "Unknown" zur�ckkommen.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetCurrentOsDisplayVersion_ShouldReturnUnknown_WhenKeyIsMissingOrEmpty(object? invalidValue)
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetSystemValue(RegistryPathCurrentVersion, "DisplayVersion"))
                         .Returns(invalidValue);

            // ACT
            var result = _sut.RetrieveCurrentOsDisplayVersion();

            // ASSERT
            result.ShouldBe("Unknown");
        }

        /// <summary>
        /// Wenn die Registry eine Exception wirft (z.B. Rechteproblem), muss die Methode
        /// den Fehler loggen und als Fallback "Error" zur�ckgeben.
        /// </summary>
        [Fact]
        public void GetCurrentOsDisplayVersion_ShouldReturnError_OnException()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetSystemValue(It.IsAny<string>(), It.IsAny<string>()))
                         .Throws(new UnauthorizedAccessException("No rights"));

            // ACT
            var result = _sut.RetrieveCurrentOsDisplayVersion();

            // ASSERT
            result.ShouldBe("Error");
            // Wir pr�fen nicht explizit den Logger mit Verify, da das R�ckgabeergebnis bereits beweist,
            // dass der catch-Block erfolgreich betreten wurde.
        }
        // 2. OS BUILD VERSION TESTS

        /// <summary>
        /// Die Methode setzt den Build-String aus zwei Teilen zusammen:
        /// Dem statischen Environment-Build und der Update Build Revision (UBR) aus der Registry.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren eine UBR von "1234" und pr�fen, ob sie korrekt mit dem
        /// echten Environment.OSVersion.Version.Build String verkettet wird.
        /// </summary>
        [Fact]
        public void GetCurrentOsBuildVersion_ShouldCombineEnvironmentBuild_WithUbrFromRegistry()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetSystemValue(RegistryPathCurrentVersion, "UBR"))
                         .Returns(1234); // UBR wird oft als DWord (int) gespeichert

            // Da Environment.OSVersion statisch ist, lesen wir es f�r den Test dynamisch aus
            string expectedBuild = $"{Environment.OSVersion.Version.Build}.1234";

            // ACT
            var result = _sut.RetrieveCurrentOsBuildVersion();

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
            _mockRegistry.Setup(r => r.GetSystemValue(RegistryPathCurrentVersion, "UBR"))
                         .Returns(null);

            string expectedBuild = $"{Environment.OSVersion.Version.Build}.0";

            // ACT
            var result = _sut.RetrieveCurrentOsBuildVersion();

            // ASSERT
            result.ShouldBe(expectedBuild);
        }

        [Fact]
        public void GetCurrentOsBuildVersion_ShouldReturnError_OnException()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetSystemValue(It.IsAny<string>(), It.IsAny<string>()))
                         .Throws(new Exception("Registry defekt"));

            // ACT
            var result = _sut.RetrieveCurrentOsBuildVersion();

            // ASSERT
            result.ShouldBe("Error");
        }
        // 3. STANDARD BROWSER TESTS

        /// <summary>
        /// Die ProgId aus der Registry muss in lesbare Browsernamen �bersetzt werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir testen alle bekannten ProgIds durch InlineData und pr�fen das Mapping.
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
            _mockRegistry.Setup(r => r.GetUserValue(RegistryPathUserChoiceHttp, "ProgId")).Returns(progId);
            _mockRegistry.Setup(r => r.GetUserValue(RegistryPathUserChoiceHttps, "ProgId")).Returns(progId);

            // ACT
            var result = _sut.RetrieveCurrentStandardBrowserName();

            // ASSERT
            result.ShouldBe(expectedBrowserName);
        }

        /// <summary>
        /// Wenn jemand manuell herumgepfuscht hat und HTTP/HTTPS verschiedene Browser haben,
        /// loggt der Service das, gewinnt aber die Information prim�r aus HTTP.
        /// </summary>
        [Fact]
        public void GetCurrentStandardBrowserName_ShouldHandleMismatch_BetweenHttpAndHttps()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetUserValue(RegistryPathUserChoiceHttp, "ProgId")).Returns("ChromeHTML");
            _mockRegistry.Setup(r => r.GetUserValue(RegistryPathUserChoiceHttps, "ProgId")).Returns("FirefoxURL");

            // ACT
            var result = _sut.RetrieveCurrentStandardBrowserName();

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
            _mockRegistry.Setup(r => r.GetUserValue(RegistryPathUserChoiceHttp, "ProgId")).Returns(httpProgId);
            _mockRegistry.Setup(r => r.GetUserValue(RegistryPathUserChoiceHttps, "ProgId")).Returns(httpsProgId);

            // ACT
            var result = _sut.RetrieveCurrentStandardBrowserName();

            // ASSERT
            result.ShouldBe("-");
        }

        /// <summary>
        /// Der private Helper "GetRegistryValueAsString" f�ngt Exceptions ab.
        /// Wenn die Registry crasht, muss null geliefert werden, was in "-" resultiert.
        /// </summary>
        [Fact]
        public void GetCurrentStandardBrowserName_ShouldReturnDash_OnException()
        {
            // ARRANGE
            _mockRegistry.Setup(r => r.GetUserValue(It.IsAny<string>(), It.IsAny<string>()))
                         .Throws(new UnauthorizedAccessException("No rights for HKCU"));

            // ACT
            var result = _sut.RetrieveCurrentStandardBrowserName();

            // ASSERT
            result.ShouldBe("-");
        }
    }
}



