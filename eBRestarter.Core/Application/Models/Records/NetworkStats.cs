namespace eBRestarter.Core.Application.Models.Records;

public sealed record NetworkStats(string Name, long BytesReceived, long BytesSent, bool IsActive);
