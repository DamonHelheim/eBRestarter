using eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;
using FluentValidation;

namespace eBRestarter.Infrastructure.Adapters.Validators;

/// <summary>
/// Validierungsregel für <see cref="DeleteBrowserContentRequest"/>.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): KEIN ADAPTER (Fall A - Infrastruktur-Hilfsklasse)</strong><br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 1.1 des Leitfadens ist diese Klasse <strong>kein Adapter</strong>, da sie kein Port-Interface aus dem Application Core implementiert. Sie erbt lediglich von <see cref="AbstractValidator{T}"/> und definiert interne Validierungsregeln für die FluentValidation-Bibliothek.<br/>
/// - <strong>Aktion:</strong> Wurde aus <c>Infrastructure/Adapters/Validators</c> in den Ordner <c>Infrastructure/Validators</c> verschoben.
/// </para>
/// </summary>
public sealed class DeleteBrowserContentValidator : AbstractValidator<DeleteBrowserContentRequest>
{
    public DeleteBrowserContentValidator()
    {
        RuleFor(x => x.BrowserType)
            .IsInEnum()
            .WithMessage("A valid browser type must be selected.");

        RuleFor(x => x)
            .Must(x => x.DeleteCache || x.DeleteCookies)
            .WithMessage("At least one deletion option (Cache or Cookies) must be selected.");
    }
}
