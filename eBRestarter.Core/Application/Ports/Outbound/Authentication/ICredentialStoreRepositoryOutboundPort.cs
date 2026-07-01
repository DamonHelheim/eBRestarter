using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Outbound.Authentication;

public interface ICredentialStoreRepositoryOutboundPort
{
    /// <summary>
    /// Securely saves the username and key.
    /// </summary>
    void SaveCredentials(ApiCredentials credentials);

    /// <summary>
    /// Loads the stored data (if available).
    /// </summary>
    ApiCredentials? LoadCredentials();

    /// <summary>
    /// Deletes the data (reset).
    /// </summary>
    void ClearCredentials();

    /// <summary>
    /// Imports credentials from a legacy file (used for migration).
    /// </summary>
    ApiCredentials? ImportFromLegacyFile(string filePath);
}


