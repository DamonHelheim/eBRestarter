using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using System.Text.Json;

namespace eBRestarter.Infrastructure.Repositories.Authentication;

/// <summary>
/// Implements the credential store based on a JSON file.
/// Completely decoupled from System.IO via the IFileSystemPort (Repository Pattern).
/// </summary>
public sealed class JsonCredentialStoreRepository : IOutboundPortCredentialStoreRepository
{
    private readonly IOutboundPortFileSystem _fileSystem;
    private readonly IOutboundPortAppPathProvider _pathProvider;
    private readonly string _storagePath;

    public JsonCredentialStoreRepository(IOutboundPortFileSystem fileSystem, IOutboundPortAppPathProvider pathProvider)
    {
        _fileSystem = fileSystem;
        _pathProvider = pathProvider;

        // Resolve path via IAppPathPort and combine it using the FileSystem adapter
        var appData = _pathProvider.RetrieveLocalAppDataDirectory();
        _storagePath = _fileSystem.CombinePaths(appData, "Skylar", "eBRestarter", "eBRestarterConfig.json");
    }

    /// <summary>
    /// Saves the API credentials.
    /// </summary>
    public void SaveCredentials(ApiCredentials credentials)
    {
        var json = JsonSerializer.Serialize(credentials);
        _fileSystem.WriteAllText(_storagePath, json);
    }

    /// <summary>
    /// Loads the API credentials.
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
    /// Deletes the API credentials.
    /// </summary>
    public void ClearCredentials()
    {
        if (_fileSystem.FileExists(_storagePath))
        {
            _fileSystem.DeleteFile(_storagePath);
        }
    }

    /// <summary>
    /// Imports credentials from a legacy binary file.
    /// Consistently utilizes the OpenRead stream port instead of direct static file access.
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





