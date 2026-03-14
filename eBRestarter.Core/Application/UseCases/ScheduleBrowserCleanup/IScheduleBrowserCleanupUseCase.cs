namespace eBRestarter.Core.Application.UseCases.ScheduleBrowserCleanup;

public interface IScheduleBrowserCleanupUseCase
{
    ScheduleBrowserCleanupResponse UpdateSchedule(ScheduleBrowserCleanupRequest request);
}
