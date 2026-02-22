using eBRestarter.Core.Application.Interfaces.Browser;
using eBRestarter.Core.Application.Interfaces.OperatingSystem;
using eBRestarter.Core.Domain.Enums;
using eBRestarter.Infrastructure.Browsers;
using eBRestarter.Infrastructure.Services.WindowsOS;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.XUnit.Test.IntegrationTests.Infrastructure
{
    public class BrowserDataGeneratorTests
    {
        /// <summary>
        /// Generiert Dummy-Dateien in den Cache-, Cookie- und Extension-Verzeichnissen des angegebenen Browsers.
        /// ACHTUNG: Dies ist ein Integrationstest, der in die ECHTEN Browser-Pfade deines Windows-Systems schreibt!
        /// </summary>
        /// <param name="browserType">Der zu testende Browser</param>
        /// <param name="filesPerFolder">Wie viele Dateien PRO ORDNER generiert werden sollen</param>
        [Theory]
        [InlineData(BrowserType.Brave, 10000)]   // Erzeugt 100 Dateien je Ordner in Vivaldi
        [InlineData(BrowserType.Vivaldi, 10000)]   // Erzeugt 100 Dateien je Ordner in Vivaldi
        //[InlineData(BrowserType.Chrome, 50)]     // Erzeugt 50 Dateien je Ordner in Chrome
        //[InlineData(BrowserType.Edge, 10)]       // Erzeugt 10 Dateien je Ordner in Edge
        //[InlineData(BrowserType.Firefox, 1000)]  // Erzeugt 1000 Dateien je Ordner in Firefox
        public void GenerateBrowserDumpFiles_IntegrationTest(BrowserType browserType, int filesPerFolder)
        {
            // Arrange: 1. Reale Services für Dateisystem aufbauen
            // Da GetPaths() reale Systempfade (wie LocalAppData) nutzt, verwenden wir die echte Implementierung
            var realFileSystemService = new WindowsFileSystemService();

            // Wir mocken die Facade, geben aber den echten FileSystem-Service zurück
            var osFacadeMock = new Mock<IOperatingSystemFacade>();
            osFacadeMock.Setup(os => os.WindowsFileSystemService).Returns(realFileSystemService);

            // Wenn Registry-Zugriffe für GetPaths benötigt werden (wie bei Firefox), mocken oder reale Klasse nutzen
            osFacadeMock.Setup(os => os.WindowsRegistryService).Returns(new WindowsRegistryService());

            // 2. Den entsprechenden Browser über eine Hilfsmethode instanziieren
            IBrowser browser = CreateBrowserInstance(browserType, osFacadeMock.Object);

            // Act: 3. Pfade vom Browser generieren lassen
            var paths = browser.GetPaths();

            // 4. Dummy-Dateien in die ermittelten Pfade schreiben (3 KB pro Datei)
            int sizeInKb = 3;

            // Zähle zur Kontrolle, wie viele Dateien insgesamt generiert wurden
            int totalFilesCreated = 0;

            if (paths.CacheDirs != null)
                totalFilesCreated += GenerateDummyFilesInDirectories(paths.CacheDirs, filesPerFolder, sizeInKb);

            if (paths.CookiesDirs != null)
                totalFilesCreated += GenerateDummyFilesInDirectories(paths.CookiesDirs, filesPerFolder, sizeInKb);

            if (paths.ExtensionsDirs != null)
                totalFilesCreated += GenerateDummyFilesInDirectories(paths.ExtensionsDirs, filesPerFolder, sizeInKb);

            // Assert: Prüfen, ob überhaupt etwas geschrieben wurde
            // (Wenn der Browser auf dem PC nicht installiert ist, oder noch nie geöffnet wurde, 
            // gibt GetPaths() ggf. keine Ordner zurück)
            Assert.True(totalFilesCreated > 0, $"Es wurden keine Dateien für {browserType} erstellt. Ist der Browser installiert und wurde er mindestens einmal gestartet?");
        }

        // =========================================================
        // HILFSMETHODEN
        // =========================================================

        /// <summary>
        /// Schreibt X Dateien mit Y Kilobyte Größe in die angegebenen Verzeichnisse.
        /// </summary>
        private int GenerateDummyFilesInDirectories(IEnumerable<string> directories, int count, int sizeInKb)
        {
            int createdCount = 0;

            // Zufällige Daten für die Datei erzeugen (1 KB = 1024 Bytes)
            byte[] dummyContent = new byte[sizeInKb * 1024];
            new Random().NextBytes(dummyContent);

            foreach (var dir in directories)
            {
                // Wenn das Verzeichnis (z.B. "Service Worker") vom Browser noch nie erstellt wurde, 
                // legen wir es für unseren Test einfach an.
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                for (int i = 0; i < count; i++)
                {
                    // Dateiname: dummy_12345678.tmp
                    string filePath = Path.Combine(dir, $"dummy_{Guid.NewGuid():N}.tmp");
                    File.WriteAllBytes(filePath, dummyContent);
                    createdCount++;
                }
            }

            return createdCount;
        }

        /// <summary>
        /// Factory-Hilfsmethode, um den richtigen Browser mit gemockten Loggern zu erstellen.
        /// </summary>
        private IBrowser CreateBrowserInstance(BrowserType type, IOperatingSystemFacade osFacade)
        {
            return type switch
            {
                BrowserType.Chrome => new ChromeBrowser(osFacade, Mock.Of<ILogger<ChromeBrowser>>()),
                BrowserType.Firefox => new FirefoxBrowser(osFacade, Mock.Of<ILogger<FirefoxBrowser>>()),
                BrowserType.Edge => new EdgeBrowser(osFacade, Mock.Of<ILogger<EdgeBrowser>>()),
                BrowserType.Brave => new BraveBrowser(osFacade, Mock.Of<ILogger<BraveBrowser>>()),
                BrowserType.Vivaldi => new VivaldiBrowser(osFacade, Mock.Of<ILogger<VivaldiBrowser>>()),
                _ => throw new NotImplementedException($"Browser {type} wird nicht vom Test unterstützt.")
            };
        }
    }
}
