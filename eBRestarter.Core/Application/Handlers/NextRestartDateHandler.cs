using eBRestarter.Core.Application.Ports.Inbound.Handlers;
using eBRestarter.Core.Domain.Handlers;

namespace eBRestarter.Core.Application.Services;

public class NextRestartDateHandler(IRestartCalculationHandler restartCalculationHandler) : INextRestartDateHandler
{
    private readonly IRestartCalculationHandler _restartCalculation = restartCalculationHandler;

    /// <inheritdoc />
    public DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime) => _restartCalculation.RetrieveNextRestartDate(intervalDays, restartClockTime);
}

