namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Encapsulates the activation status of browser content deletion.
/// </summary>
/// <param name="IsActiveOrNot">Indicates whether browser content deletion is active.</param>
public sealed record DeleteBrowserContentIsActive(bool IsActiveOrNot);
