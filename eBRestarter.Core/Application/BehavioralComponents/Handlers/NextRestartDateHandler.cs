using System;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;
using eBRestarter.Core.Domain.Handlers;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers;

/// <summary>
/// Inbound port handler responsible for retrieving the next scheduled restart date.
/// </summary>
public sealed class NextRestartDateHandler(IRestartCalculationHandler restartCalculationHandler) : IInboundPortNextRestartDateHandler
{
    // ═══════════════════════════════════════════════════════
    //  2. Fields
    // ═══════════════════════════════════════════════════════
    private readonly IRestartCalculationHandler _restartCalculationHandler = restartCalculationHandler ?? throw new ArgumentNullException(nameof(restartCalculationHandler));


    // ═══════════════════════════════════════════════════════
    //  8. Methods
    // ═══════════════════════════════════════════════════════
    /// <summary>
    /// Calculates and retrieves the next restart date based on interval days and clock time.
    /// </summary>
    /// <param name="intervalDays">The interval in days between restarts.</param>
    /// <param name="restartClockTime">The target clock time for the restart.</param>
    /// <returns>The calculated next <see cref="DateTime"/> for the restart.</returns>
    public DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime) =>
        _restartCalculationHandler.RetrieveNextRestartDate(intervalDays, restartClockTime);
}
