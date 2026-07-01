namespace eBRestarter.Core.Application.Ports.Inbound.Providers;

public interface IOsPathProvider
{
    string RetrieveAppDataPath();
    string RetrieveDownloadsPath();
    string RetrieveConfigFilePath();
    string RetrieveLogFilePath();
}
