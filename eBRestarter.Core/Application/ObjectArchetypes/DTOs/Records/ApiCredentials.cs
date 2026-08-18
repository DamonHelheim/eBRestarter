namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// eBesucher API credentials.
/// </summary>
/// <param name="Username">The eBesucher account username.</param>
/// <param name="ApiKey">The secret eBesucher API key.</param>
/// <remarks>
/// Overrides <see cref="object.ToString"/> to mask the sensitive API key property, preventing accidental leakage in log outputs or diagnostic traces.
/// </remarks>
public sealed record ApiCredentials(string Username, string ApiKey)
{
    /// <inheritdoc />
    public override string ToString() => $"{nameof(ApiCredentials)} {{ {nameof(Username)} = {Username}, {nameof(ApiKey)} = ***** }}";
}
