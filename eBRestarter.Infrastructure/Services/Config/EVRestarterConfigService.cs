using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Config;
using eBRestarter.Core.Application.Interfaces.Security;
using eBRestarter.Core.Domain.Models.Records.Config;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization; // WICHTIG: Für den Source Generator hinzugefügt

namespace eBRestarter.Infrastructure.Services.Config;

/// <summary>
/// Implementiert den Konfigurations-Service basierend auf JSON-Dateien.
/// <br/>
/// <b>Architektur-Layer:</b> Infrastructure (Adapter)
/// <br/>
/// <b>Verantwortlichkeit:</b> Lädt und speichert die <see cref="AppConfig"/> in eine JSON-Datei im Dateisystem.
/// Kapselt die Serialisierungs-Logik (System.Text.Json) und den Dateizugriff.
/// </summary>
/// <remarks>
/// Initialisiert eine neue Instanz des <see cref="EVisitorConfigService"/>.
/// </remarks>
/// <param name="pathService">Service zum Ermitteln des Speicherpfads (z.B. AppData).</param>
/// <param name="encryptionService">Service zur Ver-/Entschlüsselung sensibler Daten.</param>
/// <param name="logger">Logger für Fehler- und Statusmeldungen.</param>
public class EVisitorConfigService(IPathService pathService, IEncryptionService encryptionService, ILogger<EVisitorConfigService> logger) : IEVisitorConfigService
{
    private readonly IPathService _pathService = pathService;
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly ILogger<EVisitorConfigService> _logger = logger;

    // Optionen für die JSON-Serialisierung.
    // WriteIndented: Erzeugt lesbares JSON (mit Zeilenumbrüchen).
    // PropertyNameCaseInsensitive: Ignoriert Groß-/Kleinschreibung beim Laden (Robustheit).
    // TypeInfoResolver: Nutzt den Source Generator, damit es im Release-Modus (Trimmed) nicht abstürzt!
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = AppConfigJsonContext.Default
    };

    /// <summary>
    /// Lädt die aktuelle Konfiguration aus der Datei.
    /// Falls die Datei nicht existiert oder fehlerhaft ist, wird eine Standard-Konfiguration zurückgegeben.
    /// </summary>
    /// <returns>Das geladene <see cref="AppConfig"/>-Objekt oder ein neues Standard-Objekt bei Fehlern.</returns>
    public AppConfig LoadConfig()
    {
        var filePath = _pathService.GetConfigFilePath();

        if (!File.Exists(filePath))
        {
            var defaultConfig = new AppConfig();
            SaveConfig(defaultConfig);
            return defaultConfig;
        }

        try
        {
            // 1. JSON lesen (Das klappt auch auf einem fremden PC, da nur Text)
            string jsonString = File.ReadAllText(filePath);

            // Hier sind alle Einstellungen (Theme, User etc.) geladen!
            // Der ApiKey enthält jetzt aber noch den verschlüsselten "Müll".
            var config = JsonSerializer.Deserialize<AppConfig>(jsonString, _jsonOptions) ?? new AppConfig();

            // 2. Versuchen zu entschlüsseln
            if (!string.IsNullOrEmpty(config.Settings.ApiKey))
            {
                // Decrypt fängt intern Fehler ab und gibt string.Empty zurück,
                // wenn es nicht entschlüsselt werden kann (z.B. falscher PC).
                var decryptedKey = _encryptionService.Decrypt(config.Settings.ApiKey);

                // Wenn decryptedKey leer ist (wegen PC-Wechsel), ist das okay.
                // Der User muss ihn dann halt neu eingeben.
                // Wir überschreiben den verschlüsselten Wert im RAM mit dem Ergebnis (Klartext oder leer).

                config = config with
                {
                    Settings = config.Settings with { ApiKey = decryptedKey }
                };

                if (string.IsNullOrEmpty(decryptedKey))
                {
                    _logger.LogWarning("API Key konnte nicht entschlüsselt werden (evtl. PC gewechselt). Der Key muss neu eingegeben werden.");
                }
            }

            // 3. Wir geben die Config zurück – mit allen importierten Settings,
            // nur der Key fehlt eventuell.
            return config;
        }
        catch (JsonException)
        {
            _logger.LogError("Config-Datei ist kein gültiges JSON.");
            return new AppConfig();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kritischer Fehler beim Laden.");
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

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // --- VERSCHLÜSSELUNG ---
            // Wir wollen den Klartext-Key aus dem RAM nicht direkt speichern.
            // Wir erstellen eine Kopie des Config-Objekts nur für den Speichervorgang.
            var encryptedKey = _encryptionService.Encrypt(config.Settings.ApiKey);

            var configToSave = config with
            {
                Settings = config.Settings with { ApiKey = encryptedKey }
            };

            string jsonString = JsonSerializer.Serialize(configToSave, _jsonOptions);
            File.WriteAllText(filePath, jsonString);

            _logger.LogInformation("Konfiguration gespeichert.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Speichern der Konfiguration.");
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

// =========================================================
// SOURCE GENERATOR KONTEXT (Für Release/Trim-Kompatibilität)
// =========================================================
[JsonSerializable(typeof(AppConfig))]
internal partial class AppConfigJsonContext : JsonSerializerContext
{
}