namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record NetworkStats(string Name, long BytesReceived, long BytesSent, bool IsActive);
