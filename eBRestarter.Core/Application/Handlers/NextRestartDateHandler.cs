using eBRestarter.Core.Application.Ports.Inbound.Interfaces.Handlers;
using eBRestarter.Core.Domain.Handlers;

namespace eBRestarter.Core.Application.Handlers;

public class NextRestartDateHandler(IRestartCalculationHandler restartCalculationHandler) : IInboundPortNextRestartDateHandler
{
    private readonly IRestartCalculationHandler _restartCalculation = restartCalculationHandler;

    public DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime) => _restartCalculation.RetrieveNextRestartDate(intervalDays, restartClockTime);
}

