using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.Security;
using eBRestarter.Core.Domain.Models.Records.Config;
using eBRestarter.Infrastructure.Services.Config;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace eBRestarter.XUnit.Test.Infrastructure.Services.Config
{
    /// <summary>
    /// Testet den EVisitorConfigService. Da dieser direkt System.IO nutzt,
    /// arbeiten wir hier mit echten, temporären Dateien, die wir nach jedem Test aufräumen.
    /// </summary>
    public class EVisitorConfigServiceTests : IDisposable
    {
        private readonly string _tempFilePath;
        private readonly Mock<IPathService> _mockPathService;
        private readonly Mock<IEncryptionService> _mockEncryptionService;
        private readonly Mock<ILogger<EVisitorConfigService>> _mockLogger;

        public EVisitorConfigServiceTests()
        {
            // Wir generieren für JEDEN Testdurchlauf einen einzigartigen, temporären Dateipfad.
            // Dadurch stören sich die Tests nicht gegenseitig, falls xUnit sie parallel ausführt.
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"TestConfig_{Guid.NewGuid()}.json");

            _mockPathService = new Mock<IPathService>();
            _mockEncryptionService = new Mock<IEncryptionService>();
            _mockLogger = new Mock<ILogger<EVisitorConfigService>>();

            // Wir weisen den PathService an, immer unsere sichere, temporäre Datei zurückzugeben
            _mockPathService.Setup(p => p.GetConfigFilePath()).Returns(_tempFilePath);
        }

        // =========================================================
        // 1. SAVE TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn wir die Konfiguration speichern, darf der API-Key niemals im Klartext auf der Festplatte landen!
        ///
        /// WAS WIRD GETESTET?
        /// Wir übergeben ein Config-Objekt mit Klartext-Key. Wir prüfen, ob der EncryptionService aufgerufen wird
        /// und ob in der physisch geschriebenen JSON-Datei tatsächlich der verschlüsselte String steht.
        /// </summary>
        [Fact]
        public void SaveConfig_ShouldEncryptApiKey_AndWriteJsonToFile()
        {
            // ARRANGE
            var service = new EVisitorConfigService(_mockPathService.Object, _mockEncryptionService.Object, _mockLogger.Object);

            // Dummy Config erstellen (ohne Umlaute, um System.Text.Json Unicode-Escaping zu umgehen!)
            var configToSave = new AppConfig();
            configToSave = configToSave with
            {
                Settings = configToSave.Settings with { ApiKey = "SecretPlaintextKey123" }
            };

            // Mock für die Verschlüsselung
            // Wir nutzen It.IsAny<string>(), um absolut sicherzugehen, dass der Mock auch feuert,
            // unabhängig davon, wie das Record-Objekt exakt initialisiert wurde.
            _mockEncryptionService
                .Setup(e => e.Encrypt(It.IsAny<string>()))
                .Returns("EncryptedSecretData789");

            // ACT
            service.SaveConfig(configToSave);

            // ASSERT
            // 1. Hat er die Datei physisch angelegt?
            File.Exists(_tempFilePath).ShouldBeTrue();

            // 2. Wir lesen die Datei händisch aus und prüfen, was wirklich drin steht
            string savedJson = File.ReadAllText(_tempFilePath);

            savedJson.ShouldContain("EncryptedSecretData789"); // Der verschlüsselte Wert MUSS drin sein
            savedJson.ShouldNotContain("SecretPlaintextKey123"); // Der Klartext DARF NICHT drin sein
        }

        // =========================================================
        // 2. LOAD TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Beim allerersten Start der App gibt es noch keine config.json.
        /// Das Programm darf nicht abstürzen, sondern muss eine neue Standard-Config erstellen.
        ///
        /// WAS WIRD GETESTET?
        /// Wir stellen sicher, dass die Datei NICHT existiert, rufen LoadConfig() auf
        /// und prüfen, ob die Methode eine Default-Config liefert UND die Datei direkt anlegt.
        /// </summary>
        [Fact]
        public void LoadConfig_ShouldCreateAndReturnDefault_WhenFileDoesNotExist()
        {
            // ARRANGE
            // Sicherstellen, dass keine Reste existieren
            if (File.Exists(_tempFilePath)) File.Delete(_tempFilePath);

            var service = new EVisitorConfigService(_mockPathService.Object, _mockEncryptionService.Object, _mockLogger.Object);

            // ACT
            var result = service.LoadConfig();

            // ASSERT
            result.ShouldNotBeNull();
            File.Exists(_tempFilePath).ShouldBeTrue(); // SaveConfig wurde als Fallback aufgerufen!
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn eine Datei existiert und gültig ist, muss der darin verschlüsselte API-Key
        /// wieder in Klartext verwandelt werden, damit das Programm ihn nutzen kann.
        ///
        /// WAS WIRD GETESTET?
        /// Wir schreiben händisch eine JSON-Datei mit verschlüsseltem Key. Wir prüfen, ob LoadConfig()
        /// den Decrypt-Service aufruft und uns das Objekt mit dem entschlüsselten Klartext-Key liefert.
        /// </summary>
        [Fact]
        public void LoadConfig_ShouldDecryptApiKey_WhenFileIsValid()
        {
            // ARRANGE
            string fakeEncryptedJson = @"{
                ""Settings"": {
                    ""ApiKey"": ""VerschlüsselterWert""
                }
            }";
            File.WriteAllText(_tempFilePath, fakeEncryptedJson);

            _mockEncryptionService
                .Setup(e => e.Decrypt("VerschlüsselterWert"))
                .Returns("EntschlüsselterKlartext");

            var service = new EVisitorConfigService(_mockPathService.Object, _mockEncryptionService.Object, _mockLogger.Object);

            // ACT
            var result = service.LoadConfig();

            // ASSERT
            result.Settings.ApiKey.ShouldBe("EntschlüsselterKlartext");
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn der Nutzer den USB-Stick mit der App an einen anderen PC steckt, schlägt die Entschlüsselung
        /// (Windows DPAPI) fehl. Die App darf nicht crashen, sondern muss den Key leeren.
        ///
        /// WAS WIRD GETESTET?
        /// Wir zwingen den Decryption-Service dazu, einen leeren String zurückzugeben (Fehlverhalten).
        /// Die Load-Methode muss das überleben und eine Config mit leerem Key zurückliefern.
        /// </summary>
        [Fact]
        public void LoadConfig_ShouldHandleDecryptionFailure_Gracefully()
        {
            // ARRANGE
            string fakeEncryptedJson = @"{ ""Settings"": { ""ApiKey"": ""FalscherPC_Key"" } }";
            File.WriteAllText(_tempFilePath, fakeEncryptedJson);

            // Mock simuliert: "Konnte nicht entschlüsselt werden!"
            _mockEncryptionService
                .Setup(e => e.Decrypt(It.IsAny<string>()))
                .Returns(string.Empty);

            var service = new EVisitorConfigService(_mockPathService.Object, _mockEncryptionService.Object, _mockLogger.Object);

            // ACT
            var result = service.LoadConfig();

            // ASSERT
            result.ShouldNotBeNull();
            result.Settings.ApiKey.ShouldBeEmpty();

            // Optional: Prüfen ob die Warnung geloggt wurde
            // Da ILogger eine Extension-Method nutzt, ist Verify() bei Moq hier etwas komplexer,
            // wir verlassen uns auf den korrekten Return-Wert.
        }

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// Wenn die JSON-Datei manuell bearbeitet wurde (z.B. Tippfehler, Klammer vergessen),
        /// wirft System.Text.Json eine JsonException. Das muss sauber abgefangen werden.
        ///
        /// WAS WIRD GETESTET?
        /// Wir schreiben absoluten Müll in die Datei und prüfen, ob die Methode 'new AppConfig()' zurückgibt.
        /// </summary>
        [Fact]
        public void LoadConfig_ShouldReturnDefault_WhenJsonIsBroken()
        {
            // ARRANGE
            File.WriteAllText(_tempFilePath, "Das hier ist definitiv { kein gültiges ] JSON!");

            var service = new EVisitorConfigService(_mockPathService.Object, _mockEncryptionService.Object, _mockLogger.Object);

            // ACT
            var result = service.LoadConfig();

            // ASSERT
            result.ShouldNotBeNull(); // Ein sauberes Default-Objekt
            // Keine Exception ist nach außen gedrungen!
        }

        // =========================================================
        // 3. RESET TESTS
        // =========================================================

        /// <summary>
        /// WARUM WIRD DAS GETESTET?
        /// ResetConfig muss eine fehlerhafte oder alte Config-Datei kompromisslos
        /// durch die Werkseinstellungen überschreiben.
        /// </summary>
        [Fact]
        public void ResetConfig_ShouldWriteDefaultConfigToFile()
        {
            // ARRANGE
            File.WriteAllText(_tempFilePath, "Alte, kaputte Daten");

            var service = new EVisitorConfigService(_mockPathService.Object, _mockEncryptionService.Object, _mockLogger.Object);

            // ACT
            service.ResetConfig();

            // ASSERT
            string newContent = File.ReadAllText(_tempFilePath);
            newContent.ShouldNotContain("Alte, kaputte Daten");
            newContent.ShouldContain("{"); // Muss JSON sein
        }

        // =========================================================
        // CLEANUP (wird nach JEDEM Test automatisch ausgeführt)
        // =========================================================
        public void Dispose()
        {
            // Wir löschen die temporäre Test-Datei restlos von der Festplatte
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }
    }
}