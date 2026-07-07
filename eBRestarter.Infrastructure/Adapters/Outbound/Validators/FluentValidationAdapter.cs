using eBRestarter.Core.Application.Common.Results;
using eBRestarter.Core.Application.Ports.Inbound.Validators;
using FluentValidation;
using ValidationResult = eBRestarter.Core.Application.Common.Results.ValidationResult;

namespace eBRestarter.Infrastructure.Adapters.Validators;

/// <summary>
/// Adapter: Driven Adapter (Outbound Handler/Adapter) wrapping FluentValidation to validate domain models and commands.
/// <para>
/// <strong>Architektonische Klassifizierung (Leitfaden): OUTBOUND ADAPTER (Driven Adapter)</strong><br/>
/// - <strong>Rolle &amp; Verantwortung:</strong> Erfüllt als technologischer Baustein im äußeren Ring (Infrastructure Layer) Vorgaben aus dem Core zur Validierung via FluentValidation-Bibliothek.<br/>
/// - <strong>Implementierter Port:</strong> <see cref="IInboundPortApplicationValidator{T}"/> (aus dem Application Core).<br/>
/// - <strong>Begründung:</strong> Gemäß Abschnitt 2.2 des Leitfadens ist diese Klasse ein <strong>Outbound Adapter</strong>, da sie im Infrastructure-Layer liegt und einen Port implementiert, um externes Validierungs-Framework-Verhalten für den Core bereitzustellen.
/// </para>
/// </summary>
public sealed class FluentValidationAdapter<T>(IValidator<T> validator) : IInboundPortApplicationValidator<T>
{
    public ValidationResult Validate(T request)
    {
        var result = validator.Validate(request);
        if (result.IsValid)
        {
            return ValidationResult.Success();
        }

        var errors = result.Errors.Select(e => new ValidationError(e.ErrorMessage)).ToList();
        return new ValidationResult(false, errors);
    }
}
