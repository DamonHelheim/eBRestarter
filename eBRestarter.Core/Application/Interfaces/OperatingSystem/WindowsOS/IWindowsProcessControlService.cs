namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

public interface IWindowsProcessControlService
{
    void StartMsiFile(string msiFilePath);
    void StartExecutable(string exeFilePath);
    Task StartExecutableAsync(string exeFilePath);

    /// <summary>
    /// Öffnet den Windows Explorer an einem bestimmten Pfad.
    /// </summary>
    void OpenExplorer(string folderPath);

    void OpenUrlInBrowser(string exeFilePath, string arguments);
    void CloseApplication(string processName);
    Task CloseAllOpenProgramsAsync(int timeoutMilliseconds);
    void ShutdownComputer();
    bool IsProcessAlive(string processName);
}
