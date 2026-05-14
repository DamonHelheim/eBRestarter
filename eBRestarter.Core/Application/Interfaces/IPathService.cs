namespace eBRestarter.Core.Application.Interfaces;

public interface IPathService
{
    string RetrieveAppDataPath();
    string RetrieveDownloadsPath();
    string RetrieveConfigFilePath();
    string RetrieveLogFilePath();
}
