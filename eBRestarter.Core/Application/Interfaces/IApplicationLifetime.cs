namespace eBRestarter.Core.Application.Interfaces;

public interface IApplicationLifetime
{
    void ExitApplication(int exitCode);
}
