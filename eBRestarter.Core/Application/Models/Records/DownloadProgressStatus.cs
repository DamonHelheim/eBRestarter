namespace eBRestarter.Core.Application.Models.Records;

public sealed record DownloadProgressStatus(long BytesReceived, long TotalBytes, double Percentage);
