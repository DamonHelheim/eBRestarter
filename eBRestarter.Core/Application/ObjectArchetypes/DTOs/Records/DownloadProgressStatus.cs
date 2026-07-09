namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record DownloadProgressStatus(long BytesReceived, long TotalBytes, double Percentage);
