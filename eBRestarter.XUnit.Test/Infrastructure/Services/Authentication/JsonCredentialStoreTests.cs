using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Models.Records;
using eBRestarter.Infrastructure.Services.Authentication;
using Moq;
using Shouldly;
using System.IO;
using System.Text.Json;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Services.Authentication
{
    /// <summary>
    /// Testet den JsonCredentialStore, der API-Zugangsdaten auf der Festplatte speichert,
    /// lädt, löscht und alte Binärdateien importiert.
    /// </summary>
    public class JsonCredentialStoreTests
    {
        // Hilfsvariable für den erwarteten Pfad, damit wir ihn nicht überall hartkodieren müssen.
        // Path.Combine verhält sich je nach Betriebssystem leicht anders (Slashes),
        // daher bauen wir ihn hier dynamisch genau so auf, wie die Klasse es tut.
        private readonly string _expectedStoragePath = Path.Combine(@"C:\FakeAppData", "Skylar", "eBRestarter", "eBRestarterConfig.json");

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn Zugangsdaten gespeichert werden, dürfen sie nicht verloren gehen. Wir müssen
        /// sicherstellen, dass das Objekt korrekt in einen JSON-String umgewandelt und an den
        /// richtigen Pfad auf der Festplatte (über den gemockten Service) geschrieben wird.
        ///
        /// WAS WIRD GETESTET?
        /// Wir übergeben ein gültiges ApiCredentials-Objekt. Dann prüfen wir, ob WriteAllText
        /// exakt einmal mit dem korrekten Pfad und dem gültigen JSON-String aufgerufen wurde.
        /// </summary>
        [Fact]
        public void SaveCredentials_ShouldSerializeToJson_AndWriteToFile()
        {
            // ARRANGE
            var mockFileSystem = new Mock<IWindowsFileSystemService>();
            var mockPathProvider = new Mock<IPathProvider>();

            mockPathProvider.Setup(p => p.GetLocalAppDataDirectory()).Returns(@"C:\FakeAppData");

            var store = new JsonCredentialStore(mockFileSystem.Object, mockPathProvider.Object);
            var credentials = new ApiCredentials("TestUser", "TestKey123");

            // Erwartetes JSON (genau so, wie System.Text.Json es standardmäßig serialisiert)
            string expectedJson = JsonSerializer.Serialize(credentials);

            // ACT
            store.SaveCredentials(credentials);

            // ASSERT
            // Verify prüft, ob die Methode exakt 1x mit den korrekten Parametern gefeuert wurde.
            mockFileSystem.Verify(fs => fs.WriteAllText(_expectedStoragePath, expectedJson), Times.Once);
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Beim Starten der App muss sie wissen, ob der Nutzer schon eingeloggt ist.
        /// Wenn die Config-Datei existiert, muss sie fehlerfrei ausgelesen und in ein
        /// C#-Objekt zurückverwandelt werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir simulieren, dass die Datei existiert und einen korrekten JSON-String enthält.
        /// Die Methode muss das korrekte ApiCredentials-Objekt zurückgeben.
        /// </summary>
        [Fact]
        public void LoadCredentials_ShouldReturnCredentials_WhenFileExistsAndIsValidJson()
        {
            // ARRANGE
            var mockFileSystem = new Mock<IWindowsFileSystemService>();
            var mockPathProvider = new Mock<IPathProvider>();

            mockPathProvider.Setup(p => p.GetLocalAppDataDirectory()).Returns(@"C:\FakeAppData");

            // Die simulierte Datei existiert
            mockFileSystem.Setup(fs => fs.FileExists(_expectedStoragePath)).Returns(true);

            // Der Inhalt der simulierten Datei
            string fakeJson = "{\"Username\":\"TestUser\",\"ApiKey\":\"TestKey123\"}";
            mockFileSystem.Setup(fs => fs.ReadAllText(_expectedStoragePath)).Returns(fakeJson);

            var store = new JsonCredentialStore(mockFileSystem.Object, mockPathProvider.Object);

            // ACT
            var result = store.LoadCredentials();

            // ASSERT
            result.ShouldNotBeNull();
            result.Username.ShouldBe("TestUser");
            result.ApiKey.ShouldBe("TestKey123");
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn ein neuer Nutzer die App zum ersten Mal öffnet, gibt es noch keine Datei.
        /// Die App darf dann nicht abstürzen, sondern muss geordnet 'null' zurückgeben.
        /// Auch wenn die Datei z. B. durch einen Virenscanner zerstört wurde (ungültiges JSON),
        /// darf das Programm nicht crashen (Try-Catch greift).
        ///
        /// WAS WIRD GETESTET?
        /// Fall 1: Datei fehlt -> Ergebnis null.
        /// Fall 2: Datei existiert, aber JSON ist kaputt -> Ergebnis null.
        /// </summary>
        [Theory]
        [InlineData(false, null)] // Datei existiert nicht
        [InlineData(true, "Kein Gültiges JSON {")] // Datei existiert, aber JSON ist kaputt
        public void LoadCredentials_ShouldReturnNull_WhenFileIsMissingOrBroken(bool fileExists, string? fileContent)
        {
            // ARRANGE
            var mockFileSystem = new Mock<IWindowsFileSystemService>();
            var mockPathProvider = new Mock<IPathProvider>();

            mockPathProvider.Setup(p => p.GetLocalAppDataDirectory()).Returns(@"C:\FakeAppData");
            mockFileSystem.Setup(fs => fs.FileExists(_expectedStoragePath)).Returns(fileExists);

            if (fileContent != null)
            {
                mockFileSystem.Setup(fs => fs.ReadAllText(_expectedStoragePath)).Returns(fileContent);
            }

            var store = new JsonCredentialStore(mockFileSystem.Object, mockPathProvider.Object);

            // ACT
            var result = store.LoadCredentials();

            // ASSERT
            result.ShouldBeNull();
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Der Logout-Prozess muss zuverlässig die Datei von der Festplatte putzen,
        /// damit nach einem Neustart niemand automatisch eingeloggt wird.
        /// </summary>
        [Fact]
        public void ClearCredentials_ShouldDeleteFile_WhenFileExists()
        {
            // ARRANGE
            var mockFileSystem = new Mock<IWindowsFileSystemService>();
            var mockPathProvider = new Mock<IPathProvider>();

            mockPathProvider.Setup(p => p.GetLocalAppDataDirectory()).Returns(@"C:\FakeAppData");
            mockFileSystem.Setup(fs => fs.FileExists(_expectedStoragePath)).Returns(true);

            var store = new JsonCredentialStore(mockFileSystem.Object, mockPathProvider.Object);

            // ACT
            store.ClearCredentials();

            // ASSERT
            mockFileSystem.Verify(fs => fs.DeleteFile(_expectedStoragePath), Times.Once);
        }

        // =========================================================
        // INTEGRATION-TESTS FÜR DIE LEGACY FUNKTION
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Die ImportFromLegacyFile Methode nutzt direkt System.IO.File und den BinaryReader.
        /// Wir können das nicht mit Moq simulieren. Stattdessen machen wir einen Mini-Integrationstest
        /// und legen eine ECHTE temporäre Binärdatei an, um zu prüfen, ob die Auslese-Logik
        /// von früher (BinaryReader.ReadString) noch korrekt funktioniert.
        /// </summary>
        [Fact]
        public void ImportFromLegacyFile_ShouldReadBinaryFileCorrectly()
        {
            // ARRANGE
            // Mocks für die restliche Klasse, die in diesem Test aber kaum gebraucht werden
            var mockFileSystem = new Mock<IWindowsFileSystemService>();
            var mockPathProvider = new Mock<IPathProvider>();
            mockPathProvider.Setup(p => p.GetLocalAppDataDirectory()).Returns(@"C:\FakeAppData");

            var store = new JsonCredentialStore(mockFileSystem.Object, mockPathProvider.Object);

            // Wir legen eine echte, temporäre Datei in Windows an
            string tempFilePath = Path.GetTempFileName();

            try
            {
                // Wir schreiben Binärdaten in die Datei, exakt so,
                // wie es das alte eBRestarter System gemacht hat.
                using (var writer = new BinaryWriter(File.Open(tempFilePath, FileMode.OpenOrCreate)))
                {
                    writer.Write("LegacyUser");
                    writer.Write("LegacyKey999");
                }

                // ACT
                // Jetzt testen wir unsere Import-Methode auf die echte Datei
                var result = store.ImportFromLegacyFile(tempFilePath);

                // ASSERT
                result.ShouldNotBeNull();
                result.Username.ShouldBe("LegacyUser");
                result.ApiKey.ShouldBe("LegacyKey999");
            }
            finally
            {
                // CLEANUP: Egal ob der Test fehlschlägt oder durchgeht,
                // wir MÜSSEN die echte Datei am Ende wieder löschen,
                // damit wir die Festplatte des Test-Rechners nicht zumüllen!
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn der Nutzer gar keine alte Binärdatei hat (weil er die App neu installiert),
        /// muss die Methode einfach 'null' zurückgeben und darf nicht abstürzen.
        /// </summary>
        [Fact]
        public void ImportFromLegacyFile_ShouldReturnNull_WhenFileDoesNotExist()
        {
            // ARRANGE
            var mockFileSystem = new Mock<IWindowsFileSystemService>();
            var mockPathProvider = new Mock<IPathProvider>();

            // FEHLERBEHEBUNG: Auch hier müssen wir dem Konstruktor einen Pfad vorgaukeln,
            // sonst stürzt Path.Combine direkt ab, weil GetLocalAppDataDirectory 'null' liefert!
            mockPathProvider.Setup(p => p.GetLocalAppDataDirectory()).Returns(@"C:\FakeAppData");

            var store = new JsonCredentialStore(mockFileSystem.Object, mockPathProvider.Object);

            // Ein Pfad, der garantiert nicht existiert
            string nonExistentPath = Path.Combine(Path.GetTempPath(), "definitely_not_existing_file.bin");

            // ACT
            var result = store.ImportFromLegacyFile(nonExistentPath);

            // ASSERT
            result.ShouldBeNull();
        }
    }
}