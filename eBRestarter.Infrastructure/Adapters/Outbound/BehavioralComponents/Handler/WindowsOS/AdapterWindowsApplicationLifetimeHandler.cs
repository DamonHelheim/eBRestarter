using eBRestarter.Core.Application.Ports.Outbound.Interfaces.Application;

namespace eBRestarter.Infrastructure.Adapters.Outbound.BehavioralComponents.Handler.WindowsOS;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) managing Windows application shutdown and restart lifecycle.
/// <para>
/// <strong>Architecture Classification: OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Encapsulates OS application termination and restart process calls in the Infrastructure layer.<br/>
/// - <strong>Implemented Port:</strong> <see cref="IOutboundPortApplicationLifetime"/>.<br/>
/// </para>
/// </summary>
public sealed class AdapterWindowsApplicationLifetimeHandler : IOutboundPortApplicationLifetime
{
    /// <summary>
    /// Terminates the application process with the specified exit code.
    /// </summary>
    /// <param name="exitCode">The exit code to return to the operating system.</param>
    public void ExitApplication(int exitCode)
    {
        Environment.Exit(exitCode);
    }
}
