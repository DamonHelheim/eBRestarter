using eBRestarter.Core.Application.Interfaces;

namespace eBRestarter.Infrastructure.Services.WindowsOS;

public class WindowsApplicationLifetime : IApplicationLifetime
{
    public void ExitApplication(int exitCode)
    {
        Environment.Exit(exitCode);
    }
}
