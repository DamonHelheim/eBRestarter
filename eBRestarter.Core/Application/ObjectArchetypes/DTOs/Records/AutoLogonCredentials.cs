namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Windows account credentials used to configure AutoLogon.
/// </summary>
/// <param name="Username">The Windows account username.</param>
/// <param name="Domain">The Windows domain name or local machine name.</param>
/// <param name="Password">The secret Windows account password.</param>
/// <remarks>
/// Overrides <see cref="object.ToString"/> to mask the sensitive Windows password property, preventing accidental leakage in log outputs or diagnostic traces.
/// </remarks>
public sealed record AutoLogonCredentials(string Username, string Domain, string Password)
{
    /// <inheritdoc />
    // Sonar S2068 false positive: The string literal represents mask characters rather than cleartext password values.
#pragma warning disable S2068 // Hard-coded credentials are security-sensitive
    public override string ToString()
        => $"{nameof(AutoLogonCredentials)} {{ {nameof(Username)} = {Username}, {nameof(Domain)} = {Domain}, {nameof(Password)} = ***** }}";
#pragma warning restore S2068
}
