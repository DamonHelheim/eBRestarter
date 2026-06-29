namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.ManageApplicationUpdates;

using eBRestarter.Core.Application.Models.Records;
using eBRestarter.Core.Application.Enums;
using eBRestarter.Core.Application.Models.Errors;

public interface IManageApplicationUpdatesUseCase
{
    Task<CheckUpdateResponse> CheckForUpdatesAsync();
    Task PerformUpdateAsync();
}

