using System.Threading.Tasks;

namespace eBRestarter.Core.Application.UseCases.GetSystemInformation;

public interface IGetSystemInformationUseCase
{
    Task<SystemInformationResponse> ExecuteAsync();
}
