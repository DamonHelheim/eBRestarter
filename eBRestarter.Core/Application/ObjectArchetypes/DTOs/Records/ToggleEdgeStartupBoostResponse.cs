namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record ToggleEdgeStartupBoostResponse(bool Success, bool NewState, string ErrorMessage = "");

