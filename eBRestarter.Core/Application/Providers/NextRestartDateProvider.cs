using eBRestarter.Core.Application.Ports.Inbound.Providers;
using eBRestarter.Core.Domain.Handlers;

namespace eBRestarter.Core.Application.Providers;

public class NextRestartDateProvider(IRestartCalculationHandler restartCalculationHandler) : INextRestartDateProvider
{
    private readonly IRestartCalculationHandler _restartCalculation = restartCalculationHandler;

    /// <inheritdoc />
    public DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime) => _restartCalculation.RetrieveNextRestartDate(intervalDays, restartClockTime);
}

