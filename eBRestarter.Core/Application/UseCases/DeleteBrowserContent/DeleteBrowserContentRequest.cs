using eBRestarter.Core.Application.Enums;
namespace eBRestarter.Core.Application.UseCases.DeleteBrowserContent;

public record DeleteBrowserContentRequest(
    BrowserType BrowserType,
    bool DeleteCookies,
    bool DeleteCache,
    bool ForceCloseProcess);
