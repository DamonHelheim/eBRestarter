using FluentValidation;

using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

namespace eBRestarter.Infrastructure.BehavioralComponents.Validators;

/// <summary>
/// Validierungsregel für <see cref="ManageRestarterCycleRequest"/>.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): KEIN ADAPTER (Fall A - Infrastruktur-Hilfsklasse)</strong><br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 1.1 des Leitfadens ist diese Klasse <strong>kein Adapter</strong>, da sie kein Port-Interface aus dem Application Core implementiert. Sie erbt lediglich von <see cref="AbstractValidator{T}"/> und definiert interne Validierungsregeln für die FluentValidation-Bibliothek.<br/>
/// - <strong>Aktion:</strong> Wurde aus <c>Infrastructure/Adapters/Validators</c> in den Ordner <c>Infrastructure/BehavioralComponents/Validators</c> verschoben.
/// </para>
/// </summary>
public sealed class ManageRestarterCycleValidator : AbstractValidator<ManageRestarterCycleRequest>
{
    // ═══════════════════════════════════════════════════════
    //  1. Constants
    // ═══════════════════════════════════════════════════════

    // ── Block 2: Primitive Typen & Strings (alphabetisch) ──
    private const string BrowserTypeRequiredErrorMessage = "A valid browser type must be selected.";
    private const string PauseTimeCannotBeNegativeErrorMessage = "Pause time cannot be negative.";
    private const string RuntimeGreaterThanZeroErrorMessage = "Runtime must be greater than 0 seconds.";


    // ═══════════════════════════════════════════════════════
    //  6. Constructors
    // ═══════════════════════════════════════════════════════

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
    }
}
