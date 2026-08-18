using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Interfaces.Authentication;

/// <summary>
/// Port: Driven Port (Outbound) for securely persisting, loading, and removing API credentials (username/key) to/from storage.
/// <para>
/// <strong>Architectural Classification: OUTBOUND PORT (Driven Port / Credential Storage Repository)</strong><br/>
/// - <strong>Consumer:</strong> Located inside the Application Core (e.g., managing or clearing API authentication credentials).<br/>
/// - <strong>Implementer:</strong> Located in the Infrastructure Layer (<see cref="eBRestarter.Infrastructure.BehavioralComponents.Repositories.Authentication.JsonCredentialStoreRepository"/> via file system/JSON).<br/>
/// - <strong>Rationale:</strong> Encapsulates physical data access to encrypted credential persistence for the application core.
/// </para>
/// </summary>
public interface IOutboundPortCredentialStoreRepository
{
    /// <summary>
    /// Securely persists API credentials to storage.
    /// </summary>
    /// <param name="credentials">The credentials containing username and API key to persist.</param>
    void SaveCredentials(ApiCredentials credentials);

    /// <summary>
    /// Loads stored API credentials from storage, decrypting the sensitive key.
    /// </summary>
    /// <returns>The decrypted <see cref="ApiCredentials"/> if stored; otherwise, <see langword="null"/>.</returns>
    ApiCredentials? LoadCredentials();

    /// <summary>
    /// Deletes stored credentials from storage.
    /// </summary>
    void ClearCredentials();

    /// <summary>
    /// Imports credentials from a legacy binary file for migration.
    /// </summary>
    /// <param name="filePath">The file path to the legacy binary credentials file.</param>
    /// <returns>The imported <see cref="ApiCredentials"/> if valid; otherwise, <see langword="null"/>.</returns>
    ApiCredentials? ImportFromLegacyFile(string filePath);
}
