using FluentValidation;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Infrastructure.BehavioralComponents.Validators;

/// <summary>
/// Validator for <see cref="DeleteBrowserContentRequest"/>.
/// <para>
/// <strong>Architecture Classification: INFRASTRUCTURE HELPER / VALIDATOR</strong><br/>
/// - <strong>Role &amp; Responsibility:</strong> Encapsulates FluentValidation rules for browser cache and cookie deletion requests.<br/>
/// </para>
/// </summary>
public sealed class DeleteBrowserContentValidator : AbstractValidator<DeleteBrowserContentRequest>
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Types & Strings (alphabetical) ──
    private const string BrowserTypeRequiredErrorMessage = "A valid browser type must be selected.";
    private const string DeletionOptionRequiredErrorMessage = "At least one deletion option (Cache or Cookies) must be selected.";


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteBrowserContentValidator"/> with validation rules.
    /// </summary>
    public DeleteBrowserContentValidator()
    {
        RuleFor(request => request.BrowserType)
            .IsInEnum()
            .WithMessage(BrowserTypeRequiredErrorMessage);

        RuleFor(request => request)
            .Must(request => request.DeleteCache || request.DeleteCookies)
            .WithMessage(DeletionOptionRequiredErrorMessage);
    }
}
