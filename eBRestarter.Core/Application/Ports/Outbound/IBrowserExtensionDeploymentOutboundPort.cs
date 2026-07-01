namespace eBRestarter.Core.Application.Ports.Outbound;

public interface IBrowserExtensionDeploymentOutboundPort
{
    // Primarily executes the automated background setup action EnsureExtensionIsDeployed(). Can remain declared as a HANDLER.

    /// <summary>
    /// Ensures that the Chrome extension is located in the correct target directory (e.g., AppData).
    /// Copies the files if they do not exist there yet.
    /// </summary>
    void EnsureExtensionIsDeployed();
}
