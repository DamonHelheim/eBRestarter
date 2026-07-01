namespace eBRestarter.Core.Application.Ports.Outbound;

public interface IApplicationLifetimeOutboundPort
{
    void ExitApplication(int exitCode);
}
