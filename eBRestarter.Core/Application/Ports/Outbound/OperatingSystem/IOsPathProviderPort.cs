namespace eBRestarter.Core.Application.Ports.Outbound.OperatingSystem;

public interface IOsPathProviderPort
{
    string RetrieveAppDataPath();
    string RetrieveDownloadsPath();
    string RetrieveConfigFilePath();
    string RetrieveLogFilePath();
}



