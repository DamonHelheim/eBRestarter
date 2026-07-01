namespace eBRestarter.Core.Application.Ports.Outbound;

public interface IBrowserExtensionPathProviderOutboundPort
{
    /// <summary>
    /// Returns the current path to the extension folder (Debug vs. Release).
    /// </summary>
    /// <returns>The absolute path to the extension folder.</returns>
    string RetrieveExtensionFolderPath();
}
