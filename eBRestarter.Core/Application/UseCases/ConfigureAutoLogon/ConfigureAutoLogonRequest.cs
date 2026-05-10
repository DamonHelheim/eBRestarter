namespace eBRestarter.Core.Application.UseCases.ConfigureAutoLogon;

public record ConfigureAutoLogonRequest(
    bool IsDeactivateAction,
    bool DisablePasswordlessMode = false,
    bool RestorePasswordlessMode = false,
    string? Username = null,
    string? Domain = null,
    string? Password = null
);