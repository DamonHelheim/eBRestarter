using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Domain.Services;

namespace eBRestarter.Core.Application.Services;

public class RetrieveNextRestartDateUseCase(IRestartCalculationHandler restartCalculationHandler)
    : IRetrieveNextRestartDateUseCase
{
    private readonly IRestartCalculationHandler _restartCalculation = restartCalculationHandler;

    /// <inheritdoc />
    public DateTime RetrieveNextRestartDate(int intervalDays, int restartClockTime) =>
        _restartCalculation.RetrieveNextRestartDate(intervalDays, restartClockTime);
}
