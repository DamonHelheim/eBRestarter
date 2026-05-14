using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Models.Records;
using System.IO;
using System.Text.Json;

namespace eBRestarter.Infrastructure.Services.Authentication;

/// <summary>
/// Implementiert den Anmeldedatenspeicher basierend auf einer JSON-Datei.
/// Vollständig entkoppelt von System.IO über den IWindowsFileSystemService (Repository-Pattern).
/// </summary>
public class JsonCredentialStore : ICredentialStore
{
    private readonly IWindowsFileSystemService _fileSystem;
    private readonly IPathProvider _pathProvider;
    private readonly string _storagePath;

    public JsonCredentialStore(IWindowsFileSystemService fileSystem, IPathProvider pathProvider)
    {
        _fileSystem = fileSystem;
        _pathProvider = pathProvider;

        // Pfad via IPathProvider holen und über das FileSystem kombinieren
        var appData = _pathProvider.RetrieveLocalAppDataDirectory();
        _storagePath = _fileSystem.CombinePaths(appData, "Skylar", "eBRestarter", "eBRestarterConfig.json");
    }

    /// <summary>
    /// Speichert die API-Credentials.
    /// </summary>
    public void SaveCredentials(ApiCredentials credentials)
    {
        var json = JsonSerializer.Serialize(credentials);
        _fileSystem.WriteAllText(_storagePath, json);
    }

    /// <summary>
    /// Lädt die API-Credentials.
    /// </summary>
    public ApiCredentials? LoadCredentials()
    {
        if (!_fileSystem.FileExists(_storagePath)) return null;

        try
        {
            var json = _fileSystem.ReadAllText(_storagePath);
            return JsonSerializer.Deserialize<ApiCredentials>(json);
        }
        catch 
        { 
            return null; 
        }
    }

    /// <summary>
    /// Löscht die API-Credentials.
    /// </summary>
    public void ClearCredentials()
    {
        if (_fileSystem.FileExists(_storagePath)) 
        {
            _fileSystem.DeleteFile(_storagePath);
        }
    }

    /// <summary>
    /// Importiert Anmeldedaten aus einer alten Binärdatei.
    /// Nutzt konsequent den OpenRead-Stream des Ports statt direkten File-Zugriffen.
    /// </summary>
    public ApiCredentials? ImportFromLegacyFile(string filePath)
    {
        if (!_fileSystem.FileExists(filePath)) return null;

        try
        {
            using var stream = _fileSystem.OpenRead(filePath);
            using var reader = new BinaryReader(stream);
            var user = reader.ReadString();
            var key = reader.ReadString();
            return new ApiCredentials(user, key);
        }
        catch
        {
            return null;
        }
    }
}
