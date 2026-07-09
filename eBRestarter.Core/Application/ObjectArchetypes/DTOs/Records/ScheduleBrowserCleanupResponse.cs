namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record ScheduleBrowserCleanupResponse(bool IsActive, DateTime? NextDate);

