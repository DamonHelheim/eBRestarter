using System.Threading.Tasks;

namespace eBRestarter.Core.Application.Ports.Inbound.UseCases.InitializeBrowserCleanup;

public interface IInitializeBrowserCleanupUseCase
{
    Task ExecuteAsync();
}
