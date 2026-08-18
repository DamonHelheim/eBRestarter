using FluentValidation;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using eBRestarter.Core.Domain.Validators;

namespace eBRestarter.Infrastructure.BehavioralComponents.Validators;

/// <summary>
/// Validator for <see cref="ManageRestarterCycleRequest"/>.
/// <para>
/// <strong>Architecture Classification: INFRASTRUCTURE HELPER / VALIDATOR</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Encapsulates FluentValidation rules for restarter cycle execution requests.<br/>
/// - <strong>Design Rationale:</strong> Implements <see cref="AbstractValidator{T}"/> for validation rules without implementing application port interfaces.<br/>
/// </para>
/// </summary>
public sealed class ManageRestarterCycleValidator : AbstractValidator<ManageRestarterCycleRequest>
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Types & Strings (alphabetical) ──
    private const string BrowserTypeRequiredErrorMessage = "A valid browser type must be selected.";
    private const string PauseTimeCannotBeNegativeErrorMessage = "Pause time cannot be negative.";
    private const string RuntimeGreaterThanZeroErrorMessage = "Runtime must be greater than 0 seconds.";
    private const string UsernameRequiredErrorMessage = "A username is required to start the restarter.";


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="ManageRestarterCycleValidator"/> with validation rules.
    /// </summary>
    public ManageRestarterCycleValidator()
    {
        RuleFor(request => request.BrowserType)
            .IsInEnum()
            .WithMessage(BrowserTypeRequiredErrorMessage);

        RuleFor(request => request.RuntimeSeconds)
            .GreaterThan(0)
            .WithMessage(RuntimeGreaterThanZeroErrorMessage);

        RuleFor(request => request.PauseSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage(PauseTimeCannotBeNegativeErrorMessage);

        // 🔒 Security Guidelines Section 5.1 (Whitelisting): Validates username formatting to prevent
        // command line switch injection when the username is passed into browser launch arguments.
        RuleFor(request => request.Username)
            .NotEmpty()
            .WithMessage(UsernameRequiredErrorMessage);

        RuleFor(request => request.Username)
            .Must(EVisitorUsernamePolicy.IsValid)
            .WithMessage(EVisitorUsernamePolicy.RuleDescription);
    }
}
