using eBRestarter.Core.Application.Models.Records;
using FluentValidation;

namespace eBRestarter.Infrastructure.Adapters.Validators;

/// <summary>
/// Validierungsregel für <see cref="ConfigureAutoLogonRequest"/>.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): KEIN ADAPTER (Fall A - Infrastruktur-Hilfsklasse)</strong><br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 1.1 des Leitfadens ist diese Klasse <strong>kein Adapter</strong>, da sie kein Port-Interface aus dem Application Core implementiert. Sie erbt lediglich von <see cref="AbstractValidator{T}"/> und definiert interne Validierungsregeln für die FluentValidation-Bibliothek.<br/>
/// - <strong>Aktion:</strong> Wurde aus <c>Infrastructure/Adapters/Validators</c> in den Ordner <c>Infrastructure/Validators</c> verschoben.
/// </para>
/// </summary>
public sealed class ConfigureAutoLogonValidator : AbstractValidator<ConfigureAutoLogonRequest>
{
    public ConfigureAutoLogonValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .When(x => !x.IsDeactivateAction)
            .WithMessage("Username is required when activating AutoLogon.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .When(x => !x.IsDeactivateAction)
            .WithMessage("Password is required when activating AutoLogon.");
    }
}
