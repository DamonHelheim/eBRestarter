using System;

using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;
using eBRestarter.Core.Domain.Handlers;

namespace eBRestarter.Core.Application.BehavioralComponents.Handlers;

/// <summary>
/// Inbound port handler responsible for retrieving the next scheduled restart date.
/// </summary>
/// <param name="restartCalculationHandler">The domain calculation handler used for computing the next restart date.</param>
public sealed class NextRestartDateHandler(IRestartCalculationHandler restartCalculationHandler) : IInboundPortNextRestartDateHandler
{
    private readonly IRestartCalculationHandler _restartCalculationHandler = restartCalculationHandler ?? throw new ArgumentNullException(nameof(restartCalculationHandler));

    /// <inheritdoc />
    public DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime) =>
        _restartCalculationHandler.RetrieveNextRestartDate(intervalDays, restartClockTime);
}
