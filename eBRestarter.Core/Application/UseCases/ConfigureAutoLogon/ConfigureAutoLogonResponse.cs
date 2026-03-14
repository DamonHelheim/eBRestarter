namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public record ConfigureAutoLogonResponse(
    bool Success,
    AutoLogonResultStatus Status,
    string ErrorMessage = ""
);