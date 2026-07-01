using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.Ports.Outbound.Browser;

public interface IBrowserFactoryOutboundPort
{
    IBrowserOutboundPort Create(BrowserType type);
}



