using eBRestarter.Core.Application.Interfaces;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

public class WindowsApplicationLifetimeHandler : IApplicationLifetimeUseCase
{
    public WindowsApplicationLifetimeHandler()
    {
    }

    public void ExitApplication(int exitCode)
    {
        Environment.Exit(exitCode);
    }
}
