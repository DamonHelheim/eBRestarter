namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

public sealed record ConfigureAutoLogonRequest(
    bool IsDeactivateAction,
    bool DisablePasswordlessMode = false,
    bool RestorePasswordlessMode = false,
    string? Username = null,
    string? Domain = null,
    string? Password = null
);

