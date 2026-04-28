using eBRestarter.Core.Application.Interfaces;
using eBRestarter.Core.Domain.Services;

namespace eBRestarter.Core.Application.Services;

public class ComputerRestartDateService(IRestartCalculationService restartCalculationService)
    : IComputerRestartDateService
{
    private readonly IRestartCalculationService _restartCalculation = restartCalculationService;

    /// <inheritdoc />
    public DateTime GetNextRestartDate(int intervalDays, int restartClockTime) =>
        _restartCalculation.GetNextRestartDate(intervalDays, restartClockTime);
}
