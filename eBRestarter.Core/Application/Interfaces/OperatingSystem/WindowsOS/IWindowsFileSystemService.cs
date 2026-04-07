namespace eBRestarter.Core.Application.Interfaces.OperatingSystem.WindowsOS;

public interface IWindowsFileSystemService
{
    bool FileExists(string path);
    void DeleteFile(string path);
    bool DirectoryExists(string path);
    string GetEnvironmentPath(string variable); // Z.B. für %AppData%
    string CombinePaths(params string[] paths);
    void WriteAllText(string path, string content);
    string ReadAllText(string path);
    string[] ReadAllLines(string path);
}
