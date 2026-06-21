namespace eBRestarter.Core.Application.Interfaces;

public interface IPathUseCase
{
    string RetrieveAppDataPath();
    string RetrieveDownloadsPath();
    string RetrieveConfigFilePath();
    string RetrieveLogFilePath();
}
