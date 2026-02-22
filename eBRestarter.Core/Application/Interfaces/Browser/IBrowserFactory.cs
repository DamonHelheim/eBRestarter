using eBRestarter.Core.Domain.Enums;

namespace eBRestarter.Core.Application.Interfaces.Browser;

//(Wie erstelle ich einen?)
public interface IBrowserFactory
{
    IBrowser Create(BrowserType type);
}
