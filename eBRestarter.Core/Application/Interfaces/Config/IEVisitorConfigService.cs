using eBRestarter.Core.Application.Models.Config;

namespace eBRestarter.Core.Application.Interfaces.Config;

public interface IEVisitorConfigService
{
    // Lädt die Config. Falls keine existiert, wird eine Standard-Config erstellt.
    AppConfig LoadConfig();

    // Speichert die komplette Config (ersetzt SaveXMLConfigFileByTagValue)
    void SaveConfig(AppConfig config);

    // Löscht oder resettet die Config
    void ResetConfig();
}
