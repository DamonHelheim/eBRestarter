using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ScheduleBrowserCleanup;

public interface IScheduleBrowserCleanupUseCase
{
    ScheduleBrowserCleanupResponse UpdateSchedule(ScheduleBrowserCleanupRequest request);
}

