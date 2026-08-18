namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Represents the outcome of a validation or verification check.
/// </summary>
/// <param name="IsValid">Indicates whether verification succeeded.</param>
/// <param name="Message">The message explaining the verification outcome.</param>
public record VerificationResult(bool IsValid, string Message);

