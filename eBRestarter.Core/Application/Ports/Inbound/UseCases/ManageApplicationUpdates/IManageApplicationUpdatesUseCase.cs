using eBRestarter.Core.Application.Models.Records;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageApplicationUpdates;

public interface IManageApplicationUpdatesUseCase
{
    Task<CheckUpdateResponse> CheckForUpdatesAsync();
    Task PerformUpdateAsync();
}

