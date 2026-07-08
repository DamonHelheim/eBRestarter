using eBRestarter.Core.Application.Enums;

namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record DeleteBrowserContentRequest(
    BrowserType BrowserType,
    bool DeleteCookies,
    bool DeleteCache,
    bool ForceCloseProcess);

