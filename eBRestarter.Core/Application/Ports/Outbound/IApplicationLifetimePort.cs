namespace eBRestarter.Core.Application.Ports.Outbound;

public interface IApplicationLifetimePort
{
    void ExitApplication(int exitCode);
}
