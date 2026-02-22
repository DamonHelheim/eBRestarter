namespace eBRestarter.Core.Domain.Models.Records;

// Einfaches DTO für den Datentransport vom Service zur UI
public record NetworkStats(string Name, long BytesReceived, long BytesSent, bool IsActive);
