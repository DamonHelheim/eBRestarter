using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Infrastructure.Browsers;
using eBRestarter.Infrastructure.Factories;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Factories
{
    /// <summary>
    /// Testet die BrowserFactory, welche fÃ¼r die Instanziierung der korrekten Browser-Klassen
    /// Ã¼ber den Dependency Injection Container (IServiceProvider) zustÃ¤ndig ist.
    /// </summary>
    public class BrowserFactoryTests
    {
        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Die Factory nutzt ein 'switch'-Statement, um das Enum auf konkrete Klassen zu mappen.
        /// Ein simpler Kopierfehler im Code (z.B. Edge => gibt Chrome zurÃ¼ck) wÃ¼rde das Programm crashen.
        ///
        /// WAS WIRD GETESTET?
        /// Wir prÃ¼fen fÃ¼r JEDEN gÃ¼ltigen BrowserType, ob die Factory den ServiceProvider nach dem
        /// exakt korrekten Typ fragt und diesen zurÃ¼ckgibt. Wir nutzen [Theory] und [InlineData],
        /// um alle Browser-Arten in einem einzigen Test-Durchlauf zu verifizieren.
        /// </summary>
        [Theory]
        [InlineData(BrowserType.Chrome, typeof(ChromeBrowser))]
        [InlineData(BrowserType.Firefox, typeof(FirefoxBrowser))]
        [InlineData(BrowserType.Edge, typeof(EdgeBrowser))]
        [InlineData(BrowserType.Brave, typeof(BraveBrowser))]
        [InlineData(BrowserType.Vivaldi, typeof(VivaldiBrowser))]
        public void Create_ShouldReturnCorrectBrowserInstance_ForValidBrowserType(BrowserType inputType, Type expectedClassType)
        {
            // ==========================================
            // ARRANGE (Vorbereitung)
            // ==========================================
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Um die konkreten Browser zu erstellen, brauchen wir Dummy-Mocks fÃ¼r deren Konstruktoren
            var mockOs = new Mock<IOperatingSystemFacade>();

            // Wir definieren fÃ¼r jeden Browser-Typ eine Dummy-Instanz.
            // Der ServiceProvider soll diese zurÃ¼ckgeben, wenn er danach gefragt wird.
            var fakeChrome = new ChromeBrowser(mockOs.Object, new Mock<ILogger<ChromeBrowser>>().Object);
            var fakeFirefox = new FirefoxBrowser(mockOs.Object, new Mock<ILogger<FirefoxBrowser>>().Object);
            var fakeEdge = new EdgeBrowser(mockOs.Object, new Mock<ILogger<EdgeBrowser>>().Object);
            var fakeBrave = new BraveBrowser(mockOs.Object, new Mock<ILogger<BraveBrowser>>().Object);
            var fakeVivaldi = new VivaldiBrowser(mockOs.Object, new Mock<ILogger<VivaldiBrowser>>().Object);

            // WICHTIGER MOCKING-TRICK:
            // GetRequiredService<T>() ruft intern GetService(typeof(T)) auf!
            // Hier bringen wir dem Mock-ServiceProvider bei, auf Typ-Anfragen korrekt zu antworten.
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ChromeBrowser))).Returns(fakeChrome);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(FirefoxBrowser))).Returns(fakeFirefox);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(EdgeBrowser))).Returns(fakeEdge);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(BraveBrowser))).Returns(fakeBrave);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(VivaldiBrowser))).Returns(fakeVivaldi);

            var factory = new BrowserFactory(mockServiceProvider.Object);

            // ==========================================
            // ACT (AusfÃ¼hrung)
            // ==========================================
            var result = factory.Create(inputType);

            // ==========================================
            // ASSERT (PrÃ¼fung)
            // ==========================================
            // 1. PrÃ¼fen wir, ob Ã¼berhaupt etwas zurÃ¼ckkam
            result.ShouldNotBeNull();

            // 2. PrÃ¼fen wir, ob das zurÃ¼ckgegebene Objekt vom ERWARTETEN Typ ist.
            // z.B. wenn inputType = BrowserType.Edge ist, muss das Ergebnis vom Typ 'EdgeBrowser' sein.
            result.ShouldBeOfType(expectedClassType);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn in der Zukunft jemand im 'BrowserType' Enum einen neuen Browser (z.B. Opera) hinzufÃ¼gt,
        /// aber vergisst, die Factory anzupassen, soll das Programm kontrolliert mit einer
        /// klaren NotSupportedException abbrechen, anstatt seltsame Fehler zu werfen.
        ///
        /// WAS WIRD GETESTET?
        /// Wir werfen einen absichtlich ungÃ¼ltigen Enum-Wert in die Factory und nutzen die
        /// Shouldly-Methode 'ShouldThrow', um zu garantieren, dass exakt diese Exception fliegt.
        /// </summary>
        [Fact]
        public void Create_ShouldThrowNotSupportedException_ForInvalidBrowserType()
        {
            // ==========================================
            // ARRANGE
            // ==========================================
            var mockServiceProvider = new Mock<IServiceProvider>();
            var factory = new BrowserFactory(mockServiceProvider.Object);

            // Wir "erfinden" einen ungÃ¼ltigen BrowserType, indem wir eine Zahl casten,
            // die gar nicht im Enum definiert ist.
            var invalidBrowserType = (BrowserType)999;

            // ==========================================
            // ACT & ASSERT
            // ==========================================
            // Shouldly fÃ¤ngt die Exception und prÃ¼ft ihren Typ und (optional) die Nachricht
            var exception = Should.Throw<NotSupportedException>(() =>
            {
                factory.Create(invalidBrowserType);
            });

            exception.Message.ShouldContain("ist noch nicht implementiert");
        }
    }
}