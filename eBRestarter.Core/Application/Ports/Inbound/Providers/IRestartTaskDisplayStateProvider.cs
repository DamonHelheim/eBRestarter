using eBRestarter.Core.Application.Models;
using eBRestarter.Core.Domain.Entities;

namespace eBRestarter.Core.Application.Ports.Inbound.Providers;

/// <summary>
/// Port: Generates the initial display state for the restart task UI from configuration and localization.
/// </summary>
public interface IRestartTaskDisplayStateProvider
{
    /// <summary>
    /// Builds the display DTO from the current configuration (including cache deletion status and formatted texts).
    /// </summary>
    RestartTaskDisplayState RetrieveInitialState(AppConfig config);
}