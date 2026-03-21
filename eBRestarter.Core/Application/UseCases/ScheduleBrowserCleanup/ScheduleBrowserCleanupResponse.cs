namespace eBRestarter.Core.Application.UseCases.ScheduleBrowserCleanup;

public record ScheduleBrowserCleanupResponse(bool IsActive, DateTime? NextDate);
