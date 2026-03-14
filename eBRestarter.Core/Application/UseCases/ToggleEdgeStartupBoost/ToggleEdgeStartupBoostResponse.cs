namespace eBRestarter.Core.Application.UseCases.ToggleEdgeStartupBoost;

public record ToggleEdgeStartupBoostResponse(bool Success, bool NewState, string ErrorMessage = "");
