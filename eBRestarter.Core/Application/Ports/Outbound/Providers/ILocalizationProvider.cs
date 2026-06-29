namespace eBRestarter.Core.Application.Ports.Outbound.Providers;

public interface ILocalizationProvider
{
    string RetrieveString(string key);
}
