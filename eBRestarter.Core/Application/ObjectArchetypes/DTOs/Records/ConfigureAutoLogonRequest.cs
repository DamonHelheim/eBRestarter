namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Request parameters for activating or deactivating Windows AutoLogon.
/// </summary>
/// <param name="IsDeactivateAction">Indicates whether the operation is a deactivation request.</param>
/// <param name="DisablePasswordlessMode">Indicates whether Windows passwordless sign-in mode should be disabled during activation.</param>
/// <param name="RestorePasswordlessMode">Indicates whether Windows passwordless sign-in mode should be restored during deactivation.</param>
/// <param name="Username">The optional Windows account username for activation.</param>
/// <param name="Domain">The optional Windows domain name or local machine name for activation.</param>
/// <param name="Password">The optional secret Windows account password for activation.</param>
/// <remarks>
/// Overrides <see cref="object.ToString"/> to mask the sensitive Windows password property, preventing accidental leakage in log outputs or diagnostic traces.
/// </remarks>
public sealed record ConfigureAutoLogonRequest(
    bool IsDeactivateAction,
    bool DisablePasswordlessMode = false,
    bool RestorePasswordlessMode = false,
    string? Username = null,
    string? Domain = null,
    string? Password = null
)
{
    /// <inheritdoc />
    // Sonar S2068 false positive: The string literal represents mask characters rather than cleartext password values.
#pragma warning disable S2068 // Hard-coded credentials are security-sensitive
    public override string ToString()
        => $"{nameof(ConfigureAutoLogonRequest)} {{ {nameof(IsDeactivateAction)} = {IsDeactivateAction}, "
           + $"{nameof(DisablePasswordlessMode)} = {DisablePasswordlessMode}, "
           + $"{nameof(RestorePasswordlessMode)} = {RestorePasswordlessMode}, "
           + $"{nameof(Username)} = {Username}, {nameof(Domain)} = {Domain}, {nameof(Password)} = ***** }}";
#pragma warning restore S2068
}
