using eBRestarter.Core.Application.Ports.Inbound.Handlers;
using eBRestarter.Core.Domain.Handlers;

namespace eBRestarter.Core.Application.Handlers;

public class NextRestartDateHandler(IRestartCalculationHandler restartCalculationHandler) : IInboundPortNextRestartDateHandler
{
    private readonly IRestartCalculationHandler _restartCalculation = restartCalculationHandler;

    /// <inheritdoc />
    public DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime) => _restartCalculation.RetrieveNextRestartDate(intervalDays, restartClockTime);
}

