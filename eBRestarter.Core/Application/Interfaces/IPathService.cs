namespace eBRestarter.Core.Application.Interfaces;

public interface IPathService
{
    string GetAppDataPath();
    string GetDownloadsPath();
    string GetConfigFilePath();
    string GetLogFilePath();
}
