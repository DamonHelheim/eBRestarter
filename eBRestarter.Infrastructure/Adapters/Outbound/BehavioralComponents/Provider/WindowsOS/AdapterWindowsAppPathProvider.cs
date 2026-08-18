using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for resolving application-specific Windows file system paths.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Resolves OS-specific directory paths in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortAppPathProvider"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterWindowsAppPathProvider : IOutboundPortAppPathProvider
{
    /// <summary>Retrieves the roaming application data directory path (AppData\Roaming).</summary>
    public string RetrieveAppDataDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    /// <summary>Retrieves the local application data directory path (AppData\Local).</summary>
    public string RetrieveLocalAppDataDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    /// <summary>Retrieves the user profile directory path.</summary>
    public string RetrieveUserProfileDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    /// <summary>Retrieves the 64-bit Program Files directory path.</summary>
    public string RetrieveProgramFilesDirectory() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    /// <summary>Retrieves the 32-bit Program Files (x86) directory path.</summary>
    public string RetrieveProgramFilesX86Directory() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
}





