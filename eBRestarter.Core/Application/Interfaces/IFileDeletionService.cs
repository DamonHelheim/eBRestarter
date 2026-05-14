namespace eBRestarter.Core.Application.Interfaces;

public interface IFileDeletionService
{
    // Zählt Dateien in einem Verzeichnis rekursiv
    Task<int> CountFilesAsync(List<string> directories);

    // Löscht Dateien und meldet Fortschritt
    Task DeleteFilesAsync(List<string> directories, IProgress<string> statusReporter, IProgress<int> valueReporter, CancellationToken token);

    // Einzelne Datei löschen (für Firefox Cookies)
    void DeleteSingleFile(string filePath);
}
