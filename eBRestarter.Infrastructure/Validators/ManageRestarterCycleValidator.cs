using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using FluentValidation;

namespace eBRestarter.Infrastructure.Adapters.Validators;

/// <summary>
/// Validierungsregel für <see cref="ManageRestarterCycleRequest"/>.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): KEIN ADAPTER (Fall A - Infrastruktur-Hilfsklasse)</strong><br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 1.1 des Leitfadens ist diese Klasse <strong>kein Adapter</strong>, da sie kein Port-Interface aus dem Application Core implementiert. Sie erbt lediglich von <see cref="AbstractValidator{T}"/> und definiert interne Validierungsregeln für die FluentValidation-Bibliothek.<br/>
/// - <strong>Aktion:</strong> Wurde aus <c>Infrastructure/Adapters/Validators</c> in den Ordner <c>Infrastructure/Validators</c> verschoben.
/// </para>
/// </summary>
public sealed class ManageRestarterCycleValidator : AbstractValidator<ManageRestarterCycleRequest>
{
    public ManageRestarterCycleValidator()
    {
        RuleFor(x => x.BrowserType).IsInEnum().WithMessage("A valid browser type must be selected.");

        RuleFor(x => x.RuntimeSeconds)
            .GreaterThan(0)
            .WithMessage("Runtime must be greater than 0 seconds.");

        RuleFor(x => x.PauseSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Pause time cannot be negative.");
    }
}
