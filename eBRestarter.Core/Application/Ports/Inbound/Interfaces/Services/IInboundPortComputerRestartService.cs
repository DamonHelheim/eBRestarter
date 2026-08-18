using System;
using System.Threading.Tasks;

namespace eBRestarter.Core.Application.Ports.Inbound.Interfaces.Services;

/// <summary>
/// Port: Manages the background computer restart schedule and timeline notifications for the presentation layer.
/// </summary>
/// <remarks>
/// <b>Architectural Classification: INBOUND PORT (System-Trigger / Application Service Port)</b>
/// <list type="bullet">
/// <item><b>Consumer:</b> Presentation layer view models.</item>
/// <item><b>Implementer:</b> Application Core (<see cref="Application.BehavioralComponents.Services.ComputerRestartService"/>).</item>
/// <item><b>Rationale:</b> Entry point into the core application to start/stop the computer restart scheduler and listen for date changes.</item>
/// </list>
/// </remarks>
public interface IInboundPortComputerRestartService
{
    /// <summary>
    /// Occurs when the scheduled next restart date is updated or recalculated.
    /// </summary>
    event EventHandler<DateTime?>? NextRestartDateChanged;

    /// <summary>
    /// Starts the background restart scheduler timer loop.
    /// </summary>
    void StartScheduler();

    /// <summary>
    /// Stops the background restart scheduler gracefully.
    /// </summary>
    Task StopSchedulerAsync();
}