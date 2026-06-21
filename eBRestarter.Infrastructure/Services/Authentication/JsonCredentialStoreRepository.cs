using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Application.Models.Records;
using System.IO;
using System.Text.Json;

namespace eBRestarter.Infrastructure.Services.Authentication;

/// <summary>
/// Implementiert den Anmeldedatenspeicher basierend auf einer JSON-Datei.
/// VollstÃ¤ndig entkoppelt von System.IO Ã¼ber den IWindowsFileSystemService (Repository-Pattern).
/// </summary>
public class JsonCredentialStoreRepository : ICredentialStore
{
    private readonly IWindowsFileSystemService _fileSystem;
    private readonly IPathProvider _pathProvider;
    private readonly string _storagePath;

    public JsonCredentialStoreRepository(IWindowsFileSystemService fileSystem, IPathProvider pathProvider)
    {
        _fileSystem = fileSystem;
        _pathProvider = pathProvider;

        // Pfad via IPathProvider holen und Ã¼ber das FileSystem kombinieren
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
    /// LÃ¤dt die API-Credentials.
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
    /// LÃ¶scht die API-Credentials.
    /// </summary>
    public void ClearCredentials()
    {
        if (_fileSystem.FileExists(_storagePath)) 
        {
            _fileSystem.DeleteFile(_storagePath);
        }
    }

    /// <summary>
    /// Importiert Anmeldedaten aus einer alten BinÃ¤rdatei.
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

