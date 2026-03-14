namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

public record DeleteBrowserContentRequest(
    eBRestarter.Core.Domain.Enums.BrowserType BrowserType,
    bool DeleteCookies,
    bool DeleteCache,
    bool ForceCloseProcess);
