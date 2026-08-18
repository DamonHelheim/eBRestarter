using FluentValidation;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Infrastructure.BehavioralComponents.Validators;

/// <summary>
/// Validator for <see cref="ConfigureAutoLogonRequest"/>.
/// <para>
/// <strong>Architecture Classification: INFRASTRUCTURE HELPER / VALIDATOR</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Encapsulates FluentValidation rules for Windows AutoLogon configuration requests.<br/>
/// </para>
/// </summary>
public sealed class ConfigureAutoLogonValidator : AbstractValidator<ConfigureAutoLogonRequest>
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Types & Strings (alphabetical) ──
    private const string PasswordRequiredErrorMessage = "Password is required when activating AutoLogon.";
    private const string UsernameRequiredErrorMessage = "Username is required when activating AutoLogon.";


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="ConfigureAutoLogonValidator"/> with validation rules.
    /// </summary>
    public ConfigureAutoLogonValidator()
    {
        RuleFor(request => request.Username)
            .NotEmpty()
            .When(request => !request.IsDeactivateAction)
            .WithMessage(UsernameRequiredErrorMessage);

        RuleFor(request => request.Password)
            .NotEmpty()
            .When(request => !request.IsDeactivateAction)
            .WithMessage(PasswordRequiredErrorMessage);
    }
}
