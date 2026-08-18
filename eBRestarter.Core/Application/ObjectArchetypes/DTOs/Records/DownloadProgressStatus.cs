namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Represents the progress state of an ongoing file download operation.
/// </summary>
/// <param name="BytesReceived">The number of bytes received so far.</param>
/// <param name="TotalBytes">The total size of the download payload in bytes.</param>
/// <param name="Percentage">The completion percentage of the download.</param>
public sealed record DownloadProgressStatus(long BytesReceived, long TotalBytes, double Percentage);
