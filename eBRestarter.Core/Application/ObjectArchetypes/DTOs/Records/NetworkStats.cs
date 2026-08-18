namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Encapsulates traffic statistics for a network interface adapter.
/// </summary>
/// <param name="Name">The display name of the network interface.</param>
/// <param name="BytesReceived">The total number of bytes received.</param>
/// <param name="BytesSent">The total number of bytes transmitted.</param>
/// <param name="IsActive">Indicates whether the network interface is active.</param>
public sealed record NetworkStats(string Name, long BytesReceived, long BytesSent, bool IsActive);
