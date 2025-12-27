using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Domain.Models.Records.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace eBRestarter.Infrastructure.Services.Config
{
    /// <summary>
    /// Implementiert den Konfigurations-Service basierend auf JSON-Dateien.
    /// <br/>
    /// <b>Architektur-Layer:</b> Infrastructure (Adapter)
    /// <br/>
    /// <b>Verantwortlichkeit:</b> Lädt und speichert die <see cref="AppConfig"/> in eine JSON-Datei im Dateisystem.
    /// Kapselt die Serialisierungs-Logik (System.Text.Json) und den Dateizugriff.
    /// </summary>
    public class JsonConfigService : IConfigService
    {
        private readonly IPathService _pathService;
        private readonly ILogger<JsonConfigService> _logger;

        // Optionen für die JSON-Serialisierung.
        // WriteIndented: Erzeugt lesbares JSON (mit Zeilenumbrüchen).
        // PropertyNameCaseInsensitive: Ignoriert Groß-/Kleinschreibung beim Laden (Robustheit).
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="JsonConfigService"/>.
        /// </summary>
        /// <param name="pathService">Service zum Ermitteln des Speicherpfads (z.B. AppData).</param>
        /// <param name="logger">Logger für Fehler- und Statusmeldungen.</param>
        public JsonConfigService(IPathService pathService, ILogger<JsonConfigService> logger)
        {
            _pathService = pathService;
            _logger = logger;
        }

        /// <summary>
        /// Lädt die aktuelle Konfiguration aus der Datei.
        /// Falls die Datei nicht existiert oder fehlerhaft ist, wird eine Standard-Konfiguration zurückgegeben.
        /// </summary>
        /// <returns>Das geladene <see cref="AppConfig"/>-Objekt oder ein neues Standard-Objekt bei Fehlern.</returns>
        public AppConfig LoadConfig()
        {
            var filePath = _pathService.GetConfigFilePath();

            // Fall 1: Datei existiert noch nicht (erster Start)
            if (!File.Exists(filePath))
            {
                _logger.LogInformation("Konfigurationsdatei '{Path}' nicht gefunden. Erstelle Standardwerte.", filePath);

                var defaultConfig = new AppConfig();
                // Wir speichern direkt ab, damit der User sofort eine Datei zum Bearbeiten hat.
                SaveConfig(defaultConfig);

                return defaultConfig;
            }

            // Fall 2: Datei lesen und parsen
            try
            {
                string jsonString = File.ReadAllText(filePath);

                // Deserialisieren in unser Model. 
                var config = JsonSerializer.Deserialize<AppConfig>(jsonString, _jsonOptions);

                // Falls die Datei leer war (null), geben wir trotzdem ein valides Objekt zurück.
                return config ?? new AppConfig();
            }
            catch (JsonException jsonEx)
            {
                // Spezifischer Catch für defektes JSON (z.B. Syntaxfehler durch manuelles Editieren)
                _logger.LogError(jsonEx, "Die Konfigurationsdatei ist beschädigt (ungültiges JSON). Nutze Standardwerte.");
                return new AppConfig();
            }
            catch (Exception ex)
            {
                // Allgemeiner Catch für Zugriffsprobleme (z.B. Datei gesperrt, keine Rechte)
                _logger.LogError(ex, "Allgemeiner Fehler beim Laden der Konfiguration.");
                return new AppConfig();
            }
        }

        /// <summary>
        /// Speichert das übergebene Konfigurationsobjekt als JSON-Datei.
        /// Erstellt automatisch das Zielverzeichnis, falls es fehlt.
        /// </summary>
        /// <param name="config">Das zu speichernde Konfigurationsobjekt.</param>
        public void SaveConfig(AppConfig config)
        {
            try
            {
                var filePath = _pathService.GetConfigFilePath();
                var directory = Path.GetDirectoryName(filePath);

                // Self-Healing: Wenn der Ordner (z.B. %AppData%/eBRestarter) gelöscht wurde, erstellen wir ihn neu.
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string jsonString = JsonSerializer.Serialize(config, _jsonOptions);

                // Überschreibt die Datei vollständig mit dem neuen Inhalt.
                File.WriteAllText(filePath, jsonString);

                _logger.LogInformation("Konfiguration erfolgreich unter '{Path}' gespeichert.", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kritischer Fehler beim Speichern der Konfiguration. Änderungen gingen verloren.");
            }
        }

        /// <summary>
        /// Setzt die Konfiguration auf die werksseitigen Standardwerte zurück und überschreibt die Datei.
        /// </summary>
        public void ResetConfig()
        {
            _logger.LogWarning("Konfiguration wird auf Standardwerte zurückgesetzt.");

            var defaultConfig = new AppConfig();
            SaveConfig(defaultConfig);
        }
    }
}
