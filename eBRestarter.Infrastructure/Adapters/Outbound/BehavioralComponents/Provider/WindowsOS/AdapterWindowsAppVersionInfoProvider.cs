using System.Reflection;
using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for reading Windows executable assembly version and build metadata.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Reads assembly version and metadata from the entry executable in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortAppVersionInfoProvider"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterWindowsAppVersionInfoProvider : IOutboundPortAppVersionInfoProvider
{
    private static readonly string FallbackVersion = new Version(1, 0, 0, 0).ToString();

    /// <summary>
    /// Retrieves the entry assembly version string, or fallback version if unavailable.
    /// </summary>
    public string RetrieveAppVersion() =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? FallbackVersion;
}
