using eBRestarter.Core.Application.ObjectArchetypes.Enums;

namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Request parameters for executing browser content deletion.
/// </summary>
/// <param name="BrowserType">The targeted browser application type.</param>
/// <param name="DeleteCookies">Indicates whether stored browser cookies should be deleted.</param>
/// <param name="DeleteCache">Indicates whether browser cache files should be deleted.</param>
/// <param name="ForceCloseProcess">Indicates whether running browser processes should be forcefully closed before deletion.</param>
public sealed record DeleteBrowserContentRequest(
    BrowserType BrowserType,
    bool DeleteCookies,
    bool DeleteCache,
    bool ForceCloseProcess);

