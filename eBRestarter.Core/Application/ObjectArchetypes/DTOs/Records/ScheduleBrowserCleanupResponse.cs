namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Represents the result of querying or configuring scheduled browser cleanup status.
/// </summary>
/// <param name="IsActive">Indicates whether automatic browser cleanup is enabled.</param>
/// <param name="NextDate">The scheduled date and time of the next automatic cleanup, or <c>null</c> if disabled.</param>
public sealed record ScheduleBrowserCleanupResponse(bool IsActive, DateTime? NextDate);

