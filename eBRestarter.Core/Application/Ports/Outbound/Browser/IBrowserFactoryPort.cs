using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.Ports.Outbound.Browser;

public interface IBrowserFactoryPort
{
    IBrowserPort Create(BrowserType type);
}



