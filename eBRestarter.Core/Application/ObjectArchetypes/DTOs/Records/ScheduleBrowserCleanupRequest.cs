namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Request parameters for scheduling automatic browser data cleanup interval.
/// </summary>
/// <param name="IntervalDays">The number of days between automatic cleanup executions.</param>
public sealed record ScheduleBrowserCleanupRequest(int IntervalDays);

