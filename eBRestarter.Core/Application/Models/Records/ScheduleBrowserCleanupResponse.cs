namespace eBRestarter.Core.Application.Models.Records;

public sealed record ScheduleBrowserCleanupResponse(bool IsActive, DateTime? NextDate);

