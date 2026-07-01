namespace eBRestarter.Core.Application.Ports.Outbound.Application;

/// <summary>
/// Provides basic information about the application.
/// </summary>
public interface IAppVersionInfoProviderOutboundPort
{
    /// <summary>
    /// Returns the current version number of the application (e.g., "2.0.1.0").
    /// </summary>
    string RetrieveAppVersion();
}

