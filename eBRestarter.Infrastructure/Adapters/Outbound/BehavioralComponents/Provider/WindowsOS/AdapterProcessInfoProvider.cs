using eBRestarter.Core.Application.Ports.Outbound.Interfaces.OperatingSystem;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Provider.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Provider) for retrieving current running process information.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter / Provider)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Retrieves process execution metadata in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortProcessInfoProvider"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterProcessInfoProvider : IOutboundPortProcessInfoProvider
{
    /// <summary>
    /// Returns the file path of the currently executing application process.
    /// </summary>
    public string GetCurrentExecutablePath() => Environment.ProcessPath!;
}
