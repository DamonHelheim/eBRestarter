namespace eBRestarter.Core.Application.Interfaces;

public interface IApplicationLifetimeUseCase
{
    void ExitApplication(int exitCode);
}
