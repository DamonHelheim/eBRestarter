namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Represents the result of toggling Microsoft Edge startup boost registry settings.
/// </summary>
/// <param name="Success">Indicates whether the registry toggle operation succeeded.</param>
/// <param name="NewState">The updated startup boost activation state.</param>
/// <param name="ErrorMessage">An optional error message explaining any toggle failure.</param>
public sealed record ToggleEdgeStartupBoostResponse(bool Success, bool NewState, string ErrorMessage = "");

