namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public record ConfigureAutoLogonRequest(
    bool IsDeactivateAction,
    string? Username = null,
    string? Domain = null,
    string? Password = null
);