using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Providers;

/// <summary>
/// Port: Retrieves aggregated system hardware, OS, and runtime information for display in the client UI.
/// </summary>
/// <remarks>
/// <b>Architectural Classification: INBOUND PORT / USE CASE INTERFACE</b>
/// <list type="bullet">
/// <item><b>Consumer:</b> Presentation layer (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelInfocenter"/> via MVVM).</item>
/// <item><b>Implementer:</b> Application Core (<see cref="BehavioralComponents.Providers.SystemInformationProvider"/>).</item>
/// <item><b>Rationale:</b> Serves as an entry point into the application core for the Infocenter view model to retrieve hardware and OS details.</item>
/// </list>
/// </remarks>
public interface IInboundPortSystemInformationProvider
{
    /// <summary>
    /// Retrieves aggregated hardware, OS edition, OS version, and browser information asynchronously.
    /// </summary>
    /// <returns>A <see cref="SystemInformationResponse"/> containing the aggregated system details.</returns>
    Task<SystemInformationResponse> RetrieveAsync();
}
