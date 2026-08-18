using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Security;
using eBRestarter.Core.Application.ObjectArchetypes.Constants;

namespace eBRestarter.Infrastructure.BehavioralComponents.Repositories.Authentication;

/// <summary>
/// Implements the credential store based on a JSON file.
/// Completely decoupled from System.IO via the IFileSystemPort (Repository Pattern).
/// </summary>
/// <remarks>
/// <para>
/// 🔒 Security Guidelines Section 2.1: Previously the API key was written as plaintext to
/// <c>%LocalAppData%\Skylar\eBRestarter\eBRestarterConfig.json</c> via <c>JsonSerializer.Serialize(credentials)</c>,
/// bypassing the DPAPI protection utilized by <c>EncryptedEVisitorConfigRepositoryDecorator</c>.
/// Any process under the user context or profile backup could directly access the plaintext key.
/// </para>
/// <para>
/// The key is now encrypted before storage using <see cref="IOutboundPortEncryption"/> (Windows DPAPI,
/// <c>CurrentUser</c> scope) and decrypted upon retrieval. The username remains plaintext for operational diagnostics.
/// </para>
/// </remarks>
/// <param name="encryption">The outbound encryption port for encrypting and decrypting sensitive API keys via DPAPI.</param>
/// <param name="fileSystem">The outbound file system port decoupling physical I/O operations.</param>
/// <param name="logger">The logger instance for telemetry and error logging.</param>
/// <param name="pathProvider">The outbound path provider determining application directories.</param>
public sealed class JsonCredentialStoreRepository(
    IOutboundPortEncryption encryption,
    IOutboundPortFileSystem fileSystem,
    ILogger<JsonCredentialStoreRepository> logger,
    IOutboundPortAppPathProvider pathProvider)
    : IOutboundPortCredentialStoreRepository
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Types & Strings (alphabetical) ──
    private const string AppFolderName = "eBRestarter";
    private const string ConfigFileName = "eBRestarterConfig.json";
    private const string SkylarFolderName = "Skylar";

    // 🔒 Section 8.3: Credential failure paths must be logged explicitly.
    private const string DecryptionFailedLogMessage = "The stored API key could not be decrypted (possibly copied from another machine or user profile). The key must be re-entered.";
    private const string EncryptionFailedExceptionMessage = "The API key could not be encrypted and was therefore NOT written to disk.";
    private const string LegacyImportFailedLogMessage = "Legacy credential file {Path} could not be read.";
    private const string LoadFailedLogMessage = "Stored credentials at {Path} could not be read.";


    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════

    // ── Block 1: Injected Dependencies (alphabetical) ──
    private readonly IOutboundPortEncryption _encryption = encryption ?? throw new ArgumentNullException(nameof(encryption));
    private readonly IOutboundPortFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly ILogger<JsonCredentialStoreRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // ── Block 2: Primitive Types & Strings (alphabetical) ──
    private readonly string _storagePath = (fileSystem ?? throw new ArgumentNullException(nameof(fileSystem))).CombinePaths(
        (pathProvider ?? throw new ArgumentNullException(nameof(pathProvider))).RetrieveLocalAppDataDirectory(),
        SkylarFolderName, AppFolderName, ConfigFileName);


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════

    /// <inheritdoc />
    public void ClearCredentials()
    {
        if (_fileSystem.FileExists(_storagePath))
        {
            _fileSystem.DeleteFile(_storagePath);
        }
    }

    /// <inheritdoc />
    public ApiCredentials? ImportFromLegacyFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!_fileSystem.FileExists(filePath))
        {
            return null;
        }

        try
        {
            using var stream = _fileSystem.OpenRead(filePath);

            return LegacyCredentialFileReader.Read(stream);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(LogEventIds.Configuration.CredentialStoreLegacyImportFailed, exception, LegacyImportFailedLogMessage, filePath);

            return null;
        }
    }

    /// <inheritdoc />
    public ApiCredentials? LoadCredentials()
    {
        if (!_fileSystem.FileExists(_storagePath))
        {
            return null;
        }

        try
        {
            var json = _fileSystem.ReadAllText(_storagePath);
            var storedCredentials = JsonSerializer.Deserialize<ApiCredentials>(json);

            if (storedCredentials is null)
            {
                return null;
            }

            var plainApiKey = _encryption.Decrypt(storedCredentials.ApiKey);

            if (string.IsNullOrEmpty(plainApiKey) && !string.IsNullOrEmpty(storedCredentials.ApiKey))
            {
                _logger.LogWarning(LogEventIds.Configuration.ApiKeyDecryptionFailed, DecryptionFailedLogMessage);
            }

            return storedCredentials with { ApiKey = plainApiKey };
        }
        catch (Exception exception)
        {
            _logger.LogError(LogEventIds.Configuration.CredentialStoreLoadFailed, exception, LoadFailedLogMessage, _storagePath);

            return null;
        }
    }

    /// <inheritdoc />
    public void SaveCredentials(ApiCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        var cipherText = string.IsNullOrEmpty(credentials.ApiKey)
            ? string.Empty
            : _encryption.Encrypt(credentials.ApiKey);

        // 🔒 Section 8.3 (OWASP A10:2025): A failed encryption attempt must never result in an empty
        // or unencrypted credential payload being written to persistent storage.
        if (!string.IsNullOrEmpty(credentials.ApiKey) && string.IsNullOrEmpty(cipherText))
        {
            throw new InvalidOperationException(EncryptionFailedExceptionMessage);
        }

        var json = JsonSerializer.Serialize(credentials with { ApiKey = cipherText });

        _fileSystem.WriteAllText(_storagePath, json);
    }


    // ═══════════════════════════════════════════════════════
    //  9. Nested Types
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Reads the two length-prefixed strings of the legacy binary credential format.
    /// </summary>
    /// <remarks>
    /// 🔒 Security Guidelines Section 5.8 (Unbounded stream consumption):
    /// <c>BinaryReader.ReadString()</c> reads a 7-bit encoded length up to <see cref="int.MaxValue"/>
    /// and allocates memory accordingly. A malformed binary file could induce an Out-Of-Memory condition.
    /// Setting a 64 KB size limit enforces bounded allocations without operational overhead.
    /// </remarks>
    private static class LegacyCredentialFileReader
    {
        private const long MaximumLegacyFileSizeBytes = 64 * 1024;

        /// <summary>
        /// Reads credentials from the provided binary stream up to the maximum permitted file size.
        /// </summary>
        /// <param name="stream">The readable binary stream containing legacy credentials.</param>
        /// <returns>The deserialized <see cref="ApiCredentials"/> if valid; otherwise, <see langword="null"/>.</returns>
        public static ApiCredentials? Read(Stream stream)
        {
            if (stream.CanSeek && stream.Length > MaximumLegacyFileSizeBytes)
            {
                return null;
            }

            using var reader = new BinaryReader(stream);

            var username = reader.ReadString();
            var apiKey = reader.ReadString();

            return new ApiCredentials(username, apiKey);
        }
    }
}
