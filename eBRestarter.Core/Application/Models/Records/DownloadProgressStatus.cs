namespace eBRestarter.Core.Application.Models.Records;

public record DownloadProgressStatus(long BytesReceived, long TotalBytes, double Percentage);
