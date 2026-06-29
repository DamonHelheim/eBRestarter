namespace eBRestarter.Core.Application.Models.Records;

public sealed record ToggleEdgeStartupBoostResponse(bool Success, bool NewState, string ErrorMessage = "");

