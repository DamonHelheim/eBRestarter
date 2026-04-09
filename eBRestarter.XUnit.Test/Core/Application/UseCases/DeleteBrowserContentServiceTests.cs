using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.UseCases.DeleteBrowserContent;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace eBRestarter.Tests.Core.Application.UseCases.DeleteBrowserContent
{
    /// <summary>
    /// Testet den DeleteBrowserContentService.
    /// Der Fokus liegt auf der Einhaltung des Lösch-Workflows: Prozessprüfung,
    /// Pfadermittlung, Fortschrittsmeldung und Fehlerbehandlung.
    /// </summary>
    public class DeleteBrowserContentServiceTests
    {
        private readonly Mock<IBrowserFactory> _mockBrowserFactory;
        private readonly Mock<IFileDeletionService> _mockFileDeletionService;
        private readonly Mock<IWindowsProcessControlService> _mockProcessService;
        private readonly Mock<ILocalizationService> _mockLocalizationService;
        private readonly DeleteBrowserContentService _sut;

        public DeleteBrowserContentServiceTests()
        {
            _mockBrowserFactory = new Mock<IBrowserFactory>();
            _mockFileDeletionService = new Mock<IFileDeletionService>();
            _mockProcessService = new Mock<IWindowsProcessControlService>();
            _mockLocalizationService = new Mock<ILocalizationService>();

            // Hilfs-Setup für die Lokalisierung: Gibt einfach den angefragten Key zurück,
            // damit wir im Test verifizieren können, ob die richtige Meldung angefordert wurde.
            _mockLocalizationService
                .Setup(l => l.GetString(It.IsAny<string>()))
                .Returns((string key) => key);

            _sut = new DeleteBrowserContentService(
                _mockBrowserFactory.Object,
                _mockFileDeletionService.Object,
                _mockProcessService.Object,
                _mockLocalizationService.Object);
        }

        // =========================================================
        // 1. PROZESS- UND KONFLIKT-TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn der Browser noch läuft und "ForceCloseProcess" auf false steht,
        /// darf nicht gelöscht werden. Die Methode muss abbrechen und einen Konflikt melden.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldReturnProcessConflict_WhenBrowserIsStillRunning()
        {
            // ARRANGE
            var request = new DeleteBrowserContentRequest(BrowserType.Chrome, true, true, ForceCloseProcess: false);
            var progress = new Progress<DeleteBrowserContentProgress>();

            var mockBrowser = new Mock<IBrowser>();
            _mockBrowserFactory.Setup(f => f.Create(BrowserType.Chrome)).Returns(mockBrowser.Object);

            // Simulieren: Chrome läuft noch!
            _mockProcessService.Setup(p => p.IsProcessAlive("chrome")).Returns(true);

            // ACT
            var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

            // ASSERT
            result.Success.ShouldBeFalse();
            result.ProcessConflict.ShouldBeTrue();
            result.ErrorMessage.ShouldBe("Cleanup_BrowserRunning");

            // Sicherstellen, dass keine Löschung versucht wurde
            _mockFileDeletionService.Verify(f => f.DeleteFilesAsync(It.IsAny<List<string>>(), It.IsAny<IProgress<string>>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn "ForceCloseProcess" auf true steht, muss der Service versuchen,
        /// den Prozess aktiv über den IWindowsProcessControlService zu beenden.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldForceCloseProcess_WhenRequested()
        {
            // ARRANGE
            var request = new DeleteBrowserContentRequest(BrowserType.Edge, true, true, ForceCloseProcess: true);
            var progressUpdates = new List<DeleteBrowserContentProgress>();
            var progress = new Progress<DeleteBrowserContentProgress>(p => progressUpdates.Add(p));

            var mockBrowser = new Mock<IBrowser>();

            // HIER KORRIGIERT: Nutzung des Konstruktors vom Record (CacheDirs, CookiesDirs, ExtensionsDirs)
            mockBrowser.Setup(b => b.GetPaths()).Returns(new BrowserPaths(
                new List<string> { "C:\\Cache" },
                new List<string>(),
                new List<string>()));

            _mockBrowserFactory.Setup(f => f.Create(BrowserType.Edge)).Returns(mockBrowser.Object);

            // Simulieren: Edge läuft anfangs, wird aber erfolgreich geschlossen (IsProcessAlive liefert danach false)
            _mockProcessService.Setup(p => p.IsProcessAlive("msedge")).Returns(false);

            // ACT
            var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

            // ASSERT
            // Prüfen, ob der Befehl zum Schließen gesendet wurde
            _mockProcessService.Verify(p => p.CloseApplication("msedge"), Times.Once);

            // Prüfen, ob die UI über das Schließen informiert wurde
            progressUpdates.ShouldContain(p => p.StatusMessage == "Cleanup_ClosingBrowser");
        }

        // =========================================================
        // 2. PFAD- UND LÖSCH-LOGIK TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn der Benutzer zwar den Löschvorgang startet, aber weder Cache noch Cookies
        /// löschen möchte (oder der Browser keine Pfade liefert), muss abgebrochen werden.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldReturnNoPathsError_WhenNothingIsSelectedOrAvailable()
        {
            // ARRANGE
            // Weder Cookies noch Cache sollen gelöscht werden
            var request = new DeleteBrowserContentRequest(BrowserType.Firefox, DeleteCookies: false, DeleteCache: false, ForceCloseProcess: false);
            var progress = new Progress<DeleteBrowserContentProgress>();

            var mockBrowser = new Mock<IBrowser>();

            // HIER KORRIGIERT: Konstruktor mit den 3 Listen
            mockBrowser.Setup(b => b.GetPaths()).Returns(new BrowserPaths(
                new List<string> { "C:\\Cache" },
                new List<string> { "C:\\Cookies" },
                new List<string>()));

            _mockBrowserFactory.Setup(f => f.Create(BrowserType.Firefox)).Returns(mockBrowser.Object);

            _mockProcessService.Setup(p => p.IsProcessAlive("firefox")).Returns(false);

            // ACT
            var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

            // ASSERT
            result.Success.ShouldBeFalse();
            result.ProcessConflict.ShouldBeFalse();
            result.ErrorMessage.ShouldBe("Cleanup_NoPaths");
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Dies ist der "Happy Path". Es prüft, ob die Listen aus Cache und Cookies korrekt
        /// zusammengeführt und an den FileDeletionService übergeben werden.
        /// Außerdem wird geprüft, ob die Progress-Reporter korrekt aufgerufen werden.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldCallDeletionService_WithCombinedPaths()
        {
            // ARRANGE
            var request = new DeleteBrowserContentRequest(BrowserType.Brave, DeleteCookies: true, DeleteCache: true, ForceCloseProcess: false);
            var progressUpdates = new List<DeleteBrowserContentProgress>();
            var progress = new Progress<DeleteBrowserContentProgress>(p => progressUpdates.Add(p));

            var mockBrowser = new Mock<IBrowser>();

            // HIER KORRIGIERT: Konstruktor mit den 3 Listen
            mockBrowser.Setup(b => b.GetPaths()).Returns(new BrowserPaths(
                new List<string> { "C:\\Brave\\Cache" },
                new List<string> { "C:\\Brave\\Cookies" },
                new List<string>()));

            _mockBrowserFactory.Setup(f => f.Create(BrowserType.Brave)).Returns(mockBrowser.Object);

            _mockProcessService.Setup(p => p.IsProcessAlive("brave")).Returns(false);

            // Wir simulieren 100 gefundene Dateien
            _mockFileDeletionService.Setup(f => f.CountFilesAsync(It.IsAny<List<string>>())).ReturnsAsync(100);

            // ACT
            var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

            // ASSERT
            result.Success.ShouldBeTrue();
            result.ErrorMessage.ShouldBeEmpty();

            // Prüfen, ob DeleteFilesAsync exakt einmal mit beiden Pfaden aufgerufen wurde
            _mockFileDeletionService.Verify(f => f.DeleteFilesAsync(
                It.Is<List<string>>(list => list.Contains("C:\\Brave\\Cache") && list.Contains("C:\\Brave\\Cookies") && list.Count == 2),
                It.IsAny<IProgress<string>>(),
                It.IsAny<IProgress<int>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

            // Prüfen, ob die Final-Meldung in der UI gelandet ist (inkl. der korrekten Gesamtanzahl)
            progressUpdates.ShouldContain(p => p.StatusMessage == "Cleanup_Finished" && p.TotalFiles == 100 && p.CurrentFile == 100);
        }

        // =========================================================
        // 3. EXCEPTION HANDLING TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn ein unvorhergesehener Fehler passiert (z.B. ein Dienst wirft eine Exception),
        /// darf die Anwendung nicht abstürzen. Der Fehler muss sicher in der Response verpackt werden.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldCatchExceptions_AndReturnFailure()
        {
            // ARRANGE
            // HIER KORRIGIERT: Vivaldi statt Opera, da Opera nicht im Enum existiert
            var request = new DeleteBrowserContentRequest(BrowserType.Vivaldi, true, true, false);
            var progress = new Progress<DeleteBrowserContentProgress>();

            // Wir zwingen die BrowserFactory zum Absturz
            _mockBrowserFactory.Setup(f => f.Create(It.IsAny<BrowserType>())).Throws(new UnauthorizedAccessException("Zugriff verweigert"));

            // ACT
            var result = await _sut.ExecuteAsync(request, progress, CancellationToken.None);

            // ASSERT
            result.Success.ShouldBeFalse();
            result.ErrorMessage.ShouldBe("Zugriff verweigert");
        }
    }
}