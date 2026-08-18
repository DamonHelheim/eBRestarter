using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;

/// <summary>
/// Port: Exposes active network statistics and availability status to the presentation layer.
/// <para>
/// <strong>Architectural Classification: INBOUND PORT / USE CASE INTERFACE</strong><br/>
/// - <strong>Consumer:</strong> Located outside the Application Core (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelNetworkTraffic"/> in the Presentation Layer via MVVM).<br/>
/// - <strong>Implementer:</strong> Located in the Infrastructure Layer (<see cref="eBRestarter.Infrastructure.Providers.NetworkInfoProvider"/>).<br/>
/// - <strong>Rationale:</strong> Serves the user interface as an inbound port to query and display network traffic metrics.
/// </para>
/// </summary>
public interface IInboundPortNetworkInfoProvider
{
    /// <summary>
    /// Checks whether network connectivity is currently available.
    /// </summary>
    /// <returns><see langword="true"/> if network connectivity exists; otherwise, <see langword="false"/>.</returns>
    bool IsNetworkAvailable();

    /// <summary>
    /// Retrieves active, non-loopback network interfaces with recorded traffic statistics.
    /// </summary>
    /// <returns>An enumerable collection of active <see cref="NetworkStats"/>.</returns>
    IEnumerable<NetworkStats> RetrieveActiveInterfaces();
}
