using System.Threading.Tasks;

namespace eBRestarter.Core.Application.UseCases.ManageApplicationUpdates;

public interface IManageApplicationUpdatesUseCase
{
    Task<CheckUpdateResponse> CheckForUpdatesAsync();
    Task PerformUpdateAsync();
}
