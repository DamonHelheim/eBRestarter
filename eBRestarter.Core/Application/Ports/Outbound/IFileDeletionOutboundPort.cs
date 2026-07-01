namespace eBRestarter.Core.Application.Ports.Outbound;

public interface IFileDeletionOutboundPort
{
    // Even though it has three methods (counting, deleting, single deletion), all three are highly coherent
    // and focused on a single automated purpose ("destroy files"). Remains a technical HANDLER.

    /// <summary>
    /// Recursively counts files within the specified directories.
    /// </summary>
    Task<int> CountFilesAsync(List<string> directories);

    /// <summary>
    /// Deletes files within the specified directories and reports progress.
    /// </summary>
    Task DeleteFilesAsync(List<string> directories, IProgress<string> statusReporter, IProgress<int> valueReporter, CancellationToken token);

    /// <summary>
    /// Deletes a single file (e.g., for Firefox cookies).
    /// </summary>
    void DeleteSingleFile(string filePath);
}
