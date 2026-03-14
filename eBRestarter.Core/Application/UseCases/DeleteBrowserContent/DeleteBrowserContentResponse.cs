namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

public record DeleteBrowserContentResponse(bool Success, bool ProcessConflict, string ErrorMessage);
