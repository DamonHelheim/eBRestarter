namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ScheduleBrowserCleanup;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public interface IScheduleBrowserCleanupUseCase
{
    ScheduleBrowserCleanupResponse UpdateSchedule(ScheduleBrowserCleanupRequest request);
}

