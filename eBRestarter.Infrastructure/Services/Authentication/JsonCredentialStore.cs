using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Application.Interfaces.Authentication;
using eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;
using eBRestarter.Core.Domain.Models.Records;
using System.IO;
using System.Text.Json;

namespace eBRestarter.Infrastructure.Services.Authentication;

public class JsonCredentialStore : ICredentialStore
{
    private readonly IWindowsFileSystemService _fileSystem;
    private readonly IPathProvider _pathProvider;
    // Pfad zur secrets.json (statt binärer Datei)
    private readonly string _storagePath;

    public JsonCredentialStore(IWindowsFileSystemService fileSystem, IPathProvider pathProvider)
    {
        _fileSystem = fileSystem;
        _pathProvider = pathProvider;

        // Pfad via IPathProvider holen
        var appData = _pathProvider.GetLocalAppDataDirectory();

        _storagePath = Path.Combine(appData, "Skylar", "eBRestarter", "eBRestarterConfig.json");
    }

    public void SaveCredentials(ApiCredentials credentials)
    {
        var json = JsonSerializer.Serialize(credentials);
        _fileSystem.WriteAllText(_storagePath, json); // Nutzt IFileSystemService Wrapper
    }

    public ApiCredentials? LoadCredentials()
    {
        if (!_fileSystem.FileExists(_storagePath)) return null;

        try
        {
            var json = _fileSystem.ReadAllText(_storagePath);
            return JsonSerializer.Deserialize<ApiCredentials>(json);
        }
        catch { return null; }
    }

    public void ClearCredentials()
    {
        if (_fileSystem.FileExists(_storagePath)) _fileSystem.DeleteFile(_storagePath);
    }

    public ApiCredentials? ImportFromLegacyFile(string filePath)
    {
        // Alte BinaryReader Logik für Import
        if (!File.Exists(filePath)) return null;

        try
        {
            using var reader = new BinaryReader(File.Open(filePath, FileMode.Open));
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
