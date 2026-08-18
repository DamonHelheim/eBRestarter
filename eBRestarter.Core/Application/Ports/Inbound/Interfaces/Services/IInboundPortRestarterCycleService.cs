using System;
using System.Threading.Tasks;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;

/// <summary>
/// Port: Controls the execution lifecycle of the browser restarting cycle from the UI/presentation layer.
/// </summary>
/// <remarks>
/// <b>Architectural Classification: INBOUND PORT (System-Trigger / Application Service Port)</b>
/// <list type="bullet">
/// <item><b>Consumer:</b> Presentation layer view models via MVVM (<c>ViewModelRestartTask</c>).</item>
/// <item><b>Implementer:</b> Application Core (<see cref="Application.BehavioralComponents.Services.RestarterCycleService"/>).</item>
/// <item><b>Rationale:</b> Entry point into the core application for starting, stopping, and monitoring the main restarter cycle execution.</item>
/// </list>
/// </remarks>
public interface IInboundPortRestarterCycleService
{
    /// <summary>
    /// Occurs when the restarter cycle progress or state changes.
    /// </summary>
    event EventHandler<RestarterCycleProgress> ProgressChanged;

    /// <summary>
    /// Starts the restarter cycle execution loop asynchronously.
    /// </summary>
    /// <param name="request">The cycle configuration request parameters.</param>
    /// <param name="performCleanupCallback">Callback delegate invoked when browser cache cleanup is due.</param>
    Task StartAsync(ManageRestarterCycleRequest request, Func<Task> performCleanupCallback);

    /// <summary>
    /// Stops the currently active restarter cycle execution loop.
    /// </summary>
    void Stop();
}

