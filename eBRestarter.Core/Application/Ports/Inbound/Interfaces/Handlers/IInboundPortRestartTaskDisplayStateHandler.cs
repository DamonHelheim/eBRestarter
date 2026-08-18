using eBRestarter.Core.Application.ObjectArchetypes.DTOs.ImmutableSnapshot;
using eBRestarter.Core.Domain.ValueObjects;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;

/// <summary>
/// Port: Generates the initial display state for the restart task UI from configuration and localization.
/// </summary>
/// <remarks>
/// <b>Architectural Classification: INBOUND PORT / USE CASE INTERFACE</b>
/// <list type="bullet">
/// <item><b>Consumer:</b> Presentation layer (<see cref="eBRestarter.Desktop.WinUI3.ViewModels.ViewModelRestartTask"/> via MVVM).</item>
/// <item><b>Implementer:</b> Application Core (<see cref="BehavioralComponents.Handlers.RestartTaskDisplayStateHandler"/>).</item>
/// <item><b>Rationale:</b> Serves as an entry point into the application core to prepare the initial display state for the UI.</item>
/// </list>
/// </remarks>
public interface IInboundPortRestartTaskDisplayStateHandler
{
    /// <summary>
    /// Builds the display DTO from the current configuration (including cache deletion status and formatted texts).
    /// </summary>
    /// <param name="config">The current application configuration.</param>
    /// <returns>An immutable <see cref="RestartTaskDisplayState"/> containing prepared display values.</returns>
    RestartTaskDisplayState RetrieveInitialState(AppConfig config);
}